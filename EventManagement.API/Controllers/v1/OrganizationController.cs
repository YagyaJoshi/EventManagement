using EventManagement.BusinessLogic.Exceptions;
using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.Utilities.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + EventManagement.BusinessLogic.Version.Value + "/[controller]")]
    public class OrganizationController : BaseController
    {
        private readonly IOrganizationServices _organizationServices;

        public OrganizationController(IOrganizationServices organizationServices, IEmailService emailService) : base(emailService)
        {
            _organizationServices = organizationServices;
        }

        [Route("")]
        [HttpPost]
        [Authorize(Roles = "superAdmin")]
        public async Task<IActionResult> AddOrganization([FromForm] OrganizationInput input)
        {
            List<IFormFile> bannerImage = new List<IFormFile>();
            bannerImage.Add(input.bannerImage);
            var bannerImg = new List<string>();
            if (input.bannerImage != null)
                bannerImg = await RetrieveFiles(bannerImage);

            List<IFormFile> logoImage = new List<IFormFile>();
            logoImage.Add(input.logo);
            var logoImg = await RetrieveFiles(logoImage);

            if (logoImg == null)
                throw new BadRequestException(); 

            return await ExecuteAsync(() => _organizationServices.AddOrganization(input, bannerImg.Any() ? bannerImg.First() :null, logoImg.First()), Resource.ADD_ORGANIZATION_SUCCESS) ;
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromForm] CreateOrganizationInput input)
        {

            List<IFormFile> logoImage = new List<IFormFile>();
            logoImage.Add(input.logo);
            var logoImg = await RetrieveFiles(logoImage);

            if (logoImg == null)
                throw new BadRequestException();

            return await ExecuteAsync(() => _organizationServices.CreateOrganization(input, logoImg.First()), Resource.CREATE_ORGANIZATION_SUCCESS);
        }

        [Route("{id}")]
        [HttpPut]
        [Authorize(Roles = "superAdmin,admin")]
        public async Task<IActionResult> UpdateOrganization([FromRoute] long id, [FromBody] OrganizationUpdateInput input)
        {
            return await ExecuteAsync(() => _organizationServices.UpdateOrganization(id, input),  Resource.UPDATE_ORGANIZATION_SUCCESS);
        }

        [HttpGet]
        [Authorize]
        [Route("all")]
        public async Task<IActionResult> GetAllOrganizations( string sortColumn = null, string sortOrder = null, string searchText = null, int? pageNo = 1, int? pageSize = 10)
        {
            return await ExecuteAsync(() => _organizationServices.GetAllOrganizations(sortColumn, sortOrder, searchText, pageNo, pageSize), Resource.SUCCESS);
        }

        [HttpGet]
        [Authorize(Roles = "superAdmin,admin")]
        [Route("{id}")]
        public async Task<IActionResult> GetOrganizationById([FromRoute] long id)
        {
            return await ExecuteAsync(() => _organizationServices.GetOrganizationById(id), Resource.SUCCESS);
        }

        [HttpDelete]
        [Authorize(Roles = "superAdmin")]
        [Route("{id}")]
        public async Task<IActionResult> DeleteOrganization([FromRoute] long id)
        {
            return await ExecuteAsync(() => _organizationServices.DeleteOrganization(id), Resource.DELETE_ORGANIZATION_SUCCESS);
        }

        [Route("addPaymentProvider")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddOrganizationPayment([FromBody] OrganizationPaymentProviderInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _organizationServices.AddOrganizationPayment(null, organizationId, input), Resource.ADD_PAYMENT_PROVIDER_SUCCESS);
        }

        [Route("updatePaymentProvider/{id}")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateOrganizationPayment([FromRoute] long id, [FromBody] OrganizationPaymentProviderInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _organizationServices.AddOrganizationPayment(id,  organizationId, input), Resource.UPDATE_PAYMENT_PROVIDER_SUCCESS);
        }

        [HttpGet]
        [Authorize]
        [Route("paymentProvider")]
        public async Task<IActionResult> GetOrganizationPaymentProviders()
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _organizationServices.GetOrganizationPaymentProviders(organizationId), Resource.SUCCESS);
        }

        [HttpGet]
        [Authorize]
        [Route("paymentProvider/list")]
        public async Task<IActionResult> GetListOrganizationPaymentProviders()
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _organizationServices.GetListOrganizationPaymentProviders(organizationId), Resource.SUCCESS);
        }

        [HttpGet]
        [Route("public")]
        public async Task<IActionResult> GetOrganizationDetails([FromQuery] long? id = null, [FromQuery] string domain = "")
        {
            return await ExecuteAsync(() => _organizationServices.GetOrganizationDetails(id, domain), Resource.SUCCESS);
        }


        [Route("settings")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateOrganizationDetails([FromBody] OrganizationDetailsUpdateInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;

            return await ExecuteAsync(() => _organizationServices.UpdateOrganizationDetails(organizationId, input), Resource.UPDATE_ORGANIZATION_SUCCESS);
        }


        [Route("visa/add")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddVisaFees([FromBody] AddVisaFeesInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _organizationServices.AddVisaFees(null, organizationId, input), Resource.ADD_VISA_FEES_SUCCESS);
        }

        [Route("visa/update/{id}")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateVisaFees([FromRoute] long id, [FromBody] AddVisaFeesInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _organizationServices.AddVisaFees(id, organizationId, input), Resource.UPDATE_VISA_FEES_SUCCESS);
        }


        [Route("types")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddOrganizationTypes([FromBody] OrganizationTypes input)
        {
            return await ExecuteAsync(() => _organizationServices.AddUpdateOrganizationTypes(null,input), Resource.ADD_ORGANIZATIONTYPES_SUCCESS);
        }

        [Route("types/{id}")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateOrganizationTypes([FromRoute] long id, [FromBody] OrganizationTypes input)
        {
            return await ExecuteAsync(() => _organizationServices.AddUpdateOrganizationTypes(id, input), Resource.UPDATE_ORGANIZATIONTYPES_SUCCESS);
        }

        [Route("types/public/all")]
        [HttpGet]
        public async Task<IActionResult> GetOrganizationTypes()
        {
            return await ExecuteAsync(() => _organizationServices.GetOrganizationTypes(), Resource.SUCCESS);
        }

        [Route("types/{id}")]
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteOrganizationTypes(int id)
        {
            return await ExecuteAsync(() => _organizationServices.DeleteOrganizationTypes(id), Resource.DELETE_ORGANIZATIONTYPE_SUCCESS);
        }

        [Route("template")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateIdCardTemplate([FromBody] IdCardUpdateInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _organizationServices.UpdateIdCardTemplate(organizationId, input), Resource.UPDATE__ACCREDITATION_SUCCESS);
        }

        [HttpGet]
        [Route("template")]
        public async Task<IActionResult> GetIdCardTemplate()
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _organizationServices.GetIdCardTemplate(organizationId), Resource.SUCCESS);
        }

        [HttpGet]
        [Route("subscriptionPlan")]
        [Authorize]
        public async Task<IActionResult> GetSubscriptionPlan()
        {
            long organizationId = 0;
            string userRole = HttpContext.Items["UserRole"] as string ?? string.Empty;;
            if (userRole.ToLower() != "superadmin")
                organizationId = (long)HttpContext.Items["OrganizationId"];
            return await ExecuteAsync(() => _organizationServices.GetSubscriptionPlan(organizationId), Resource.SUCCESS);
        }
    }
}
