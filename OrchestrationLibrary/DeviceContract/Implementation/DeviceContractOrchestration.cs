﻿using block_auth_api.Connection;
using block_auth_api.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace block_auth_api.Orchestration
{
    public class DeviceContractOrchestration : IDeviceContractOrchestration
    {
        private readonly IDeviceContractManager _ContractManager;

        public DeviceContractOrchestration(IDeviceContractManager contractManager)
        {
            _ContractManager = contractManager;
        }

        public Device GetDevice(int index)
        {
            var result = _ContractManager
                    .GetDevicesFunction()
                    .CallDeserializingToObjectAsync<Device>(index);

            result.Wait();

            return result.Result;
        }

        public int GetDeviceCount()
        {
            var deviceCountFunction = _ContractManager
                .GetDeviceCountFunction()
                .CallAsync<BigInteger>();

            deviceCountFunction.Wait();

            var deviceCount = (int)deviceCountFunction.Result;

            return deviceCount;
        }

        public bool AddDevice(Device device)
        {
            try
            {
                var accountAddress = _ContractManager.AdminAccount();
                var gas = _ContractManager.GetGasAmount();
                var value = _ContractManager.GetValueAmount();

                var loginFunction = _ContractManager
                    .GetAddDeviceFunction()
                    .SendTransactionAsync(accountAddress, gas, value, device.Name, device.Ip, device.Role);

                loginFunction.Wait();

                return true;

            }catch(Exception ex)
            {
                return false;
            }
            
        }

        public void RemoveDevice(Device device)
        {
            var accountAddress = _ContractManager.AdminAccount();
            var gas = _ContractManager.GetGasAmount();
            var value = _ContractManager.GetValueAmount();

            var loginFunction = _ContractManager
                .GetAddDeviceFunction()
                .SendTransactionAsync(accountAddress, gas, value, device.Name, device.Ip);

            loginFunction.Wait();
        }

        public Dictionary<string, List<Device>> GetDevices()
        {
            try
            {
                var deviceList = new List<Device>();
                var size = GetDeviceCount();
                if (size > 0)
                {

                    for (int i = 1; i <= size; i++)
                    {

                        var device = GetDevice(i);
                        var status = GetDevice(device.Ip);
                        if (status)
                        {
                            device.Status = "online";
                        }
                        deviceList.Add(device);
                    }

                    var deviceDictionary = new Dictionary<string, List<Device>>
                    {
                        { "devices", deviceList }
                    };

                    return deviceDictionary;

                }
                else
                {
                    return null;
                }
            }catch(Exception ex)
            {
                return new Dictionary<string, List<Device>>();
            }
        }

        public void TriggerEvent(LoggedIn loggedIn)
        {
            var accountAddress = loggedIn.Sender;
            var gas = _ContractManager.GetGasAmount();
            var value = _ContractManager.GetValueAmount();

            var loginFunction = _ContractManager
                .GetLoginAdminFunction()
                .SendTransactionAsync(accountAddress, gas, value, loggedIn.Ip, loggedIn.Role);

            loginFunction.Wait();
        }

        public LoggedIn DeviceAuth(LoggedIn loggedIn)
        {
            var request = new RestRequest()
            {
                Method = Method.POST,
                Resource = "/auth_data"
            };

            var client = new RestClient($"http://{loggedIn.Ip}");

            var response = client.Post<LoggedIn>(request);

            return response.Data;
        }

        public string AccessDevice(LoggedIn loggedIn)
        {
            var request = new RestRequest()
            {
                Method = Method.POST,
                Resource = "/connect"
            };

            var client = new RestClient($"http://{loggedIn.Ip}");

            var message = $"{loggedIn.Token},{loggedIn.Ip},{loggedIn.Sender},{loggedIn.Role}";

            request.AddParameter("message", message);

            var response = client.Execute(request);

            return response.Content;
        }

        public bool GetDevice(string url)
        {
            var request = new RestRequest()
            {
                Method = Method.GET,
                Resource = "/"
            };

            var client = new RestClient($"http://{url}");

            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                return true;
            }
            return false;
        }

        public string TurnDeviceOn(Device device)
        {
            var request = new RestRequest()
            {
                Method = Method.POST,
                Resource = "/on"
            };

            var client = new RestClient($"http://{device.Ip}");

            var response = client.Execute(request);

            return response.Content;
        }

        public string TurnDeviceOff(Device device)
        {
            var request = new RestRequest()
            {
                Method = Method.POST,
                Resource = "/off"
            };

            var client = new RestClient($"http://{device.Ip}");

            var response = client.Execute(request);

            return response.Content;
        }
    }
}