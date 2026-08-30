
using System.ComponentModel.DataAnnotations;


namespace Trustesse.Ivoluntia.Commons.Models.Request
{
    public class ForgotPasswordDto
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Token { get; set; }   

        public ForgotPasswordDto Validate()
        {
            if (this == null)
                throw new Exception("invalid request");
            if (this.NewPassword != this.ConfirmPassword)
                throw new Exception("new password and confirm password must be the same");
            return this;
        }
    }
}
