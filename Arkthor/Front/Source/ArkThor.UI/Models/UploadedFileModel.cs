using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArkThor.Models
{
    public class UploadedFileModel : FileModel
    {
        public byte[] Data { get; set; }
    }
}
