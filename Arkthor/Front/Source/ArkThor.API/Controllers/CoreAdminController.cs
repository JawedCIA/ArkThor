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
using Microsoft.AspNetCore.Cors;

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
        [EnableCors("AllowArkthorDomain")]
        public async Task<IActionResult> Updateip2asn()
        {
            try
            {
                if (Util.IsInternetConnected())
                {
                    var msginfo = DateTime.Now.ToString("yyyyMMddHHmmss");
                    //RabbitMQMessage msginfo = new()
                    //{
                    //    message = DateTime.Now.ToString("yyyyMMddHHmmss")

                    //};

                    _rabbitMQService.SendMessage(msginfo, null, "ip2asn_exchange");
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
        [EnableCors("AllowArkthorDomain")]
        public async Task<IActionResult> UpdateThreatFoxRule()
        {
            try
            {
                if (Util.IsInternetConnected())
                {
                    var msginfo = DateTime.Now.ToString("yyyyMMddHHmmss");
                    //RabbitMQMessage msginfo = new()
                    //{
                    //    message = DateTime.Now.ToString("yyyyMMddHHmmss")

                    //};

                    _rabbitMQService.SendMessage(msginfo, null, "threatfoxRule_exchange");
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

        [HttpPost]
        [Route("UpdateCoreConfig")]
        [EnableCors("AllowArkthorDomain")]
        public async Task<IActionResult> UpdateCoreConfig([FromBody] ConfigData configData)
        {
            try
            {

               var msginfo= configData.ConfigContent;
                    _rabbitMQService.SendMessage(msginfo, null, "configchange_exchange");
                    return Ok("configchange_exchange Rule action posted for auto update..");               

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }


        }
        public class ConfigData
        {
            public string ConfigContent { get; set; }
        }

    }
}
