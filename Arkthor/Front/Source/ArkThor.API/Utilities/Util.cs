using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;

namespace ArkThor.API.Utilities
{
    public static class Util
    {
        public static bool IsInternetConnected()
        {
            // Check if any network interface is available
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Iterate through all network interfaces
                foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // Check if the interface is up and connected
                    if (networkInterface.OperationalStatus == OperationalStatus.Up)
                    {
                        // Check if the interface supports Internet Protocol (IP) traffic
                        IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                        if (ipProperties != null && ipProperties.GatewayAddresses.Count > 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

    }

}
