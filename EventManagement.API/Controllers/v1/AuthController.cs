using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.BusinessLogic.Services.v1.Implementations;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.Utilities.Email;
using EventManagement.Utilities.FireBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + EventManagement.BusinessLogic.Version.Value + "/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthServices _authServices;
        public AuthController(IAuthServices authServices, IEmailService emailService) : base(emailService)
        {
            _authServices = authServices;
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] SignIn signIn)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _authServices.Login(organizationId, signIn), Resource.SIGNIN_SUCCESS);
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] SignUp signUp)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _authServices.Register(organizationId, signUp), Resource.SIGNUP_SUCCESS);
        }

        [HttpPut]
        [Authorize]
        [Route("changePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordInput input)
        {
            return await ExecuteAsync(() => _authServices.ChangePassword(input), Resource.CHANGE_PASSWORD_SUCCESS);
        }

        [HttpPost]
        [Route("generateOTP")]
        public async Task<IActionResult> GenerateAndSaveOTP([FromBody] ForgetPasswordInput input)
        {
            return await ExecuteAsync(() => _authServices.GenerateAndSaveOTP(input), Resource.OTP_SUCCESS);
        }

        [HttpPost]
        [Route("verifyOtpAndPassword")]
        public async Task<IActionResult> VerifyOtpandPassword([FromBody] VerifyOtpInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _authServices.VerifyOtpandPassword(organizationId, input), Resource.CHANGE_PASSWORD_SUCCESS);
        }


        //[HttpGet]
        //[Route("send-account-info/{bookingId}")]
        //[Authorize]
        //public async Task<IActionResult> SendAccountInformationEmail([FromRoute] long bookingId)
        //{
        //    try
        //    {
        //        // Call the method to generate the HTML for account information
        //        long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
        //        var userId = (HttpContext.Items["UserId"] as long?) ?? 0;
        //        var htmlContent = await _authServices.SendAccountInformation(bookingId, organizationId, userId);

        //        // Return the HTML content as the response
        //        return Ok(htmlContent); // This will return the HTML as a string
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        //[HttpGet]
        //[Authorize]
        //[Route("{organizationId}")]
        //public async Task<IActionResult> GetFcmToken(long organizationId)
        //{
        //    var userId = (HttpContext.Items["UserId"] as long?) ?? 0;
        //    return await ExecuteAsync(() => _authServices.GetFcmToken(organizationId, userId), Resource.SUCCESS);
        //}

        //[Route("booking/{bookingId}")]
        //[HttpGet]
        //public async Task<IActionResult> SendMailToCustomer([FromRoute] long bookingId)
        //{
        //    return await ExecuteAsync(() => _authServices.SendMailToCustomer(bookingId), Resource.SUCCESS);
        //}
    }
}