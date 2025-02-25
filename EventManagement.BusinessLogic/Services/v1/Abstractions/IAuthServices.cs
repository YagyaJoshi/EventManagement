using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface IAuthServices
    {
        Task<LoginResponseDto> Login(long? organizationId, SignIn signIn);

        Task<LoginResponseDto> Register(long organizationId, SignUp signUp);
        Task ChangePassword(ChangePasswordInput input);

        Task<CustomerResponseDto> LoginCustomer(SignIn signIn);

        Task<GenerateOTPDto> GenerateAndSaveOTP(ForgetPasswordInput input);

        Task<string> VerifyOtpandPassword(long organizationId, VerifyOtpInput input);

        Task<Email> SendMailToCustomer(long bookingId);

        Task<string> SendAccountInformation(long bookingId, long? organizationid, long? userId);

        Task<NotificationDetails> GetFcmToken(long organizationId, long userId);

        Task<NotificationDetails> GetFcmTokenForOrganization(long organizationId, long userId);
        Task<NotificationDetails> GetFcmTokenForCustomer(long organizationId, long userId);
        Task<string> UpdatePaymentDetails(long bookingId, long? organizationid, long? userId);
    }
}
