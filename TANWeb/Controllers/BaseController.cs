using Microsoft.AspNetCore.Mvc;
using TAN.Repository.Abstractions;

namespace TANWeb.Controllers
{
    public class BaseController : Controller
    {
        protected IUnitOfWork uow;
        public BaseController(IUnitOfWork _uow)
        {
            uow = _uow;

        }
    }
}
