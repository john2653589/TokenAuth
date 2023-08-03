namespace Rugal.TokenAuth.Common.Model
{
    public partial class JwtParamModel
    {
        public string BearerKey { get; set; }
        public string IssuerSigningKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int TokenExpiresHours { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public bool ValidateIssuer { get; set; }
        public bool ValidateAudience { get; set; }
        public bool ValidateLifetime { get; set; }
        public bool RequireExpirationTime { get; set; }
    }
    public enum BaseRoleType
    {
        Admin,
        User,
    }
}