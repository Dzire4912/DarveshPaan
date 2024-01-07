using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAN.ApplicationCore.Migrations;
using TAN.DomainModels.Entities;

namespace TAN.Repository.Abstractions
{
    public interface IOneTimeDeduction:IRepository<KronosOneTimeUpdate>
    {
    }
}
