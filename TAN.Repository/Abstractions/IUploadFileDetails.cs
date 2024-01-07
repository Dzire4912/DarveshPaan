using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Models;
using TAN.DomainModels.ViewModels;
using static TAN.DomainModels.Models.UploadModel;

namespace TAN.Repository.Abstractions
{
    public interface IUploadFileDetails : IRepository<UploadFileDetails>
    {
        List<UploadFileDetails> GetUploadFileDetails(int FacilityId, int PageSize, int PageNo, out int TotalCount);
        Task<UploadFileDetails> GetUploadFileDetailsByFileName(string Filename, string facilityId);
        Task<UploadFileDetails> AddFileDetails(string Filename, string facilityId, int RowCount, RequestModel requestModel);
        List<FileDetailsResponse> GetFileRecord(BackoutTableRequest request, int PageSize, int PageNo, out int TotalCount, string sortOrder);
        Task<BackOutAllRecordResponse> BackOutAllRecord(BackOutAllRecordRequest request);
        List<FileErrorList> GetFileErrorList(BackOutAllRecordRequest request, int PageSize, int PageNo, out int TotalCount, string sortOrder);
        Task<ErrorData> GetErrorRawData(int Id);
        Task<bool> DeleteErrorRawData(int Id);
        Task<bool> DeleteInvalidRecord(int FileDetailId);
        List<DynamicDataTableModel> ListValidationByFileId(ValidationErrorListRequest request, out int totalCount);
        Task<List<FileDetailList>> GetAllFileDetails(FileDetailListRequest request);
        Task<UploadFileIdResponse> GetUploadFileDetailsByFileId(int fileId);
        List<ValidDataViewModel> GetFileValidRecordList(BackOutAllRecordRequest request, int pageSize, int pageNo, out int totalCount, string sortOrder);
        DataTable GetInvalidErrorFileDataForCsv(int facilityId, int reportQuarter, int year, int fileDetailsId);
        DataTable GetValidFileDataForCsv(int facilityId, int fileDetailsId);
        Task<FileDetailList> GetFileDetails(FileDetailListRequest request);
        DataTable GetInvalidErrorFileDynamicDataForCsv(int fileDetailsId);
        string GetFacilityName(int facilityId);
        Task<List<ValidDataViewModel>> GetWorkDayListByEmployee(EmployeeWorkDayRequest request);
    }
}
