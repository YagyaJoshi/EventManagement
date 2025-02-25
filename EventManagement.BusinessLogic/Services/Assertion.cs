using System.Data;
using EventManagement.BusinessLogic.Exceptions;

namespace EventManagement.BusinessLogic.Services
{
    public static class Assertions
    {
        public static void IsNotNull(object @object, string errorText = "")
        {
            Requires(@object != null, errorText);
        }

        public static void Requires(bool condition, string errorText = null)
        {

            if (!condition && string.IsNullOrEmpty(errorText))
                throw new BadRequestException();
            if (!condition)
                throw new ServiceException(errorText);
        }
    }
}
