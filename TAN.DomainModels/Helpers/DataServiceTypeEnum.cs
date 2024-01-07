using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Helpers
{
    public enum ServiceTypeStatus
    {
        Success = 200,
        AllReadyExist = 405,
        DataUpdated = 203,
        Failed = 400,
        DataDeleted = 204,
        Error = 401,
        NotExist = 406
    }
}
