using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess;
using EventManagement.DataAccess.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.BusinessLogic.Exceptions;
using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Helpers;
using EventManagement.DataAccess.Enums;
using EventManagement.DataAccess.Extensions;

namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class SubscriptionServices : ISubscriptionServices
    {
        private readonly IConfiguration _configuration;

        private readonly IStripeServices _stripeServices;

        public SubscriptionServices(IConfiguration configuration, IStripeServices stripeServices)
        {
            _configuration = configuration;
            _stripeServices = stripeServices;
        }

        public async Task<long> CreateSubscription(long organizationId, CreateSubscriptionInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddOrganizationSubscription");
            try
            {
                var session = _stripeServices.CreateSubscription(input.SessionId);

                var plan = GetSubscriptionById(Convert.ToInt64(session.SubscriptionPlanId)).Result;

                if (plan == null)
                    throw new ServiceException(Resource.INVALID_PLAN);

                var startDate = DateTimeOffset.UtcNow;
                var endDate = DateTimeOffset.UtcNow.AddMonths(Convert.ToInt32(plan.Duration));

                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@SubscriptionPlanId", plan.Id);
                objCmd.Parameters.AddWithValue("@StartDate", startDate);
                objCmd.Parameters.AddWithValue("@EndDate", endDate);
                objCmd.Parameters.AddWithValue("@CustomerId", session.CustomerId);
                objCmd.Parameters.AddWithValue("@SubscriptionId", session.SubscriptionId);

                DataTable dt = await objSQL.FetchDT(objCmd);

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

        public async Task<bool> CancelSubscription(long organizationId, CancelSubscriptionInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_CancelOrganizationSubscription");

            try
            {
                DateTime? cancelAt = _stripeServices.CancelSubscription(input.SubscriptionId);

                objCmd.Parameters.AddWithValue("@SubscriptionId", input.SubscriptionId);
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@CancelledDateTime", cancelAt);

                DataTable dtSubscription = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtSubscription.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return true;
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

        public async Task<SubscriptionPlanMst> GetSubscriptionById(long id)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetSubscriptionById");

            try
            {
                objCmd.Parameters.AddWithValue("@Id", id);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var subscriptionPlan = (from DataRow dr in dt.Rows
                             select new SubscriptionPlanMst
                             {
                                 Id = Convert.ToInt64(dr["Id"]),
                                 PriceId = Convert.ToString(dr["PriceId"]),
                                 Name = Convert.ToString(dr["Name"]),
                                 Duration = Convert.ToInt32(dr["Duration"]),
                                 Amount = Convert.ToDecimal(dr["Amount"]),
                                 CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
                                 IsAccommodationEnabled = Convert.ToBoolean(dr["IsAccommodationEnabled"]),
                                 IsTicketingSystemEnabled = Convert.ToBoolean(dr["IsTicketingSystemEnabled"]),
                                 IsVisaEnabled = Convert.ToBoolean(dr["IsVisaEnabled"]),
                                 NoOfEvents = Convert.ToInt32(dr["NoOfEvents"]),
                                 NoOfStaffs = Convert.ToInt32(dr["NoOfStaffs"]),
                                 Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                 IsAccreditationEnabled = Convert.ToBoolean(dr["IsAccreditationEnabled"]),
                             }).FirstOrDefault();

                Assertions.IsNotNull(subscriptionPlan, Resources.Resource.INVALID_SUBSCRIPTION);

                return subscriptionPlan;

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

        public async Task<List<SubscriptionPlanMst>> GetSubscriptions()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetSubscriptions");
            try
            {
                DataTable dt = await objSQL.FetchDT(objCmd);

                var subscriptions = (from DataRow dr in dt.Rows
                                    select new SubscriptionPlanMst
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
                                        IsAccreditationEnabled = Convert.ToBoolean(dr["IsAccreditationEnabled"])
                                    }).ToList();

                return subscriptions;
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

        public async Task<long> UpdateSubscriptionplan(long? id, UpdateSubscriptionPlanInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateSubscriptionPlan");

            try
            {
                //var status = StatusExtensions.ToStatusEnum(input.Status.ToLower()) ?? throw new ServiceException($"Invalid status: {input.Status}");

                objCmd.Parameters.AddWithValue("@Id", id);
                objCmd.Parameters.AddWithValue("@Name", input.Name);
                objCmd.Parameters.AddWithValue("@PriceId", input.PriceId);
                objCmd.Parameters.AddWithValue("@Duration", input.Duration);
                objCmd.Parameters.AddWithValue("@Amount", input.Amount);
                objCmd.Parameters.AddWithValue("@CurrencyId", input.CurrencyId);
                objCmd.Parameters.AddWithValue("@IsAccommodationEnabled", input.IsAccommodationEnabled);
                objCmd.Parameters.AddWithValue("@IsTicketingSystemEnabled", input.IsTicketingSystemEnabled);
                objCmd.Parameters.AddWithValue("@IsVisaEnabled", input.IsVisaEnabled);
                objCmd.Parameters.AddWithValue("@NoOfEvents", input.NoOfEvents);
                objCmd.Parameters.AddWithValue("@NoOfStaffs", input.NoOfStaffs);
                //objCmd.Parameters.AddWithValue("@Status", status);
                objCmd.Parameters.AddWithValue("@IsAccreditationEnabled", input.IsAccreditationEnabled);

                DataTable dtSubscription = await objSQL.FetchDT(objCmd);

                return Convert.ToInt64(dtSubscription.Rows[0]["Id"]);
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
    }
}
