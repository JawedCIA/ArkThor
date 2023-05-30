using ArkThor.API.Entities;
using ArkThor.API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ArkThor.API.Utilities;
using System.Security.Cryptography;
using ArkThor.API.Models.Records;
using System.Diagnostics.Eventing.Reader;

namespace ArkThor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoreAdminController : ControllerBase
    {
        private readonly ILogger<CoreAdminController> _logger;
        private IMapper _mapper;
        private IRabbitMQService _rabbitMQService;
        public CoreAdminController(ILogger<CoreAdminController> logger,  IMapper mapper, IConfiguration config, IRabbitMQService rabbitMQService)
        {
            _logger = logger;
           
            _mapper = mapper;           
            _rabbitMQService = rabbitMQService;
        }

        [HttpPost]
        [Route("Updateip2asn")]
        public async Task<IActionResult> Updateip2asn()
        {
            try
            {
                if (Util.IsInternetConnected())
                {

                    RabbitMQMessage msginfo = new()
                    {
                        message = DateTime.Now.ToString("yyyyMMddHHmmss")

                    };

                    _rabbitMQService.SendMessage(msginfo, "ip2asn");
                    return Ok("ip2asn action posted for auto update..");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,"Unable to connect to Internet");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,ex.Message);
            }
        }

        [HttpPost] 
        [Route("UpdateThreatFoxRule")]
        public async Task<IActionResult> UpdateThreatFoxRule()
        {
            try
            {
                if (Util.IsInternetConnected())
                {

                    RabbitMQMessage msginfo = new()
                    {
                        message = DateTime.Now.ToString("yyyyMMddHHmmss")

                    };

                    _rabbitMQService.SendMessage(msginfo, "ThreatFoxRule");
                    return Ok("ThreatFox Rule action posted for auto update..");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to connect to Internet");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
           

        }

    }
}
