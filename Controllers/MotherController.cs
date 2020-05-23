using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AisleAware.Common.Mother;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mother.Web.Models;

namespace Mother.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MotherController : Controller
    {
        private readonly IMotherRepository motherRepository;
        private readonly ILogger<HomeController> _logger;

        public MotherController(IMotherRepository motherRepository, ILogger<HomeController> logger)
        {
            this.motherRepository = motherRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCalls()
        {
            try
            {
                return Ok(await motherRepository.GetAllCalls());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RepoInfo>> GetOneCall(int? id)
        {
            try
            {
                var result = await motherRepository.GetCall(id ?? 1);

                if (result == null)
                    return NotFound();
                else
                    return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("unique/names")]
        public async Task<ActionResult<string>> GetUniqueNames(int Id)
        {
            try
            {
                var result = await motherRepository.GetUniqueCallerNames();

                if (result == null)
                    return NotFound();
                else
                    return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("unique")]
        [HttpGet("unique/latest")]
        public async Task<ActionResult<RepoInfo>> GetLatestUnique()
        {
            try
            {
                var result = await motherRepository.GetLatestUniqueCalls();

                if (result == null)
                    return NotFound();
                else
                    return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("unique/{filter}")]
        public async Task<ActionResult<RepoInfo>> SearchLatestUniqueCalls(string name, int? type, int? status, DateTime? timestart, DateTime? timeend)
        {
            ProductId productId;
            try
            {
                productId = (ProductId)type;
            }
            catch
            {
                productId = ProductId.All;
            }
            StatusId statusId;
            try
            {
                statusId = (StatusId)status;
            }
            catch
            {
                statusId = StatusId.All;
            }

            try
            {
                var result = await motherRepository.GetCalls(true, name, productId, statusId, timestart, timeend);

                if (result == null)
                    return NotFound();
                else
                    return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("product/{id:int}")]
        public async Task<ActionResult<RepoInfo>> GetProduct(int Id)
        {
            try
            {
                var result = await motherRepository.GetProductCalls((ProductId)Id);

                if (result == null)
                    return NotFound();
                else
                    return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveCall(Request request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest();
                }

                return Ok(await motherRepository.AddMotherInfo(request));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding data to the database");
            }
        }

        [HttpDelete("deleteall")]
        public async Task<ActionResult> DeleteAll()
        {
            try
            {
                await motherRepository.DeleteAll();

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting data from the database");
            }
        }
    }
}
