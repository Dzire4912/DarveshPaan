using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.Repository.Abstractions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TAN.DomainModels.Models.UploadModel;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class UserMappingFieldDataRepository : Repository<UserMappingFieldData>, IUserMappingFieldData
    {
        private DatabaseContext Context
        {
            get
            {
                return db as DatabaseContext;
            }
        }

        public UserMappingFieldDataRepository(DbContext db)
        {
            this.db = db;
        }

        public async Task<List<UserMappingFieldData>> GetUserMappingFieldDataByFacility(int FacilityId)
        {
            List<UserMappingFieldData> model= new List<UserMappingFieldData>();
            try
            {
                model = await Context.UserMappingFieldData.Where(x => x.IsActive && x.FacilityId == FacilityId).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetUserMappingFieldDataByFacility :{ErrorMsg}", ex.Message);
            }
            return model;
        }
        public async Task<bool> GetUserMappingFieldDataFindAddUpdate(MappingRequestData mappingRequestData)
        {
            try
            {
                foreach (MappingData item in mappingRequestData.MappingDatas)
                {
                    UserMappingFieldData fieldData = await Context.UserMappingFieldData.Where(x => x.IsActive && x.ColumnName == item.ColumnName
                    && x.FacilityId == Convert.ToInt32(mappingRequestData.requestModel.FacilityId)).FirstOrDefaultAsync();
                    if (fieldData != null)
                    {
                        fieldData.UpdatedDate = DateTime.Now;
                        fieldData.FacilityId = Convert.ToInt32(mappingRequestData.requestModel.FacilityId);
                        fieldData.ColumnName = item.ColumnName;
                        fieldData.CMSMappingId = item.CmsMappingID; 
                        Context.UserMappingFieldData.Update(fieldData);
                        await Context.SaveChangesAsync();
                    }
                    else
                    {
                        UserMappingFieldData userMapping = new UserMappingFieldData()
                        {
                            CreatedDate = DateTime.Now,
                            FacilityId = Convert.ToInt32(mappingRequestData.requestModel.FacilityId),
                            ColumnName = item.ColumnName,
                            CMSMappingId = item.CmsMappingID,
                            IsActive = true
                        };
                        await Context.UserMappingFieldData.AddAsync(userMapping); 
                        await Context.SaveChangesAsync();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An Error Occured in GetUserMappingFieldDataFindAddUpdate :{ErrorMsg}", ex.Message);
                throw;
            }
        }
    }
}
