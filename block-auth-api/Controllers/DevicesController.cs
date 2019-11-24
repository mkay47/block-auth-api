﻿using block_auth_api.Models;
using block_auth_api.Orchestration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelsLibrary.Models;
using OrchestrationLibrary.LogsOrchestration;
using System;

namespace block_auth_api.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class DevicesController : Controller
    {
        private readonly IDeviceContractOrchestration _DCO;
        private readonly ILogsOrchestration _LCO;
        public DevicesController(IDeviceContractOrchestration dco, ILogsOrchestration lco)
        {
            _DCO = dco;
            _LCO = lco;
        }

        [HttpGet]
        [Route("get_devices")]
        public ActionResult GetDevices()
        {
            try
            {
                var deviceDictionary = _DCO.GetDevices();

                if (deviceDictionary != null)
                {
                    return Ok(deviceDictionary);
                }
                else
                {
                    return Ok("No devices registered");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("get_device/{url}")]
        public ActionResult GetDevice(string url)
        {
            try
            {
                var result = _DCO.GetDevice(url);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("devices_trigger_event")]
        public ActionResult TriggerEvent([FromBody] LoggedIn loggedIn)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Model Passed");
                }

                _DCO.TriggerEvent(loggedIn);
                var log = new Log()
                {
                    Account = loggedIn.Sender,
                    Action = $"{loggedIn.Ip} accessed",
                    Role = loggedIn.Role
                };
                _LCO.AddLog(log);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("add_device")]
        public ActionResult AddDevice([FromBody] Device device)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Model Passed");
                }

                if (!_DCO.AddDevice(device))
                {
                    BadRequest("Failed To Add Device");
                };
                var log = new Log()
                {
                    Action = $"{device.Ip} accessed",
                    Role = device.Role
                };
                _LCO.AddLog(log);
                return Ok(device);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("remove_device")]
        public ActionResult RemoveDevice([FromBody] Device device)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Model Passed");
                }

                //_DCO.AddDevice(device);

                return Ok(device);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("devices_auth/")]
        public ActionResult DeviceAuth([FromBody] LoggedIn loggedIn)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Model Passed");
                }

                var result = _DCO.DeviceAuth(loggedIn);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("devices_connect/")]
        public ActionResult AccessDevice([FromBody] LoggedIn loggedIn)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Model Passed");
                }

                var result = _DCO.AccessDevice(loggedIn);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("turn_device_on/")]
        public ActionResult TurnDeviceOn([FromBody] Device device)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Model Passed");
                }

                var result = _DCO.TurnDeviceOn(device);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("turn_device_off/")]
        public ActionResult TurnDeviceOff([FromBody] Device device)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Model Passed");
                }

                var result = _DCO.TurnDeviceOff(device);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}