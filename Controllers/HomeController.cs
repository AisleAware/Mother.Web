using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AisleAware.Common.Mother;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Mother.Web.Models;
using Mother.Web.ViewModels;

namespace Mother.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMotherRepository _motherRepository;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IMotherRepository motherRepository, SignInManager<IdentityUser> signInManager, ILogger<HomeController> logger)
        {
            _motherRepository = motherRepository;
            this.signInManager = signInManager;
            _logger = logger;
        }

        [Route("")]
        [Route("Index")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Bypass index view if already logged in
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("report");
            }

            return View();
        }

        [Route("Home/Report")]
        public async Task<IActionResult> Report(int? days, int? product, int? active, int? sortby)
        {
            ProductId productId;
            try
            {
                productId = (ProductId)product;
            }
            catch
            {
                productId = ProductId.All;
            }

            ActiveId activeId;
            try
            {
                activeId = (ActiveId)active;
            }
            catch
            {
                activeId = ActiveId.Both;
            }

            SortId sortId;
            try
            {
                sortId = (SortId)sortby;
            }
            catch
            {
                sortId = SortId.None;
            }

            var model = await _motherRepository.BuildViewModels(days ?? 1, productId);

            // Keep the same active values (the repo doesn't use these)
            model.FilterActive = activeId;
            model.SortBy = sortId;

            return View(model);
        }

        [HttpGet("Home/Details")]
        public async Task<IActionResult> Details(string name, int? status, DateTime? datestart)
        {
            StatusId statusId;
            try
            {
                statusId = (StatusId)status;
            }
            catch
            {
                statusId = StatusId.All;
            }

            var model = new DetailsViewModel();
            model.Name = name;
            model.FilterStatus = statusId;
            if (datestart == null)
                model.FilterTimeStart = DateTime.Now - TimeSpan.FromDays(1);
            else
                model.FilterTimeStart = (DateTime)datestart;

            var calls = await _motherRepository.GetCalls(false, model.Name, ProductId.All, statusId, model.FilterTimeStart, null);

            model.Calls = calls.Reverse();  // Show the latest calls at the top of the list

            return View(model);
        }

        [HttpPost("Home/Delete")]
        public async Task<IActionResult> Delete(string name)
        {
            var model = new DetailsViewModel();
            model.Name = name;

            await _motherRepository.DeleteNamed(name);

            return RedirectToAction("index");
        }

        [Route("Home/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
