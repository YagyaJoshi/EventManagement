using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.Utilities.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + EventManagement.BusinessLogic.Version.Value + "/[controller]")]
    public class ReportsController : BaseController
    {
        public readonly IReportsServices _reportServices;
        public ReportsController(IEmailService emailService, IReportsServices reportServices) : base(emailService)
        {
            _reportServices = reportServices;
        }

        [HttpGet]
        [Authorize(Roles = "superAdmin, admin, transportationStaff, financeStaff, accommodationStaff, supportStaff")]
        [Route("admin/statistics/summary")]
        public async Task<IActionResult> GetSummaryStatistics([FromQuery] long eventId)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _reportServices.GetSummaryStatistics(organizationId, eventId), Resource.SUCCESS);
        }

        [HttpGet]
        [Authorize(Roles = "superAdmin, admin, transportationStaff, financeStaff, accommodationStaff, supportStaff")]
        [Route("admin/booking/pie-chart")]
        public async Task<IActionResult> GetBookingStatsPie([FromQuery] long eventId, [FromQuery] int year, [FromQuery] int? month)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _reportServices.GetBookingStatsPie(organizationId, eventId, year, month), Resource.SUCCESS);
        }

        [HttpGet]
        [Authorize(Roles = "superAdmin, admin, transportationStaff, financeStaff, accommodationStaff, supportStaff")]
        [Route("admin/booking/bar-graph")]
        public async Task<IActionResult> GetBookingsBarGraphReport([FromQuery] long eventId, [FromQuery] int year, [FromQuery] int? month)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _reportServices.GetBookingsBarGraphReport(organizationId, eventId, year, month), Resource.SUCCESS);
        }

        [HttpGet]
        [Authorize(Roles = "superAdmin, admin")]
        [Route("superadmin/statistics/summary")]
        public async Task<IActionResult> GetOrganizationSummary([FromQuery] long? organizationId)
        {
            return await ExecuteAsync(() => _reportServices.GetOrganizationSummary(organizationId), Resource.SUCCESS);
        }

        [HttpGet]
        [Authorize(Roles = "superAdmin, admin")]
        [Route("superadmin/booking/revenue-bar-graph")]
        public async Task<IActionResult> GetBookingsRevenueReport([FromQuery] long? organizationId, [FromQuery] int year, [FromQuery] int? month)
        {
            return await ExecuteAsync(() => _reportServices.GetBookingsRevenueReport(organizationId, year, month), Resource.SUCCESS);
        }

        [HttpGet]
        [Authorize(Roles = "superAdmin, admin, transportationStaff, financeStaff, accommodationStaff, supportStaff")]
        [Route("admin/payment/summary")]
        public async Task<IActionResult> GetPaymentSummaryReport([FromQuery] long? eventId, [FromQuery] int? year, [FromQuery] int? month, [FromQuery] string? date)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _reportServices.GetPaymentSummary(organizationId,  eventId, year, month, date), Resource.SUCCESS);
        }
    }
}
