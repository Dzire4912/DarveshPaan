using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TANWeb.Areas.TelecomReporting.Controllers
{
    [Area("telecomreporting")]
    [Authorize]
    public class DataServiceReportsController : Controller
    {
        [Route("telecomreporting/reports/invoicesummaryreport")]

        public IActionResult InvoiceSummaryReport()
        {
            return View();
        }
        [Route("telecomreporting/reports/paymentcompliancereport")]

        public IActionResult PaymentComplianceReport()
        {
            return View();
        }

        [Route("telecomreporting/reports/ageingreport")]
        public IActionResult AgeingReport()
        {
            return View();
        }
    }
}
