using EventManagement.BusinessLogic.Exceptions;
using EventManagement.BusinessLogic.Helpers;
using EventManagement.DataAccess.ViewModels;
using EventManagement.Utilities.Email;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace EventManagement.API.Controllers
{
    public class ControllerDispacher
    {
        public const string ErrorHttpResult = "error";
        public const string SuccessHttpResult = "success";
        public const string FatalErrorHttpResult = "fatal-error";

        private const string UnexpectedErrorMessage = "Sorry! An unexpected error occurred - we're looking into it";
        private const string ForbiddenErrorMessage = "Sorry! You don't have permission to perform this action";

        private readonly IEmailService _emailService;
        public ControllerDispacher(IEmailService emailService)
        {
            _emailService = emailService;
        }
        /// <summary>
        ///     Dispach response async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="loggerServices"></param>
        /// <returns></returns>
        public async Task<Response<T>> DispatchResponse<T>(Func<Task<T>> method, string successMessage = null)
        {
            Response<T> response;

            try
            {
                // Capture the result by invoking the provided method
                var result = await method.Invoke();

                response = new Response<T>
                {
                    Data = result,
                    HttpCode = HttpStatusCode.OK,
                    Message = successMessage
                };
            }
            catch (BadRequestException)
            {
                response = new Response<T>
                {
                    HttpCode = HttpStatusCode.BadRequest,
                    Message = UnexpectedErrorMessage,
                };
            }
            catch (UnauthorizedAccessException exception)
            {
                response = new Response<T>
                {
                    Message = exception.Message,
                    HttpCode = HttpStatusCode.Unauthorized
                };
            }
            catch (ServiceException exception)
            {
                response = new Response<T>
                {
                    Message = exception.Message,
                    HttpCode = HttpStatusCode.BadRequest
                };
            }
            catch (Exception e)
            {
                var stackTraceLines = e.StackTrace?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                var methodInfo = stackTraceLines?.ElementAtOrDefault(1) ?? "";
                string methodName = methodInfo.Contains("at") ? methodInfo.Split('(')[0].Split('.').Last().Trim() : "Unknown Method";
                string lineNumber = methodInfo.Contains(":line") ? methodInfo.Split(":line ").Last().Trim() : "Unknown Line";

                var httpContext = HttpContextHelper.Current;
                string httpMethod = httpContext?.Request.Method ?? "Unknown";
                string httpUrl = httpContext != null ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}" : "Unknown";
                string queryParameters = httpContext?.Request.Query.Any() == true
                    ? string.Join("&", httpContext.Request.Query.Select(q => $"{q.Key}={q.Value}"))
                    : "No query parameters";

                response = new Response<T>
                {
                    Message = e.Message,
                    HttpCode = HttpStatusCode.InternalServerError
                };

                var html = $"Error Message: {e.Message}\nLocation: {methodName}, Line: {lineNumber}\n\nHTTP Method: {httpMethod}\nHTTP URL: {httpUrl}\nQuery Parameters: {queryParameters}\n StackTrace:  {e.StackTrace}";
                // Log the error or send an email with the details
                _emailService.SendEmail("yagyajoshi.synsoft@gmail.com","Event Management - Error Occurred", html);
                _emailService.SendEmail("mahimakhore.synsoft@gmail.com", "Event Management - Error Occurred", html);
            }


            return response;
        }





        public static async Task<HttpResponseMessage> DispatchResponse(Func<Task<HttpResponseMessage>> method)
        {
            try
            {
                var result = await method.Invoke();
                return result;
            }
            catch (Exception e)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
