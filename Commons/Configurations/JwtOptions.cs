using System;

namespace Trustesse.Ivoluntia.Commons.Configurations;

public class JwtOptions
{
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}
