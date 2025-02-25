using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.BusinessLogic.Services.v1.Implementations;
using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.Utilities.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + EventManagement.BusinessLogic.Version.Value + "/[controller]")]
    public class AdminController : BaseController
    {
        private readonly IAdminServices _adminServices;

        private readonly ICustomerServices _customerServices;
        public AdminController(IAdminServices adminServices,ICustomerServices customerServices, IEmailService emailService) : base(emailService)
        {
            _adminServices = adminServices;
            _customerServices = customerServices;
        }

        [Route("accreditation/template")]
        [HttpPost]
        [Authorize]
        public Task<IActionResult> AddAccreditationTemplate([FromBody] AccreditationInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return ExecuteAsync(() => _adminServices.AddAccreditationTemplate(organizationId,input), Resource.ADD_ACCREDITATION_SUCCESS);
        }

        [Route("accreditation/all")]
        [HttpGet]
        [Authorize]
        public Task<IActionResult> GetAccreditationTemplate(int? pageNo, int? pageSize, string? sortOrder)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return ExecuteAsync(() => _adminServices.GetAccreditationTemplate(organizationId, pageNo, pageSize, sortOrder), Resource.SUCCESS);
        }
    }
}
