using AisleAware.Common.Mother;
using Microsoft.AspNetCore.Mvc;
using Mother.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mother.Web.Models
{
    public interface IMotherRepository
    {
        Task<IEnumerable<RepoInfo>> GetAllCalls();
        Task<IEnumerable<RepoInfo>> GetProductCalls(ProductId productId);
        Task<RepoInfo> GetCall(int Id);
        Task<IEnumerable<string>> GetUniqueCallerNames();
        Task<IEnumerable<RepoInfo>> GetLatestUniqueCalls();
        Task<IEnumerable<RepoInfo>> GetCalls(bool unique, string productName, ProductId productId, StatusId statusId, DateTime? timeStart, DateTime? timeEnd);
        Task<MotherTableViewModel> BuildViewModels(int DaysToInclude, ProductId productId);
        Task<RepoInfo> AddMotherInfo(Request requestInfo);
        Task DeleteAll();
        Task DeleteNamed(string callerName);
    }
}
