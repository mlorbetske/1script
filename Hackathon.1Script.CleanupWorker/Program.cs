using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;

namespace Hackathon._1Script.CleanupWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        private static async Task RunAsync()
        {
            const string clientId = "fa24eac7-0684-4964-ab13-9d4ff772e3d1";
            X509Certificate2 cert = new X509Certificate2(
                @"..\..\..\App_Data\1script.pfx",
                "No password");
            ClientAssertionCertificate certCred = new ClientAssertionCertificate(clientId, cert);
            AuthenticationContext authContext = new AuthenticationContext("https://login.windows.net/b550583b-bc56-47d4-b547-6982b363a8b0/");
            AuthenticationResult authResult = await authContext.AcquireTokenAsync("https://management.azure.com/", certCred);
            string token = authResult.AccessToken;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            string location = "https://management.azure.com/subscriptions/c09ce800-abd2-48c5-8565-dc51435d90ec/resourceGroups/1script/providers/Microsoft.Web/sites?api-version=2016-08-01";
            HttpResponseMessage responseMessage = await client.GetAsync(location);
            string responseText = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            JObject outer = JObject.Parse(responseText);
            JArray items = (JArray)outer["value"];

            foreach (JObject site in items.OfType<JObject>())
            {
                string name = site["name"].ToString();

                if (string.Equals(name, "1script", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string modified = site["properties"]["lastModifiedTimeUtc"].ToString();
                DateTime created = DateTime.Parse(modified, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

                if (created < DateTime.Now.AddDays(-1))
                {
                    location = $"https://management.azure.com/subscriptions/c09ce800-abd2-48c5-8565-dc51435d90ec/resourceGroups/1script/providers/Microsoft.Web/sites/{name}?api-version=2016-08-01";
                    responseMessage = await client.DeleteAsync(location);
                }
            }
        }
    }
}
