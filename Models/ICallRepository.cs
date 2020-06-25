using AisleAware.Common.Mother;
using Microsoft.AspNetCore.Mvc;
using Mother.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mother.Web.Models
{
    public interface ICallRepository
    {
        Task<IEnumerable<CallInfo>> GetAllCalls();
        Task<IEnumerable<CallInfo>> GetProductCalls(ProductId productId);
        Task<CallInfo> GetCall(int Id);
        Task<IEnumerable<string>> GetLocationNames();
        Task<IEnumerable<CallInfo>> GetLatestUniqueCalls();
        Task<IEnumerable<CallInfo>> GetCalls(bool unique, string productName, ProductId productId, StatusId statusId, DateTime? timeStart, DateTime? timeEnd);
        Task<MotherTableViewModel> BuildViewModels(int DaysToInclude, ProductId productId);
        Task<CallInfo> Add(Request requestInfo);
        Task DeleteAll();
        Task DeleteNamed(string locationName);
    }
}
