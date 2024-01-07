using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using UAParser;

namespace TAN.DomainModels.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class UserAgentHelper
    {
        public static string GetBrowserName(string userAgent)
        {
            var uaParser = Parser.GetDefault();
            var clientInfo = uaParser.Parse(userAgent);
            return clientInfo.UA.Family;
        }
     

    }
}
