using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;

namespace TAN.Repository.Abstractions
{
    public interface IFileErrorRecordsRepository
    {
        Task<FileErrorRecords> GetAllErrRecords();
        Task<bool> InsertErrorValidationRecords(FileErrorRecords fileErrorRecords);
        Task<bool> UpdateErrorValidationRecords(int ErrorId);
        Task<bool> InsertErrorValidationRecordsAddRange(List<FileErrorRecords> FileErrorRecordsList);
    }
}
