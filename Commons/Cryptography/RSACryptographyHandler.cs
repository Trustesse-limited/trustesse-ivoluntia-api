using System.Security.Cryptography;
using System.Text;
using Trustesse.Ivoluntia.Commons.DTOs.RSA;

namespace Trustesse.Ivoluntia.Commons.Cryptography
{
    public class RSACryptographyHandler
    {
        private RSAParameters PartnerpublicRSAInfo;
        private RSAParameters privateRSAInfo;
        private string? IV { get; set; }
        private string? Key { get; set; } 
        public void CreateAES(string key, string iv)
        {
            Key = key;
            IV = iv;
        }

        public Response EncryptData(EncryptRequest request)
        {
            string acceptedpathformat = ".txt,.key";
            var keyformats = acceptedpathformat.Split(',');
            try
            {
                // Ensure Key and IV are not null
                if (string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(IV))
                {
                    return new Response
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Key and IV must not be null"
                    };
                }

                if (!string.IsNullOrEmpty(request.PartnerPublicKeyPath) && keyformats.Contains(Path.GetExtension(request.PartnerPublicKeyPath).ToLower()))
                {
                    if (!string.IsNullOrEmpty(request.TextInput))
                    {
                        //reaing the file path
                        var PartnerKey = File.ReadAllText(request.PartnerPublicKeyPath);

                        var formatPartnerPublicKey = FormatKey(PartnerKey);
                        if (string.IsNullOrEmpty(formatPartnerPublicKey))
                        {
                            return new Response
                            {
                                ResponseCode = "01",
                                ResponseMessage = "Invalid RSA Format"
                            };
                        }
                        var rsaParameterResponse = LoadRsaPublicKey(formatPartnerPublicKey);
                        if (rsaParameterResponse.ResponseCode == "00")
                        {
                            PartnerpublicRSAInfo = rsaParameterResponse.Parameters;
                            //Decrypt the main text with AES crypto

                            var EncryptText = AES.Encrypt(request.TextInput, Key, IV);
                            if (!string.IsNullOrEmpty(EncryptText))
                            {
                                var encryptedKey = RsaEncryptWithPublicKey(Key, PartnerpublicRSAInfo);
                                if (encryptedKey.temp != null && encryptedKey.ResponseCode == "00")
                                {
                                    var final = Convert.ToHexString(Encoding.UTF8.GetBytes(string.Format("{0}!{1}", EncryptText, encryptedKey.temp)));

                                    return new Response
                                    {
                                        ResponseCode = "00",
                                        ResponseMessage = "Successfull",
                                        EncryptedData = final
                                    };
                                }
                                else
                                {
                                    return new Response
                                    {
                                        ResponseCode = "01",
                                        ResponseMessage = encryptedKey.ResponseMessage
                                    };
                                }
                            }
                            else
                            {
                                return new Response
                                {
                                    ResponseCode = "01",
                                    ResponseMessage = "Error Occurred"
                                };
                            }


                        }
                        else
                        {
                            return new Response
                            {
                                ResponseCode = "01",
                                ResponseMessage = rsaParameterResponse.ResponseMessage
                            };
                        }

                    }
                    else
                    {
                        return new Response
                        {
                            ResponseCode = "01",
                            ResponseMessage = "Text can not be empty"
                        };
                    }

                }
                else
                {
                    return new Response
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Invalid Path Extension"
                    };

                }
            }
            catch (Exception ex)
            {
                return new Response
                {
                    ResponseCode = "01",
                    ResponseMessage = ex.Message
                };
            }
        }
        public Response DecryptData(DecryptRequest decryptRequest)
        {
            string acceptedpathformat = ".txt,.key";
            var keyformats = acceptedpathformat.Split(',');
            try
            {
                // Ensure Key and IV are not null
                if (string.IsNullOrEmpty(IV))
                {
                    return new Response
                    {
                        ResponseCode = "01",
                        ResponseMessage = "IV must not be null"
                    };
                }
                if (!string.IsNullOrEmpty(decryptRequest.MyPrivateKeyPath) && keyformats.Contains(Path.GetExtension(decryptRequest.MyPrivateKeyPath).ToLower()))
                {

                    if (!string.IsNullOrEmpty(decryptRequest.EncryptedData))
                    {
                        //reaing the file path
                        var readmyprivatekey = File.ReadAllText(decryptRequest.MyPrivateKeyPath);

                        var formatMyPrivatekey = ReverseFormatPrivateKey(readmyprivatekey);
                        if (string.IsNullOrEmpty(formatMyPrivatekey))
                        {
                            return new Response
                            {
                                ResponseCode = "01",
                                ResponseMessage = "Invalid RSA Format"
                            };
                        }
                        var parameterresponse = LoadRsaPrivateKey(formatMyPrivatekey);
                        if (parameterresponse != null && parameterresponse.ResponseCode == "00")
                        {
                            privateRSAInfo = parameterresponse.Parameters;

                            var rawFroHex = Encoding.UTF8.GetString(Convert.FromHexString(decryptRequest.EncryptedData));
                            if (string.IsNullOrEmpty(rawFroHex))
                            {
                                return new Response
                                {
                                    ResponseCode = "01",
                                    ResponseMessage = "Encrypted Data Corrupted"
                                };
                            }
                            // Split the raw hex data into payload and random key
                            var cryptoData = rawFroHex.Split("!");
                          
                            var decryptedKey = RsaDecryptWithPrivateKey(cryptoData[1], privateRSAInfo);
                            if (decryptedKey != null && decryptedKey.ResponseCode == "00")
                            {
                                
                                var decrypted = AES.Decrypt(cryptoData[0], decryptedKey.DecryptedData!, IV);

                                return new Response
                                {
                                    ResponseCode = "00",
                                    ResponseMessage = "Successfull",
                                    DecryptedData = decrypted
                                };
                            }
                            else
                            {
                                return new Response
                                {
                                    ResponseCode = "01",
                                    ResponseMessage = decryptedKey!.ResponseMessage
                                };
                            }
                        }
                        else
                        {
                            return new Response
                            {
                                ResponseCode = "01",
                                ResponseMessage = parameterresponse!.ResponseMessage
                            };
                        }
                    }
                    else
                    {
                        return new Response
                        {
                            ResponseCode = "01",
                            ResponseMessage = "EncryptedText can not be empty"
                        };
                    }

                }
                else
                {
                    return new Response
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Invalid Path Extension"
                    };
                }
            }
            catch (Exception ex)
            {
                return new Response
                {
                    ResponseCode = "01",
                    ResponseMessage = ex.Message
                };
            }
        }
        public Response RsaDecryptWithPrivateKey(string base64Input, RSAParameters sAParameters)
        {

            var bytesToDecrypt = Convert.FromBase64String(base64Input);

            try
            {

                byte[] decryptedData;

                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(sAParameters);

                    decryptedData = RSA.Decrypt(bytesToDecrypt, true);
                }
                return new Response()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successfull",
                    DecryptedData = Encoding.UTF8.GetString(decryptedData)
                };

            }

            catch (CryptographicException ex)
            {
                return new Response()
                {
                    ResponseCode = "01",
                    ResponseMessage = ex.Message
                };
            }
        }
        private static Response RsaEncryptWithPublicKey(string clearText, RSAParameters parameters)
        {
            try
            {
                byte[] encryptedData;


                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(parameters);

                    //Encrypt the passed byte array and specify fOAEP padding.
                    encryptedData = RSA.Encrypt(Encoding.UTF8.GetBytes(clearText), true);

                }
                var result = Convert.ToBase64String(encryptedData);
                return new Response()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successfull",
                    temp = result
                };
            }
            catch (Exception ex)
            {
                return new Response()
                {
                    ResponseCode = "01",
                    ResponseMessage = ex.Message
                };

            }


        }
        private static RsaParameterResponse LoadRsaPrivateKey(string privateKeyData)
        {
            try
            {

                using (RSA rsa = RSA.Create())
                {
                    // Import the private key
                    rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKeyData), out _);

                    // Get RSA parameters
                    RSAParameters rsaParams = rsa.ExportParameters(true);


                    return new RsaParameterResponse()
                    {
                        ResponseCode = "00",
                        ResponseMessage = "success",
                        Parameters = new RSAParameters()
                        {
                            Modulus = rsaParams.Modulus,
                            Exponent = rsaParams.Exponent,
                            D = rsaParams.D,
                            DP = rsaParams.DP,
                            DQ = rsaParams.DQ,
                            InverseQ = rsaParams.InverseQ,
                            P = rsaParams.P,
                            Q = rsaParams.Q,
                        }
                    };
                }
            }
            catch (Exception ex)
            {

                return new RsaParameterResponse()
                {
                    ResponseCode = "01",
                    ResponseMessage = ex.Message,
                };
            }


        }
        private static RsaParameterResponse LoadRsaPublicKey(string publicKeyData)
        {
            try
            {
                using (RSA rsa = RSA.Create())
                {
                    // Import the public key
                    rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKeyData), out _);

                    // Get RSA parameters
                    RSAParameters rsaParams = rsa.ExportParameters(false);

                    return new RsaParameterResponse()
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success",
                        Parameters = new RSAParameters()
                        {
                            Modulus = rsaParams.Modulus,
                            Exponent = rsaParams.Exponent
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new RsaParameterResponse()
                {
                    ResponseCode = "01",
                    ResponseMessage = ex.Message,

                };
            }


        }
        private static string FormatKey(string publicKey)
        {
            var header = "-----BEGIN PUBLIC KEY-----";
            var footer = "-----END PUBLIC KEY-----";

            publicKey = publicKey.Replace(header, string.Empty);
            publicKey = publicKey.Replace(footer, string.Empty);
            publicKey = publicKey.Replace(Environment.NewLine, string.Empty);

            return publicKey;

        }
        private static string ReverseFormatPrivateKey(string privateKey)
        {
            var header = "-----BEGIN RSA PRIVATE KEY-----";
            var footer = "-----END RSA PRIVATE KEY-----";
            privateKey = privateKey.Replace(header, string.Empty);
            privateKey = privateKey.Replace(footer, string.Empty);
            privateKey = privateKey.Replace(Environment.NewLine, string.Empty);

            return privateKey;

        }
    }

}
