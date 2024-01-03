using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ArkThor.UI.Utilities
{
    public class UploaderServiceName
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UploaderServiceName(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUploaderName()
        {
            string cookieName = "UploaderId";
            string uploaderId = _httpContextAccessor.HttpContext.Request.Cookies[cookieName];

            if (string.IsNullOrEmpty(uploaderId))
            {
                uploaderId = GenerateUploaderId();
                _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName, uploaderId);
            }

            return uploaderId;
        }

        private string GenerateUploaderId()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd"); ;// DateTime.Now.ToString("yyyyMMddHHmmss");
            string random = new Random().Next(1000, 9999).ToString();
            string randomPart = GenerateRandomString(4); // Generate a random string with 4 characters
            return  randomPart + random + timestamp ;
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length)
                                                  .Select(s => s[random.Next(s.Length)])
                                                  .ToArray());
            return randomString;
        }

    }
}
