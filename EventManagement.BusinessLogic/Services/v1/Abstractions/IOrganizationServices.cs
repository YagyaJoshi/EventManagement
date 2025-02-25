using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface IOrganizationServices
    {
        Task<long> AddOrganization(OrganizationInput input, string bannerImage, string logo);

        Task<OrganizationCreationDto> CreateOrganization(CreateOrganizationInput input, string logo);

        Task<long> UpdateOrganization(long id, OrganizationUpdateInput input);
        Task<OrganizationListDto> GetAllOrganizations(string sortColumn, string sortOrder, string searchText, int? pageNo, int? pageSize);

        Task<Organization> GetOrganizationById(long id);

        Task<long> DeleteOrganization(long id);

        Task<long> AddOrganizationPayment(long? id, long organizationId, OrganizationPaymentProviderInput input);

        Task<OrganizationPaymentProviders> GetOrganizationPaymentProviders(long organizationId);

        Task<List<OrganizationPaymentProviders>> GetListOrganizationPaymentProviders(long organizationId);
        void UpdateOrganizationLogoImage(long id, string bannerImage, string logo);

        Task<OrganizationDetailsDto> GetOrganizationDetails(long? id, string domain);

        Task<long> UpdateOrganizationDetails(long id, OrganizationDetailsUpdateInput input);

        Task<long> AddVisaFees(long? id, long organizationId, AddVisaFeesInput input);

        Task<OrganizationVisaFeeDto> GetVisaFeesByCountry(long countryId);

        Task<long> AddUpdateOrganizationTypes(long? id,OrganizationTypes input);

        Task<List<OrganizationTypes>> GetOrganizationTypes();

        Task<long> DeleteOrganizationTypes(int id);

        Task<List<Templates>> GetIdCardTemplates();
        Task<long> UpdateIdCardTemplate(long organizationId, IdCardUpdateInput input);

        Task<string> GetIdCardTemplate(long organizationId);

        Task<SubscriptionPlanDto> GetSubscriptionPlan(long organizationId);

        Task<MerchantDetails> MerchantDetails(long organizationId);
    }
}
