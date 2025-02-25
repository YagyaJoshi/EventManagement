using EventManagement.BusinessLogic.Resources;
using EventManagement.Utilities.Email;
using EventManagement.Utilities.Storage.AlibabaCloud;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + EventManagement.BusinessLogic.Version.Value + "/[controller]")]
    public class AttachmentController : BaseController
    {
        private readonly IStorageServices _storageServices;

        public AttachmentController(IStorageServices storageServices, IEmailService emailService) : base(emailService)
        {
            _storageServices = storageServices;
        }

        [Route("uploadFile")]
        [HttpPost]
        public async Task<IActionResult> UploadFile(List<IFormFile> files, string folderName)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            var image = await RetrieveFiles(files);
            if (image == null)
                return BadRequest(null);

            return await ExecuteAsync(() =>  _storageServices.UploadFiles(image, folderName, organizationId), Resource.SUCCESS);
        }
    }
}
