using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Commons.Extensions.Helpers
{
    public class EmailModelBuilder
    {
        public static EmailModel EmailModelObjectBuilder(List<string> receivers, string subject, string message)
        {
            var emailModel = new EmailModel
            {
                Receivers = receivers,
                Subject = subject,
                Message = message
            };
            return emailModel;  
        }
    }
}
