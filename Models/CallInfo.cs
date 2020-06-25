using AisleAware.Common.Mother;
using Mother.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mother.Web.Models
{
    public class CallInfo
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        /// <summary>The Id of this location in the Locations table.</summary>
        public int LocationId { get; set; }
        /// <summary>The version of the deployed product (do not prepend V).</summary>
        public string Version { get; set; }
        /// <summary>The current status of the deployed product.</summary>
        public StatusId Status { get; set; }
        /// <summary>The current state of the licence of the deployed product.</summary>
        public string License { get; set; }
    }
}
