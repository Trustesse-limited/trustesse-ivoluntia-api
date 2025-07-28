using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.Extensions.Helpers
{
    public class NetworkFilter(IHttpContextAccessor httpContextAccessor)
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public (string? IpAddress, string? MacAddress) GetClientIpAndMac()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return (null, null);

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            string? macAddress = null;

            var allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                              nic.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var ni in allNetworkInterfaces)
            {
                var ipProps = ni.GetIPProperties();

                foreach (var addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.ToString() == ipAddress)
                    {
                        macAddress = BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes());
                        break;
                    }
                }

                if (macAddress != null)
                    break;
            }

            return (ipAddress, macAddress);
        }

    }
}
