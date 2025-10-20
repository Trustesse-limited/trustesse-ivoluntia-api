

namespace Trustesse.Ivoluntia.Commons.Models.Request
{
    public class EmailModel
    {
        public List<string> Receivers { get; set; }
        public string Attachments { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; } 
    }
}
