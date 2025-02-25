using Aliyun.OSS;
using EventManagement.BusinessLogic.Exceptions;
using EventManagement.BusinessLogic.Helpers;
using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.BusinessLogic.Services.v1.Mappings;
using EventManagement.DataAccess;
using EventManagement.DataAccess.Enums;
using EventManagement.DataAccess.Extensions;
using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;
using EventManagement.Utilities.Email;
using EventManagement.Utilities.Helpers;
using EventManagement.Utilities.Payment.Mastercard;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Stripe;
using Stripe.Climate;
using System.Data;
using System.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using Microsoft.Extensions.Hosting;
using EventManagement.Utilities.FireBase;
using Microsoft.Extensions.Logging;
using EventManagement.Utilities.Storage.AlibabaCloud;

namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class CustomerServices : ICustomerServices
    {
        private readonly IConfiguration _configuration;

        private readonly IEventServices _eventServices;

        private readonly IMastercardPaymentService _mastercardPaymentService;

        private readonly IUserServices _userServices;

        private readonly IEmailService _emailService;

        private readonly IHostEnvironment _env;
        private readonly IAuthServices _authServices;
        private readonly IFirebaseServices _firebaseServices;
        private readonly IStorageServices _storageServices;

        public CustomerServices(IConfiguration configuration, IEventServices eventServices, IMastercardPaymentService mastercardPaymentService, IEmailService emailService, IHostEnvironment env, IUserServices userServices, IAuthServices authServices,IFirebaseServices firebaseServices, IStorageServices storageServices)

        {
            _configuration = configuration;
            _eventServices = eventServices;
            _mastercardPaymentService = mastercardPaymentService;
            _userServices = userServices;
            _emailService = emailService;
            _emailService = emailService;
            _env = env;
            _authServices = authServices;
            _firebaseServices = firebaseServices;
            _storageServices = storageServices;
        }
        
        public async Task<long> AddGuest(long organizationId, long? orderId, long? guestId, long UserId, GuestInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddOrUpdateGuest");

            try
            { 
                if(orderId.HasValue)
                    objCmd.Parameters.AddWithValue("@OrderId", orderId);
                if(guestId.HasValue)
                    objCmd.Parameters.AddWithValue("@GuestId", guestId);
                objCmd.Parameters.AddWithValue("@UserId", UserId);
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);  
                objCmd.Parameters.AddWithValue("@Role", input.Role);
                objCmd.Parameters.AddWithValue("@EventId", input.EventId);
                objCmd.Parameters.AddWithValue("@PassportFirstName", input.PassportFirstName);
                objCmd.Parameters.AddWithValue("@PassportLastName", input.PassportLastName);
                objCmd.Parameters.AddWithValue("@PassportNumber", input.PassportNumber);
                objCmd.Parameters.AddWithValue("@Status", Status.Draft);
                objCmd.Parameters.AddWithValue("@PaymentStatus", PaymentStatus.unpaid);
                objCmd.Parameters.AddWithValue("@Type", OrderType.registration);
                objCmd.Parameters.AddWithValue("@GuestStatus", ProfileStatus.profileIncomplete);
                objCmd.Parameters.AddWithValue("@VisaStatus", VisaStatus.notApplied);

                DataTable dtGuest = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtGuest.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return Convert.ToInt64(dtGuest.Rows[0]["OrderId"]);
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

        public async Task<long> DeleteGuest(long guestId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DeleteGuest");

            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@GuestId", guestId);
                objCmd.Parameters.AddWithValue("@Type", OrderType.registration);
                await objSQL.UpdateDB(objCmd);
                return guestId;
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

        public async Task<List<GuestDetailDto>> GetAllGuest(long? orderId, string searchText = null, string status = null)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllGuest");

            try
            {
                objCmd.Parameters.AddWithValue("@OrderId", orderId);
                objCmd.Parameters.AddWithValue("@SearchText", string.IsNullOrEmpty(searchText) ? (object)DBNull.Value : searchText);
                objCmd.Parameters.AddWithValue("@Status", !string.IsNullOrEmpty(status) ? StatusExtensions.ToStatusEnum(status.ToLower()) : null);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var guests = (from DataRow dr in dt.Rows
                              select GuestDetailMappings.MapToGuestDetailDto(dr, this)).ToList();


                return guests;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                objSQL?.Dispose();
                objCmd?.Dispose();
            }
        }


        public async Task<GuestDetailDto> GetGuestById(long guestId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetGuestById");

            try
            {
                GuestDetailDto guest = null;
                objCmd.Parameters.AddWithValue("@GuestId", guestId);
                DataTable dt = await objSQL.FetchDT(objCmd);

                if (dt.Rows.Count > 0)
                {
                    var dr = dt.Rows[0]; // Assuming only one row is returned
                    guest = GuestDetailMappings.MapToGuestDetailDto(dr, this);

                  
                    return guest;
                }

                Assertions.IsNotNull(guest, Resources.Resource.DATABASE_ERROR_1020);

                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                objSQL?.Dispose();
                objCmd?.Dispose();
            }
        }

        public async Task<long> Booking_AddAccomodationInfo(bool isUpdate, AccommodationInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddOrUpdateAccomodationInfo");
            try
            {
                var noOfGuests = input.GuestIds.Count;
                TimeSpan noOfNights = input.ToDate - input.FromDate;
                string guestIds = string.Join(",", input.GuestIds);
                if(input.OrderDetailId.HasValue)
                    objCmd.Parameters.AddWithValue("@OrderDetailId", input.OrderDetailId);
                objCmd.Parameters.AddWithValue("@OrderId", input.OrderId);
                objCmd.Parameters.AddWithValue("@HotelId", input.HotelId);
                objCmd.Parameters.AddWithValue("@HotelRoomTypeId", input.HotelRoomTypeId);
                objCmd.Parameters.AddWithValue("@FromDate", input.FromDate);
                objCmd.Parameters.AddWithValue("@ToDate", input.ToDate);
                objCmd.Parameters.AddWithValue("@Type", OrderType.accommodation);
                objCmd.Parameters.AddWithValue("@GuestIds", guestIds);
                objCmd.Parameters.AddWithValue("@NoOfGuests", noOfGuests);
                objCmd.Parameters.AddWithValue("@IsUpdate", isUpdate);
                objCmd.Parameters.AddWithValue("@SequenceNo", input.SequenceNo);
                objCmd.Parameters.AddWithValue("@NightDays", noOfNights.Days);

                DataTable dtHotel = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtHotel.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return Convert.ToInt64(dtHotel.Rows[0]["test"]);
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

        public async Task<long> Booking_AddVisaInfo(long organizationId,VisaDetailInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddOrUpdateVisaInfo");
            try
            {
                var noOfGuests = input.GuestVisaInfos.Where(e => e.VisaAssistanceRequired).ToList().Count;

                // Create a new list of guest visa information with the updated VisaStatus using LINQ
                var updatedGuestVisaInfos = input.GuestVisaInfos
                    .Select(guest =>
                    {
                        guest.VisaStatus = guest.VisaAssistanceRequired || guest.VisaOfficialLetterRequired
                            ? (int)VisaStatus.pending
                            : (int)VisaStatus.notApplied;
                        return guest;
                    }).ToList();

                objCmd.Parameters.AddWithValue("@OrderId", input.OrderId);
                objCmd.Parameters.AddWithValue("@Type", OrderType.visa);
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@NoOfGuests", noOfGuests);
                objCmd.Parameters.AddWithValue("@VisaDetails", MapDataTable.ToDataTable(input.GuestVisaInfos));

                DataTable dtVisa = await objSQL.FetchDT(objCmd);

                return Convert.ToInt64(dtVisa.Rows[0]["OrderId"]);
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

        public async Task<int> DeleteAccommodationInfo(DeletesAccomodationRequest input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DeleteAccomodationInfo");

            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@OrderId", input.OrderId);
                objCmd.Parameters.AddWithValue("@SequenceNo", input.SequenceNo);
                objCmd.Parameters.AddWithValue("@Type", OrderType.accommodation);
                await objSQL.UpdateDB(objCmd);
                return input.OrderId;
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


        public async Task<OrderDetailsDto> GetDraftOrderByEventId(long eventId, long userId)
           {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetInCompleteOrderDetailsV1");
            var order = new OrderDetailsDto();

            try
            {
                objCmd.Parameters.AddWithValue("@EventId", eventId);
                objCmd.Parameters.AddWithValue("@UserId", userId);
                objCmd.Parameters.AddWithValue("@Status", Status.Draft);
               
                DataSet ds = await objSQL.FetchDB(objCmd);
                var orderPenalties = new List<OrderPenaltyDto>();

                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        order = (from DataRow dr in ds.Tables[0].Rows
                                 select new OrderDetailsDto
                                 {
                                     Id = Convert.ToInt64(dr["Id"]),
                                     UserId = Convert.ToInt64(dr["UserId"]),
                                     OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                     OrderDate = dr["OrderDate"] != DBNull.Value ? Convert.ToDateTime(dr["OrderDate"]) : (DateTime?)null,
                                     EventId = Convert.ToInt64(dr["OrganizationEventId"]),
                                     DisplayCurrencyRate = dr["DisplayCurrencyRate"] != DBNull.Value ? Convert.ToDecimal(dr["DisplayCurrencyRate"]) : 0,
                                     TotalAmountInDisplayCurrency = dr["TotalAmount"] !=    DBNull.Value ? Convert.ToDecimal(dr["TotalAmount"]) + (dr["Penality"] != DBNull.Value ? Convert.ToDecimal(dr["Penality"]) : 0m) : 0m,
                                     TotalAmountInDefaultCurrency = dr["DisplayCurrencyRate"] !=  DBNull.Value && dr["TotalAmount"] != DBNull.Value ? CurrencyHelper.CalculateDefaultCurrencyAmount(Convert.ToDecimal(dr["TotalAmount"]) + (dr["Penality"] != DBNull.Value ? Convert.ToDecimal(dr["Penality"]) : 0m), Convert.ToDecimal(dr["DisplayCurrencyRate"])) : 0m,
                        PaidAmount = dr["AmountPaid"] != DBNull.Value ? Convert.ToDecimal(dr["AmountPaid"]) : 0,
                                     Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                     PaymentStatus = ((PaymentStatus)Convert.ToInt32(dr["PaymentStatus"])).ToString(),
                                     CreatedDate = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : (DateTime?)null,
                                     UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                     IsAccommodationEnabled = dr["IsAccommodationEnabled"] != DBNull.Value ? Convert.ToBoolean(dr["IsAccommodationEnabled"]) : false,
                                     IsTicketingSystemEnabled = dr["IsTicketingSystemEnabled"] != DBNull.Value ? Convert.ToBoolean(dr["IsTicketingSystemEnabled"]) : false,
                                     IsVisaEnabled = dr["IsVisaEnabled"] != DBNull.Value ? Convert.ToBoolean(dr["IsVisaEnabled"]) : false,
                                     Penalties = dr["Penality"] != DBNull.Value ? Convert.ToDecimal(dr["Penality"]) : 0m,
                                 }).FirstOrDefault();

                        order.UnpaidAmount = order.TotalAmountInDefaultCurrency - order.PaidAmount;

                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            var details = (from DataRow dr in ds.Tables[1].Rows
                                           select new OrderDetail
                                           {
                                               Id = Convert.ToInt64(dr["Id"]),
                                               OrderId = Convert.ToInt64(dr["OrderId"]),
                                               Type = dr["Type"] != DBNull.Value ? ((OrderType)Convert.ToInt32(dr["Type"])).ToString().ToLower(): string.Empty,
                                               Amount = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : 0m,
                                               CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                               UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null
                                           }).ToList();

                            order.OrderDetails = details;
                        }

                        if (ds.Tables[2].Rows.Count > 0)
                        {

                            var accomodation = (from DataRow dr in ds.Tables[2].Rows
                                                select new AccomodationInfoDto
                                                {
                                                    OrderDetailId = Convert.ToInt64(dr["OrderDetailId"]),
                                                    OrderId = Convert.ToInt64(dr["OrderId"]),
                                                    NumberOfNights = Convert.ToInt32(dr["NumberOfNights"]),
                                                    Amount = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : 0m,
                                                    FromDate = Convert.ToString(dr["FromDate"]),
                                                    ToDate = Convert.ToString(dr["ToDate"]),
                                                    GuestIds = dr["GuestIds"].ToString().Split(',').Select(int.Parse).ToList(),
                                                    GuestNames = dr["GuestNames"].ToString().Split(',').ToList(),
                                                    SequenceNo = dr["SequenceNo"] != DBNull.Value ?Convert.ToInt32(dr["SequenceNo"]) : (int?)null,
                                                    HotelId = Convert.ToInt64(dr["HotelId"]),
                                                    HotelRoomTypeId = Convert.ToInt64(dr["HotelRoomTypeId"]),
                                                    Hotel = new HotelDetailsDto()
                                                    {
                                                        Id = Convert.ToInt64(dr["HotelId"]),
                                                        OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                                        Name = Convert.ToString(dr["Name"]),
                                                        Rating = Convert.ToDouble(dr["Rating"]),
                                                        Address = Convert.ToString(dr["Address"]),
                                                        PostalCode = Convert.ToString(dr["PostalCode"]),
                                                        City = Convert.ToString(dr["City"]),
                                                        State = Convert.ToString(dr["State"]),
                                                        Country = Convert.ToString(dr["Country"]),
                                                        CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                                        LocationLatLong = Convert.ToString(dr["LocationLatLong"]),
                                                        CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                                        UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,


                                                            RoomType = new HotelRoomType()
                                                            {
                                                                Id = Convert.ToInt64(dr["HotelRoomTypeId"]),
                                                                HotelId = Convert.ToInt64(dr["HotelId"]),
                                                                RoomSize = Convert.ToString(dr["RoomSize"]),
                                                                PackagePrice = Convert.ToDecimal(dr["PackagePrice"]),
                                                                CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
                                                                NightPrice = Convert.ToDecimal(dr["NightPrice"]),
                                                                Availability = Convert.ToInt32(dr["Availability"]),
                                                                Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                                                MinimumOccupancy = dr["MinimumOccupancy"] != DBNull.Value ? Convert.ToInt32(dr["MinimumOccupancy"]) : (int?)null,
                                                                MaximumOccupancy = dr["MaximumOccupancy"] != DBNull.Value ? Convert.ToInt32(dr["MaximumOccupancy"]) : (int?)null
                                                            }
                                                    }
                                                    }).ToList();

                            order.AccommodationInfo = accomodation;
                        }

                        if (ds.Tables[3].Rows.Count > 0)
                        {
                            var guestDetails = (from DataRow dr in ds.Tables[3].Rows
                                           select new GuestDetailDto
                                           {
                                               Id = Convert.ToInt64(dr["Id"]),
                                               OrderId = Convert.ToInt64(dr["OrderId"]),
                                               EventId = dr["EventId"] != DBNull.Value ? Convert.ToInt64(dr["EventId"]) : 0,
                                               Role = Convert.ToString(dr["Role"]),
                                               PassportFirstName = dr["PassportFirstName"] != DBNull.Value ? dr["PassportFirstName"].ToString() : null,
                                               PassportLastName = dr["PassportLastName"] != DBNull.Value ? dr["PassportLastName"].ToString() : null,
                                               PassportNumber = dr["PassportNumber"] != DBNull.Value ? dr["PassportNumber"].ToString() : null,
                                               PassportIssueDate = dr["PassportIssueDate"].ToString(),
                                               PassportExpiryDate = dr["PassportExpiryDate"].ToString(),
                                               DOB = Convert.ToString(dr["DOB"]),
                                               Occupation = !string.IsNullOrEmpty(dr["Occupation"].ToString()) ? dr["Occupation"].ToString() : string.Empty,
                                               Nationality = !string.IsNullOrEmpty(dr["Nationality"].ToString()) ? dr["Nationality"].ToString() : string.Empty,
                                               JobTitle = !string.IsNullOrEmpty(dr["JobTitle"].ToString()) ? dr["JobTitle"].ToString() : string.Empty,
                                               WorkPlace = !string.IsNullOrEmpty(dr["WorkPlace"].ToString()) ? dr["WorkPlace"].ToString() : string.Empty,
                                               DepartureFlightAirport = !string.IsNullOrEmpty(dr["DepartureFlightAirport"].ToString()) ? dr["DepartureFlightAirport"].ToString() : string.Empty,
                                               ArrivalFlightAirport = !string.IsNullOrEmpty(dr["ArrivalFlightAirport"].ToString()) ? dr["ArrivalFlightAirport"].ToString() : string.Empty,
                                               ArrivalFlightNumber = !string.IsNullOrEmpty(dr["ArrivalFlightNumber"].ToString()) ? dr["ArrivalFlightNumber"].ToString() : string.Empty,
                                               DepartureFlightNumber = !string.IsNullOrEmpty(dr["DepartureFlightNumber"].ToString()) ? dr["DepartureFlightNumber"].ToString() : string.Empty,
                                               ArrivalDateTime = dr["ArrivalDateTime"].ToString(),
                                               DepartureDateTime = dr["DepartureDateTime"].ToString(),
                                               ArrivalNotes = !string.IsNullOrEmpty(dr["ArrivalNotes"].ToString()) ? dr["ArrivalNotes"].ToString() : string.Empty,
                                               DepartureNotes = !string.IsNullOrEmpty(dr["DepartureNotes"].ToString()) ? dr["DepartureNotes"].ToString() : string.Empty,
                                               AccessibilityInfo = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfo"])) ? ConversionHelper.ConvertStringToArray(Convert.ToString(dr["AccessibilityInfo"])) : new List<int>() ,
                                               HotelId = dr["HotelId"] != DBNull.Value ? Convert.ToInt32(dr["HotelId"]) : (int?)null,
                                               HotelRoomTypeId = dr["HotelRoomTypeId"] != DBNull.Value ? Convert.ToInt32(dr["HotelRoomTypeId"]) : (int?)null,
                                               FromDate = Convert.ToString(dr["FromDate"]),
                                               ToDate = Convert.ToString(dr["ToDate"]),
                                               VisaAssistanceRequired = dr["VisaAssistanceRequired"] != DBNull.Value ? Convert.ToBoolean(dr["VisaAssistanceRequired"]) : false,
                                               VisaOfficialLetterRequired = dr["VisaOfficialLetterRequired"] != DBNull.Value ? Convert.ToBoolean(dr["VisaOfficialLetterRequired"]) : false,
                                               VisaDocument = Convert.ToString(dr["VisaDocument"]),
                                               CreatedDate = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : (DateTime?)null,
                                               UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                               SequenceNo = dr["SequenceNo"] != DBNull.Value ? Convert.ToInt32(dr["SequenceNo"]) : 0,
                                               RegistrationFee = dr["RegistrationFee"] != DBNull.Value ? Convert.ToDecimal(dr["RegistrationFee"]) : 0,
                                               Status = dr["Status"] != DBNull.Value ? StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])) : null,
                                               VisaStatus = dr["VisaStatus"] != DBNull.Value ? ((VisaStatus)Convert.ToInt32(dr["VisaStatus"])).ToString() : null,
                                               Photo = Convert.ToString(dr["Photo"])
                                           }).ToList();
                            order.GuestDetails = guestDetails;
                        }

                        if (ds.Tables[6].Rows.Count > 0)
                        {
                            orderPenalties = (from DataRow dr in ds.Tables[6].Rows
                                              select new OrderPenaltyDto
                                              {
                                                  Id = dr["Id"] != DBNull.Value ? Convert.ToInt32(dr["Id"]) : 0,
                                                  PenaltyType = dr["PenaltyType"] != DBNull.Value ? Convert.ToInt32(dr["PenaltyType"]) : 0,
                                                  PenaltyName = dr["PenaltyType"] != DBNull.Value
                                                    ? Enum.GetName(typeof(PenaltyType), Convert.ToInt32(dr["PenaltyType"]))
                                                    : string.Empty,
                                                  Amount = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : (decimal?)null

                                              }).ToList();
                        }

                        order.OrderPenalties = orderPenalties;

                        if (orderPenalties == null || orderPenalties.Count == 0)
                        {
                            if (order.Penalties > 0)
                            {
                                orderPenalties.Add(new OrderPenaltyDto
                                {
                                    Id = 0, // Assuming no actual ID exists for this default penalty
                                    PenaltyType = (int)PenaltyType.lateRegistration,
                                    PenaltyName = PenaltyType.lateRegistration.ToString(),
                                    Amount = order.Penalties
                                });
                            }
                        }

                        if (order.GuestDetails.Count > 0)
                            order.TotalRegistrationFee = order.GuestDetails.Sum(g => g.RegistrationFee);
     
                        return order;
                    }
                }
                return null;
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

        public async Task<decimal> GetRegistrationFee(long eventId, string role, long? guestId = null)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetRegistrationFee");
            try
            {  
                objCmd.Parameters.AddWithValue("@EventId", eventId);
                objCmd.Parameters.AddWithValue("@Role", role);
                if(guestId.HasValue)
                    objCmd.Parameters.AddWithValue("@guestId", guestId);

                DataTable dtVisa = await objSQL.FetchDT(objCmd);

                return Convert.ToDecimal(dtVisa.Rows[0]["RegistrationFee"]);
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

        public async Task<List<OrderDetail>> GetOrderDetailsByOrderId(long orderId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetOrderDetailByOrderId");

            try
            {
                objCmd.Parameters.AddWithValue("@OrderId", orderId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var orders = (from DataRow dr in dt.Rows
                              select new OrderDetail
                              {
                                  Id = Convert.ToInt64(dr["Id"]),
                                  OrderId = Convert.ToInt64(dr["OrderId"]),
                                  Type = dr["Type"] != DBNull.Value ? ((OrderType)Convert.ToInt32(dr["Type"])).ToString().ToLower(): null,
                                  Amount = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : 0m,
                                  CreatedDate = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : (DateTime?)null,
                                  UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null
                              }).ToList();

                return orders;
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

        public async Task<BookingListsDto> GetAllBookings(long? organizationId, long? userId, long? eventId, DateTimeOffset? orderDate, string role, string sortColumn, string sortOrder, string searchText, int? pageNo, int? pageSize)
        {
            
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllBooking");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId ?? (object)DBNull.Value);

                objCmd.Parameters.AddWithValue("@UserId", role.ToString().ToLower() == "customer" ? userId : (object)DBNull.Value);
                objCmd.Parameters.AddWithValue("@EventId", eventId);
                objCmd.Parameters.AddWithValue("@OrderDate", orderDate);
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SearchText", searchText);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);

                DataTable dt = await objSQL.FetchDT(objCmd);
                List<BookingsDto> bookings = new List<BookingsDto>();
                var totalCount = 0;
                var totalRecords = 0;

                if (dt.Rows.Count > 0)
                {
                    bookings = (from DataRow dr in dt.Rows
                                   select new BookingsDto
                                   {
                                       Id = Convert.ToInt64(dr["Id"]),
                                       UserId = Convert.ToInt64(dr["UserId"]),
                                       OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                       OrderDate = Convert.ToString(dr["OrderDate"]),
                                       EventId = Convert.ToInt64(dr["OrganizationEventId"]),
                                       TotalAmountInDisplayCurrency = Convert.ToDecimal(dr["TotalAmount"]),
                                       TotalAmountInDefaultCurrency =  CurrencyHelper.CalculateDefaultCurrencyAmount(Convert.ToDecimal(dr["TotalAmount"]), Convert.ToDecimal(dr["DisplayCurrencyRate"])),
                                       PaidAmount = Convert.ToDecimal(dr["AmountPaid"]),
                                       UnpaidAmount = Convert.ToDecimal(dr["TotalAmount"]) - Convert.ToDecimal(dr["AmountPaid"]),
                                       Status = dr["Status"] != DBNull.Value ? StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])) : string.Empty,
                                       PaymentStatus = dr["PaymentStatus"] != DBNull.Value ? ((PaymentStatus)Convert.ToInt32(dr["PaymentStatus"])).ToString() : string.Empty,
                                       CreatedDate = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : (DateTime?)null,
                                       UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                       User = new UserDetailsDto()
                                       {
                                           Id = Convert.ToInt64(dr["UserId"]),
                                           FirstName = Convert.ToString(dr["FirstName"]),
                                           LastName = Convert.ToString(dr["LastName"]),
                                           Phone = Convert.ToString(dr["Phone"]),
                                           Email = Convert.ToString(dr["Email"]),
                                           Country = Convert.ToString(dr["Country"]),
                                           CountryId = dr["CountryId"] != DBNull.Value ? (int?)Convert.ToInt32(dr["CountryId"]) : null,
                                           OrganizationId = dr["OrganizationId"] != DBNull.Value ? (long?)Convert.ToInt64(dr["OrganizationId"]) : null,
                                           CustomerOrganizationType = Convert.ToString(dr["OrganizationTypeId"]),
                                           CustomerOrganizationName = Convert.ToString(dr["OrganizationName"]),
                                           Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                           RoleId = Convert.ToInt32(dr["RoleId"]),
                                           Role = Convert.ToString(dr["Role"]),
                                       },
                                       Event = new EventInfoDto
                                       {
                                           Id = Convert.ToInt64(dr["EventId"]),
                                           OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                           Name = Convert.ToString(dr["EventName"]),
                                           BannerImage = Convert.ToString(dr["BannerImage"]),
                                           Description = Convert.ToString(dr["Description"]),
                                           Latitude = Convert.ToString(dr["Latitude"]),
                                           Longitude = Convert.ToString(dr["Longitude"]),
                                           Address = Convert.ToString(dr["Address"]),
                                           City = Convert.ToString(dr["City"]),
                                           State = Convert.ToString(dr["State"]),
                                           Country = Convert.ToString(dr["EventCountry"]),
                                           CountryId = Convert.ToInt32(dr["EventCountryId"]),
                                           TimeZoneId = Convert.ToInt32(dr["TimeZoneId"]),
                                           StartDate = Convert.ToString(dr["StartDate"]),
                                           EndDate = Convert.ToString(dr["EndDate"]),
                                           AccommodationInfoFile = dr["AccommodationInfoFile"].ToString(),
                                           TransportationInfoFile = dr["TransportationInfoFile"].ToString(),
                                           Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["EventStatus"])),
                                           CreatedDate = Convert.ToDateTime(dr["EventCreatedDate"]),
                                           UpdatedDate = dr["EventUpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["EventUpdatedDate"]) : (DateTime?)null,
                                           AccommodationPackageInfo = !string.IsNullOrEmpty(Convert.ToString(dr["AccommodationPackageInfo"])) ? ConversionHelper.ConvertStringToList(Convert.ToString(dr["AccommodationPackageInfo"])) : new List<string>(),
                                           AccessibilityInfo = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfo"])) ? ConversionHelper.ConvertStringToArray(Convert.ToString(dr["AccessibilityInfo"])) : new List<int>(),
                                           AccessibilityInfoData = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfoData"])) ? JsonConvert.DeserializeObject<List<AccessiblityInfoDto>>(Convert.ToString(dr["AccessibilityInfoData"])) : new List<AccessiblityInfoDto>(),
                                           PaymentMethodSupported = JsonConvert.DeserializeObject<List<PaymentMethodSupported>>(dr["PaymentMethodSupported"].ToString()),
                                           RoleWiseData = JsonConvert.DeserializeObject<List<RoleWiseData>>(dr["RoleWiseData"].ToString()),
                                           PaymentproviderId = dr["PaymentproviderId"] != DBNull.Value ? Convert.ToInt32(dr["PaymentproviderId"]) : (int?)null
                                       }
                                   }).ToList();

                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                    totalRecords = Convert.ToInt32(dt.Rows[0]["TotalRecords"]);
                }
                return new BookingListsDto { List = bookings, TotalCount = totalCount, TotalRecords = totalRecords };
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

        public async Task<List<AccomodationInfoDto>> Booking_GetAllAccomodationInfo(long orderId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAccomodationInfo");
            try
            {
                objCmd.Parameters.AddWithValue("@OrderId", orderId);
                objCmd.Parameters.AddWithValue("@Type", OrderType.accommodation);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var accomodation = (from DataRow dr in dt.Rows
                                    select new AccomodationInfoDto
                                    {
                                        OrderDetailId = Convert.ToInt64(dr["OrderDetailId"]),
                                        OrderId = Convert.ToInt64(dr["OrderId"]),
                                        NumberOfNights = Convert.ToInt32(dr["NumberOfNights"]),
                                        Amount = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : 0m,
                                        FromDate = Convert.ToString(dr["FromDate"]),
                                        ToDate = Convert.ToString(dr["ToDate"]),
                                        GuestIds = dr["GuestIds"].ToString().Split(',').Select(int.Parse).ToList(),
                                        GuestNames = dr["GuestNames"].ToString().Split(',').ToList(),
                                        SequenceNo = Convert.ToInt32(dr["SequenceNo"]),
                                        HotelId = Convert.ToInt64(dr["HotelId"]),
                                        HotelRoomTypeId = Convert.ToInt64(dr["HotelRoomTypeId"]),
                                        Hotel = new HotelDetailsDto()
                                        {
                                            Id = Convert.ToInt64(dr["HotelId"]),
                                            OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                            Name = Convert.ToString(dr["HotelName"]),
                                            Rating = Convert.ToDouble(dr["Rating"]),
                                            Address = Convert.ToString(dr["Address"]),
                                            PostalCode = Convert.ToString(dr["PostalCode"]),
                                            City = Convert.ToString(dr["City"]),
                                            State = Convert.ToString(dr["State"]),
                                            Country = Convert.ToString(dr["Country"]),
                                            CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                            LocationLatLong = Convert.ToString(dr["LocationLatLong"]),    
                                            CreatedDate = Convert.ToDateTime(dr["HotelCreatedDate"]),
                                            UpdatedDate = dr["HotelUpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["HotelUpdatedDate"]) : (DateTime?)null,
                                            RoomType = new HotelRoomType()
                                            {
                                                Id = Convert.ToInt64(dr["RoomTypeId"]),
                                                HotelId = Convert.ToInt64(dr["HotelId"]),
                                                RoomSize = Convert.ToString(dr["RoomSize"]),
                                                PackagePrice = Convert.ToDecimal(dr["PackagePrice"]),
                                                CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
                                                NightPrice = Convert.ToDecimal(dr["NightPrice"]),
                                                Availability = Convert.ToInt32(dr["Availability"]),
                                                Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["RoomStatus"])),
                                                MinimumOccupancy = dr["MinimumOccupancy"] != DBNull.Value ? Convert.ToInt32(dr["MinimumOccupancy"]) : (int?)null,
                                                MaximumOccupancy = dr["MaximumOccupancy"] != DBNull.Value ? Convert.ToInt32(dr["MaximumOccupancy"]) : (int?)null
                                            }
                                        }
                                    }).ToList();
                return accomodation;

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


        public async Task<AccomodationInfoDto> Booking_GetAccomodationInfo(long orderId, string guestIds = "")
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAccomodationInfo");
            try
            {
                objCmd.Parameters.AddWithValue("@OrderId", orderId);
                objCmd.Parameters.AddWithValue("@GuestIds", guestIds);
                objCmd.Parameters.AddWithValue("@Status", OrderType.accommodation);

                DataSet ds = await objSQL.FetchDB(objCmd);

                var accomodation = (from DataRow dr in ds.Tables[0].Rows
                                    select new AccomodationInfoDto
                                    {
                                        OrderDetailId = Convert.ToInt64(dr["OrderDetailId"]),
                                        OrderId = Convert.ToInt64(dr["OrderId"]),
                                        NumberOfNights = Convert.ToInt32(dr["NumberOfNights"]),
                                        Amount = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : 0m,
                                        FromDate = Convert.ToString(dr["FromDate"]),
                                        ToDate = Convert.ToString(dr["ToDate"]),
                                        GuestIds = dr["GuestIds"].ToString().Split(',').Select(int.Parse).ToList(),
                                        GuestNames = dr["GuestNames"].ToString().Split(',').ToList(),
                                        SequenceNo = Convert.ToInt32(dr["SequenceNo"]),
                                        HotelId = Convert.ToInt64(dr["HotelId"]),
                                        HotelRoomTypeId = Convert.ToInt64(dr["HotelRoomTypeId"])
                                    }).FirstOrDefault();

                return accomodation;

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

        public async Task<BookingDetailDto> GetBookingById(long bookingId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetBookingById");

            try
            {
                objCmd.Parameters.AddWithValue("@BookingId", bookingId);
                objCmd.Parameters.AddWithValue("@Type", OrderType.accommodation);

                DataSet ds = await objSQL.FetchDB(objCmd);

                var booking = new BookingDetailDto();
                var orderDetail = new List<OrderDetail>();
                var guestDetail = new List<GuestDetailDto>();
                var accomodation = new List<AccomodationInfoDto>();
                var orderPenalties = new List<OrderPenaltyDto>();
                var eventDetails = new EventBasicDetails();

                if (ds.Tables[1].Rows.Count > 0)
                {
                    orderDetail = (from DataRow dr in ds.Tables[1].Rows
                                   select new OrderDetail
                                   {
                                       Id = Convert.ToInt64(dr["Id"]),
                                       OrderId = Convert.ToInt64(dr["OrderId"]),
                                       Type = dr["Type"] != DBNull.Value ? ((OrderType)Convert.ToInt32(dr["Type"])).ToString().ToLower() : null,
                                       Amount = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : 0m,
                                       CreatedDate = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : (DateTime?)null,
                                       UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null
                                   }).ToList();
                }

                if (ds.Tables[2].Rows.Count > 0)
                {
                    guestDetail = (from DataRow dr in ds.Tables[2].Rows
                                   select new GuestDetailDto
                                   {
                                       Id = Convert.ToInt64(dr["Id"]),
                                       OrderId = Convert.ToInt64(dr["OrderId"]),
                                       EventId = dr["EventId"] != DBNull.Value ? Convert.ToInt64(dr["EventId"]) : 0,
                                       Role = Convert.ToString(dr["Role"]),
                                       PassportFirstName = dr["PassportFirstName"] != DBNull.Value ? dr["PassportFirstName"].ToString() : null,
                                       PassportLastName = dr["PassportLastName"] != DBNull.Value ? dr["PassportLastName"].ToString() : null,
                                       PassportNumber = dr["PassportNumber"] != DBNull.Value ? dr["PassportNumber"].ToString() : null,
                                       PassportIssueDate = dr["PassportIssueDate"].ToString(),
                                       PassportExpiryDate = dr["PassportExpiryDate"].ToString(),
                                       DOB = Convert.ToString(dr["DOB"]),
                                       Occupation = !string.IsNullOrEmpty(dr["Occupation"].ToString()) ? dr["Occupation"].ToString() : string.Empty,
                                       Nationality = !string.IsNullOrEmpty(dr["Nationality"].ToString()) ? dr["Nationality"].ToString() : string.Empty,
                                       JobTitle = !string.IsNullOrEmpty(dr["JobTitle"].ToString()) ? dr["JobTitle"].ToString() : string.Empty,
                                       WorkPlace = !string.IsNullOrEmpty(dr["WorkPlace"].ToString()) ? dr["WorkPlace"].ToString() : string.Empty,
                                       DepartureFlightAirport = !string.IsNullOrEmpty(dr["DepartureFlightAirport"].ToString()) ? dr["DepartureFlightAirport"].ToString() : string.Empty,
                                       ArrivalFlightAirport = !string.IsNullOrEmpty(dr["ArrivalFlightAirport"].ToString()) ? dr["ArrivalFlightAirport"].ToString() : string.Empty,
                                       ArrivalFlightNumber = !string.IsNullOrEmpty(dr["ArrivalFlightNumber"].ToString()) ? dr["ArrivalFlightNumber"].ToString() : string.Empty,
                                       DepartureFlightNumber = !string.IsNullOrEmpty(dr["DepartureFlightNumber"].ToString()) ? dr["DepartureFlightNumber"].ToString() : string.Empty,
                                       ArrivalDateTime = dr["ArrivalDateTime"].ToString(),
                                       DepartureDateTime = dr["DepartureDateTime"].ToString(),
                                       ArrivalNotes = !string.IsNullOrEmpty(dr["ArrivalNotes"].ToString()) ? dr["ArrivalNotes"].ToString() : string.Empty,
                                       DepartureNotes = !string.IsNullOrEmpty(dr["DepartureNotes"].ToString()) ? dr["DepartureNotes"].ToString() : string.Empty,
                                       AccessibilityInfo = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfo"])) ? ConversionHelper.ConvertStringToArray(Convert.ToString(dr["AccessibilityInfo"])) : new List<int>(),
                                       AccessibilityInfoData = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfoData"])) ? JsonConvert.DeserializeObject<List<AccessiblityInfoDto>>(Convert.ToString(dr["AccessibilityInfoData"])) : new List<AccessiblityInfoDto>(),
                                       HotelId = dr["HotelId"] != DBNull.Value ? Convert.ToInt32(dr["HotelId"]) : (int?)null,
                                       HotelRoomTypeId = dr["HotelRoomTypeId"] != DBNull.Value ? Convert.ToInt32(dr["HotelRoomTypeId"]) : (int?)null,
                                       FromDate = Convert.ToString(dr["FromDate"]),
                                       ToDate = Convert.ToString(dr["ToDate"]),
                                       VisaAssistanceRequired = dr["VisaAssistanceRequired"] != DBNull.Value ? Convert.ToBoolean(dr["VisaAssistanceRequired"]) : false,
                                       VisaOfficialLetterRequired = dr["VisaOfficialLetterRequired"] != DBNull.Value ? Convert.ToBoolean(dr["VisaOfficialLetterRequired"]) : false,
                                       VisaDocument = Convert.ToString(dr["VisaDocument"]),
                                       VisaOfficialLetterDocument = Convert.ToString(dr["VisaOfficialLetterDocument"]),
                                       CreatedDate = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : (DateTime?)null,
                                       UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                       SequenceNo = dr["SequenceNo"] != DBNull.Value ? Convert.ToInt32(dr["SequenceNo"]) : 0,
                                       RegistrationFee = dr["RegistrationFee"] != DBNull.Value ? Convert.ToDecimal(dr["RegistrationFee"]) : 0,
                                       Status = dr["Status"] != DBNull.Value ? ((ProfileStatus)Convert.ToInt32(dr["Status"])).ToString() : string.Empty,
                                       VisaStatus = dr["VisaStatus"] != DBNull.Value ? ((VisaStatus)Convert.ToInt32(dr["VisaStatus"])).ToString() : null,
                                       Photo = Convert.ToString(dr["Photo"]),
                                       PassportImage = Convert.ToString(dr["PassportImage"])
                                   }).ToList();
                }

                if (ds.Tables[3].Rows.Count > 0)
                {
                    accomodation = (from DataRow dr in ds.Tables[3].Rows
                                    select new AccomodationInfoDto
                                    {
                                        OrderDetailId = Convert.ToInt64(dr["OrderDetailId"]),
                                        OrderId = Convert.ToInt64(dr["OrderId"]),
                                        NumberOfNights = Convert.ToInt32(dr["NumberOfNights"]),
                                        Amount = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : 0m,
                                        FromDate = Convert.ToString(dr["FromDate"]),
                                        ToDate = Convert.ToString(dr["ToDate"]),
                                        GuestIds = dr["GuestIds"].ToString().Split(',').Select(int.Parse).ToList(),
                                        GuestNames = dr["GuestNames"].ToString().Split(',').ToList(),
                                        SequenceNo = Convert.ToInt32(dr["SequenceNo"]),
                                        HotelId = Convert.ToInt64(dr["HotelId"]),
                                        HotelRoomTypeId = Convert.ToInt64(dr["HotelRoomTypeId"]),
                                        Hotel = new HotelDetailsDto()
                                        {
                                            Id = Convert.ToInt64(dr["HotelId"]),
                                            OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                            Name = Convert.ToString(dr["HotelName"]),
                                            Rating = Convert.ToDouble(dr["Rating"]),
                                            Address = Convert.ToString(dr["Address"]),
                                            PostalCode = Convert.ToString(dr["PostalCode"]),
                                            City = Convert.ToString(dr["City"]),
                                            State = Convert.ToString(dr["State"]),
                                            Country = Convert.ToString(dr["Country"]),
                                            CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                            LocationLatLong = Convert.ToString(dr["LocationLatLong"]),
                                            CreatedDate = Convert.ToDateTime(dr["HotelCreatedDate"]),
                                            UpdatedDate = dr["HotelUpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["HotelUpdatedDate"]) : (DateTime?)null,
                                            RoomType = new HotelRoomType()
                                            {
                                                Id = Convert.ToInt64(dr["RoomTypeId"]),
                                                HotelId = Convert.ToInt64(dr["HotelId"]),
                                                RoomSize = Convert.ToString(dr["RoomSize"]),
                                                PackagePrice = Convert.ToDecimal(dr["PackagePrice"]),
                                                CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
                                                NightPrice = Convert.ToDecimal(dr["NightPrice"]),
                                                Availability = Convert.ToInt32(dr["Availability"]),
                                                Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["RoomStatus"])),
                                                MinimumOccupancy = dr["MinimumOccupancy"] != DBNull.Value ? Convert.ToInt32(dr["MinimumOccupancy"]) : (int?)null,
                                                MaximumOccupancy = dr["MaximumOccupancy"] != DBNull.Value ? Convert.ToInt32(dr["MaximumOccupancy"]) : (int?)null
                                            }
                                        }
                                    }).ToList();
                }


                if (ds.Tables[0].Rows.Count > 0)
                {
                    booking = (from DataRow dr in ds.Tables[0].Rows
                               select new BookingDetailDto
                               {
                                   Id = Convert.ToInt64(dr["Id"]),
                                   UserId = Convert.ToInt64(dr["UserId"]),
                                   OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                   OrderDate = Convert.ToString(dr["OrderDate"]),
                                   EventId = Convert.ToInt64(dr["OrganizationEventId"]),
                                   OrganizationName = Convert.ToString(dr["OrganizationName"]),
                                   TotalAmountInDisplayCurrency = dr["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(dr["TotalAmount"]) : 0m,
                                   TotalAmountInDefaultCurrency = dr["CurrencyRate"] != DBNull.Value && dr["TotalAmount"] != DBNull.Value
                                ? CurrencyHelper.CalculateDefaultCurrencyAmount(Convert.ToDecimal(dr["TotalAmount"]), Convert.ToDecimal(dr["CurrencyRate"]))
                                : 0m,
                                   PaidAmount = dr["AmountPaid"] != DBNull.Value ? Convert.ToDecimal(dr["AmountPaid"]) : 0m,      
                                   Status = dr["Status"] != DBNull.Value ? StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])) : string.Empty,
                                   PaymentStatus = dr["PaymentStatus"] != DBNull.Value ? ((PaymentStatus)Convert.ToInt32(dr["PaymentStatus"])).ToString() : string.Empty,
                                   CreatedDate = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : (DateTime?)null,
                                   UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                   CurrencyCode = Convert.ToString(dr["CurrencyCode"]),
                                   OrderDetails = orderDetail.Where(e => e.OrderId == Convert.ToInt64(dr["Id"])).ToList(),
                                   GuestDetails = guestDetail.Where(e => e.OrderId == Convert.ToInt64(dr["Id"])).ToList(),
                                   User = new UserDetailsDto
                                   {
                                       Id = Convert.ToInt64(dr["UserId"]),
                                       FirstName = dr["FirstName"].ToString(),
                                       LastName = dr["LastName"].ToString(),
                                       Phone = dr["Phone"].ToString(),
                                       Email = dr["Email"].ToString(),
                                       Country = dr["Country"].ToString(),
                                       CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                       OrganizationId = dr["OrganizationId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["OrganizationId"]),
                                       CustomerOrganizationType = Convert.ToString(dr["OrganizationType"]),
                                       CustomerOrganizationName = Convert.ToString(dr["CustomerOrganizationName"]),
                                       Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                       RoleId = Convert.ToInt32(dr["RoleId"]),
                                       Role = dr["Role"].ToString()
                                   },
                                   AccommodationInfo = accomodation.Where(e => e.OrderId == Convert.ToInt64(dr["Id"])).ToList(),
                                   IsAccommodationEnabled = dr["IsAccommodationEnabled"] != DBNull.Value ? Convert.ToBoolean(dr["IsAccommodationEnabled"]) : false,
                                   IsVisaEnabled = dr["IsVisaEnabled"] != DBNull.Value ? Convert.ToBoolean(dr["IsVisaEnabled"]) : false,
                                   Penalties = Convert.ToInt32(dr["PenaltyAmount"]),
                                   PaymentType = dr["PaymentTypeId"] != DBNull.Value ? ((PaymentType)Convert.ToInt32(dr["PaymentTypeId"])).ToString() : null,
                                   BankReceiptImage = Convert.ToString(dr["BankReceiptImage"])
                               }).FirstOrDefault();

                    booking.UnpaidAmount = booking.TotalAmountInDisplayCurrency - booking.PaidAmount;

                    Assertions.IsNotNull(booking, Resources.Resource.DATABASE_ERROR_1028);
                }

                else
                {
                    throw new ServiceException(Resources.Resource.BOOKING_NOT_FOUND);
                }

                if (ds.Tables[4].Rows.Count > 0)
                {
                    orderPenalties = (from DataRow dr in ds.Tables[4].Rows
                                      select new OrderPenaltyDto
                                      {
                                          Id = dr["Id"] != DBNull.Value ? Convert.ToInt32(dr["Id"]) : 0,
                                          PenaltyType = dr["PenaltyType"] != DBNull.Value ? Convert.ToInt32(dr["PenaltyType"]) : 0,
                                          PenaltyName = dr["PenaltyType"] != DBNull.Value
                                            ? Enum.GetName(typeof(PenaltyType), Convert.ToInt32(dr["PenaltyType"]))
                                            : string.Empty,
                                          Amount = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : (decimal?)null

                                      }).ToList();
                }

                if (ds.Tables[5].Rows.Count > 0)
                {
                    eventDetails = (from DataRow dr in ds.Tables[5].Rows
                                    select new EventBasicDetails
                                    {
                                        Id = Convert.ToInt64(dr["Id"]),
                                        Name = Convert.ToString(dr["Name"]),
                                        //PaymentMethodSupported = JsonConvert.DeserializeObject<List<PaymentMethodSupported>>(dr["PaymentMethodSupported"].ToString()),
                                        BannerImage = Convert.ToString(dr["BannerImage"]),
                                        Address = Convert.ToString(dr["Address"]),
                                        //Description = Convert.ToString(dr["Description"]),
                                        StartDate = Convert.ToString(dr["StartDate"]),
                                        EndDate = Convert.ToString(dr["EndDate"]),
                                        City = Convert.ToString(dr["City"]),
                                        State = Convert.ToString(dr["State"]),
                                        Country = Convert.ToString(dr["CountryName"])
                                    }).FirstOrDefault(); ;
                }

                if (orderPenalties == null || orderPenalties.Count == 0)
                {
                    if (booking.Penalties > 0)
                    {
                        orderPenalties.Add(new OrderPenaltyDto
                        {
                            Id = 0, // Assuming no actual ID exists for this default penalty
                            PenaltyType = (int)PenaltyType.lateRegistration,
                            PenaltyName = PenaltyType.lateRegistration.ToString(),
                            Amount = booking.Penalties
                        });
                    }
                }

                booking.OrderPenalties = orderPenalties;
                booking.Event = eventDetails;

                return booking;
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

      
        public async Task<BookingBasicDetailDto> GetBookingDetailsId(long bookingId, bool isPayingUnpaidAmount)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetBookingDetailsById");

            try
            {
                objCmd.Parameters.AddWithValue("@BookingId", bookingId);
                objCmd.Parameters.AddWithValue("@IsPayingUnpaidAmount", isPayingUnpaidAmount);


                DataTable dt = await objSQL.FetchDT(objCmd);

                var booking = (from DataRow dr in dt.Rows
                               select new BookingBasicDetailDto
                               {
                                   Id = Convert.ToInt64(dr["Id"]),
                                   UserId = Convert.ToInt64(dr["UserId"]),
                                   OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                   OrganizationName = Convert.ToString(dr["OrganizationName"]),
                                   DisplayCurrencyRate = Convert.ToDecimal(dr["DisplayCurrencyRate"]),
                                   TotalAmount = Convert.ToDecimal(dr["TotalAmount"]),
                                   AmountPaid = Convert.ToDecimal(dr["AmountPaid"]),
                                   CurrencyCode = Convert.ToString(dr["CurrencyCode"]),
                                   WalletAmount = dr["WalletAmount"] != DBNull.Value
                                                ? Convert.ToDecimal(dr["WalletAmount"])
                                                : 0,
                                   MerchantId = Convert.ToString(dr["MerchantId"]),
                                   ApiPassword = Convert.ToString(dr["ApiPassword"]),
                                   User = new UserDetailsDto()
                                   {
                                       Email = Convert.ToString(dr["Email"])
                                   },
                                   PenaltyAmount = Convert.ToDecimal(dr["PenaltyAmount"]),
                                   PaymentUrl = Convert.ToString(dr["PaymentUrl"]),
                                   EventName = Convert.ToString(dr["EventName"]),
                                   ApiVersion = Convert.ToInt32(dr["ApiVersion"])
                               }).FirstOrDefault();

                booking.TotalAmountInDisplayCurrency = booking.TotalAmount;
                booking.TotalAmountInDefaultCurrency = CurrencyHelper.CalculateDefaultCurrencyAmount(booking.TotalAmount, booking.DisplayCurrencyRate);       
                var amount = booking.WalletAmount >= booking.TotalAmountInDisplayCurrency ? 0 : booking.TotalAmountInDisplayCurrency - booking.WalletAmount;
                booking.TotalWalletAmountInDisplayCurrency = amount;
                booking.TotalWalletAmountInDefaultCurrency = CurrencyHelper.CalculateDefaultCurrencyAmount(amount, booking.DisplayCurrencyRate);

                Assertions.IsNotNull(booking, Resources.Resource.DATABASE_ERROR_1028);

                return booking;
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
        public async Task<long> UpdateTransportationDetails(long organizationId, long userId, TransportationInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateTransportationDetail");

            try
            {
                if (string.IsNullOrEmpty(input.VisaStatus) && string.IsNullOrEmpty(input.PassportNumber))
                    throw new ServiceException("PassportNumber is required.");

                if (string.IsNullOrEmpty(input.VisaStatus) && string.IsNullOrEmpty(input.PassportImage))
                    throw new ServiceException("PassportImage is required.");

                string accessibilityInfoString = (input.AccessibilityInfo != null && input.AccessibilityInfo.Any()) ? $"[{string.Join(",", input.AccessibilityInfo)}]" : null;

                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@GuestId", input.GuestId);
                objCmd.Parameters.AddWithValue("@BookingId", input.OrderId);
                objCmd.Parameters.AddWithValue("@UserId", userId);
                objCmd.Parameters.AddWithValue("@PassportNumber", input.PassportNumber);
                objCmd.Parameters.AddWithValue("@PassportIssueDate", input.PassportIssueDate);
                objCmd.Parameters.AddWithValue("@PassportExpiryDate", input.PassportExpiryDate);
                objCmd.Parameters.AddWithValue("@DOB", input.DOB);
                objCmd.Parameters.AddWithValue("@Occupation", input.Occupation);
                objCmd.Parameters.AddWithValue("@Nationality", input.Nationality);
                objCmd.Parameters.AddWithValue("@JobTitle", input.JobTitle);
                objCmd.Parameters.AddWithValue("@WorkPlace", input.WorkPlace);
                objCmd.Parameters.AddWithValue("@DepartureFlightAirport", input.DepartureFlightAirport);
                objCmd.Parameters.AddWithValue("@ArrivalFlightAirport", input.ArrivalFlightAirport);
                objCmd.Parameters.AddWithValue("@ArrivalFlightNumber", input.ArrivalFlightNumber);
                objCmd.Parameters.AddWithValue("@DepartureFlightNumber", input.DepartureFlightNumber);
                objCmd.Parameters.AddWithValue("@ArrivalDateTime", input.ArrivalDateTime);
                objCmd.Parameters.AddWithValue("@DepartureDateTime", input.DepartureDateTime);
                objCmd.Parameters.AddWithValue("@ArrivalNotes", input.ArrivalNotes);
                objCmd.Parameters.AddWithValue("@DepartureNotes", input.DepartureNotes);
                objCmd.Parameters.AddWithValue("@AccessibilityInfo", accessibilityInfoString);
                objCmd.Parameters.AddWithValue("@Status", input.IsCompleted() ? ProfileStatus.profileCompleted : ProfileStatus.profileIncomplete);
                objCmd.Parameters.AddWithValue("@Photo", input.Photo);
                objCmd.Parameters.AddWithValue("@VisaDocument", input.VisaDocument);
                objCmd.Parameters.AddWithValue("@VisaOfficialLetterDocument", input.VisaOfficialLetterDocument);
                objCmd.Parameters.AddWithValue("@VisaStatus", !string.IsNullOrEmpty(input.VisaStatus) ? StatusExtensions.ToVisaStatusEnum(input.VisaStatus) : null);
                objCmd.Parameters.AddWithValue("@PassportImage", input.PassportImage);

                DataTable dtGuest = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtGuest.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                if(!string.IsNullOrEmpty(input.VisaDocument) || !string.IsNullOrEmpty(input.VisaOfficialLetterDocument))
                {

                    var user = await _authServices.GetFcmTokenForCustomer(organizationId, userId);

                    string emailBody = $@"
                        <div style='max-width: 600px; margin: 20px auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9; font-family: Arial, sans-serif;'>
                            <div style='margin-bottom: 20px;'>
                                <img src='{user.Logo}' alt='Organization Logo'
                                    style='max-width: 140px; max-height: 80px; object-fit: contain; object-position: center;
                                    background-color: #f5f5f5; padding: 6px; display: block; border-radius: 4px;' />
                            </div>

                            <div style='padding: 20px; background-color: #ffffff; border-radius: 8px;'>
                                <p style='font-size: 16px; color: #333;'>Hi {user.Name},</p>
                                <p style='font-size: 14px; color: #555;'>
                                    The organization has uploaded the visa document. Please download it from the booking details page.
                                </p>
                                <br/>
                                <p style='font-size: 14px; color: #333; font-weight: bold;'>Best regards,</p>
                                <p style='font-size: 14px; color: #333;'>{user.OrganizationName}</p>
                            </div>
                        </div>";


                    _emailService.SendEmail(user.Email, "Visa Uploaded", emailBody);

                    // Check if user or token is not null
                    if (user != null && !string.IsNullOrEmpty(user.FcmToken)) // Assuming 'FcmToken' is the property holding the token
                    {
                        // Create a string array with the token
                        string[] fcmTokensArray = new string[1];
                        fcmTokensArray[0] = user.FcmToken; // Assign the FCM token to the array

                        // Step 7: Send push notifications
                        string title = "Visa uploaded";
                        string message = $"Visa document has been uploaded by {user.OrganizationName}.";

                        var rs = await _firebaseServices.SendFirebaseNotification(fcmTokensArray, title, message, true);

                    }
                }
               
                return Convert.ToInt64(dtGuest.Rows[0]["Id"]);
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

        public async Task<TransportationListDto> GetAllTransportationDetails(long organizationId, long? hotelId, long? eventId, string? date, int? pageNo, int? pageSize, string sortColumn = "", string sortOrder = "", string searchText = "", bool isArrival = true)
        {
            List<GuestDetailDto> guests = new List<GuestDetailDto>();
            var totalCount = 0;
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllTransportationDetails");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                if(hotelId.HasValue)
                    objCmd.Parameters.AddWithValue("@HotelId", hotelId.Value);
                objCmd.Parameters.AddWithValue("@EventId", eventId);
                objCmd.Parameters.AddWithValue("@Date", date);
                objCmd.Parameters.AddWithValue("@IsArrival", isArrival);
                objCmd.Parameters.AddWithValue("@PageNumber", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SearchText", searchText);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);

                DataTable dt = await objSQL.FetchDT(objCmd);

                if (dt.Rows.Count > 0)
                {
                    guests = (from DataRow dr in dt.Rows
                              select new GuestDetailDto
                              {
                                  Id = Convert.ToInt64(dr["Id"]),
                                  OrderId = Convert.ToInt64(dr["OrderId"]),
                                  EventId = dr["EventId"] != DBNull.Value ? Convert.ToInt64(dr["EventId"]) : 0,
                                  Role = Convert.ToString(dr["Role"]),
                                  PassportFirstName = dr["PassportFirstName"] != DBNull.Value ? dr["PassportFirstName"].ToString() : null,
                                  PassportLastName = dr["PassportLastName"] != DBNull.Value ? dr["PassportLastName"].ToString() : null,
                                  PassportNumber = dr["PassportNumber"] != DBNull.Value ? dr["PassportNumber"].ToString() : null,
                                  PassportIssueDate = dr["PassportIssueDate"].ToString(),
                                  PassportExpiryDate = dr["PassportExpiryDate"].ToString(),
                                  DOB = Convert.ToString(dr["DOB"]),
                                  Occupation = !string.IsNullOrEmpty(dr["Occupation"].ToString()) ? dr["Occupation"].ToString() : string.Empty,
                                  Nationality = !string.IsNullOrEmpty(dr["Nationality"].ToString()) ? dr["Nationality"].ToString() : string.Empty,
                                  JobTitle = !string.IsNullOrEmpty(dr["JobTitle"].ToString()) ? dr["JobTitle"].ToString() : string.Empty,
                                  WorkPlace = !string.IsNullOrEmpty(dr["WorkPlace"].ToString()) ? dr["WorkPlace"].ToString() : string.Empty,
                                  DepartureFlightAirport = !string.IsNullOrEmpty(dr["DepartureFlightAirport"].ToString()) ? dr["DepartureFlightAirport"].ToString() : string.Empty,
                                  ArrivalFlightAirport = !string.IsNullOrEmpty(dr["ArrivalFlightAirport"].ToString()) ? dr["ArrivalFlightAirport"].ToString() : string.Empty,
                                  ArrivalFlightNumber = !string.IsNullOrEmpty(dr["ArrivalFlightNumber"].ToString()) ? dr["ArrivalFlightNumber"].ToString() : string.Empty,
                                  DepartureFlightNumber = !string.IsNullOrEmpty(dr["DepartureFlightNumber"].ToString()) ? dr["DepartureFlightNumber"].ToString() : string.Empty,
                                  ArrivalDateTime = dr["ArrivalDateTime"].ToString(),
                                  DepartureDateTime = dr["DepartureDateTime"].ToString(),
                                  ArrivalNotes = !string.IsNullOrEmpty(dr["ArrivalNotes"].ToString()) ? dr["ArrivalNotes"].ToString() : string.Empty,
                                  DepartureNotes = !string.IsNullOrEmpty(dr["DepartureNotes"].ToString()) ? dr["DepartureNotes"].ToString() : string.Empty,
                                  AccessibilityInfo = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfo"])) ? ConversionHelper.ConvertStringToArray(Convert.ToString(dr["AccessibilityInfo"])) : new List<int>(),
                                  AccessibilityInfoData = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfoData"])) ? JsonConvert.DeserializeObject<List<AccessiblityInfoDto>>(Convert.ToString(dr["AccessibilityInfoData"])) : new List<AccessiblityInfoDto>(),
                                  HotelId = dr["GuestHotelId"] != DBNull.Value ? Convert.ToInt32(dr["GuestHotelId"]) : (int?)null,
                                  HotelRoomTypeId = dr["HotelRoomTypeId"] != DBNull.Value ? Convert.ToInt32(dr["HotelRoomTypeId"]) : (int?)null,
                                  FromDate = Convert.ToString(dr["FromDate"]),
                                  ToDate = Convert.ToString(dr["ToDate"]),
                                  VisaAssistanceRequired = dr["VisaAssistanceRequired"] != DBNull.Value ? Convert.ToBoolean(dr["VisaAssistanceRequired"]) : false,
                                  VisaOfficialLetterRequired = dr["VisaOfficialLetterRequired"] != DBNull.Value ? Convert.ToBoolean(dr["VisaOfficialLetterRequired"]) : false,
                                  VisaDocument = Convert.ToString(dr["VisaDocument"]),
                                  CreatedDate = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : (DateTime?)null,
                                  UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                  SequenceNo = dr["SequenceNo"] != DBNull.Value ? Convert.ToInt32(dr["SequenceNo"]) : 0,
                                  RegistrationFee = dr["RegistrationFee"] != DBNull.Value ? Convert.ToDecimal(dr["RegistrationFee"]) : 0,
                                  Status = dr["Status"] != DBNull.Value ? StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])) : null,
                                  VisaStatus = dr["VisaStatus"] != DBNull.Value ? ((VisaStatus)Convert.ToInt32(dr["VisaStatus"])).ToString() : null,
                                  Photo = Convert.ToString(dr["Photo"]),
                                  PassportImage = Convert.ToString(dr["PassportImage"]),
                                  Hotel = new HotelDetailsDto()
                                  {
                                      Id = Convert.ToInt64(dr["Id"]),
                                      OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                      Name = Convert.ToString(dr["HotelName"]),
                                      Rating = Convert.ToDouble(dr["Rating"]),
                                      Address = Convert.ToString(dr["Address"]),
                                      PostalCode = Convert.ToString(dr["PostalCode"]),
                                      City = Convert.ToString(dr["City"]),
                                      State = Convert.ToString(dr["State"]),
                                      Country = Convert.ToString(dr["Country"]),
                                      CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                      LocationLatLong = Convert.ToString(dr["LocationLatLong"]),
                                      //Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                      CreatedDate = Convert.ToDateTime(dr["HotelCreatedDate"]),
                                      UpdatedDate = dr["HotelUpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["HotelUpdatedDate"]) : (DateTime?)null,
                                      RoomType = new HotelRoomType()
                                      {
                                          Id = Convert.ToInt64(dr["RoomTypeId"]),
                                          HotelId = Convert.ToInt64(dr["HotelId"]),
                                          RoomSize = Convert.ToString(dr["RoomSize"]),
                                          PackagePrice = Convert.ToDecimal(dr["PackagePrice"]),
                                          CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
                                          NightPrice = Convert.ToDecimal(dr["NightPrice"]),
                                          Availability = Convert.ToInt32(dr["Availability"]),
                                          Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                          MinimumOccupancy = dr["MinimumOccupancy"] != DBNull.Value ? Convert.ToInt32(dr["MinimumOccupancy"]) : (int?)null,
                                          MaximumOccupancy = dr["MaximumOccupancy"] != DBNull.Value ? Convert.ToInt32(dr["MaximumOccupancy"]) : (int?)null
                                      }
                                  }
                              }).ToList();

                    
                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                }
                return new TransportationListDto { List = guests, TotalCount = totalCount };
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

        public async Task<ReplaceGuestDto> ReplaceGuest(ReplaceGuestInput input, long organizationId, bool isPenaltyAmountPaid = false)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_ReplaceGuest");

            try
            {
                bool isPenalityApplied = false;
                string PaymentUrl = "";
                var details = await GetPenalityDetails(input.OrderId, PenaltyType.guestReplacement, organizationId);

                if (!isPenaltyAmountPaid  && details != null && DateTime.UtcNow >= Convert.ToDateTime(details.Deadline))
                {

                    if (string.IsNullOrEmpty(details.MerchantId) && string.IsNullOrEmpty(details.ApiPassword))
                        throw new ServiceException(Resource.PAYMENT_SETUP_MISSING);

                    isPenalityApplied = true;
                    var fee = details.Fees;

                    var amountInDefaultCurrency = CurrencyHelper.CalculateDefaultCurrencyAmount(details.Fees, details.DisplayCurrencyRate);


                    string sessionId = await _mastercardPaymentService.CreateSessionForPenality(details.MerchantId, details.ApiPassword, input.OrderId, amountInDefaultCurrency, details.CurrencyCode, input.SuccessUrl, input.CancelUrl, details.Id, input.OldGuestId, input.NewGuestId, input.PassportFirstName, input.PassportLastName, input.PassportNumber, details.OrganizationName, organizationId, details.Fees, details.PaymentUrl, details.EventName, details.ApiVersion);

                    PaymentUrl = $"{details.PaymentUrl}/checkout/pay/{sessionId}?checkoutVersion=1.0.0";

                }
                else
                {
                    objCmd.CommandType = CommandType.StoredProcedure;
                    objCmd.Parameters.AddWithValue("@OldGuestId", input.OldGuestId);
                    if(input.NewGuestId.HasValue)
                        objCmd.Parameters.AddWithValue("@NewGuestId", input.NewGuestId);
                    objCmd.Parameters.AddWithValue("@OrderId", input.OrderId);
                    if (input.NewGuestId == null || input.NewGuestId <= 0)
                    {
                        objCmd.Parameters.AddWithValue("@PassportFirstName", input.PassportFirstName);
                        objCmd.Parameters.AddWithValue("@PassportLastName", input.PassportLastName);
                        objCmd.Parameters.AddWithValue("@PassportNumber", input.PassportNumber);
                    }
                    await objSQL.UpdateDB(objCmd);
                }
                return new ReplaceGuestDto
                {
                    IsPenalityApplied = isPenalityApplied,
                    PaymentUrl = PaymentUrl
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }
   

        public async Task<PenaltiesInfo> GetPenalityDetails(long orderId, PenaltyType type, long organizationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetPenalityDetails");

            try
            {
                objCmd.Parameters.AddWithValue("@OrderId", orderId);
                objCmd.Parameters.AddWithValue("@PenaltyType", type);
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var penalityInfo = (from DataRow dr in dt.Rows
                                    select new PenaltiesInfo
                                    {
                                        Id = Convert.ToInt64(dr["Id"]),
                                        OrganizationName = Convert.ToString(dr["OrganizationName"]),
                                        PenaltyType = Convert.ToInt32(dr["PenaltyType"]),
                                        Deadline = Convert.ToString(dr["Deadline"]),
                                        Fees = Convert.ToDecimal(dr["Fees"]),
                                        CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
                                        IsPercentage = Convert.ToBoolean(dr["IsPercentage"]),
                                        CurrencyCode = Convert.ToString(dr["Code"]),
                                        MerchantId = Convert.ToString(dr["MerchantId"]),
                                        ApiPassword = Convert.ToString(dr["ApiPassword"]),
                                        DisplayCurrencyRate = dr["DisplayCurrencyRate"] != DBNull.Value ? Convert.ToDecimal(dr["DisplayCurrencyRate"]) : 0,
                                        PaymentUrl = Convert.ToString(dr["PaymentUrl"]),
                                        EventName = Convert.ToString(dr["EventName"]),
                                        ApiVersion = Convert.ToInt32(dr["ApiVersion"])
                                    }).FirstOrDefault();

                return penalityInfo;
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

        public async Task<long> CancelGuest(long guestId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_CancelGuest");

            try
            {
                objCmd.Parameters.AddWithValue("@GuestId", guestId);
                objCmd.Parameters.AddWithValue("@Status", ProfileStatus.Cancelled);

                DataTable dtGuest = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtGuest.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return guestId;
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

        public async Task<decimal> CancelBooking(long orderId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_CancelBooking");

            try
            {
                // Call GetAmountandCurrency method to retrieve amount and currency
                //OrderDto orderDetails = await GetAmountandCurrency(orderId);

                // Perform refund using RefundPayment method
                //instead we are sending amount to wallet 
                //string refundId = await _mastercardPaymentService.RefundPayment(orderId, orderDetails.Amount, orderDetails.Currency);

                objCmd.Parameters.AddWithValue("@OrderId", orderId);
                objCmd.Parameters.AddWithValue("@PenaltyType", PenaltyType.cancellation);
                objCmd.Parameters.AddWithValue("@Status", Status.Cancelled);

                DataTable dtGuest = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtGuest.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                decimal refundAmount = dtGuest.Rows[0]["RefundAmount"] != DBNull.Value
                                ? Convert.ToDecimal(dtGuest.Rows[0]["RefundAmount"])
                                : 0m;
                string logo = Convert.ToString(dtGuest.Rows[0]["Logo"]);
                string supportEmail = Convert.ToString(dtGuest.Rows[0]["SupportEmail"]);
                string eventName = Convert.ToString(dtGuest.Rows[0]["EventName"]);
                string organizationName = Convert.ToString(dtGuest.Rows[0]["OrganizationName"]);
                string userName = Convert.ToString(dtGuest.Rows[0]["UserName"]);
                string userEmail = Convert.ToString(dtGuest.Rows[0]["UserEmail"]);
                decimal penaltyAmount = dtGuest.Rows[0]["PenaltyAmount"] != DBNull.Value
                                    ? Convert.ToDecimal(dtGuest.Rows[0]["PenaltyAmount"])
                                    : 0m; // Default to 0 if NULL


                // Determine whether to display RefundAmount or PenaltyAmount
                string amountLabel;
                string displayAmount;

                if (refundAmount > 0)
                {
                    amountLabel = "Refund Amount";
                    displayAmount = refundAmount.ToString("F2");
                }
                else
                {
                    amountLabel = "Penalty Amount";
                    displayAmount = penaltyAmount.ToString("F2");
                }

                // Load the email template
                var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "cancel-booking.html");
                var html = await System.IO.File.ReadAllTextAsync(templatePath);

                // Replace placeholders in email template
                html = html.Replace("{{OrganizationLogo}}", logo)
                           .Replace("{{OrganizationName}}", organizationName)
                           .Replace("{{SupportEmail}}", supportEmail)
                           .Replace("{{EventName}}", eventName)
                           .Replace("{{UserName}}", userName)
                           //.Replace("{{RefundAmount}}", refundAmount.ToString("F2"))
                           //.Replace("{{PenaltyAmount}}", penaltyAmount.ToString("F2"));
                           .Replace("{{AmountLabel}}", amountLabel)
                           .Replace("{{DisplayAmount}}", displayAmount);

                // Send email (assuming SendEmailAsync is implemented)
                _emailService.SendEmail(userEmail, "Event Booking Cancellation", html);

                return refundAmount;
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

        public async Task<VisaGuestsDto> GetVisaRequiredGuests(long organizationId, long? eventId, string? status, int? pageNo, int? pageSize, string searchText, string sortOrder, string sortColumn)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetVisaRequiredGuests");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("EventId", eventId);
                objCmd.Parameters.AddWithValue("@Status", !string.IsNullOrEmpty(status) ? StatusExtensions.ToStatusEnum(status.ToLower()) : null);
                objCmd.Parameters.AddWithValue("@PageNumber", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SearchText", searchText);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);

                DataTable dt = await objSQL.FetchDT(objCmd);
                var guests = new List<VisaGuestDto>();
                var totalCount = 0;

                if (dt.Rows.Count > 0)
                {
                    guests = (from DataRow dr in dt.Rows
                              select new VisaGuestDto
                              {
                                  Id = Convert.ToInt64(dr["Id"]),
                                  OrderId = Convert.ToInt64(dr["OrderId"]),
                                  EventId = dr["EventId"] != DBNull.Value ? Convert.ToInt64(dr["EventId"]) : 0,
                                  Role = Convert.ToString(dr["Role"]),
                                  PassportFirstName = dr["PassportFirstName"] != DBNull.Value ? dr["PassportFirstName"].ToString() : null,
                                  PassportLastName = dr["PassportLastName"] != DBNull.Value ? dr["PassportLastName"].ToString() : null,
                                  PassportNumber = dr["PassportNumber"] != DBNull.Value ? dr["PassportNumber"].ToString() : null,
                                  PassportIssueDate = dr["PassportIssueDate"].ToString(),
                                  PassportExpiryDate = dr["PassportExpiryDate"].ToString(),
                                  DOB = Convert.ToString(dr["DOB"]),
                                  Occupation = !string.IsNullOrEmpty(dr["Occupation"].ToString()) ? dr["Occupation"].ToString() : string.Empty,
                                  Nationality = !string.IsNullOrEmpty(dr["Nationality"].ToString()) ? dr["Nationality"].ToString() : string.Empty,
                                  JobTitle = !string.IsNullOrEmpty(dr["JobTitle"].ToString()) ? dr["JobTitle"].ToString() : string.Empty,
                                  WorkPlace = !string.IsNullOrEmpty(dr["WorkPlace"].ToString()) ? dr["WorkPlace"].ToString() : string.Empty,
                                  DepartureFlightAirport = !string.IsNullOrEmpty(dr["DepartureFlightAirport"].ToString()) ? dr["DepartureFlightAirport"].ToString() : string.Empty,
                                  ArrivalFlightAirport = !string.IsNullOrEmpty(dr["ArrivalFlightAirport"].ToString()) ? dr["ArrivalFlightAirport"].ToString() : string.Empty,
                                  ArrivalFlightNumber = !string.IsNullOrEmpty(dr["ArrivalFlightNumber"].ToString()) ? dr["ArrivalFlightNumber"].ToString() : string.Empty,
                                  DepartureFlightNumber = !string.IsNullOrEmpty(dr["DepartureFlightNumber"].ToString()) ? dr["DepartureFlightNumber"].ToString() : string.Empty,
                                  ArrivalDateTime = dr["ArrivalDateTime"].ToString(),
                                  DepartureDateTime = dr["DepartureDateTime"].ToString(),
                                  ArrivalNotes = !string.IsNullOrEmpty(dr["ArrivalNotes"].ToString()) ? dr["ArrivalNotes"].ToString() : string.Empty,
                                  DepartureNotes = !string.IsNullOrEmpty(dr["DepartureNotes"].ToString()) ? dr["DepartureNotes"].ToString() : string.Empty,
                                  AccessibilityInfo = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfo"])) ? ConversionHelper.ConvertStringToArray(Convert.ToString(dr["AccessibilityInfo"])) : new List<int>(),
                                  AccessibilityInfoData = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfoData"])) ? JsonConvert.DeserializeObject<List<AccessiblityInfoDto>>(Convert.ToString(dr["AccessibilityInfoData"])) : new List<AccessiblityInfoDto>(),
                                  HotelId = dr["HotelId"] != DBNull.Value ? Convert.ToInt32(dr["HotelId"]) : (int?)null,
                                  HotelRoomTypeId = dr["HotelRoomTypeId"] != DBNull.Value ? Convert.ToInt32(dr["HotelRoomTypeId"]) : (int?)null,
                                  FromDate = Convert.ToString(dr["FromDate"]),
                                  ToDate = Convert.ToString(dr["ToDate"]),
                                  VisaAssistanceRequired = dr["VisaAssistanceRequired"] != DBNull.Value ? Convert.ToBoolean(dr["VisaAssistanceRequired"]) : false,
                                  VisaOfficialLetterRequired = dr["VisaOfficialLetterRequired"] != DBNull.Value ? Convert.ToBoolean(dr["VisaOfficialLetterRequired"]) : false,
                                  VisaDocument = Convert.ToString(dr["VisaDocument"]),
                                  CreatedDate = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : (DateTime?)null,
                                  UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                  SequenceNo = dr["SequenceNo"] != DBNull.Value ? Convert.ToInt32(dr["SequenceNo"]) : 0,
                                  RegistrationFee = dr["RegistrationFee"] != DBNull.Value ? Convert.ToDecimal(dr["RegistrationFee"]) : 0,
                                  Status = dr["Status"] != DBNull.Value ? ((ProfileStatus)Convert.ToInt32(dr["Status"])).ToString() : string.Empty,
                                  VisaStatus = dr["VisaStatus"] != DBNull.Value ? ((VisaStatus)Convert.ToInt32(dr["VisaStatus"])).ToString() : null,
                                  Photo = Convert.ToString(dr["Photo"]),
                                  PassportImage = Convert.ToString(dr["PassportImage"]),
                                  Customer = new CustomerDetail()
                                  {
                                      Id = Convert.ToInt64(dr["CustomerId"]),
                                      FirstName = dr["FirstName"].ToString(),
                                      LastName = dr["LastName"].ToString(),
                                      Country = dr["Country"].ToString(),
                                      CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                      Email = dr["Email"].ToString(),
                                      Phone = dr["Phone"].ToString(),
                                      Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                      CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                      UpdatedDate = dr["UpdatedDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedDate"]),
                                  },
                                  Order = new OrdersDto()
                                  {
                                      Id = Convert.ToInt64(dr["OrderId"]),
                                      UserId = Convert.ToInt64(dr["UserId"]),
                                      OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                      Status = dr["OrderStatus"] != DBNull.Value ? StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["OrderStatus"])) : null,
                                      PaymentStatus = dr["PaymentStatus"] != DBNull.Value ? ((PaymentStatus)Convert.ToInt32(dr["PaymentStatus"])).ToString() : string.Empty   
                                  }
                              }).ToList();

                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                }

                return new VisaGuestsDto { List = guests, TotalCount = totalCount };

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                objSQL?.Dispose();
                objCmd?.Dispose();
            }
        }

        public async Task<long> UpdateVisaStatus(long id, VisaStatusInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateVisaStatus");

            try
            {
                var visaStatus = StatusExtensions.ToVisaStatusEnum(input.VisaStatus);

                if (visaStatus == null || (visaStatus != VisaStatus.rejected && visaStatus != VisaStatus.issued))
                {
                    throw new ServiceException($"Invalid status: {input.VisaStatus}");
                }

                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@GuestId", id);
                objCmd.Parameters.AddWithValue("@VisaStatus", visaStatus);

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

        public async Task<long> AddUpdateTickets(long organizationId, long userId, TicketInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_CreateTicket");
            try
            {
                if (input.TicketId.HasValue)
                    objCmd.Parameters.AddWithValue("@TicketId", input.TicketId);
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@UserId", userId);
                objCmd.Parameters.AddWithValue("@Subject", input.Subject);
                objCmd.Parameters.AddWithValue("@Message", input.Message);
                objCmd.Parameters.AddWithValue("@Status", TicketStatus.open);

                DataTable dtTicket = await objSQL.FetchDT(objCmd);

                return Convert.ToInt64(dtTicket.Rows[0]["TicketId"]);

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

        public async Task<Ticket> ReplyToTicket(long organizationId, long userId, string userRole, ReplyTicketInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_ReplyToTicket");
            try
            {
                // Determine if user is staff or customer
                bool isCustomer = userRole == DataAccess.Enums.Roles.Customer.ToString().ToLower();

                // Validate input based on user role
                if (isCustomer)
                {
                    if (string.IsNullOrEmpty(input.Message))
                        throw new ServiceException("Message is required.");
                }
                else
                {
                    if (string.IsNullOrEmpty(input.ReplyMessage))
                        throw new ServiceException("ReplyMessage is required.");
                }

                objCmd.Parameters.AddWithValue("@TicketId", input.TicketId);
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@UserId", userId);
                if(isCustomer)
                    objCmd.Parameters.AddWithValue("@Message", input.Message);
                else
                    objCmd.Parameters.AddWithValue("@ReplyMessage", input.ReplyMessage);
                objCmd.Parameters.AddWithValue("@Status", TicketStatus.inProgress);

                DataTable dt = await objSQL.FetchDT(objCmd);
                Ticket ticket = new Ticket();

                if (dt.Rows.Count > 0)
                {
                    var error = Convert.ToInt64(dt.Rows[0]["ErrorCode"]);
                    var errorMessage = CommonUtilities.GetErrorMessage(error);
                    if (!string.IsNullOrEmpty(errorMessage))
                        throw new ServiceException(errorMessage);

                    var dr = dt.Rows[0];

                    ticket = new Ticket
                    {
                        Id = Convert.ToInt64(dr["Id"]),
                        OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                        UserId = Convert.ToInt64(dr["UserId"]),
                        Subject = Convert.ToString(dr["Subject"]),
                        Message = Convert.ToString(dr["Message"]),
                        ReplyMessage = Convert.ToString(dr["ReplyMessage"]),
                        AssignedToId = dr["AssignedToId"] != DBNull.Value ? Convert.ToInt64(dr["AssignedToId"]) : (long?)null,
                        AssignedById = dr["AssignedById"] != DBNull.Value ? Convert.ToInt64(dr["AssignedById"]) : (long?)null,
                        ParentTicketId = dr["ParentTicketId"] != DBNull.Value ? Convert.ToInt64(dr["ParentTicketId"]) : (long?)null,
                        Status = dr["Status"] != DBNull.Value ? ((TicketStatus)Convert.ToInt32(dr["Status"])).ToString() : string.Empty,
                        CreatedAt = dr["CreatedAt"] != DBNull.Value ? DateTimeOffset.Parse(dr["CreatedAt"].ToString()) : (DateTimeOffset?)null,
                        ClosedAt = dr["ClosedAt"] != DBNull.Value ? DateTimeOffset.Parse(dr["ClosedAt"].ToString()) : (DateTimeOffset?)null,
                        User = new UserInfo()
                        {
                            Id = Convert.ToInt64(dr["Id"]),
                            FirstName = Convert.ToString(dr["FirstName"]),
                            LastName = Convert.ToString(dr["LastName"]),
                            RoleId = Convert.ToInt32(dr["RoleId"]),
                            Role = Convert.ToString(dr["Role"])
                        }
                    };
                }
                return ticket;
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


        public async Task<long> AssignTicket(long userId, string userRole, AssignTicketInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AssignTicket");
            try
            {

                objCmd.Parameters.AddWithValue("@TicketId", input.TicketId);
                objCmd.Parameters.AddWithValue("@AssignedToId", input.AssignedToId);
                objCmd.Parameters.AddWithValue("@AssignedById", userId);
                objCmd.Parameters.AddWithValue("@UserRole", userRole);

                DataTable dtTicket = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtTicket.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return input.TicketId;

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

        public async Task<long> CloseTicket(long ticketId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_CloseTicket");
            try
            {

                objCmd.Parameters.AddWithValue("@TicketId", ticketId);
                objCmd.Parameters.AddWithValue("@Status", TicketStatus.closed);

                DataTable dtTicket = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtTicket.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return ticketId;

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

        public async Task<List<Ticket>> GetMessageForTickets(long ticketId, long userId, long organizationId, string userRole)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetMessagesForTicket");

            try
            {
                objCmd.Parameters.AddWithValue("@TicketId", ticketId);
                objCmd.Parameters.AddWithValue("@UserId",  userId);
                objCmd.Parameters.AddWithValue("@UserRole", userRole);
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);

                DataTable dt = await objSQL.FetchDT(objCmd);
                List<Ticket> tickets = new List<Ticket>();

                if (dt.Rows.Count > 0)
                {
                    if (dt.Columns.Contains("ErrorCode"))
                    {
                        var error = Convert.ToInt64(dt.Rows[0]["ErrorCode"]);
                        var errorMessage = CommonUtilities.GetErrorMessage(error);
                        if (!string.IsNullOrEmpty(errorMessage))
                            throw new ServiceException(errorMessage);
                    }
                    else
                    {
                        tickets = (from DataRow dr in dt.Rows
                                   select new Ticket
                                   {
                                       Id = Convert.ToInt64(dr["Id"]),
                                       OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                       UserId = Convert.ToInt64(dr["UserId"]),
                                       Subject = Convert.ToString(dr["Subject"]),
                                       Message = Convert.ToString(dr["Message"]),
                                       ReplyMessage = Convert.ToString(dr["ReplyMessage"]),
                                       AssignedToId = dr["AssignedToId"] != DBNull.Value ? Convert.ToInt64(dr["AssignedToId"]) : (long?)null,
                                       AssignedById = dr["AssignedById"] != DBNull.Value ? Convert.ToInt64(dr["AssignedById"]) : (long?)null,
                                       ParentTicketId = dr["ParentTicketId"] != DBNull.Value ? Convert.ToInt32(dr["ParentTicketId"]) : (long?)null,
                                       Status = dr["Status"] != DBNull.Value ? ((TicketStatus)Convert.ToInt32(dr["Status"])).ToString() : string.Empty,
                                       CreatedAt = dr["CreatedAt"] != DBNull.Value ? DateTimeOffset.Parse(dr["CreatedAt"].ToString()) : (DateTimeOffset?)null,
                                       ClosedAt = dr["ClosedAt"] != DBNull.Value ? DateTimeOffset.Parse(dr["ClosedAt"].ToString()) : (DateTimeOffset?)null,
                                       User = new UserInfo()
                                       {
                                           Id = Convert.ToInt64(dr["Id"]),
                                           FirstName = Convert.ToString(dr["FirstName"]),
                                           LastName = Convert.ToString(dr["LastName"]),
                                           RoleId = Convert.ToInt32(dr["RoleId"]),
                                           Role = Convert.ToString(dr["Role"])
                                       }
                                   }).ToList();
                    }
                }

                return tickets;
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

        public async Task<TicketDto> GetTicketsByOrganizationId(long organizationId, long? userId, string role, int? pageNo, int? pageSize, string? searchText, string sortColumn, string sortOrder)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetTicketsByOrganizationId");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@UserId", role.ToString().ToLower() == "customer" ? userId : (object)DBNull.Value);
                objCmd.Parameters.AddWithValue("@StaffId", role.ToString().ToLower() != "customer" ? userId : (object)DBNull.Value);
                objCmd.Parameters.AddWithValue("@UserRole", role);
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SearchText", searchText);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);

                DataTable dt = await objSQL.FetchDT(objCmd);
                int totalCount = 0;
                List<Ticket> tickets = new List<Ticket>();

                if (dt.Rows.Count > 0)
                {
                    tickets = (from DataRow dr in dt.Rows
                               select new Ticket
                               {
                                   Id = Convert.ToInt64(dr["Id"]),
                                   OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                   UserId = Convert.ToInt64(dr["UserId"]),
                                   Subject = Convert.ToString(dr["Subject"]),
                                   Message = Convert.ToString(dr["Message"]),
                                   ReplyMessage = Convert.ToString(dr["ReplyMessage"]),
                                   AssignedToId = dr["AssignedToId"] != DBNull.Value ? Convert.ToInt64(dr["AssignedToId"]) : (long?)null,
                                   AssignedById = dr["AssignedById"] != DBNull.Value ? Convert.ToInt64(dr["AssignedById"]) : (long?)null,
                                   ParentTicketId = dr["ParentTicketId"] != DBNull.Value ? Convert.ToInt32(dr["ParentTicketId"]) : (int?)null,
                                   Status = ((TicketStatus)Convert.ToInt32(dr["Status"])).ToString(),
                                   CreatedAt = dr["CreatedAt"] != DBNull.Value ? DateTimeOffset.Parse(dr["CreatedAt"].ToString()) : (DateTimeOffset?)null,
                                   ClosedAt = dr["ClosedAt"] != DBNull.Value ? DateTimeOffset.Parse(dr["ClosedAt"].ToString()) : (DateTimeOffset?)null,
                                   User = new UserInfo()
                                   {
                                       Id = Convert.ToInt64(dr["Id"]),
                                       FirstName = Convert.ToString(dr["FirstName"]),
                                       LastName = Convert.ToString(dr["LastName"]),
                                       RoleId = Convert.ToInt32(dr["RoleId"]),
                                       Role = Convert.ToString(dr["Role"])
                                   }
                               }).ToList();

                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                }

                return new TicketDto { List = tickets, TotalCount = totalCount };
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

        public async Task<List<GuestDto>> GetGuestWithoutHotel(long orderId, long guestId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GuestsListWithoutHotel");

            try
            {
                objCmd.Parameters.AddWithValue("@OrderId", orderId);
                objCmd.Parameters.AddWithValue("@GuestId", guestId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var guest = (from DataRow dr in dt.Rows
                              select new GuestDto
                              {
                                  Id = Convert.ToInt64(dr["Id"]),
                                  PassportFirstName = Convert.ToString(dr["PassportFirstName"]),
                                  PassportLastName = Convert.ToString(dr["PassportLastName"]),
                                  PassportNumber = Convert.ToString(dr["PassportNumber"])
                              }).ToList();

                return guest;
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

        public async Task<EventGuestsDto> GetAccreditationList(long organizationId,long? eventId, string? status, string? visaStatus,string? searchText, string? sortOrder, int? pageNo, int? pageSize)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAccreditationList");

            try
            {
                objCmd.Parameters.AddWithValue("OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("EventId", eventId);
                objCmd.Parameters.AddWithValue("@Status", !string.IsNullOrEmpty(status) ? StatusExtensions.ToStatusEnum(status.ToLower()) : null);
                objCmd.Parameters.AddWithValue("@VisaStatus", !string.IsNullOrEmpty(visaStatus) ? StatusExtensions.ToVisaStatusEnum(visaStatus.ToLower()) : null);
                objCmd.Parameters.AddWithValue("@SearchText", string.IsNullOrEmpty(searchText) ? (object)DBNull.Value : searchText);
                objCmd.Parameters.AddWithValue("SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);

                DataSet ds = await objSQL.FetchDB(objCmd);
                int totalCount = 0;
                var events = new List<EventGuestDto>();
                if (ds.Tables[0].Rows.Count > 0)
                {
                      var penalties = (from DataRow dr in ds.Tables[1].Rows
                                     select new PenaltiesDetail
                                     {
                                         PenaltyType = Convert.ToInt32(dr["PenaltyType"]),
                                         Deadline = Convert.ToString(dr["Deadline"]),
                                         Fees = Convert.ToDecimal(dr["Fees"]),
                                         CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
                                         IsPercentage = Convert.ToBoolean(dr["IsPercentage"]),
                                         EventId = Convert.ToInt64(dr["OrganizationEventId"]),
                                     }).ToList();

                    events = (from DataRow dr in ds.Tables[0].Rows
                              select GuestDetailMappings.GetAccreditationList(dr, this, penalties).Result).ToList();

                    totalCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalCount"]);
                }

                return new EventGuestsDto { List = events, TotalCount = totalCount };
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                objSQL?.Dispose();
                objCmd?.Dispose();
            }
		}

        public async Task<OrderDto> GetAmountandCurrency (long orderId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAmountandCurrency");

            try
            {
                objCmd.Parameters.AddWithValue("@OrderId", orderId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var order = (from DataRow dr in dt.Rows
                               select new OrderDto
                               {
                                   Amount = Convert.ToDecimal(dr["Amount"]),
                                   Currency = Convert.ToString(dr["Code"]),
                                   TransactionId = Convert.ToString(dr["TransactionId"])
                               }).FirstOrDefault();

                return order;
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

        public async Task UpdateWallet(long organizationId, long userId, UpdateWalletInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateCustomerWallet");
            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@UserId", userId);
                objCmd.Parameters.AddWithValue("@Amount", input.Amount);
                objCmd.Parameters.AddWithValue("@Status", WalletStatus.creditIn);
                await objSQL.UpdateDB(objCmd);
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

        public async Task<WalletDetailDto> GetWalletDetails(long oraganizationId, long userId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetWalletDetails");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", oraganizationId);
                objCmd.Parameters.AddWithValue("@UserId", userId);

                DataTable dt = await objSQL.FetchDT(objCmd);
                var details = (from DataRow dr in dt.Rows
                               select new WalletDetailDto
                               {
                                   Id = Convert.ToInt64(dr["Id"]),
                                   OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                   UserId = Convert.ToInt64(dr["UserId"]),
                                   Amount = Convert.ToDecimal(dr["Amount"]),
                                   CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                   UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null
                               }).FirstOrDefault();

                if (details == null)
                    return new WalletDetailDto();

                return details;
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

        public async Task<CustomerBasicDetailDto> GetCustomerBasicDetails(long userId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetUserBasicDetails");

            try
            {
                objCmd.Parameters.AddWithValue("@UserId", userId);

                DataTable dt = await objSQL.FetchDT(objCmd);
                var users = (from DataRow dr in dt.Rows
                            select new CustomerBasicDetailDto
                            {
                                UserId = Convert.ToInt64(dr["Id"]),
                                FirstName = Convert.ToString(dr["FirstName"]),
                                LastName = Convert.ToString(dr["LastName"]),
                                Email = Convert.ToString(dr["Email"]),
                                OrganizationName =Convert.ToString(dr["OrganizationName"]),
                                OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                MerchantId = Convert.ToString(dr["MerchantId"]),
                                ApiPassword = Convert.ToString(dr["ApiPassword"]),
                                DisplayCurrencyRate = dr["DisplayCurrencyRate"] != DBNull.Value ? Convert.ToDecimal(dr["DisplayCurrencyRate"]) : 0,
                                PaymentUrl = Convert.ToString(dr["PaymentUrl"]),
                                CurrencyCode = Convert.ToString(dr["CurrencyCode"]),
                                ApiVersion = Convert.ToInt32(dr["ApiVersion"])
                            }).FirstOrDefault();

                return users;
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

        public async Task<WalletSummaryList> WalletSummary(long organizationId, long userId,int? pageNo, int? pageSize, string sortColumn = null, string sortOrder = null)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_WalletSummary");

            
            var wallet = new List<WalletSummaryDto>();
            var totalCount = 0;

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@UserId", userId);
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);

                DataTable dt = await objSQL.FetchDT(objCmd);

                if (dt.Rows.Count > 0)
                {
                    wallet = (from DataRow dr in dt.Rows
                              select new WalletSummaryDto
                              {
                                  Id = Convert.ToInt64(dr["Id"]),
                                  OrderId = dr["OrderId"] != DBNull.Value ? (long?)Convert.ToInt64(dr["OrderId"]) : null, // Handle null
                                  Amount = dr["Amount"] != DBNull.Value ? (decimal?)Convert.ToDecimal(dr["Amount"]) : null, // Handle null
                                  Date = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : (DateTime?)null, // Handle null
                                  Status = dr["Status"] != DBNull.Value ? ((WalletStatus)Convert.ToInt32(dr["Status"])).ToString() : string.Empty ,
                                  Notes = Convert.ToString(dr["Notes"]),
                              }).ToList();

                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                }

                return new WalletSummaryList { List = wallet, TotalCount = totalCount };
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

        public async Task<long> UploadBankReceiptImage(long organizationId, UploadBankReceiptDto model)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UploadBankReceiptImage");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@bookingId", model.BookingId);
                objCmd.Parameters.AddWithValue("@BankReceiptImage", model.BankReceiptImage);

                DataTable dtUser = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtUser.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return model.BookingId;
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
