using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.BusinessLogic.Services.v1.Implementations;
using EventManagement.Utilities.Email;
using EventManagement.Utilities.FireBase;
using EventManagement.Utilities.Jwt;
using EventManagement.Utilities.Payment.Mastercard;
using EventManagement.Utilities.Storage.AlibabaCloud;

namespace EventManagement.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void RegisterDI(this IServiceCollection services)
        {
            services.AddScoped<IAdminServices, AdminServices>();
            services.AddScoped<IAuthServices, AuthServices>();
            services.AddScoped<IJwtGenerator, JwtGenerator>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<IOrganizationServices, OrganizationServices>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IStaffServices, StaffServices>();
            services.AddScoped<IEventServices, EventServices>();
            services.AddScoped<IMasterServices, MasterServices>();
            services.AddScoped<IStorageServices, ObjectStorageServices>();
            services.AddScoped<IHotelServices, HotelServices>();
            services.AddScoped<ISubscriptionServices, SubscriptionServices>();
            services.AddScoped<IStripeServices, StripeServices>();
            services.AddScoped<ICustomerServices, CustomerServices>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IMastercardPaymentService, MastercardPaymentService>();
            services.AddScoped<IReportsServices, ReportsServices>();
            services.AddScoped<IFirebaseServices, FirebaseServices>();
        }
    }
}
