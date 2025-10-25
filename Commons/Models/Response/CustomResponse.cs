using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.Models.Response
{
    public class CustomResponse
    {
        public CustomResponse() { }
        public CustomResponse(int code, string message, object? data = null, object? pagedata = null)
        {
            ResponseMessage = message;
            ResponseCode = code;
            Data = data is not null ? data : default;
            PaginationData = pagedata is not null ? pagedata : default;
        }
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; } 
        public object? Data { get; set; }

        public object PaginationData { get; set; }
    }
}
