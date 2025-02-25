using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.BusinessLogic.Services.v1.Implementations;
using EventManagement.DataAccess.Enums;
using EventManagement.DataAccess.Extensions;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;
using EventManagement.Utilities.Helpers;
using Newtonsoft.Json;
using Stripe;
using System.Data;

namespace EventManagement.BusinessLogic.Services.v1.Mappings
{
    public static class GuestDetailMappings
    {

        private static T MapToDto<T>(DataRow dr, ICustomerServices customerServices) where T : GuestDetailDto, new()
        {
            return new T
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
            };
        }

        public static GuestDetailDto MapToGuestDetailDto(DataRow dr, ICustomerServices customerServices)
        {
            return MapToDto<GuestDetailDto>(dr, customerServices);
        }

        public static VisaGuestDto SelectVisaRequiredGuest(DataRow dr, ICustomerServices customerServices)
        {
            var visaGuestDto = MapToDto<VisaGuestDto>(dr, customerServices);

            visaGuestDto.Order = new OrdersDto
            {
                Id = Convert.ToInt64(dr["OrderId"]),
                UserId = Convert.ToInt64(dr["UserId"]),
                OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                Status = dr["Status"] != DBNull.Value ? StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])) : null,
                PaymentStatus = dr["PaymentStatus"] != DBNull.Value ? ((PaymentStatus)Convert.ToInt32(dr["PaymentStatus"])).ToString() : string.Empty,
                OrderDate = Convert.ToString(dr["OrderDate"]),
                PaidAmount = dr["AmountPaid"] != DBNull.Value ? Convert.ToDecimal(dr["AmountPaid"]) : 0m,
                TotalAmount = dr["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(dr["TotalAmount"]) : 0m
            };

            //visaGuestDto.Event = new EventBookings
            //{
            //    Id = Convert.ToInt64(dr["EventId"]),
            //    Name = Convert.ToString(dr["EventName"])
            //};

            visaGuestDto.Customer = new CustomerDetail
            {
                Id = Convert.ToInt64(dr["CustomerId"]),
                UserId = Convert.ToInt64(dr["CustomerUserId"]),
                FirstName = dr["FirstName"].ToString(),
                LastName = dr["LastName"].ToString(),
                Phone = dr["Phone"].ToString(),
                Email = dr["Email"].ToString(),
                Country = dr["Country"].ToString(),
                CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : 0,
                OrganizationType = dr["OrganizationType"].ToString(),
                OrganizationName = dr["OrganizationName"].ToString(),
            };

            return visaGuestDto;
        }


        public static async Task<EventGuestDto> GetAccreditationList(DataRow dr, ICustomerServices customerServices, List<PenaltiesDetail> penalties)
        {
            var eventDetailDto = new EventGuestDto();
            var guestInfo = MapToDto<GuestDetailDto>(dr, customerServices);

            eventDetailDto.Guest = guestInfo;
            long id = Convert.ToInt64(dr["EventId"]);
            eventDetailDto.Event = new EventDetailDto
            {
                Id = Convert.ToInt64(dr["EventId"]),
                OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                Name = dr["Name"].ToString(),
                BannerImage = dr["BannerImage"].ToString(),
                Description = dr["Description"].ToString(),
                Latitude = Convert.ToString(dr["Latitude"]),
                Longitude = Convert.ToString(dr["Longitude"]),
                Address = dr["Address"].ToString(),
                City = dr["City"].ToString(),
                State = dr["State"].ToString(),
                Country = dr["Country"].ToString(),
                CountryId = Convert.ToInt32(dr["CountryId"]),
                TimeZoneId = Convert.ToInt32(dr["TimeZoneId"]),
                StartDate = dr["StartDate"].ToString(),
                EndDate = dr["EndDate"].ToString(),
                AccommodationInfoFile = dr["AccommodationInfoFile"].ToString(),
                TransportationInfoFile = dr["TransportationInfoFile"].ToString(),
                Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                AccommodationPackageInfo = !string.IsNullOrEmpty(Convert.ToString(dr["AccommodationPackageInfo"])) ? ConversionHelper.ConvertStringToList(Convert.ToString(dr["AccommodationPackageInfo"])) : new List<string>(),
                AccessibilityInfo = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfo"])) ? ConversionHelper.ConvertStringToArray(Convert.ToString(dr["AccessibilityInfo"])) : new List<int>(),
                AccessibilityInfoData = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfoData"])) ? JsonConvert.DeserializeObject<List<AccessiblityInfoDto>>(Convert.ToString(dr["AccessibilityInfoData"])) : new List<AccessiblityInfoDto>(),
                PaymentMethodSupported = JsonConvert.DeserializeObject<List<PaymentMethodSupported>>(dr["PaymentMethodSupported"].ToString()),
                RoleWiseData = JsonConvert.DeserializeObject<List<RoleWiseData>>(dr["RoleWiseData"].ToString()),
                PaymentproviderId = dr["PaymentproviderId"] != DBNull.Value ? Convert.ToInt32(dr["PaymentproviderId"]) : (int?)null,
                //Penalties = await eventServices.GetPenaltyByEventId(id)
                Penalties = penalties.Where(e => e.EventId == id).Select(e => new Penalties()
                {
                    CurrencyId = e.CurrencyId,
                    Deadline = e.Deadline,
                    Fees = e.Fees,
                    IsPercentage = e.IsPercentage,
                    PenaltyType = e.PenaltyType
                }).ToList()
            };

            return eventDetailDto;
        }
    }
}
