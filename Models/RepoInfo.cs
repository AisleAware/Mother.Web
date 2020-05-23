using AisleAware.Common.Mother;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mother.Web.Models
{
    public class RepoInfo : Request
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
    }
}
