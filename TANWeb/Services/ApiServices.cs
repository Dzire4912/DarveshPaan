using Serilog;
using System.Net.Http.Headers;
using System.Text;
using RestSharp;
using Newtonsoft.Json;
using TAN.DomainModels.Models;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Services
{
    [ExcludeFromCodeCoverage]
    public class ApiServices
    {
        private readonly IConfiguration _configuration;
        public ApiServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<CallDetails> GetMethod(string url)
        {
            CallDetails CallDetails = new CallDetails();
            try
            {
                var _client = new RestClient();
                var request = new RestRequest(url, Method.Get);
                request.AddHeader("Authorization", "Basic " + _configuration.GetValue<string>("Bandwidth:Token"));
                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    CallDetails = JsonConvert.DeserializeObject<CallDetails>(response.Content);
                    Log.Information("BANDWIDTH_API", "BANDWIDTH_API", "");
                }
                else
                {
                    CallDetails = JsonConvert.DeserializeObject<CallDetails>(response.Content);
                    Log.Error("BANDWIDTH_API", "BANDWIDTH_API", response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured ApiServices GetMethod :{ErrorMsg}", ex.Message);
            }
            return CallDetails;
        }
    }
}
