using System;

namespace Centrifugo.Client.Options
{
    public class AuthOptions
    {
        public string Issuer { get; set; } = "MyAuthServer"; // издатель токена
        public string Audience { get; set; } = "MyAuthClient"; // потребитель токена
        public string SecretKey { get; set; } = "very-long-secret-key"; // ключ для шифрации
        public TimeSpan? TokenLifetime { get; set; } = TimeSpan.FromSeconds(3600); // время жизни токена
    }
}