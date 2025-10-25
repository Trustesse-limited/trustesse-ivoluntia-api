using Trustesse.Ivoluntia.Commons.Cryptography;
using Trustesse.Ivoluntia.Commons.DTOs.RSA;
using Microsoft.AspNetCore.Mvc;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            //i want to test my RSAcrypto handler
            var rsa = new RSACryptographyHandler();
            rsa.CreateAES(Guid.NewGuid().ToString("N").Substring(0, 16), Guid.NewGuid().ToString("N").Substring(0, 16));
            var outputEnc = rsa.EncryptData(new EncryptRequest()
            {
                PartnerPublicKeyPath= "C:\\Users\\HP\\source\\Personal\\iVoluntia\\Commons\\RSAKeys\\public.key",
                TextInput = "Hello, this is a test message for RSA encryption!"
            });

            Console.WriteLine($"Encrypted Data: {outputEnc.EncryptedData}");

            var outputDec = rsa.DecryptData(new DecryptRequest()
            {
                EncryptedData = outputEnc.EncryptedData,
                MyPrivateKeyPath= "C:\\Users\\HP\\source\\Personal\\iVoluntia\\Commons\\RSAKeys\\private.key"
            });

            Console.WriteLine($"Encrypted Data: {outputDec.DecryptedData}");
            return Ok("Welcome to iVoluntia API!");
        }
    }
}
