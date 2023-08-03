using System.Text.Json.Serialization;

namespace Rugal.TokenAuth.WebApi.Model
{
    public partial class BaseSignInModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        [JsonIgnore]
        public string Ignore { get; set; }
    }

    public partial class BaseRegisterModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public partial class BaseResetPasswordModel
    {
        public ResetByType ResetBy { get; set; } = ResetByType.UserId;
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string RePassword { get; set; }
        public string Token { get; set; }
        public List<string> Roles { get; set; }
        public bool IsIgnoreRequire { get; set; }
        public bool IsIgnoreToken { get; set; }
        public bool IsResetUserName { get; set; }
        public bool IsLastHashSameCheck { get; set; }
        public int IsLastHashSameCount { get; set; } = 3;
    }

    public enum ResetByType
    {
        None,
        UserId,
        UserName,
    }
    public enum PasswordErrorLockType
    {
        None,
        Lock,
        LockUntilTime,
    }
}