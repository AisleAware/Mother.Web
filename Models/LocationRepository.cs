using AisleAware.Common.Mother;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mother.Web.Models
{
    public class LocationRepository : ILocationRepository
    {
        private readonly ApiDbContext apiDbContext;

        public LocationRepository(ApiDbContext apiDbContext)
        {
            this.apiDbContext = apiDbContext;
        }

        public Task<LocationInfo> Get(string name, ProductId productId)
        {
            throw new NotImplementedException();
        }

        public Task<LocationInfo> Get(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LocationInfo>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<LocationInfo> Add(LocationInfo location)
        {
            throw new NotImplementedException();
        }
    }
}
