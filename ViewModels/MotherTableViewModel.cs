﻿using AisleAware.Common.Mother;
using Mother.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mother.Web.ViewModels
{
    public class MotherTableViewModel
    {
        [Display(Name = "Filter Days:")]
        public int FilterDays { get; set; }
        [Display(Name = "Filter Product:")]

        public ProductId FilterProduct { get; set; }
        [Display(Name = "Filter Active:")]
        public ActiveId FilterActive { get; set; }
        [Display(Name = "Sort By:")]
        public SortId SortBy { get; set; }
        public List<Location> Locations { get; set; }
    }

    public enum ActiveId
    {
        Both = 0,
        Active = 1,
        Inactive = 2,
    }

    public enum SortId
    {
        None = 0,
        Product = 1,
        Name = 2,
        Recent = 3,
        Errors = 4,
        Warnings = 5,
        Expiring = 6,
    }

    public class Location
    {
        public CallInfo callInfo { get; set; }
        public string Name { get; set; }
        public ProductId Type { get; set; }
        public DateTime lastStartTime { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public bool IsNewContact { get; set; } = false;
        public bool IsLicenseExpiring { get; set; } = false;
        public bool IsNewVersion { get; set; } = false;
    }
}
