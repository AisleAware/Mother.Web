using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AisleAware.Common.Mother;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ICallRepository callRepository;
        private readonly ILogger<HomeController> _logger;

        public MotherController(ICallRepository callRepository, ILogger<HomeController> logger)
        {
            this.callRepository = callRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCalls()
        {
            try
            {
                return Ok(await callRepository.GetAllCalls());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CallInfo>> GetOneCall(int? id)
        {
            try
            {
                var result = await callRepository.GetCall(id ?? 1);

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
                var result = await callRepository.GetLocationNames();

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

        [HttpGet("unique/latest")]
        public async Task<ActionResult<CallInfo>> GetLatestUnique()
        {
            try
            {
                var result = await callRepository.GetLatestUniqueCalls();

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

        [HttpGet("{unique}")]
        public async Task<ActionResult<CallInfo>> SearchLatestUniqueCalls(string name, int? type, int? status, DateTime? timestart, DateTime? timeend)
        {
            // Example URL: http://localhost:61279/api/mother/unique?type=6
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
                var result = await callRepository.GetCalls(true, name, productId, statusId, timestart, timeend);

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
        public async Task<ActionResult<CallInfo>> GetProduct(int Id)
        {
            try
            {
                var result = await callRepository.GetProductCalls((ProductId)Id);

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
        [AllowAnonymous]
        public async Task<IActionResult> ReceiveCall(Request request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest();
                }

                return Ok(await callRepository.Add(request));
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
                await callRepository.DeleteAll();

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting data from the database");
            }
        }
    }
}
