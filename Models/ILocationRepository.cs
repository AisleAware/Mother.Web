using AisleAware.Common.Mother;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mother.Web.Models
{
    interface ILocationRepository
    {
        Task<LocationInfo> Get(string name, ProductId productId);
        Task<LocationInfo> Get(int Id);
        Task<IEnumerable<LocationInfo>> GetAll();
        Task<LocationInfo> Add(LocationInfo location);
    }
}
