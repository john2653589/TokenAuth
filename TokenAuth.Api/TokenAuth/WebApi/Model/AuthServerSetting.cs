namespace Rugal.TokenAuth.WebApi.Model
{
    public class AuthServerSetting
    {
        public bool RequireNonAlphanumeric { get; set; }
        public bool RequireUppercase { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireDigit { get; set; }
        public int RequiredLength { get; set; }
        public bool RequireConfirmedAccount { get; set; }
        public int AccountLockForPasswordError { get; set; }
        public TimeSpan? PasswordResetTokenLifetime { get; set; }
        public bool IsDoubleEncodeHash { get; set; }
        public DoubleEncodeAlgoType DoubleEncodeAlgo { get; set; }
        public string DoubleEncodeKey { get; set; }
        public string DoubleEncodeIV { get; set; }
    }
    public enum DoubleEncodeAlgoType
    {
        Aes128,
    }
}