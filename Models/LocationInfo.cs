using AisleAware.Common.Mother;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mother.Web.Models
{
    public class LocationInfo
    {
        /// <summary>The unique Id of this deployed product.</summary>
        public int Id { get; set; }
        /// <summary>The unique name of the deployed product.</summary>
        public string Name { get; set; }
        /// <summary>The type of the deployed product for classification purposes.</summary>
        public ProductId Type { get; set; }
    }
}
