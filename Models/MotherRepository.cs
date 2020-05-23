using AisleAware.Common.Mother;
using Microsoft.EntityFrameworkCore;
using Mother.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace Mother.Web.Models
{
    public class MotherRepository : IMotherRepository
    {
        private readonly ApiDbContext apiDbContext;

        public MotherRepository(ApiDbContext apiDbContext)
        {
            this.apiDbContext = apiDbContext;
        }

        public async Task<RepoInfo> AddMotherInfo(Request request)
        {
            RepoInfo repoInfo = new RepoInfo
            {
                Time = DateTime.Now,
                ProductName = request.ProductName,
                ProductType = request.ProductType,
                Version = request.Version,
                Status = request.Status,
                License = request.License
            };

            var result = await apiDbContext.MotherInfo.AddAsync(repoInfo);
            await apiDbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<IEnumerable<RepoInfo>> GetAllCalls()
        {
            return await apiDbContext.MotherInfo.ToListAsync();
        }

        public async Task<IEnumerable<RepoInfo>> GetProductCalls(ProductId productId)
        {
            return await apiDbContext.MotherInfo
                .Where(r => r.ProductType == productId)
                .ToListAsync();
        }

        public async Task<RepoInfo> GetCall(int Id)
        {
            return await apiDbContext.MotherInfo.FirstOrDefaultAsync(record => record.Id == Id);
        }

        public async Task<IEnumerable<string>> GetUniqueCallerNames()
        {
            return await apiDbContext.MotherInfo
                .Select(r => r.ProductName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<RepoInfo>> GetLatestUniqueCalls()
        {
            return await Task<IEnumerable<RepoInfo>>.Run(() =>
            {
                var groups = apiDbContext.MotherInfo.AsEnumerable().GroupBy(r => r.ProductName);

                List<RepoInfo> calls = new List<RepoInfo>();
                foreach (var group in groups)
                    calls.Add(group.First());

                return calls;
            });
        }

        public async Task<IEnumerable<RepoInfo>> GetCalls(bool unique, string productName, ProductId productId, StatusId statusId, DateTime? timeStart, DateTime? timeEnd)
        {
            return await Task<IEnumerable<RepoInfo>>.Run(() =>
            {
                // Set up the collection to query
                IQueryable<RepoInfo> query = apiDbContext.MotherInfo;

                // If the starting time was given then add the filter to the query
                if (timeStart != null)
                    query = query.Where(r => r.Time >= timeStart);

                // If the ending time was given then add the filter to the query
                if (timeEnd != null)
                    query = query.Where(r => r.Time <= timeEnd);

                // If the productName was given then add the filter to the query
                if (!String.IsNullOrEmpty(productName))
                    query = query.Where(r => r.ProductName.ToLower().Contains(productName.ToLower()));

                // If the productType was given then add the filter to the query
                if (productId != ProductId.All)
                    query = query.Where(r => r.ProductType == productId);

                // If the status was given then add the filter to the query
                if (statusId != StatusId.All)
                    query = query.Where(r => r.Status == statusId);

                List<RepoInfo> calls;

                if (unique)
                {
                    // Group the query results according to product name
                    var groups = query.AsEnumerable().GroupBy(r => r.ProductName);

                    // Assemble a list of the most recent product names in the groups
                    calls = new List<RepoInfo>();
                    foreach (var group in groups)
                        calls.Add(group.Last());
                }
                else
                {
                    calls = query.ToList();
                }

                return calls;
            });
        }

        public async Task<MotherTableViewModel> BuildViewModels(int DaysToInclude, ProductId productId)
        {
            DateTime RightNow = DateTime.Now;
            TimeSpan DaysSpan = TimeSpan.FromDays(DaysToInclude);
            DateTime checkDate = RightNow - DaysSpan;

            MotherTableViewModel model = new MotherTableViewModel();
            model.FilterDays = DaysToInclude;
            model.FilterType = productId;

            return await Task<MotherTableViewModel>.Run(() =>
            {
                // Set up the collection to query
                IQueryable<RepoInfo> query = apiDbContext.MotherInfo;

                // If the productType was given then add the filter to the query
                if (productId != ProductId.All)
                    query = query.Where(r => r.ProductType == productId);

                // Group the query results according to product name
                var groups = query.AsEnumerable().GroupBy(r => r.ProductName);

                // Assemble a list of the viewmodel data needed for each row of the table
                model.Locations = new List<Location>();
                foreach (var group in groups)
                {
                    var row = new Location();

                    row.repoInfo = group.Last();
                    var lastStartCall = group.LastOrDefault(r => r.Status == StatusId.Start);
                    if (lastStartCall != null)
                        row.lastStartTime = lastStartCall.Time;
                    else
                        row.lastStartTime = DateTime.MinValue;

                    row.IsNewContact = (RightNow - group.FirstOrDefault().Time) < DaysSpan;

                    // Get results within the included recent number of days
                    var checkGroup = group.Where(r => r.Time >= checkDate);
                    if (checkGroup.Count() > 0)
                    {
                        row.WarningCount = checkGroup.Count(r => r.Status == StatusId.Warning);
                        row.ErrorCount = checkGroup.Count(r => r.Status == StatusId.Error);
                        //row.WarningCount = group.Where(r => r.Time >= checkDate).Count(r => r.Status == StatusId.Warning);
                        //row.ErrorCount = group.Where(r => r.Time >= checkDate).Count(r => r.Status == StatusId.Error);
                    }

                    row.IsNewVersion = CheckVersionUpdated(DaysToInclude, group);
                    row.IsLicenseExpiring = row.repoInfo.License.Substring(0, 2).ToLower() == "ex";

                    model.Locations.Add(row);
                }

                return model;
            });
        }

        private bool CheckVersionUpdated(int DaysToInclude, IGrouping<string, RepoInfo> group)
        {
            DateTime CheckDay = DateTime.Now - TimeSpan.FromDays(DaysToInclude);

            // Try to select the version closest to one day ago
            string OneDayAgoVersion = group.LastOrDefault(s => s.Time < CheckDay)?.Version;

            if (OneDayAgoVersion == null)
            {
                // The contact first appeared less than a day ago so select the initial version to compare against
                OneDayAgoVersion = group.First().Version;
            }

            if (OneDayAgoVersion != group.Last().Version)
                return true;
            else
                return false;
        }

        public async Task DeleteAll()
        {
            var result = await apiDbContext.MotherInfo.DeleteAsync();
        }
    }
}
