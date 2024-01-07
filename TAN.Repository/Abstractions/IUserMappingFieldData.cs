using TAN.DomainModels.Entities;
using static TAN.DomainModels.Models.UploadModel;

namespace TAN.Repository.Abstractions
{
    public interface IUserMappingFieldData
    {
        Task<List<UserMappingFieldData>> GetUserMappingFieldDataByFacility(int FacilityId);

        Task<bool> GetUserMappingFieldDataFindAddUpdate(MappingRequestData mappingRequestData);

    }
}
