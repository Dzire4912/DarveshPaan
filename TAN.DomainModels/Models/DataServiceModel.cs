using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.DomainModels.Entities;
using TAN.DomainModels.ViewModels;

namespace TAN.DomainModels.Models
{
    public class DataServiceModel
    {
        public List<FacilityLocationAccess> facilityLocationAccesses { get; set; } = new List<FacilityLocationAccess>();

        public AddDataService addDataService { get; set; } = new AddDataService();
        public List<SelectListText> DataServiceTypeList { get; set; }  = new List<SelectListText>();
        public List<SelectListText> ServiceList { get; set; } = new List<SelectListText>();
    }

    public class AddDataService
    {
        public long Id { get; set; }
        public List<FacilityLocationAccess> facilityLocationAccesses { get; set; }
        public string? ProviderName { get; set; }
        public string? AccountNumber { get; set; }
        public string? LocalIP { get; set; }
        public int ServicesId { get; set; }
        public string? SupportNumber { get; set; }
        public string? ContractInformation { get; set; }
        public string? ContractDocument { get; set; }
        public DateTime ContractEndDate { get; set; }
        public string? ContractStatus { get; set; }
        public bool IsActive { get; set; }
    }

    public class DataServiceListRequest
    {
        public string? facilityId { get; set; }
        public string? serviceId { get; set; }
    }

    public class DataServiceRequest
    {
        public string? Id { get; set; }
        public string? FacilityId { get; set; }
        public string? ProviderName { get; set; }
        public string? AccountNumber { get; set; }
        public string? LocalIP { get; set; }
        public int ServicesId { get; set; }
        public string? SupportNumber { get; set; }
        public string? ContractDocument { get; set; }
        public DateTime ContractEndDate { get; set; }
        public bool ContractStatus { get; set; }
    }

    public class CheckDataServiceFileName
    {
        public int FacilityId { get; set; }
        public string? filename { get; set; }
    }

    public class DataServiceResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
    }

    public class GetDataServiceListRequest
    {
        public string? facilityId { get; set; }
        public string? serviceId { get; set; }
        public string? sortOrder { get; set; }
        public int skip { get; set; }
        public int PageSize { get; set; }
        public string? search { get; set; }
    }

    public class CheckAccountNumberRequest
    {
        public int FacilityId { get; set; }
        public string? AccountNo { get; set; }
        public int DataServiceId { get; set; }
    }

    public class DownloadDataServiceContractFile
    {
        public string? FacilityId { get; set; }
        public string? DataServiceId { get; set; }
    }

}
