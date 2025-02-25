using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess.Enums;
using EventManagement.DataAccess.Extensions;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;
using EventManagement.Utilities.Helpers;
using System.Data;

namespace EventManagement.BusinessLogic.Services.v1.Mappings
{
    public static class EventMappings
    {
        public static async Task<EventDetailsDto> MapToDto(DataRow dr)
        {
            var paymentMethodSupported = !string.IsNullOrWhiteSpace(dr["PaymentMethodSupported"].ToString())
                             ? System.Text.Json.JsonSerializer.Deserialize<List<SupportedPaymentMethod>>(dr["PaymentMethodSupported"].ToString())
                             : null;
            var bankTransferMethod = paymentMethodSupported.FirstOrDefault(e => e.Type == "bankTransfer");
            if (bankTransferMethod != null)
            {
                bankTransferMethod.Data = string.IsNullOrEmpty(dr["BankDetails"].ToString()) ? null : dr["BankDetails"].ToString();
            }

            long id = Convert.ToInt64(dr["Id"]);
            return new EventDetailsDto
            {
                Id = id,
                OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                Name = dr["Name"].ToString(),
                BannerImage = dr["BannerImage"].ToString(),
                Description = dr["Description"].ToString(),
                Latitude = Convert.ToString(dr["Latitude"]),
                Longitude = Convert.ToString(dr["Longitude"]),
                Address = Convert.ToString(dr["Address"]),
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
                AccessibilityInfoData = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfoData"])) ? System.Text.Json.JsonSerializer.Deserialize<List<AccessiblityInfoDto>>(Convert.ToString(dr["AccessibilityInfoData"])) : new List<AccessiblityInfoDto>(),
                PaymentMethodSupported = paymentMethodSupported,
                RoleWiseData = !string.IsNullOrEmpty(dr["RoleWiseData"].ToString()) ?System.Text.Json.JsonSerializer.Deserialize<List<RoleWiseData>>(dr["RoleWiseData"].ToString()) : null,
                PaymentproviderId = dr["PaymentproviderId"] != DBNull.Value ? Convert.ToInt32(dr["PaymentproviderId"]) : (int?)null
            };
        }

        public static async Task<T> MapToDto<T>(DataRow dr) where T : new()
        {
            var paymentMethodSupported = !string.IsNullOrWhiteSpace(dr["PaymentMethodSupported"].ToString())
                     ? System.Text.Json.JsonSerializer.Deserialize<List<SupportedPaymentMethod>>(dr["PaymentMethodSupported"].ToString())
                     : null;

            var bankTransferMethod = paymentMethodSupported?.FirstOrDefault(e => e.Type == "bankTransfer");
            if (bankTransferMethod != null)
            {
                bankTransferMethod.Data = string.IsNullOrEmpty(dr["BankDetails"].ToString()) ? null : dr["BankDetails"].ToString();
            }

            T dto = new T();

            foreach (var property in typeof(T).GetProperties())
            {
                if (dr.Table.Columns.Contains(property.Name) && dr[property.Name] != DBNull.Value)
                {
                    if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(dto, dr[property.Name].ToString());
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(dto, Convert.ToInt32(dr[property.Name]));
                    }
                    else if (property.PropertyType == typeof(long))
                    {
                        property.SetValue(dto, Convert.ToInt64(dr[property.Name]));
                    }
                    else if (property.PropertyType == typeof(int?))
                    {
                        property.SetValue(dto, Convert.ToInt32(dr[property.Name]));
                    }
                    else if (property.PropertyType == typeof(long?))
                    {
                        property.SetValue(dto, Convert.ToInt64(dr[property.Name]));
                    }
                    else if (property.PropertyType == typeof(DateTime?))
                    {
                        property.SetValue(dto, dr[property.Name] != DBNull.Value ? Convert.ToDateTime(dr[property.Name]) : (DateTime?)null);
                    }
                    else if (property.PropertyType == typeof(List<RoleWiseData>)) // Handle List<RoleWiseData>
                    {
                        var jsonString = dr[property.Name].ToString();
                        if (!string.IsNullOrWhiteSpace(jsonString))
                        {
                            var listValue = System.Text.Json.JsonSerializer.Deserialize<List<RoleWiseData>>(jsonString);
                            property.SetValue(dto, listValue);
                        }
                        else
                        {
                            property.SetValue(dto, new List<RoleWiseData>());
                        }
                    }
                    else if (property.PropertyType == typeof(List<SupportedPaymentMethod>)) // Handle List<SupportedPaymentMethod>
                    {
                        var jsonString = dr[property.Name].ToString();
                        if (!string.IsNullOrWhiteSpace(jsonString))
                        {
                            var listValue = System.Text.Json.JsonSerializer.Deserialize<List<SupportedPaymentMethod>>(jsonString);
                            property.SetValue(dto, listValue);
                        }
                        else
                        {
                            property.SetValue(dto, new List<SupportedPaymentMethod>());
                        }
                    }
                    // Add more types as needed
                    else
                    {
                        property.SetValue(dto, dr[property.Name]);
                    }
                }
            }

            // Set additional properties manually for known complex cases
            if (typeof(T).GetProperty("PaymentMethodSupported") != null)
            {
                typeof(T).GetProperty("PaymentMethodSupported")?.SetValue(dto, paymentMethodSupported);
            }

            return dto;
        }


    }
}
