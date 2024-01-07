using TAN.DomainModels.Entities; 

namespace TAN.Repository.Abstractions
{
    public interface IJobCodes : IRepository<JobCodes>
    {
        Task<List<JobCodes>> GetAllJobCodes();
    }
}
