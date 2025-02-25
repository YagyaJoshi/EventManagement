namespace EventManagement.BusinessLogic.Exceptions
{
    public class ServiceException : Exception
    {
        public ServiceException(string message)
           : base(message)
        {
        }

        public ServiceException()
        {
        }
    }
}
