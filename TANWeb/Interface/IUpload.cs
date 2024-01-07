using System.Data;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using static TAN.DomainModels.Models.UploadModel;

namespace TANWeb.Interface
{
    public interface IUpload
    {
        Task<DataTable> CheckValidColumn(string[] Column);
        Task<DataTable> CheckFileExtension(IFormFile file);
        Task<DataTable> PreviewData(DataTable data,string FacilityId);
        Task<bool> CheckFileName(string Filename,string facilityId);
        Task<bool> AddUpdateMappingField(MappingRequestData mappingRequestData);
        Task<DataTable> UpdateColumnName(MappingRequestData requestData,DataTable uploadTable);
        Task<bool> BusinessLogic(RequestModel requestData,DataTable FileTable);
        Task<Int32> AddFilenName(string filename, string FacilityID, int RowCount,RequestModel requestModel);
        Task<bool> HasDuplicateColumns(MappingRequestData requestData);
        Task<List<CMSMappingFieldData>> GetAllCMSFieldData();
        Task<List<string>> HasPresentRequiredCmsColumn(List<MappingData> MappingDatas);
        Task<List<Employee>> CheckEmployee(DataTable dataTable, int FacilityId);
        static UploadFileDetailsWithFilter uploadFileDetails = new UploadFileDetailsWithFilter();
        Task<ValidatDataResponse> ReValidatedWorkingHours(ErrorData errorData);
        Task<ImportFileResponse> ImportEmployee(DataTable dataTable, int facilityId);
        Task<DataTable> CheckOneDriveFileExtension(Stream file, string fileName);
        int GetQuarter(DateTime dateTime);
        Task<bool> KronosLogic(DataTable FileTable,int? count=0);
        Task AddKronosTrail(string table, string description, int status, int? count = 0, int recordsCount = 0);
    }
}
