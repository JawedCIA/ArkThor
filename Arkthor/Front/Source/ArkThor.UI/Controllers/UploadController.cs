using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArkThor.UI.Controllers
{
    public class UploadController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Files(List<IFormFile> files)
        {
            // Upload each file
            foreach (var file in files)
            {
                try
                {
                    // Read the file content
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        byte[] data = stream.ToArray();

                        // Upload the file to the server
                        // Replace this with your own code to upload the file
                        await UploadFile(file.FileName, data);
                    }
                }
                catch (System.Exception ex)
                {
                    // Handle the upload error here
                    return BadRequest("An error occurred while uploading the file: " + ex.Message);
                }
            }

            // Return a success response
            return Ok("Files uploaded successfully.");
        }

        private async Task UploadFile(string fileName, byte[] data)
        {
            // Replace this with your own code to upload the file
            // For example, you could use a WebClient or HttpWebRequest to upload the file to a server
        }
    }
}
