using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hackathon._1Script.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Primitives;

namespace Hackathon._1Script.Controllers
{
    public enum Os
    {
        Windows,
        MacOs,
        LinuxOrUnix
    }

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            bool isWindows = Request.Headers.TryGetValue("User-Agent", out StringValues userAgentValues) && userAgentValues.Any((string x) => x?.ToUpperInvariant().Contains("WINDOWS") ?? false);
            bool isMac = Request.Headers.TryGetValue("User-Agent", out userAgentValues) && userAgentValues.Any((string x) => x?.ToUpperInvariant().Contains("MAC OS") ?? false);

            Os os = isWindows ? Os.Windows : isMac ? Os.MacOs : Os.LinuxOrUnix;

            const string clientId = "fa24eac7-0684-4964-ab13-9d4ff772e3d1";
            X509Certificate2 cert = new X509Certificate2(@"App_Data\1script.pfx", "No password");
            ClientAssertionCertificate certCred = new ClientAssertionCertificate(clientId, cert);
            AuthenticationContext authContext = new AuthenticationContext("https://login.windows.net/b550583b-bc56-47d4-b547-6982b363a8b0/");
            AuthenticationResult authResult = await authContext.AcquireTokenAsync("https://management.azure.com/", certCred);
            string token = authResult.AccessToken;

            string siteName = Guid.NewGuid().ToString().Replace("-", "").ToLowerInvariant();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            string payload = @"{
    ""location"": ""West US"",
    ""name"": ""Generated156789765467i87654"",
    ""type"": ""Microsoft.Web/sites"",
    ""tags"": {
        ""hidden-related:subscriptions/c09ce800-abd2-48c5-8565-dc51435d90ec/resourceGroups/1script/providers/Microsoft.Web/serverfarms/1scriptplan"": ""empty""
    },
    ""kind"": ""app"",
    ""properties"": {
        ""name"": ""Generated156789765467i87654"",
        ""serverFarmId"": ""subscriptions/c09ce800-abd2-48c5-8565-dc51435d90ec/resourceGroups/1script/providers/Microsoft.Web/serverfarms/1scriptplan"",
        ""kind"": ""app""
    }
}".Replace("Generated156789765467i87654", siteName);

            string location = "https://management.azure.com/subscriptions/c09ce800-abd2-48c5-8565-dc51435d90ec/resourceGroups/1script/providers/Microsoft.Web/sites/Generated156789765467i87654?api-version=2016-08-01".Replace("Generated156789765467i87654", siteName);
            string credsLocation = "https://management.azure.com/subscriptions/c09ce800-abd2-48c5-8565-dc51435d90ec/resourceGroups/1script/providers/Microsoft.Web/sites/Generated156789765467i87654/config/publishingcredentials/list?api-version=2016-08-01".Replace("Generated156789765467i87654", siteName);


            StringContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = await client.PutAsync(location, content);
            responseMessage.EnsureSuccessStatusCode();
            content = new StringContent(string.Empty, Encoding.UTF8, "text/plain");
            HttpResponseMessage response = await client.PostAsync(credsLocation, content);
            string credsString = await response.Content.ReadAsStringAsync();
            JObject responseJson = JObject.Parse(credsString);
            string username = responseJson["properties"]["publishingUserName"].ToString();
            string password = responseJson["properties"]["publishingPassword"].ToString();

            string realResponse = GenerateScript(siteName, username, password, os);

            byte[] data = Encoding.UTF8.GetBytes(realResponse);
            string name = isWindows ? "get-started.cmd" : "get-started.sh";

            return File(data, "text/plain", name);
        }

        private string GenerateScript(string siteName, string username, string password, Os os)
        {
            string result = $@"
OS: {os}
Site: {siteName}
Username: {username}
Password: {password}
";
            string baseScript;
            string newline;

            switch (os)
            {
                case Os.Windows:
                    baseScript = Scripts.WindowsScript;
                    newline = Environment.NewLine;
                    break;
                case Os.MacOs:
                    baseScript = Scripts.MacOsScript;
                    newline = "\n";
                    break;
                default:
                    baseScript = Scripts.UnixScript;
                    newline = "\n";
                    break;
            }
            
            baseScript = baseScript.Replace("$SiteName$", PlatformEscape(os, siteName))
                           .Replace("$UserName$", PlatformEscape(os, username))
                           .Replace("$Password$", PlatformEscape(os, password))
                           .Replace("\r\n", newline);

            return baseScript;
        }

        private string PlatformEscape(Os os, string siteName)
        {
            return siteName;
        }
    }
}
