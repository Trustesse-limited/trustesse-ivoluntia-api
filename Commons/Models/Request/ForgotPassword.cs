
using System.ComponentModel.DataAnnotations;


namespace Trustesse.Ivoluntia.Commons.Models.Request
{
    public class ForgotPassword
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }
}
