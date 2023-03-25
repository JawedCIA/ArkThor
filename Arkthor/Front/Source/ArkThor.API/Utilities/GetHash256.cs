using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace ArkThor.API.Utilities
{
    public static class GenereteHash256
    {

      
        public static string SHA256file(string fileFullPath)
        {

            var fInfo = new FileInfo(fileFullPath);
            // Initialize a SHA256 hash object.
            using (SHA256 fileSHA256 = SHA256.Create())
            {
                // Compute and print the hash values for each file in directory.
                using (FileStream fileStream = fInfo.Open(FileMode.Open))
                    {
                        try
                        {
                            // Create a fileStream for the file.
                            // Be sure it's positioned to the beginning of the stream.
                            fileStream.Position = 0;
                            // Compute the hash of the fileStream.
                            byte[] hashValue = fileSHA256.ComputeHash(fileStream);
                        // Write the name and hash value of the file to the console.
                        System.Diagnostics.Debug.Write($"{fInfo.Name}: ");
                        string hash256Value= PrintByteArray(hashValue);

                        return hash256Value;
                        }
                        catch (IOException e)
                        {
                        System.Diagnostics.Debug.WriteLine($"I/O Exception: {e.Message}");
                        }
                        catch (UnauthorizedAccessException e)
                        {
                        System.Diagnostics.Debug.WriteLine($"Access Exception: {e.Message}");
                        }
                    return null;
                 }
                
            }

        }

        // Display the byte array in a readable format.
        public static string PrintByteArray(byte[] array)
        {
            string hashvalue=null;
            for (int i = 0; i < array.Length; i++)
            {
                hashvalue = hashvalue + $"{array[i]:X2}";
              //  System.Diagnostics.Debug.Write($"{array[i]:X2}");
                //if ((i % 4) == 3) System.Diagnostics.Debug.Write(" ");
            }
            //System.Diagnostics.Debug.WriteLine("--------------");
            return hashvalue;
        }
    }

}
