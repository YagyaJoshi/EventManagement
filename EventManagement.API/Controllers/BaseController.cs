using EventManagement.Utilities.Email;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventManagement.API.Controllers
{
    public class BaseController : ControllerBase
    {
        private readonly IEmailService _emailService;
        public BaseController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        ///     Execute a method to return something async in the custom pipeline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task">action to execute</param>
        /// <returns></returns>
        protected async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> task, string sucessMessage = null)
       {
            var controllerDispacher = new ControllerDispacher(_emailService);
            var response = await controllerDispacher.DispatchResponse(task, sucessMessage);

            // determine http code result
            switch (response.HttpCode)
            {
                case HttpStatusCode.Unauthorized:
                    return Unauthorized();

                case HttpStatusCode.Forbidden:
                    return StatusCode((int)HttpStatusCode.Forbidden, response);

                case HttpStatusCode.InternalServerError:
                    return StatusCode((int)HttpStatusCode.InternalServerError, response);

                case HttpStatusCode.BadRequest:
                    return BadRequest(response);

                case HttpStatusCode.NotFound:
                    return NotFound(response);

                default:
                    return Ok(response);
            }
        }

        protected async Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> task)
        {
            return await ControllerDispacher.DispatchResponse(task);
        }

        /// <summary>
        ///     Execute a method to do something async in the custom pipeline
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task">action to execute</param>
        /// <returns></returns>
        protected async Task<IActionResult> ExecuteAsync(Func<Task> task, string sucessMessage = null)
        {
            return await ExecuteAsync(async () =>
            {
                await task.Invoke();
                return true;
            }, sucessMessage);
        }

        /// <summary>
        ///     Execute a result in the custom pipeline
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected IActionResult Execute<T>(Func<T> task, string sucessMessage = null)
        {
            var controllerDispacher = new ControllerDispacher(_emailService);

            var response =
                controllerDispacher.DispatchResponse(() => Task.Factory.StartNew(task.Invoke), sucessMessage);

            return Ok(response);
        }

        /// <summary>
        ///     Execute a result in the custom pipeline
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected IActionResult Execute(Action action)
        {
            return Execute(() =>
            {
                action.Invoke();
                return true;
            });
        }

        /// <summary>
        ///     Get an image from the file form data from multi part
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        protected async Task<List<string>> RetrieveFiles(List<IFormFile> files)
        {
            if (!files.Any())
                return new List<string>();

            var paths = new List<string>();

            foreach (var file in files)
            {
                var path = Path.GetTempPath();
                var fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.UtcNow:hhmmss}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(path, fileName);

                if (file.Length < 0)
                    return null;

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                paths.Add(filePath);
            }

            return paths;
        }
    }
}
