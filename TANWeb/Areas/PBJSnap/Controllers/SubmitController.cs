using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Configuration;
using Serilog;
using System;
using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.Repository.Abstractions;
using TANWeb.Helpers;
using TANWeb.Services;
using static TAN.DomainModels.Helpers.Permissions;

namespace TANWeb.Areas.PBJSnap.Controllers
{
    [Area("PBJSnap")]
    [Authorize]
    public class SubmitController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AspNetUser> _UserManager;
        private readonly IHttpContextAccessor _httpContext;

        public SubmitController(IUnitOfWork uow, Microsoft.AspNetCore.Identity.UserManager<AspNetUser> UserManager, IHttpContextAccessor httpContext)
        {
            _uow = uow;
            _UserManager = UserManager;
            _httpContext = httpContext;
        }
        [Route("/get-started/submit")]
        public async Task<IActionResult> Index()
        {
            SubmitModel submitModel = new SubmitModel();
            submitModel.FacilityList = new List<FacilityModel>();
            try
            {
                submitModel.FacilityList = await _uow.FacilityRepo.GetAllFacilitiesByOrgId();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured Submit Index :{ErrorMsg}", ex.Message);
            }

            return View(submitModel);
        }

        public async Task<IActionResult> GenerateXMLFile(GenerateXml generateXml)
        {
            try
            {
                string Filename = "";
                NursingHomeData nursingHomeData = await _uow.TimesheetRepo.GenerateSubmitXMLFile(generateXml);
                if(nursingHomeData != null)
                {

                    Filename = generateXml.FileName.ToString();
                    var settings = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "\t"
                    };
                    ViewBag.FileName = Filename;
                    var memoryStream = new MemoryStream();
                    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        // Create an entry for the XML file
                        var xmlEntry = zipArchive.CreateEntry(Filename + ".xml", CompressionLevel.Optimal);

                        // Write the XML content to the entry
                        using (var entryStream = xmlEntry.Open())
                        using (var writer = XmlWriter.Create(entryStream, settings))
                        {
                            writer.WriteStartDocument();

                            writer.WriteStartElement("nursingHomeData");

                            /*  writer.WriteAttributeString("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
                              writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");*/

                            // Write header element
                            writer.WriteStartElement("header");
                            writer.WriteAttributeString("fileSpecVersion", "4.00.0");
                            writer.WriteElementString("facilityId", nursingHomeData.Header.FacilityId);
                            writer.WriteElementString("stateCode", nursingHomeData.Header.StateCode);
                            writer.WriteElementString("reportQuarter", nursingHomeData.Header.ReportQuarter.ToString());
                            writer.WriteElementString("federalFiscalYear", nursingHomeData.Header.FederalFiscalYear.ToString());
                            writer.WriteElementString("softwareVendorName", nursingHomeData.Header.SoftwareVendorName);
                            writer.WriteElementString("softwareVendorEmail", nursingHomeData.Header.SoftwareVendorEmail);
                            writer.WriteElementString("softwareProductName", nursingHomeData.Header.SoftwareProductName);
                            writer.WriteElementString("softwareProductVersion", nursingHomeData.Header.SoftwareProductVersion);
                            writer.WriteEndElement();
                            // end header

                            // Write employees element
                            writer.WriteStartElement("employees");

                            foreach (var employee in nursingHomeData.Employees)
                            {
                                writer.WriteStartElement("employee");
                                writer.WriteElementString("employeeId", employee.EmployeeId);
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                            // end employee

                            // Write staffingHours element
                            writer.WriteStartElement("staffingHours");
                            writer.WriteAttributeString("processType", "merge");

                            foreach (var staffHours in nursingHomeData.StaffingHours)
                            {
                                writer.WriteStartElement("staffHours");

                                writer.WriteElementString("employeeId", staffHours.EmployeeId);

                                // Write workDays element
                                writer.WriteStartElement("workDays");

                                foreach (var workDay in staffHours.WorkDays.WorkDayList)
                                {
                                    writer.WriteStartElement("workDay");
                                    writer.WriteElementString("date", workDay.Date.ToString("yyyy-MM-dd"));

                                    // Write hourEntries element
                                    writer.WriteStartElement("hourEntries");

                                    foreach (var hourEntry in workDay.HourEntries.HourEntry)
                                    {
                                        writer.WriteStartElement("hourEntry");
                                        writer.WriteElementString("hours", hourEntry.Hours.ToString());
                                        writer.WriteElementString("jobTitleCode", hourEntry.JobTitleCode.ToString());
                                        writer.WriteElementString("payTypeCode", hourEntry.PayTypeCode.ToString());
                                        writer.WriteEndElement(); // hourEntry
                                    }

                                    writer.WriteEndElement(); // hourEntries
                                    writer.WriteEndElement(); // workDay
                                }

                                writer.WriteEndElement(); // workDays
                                writer.WriteEndElement(); // staffHours
                            }

                            writer.WriteEndElement(); // staffingHours
                            writer.WriteEndElement(); // nursingHomeData

                            writer.WriteEndDocument();
                            writer.Flush();
                        }
                    }

                    string xmlcontent = GetGeneratedXMLData(nursingHomeData, settings);
                    await SaveUploadHistory(generateXml, xmlcontent);
                    await AddFileName(generateXml);
                    memoryStream.Position = 0; 
                    return File(memoryStream, "application/zip", Filename + ".zip");
                }  
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured Submit Index :{ErrorMsg}", ex.Message);
            }
            return File("", "application/xml", "");
        }

        private async Task<bool> SaveUploadHistory(GenerateXml generateXml, string xmlcontext)
        {
            try
            {
                var userId = _httpContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var organizationId = _uow.FacilityRepo.GetAll().Where(f => f.Id == generateXml.FacilityId).Select(o => o.OrganizationId).FirstOrDefault();
                UploadHistory uploadHistory = new UploadHistory()
                {
                    OrganizationId = organizationId,
                    FacilityId = generateXml.FacilityId,
                    UploadXMLFile = xmlcontext,
                    FileName = generateXml.FileName,
                    IsActive = true
                };

                await _uow.HistoryRepo.UploadHistory(uploadHistory);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Submit Controller / Saving Upload History :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        private string GetGeneratedXMLData(NursingHomeData nursingHomeData, XmlWriterSettings settings)
        {
            try
            {
                string xmlContent = string.Empty; 

                using (var stringWriter = new StringWriter())
                using (var writer = XmlWriter.Create(stringWriter, settings))
                {
                    writer.WriteStartDocument();

                        writer.WriteStartElement("nursingHomeData");

                        /*  writer.WriteAttributeString("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
                            writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");*/

                        // Write header element
                        writer.WriteStartElement("header");
                        writer.WriteAttributeString("fileSpecVersion", "4.00.0");
                        writer.WriteElementString("facilityId", nursingHomeData.Header.FacilityId);
                        writer.WriteElementString("stateCode", nursingHomeData.Header.StateCode);
                        writer.WriteElementString("reportQuarter", nursingHomeData.Header.ReportQuarter.ToString());
                        writer.WriteElementString("federalFiscalYear", nursingHomeData.Header.FederalFiscalYear.ToString());
                        writer.WriteElementString("softwareVendorName", nursingHomeData.Header.SoftwareVendorName);
                        writer.WriteElementString("softwareVendorEmail", nursingHomeData.Header.SoftwareVendorEmail);
                        writer.WriteElementString("softwareProductName", nursingHomeData.Header.SoftwareProductName);
                        writer.WriteElementString("softwareProductVersion", nursingHomeData.Header.SoftwareProductVersion);
                        writer.WriteEndElement();

                        writer.WriteStartElement("employees");

                        foreach (var employee in nursingHomeData.Employees)
                        {
                            writer.WriteStartElement("employee");
                            writer.WriteElementString("employeeId", employee.EmployeeId);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                        // end employee

                        // Write staffingHours element
                        writer.WriteStartElement("staffingHours");
                        writer.WriteAttributeString("processType", "merge");

                        foreach (var staffHours in nursingHomeData.StaffingHours)
                        {
                            writer.WriteStartElement("staffHours");

                            writer.WriteElementString("employeeId", staffHours.EmployeeId);

                            // Write workDays element
                            writer.WriteStartElement("workDays");

                            foreach (var workDay in staffHours.WorkDays.WorkDayList)
                            {
                                writer.WriteStartElement("workDay");
                                writer.WriteElementString("date", workDay.Date.ToString("yyyy-MM-dd"));

                                // Write hourEntries element
                                writer.WriteStartElement("hourEntries");

                                foreach (var hourEntry in workDay.HourEntries.HourEntry)
                                {
                                    writer.WriteStartElement("hourEntry");
                                    writer.WriteElementString("hours", hourEntry.Hours.ToString());
                                    writer.WriteElementString("jobTitleCode", hourEntry.JobTitleCode.ToString());
                                    writer.WriteElementString("payTypeCode", hourEntry.PayTypeCode.ToString());
                                    writer.WriteEndElement(); // hourEntry
                                }

                                writer.WriteEndElement(); // hourEntries
                                writer.WriteEndElement(); // workDay
                            }

                            writer.WriteEndElement(); // workDays
                            writer.WriteEndElement(); // staffHours
                        }

                        writer.WriteEndElement(); // staffingHours
                        writer.WriteEndElement(); // nursingHomeData

                        writer.WriteEndDocument();
                        writer.Flush();
                    xmlContent = stringWriter.ToString();
                }
                return xmlContent;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in Submit Controller / Generating XML Content :{ErrorMsg}", ex.Message);
                return null;
            }
        }

        public async Task<bool> AddFileName(GenerateXml xml)
        {
            try
            {
                var organizationId = _uow.FacilityRepo.GetAll().Where(f => f.Id == xml.FacilityId).Select(o => o.OrganizationId).FirstOrDefault();
                History history = new History()
                {
                    FileName = xml.FileName + ".xml",
                    FacilityId = xml.FacilityId,
                    Year = xml.Year,
                    ReportQuarter = xml.ReportQuarter,
                    Month = xml.Month,
                    OrganizationId = organizationId,
                    IsActive = true
                };
                await _uow.HistoryRepo.AddHistory(history);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured Submit AddFileName :{ErrorMsg}", ex.Message);
            }
            return false;
        }
    }
}
