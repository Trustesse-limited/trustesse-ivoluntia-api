using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.Models.Request
{
    public class EmailModel
    {
        public string Receiver { get; set; }
        public string Attachment { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}
