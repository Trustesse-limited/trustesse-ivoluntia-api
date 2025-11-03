using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.Models.Request
{
    public class ResetPasswordModel
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordModel
    {
        [EmailAddress]
        [Required (ErrorMessage = "Email is Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = " Old Password is Required")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "New Password is Required")]
        public string NewPassword { get; set; }
    }
}
