using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class Permissions
    {
        public static List<string> GeneratePermissionsForModule(string module, List<string> appName)
        {
            var permissions = new List<string>();
            foreach (var item in appName)
            {

                permissions.Add($"Permissions.{item}.{module}.Create");
                permissions.Add($"Permissions.{item}.{module}.View");
                permissions.Add($"Permissions.{item}.{module}.Edit");
                permissions.Add($"Permissions.{item}.{module}.Delete");

                if (item == ApplicationNameWithId.PBJSnap.ToString() && (module == AllModules.SubmissionHistory.ToString() || module == AllModules.Storage.ToString()))
                {
                    permissions.Add($"Permissions.{item}.{module}.Download");
                }
                if (item == ApplicationNameWithId.PBJSnap.ToString() && module == AllModules.Storage.ToString())
                {
                    permissions.Add($"Permissions.{item}.{module}.Upload");
                }
                if (item == ApplicationNameWithId.PBJSnap.ToString() && module == AllModules.Review.ToString())
                {
                    permissions.Add($"Permissions.{item}.{module}.AddCensus");
                    permissions.Add($"Permissions.{item}.{module}.EditCensus");
                    permissions.Add($"Permissions.{item}.{module}.DeleteCensus");
                    permissions.Add($"Permissions.{item}.{module}.Approve");
                    permissions.Add($"Permissions.{item}.{module}.Validate");
                }
                if (item == ApplicationNameWithId.PBJSnap.ToString() && module == AllModules.Backout.ToString())
                {
                    permissions.Add($"Permissions.{item}.{module}.DeleteInvalidaRecord");
                    permissions.Add($"Permissions.{item}.{module}.ViewInvalidRecord");
                }
                if (item == ApplicationNameWithId.Reporting.ToString() && module == AllModules.UploadClientFile.ToString())
                {
                    permissions.Add($"Permissions.{item}.{module}.Download");
                    permissions.Add($"Permissions.{item}.{module}.FilePreview");
                    permissions.Add($"Permissions.{item}.{module}.Upload");
                }
                if (item == ApplicationNameWithId.Reporting.ToString() && module == AllModules.Voice.ToString())
                {
                    permissions.Add($"Permissions.{item}.{module}.Export");
                }
                if (item == ApplicationNameWithId.Reporting.ToString() && module == AllModules.Data.ToString())
                {
                    permissions.Add($"Permissions.{item}.{module}.Export");
                }
            }
            return permissions;
        }
        public static class UserManagement
        {
            public const string PBJSnapView = "Permissions.PBJSnap.UserManagement.View";
            public const string PBJSnapCreate = "Permissions.PBJSnap.UserManagement.Create";
            public const string PBJSnapEdit = "Permissions.PBJSnap.UserManagement.Edit";
            public const string PBJSnapDelete = "Permissions.PBJSnap.UserManagement.Delete";
            public const string InventoryView = "Permissions.Inventory.UserManagement.View";

        }
        public static class RoleManagement
        {
            public const string PBJSnapView = "Permissions.PBJSnap.RoleManagement.View";
            public const string PBJSnapCreate = "Permissions.PBJSnap.RoleManagement.Create";
            public const string PBJSnapEdit = "Permissions.PBJSnap.RoleManagement.Edit";
            public const string PBJSnapDelete = "Permissions.PBJSnap.RoleManagement.Delete";
            public const string InventoryView = "Permissions.Inventory.RoleManagement.View";
        }
        public static class Admin
        {
            public const string PBJSnapView = "Permissions.PBJSnap.Admin.View";
            public const string PBJSnapCreate = "Permissions.PBJSnap.Admin.Create";
            public const string PBJSnapEdit = "Permissions.PBJSnap.Admin.Edit";
            public const string PBJSnapDelete = "Permissions.PBJSnap.Admin.Delete";
            public const string InventoryView = "Permissions.Inventory.Admin.View";
        }
        public static class Dashboard
        {
            public const string PBJSnapView = "Permissions.PBJSnap.Dashboard.View";
            public const string PBJSnapCreate = "Permissions.PBJSnap.Dashboard.Create";
            public const string PBJSnapEdit = "Permissions.PBJSnap.Dashboard.Edit";
            public const string PBJSnapDelete = "Permissions.PBJSnap.Dashboard.Delete";
            public const string InventoryView = "Permissions.Inventory.Dashboard.View";
        }
        public static class Storage
        {
            public const string PBJSnapView = "Permissions.PBJSnap.Storage.View";
            public const string PBJSnapCreate = "Permissions.PBJSnap.Storage.Create";
            public const string PBJSnapEdit = "Permissions.PBJSnap.Storage.Edit";
            public const string PBJSnapDelete = "Permissions.PBJSnap.Storage.Delete";
            public const string PBJSnapDownload = "Permissions.PBJSnap.Storage.Download";
            public const string PBJSnapUpload = "Permissions.PBJSnap.Storage.Upload";
        }

        public static class Configuration
        {
            public const string PBJSnapView = "Permissions.PBJSnap.Configuration.View";
            public const string PBJSnapCreate = "Permissions.PBJSnap.Configuration.Create";
            public const string PBJSnapEdit = "Permissions.PBJSnap.Configuration.Edit";
            public const string PBJSnapDelete = "Permissions.PBJSnap.Configuration.Delete";
        }
        public static class SubmissionHistory
        {
            public const string PBJSnapView = "Permissions.PBJSnap.SubmissionHistory.View";
            public const string PBJSnapCreate = "Permissions.PBJSnap.SubmissionHistory.Create";
            public const string PBJSnapEdit = "Permissions.PBJSnap.SubmissionHistory.Edit";
            public const string PBJSnapDelete = "Permissions.PBJSnap.SubmissionHistory.Delete";
            public const string PBJSnapDownload = "Permissions.PBJSnap.SubmissionHistory.Download";
        }

        public static class Organization
        {
            public const string PBJSnapView = "Permissions.PBJSnap.Organization.View";
            public const string PBJSnapCreate = "Permissions.PBJSnap.Organization.Create";
            public const string PBJSnapEdit = "Permissions.PBJSnap.Organization.Edit";
            public const string PBJSnapDelete = "Permissions.PBJSnap.Organization.Delete";
            public const string InventoryView = "Permissions.Inventory.Organization.View";
        }

        public static class Facility
        {
            public const string PBJSnapView = "Permissions.PBJSnap.Facility.View";
            public const string PBJSnapCreate = "Permissions.PBJSnap.Facility.Create";
            public const string PBJSnapEdit = "Permissions.PBJSnap.Facility.Edit";
            public const string PBJSnapDelete = "Permissions.PBJSnap.Facility.Delete";
            public const string InventoryView = "Permissions.Inventory.Facility.View";
        }

        public static class Agency
        {
            public const string PBJSnapView = "Permissions.PBJSnap.Agency.View";
            public const string PBJSnapCreate = "Permissions.PBJSnap.Agency.Create";
            public const string PBJSnapEdit = "Permissions.PBJSnap.Agency.Edit";
            public const string PBJSnapDelete = "Permissions.PBJSnap.Agency.Delete";
            public const string InventoryView = "Permissions.Inventory.Agency.View";
        }

        public static class EmployeeMaster
        {
            public const string PBJSnapView = "Permissions.PBJSnap.EmployeeMaster.View";
            public const string PBJSnapCreate = "Permissions.PBJSnap.EmployeeMaster.Create";
            public const string PBJSnapEdit = "Permissions.PBJSnap.EmployeeMaster.Edit";
            public const string PBJSnapDelete = "Permissions.PBJSnap.EmployeeMaster.Delete";
        }

        public static class Backout
        {
            public const string PBJSnapView = "Permissions.PBJSnap.Backout.View";
            public const string PBJSnapCreate = "Permissions.PBJSnap.Backout.Create";
            public const string PBJSnapEdit = "Permissions.PBJSnap.Backout.Edit";
            public const string PBJSnapDelete = "Permissions.PBJSnap.Backout.Delete";
            public const string PBJSnapDeleteInvalidRecord = "Permissions.PBJSnap.Backout.DeleteInvalidaRecord";
            public const string PBJSnapViewInvalidRecord = "Permissions.PBJSnap.Backout.ViewInvalidRecord";
        }
        public static class Upload
        {
            public const string PBJSnapView = "Permissions.PBJSnap.Upload.View";
            public const string PBJSnapUpload = "Permissions.PBJSnap.Upload.Create";
        }

        public static class Submit
        {
            public const string PBJSnapView = "Permissions.PBJSnap.Submit.View";
            public const string PBJSnapGenerate = "Permissions.PBJSnap.Submit.Create";
        }
        public static class Review
        {
            public const string PBJSnapView = "Permissions.PBJSnap.Review.View";
            public const string PBJSnapApprove = "Permissions.PBJSnap.Review.Approve";
            public const string PBJSnapValidate = "Permissions.PBJSnap.Review.Validate";

            public const string PBJSnapAddCensus = "Permissions.PBJSnap.Review.AddCensus";
            public const string PBJSnapEditCensus = "Permissions.PBJSnap.Review.EditCensus";
            public const string PBJSnapDeleteCensus = "Permissions.PBJSnap.Review.DeleteCensus";

        }

        public static class Reports
        {
            public const string TelecomReportsView = "Permissions.Reporting.Reports.View";
            public const string TelecomReportsEdit = "Permissions.Reporting.Reports.Edit";
            public const string TelecomReportsCreate = "Permissions.Reporting.Reports.Create";
            public const string TelecomReportsDelete = "Permissions.Reporting.Reports.Delete";

        }
        public static class Voice
        {
            public const string TelecomVoiceView = "Permissions.Reporting.Voice.View";
            public const string TelecomVoiceEdit = "Permissions.Reporting.Voice.Edit";
            public const string TelecomVoiceCreate = "Permissions.Reporting.Voice.Create";
            public const string TelecomVoiceDelete = "Permissions.Reporting.Voice.Delete";
            public const string TelecomVoiceExport = "Permissions.Reporting.Voice.Export";
        }
        public static class Data
        {
            public const string TelecomDataView = "Permissions.Reporting.Data.View";
            public const string TelecomDataEdit = "Permissions.Reporting.Data.Edit";
            public const string TelecomDataCreate = "Permissions.Reporting.Data.Create";
            public const string TelecomDataDelete = "Permissions.Reporting.Data.Delete";
            public const string TelecomDataExport = "Permissions.Reporting.Data.Export";
        }

        public static class DataService
        {
            public const string TelecomDataServiceView = "Permissions.TelecomReporting.DataService.View";
            public const string TelecomDataServiceEdit = "Permissions.TelecomReporting.DataService.Edit";
            public const string TelecomDataServiceCreate = "Permissions.TelecomReporting.DataService.Create";
            public const string TelecomDataServiceDelete = "Permissions.TelecomReporting.DataService.Delete";
        }

        public static class UploadClientFile
        {
            public const string TelecomUploadClientFileView = "Permissions.Reporting.UploadClientFile.View";
            public const string TelecomUploadClientFileDownload = "Permissions.Reporting.UploadClientFile.Download";
            public const string TelecomUploadClientFileUpload = "Permissions.Reporting.UploadClientFile.Upload";
            public const string TelecomUploadClientFileDelete = "Permissions.Reporting.UploadClientFile.Delete";
            public const string TelecomUploadClientFileFilePreview = "Permissions.Reporting.UploadClientFile.FilePreview";
        }
        public static class Carrier
        {
            public const string TelecomCarrierView = "Permissions.Reporting.Carrier.View";
            public const string TelecomCarrierCreate = "Permissions.Reporting.Carrier.Create";
            public const string TelecomCarrierEdit = "Permissions.Reporting.Carrier.Edit";
            public const string TelecomCarrierDelete = "Permissions.Reporting.Carrier.Delete";
        }

        public static class DataServiceType
        {
            public const string TelecomDataServiceTypeView = "Permissions.Reporting.DataServiceType.View";
            public const string TelecomDataServiceTypeCreate = "Permissions.Reporting.DataServiceType.Create";
            public const string TelecomDataServiceTypeEdit = "Permissions.Reporting.DataServiceType.Edit";
            public const string TelecomDataServiceTypeDelete = "Permissions.Reporting.DataServiceType.Delete";
        }
    }
}
