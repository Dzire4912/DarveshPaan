using Microsoft.AspNetCore.Http;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.Repository.Abstractions;
using static System.Formats.Asn1.AsnWriter;

namespace TANWeb.Services
{
    [ExcludeFromCodeCoverage]
    public class TelecomReportingBackgroundService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ApiServices apiServices;
        private readonly IServiceProvider _serviceProvider;
        public TelecomReportingBackgroundService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            apiServices = new ApiServices(_configuration);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new(TimeSpan.FromMinutes(_configuration.GetValue<int>("Bandwidth:frequencyInMins")));
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await DoWork();
                }
            }
            catch (OperationCanceledException ex)
            {
                Log.Error(ex, "An Error Occured TelecomReportingBackgroundService ExecuteAsync :{ErrorMsg}", ex.Message);
            }
        }

        private async Task<int> DoWork()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    // Get a list of organizations with bandwidth access and their subaccount ids.
                    List<BandwidthAccessList> bandwidthAccessList = await _uow.OrganizationRepo.GetBandwidthAccessList(); 
                    if (bandwidthAccessList == null && bandwidthAccessList.Count > 0)
                    {
                        string url = "";
                        int limit = _configuration.GetValue<int>("Bandwidth:limit");
                        DateTime endTime = DateTime.Now;
                        DateTime startTime = endTime.AddHours(-2);
                        foreach (var item in bandwidthAccessList)
                        {
                            url = "";
                            url = _configuration.GetValue<string>("Bandwidth:URL");
                            url += "/voice/calls?accountId=" + item.MasterAccountId + "" +
                                "&subAccount=" + item.SubAccountId + "&startTime=gte:" + startTime.ToString("yyyy-MM-ddTHH:mm:ssZ") + "" + "&endTime=gte:" + endTime.ToString("yyyy-MM-ddTHH:mm:ssZ") + "" + "&limit=" + limit;
                            CallDetails callDetails = await apiServices.GetMethod(url);
                            if (callDetails != null)
                            {
                                if (callDetails.Data != null)
                                {
                                    if (callDetails.Data.TotalCount > 0)
                                    {
                                        //Get Distinct CallId
                                        List<string> strings = new List<string>();
                                        strings = callDetails.Data.Calls.Select(x => x.CallId.ToString()).Distinct().ToList();

                                        //Check CallId exist or not
                                        List<string> callIds = await _uow.BandwidthCallEventsRepo.CheckDuplicateCallsId(strings);

                                        //remove existing callid from the list and bind
                                        List<BandwidthCallEvents> listCalls = callDetails.Data.Calls.Where(x => !callIds.Contains(x.CallId)).ToList();

                                        //Insert into the Calls table
                                        if (listCalls.Count > 0)
                                        {
                                            if (await _uow.BandwidthCallEventsRepo.AddCalls(listCalls))
                                            {
                                                BandwidthCallLogs bandwidthCall = new BandwidthCallLogs()
                                                {
                                                    URL = url,
                                                    Response = "Total Count : " + callDetails.Data.TotalCount,
                                                    IsActive = true,
                                                    ResponseStatus = 200
                                                };
                                                await _uow.BandwidthCallLogsRepo.AddBandwidthLogs(bandwidthCall);
                                            }
                                        }
                                    }
                                    if (callDetails.Data.TotalCount == 0)
                                    {
                                        BandwidthCallLogs bandwidthCall = new BandwidthCallLogs()
                                        {
                                            URL = url,
                                            Response = "Total Count : " + callDetails.Data.TotalCount,
                                            IsActive = true,
                                            ResponseStatus = 200
                                        };
                                        await _uow.BandwidthCallLogsRepo.AddBandwidthLogs(bandwidthCall);
                                    }
                                }

                                //if there is any errors in the URL
                                if (callDetails.Errors != null && callDetails.Errors.Count > 0)
                                {
                                    BandwidthCallLogs bandwidthCall = new BandwidthCallLogs()
                                    {
                                        URL = url,
                                        Response = string.Join(", ", callDetails.Errors.Select(x => x.Description)),
                                        IsActive = true,
                                        ResponseStatus = 400
                                    };
                                    await _uow.BandwidthCallLogsRepo.AddBandwidthLogs(bandwidthCall);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured TelecomReportingBackgroundService DoWork :{ErrorMsg}", ex.Message);
            }
            return 0;
        }

    }
}
