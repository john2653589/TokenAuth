namespace Rugal.TokenAuth.Common.Model
{
    public partial class AuthResultModel : ResultModel
    {
        public LoginStatusType LoginStatusTypeId { get; set; }
        public static new AuthResultModel Success(dynamic _Result = null)
        {
            var Ret = new AuthResultModel()
            {
                ErrorMessage = null,
                IsSuccess = true,
                Result = _Result,
            };
            return Ret;
        }
        public static AuthResultModel Error(string _ErrorMessage, LoginStatusType StatusType = LoginStatusType.Other)
        {
            var Ret = new AuthResultModel()
            {
                ErrorMessage = _ErrorMessage,
                IsSuccess = false,
                Result = null,
                LoginStatusTypeId = StatusType,
            };
            return Ret;
        }
        public static new AuthResultModel Error(IEnumerable<string> _ErrorMessage, string Separator = "，")
        {
            var Ret = Error(string.Join(Separator, _ErrorMessage));
            return Ret;
        }
        public AuthResultModel SetLoginStatus(LoginStatusType _LoginStatusTypeId)
        {
            LoginStatusTypeId = _LoginStatusTypeId;
            return this;
        }
    }
    public enum LoginStatusType
    {
        Success = 1,
        NoUser = 2,
        AccountLock = 3,
        AuthErorr = 4,
        AdServerError = 5,
        AuthErorrAndLock = 6,
        Other = 99,
    }
}
