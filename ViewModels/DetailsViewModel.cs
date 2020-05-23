using AisleAware.Common.Mother;
using Mother.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mother.Web.ViewModels
{
    public class DetailsViewModel
    {
        public string Name { get; set; }
        [Display(Name = "Filter Status:")]
        public StatusId FilterStatus { get; set; }
        [Display(Name = "Filter Days:")]
        public DateTime FilterTimeStart { get; set; }
        public DateTime FilterTimeEnd { get; set; }
        public IEnumerable<RepoInfo> Calls { get; set; }
    }
}
