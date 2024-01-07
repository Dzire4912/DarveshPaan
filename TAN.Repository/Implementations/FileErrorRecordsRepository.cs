using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class FileErrorRecordsRepository : Repository<FileErrorRecords>, IFileErrorRecordsRepository
    {
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public FileErrorRecordsRepository(DbContext db)
        {
            this.db = db;
        }
        public async Task<FileErrorRecords> GetAllErrRecords()
        {
            FileErrorRecords fileErrorRecords = new FileErrorRecords(); 
            try
            {
                //Get all the error records
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetAllErrRecords :{ErrorMsg}", ex.Message); 
            }
            return fileErrorRecords;
        }

        public async Task<bool> InsertErrorValidationRecords(FileErrorRecords fileErrorRecords)
        { 
            try
            {
                if(fileErrorRecords != null)
                {
                    Context.FileErrorRecords.Add(fileErrorRecords);
                    Context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in InsertErrorValidationRecords :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public async Task<bool> UpdateErrorValidationRecords(int ErrorId)
        {
            try
            {
                if (ErrorId != 0)
                {
                    var data = await Context.FileErrorRecords.Where(x => x.IsActive && x.Id == ErrorId).FirstOrDefaultAsync();
                    if (data != null)
                    {
                        data.Status = 1;
                        Context.FileErrorRecords.Update(data);
                        Context.SaveChanges();
                        return true;
                    } 
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in InsertErrorValidationRecords :{ErrorMsg}", ex.Message);
            }
            return false;
        }

        public async Task<bool> InsertErrorValidationRecordsAddRange(List<FileErrorRecords> FileErrorRecordsList)
        {
            try
            {
                if (FileErrorRecordsList.Count > 0)
                {
                    Context.FileErrorRecords.AddRange(FileErrorRecordsList);
                    Context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in InsertErrorValidationRecordsAddRange :{ErrorMsg}", ex.Message);
            }
            return false;
        }
    }
}
