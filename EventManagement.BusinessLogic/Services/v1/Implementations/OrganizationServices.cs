using EventManagement.DataAccess;
using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.Utilities.Helpers;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.Utilities.Email;
using EventManagement.DataAccess.ViewModels.Dtos;
using System.Text.Json;
using EventManagement.BusinessLogic.Exceptions;
using EventManagement.BusinessLogic.Helpers;
using EventManagement.Utilities.Storage.AlibabaCloud;
using EventManagement.DataAccess.Enums;
using EventManagement.DataAccess.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.Memory;
using EventManagement.Utilities.Jwt;

namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class OrganizationServices : IOrganizationServices
    {
        private readonly IConfiguration _configuration;

        private readonly IEmailService _emailService;

        private readonly IStorageServices _storageServices;

        private readonly IMasterServices _masterServices;

        private readonly IHostEnvironment _env;
        private readonly IMemoryCache _memoryCache;
        private readonly IJwtGenerator _jwtGenerator;

        public OrganizationServices(IConfiguration configuration, IEmailService emailService, IStorageServices storageServices, IMasterServices masterServices, IHostEnvironment env, IMemoryCache memoryCache, IJwtGenerator jwtGenerator)
        {
            _configuration = configuration;
            _emailService = emailService;
            _storageServices = storageServices;
            _masterServices = masterServices;
            _env = env;
            _memoryCache = memoryCache;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<long> AddOrganization(OrganizationInput input, string bannerImage, string logo)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_InsertOrUpdateOrganization");
            try
            {
                // hash password
                var PasswordHash = PasswordHasher.HashPassword(input.password);
                var theme = input.theme != null ? JsonSerializer.Serialize(input.theme) : null;

                var status = StatusExtensions.ToStatusEnum(input.status.ToLower()) ?? throw new ServiceException($"Invalid status: {input.status}");

                objCmd.Parameters.AddWithValue("@OrganizationName", input.organizationName);
                objCmd.Parameters.AddWithValue("@Theme", theme);
                objCmd.Parameters.AddWithValue("@Phone", input.phone);
                objCmd.Parameters.AddWithValue("@SupportEmail", input.email.ToLower());
                objCmd.Parameters.AddWithValue("@Password", PasswordHash);
                objCmd.Parameters.AddWithValue("@DomainName", input.domainName);
                objCmd.Parameters.AddWithValue("@Website", input.website);
                objCmd.Parameters.AddWithValue("@BannerHeading", input.bannerHeading);
                objCmd.Parameters.AddWithValue("@BannerSubHeading", input.bannerSubHeading);
                objCmd.Parameters.AddWithValue("@Status", status);

                DataTable dtUser = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtUser.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                var organizationId = Convert.ToInt64(dtUser.Rows[0]["OrganizationId"]);

                var bannerImg = string.Empty;
                if (!string.IsNullOrEmpty(bannerImage))
                    bannerImg = _storageServices.UploadFile(bannerImage, "Organization", organizationId).Result;

                var logoImg = _storageServices.UploadFile(logo, "Organization", organizationId).Result;
                UpdateOrganizationLogoImage(organizationId, bannerImg, logoImg);

                var registrationEmailText = CommonUtilities.GetEmailTemplateText(_env.ContentRootPath + Path.DirectorySeparatorChar.ToString() + "EmailTemplates" + Path.DirectorySeparatorChar.ToString() + "organization-register.html");

                registrationEmailText = string.Format(registrationEmailText, input.email, input.password, input.organizationName, logoImg);

                if (!string.IsNullOrEmpty(input.email))
                    _emailService.SendEmail(input.email, "Welcome", registrationEmailText);
                

                return organizationId;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        } 

        public async Task<OrganizationCreationDto> CreateOrganization(CreateOrganizationInput input, string logo)
        {
            
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_InsertOrUpdateOrganization");
            try
            {
                // hash password
                var PasswordHash = PasswordHasher.HashPassword(input.password);
                var theme = input.theme != null ? JsonSerializer.Serialize(input.theme) : null;

                objCmd.Parameters.AddWithValue("@OrganizationName", input.organizationName);
                objCmd.Parameters.AddWithValue("@Theme", theme);
                objCmd.Parameters.AddWithValue("@Phone", input.phone);
                objCmd.Parameters.AddWithValue("@SupportEmail", input.email.ToLower());
                objCmd.Parameters.AddWithValue("@Password", PasswordHash);
                objCmd.Parameters.AddWithValue("@Status", 2);

                DataTable dtUser = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtUser.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                var organizationId = Convert.ToInt64(dtUser.Rows[0]["OrganizationId"]);

                var logoImg = _storageServices.UploadFile(logo, "Organization", organizationId).Result;
                UpdateOrganizationLogoImage(organizationId,null, logoImg);

                var registrationEmailText = CommonUtilities.GetEmailTemplateText(_env.ContentRootPath + Path.DirectorySeparatorChar.ToString() + "EmailTemplates" + Path.DirectorySeparatorChar.ToString() + "organization-create.html");

                registrationEmailText = string.Format(registrationEmailText, input.email, input.password, input.organizationName, logoImg);

                if (!string.IsNullOrEmpty(input.email))
                    _emailService.SendEmail(input.email, "Welcome", registrationEmailText);


                var superAdminEmailText = CommonUtilities.GetEmailTemplateText(_env.ContentRootPath + Path.DirectorySeparatorChar.ToString() + "EmailTemplates" + Path.DirectorySeparatorChar.ToString() + "organization-support.html");

                superAdminEmailText = string.Format(superAdminEmailText, input.organizationName, input.email, input.phone, logoImg);

                _emailService.SendEmail("support@ilumis.com", "New Organization Added", superAdminEmailText);

                

                // Generate JWT token
                var jwtToken = _jwtGenerator.GenerateJwtToken(organizationId, "Admin");

                return new OrganizationCreationDto
                {
                    OrganizationId = organizationId,
                    Token = jwtToken
                };
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<long> UpdateOrganization(long id, OrganizationUpdateInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_InsertOrUpdateOrganization");
            try 
            {
                var theme = input.Theme != null ? JsonSerializer.Serialize(input.Theme) : null;
                var status = StatusExtensions.ToStatusEnum(input.Status.ToLower()) ?? throw new ServiceException($"Invalid status: {input.Status}");

                objCmd.Parameters.AddWithValue("@Id", id);
                objCmd.Parameters.AddWithValue("@OrganizationName", input.OrganizationName);
                objCmd.Parameters.AddWithValue("@Theme", theme);
                objCmd.Parameters.AddWithValue("@Phone", input.Phone);
                objCmd.Parameters.AddWithValue("@SupportEmail", input.Email.ToLower());
                objCmd.Parameters.AddWithValue("@Logo", input.Logo);
                objCmd.Parameters.AddWithValue("@DomainName", input.DomainName);
                objCmd.Parameters.AddWithValue("@Website", input.Website);
                objCmd.Parameters.AddWithValue("@Status", status);
                objCmd.Parameters.AddWithValue("@VisaFees", input.VisaFees);

                DataTable dtUser = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtUser.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                long organizationId = Convert.ToInt64(dtUser.Rows[0]["OrganizationId"]);

                // Invalidate the cache for this organization
                string cacheKey = $"OrganizationDetails-{input.DomainName.ToLower()}";
                string cacheKeyForId = $"OrganizationDetails-{id}";

                _memoryCache.Remove(cacheKey);
                _memoryCache.Remove(cacheKeyForId);

                return organizationId;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<OrganizationListDto> GetAllOrganizations(string sortColumn, string sortOrder, string searchText, int? pageNo, int? pageSize)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllOrganizations");
            try
            {
                objCmd.Parameters.AddWithValue("@PageNumber", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SearchText", searchText);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var organizations = new List<Organization>();
                var totalCount = 0;
                if (dt.Rows.Count > 0)
                {
                    organizations = (from DataRow dr in dt.Rows
                                     select new Organization
                                     {
                                         Id = Convert.ToInt64(dr["Id"]),
                                         OrganizationName = dr["Name"].ToString(),
                                         Logo = dr["Logo"].ToString(),
                                         BannerImage = dr["BannerImage"].ToString(),
                                         BannerHeading = dr["BannerHeading"].ToString(),
                                         BannerSubHeading = dr["BannerSubHeading"].ToString(),
                                         Theme = dr["Theme"].ToString() != null ? JsonSerializer.Deserialize<Theme>(dr["Theme"].ToString()) : null,
                                         Website = dr["Website"].ToString(),
                                         Email = dr["SupportEmail"].ToString(),
                                         Phone = dr["Phone"].ToString(),
                                         DomainName = dr["DomainName"].ToString(),
                                         Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                         DefaultCurrency = dr["CurrencyId"] != DBNull.Value ? _masterServices.GetCurrencies().Result.FirstOrDefault(e => e.Id == Convert.ToInt32(dr["CurrencyId"])) : null,
                                         DisplayCurrency = dr["DisplayCurrencyId"] != DBNull.Value ? _masterServices.GetCurrencies().Result.FirstOrDefault(e => e.Id == Convert.ToInt32(dr["DisplayCurrencyId"])) : null,  // Default to 1
                                         DisplayCurrencyRate = dr["DisplayCurrencyRate"] != DBNull.Value ? Convert.ToDecimal(dr["DisplayCurrencyRate"]) : (decimal?)null,
                                         VisaFees = Convert.ToDecimal(dr["VisaFees"])
                                     }).ToList();


                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                }

                return new OrganizationListDto { List = organizations, TotalCount = totalCount };
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<Organization> GetOrganizationById(long id)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetOrganizationById");

            try
            {
                objCmd.Parameters.AddWithValue("@Id", id);
                DataTable dt = await objSQL.FetchDT(objCmd);

                var organization = (from DataRow dr in dt.Rows
                                    select new Organization
                                    {
                                        Id = Convert.ToInt64(dr["Id"]),
                                        OrganizationName = dr["Name"].ToString(),
                                        Logo = dr["Logo"].ToString(),
                                        BannerImage = dr["BannerImage"].ToString(),
                                        BannerHeading = dr["BannerHeading"].ToString(),
                                        BannerSubHeading = dr["BannerSubHeading"].ToString(),
                                        Theme = dr["Theme"].ToString() != null ? JsonSerializer.Deserialize<Theme>(dr["Theme"].ToString()) : null,
                                        Website = dr["Website"].ToString(),
                                        Email = dr["SupportEmail"].ToString(),
                                        Phone = dr["Phone"].ToString(),
                                        DomainName = dr["DomainName"].ToString(),
                                        Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                        DefaultCurrency = dr["CurrencyId"] != DBNull.Value ? _masterServices.GetCurrencies().Result.FirstOrDefault(e => e.Id == Convert.ToInt32(dr["CurrencyId"])) : null,
                                        DisplayCurrency = dr["DisplayCurrencyId"] != DBNull.Value ? _masterServices.GetCurrencies().Result.FirstOrDefault(e => e.Id == Convert.ToInt32(dr["DisplayCurrencyId"])) : null,  // Default to 1
                                        DisplayCurrencyRate = dr["DisplayCurrencyRate"] != DBNull.Value ? Convert.ToDecimal(dr["DisplayCurrencyRate"]) : (decimal?)null,
                                        VisaFees = dr["VisaFees"] != DBNull.Value ? Convert.ToDecimal(dr["VisaFees"]) : (decimal?)null
                                    }).FirstOrDefault();

                Assertions.IsNotNull(organization, Resources.Resource.DATABASE_ERROR_1037);

                return organization;

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<long> DeleteOrganization(long id)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DeleteOrganization");

            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("Id", id);
                await objSQL.UpdateDB(objCmd);
                return id;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async void UpdateOrganizationLogoImage(long id, string bannerImage, string logo)                                                    
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateOrganizationImages");

            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@OrganizationId", id);
                if(!string.IsNullOrEmpty(bannerImage))
                    objCmd.Parameters.AddWithValue("@BannerImage", bannerImage);
                objCmd.Parameters.AddWithValue("@Logo", logo);
                await objSQL.UpdateDB(objCmd);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<long> AddOrganizationPayment(long? id, long organizationId, OrganizationPaymentProviderInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddUpdateOrganizationPayment");
            try
            {

                if (id.HasValue)
                    objCmd.Parameters.AddWithValue("@Id", id);

                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@PaymentMethod", input.PaymentMethod);
                objCmd.Parameters.AddWithValue("@MerchantId", input.MerchantId);
                objCmd.Parameters.AddWithValue("@ApiPassword", input.ApiPassword);
                objCmd.Parameters.AddWithValue("@BankDetails", input.BankDetails);
                objCmd.Parameters.AddWithValue("@IsProduction", input.IsProduction);
                objCmd.Parameters.AddWithValue("@PaymentUrl", _configuration["MasterCardApi:BaseUrl"]);
                objCmd.Parameters.AddWithValue("@ApiVersion", _configuration["MasterCardApi:Version"]);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dt.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return Convert.ToInt64(dt.Rows[0]["OrganizationPaymentId"]);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<OrganizationPaymentProviders> GetOrganizationPaymentProviders(long organizationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetOrganizationPaymentProviders");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                DataTable dt = await objSQL.FetchDT(objCmd);

                var organizationPayment = (from DataRow dr in dt.Rows
                                           select new OrganizationPaymentProviders
                                           {
                                               Id = Convert.ToInt64(dr["Id"]),
                                               OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                               PaymentMethod = dr["PaymentMethod"].ToString(),
                                               MerchantId = Convert.ToString(dr["MerchantId"]),
                                               ApiPassword = Convert.ToString(dr["ApiPassword"]),
                                               BankDetails = Convert.ToString(dr["BankDetails"]),
                                               IsProduction = Convert.ToBoolean(dr["IsProduction"]),
                                               CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                               UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : null,
                                               PaymentUrl = Convert.ToString(dr["PaymentUrl"]),
                                               ApiVersion = Convert.ToInt32(dr["ApiVersion"])
                                           }).FirstOrDefault();

                return organizationPayment;

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<OrganizationDetailsDto> GetOrganizationDetails(long? id, string domain)
        {

            if (id == null && string.IsNullOrEmpty(domain))
            {
                throw new ServiceException(Resources.Resource.ID_DOMAIN_REQUIRED);
            }

            // Define a cache key using the organization ID
            string cacheKey = !string.IsNullOrEmpty(domain)
                                ? $"OrganizationDetails-{domain.ToLower()}"
                                : $"OrganizationDetails-{id}";


            // Check if the data is in the cache
            if (!_memoryCache.TryGetValue(cacheKey, out OrganizationDetailsDto cachedOrganization))
            {
                SQLManager objSQL = new SQLManager(_configuration);
                SqlCommand objCmd = new SqlCommand("sp_GetOrganizationByDomain");

                try
                {
                    if(id > 0)
                        objCmd.Parameters.AddWithValue("@Id", id);
                    if(!string.IsNullOrEmpty(domain))
                        objCmd.Parameters.AddWithValue("@Domain", domain);

                    DataTable dt = await objSQL.FetchDT(objCmd);

                    var organization = (from DataRow dr in dt.Rows
                                        select new OrganizationDetailsDto
                                        {
                                            Id = Convert.ToInt64(dr["Id"]),
                                            OrganizationName = dr["Name"].ToString(),
                                            Logo = dr["Logo"].ToString(),
                                            BannerImage = dr["BannerImage"].ToString(),
                                            BannerHeading = dr["BannerHeading"].ToString(),
                                            BannerSubHeading = dr["BannerSubHeading"].ToString(),
                                            Theme = dr["Theme"].ToString() != null ? JsonSerializer.Deserialize<Theme>(dr["Theme"].ToString()) : null,
                                            Website = dr["Website"].ToString(),
                                            Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                            DomainName = dr["DomainName"].ToString(),
                                            DisplayCurrency = dr["DisplayCurrencyId"] != DBNull.Value ? _masterServices.GetCurrencies().Result.FirstOrDefault(e => e.Id == Convert.ToInt32(dr["DisplayCurrencyId"])) : null,
                                            DefaultCurrency =  _masterServices.GetCurrencies().Result.FirstOrDefault(e => e.Id == (dr["CurrencyId"] != DBNull.Value ? Convert.ToInt32(dr["CurrencyId"]) : 1)),
                                            DisplayCurrencyRate = dr["DisplayCurrencyRate"] != DBNull.Value ? Convert.ToDecimal(dr["DisplayCurrencyRate"]) : (decimal?)null,
                                            VisaFees = dr["VisaFees"] != DBNull.Value ? Convert.ToDecimal(dr["VisaFees"]) : (decimal?)null,
                                            Phone = dr["Phone"].ToString(),
                                            Email = dr["SupportEmail"].ToString()
                                        }).FirstOrDefault();

                    Assertions.IsNotNull(organization, Resources.Resource.DATABASE_ERROR_1037);

                    // Set cache options
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromDays(365)); // Cache for 30 days

                    // Save data in cache
                    _memoryCache.Set(cacheKey, organization, cacheEntryOptions);

                    return organization;

                }
                catch (Exception)
                {
                    throw; // Consider logging the exception for better diagnostics
                }
                finally
                {
                    objSQL?.Dispose();
                    objCmd?.Dispose();
                }
            }

            // Return the cached organization details if available
            return cachedOrganization;
        }


        public async Task<List<OrganizationPaymentProviders>> GetListOrganizationPaymentProviders(long organizationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetOrganizationPaymentProviders");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                DataTable dt = await objSQL.FetchDT(objCmd);

                var organizationPayment = (from DataRow dr in dt.Rows
                                           select new OrganizationPaymentProviders
                                           {
                                               Id = Convert.ToInt64(dr["Id"]),
                                               OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                               PaymentMethod = dr["PaymentMethod"].ToString(),
                                               MerchantId = Convert.ToString(dr["MerchantId"]),
                                               ApiPassword = Convert.ToString(dr["ApiPassword"]),
                                               BankDetails = Convert.ToString(dr["BankDetails"]),
                                               IsProduction = Convert.ToBoolean(dr["IsProduction"]),
                                               CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                               UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : null
                                           }).ToList();

                return organizationPayment;

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<long> UpdateOrganizationDetails(long id, OrganizationDetailsUpdateInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_InsertOrUpdateOrganization");
            try
            {
                var theme = input.Theme != null ? JsonSerializer.Serialize(input.Theme) : null;


                objCmd.Parameters.AddWithValue("@Id", id);
                objCmd.Parameters.AddWithValue("@OrganizationName", input.OrganizationName);
                objCmd.Parameters.AddWithValue("@Theme", theme);
                objCmd.Parameters.AddWithValue("@Phone", input.Phone);
                objCmd.Parameters.AddWithValue("@SupportEmail", input.Email.ToLower());
                if (!string.IsNullOrEmpty(input.BannerImage))
                    objCmd.Parameters.AddWithValue("@BannerImage", input.BannerImage);
                objCmd.Parameters.AddWithValue("@BannerHeading", input.BannerHeading);
                objCmd.Parameters.AddWithValue("@BannerSubHeading", input.BannerSubHeading);
                objCmd.Parameters.AddWithValue("@Logo", input.Logo);
                if(!string.IsNullOrEmpty(input.DomainName))
                    objCmd.Parameters.AddWithValue("@DomainName", input.DomainName);
                objCmd.Parameters.AddWithValue("@Status", Status.Active);
                objCmd.Parameters.AddWithValue("@Website", input.Website);
                objCmd.Parameters.AddWithValue("@CurrencyId", input.CurrencyId);
                objCmd.Parameters.AddWithValue("@DisplayCurrencyId", input.DisplayCurrencyId);
                objCmd.Parameters.AddWithValue("@DisplayCurrencyRate", input.DisplayCurrencyRate);
                objCmd.Parameters.AddWithValue("@VisaFees", input.VisaFees);

                DataTable dtUser = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtUser.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                long organizationId = Convert.ToInt64(dtUser.Rows[0]["OrganizationId"]);

                // Invalidate the cache for this organization
                string cacheKey = $"OrganizationDetails-{input.DomainName.ToLower()}";
                string cacheKeyForId = $"OrganizationDetails-{id}";

                 _memoryCache.Remove(cacheKey);
                _memoryCache.Remove(cacheKeyForId);



                return organizationId;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        } 

        public async Task<long> AddVisaFees(long? id,long organizationId, AddVisaFeesInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddOrUpdateVisaFee");
            try
            {

                if (id.HasValue)
                    objCmd.Parameters.AddWithValue("@Id", id);

                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@CountryId", input.CountryId);
                objCmd.Parameters.AddWithValue("@Fees", input.Fees);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dt.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return Convert.ToInt64(dt.Rows[0]["Id"]);

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<OrganizationVisaFeeDto> GetVisaFeesByCountry(long countryId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetVisaFeesByCountry");

            try
            {
                objCmd.Parameters.AddWithValue("@countryId", countryId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var visafees = (from DataRow dr in dt.Rows
                                select new OrganizationVisaFeeDto
                                {
                                    CountryId = Convert.ToInt64(dr["CountryId"]),
                                    Fees = dr["Fee"] != DBNull.Value ? Convert.ToDecimal(dr["Fee"]) : (decimal?)null
                                }).FirstOrDefault();

                return visafees;

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<long> AddUpdateOrganizationTypes(long? id, OrganizationTypes input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("SP_AddOrUpdateOrganizationTypes");
            try
            {
                if (id.HasValue)
                    objCmd.Parameters.AddWithValue("@Id", id);

                objCmd.Parameters.AddWithValue("@Name", input.Type);

                DataTable dtUser = await objSQL.FetchDT(objCmd);

                return Convert.ToInt32(dtUser.Rows[0]["NewId"]);

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<List<OrganizationTypes>> GetOrganizationTypes()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllOrganizationTypes");

            try
            {
                DataTable dt = await objSQL.FetchDT(objCmd);

                var types = (from DataRow dr in dt.Rows
                             select new OrganizationTypes
                             {
                                 Id = Convert.ToInt32(dr["Id"]),
                                 Type = dr["Name"] != DBNull.Value ? Convert.ToString(dr["Name"]) : (string?)null
                             }).ToList();

                return types;

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<long> DeleteOrganizationTypes(int id)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DeleteOrganizationTypes");

            try
            {
                objCmd.Parameters.AddWithValue("Id", id);

                DataTable dtTypes = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtTypes.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return id;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<List<Templates>> GetIdCardTemplates()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetTemplates");
            try
            {
                DataTable dt = await objSQL.FetchDT(objCmd);

                var countries = (from DataRow dr in dt.Rows
                                 select new Templates
                                 {
                                     Id = Convert.ToInt32(dr["Id"]),
                                     Template = Convert.ToString(dr["Template"])
                                 }).ToList();

                return countries;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<long> UpdateIdCardTemplate(long organizationId, IdCardUpdateInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateIdCardTemplate");
            try
            {

                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@Template", input.Template);
                await objSQL.UpdateDB(objCmd);
                return organizationId;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }


        //public async Task<string> GetIdCardTemplate(long organizationId)
        //{
        //    SQLManager objSQL = new SQLManager(_configuration);
        //    SqlCommand objCmd = new SqlCommand("sp_GetIdCardTemplate");

        //    try
        //    {
        //        objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
        //        DataSet ds = await objSQL.FetchDB(objCmd);

        //        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        //        {
        //            string AccreditationTemplate = Convert.ToString(ds.Tables[0].Rows[0]["IdCardTemplate"]);
        //            return AccreditationTemplate;
        //        }

        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        if (objSQL != null) objSQL.Dispose();
        //        if (objCmd != null) objCmd.Dispose();
        //    }
        //}

        public async Task<string> GetIdCardTemplate(long organizationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            using (SqlCommand objCmd = new SqlCommand("sp_GetIdCardTemplate"))
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);

                try
                {
                    DataSet ds = await objSQL.FetchDB(objCmd);
                    return ds?.Tables[0]?.Rows.Count > 0
                        ? Convert.ToString(ds.Tables[0].Rows[0]["IdCardTemplate"])
                        : null;
                }
                catch(Exception ex)
                {
                    throw;
                }
                finally
                {
                    objSQL?.Dispose();
                }
            }
        }

        public async Task<SubscriptionPlanDto> GetSubscriptionPlan(long organizationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetSubscriptionPlan");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                DataTable dt = await objSQL.FetchDT(objCmd);

                var plan = (from DataRow dr in dt.Rows
                                 select new SubscriptionPlanDto
                                 {
                                     Id = Convert.ToInt64(dr["Id"]),
                                     Name = Convert.ToString(dr["Name"]),
                                     PriceId = Convert.ToString(dr["PriceId"]),
                                     Duration = Convert.ToInt32(dr["Duration"]),
                                     Amount = Convert.ToDecimal(dr["Amount"]),
                                     CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
                                     IsAccommodationEnabled = Convert.ToBoolean(dr["IsAccommodationEnabled"]),
                                     IsTicketingSystemEnabled = Convert.ToBoolean(dr["IsTicketingSystemEnabled"]),
                                     IsVisaEnabled = Convert.ToBoolean(dr["IsVisaEnabled"]),
                                     NoOfEvents = Convert.ToInt32(dr["NoOfEvents"]),
                                     NoOfStaffs = Convert.ToInt32(dr["NoOfStaffs"]),
                                     Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                     StartDate = Convert.ToString(dr["StartDate"]),
                                     EndDate = Convert.ToString(dr["EndDate"]),
                                     IsAccreditationEnabled = Convert.ToBoolean(dr["IsAccreditationEnabled"]),
                                     SubscriptionId = Convert.ToString(dr["SubscriptionId"])
                                 }).FirstOrDefault();

                return plan;

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<MerchantDetails> MerchantDetails(long organizationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetMerchantIdandApiPassword");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var merchantDetails = (from DataRow dr in dt.Rows
                            select new MerchantDetails
                            {
                                MerchantId = Convert.ToString(dr["MerchantId"]),
                                ApiPassword = Convert.ToString(dr["ApiPassword"])
                            }).FirstOrDefault();

                return merchantDetails;

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }
    }
}
