using System.Net;

namespace EventManagement.DataAccess.ViewModels
{
    public class Response
    {
        public HttpStatusCode HttpCode { get; set; }
        public string Message { get; set; }
    }

    public class Response<T> : Response
    {
        public T Data { get; set; }
       
    }
}
