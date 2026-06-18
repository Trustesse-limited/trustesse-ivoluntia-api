using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.Models.Response
{
    public class GlobalRequestReponse<T>
    {
        public int ResponseCode { get; set; }
        public bool isSuccessfull { get; set; }
        public string Message { get; set; }
        public List<ErrorItem> Errors { get; set; }
        public T Data { get; set; }
        public GlobalRequestReponse()
        {
            Errors = new List<ErrorItem>();
        }
    }
    public class ErrorItem
    {
        public string Message { get; set; }
        public int ErrorCode { get; set; }     
    }
}
