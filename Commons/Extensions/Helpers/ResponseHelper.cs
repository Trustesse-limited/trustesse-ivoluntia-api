using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Commons.Extensions.Helpers
{
    public class ResponseHelper
    {
        public static GlobalRequestReponse<T> BuildResponse<T>(string message, int reponseCode, T data, bool requestStatus = false
                                                       , ModelStateDictionary errs = null)
        {
            var listOfErrorItems = new List<ErrorItem>();

            if (errs != null)
            {
                foreach (var err in errs)
                {
                    var key = err.Key;
                    var errValues = err.Value;
                    var errList = new List<string>();
                    foreach (var errItem in errValues.Errors)
                    {
                        errList.Add(errItem.ErrorMessage);
                        listOfErrorItems.Add(new ErrorItem { Key = key, ErrorMessages = errList });
                    }
                }
            }

            var res = new GlobalRequestReponse<T>
            {
                isSuccessfull = requestStatus,
                Message = message,
                Data = data,
                ResponseCode = reponseCode,
                Errors = listOfErrorItems
            };
            return res;
        }
    }
}
