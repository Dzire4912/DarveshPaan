using Renci.SshNet;
using System.Diagnostics.CodeAnalysis;
using TAN.DomainModels.Entities;
using TANWeb.Areas.Inventory.Models;
using TANWeb.Services;

namespace TANWeb.Areas.Inventory.Services
{
    [ExcludeFromCodeCoverage]
    public class SftpServices
    {
        private readonly IConfiguration _config;
        private readonly KronosSftpCredModel kronosSftpCreds;
        private readonly KronosSftpFileEndpointsModel kronosSftpFileEndpoints;
        private readonly ILogger _log;
        private readonly string theYYYYMMDD;

        public SftpServices(IConfiguration config)
        {
            _config = config;
            kronosSftpCreds = new KronosSftpCredModel();
            config.Bind("KronosSftpCreds", kronosSftpCreds);
            kronosSftpFileEndpoints = new KronosSftpFileEndpointsModel();
            config.Bind("KronosSftpFileEndpoints", kronosSftpFileEndpoints);
            theYYYYMMDD = DateTime.UtcNow.AddDays(-1).ToString("yyyyMMdd");

        }

        public Stream DownloadKronosPunchExportFile(KronosSftpCredentials? kronosSftpCreds, KronosSftpFileEndpoints kronosSftpFileEndpoints)
        {
            if (kronosSftpCreds != null)
            {
                using (SftpClient sftpClient = new SftpClient(EncryptionService.Decrypt(kronosSftpCreds.KronosHostGLC), EncryptionService.Decrypt(kronosSftpCreds.KronosUserNameGLC), EncryptionService.Decrypt(kronosSftpCreds.KronosPasswordGLC)))
                {
                    try
                    {
                        sftpClient.Connect();
                        Stream stream = new MemoryStream();
                        sftpClient.DownloadFile(UpdateFileName(EncryptionService.Decrypt(kronosSftpFileEndpoints.KronosPunchExportGLC)), stream);
                        stream.Position = 0;
                        return stream;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }

                }
            }
            else
            {
                return null;
            }
        }


        public Stream DownloadKronosPayrollFile()
        {
            using (SftpClient sftpClient = new SftpClient(kronosSftpCreds.KronosHostGLC, kronosSftpCreds.KronosUserNameGLC, kronosSftpCreds.KronosPasswordGLC))
            {
                try
                {
                    sftpClient.Connect();

                    Stream stream = new MemoryStream();
                    sftpClient.DownloadFile(UpdateFileName(kronosSftpFileEndpoints.KronosPayrollGLC), stream);
                    stream.Position = 0;

                    Console.WriteLine("Downloaded payroll file successfully");

                    return stream;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        private string UpdateFileName(string fileName)
            => fileName.Replace("YYYYMMDD", theYYYYMMDD);





    }
}
