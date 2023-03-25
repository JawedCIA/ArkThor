using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArkThor.Models
{
    public class FileModel
    {
       
        public string FileName { get; set; }
        public string HashValue { get; set; }
        public string UploadedBy { get; set; }
        public DateTime? UploadedDate { get; set; }
        public string Extension { get; set; }       
        public string ContentType { get; set; }
        public string Status { get; set; }
        public Int64 Size { get; set; }
        public int Isold { get; set; }
        
    }
}
