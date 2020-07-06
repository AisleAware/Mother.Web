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
    public class CallRepository : ICallRepository
    {
        private readonly ApiDbContext apiDbContext;

        public CallRepository(ApiDbContext apiDbContext)
        {
            this.apiDbContext = apiDbContext;
        }

        public async Task<CallInfo> Add(Request request)
        {
            // Get the location id of the location in the request
            var location = await apiDbContext.Locations.FirstOrDefaultAsync(loc => loc.Name == request.ProductName && loc.Type == request.ProductType);

            // If the location is not yet known then add the location
            if (location == null)
            {
                LocationInfo newLocation = new LocationInfo
                {
                    Name = request.ProductName,
                    Type = request.ProductType 
                };

                location = (await apiDbContext.Locations.AddAsync(newLocation)).Entity;
                await apiDbContext.SaveChangesAsync();
            }

            // Prepare the data to be added to the database
            CallInfo callInfo = new CallInfo
            {
                Time = DateTime.Now,
                Location = location,
                Version = request.Version,
                Status = request.Status,
                License = request.License
            };

            var result = await apiDbContext.Calls.AddAsync(callInfo);
            await apiDbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<IEnumerable<CallInfo>> GetAllCalls()
        {
            return await apiDbContext.Calls.ToListAsync();
        }

        public async Task<IEnumerable<CallInfo>> GetProductCalls(ProductId productId)
        {
            var locationsWithProductType = await apiDbContext.Locations.Where(loc => loc.Type == productId).ToListAsync();
            
            return await apiDbContext.Calls
                .Where(call => locationsWithProductType.Any(loc => loc.Id == call.Id))
                .ToListAsync();
        }

        public async Task<CallInfo> GetCall(int Id)
        {
            return await apiDbContext.Calls.FirstOrDefaultAsync(call => call.Id == Id);
        }

        public async Task<IEnumerable<string>> GetLocationNames()
        {
            return await apiDbContext.Locations
                .Select(loc => loc.Name)
                //.Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<CallInfo>> GetLatestUniqueCalls()
        {
            return await Task<IEnumerable<CallInfo>>.Run(() =>
            {
                var groups = apiDbContext.Calls.Include(call => call.Location).AsEnumerable().GroupBy(call => call.Location.Id);

                List<CallInfo> calls = new List<CallInfo>();
                foreach (var group in groups)
                    calls.Add(group.First());

                return calls;
            });
        }

        public async Task<IEnumerable<CallInfo>> GetCalls(bool unique, string locationName, ProductId productId, StatusId statusId, DateTime? timeStart, DateTime? timeEnd)
        {
            return await Task<IEnumerable<CallInfo>>.Run(() =>
            {
                // Set up the collection to query
                IQueryable<CallInfo> query = apiDbContext.Calls;

                // If the starting time was given then add the filter to the query
                if (timeStart != null)
                    query = query.Where(call => call.Time >= timeStart);

                // If the ending time was given then add the filter to the query
                if (timeEnd != null)
                    query = query.Where(call => call.Time <= timeEnd);

                // If the productName was given then add the filter to the query
                if (!String.IsNullOrEmpty(locationName))
                {
                    // Get the location that has the specified product name
                    var location = apiDbContext.Locations.FirstOrDefault(loc => loc.Name.ToLower().Contains(locationName.ToLower()));

                    // Query all calls from that location
                    query = query.Where(call => call.Location.Id == location.Id);
                }

                // If the productType was given then add the filter to the query
                if (productId != ProductId.All)
                {
                    // Get a list of all location Ids that have the specified product type
                    var locations = apiDbContext.Locations.Where(loc => loc.Type == productId).Select(loc => loc.Id).ToList();

                    // Add a query to return calls that include any of the locations with the specified product type
                    query = query.Where(call => locations.Any(loc => loc == call.Location.Id));
                }

                // If the status was given then add the filter to the query
                if (statusId != StatusId.All)
                    query = query.Where(call => call.Status == statusId);

                List<CallInfo> calls;

                if (unique)
                {
                    // Group the query results according to product name
                    var groups = query.Include(call => call.Location).AsEnumerable().GroupBy(call => call.Location);

                    // Assemble a list of the most recent product names in the groups
                    calls = new List<CallInfo>();
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
            model.FilterProduct = productId;

            return await Task<MotherTableViewModel>.Run(() =>
            {
                // Set up the collection to query
                IQueryable<CallInfo> query = apiDbContext.Calls;

                // If the productType was given then add the filter to the query
                if (productId != ProductId.All)
                {
                    // Get a list of all location Ids that have the specified product type
                    var locations = apiDbContext.Locations.Where(loc => loc.Type == productId).Select(loc => loc.Id).ToList();

                    // Add a query to return calls that include any of the locations with the specified product type
                    query = query.Where(call => locations.Any(loc => loc == call.Location.Id));
                }

                // Group the query results according to the location
                var groups = query.Include(call => call.Location).AsEnumerable().GroupBy(call => call.Location.Id);

                // Assemble a list of the viewmodel data needed for each row of the table
                model.Locations = new List<Location>();
                foreach (var group in groups)
                {
                    var row = new Location();

                    row.callInfo = group.Last();

                    // Retrieve this location's name and type
                    //row.Name = apiDbContext.Locations.FirstOrDefault(loc => loc.Id == row.callInfo.Location.Id).Name;
                    //row.Type = apiDbContext.Locations.FirstOrDefault(loc => loc.Id == row.callInfo.Location.Id).Type;
                    // This location's name and type were already retrieved via the Include() above
                    row.Name = row.callInfo.Location.Name;
                    row.Type = row.callInfo.Location.Type;

                    var lastStartCall = group.LastOrDefault(call => call.Status == StatusId.Start);
                    if (lastStartCall != null)
                        row.lastStartTime = lastStartCall.Time;
                    else
                        row.lastStartTime = group.FirstOrDefault().Time;    // The first known call to Mother

                    row.IsNewContact = (RightNow - group.FirstOrDefault().Time) < DaysSpan;

                    // Get results within the included recent number of days
                    var checkGroup = group.Where(call => call.Time >= checkDate);
                    if (checkGroup.Count() > 0)
                    {
                        row.WarningCount = checkGroup.Count(call => call.Status == StatusId.Warning);
                        row.ErrorCount = checkGroup.Count(call => call.Status == StatusId.Error);
                    }

                    row.IsNewVersion = CheckVersionUpdated(DaysToInclude, group);
                    row.IsLicenseExpiring = row.callInfo.License.Substring(0, 2).ToLower() == "ex";

                    model.Locations.Add(row);
                }

                return model;
            });
        }

        private bool CheckVersionUpdated(int DaysToInclude, IGrouping<int, CallInfo> group)
        {
            DateTime CheckDay = DateTime.Now - TimeSpan.FromDays(DaysToInclude);

            // Try to select the version closest to one day ago
            string OneDayAgoVersion = group.LastOrDefault(call => call.Time < CheckDay)?.Version;

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
            var result = await apiDbContext.Calls.DeleteAsync();
            await apiDbContext.SaveChangesAsync();
        }

        public async Task DeleteNamed(string locationName)
        {
            // Get the location Id for the given location name
            int? locationIdToDelete = (await apiDbContext.Locations.FirstOrDefaultAsync(loc => loc.Name == locationName)).Id;

            // If the location was not found just return
            if (locationIdToDelete == null)
                return;

            // Delete all calls from the given location
            var result = await apiDbContext.Calls.Where(call => call.Location.Id == locationIdToDelete).DeleteAsync();

            // Then delete the location itself
            result = await apiDbContext.Locations.Where(loc => loc.Id == locationIdToDelete).DeleteAsync();
            
            await apiDbContext.SaveChangesAsync();
        }
    }
}
