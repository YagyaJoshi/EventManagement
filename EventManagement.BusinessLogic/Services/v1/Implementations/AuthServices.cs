using EventManagement.DataAccess;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using EventManagement.BusinessLogic.Exceptions;
using EventManagement.Utilities.Jwt;
using EventManagement.Utilities.Helpers;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Helpers;
using EventManagement.DataAccess.Enums;
using EventManagement.DataAccess.Extensions;
using EventManagement.DataAccess.Models;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Hosting;
using EventManagement.Utilities.Email;
using EventManagement.Utilities.FireBase;



namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class AuthServices : IAuthServices
    {
        private readonly IConfiguration _configuration;

        private readonly IJwtGenerator _jwtGenerator;

        private readonly IMasterServices _masterServices;

        private readonly IHostEnvironment _env;

        private readonly IEmailService _emailService;

        private readonly IFirebaseServices _firebaseServices;

        public AuthServices(IConfiguration configuration, IJwtGenerator jwtGenerator, IMasterServices masterServices, IHostEnvironment env, IEmailService emailService, IFirebaseServices firebaseServices)
        {
            _configuration = configuration;
            _jwtGenerator = jwtGenerator;
            _masterServices = masterServices;
            _env = env;
            _emailService = emailService;
            _firebaseServices = firebaseServices;
        }

        /// <summary>
        /// Authenticates a user by verifying their credentials.
        /// </summary>
        /// <param name="signIn">An object containing the user's sign-in credentials.</param>
        /// <returns>LoginResponseDto</returns>
        /// <exception cref="ServiceException"></exception>

        public async Task<LoginResponseDto> Login(long? organizationId, SignIn signIn)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_loginUser");
            try
            {
                objCmd.Parameters.AddWithValue("@Email", signIn.Email);
                if(organizationId > 0)
                    objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                //var error = Convert.ToInt64(dt.Rows[0]["ErrorCode"]);
                //var errorMessage = CommonUtilities.GetErrorMessage(error);
                //if (!string.IsNullOrEmpty(errorMessage))
                //    throw new ServiceException(errorMessage);

                var user = (from DataRow dr in dt.Rows
                            select new LoginResponseDto
                            {
                                Id = Convert.ToInt64(dr["Id"]),
                                FirstName = dr["FirstName"].ToString(),
                                LastName = dr["LastName"].ToString(),
                                Password = dr["Password"].ToString(),
                                Country = dr["Country"].ToString(),
                                CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                Email = dr["Email"].ToString(),
                                Phone = dr["Phone"].ToString(),
                                Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                UpdatedDate = dr["UpdatedDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedDate"]),
                                RoleId = Convert.ToInt32(dr["RoleId"]),
                                Role = dr["Role"].ToString(),
                                OrganizationStatus = dr["OrganizationStatus"] != DBNull.Value ? StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["OrganizationStatus"])): null
                            }).FirstOrDefault();


                if (user != null && !string.IsNullOrEmpty(user.Password) && PasswordHasher.VerifyPassword(signIn.Password, user.Password))
                {
                    if (user.Role.ToLower() == EventManagement.DataAccess.Enums.Roles.Customer.ToString().ToLower() && (organizationId == null || organizationId <= 0))
                        throw new ServiceException(Resource.ORGANIZATION_REQUIRED);

                    if (user.Status.ToLower() != Status.Active.ToString().ToLower())
                        throw new ServiceException(Resource.INACTIVE_ACCOUNT);

                    if(!string.IsNullOrEmpty(user.OrganizationStatus) && user.OrganizationStatus.ToLower() != Status.Active.ToString().ToLower())
                        throw new ServiceException(Resource.PENDING_ORGANIZATION);

                    var jwtToken = _jwtGenerator.GenerateJwtToken(user.Id, user.Role);
                    user.Token = jwtToken;
                    user.Password = null;
                    return user;
                }

                throw new ServiceException(Resource.INCORRECT_EMAIL_PASSWORD);

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

        /// <summary>
        /// Registers a new entry in the system.
        /// </summary>
        /// <param name="model">The data model containing information for registration.</param>
        /// <returns>The unique identifier (Id) of the newly registered entry.</returns>
        /// <exception cref="ServiceException"></exception>

        public async Task<LoginResponseDto> Register(long organizationId, SignUp signUp)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_RegisterUser");
            try
            {
                // hash password
                var PasswordHash = PasswordHasher.HashPassword(signUp.Password);

                objCmd.Parameters.AddWithValue("@FirstName", signUp.FirstName);
                objCmd.Parameters.AddWithValue("@LastName", signUp.LastName);
                objCmd.Parameters.AddWithValue("@Email", signUp.Email.ToLower());
                objCmd.Parameters.AddWithValue("@PasswordHash", PasswordHash);
                objCmd.Parameters.AddWithValue("@Phone", signUp.Phone);
                objCmd.Parameters.AddWithValue("@CountryId", signUp.CountryId);
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@RoleId", DataAccess.Enums.Roles.Customer);
                objCmd.Parameters.AddWithValue("@OrganizationTypeId", signUp.CustomerOrganizationTypeId);
                objCmd.Parameters.AddWithValue("@OrganizationName", signUp.CustomerOrganizationName);

                DataSet ds = await objSQL.FetchDB(objCmd);

                var error = Convert.ToInt64(ds.Tables[1].Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                var user = (from DataRow dr in ds.Tables[0].Rows
                            select new LoginResponseDto
                            {
                                Id = Convert.ToInt64(dr["Id"]),
                                FirstName = dr["FirstName"].ToString(),
                                LastName = dr["LastName"].ToString(),
                                Password = dr["Password"].ToString(),
                                Country = dr["Country"].ToString(),
                                CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                Email = dr["Email"].ToString(),
                                Phone = dr["Phone"].ToString(),
                                Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                UpdatedDate = dr["UpdatedDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedDate"]),
                                RoleId = Convert.ToInt32(dr["RoleId"]),
                                Role = dr["Role"].ToString(),
                                OrganizationStatus = dr["OrganizationStatus"] != DBNull.Value ? StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["OrganizationStatus"])) : null
                            }).FirstOrDefault();

                
                // Generate JWT token
                var jwtToken = _jwtGenerator.GenerateJwtToken(user.Id, user.Role);
                user.Token = jwtToken;
                user.Password = null; // Clear the password for security
                return user;
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

        public async Task ChangePassword(ChangePasswordInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_ChangePassword");

            try
            {
                var oldPasswordHash = PasswordHasher.HashPassword(input.OldPassword);
                var PasswordHash = PasswordHasher.HashPassword(input.Password);

                objCmd.Parameters.AddWithValue("@UserId", input.UserId);
                objCmd.Parameters.AddWithValue("@OldPassword", oldPasswordHash);
                objCmd.Parameters.AddWithValue("@NewPassword", PasswordHash);
                DataTable dt = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dt.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                objSQL.Dispose();
                objCmd.Dispose();
            }
        }

        public async Task<CustomerResponseDto> LoginCustomer(SignIn signIn)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_loginCustomer");
            try
            {
                objCmd.Parameters.AddWithValue("@Email", signIn.Email);
                DataTable dt = await objSQL.FetchDT(objCmd);
                var customer = (from DataRow dr in dt.Rows
                            select new CustomerResponseDto
                            {
                                Id = Convert.ToInt64(dr["Id"]),
                                FirstName = dr["FirstName"].ToString(),
                                LastName = dr["LastName"].ToString(),
                                Password = dr["Password"].ToString(),
                                Country = dr["Country"].ToString(),
                                CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                Email = dr["Email"].ToString(),
                                Phone = dr["Phone"].ToString(),
                                Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                UpdatedDate = dr["UpdatedDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedDate"])
                            }).FirstOrDefault();


                if (customer != null && !string.IsNullOrEmpty(customer.Password) && PasswordHasher.VerifyPassword(signIn.Password, customer.Password))
                {
                    if (customer.Status.ToLower() != "active")
                        throw new ServiceException(Resource.INACTIVE_ACCOUNT);

                    //if (!string.IsNullOrEmpty(customer.OrganizationStatus) && customer.OrganizationStatus.ToLower() != "active")
                    //    throw new ServiceException(Resource.PENDING_ORGANIZATION);

                    //var jwtToken = _jwtGenerator.GenerateJwtToken(customer.Id);
                    //customer.Token = jwtToken;
                    //customer.Password = null;
                    return customer;
                }

                throw new ServiceException(Resource.INCORRECT_EMAIL_PASSWORD);

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


        public async Task<GenerateOTPDto> GenerateAndSaveOTP(ForgetPasswordInput input)
        {
            
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GenerateOTPForUser");
            try
            {
                objCmd.Parameters.AddWithValue("@Email", input.Email);

                DataTable dtUser = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtUser.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                var user = (from DataRow dr in dtUser.Rows
                            select new GenerateOTPDto
                            {
                                GeneratedOtp = Convert.ToString(dr["GeneratedOtp"]),
                                Name = Convert.ToString(dr["OrganizationName"]),
                                UserName = dr.Table.Columns.Contains("UserName") && dr["UserName"] != DBNull.Value
                           && !string.IsNullOrWhiteSpace(Convert.ToString(dr["UserName"])) // Ensures UserName is not empty
                           ? Convert.ToString(dr["UserName"])
                           : "User"
                            }).FirstOrDefault();

                

                // Prepare the email content using the generated OTP
                var subject = "Reset Your Password - OTP Code";   

                // Step 2: Load the HTML template
                var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "generate-otp.html");
                var html = await File.ReadAllTextAsync(templatePath);

                var emailBody = new StringBuilder(html)
                   .Replace("{{GeneratedOtp}}", user.GeneratedOtp)
                   .Replace("{{Name}}", user.Name)
                   .Replace("{{UserName}}", user.UserName);

                string emailBodyFormatted = emailBody.ToString().Replace("\r\n", " ");

                // Send the email with the OTP
                _emailService.SendEmail(input.Email, subject, emailBodyFormatted);

                // Return the generated OTP and organization name
                return user;
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


        public async Task<string> VerifyOtpandPassword(long organizationId, VerifyOtpInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_VerifyAndUpdatePassword");
            try
            {
                // hash password
                var PasswordHash = PasswordHasher.HashPassword(input.Password);

                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@Email", input.Email);
                objCmd.Parameters.AddWithValue("@OTP", input.Otp);
                objCmd.Parameters.AddWithValue("@NewPassword", PasswordHash);

                DataTable dtUser = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtUser.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return null;
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

        public async Task<Email> SendMailToCustomer(long bookingId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_SendMailToCustomer");

            try
            {
                objCmd.Parameters.AddWithValue("@BookingId", bookingId);
                //objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);

                DataSet ds = await objSQL.FetchDB(objCmd);
                var booking = new Email();
                var orderDetail = new List<OrderDetail>();
                var guestDetail = new List<GuestDetailDto>();
                var accomodation = new List<AccomodationInfoDto>();

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
                                       CreatedDate = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : (DateTime?)null,
                                       UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                       SequenceNo = dr["SequenceNo"] != DBNull.Value ? Convert.ToInt32(dr["SequenceNo"]) : 0,
                                       RegistrationFee = dr["RegistrationFee"] != DBNull.Value ? Convert.ToDecimal(dr["RegistrationFee"]) : 0,
                                       Status = dr["Status"] != DBNull.Value ? StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])) : null,
                                       VisaStatus = dr["VisaStatus"] != DBNull.Value ? ((VisaStatus)Convert.ToInt32(dr["VisaStatus"])).ToString() : null,
                                       Photo = Convert.ToString(dr["Photo"]),
                                       TimeZone = Convert.ToString(dr["TimeZone"])
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
                               select new Email
                               {
                                   Id = Convert.ToInt64(dr["Id"]),
                                   UserId = Convert.ToInt64(dr["UserId"]),
                                   OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                   OrderDate = Convert.ToString(dr["OrderDate"]),
                                   EventId = dr["OrganizationEventId"] != DBNull.Value ? Convert.ToInt64(dr["OrganizationEventId"]) : (long?)null,
                                   OrganizationName = Convert.ToString(dr["OrganizationName"]),
                                   TotalAmountInDisplayCurrency = dr["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(dr["TotalAmount"]) : 0m,
                                   TotalAmountInDefaultCurrency = dr["DisplayCurrencyRate"] != DBNull.Value && dr["TotalAmount"] != DBNull.Value
                                ? CurrencyHelper.CalculateDefaultCurrencyAmount(Convert.ToDecimal(dr["TotalAmount"]), Convert.ToDecimal(dr["DisplayCurrencyRate"]))
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
                                       Id = Convert.ToInt64(dr["Id"]),
                                       FirstName = Convert.ToString(dr["FirstName"]),
                                       LastName = Convert.ToString(dr["LastName"]),
                                       Phone = Convert.ToString(dr["Phone"]),
                                       Email = Convert.ToString(dr["Email"]),
                                       Country = Convert.ToString(dr["Country"]),
                                       CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                       OrganizationId = dr["OrganizationId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["OrganizationId"]),
                                       CustomerOrganizationType = Convert.ToString(dr["OrganizationTypeId"]),
                                       CustomerOrganizationName = Convert.ToString(dr["OrganizationName"]),
                                       Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                       RoleId = Convert.ToInt32(dr["RoleId"]),
                                       Role = Convert.ToString(dr["Role"])
                                   },
                                   AccommodationInfo = accomodation.Where(e => e.OrderId == Convert.ToInt64(dr["Id"])).ToList(),
                                   IsAccommodationEnabled = dr["IsAccommodationEnabled"] != DBNull.Value ? Convert.ToBoolean(dr["IsAccommodationEnabled"]) : false,
                                   IsVisaEnabled = dr["IsVisaEnabled"] != DBNull.Value ? Convert.ToBoolean(dr["IsVisaEnabled"]) : false,
                                   OrganizationLogo = Convert.ToString(dr["OrganizationLogo"]),
                                   EventName = Convert.ToString(dr["EventName"]),
                                   VisaFees = dr["VisaFees"] != DBNull.Value ? Convert.ToDecimal(dr["VisaFees"]) : 0m,
                                   PenaltyAmount = dr["PenaltyAmount"] != DBNull.Value ? Convert.ToDecimal(dr["PenaltyAmount"]) : 0m
                               }).FirstOrDefault();

                    booking.UnpaidAmount = booking.TotalAmountInDisplayCurrency - booking.PaidAmount;

                    Assertions.IsNotNull(booking, Resources.Resource.DATABASE_ERROR_1028);
                }

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

        public async Task<string> SendAccountInformation(long bookingId, long? organizationId, long? userId)
        {
            try
            {
                // Step 1: Retrieve booking details using GetBookingById
                var booking = await SendMailToCustomer(bookingId);
                if (booking == null)
                {
                    throw new Exception("Booking not found.");
                }


                // Step 2: Load the HTML template
                var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "event-register.html");
                var html = await System.IO.File.ReadAllTextAsync(templatePath);

                var totalRegistrationFees = booking.GuestDetails.Sum(g => g.RegistrationFee);
                var totalAccommodationFees = booking.AccommodationInfo.Sum(a => a.Amount);
                var totalVisaFees = booking.GuestDetails
                                         .Where(g => g.VisaAssistanceRequired)
                                         .Count() * booking.VisaFees;
                var numberOfVisas = booking.GuestDetails.Count(g => g.VisaAssistanceRequired);

                // Step 3: Insert dynamic data from GetBookingById into the template
                var emailBody = new StringBuilder(html)
                    .Replace("{{FullName}}", $"{booking.User.FirstName}       {booking.User.LastName}")
                    .Replace("{{Email}}", booking.User.Email)
                    .Replace("{{Phone}}", booking.User.Phone)
                    .Replace("{{Country}}", booking.User.Country)
                    .Replace("{{OrganizationName}}", booking.OrganizationName)
                    .Replace("{{OrganizationLogo}}", booking.OrganizationLogo)
                    .Replace("{{EventName}}", booking.EventName)
                    .Replace("{{GrandTotalBHD}}", Math.Round(booking.TotalAmountInDefaultCurrency, 3).ToString())
                    .Replace("{{GrandTotalUSD}}", booking.TotalAmountInDisplayCurrency.ToString())
                    .Replace("{{TotalRegistrationFees}}", totalRegistrationFees.ToString())
                    .Replace("{{TotalAccommodationFees}}", totalAccommodationFees.ToString())
                    .Replace("{{TotalVisaFees}}", totalVisaFees.ToString())
                    .Replace("{{NumberOfGuests}}", booking.GuestDetails.Count.ToString())
                    .Replace("{{NumberOfVisas}}", numberOfVisas.ToString())
                    .Replace("{{PaidAmount}}", booking.PaidAmount.ToString("0.00"))
                    .Replace("{{PenaltyAmount}}", booking.PenaltyAmount.ToString("0.00"));

                // Step 4: Add guest registration fees to the email body

                var feesHtml = new StringBuilder("<div style='background-color: #f8f8ff; border-radius: 8px; padding: 15px; margin-top: 20px;'>");
                feesHtml.Append("<h3 style='color: #333; margin-top: 0;'>Guests Registration Fees</h3>");
                feesHtml.Append("<table style='width: 100%; border-collapse: collapse;'>");
                feesHtml.Append("<tr style='border-bottom: 1px solid #e0e0e0;'><th style='text-align: left; padding: 10px; color: #666;'>Guest</th><th style='text-align: left; padding: 10px; color: #666;'>Passport Number</th><th style='text-align: left; padding: 10px; color: #666;'>Role</th><th style='text-align: right; padding: 10px; color: #666;'>Fees</th></tr>");

                foreach (var guest in booking.GuestDetails)
                {
                    feesHtml.Append($"<tr><td style='padding: 10px;'>{guest.PassportFirstName} {guest.PassportLastName}</td><td style='padding: 10px;'>{guest.PassportNumber}</td><td style='padding: 10px;'>{guest.Role}</td><td style='padding: 10px; text-align: right;'>{guest.RegistrationFee} USD</td></tr>");
                }
                feesHtml.Append($"</table><p style='text-align: right; margin-top: 10px; font-weight: bold;'>Total Registration Fees: {totalRegistrationFees} USD</p><div>");


                emailBody.Replace("{{GuestRegistrationFees}}", feesHtml.ToString());

                var accommodationHtml = new StringBuilder("<div style='background-color: #f8f8ff; border-radius: 8px; padding: 15px; margin-top: 20px;'>");
                if (booking.AccommodationInfo.Any()) // Check if there's any accommodation info
                {
                    accommodationHtml.Append("<h3 style='color: #333; margin-top: 0;'>Accommodation Preferences</h3>");
                    accommodationHtml.Append("<table style='width: 100%; border-collapse: collapse;'>");
                    accommodationHtml.Append("<tr style='border-bottom: 1px solid #e0e0e0;'><th style='text-align: left; padding: 10px; color: #666;'>Guests Assigned</th><th style='text-align: left; padding: 10px; color: #666;'>Hotel Name</th><th style='text-align: left; padding: 10px; color: #666;'>From</th><th style='text-align: left; padding: 10px; color: #666;'>To</th><th style='text-align: right; padding: 10px; color: #666;'>Nightly Price</th><th style='text-align: center; padding: 10px; color: #666;'>No. of Nights</th><th style='text-align: right; padding: 10px; color: #666;'>Accommodation</th></tr>");

                    var amount = "";
                    foreach (var accommodation in booking.AccommodationInfo)
                    {
                        amount = accommodation.Amount.ToString();

                        var timeZoneId = booking.GuestDetails.Where(g => !string.IsNullOrEmpty(g.TimeZone)).Select(g => g.TimeZone).FirstOrDefault();

                        // Check if FromDate and ToDate are strings, and parse them if so
                        DateTimeOffset fromDate = DateTimeOffset.Parse(accommodation.FromDate);
                        DateTimeOffset toDate = DateTimeOffset.Parse(accommodation.ToDate);

                        TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

                        DateTimeOffset fromDateInTimeZone = TimeZoneInfo.ConvertTime(fromDate, timeZoneInfo);
                        DateTimeOffset toDateInTimeZone = TimeZoneInfo.ConvertTime(toDate, timeZoneInfo);

                        // Format the DateTimeOffset to remove time and just get the date
                        string FromDate = fromDateInTimeZone.Date.ToString("yyyy-MM-dd");
                        string ToDate = toDateInTimeZone.Date.ToString("yyyy-MM-dd");


                        accommodationHtml.Append($"<tr><td style='padding: 10px;'>{string.Join(", ", accommodation.GuestNames)}</td><td style='padding: 10px;'>{accommodation.Hotel.Name}</td><td style='padding: 10px;'>{FromDate}</td><td style='padding: 10px;'>{ToDate}</td><td style='padding: 10px; text-align: right;'>{accommodation.Hotel.RoomType.NightPrice}</td><td style='padding: 10px; text-align: center;'>{accommodation.NumberOfNights}</td><td style='padding: 10px; text-align: right;'>{accommodation.Amount}</td></tr>");
                    }
                    accommodationHtml.Append($"</table>" +
                        $"<p style = 'text-align: right; margin-top: 10px; font-weight: bold;'> Total accommodation: {totalAccommodationFees} USD" +
                        $"</p></div>");

                    emailBody.Replace("{{accommodationpreferences}}", accommodationHtml.ToString());
                }
                else
                {
                    emailBody.Replace("{{accommodationpreferences}}", string.Empty);
                }

                var visaHtml = new StringBuilder("<div style='margin-bottom: 20px;'>");

                // Filter guests requiring visa assistance
                var guestsRequiringVisa = booking.GuestDetails.Where(g => g.VisaAssistanceRequired).ToList();

                // Calculate total visa fees for guests requiring visa assistance
                var totalVisaFeesForGuests = guestsRequiringVisa.Any()
                                    ? guestsRequiringVisa.Count * booking.VisaFees
                                    : 0;

                if (guestsRequiringVisa.Any()) // Check if there are any guests requiring visa assistance
                {
                    visaHtml.Append("<h3 style='color: #333;'>Guests Requiring Visa Assistance</h3>");
                    visaHtml.Append("<table style='width: 100%;'>");

                    foreach (var visa in guestsRequiringVisa)
                    {
                        visaHtml.Append("<tr>");
                        visaHtml.Append("<td style='padding: 5px;'>");

                        // Assume 'visa.IsChecked' indicates if the checkbox should be checked
                        visaHtml.Append($"<input type='checkbox' checked {(visa.VisaAssistanceRequired ? "checked" : "")}  style='margin-right: 10px;'>");

                        visaHtml.Append($"<span>{visa.PassportFirstName} {visa.PassportLastName} - {visa.Role}</span>");
                        visaHtml.Append("</td>");
                        visaHtml.Append("</tr>");
                    }

                    visaHtml.Append("</table></div>");

                    // Replacing the placeholder in the email body
                    emailBody.Replace("{{VisaAssistance}}", visaHtml.ToString());

                    emailBody.Replace("{{TotalVisaFees}}", totalVisaFeesForGuests.ToString());

                }
                else
                {
                    emailBody.Replace("{{VisaAssistance}}", string.Empty);
                    emailBody = emailBody.Replace(totalVisaFees.ToString(), "0");
                }


                var visaLetterHtml = new StringBuilder("<div style='margin-bottom: 20px;'>");

                // Filter guests who require an official visa letter
                var guestsRequiringVisaLetter = booking.GuestDetails.Where(g => g.VisaOfficialLetterRequired).ToList();

                if (guestsRequiringVisaLetter.Any()) // Check if there are any guests requiring a visa letter
                {
                    visaLetterHtml.Append("<h3 style='color: #333;'>Guests who require an official visa letter</h3>");
                    visaLetterHtml.Append("<table style='width: 100%;'>");

                    foreach (var visaLetter in guestsRequiringVisaLetter)
                    {
                        visaLetterHtml.Append("<tr>");
                        visaLetterHtml.Append("<td style='padding: 5px;'>");

                        // Assuming there's a property that indicates if the checkbox should be checked
                        visaLetterHtml.Append($"<input type='checkbox' checked {(visaLetter.VisaOfficialLetterRequired ? "checked" : "")}  style='margin-right: 10px;'>");

                        visaLetterHtml.Append($"<span>{visaLetter.PassportFirstName} {visaLetter.PassportLastName} - {visaLetter.Role}</span>");
                        visaLetterHtml.Append("</td>");
                        visaLetterHtml.Append("</tr>");
                    }

                    visaLetterHtml.Append("</table></div>");

                    // Replacing the placeholder in the email body
                    emailBody.Replace("{{VisaLetter}}", visaLetterHtml.ToString());
                }
                else
                {
                    // If no guests require a visa letter, replace with an empty string or remove the placeholder
                    emailBody.Replace("{{VisaLetter}}", string.Empty);
                }


                /*var yourEmail = "yagyajoshi.synsoft@gmail.com";*/ // Replace with your email address
                string emailBodyFormatted = emailBody.ToString().Replace("\r\n", " ");

                // Now send the email
                 _emailService.SendEmail(booking.User.Email, "Event Booking Confirmation", emailBodyFormatted);

                // Step 6: Fetch FCM tokens for organization admins
                if (organizationId.HasValue)
                {
                    // Fetch the FCM token for the specified organization
                    var user = await GetFcmToken(organizationId.Value, userId.Value); // Assuming this returns a user model or similar

                    // Check if user or token is not null
                    if (user != null && !string.IsNullOrEmpty(user.FcmToken)) // Assuming 'FcmToken' is the property holding the token
                    {
                        // Create a string array with the token
                        string[] fcmTokensArray = new string[1];
                        fcmTokensArray[0] = user.FcmToken; // Assign the FCM token to the array

                        // Step 7: Send push notifications
                        string title = $"New Booking #{bookingId}";
                        string message = $"User: {booking.User.FirstName} {booking.User.LastName} - #{userId}\nEvent: {booking.EventName}.";

                        var rs = await _firebaseServices.SendFirebaseNotification(fcmTokensArray, title, message, true);

                        return rs;

                    }
                }
                else
                {
                    throw new Exception("Organization ID is not provided.");
                }

                //return emailBody.ToString();
                return null;
                // (You can send this email using the SMTP client, as shown in the previous example)
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }


        public async Task<NotificationDetails> GetFcmToken(long organizationId, long userId)
       {

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetFCMTokensByOrganizationId");
            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@UserId", userId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var user = (from DataRow dr in dt.Rows
                            select new NotificationDetails
                            {
                                FcmToken = dr["FcmToken"].ToString(),
                                EventName = dr["EventName"].ToString(),
                                Name = dr["Name"].ToString()
                            }).FirstOrDefault();

                return user;
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

        public async Task<NotificationDetails> GetFcmTokenForCustomer(long organizationId, long userId)
        {

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetFCMTokensForCustomer");
            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@UserId", userId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var user = (from DataRow dr in dt.Rows
                            select new NotificationDetails
                            {
                                FcmToken = dr["FcmToken"].ToString(),
                                OrganizationName = dr["OrgName"].ToString(),
                                Name = dr["Name"].ToString(),
                                Email = dr["Email"].ToString(),
                                Logo = dr["Logo"].ToString()
                            }).FirstOrDefault();

                return user;
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

        public async Task<string> UpdatePaymentDetails(long bookingId, long? organizationId, long? userId)
        {
            try
            {
                // Step 1: Retrieve booking details using GetBookingById
                var booking = await SendMailToCustomer(bookingId);
                if (booking == null)
                {
                    throw new Exception("Booking not found.");
                }


                // Step 2: Load the HTML template
                var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "updatebooking-invoice.html");
                var html = await System.IO.File.ReadAllTextAsync(templatePath);

                var totalRegistrationFees = booking.GuestDetails.Sum(g => g.RegistrationFee);
                var totalAccommodationFees = booking.AccommodationInfo.Sum(a => a.Amount);
                var totalVisaFees = booking.GuestDetails
                                         .Where(g => g.VisaAssistanceRequired)
                                         .Count() * booking.VisaFees;
                var numberOfVisas = booking.GuestDetails.Count(g => g.VisaAssistanceRequired);

                // Step 3: Insert dynamic data from GetBookingById into the template
                var emailBody = new StringBuilder(html)
                    .Replace("{{FullName}}", $"{booking.User.FirstName}       {booking.User.LastName}")
                    .Replace("{{Email}}", booking.User.Email)
                    .Replace("{{Phone}}", booking.User.Phone)
                    .Replace("{{Country}}", booking.User.Country)
                    .Replace("{{OrganizationName}}", booking.OrganizationName)
                    .Replace("{{OrganizationLogo}}", booking.OrganizationLogo)
                    .Replace("{{EventName}}", booking.EventName)
                    .Replace("{{GrandTotalBHD}}", Math.Round(booking.TotalAmountInDefaultCurrency, 3).ToString())
                    .Replace("{{GrandTotalUSD}}", booking.TotalAmountInDisplayCurrency.ToString())
                    .Replace("{{TotalRegistrationFees}}", totalRegistrationFees.ToString())
                    .Replace("{{TotalAccommodationFees}}", totalAccommodationFees.ToString())
                    .Replace("{{TotalVisaFees}}", totalVisaFees.ToString())
                    .Replace("{{NumberOfGuests}}", booking.GuestDetails.Count.ToString())
                    .Replace("{{NumberOfVisas}}", numberOfVisas.ToString())
                    .Replace("{{PaidAmount}}", booking.PaidAmount.ToString("0.00"))
                    .Replace("{{PenaltyAmount}}", booking.PenaltyAmount.ToString("0.00"))
                    .Replace("{{UnpaidAmount}}", booking.UnpaidAmount.ToString("0.00"));

               
                /*var yourEmail = "yagyajoshi.synsoft@gmail.com";*/ // Replace with your email address
                string emailBodyFormatted = emailBody.ToString().Replace("\r\n", " ");

                // Now send the email
                 _emailService.SendEmail(booking.User.Email, "Revised Payment Details", emailBodyFormatted);

                // Step 6: Fetch FCM tokens for organization admins
                if (organizationId.HasValue)
                {
                    // Fetch the FCM token for the specified organization
                    var user = await GetFcmTokenForOrganization(organizationId.Value, userId.Value); // Assuming this returns a user model or similar

                    // Check if user or token is not null
                    if (user != null && !string.IsNullOrEmpty(user.FcmToken)) // Assuming 'FcmToken' is the property holding the token
                    {
                        // Create a string array with the token
                        string[] fcmTokensArray = new string[1];
                        fcmTokensArray[0] = user.FcmToken; // Assign the FCM token to the array


                        // Step 7: Send push notifications
                        string title = $"Booking Payment Updated #{bookingId}";
                        string message = $"User: {booking.User.FirstName} {booking.User.LastName} - #{userId}\nEvent: {booking.EventName}.";

                        // Custom data payload for detailed notification when clicked
                        //var dataPayload = new Dictionary<string, string>
                        //    {
                        //        { "click_action", "FLUTTER_NOTIFICATION_CLICK" },  // Handles click event
                        //        { "full_title", title },  // Full title when clicked
                        //        { "full_message", message } // Full message when clicked
                        //    };

                        var rs = await _firebaseServices.SendFirebaseNotification(fcmTokensArray, title, "", true);

                        return rs;

                    }
                }
                else
                {
                    throw new Exception("Organization ID is not provided.");
                }

                //return emailBody.ToString();
                return null;
                // (You can send this email using the SMTP client, as shown in the previous example)
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<NotificationDetails> GetFcmTokenForOrganization(long organizationId, long userId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetFCMTokensByOrganizationId1");
            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@UserId", userId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var user = (from DataRow dr in dt.Rows
                            select new NotificationDetails
                            {
                                FcmToken = dr["FcmToken"].ToString(),
                                EventName = dr["EventName"].ToString(),
                                Name = dr["Name"].ToString()
                            }).FirstOrDefault();

                return user;
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
