namespace Rugal.TokenAuth.Common.Model
{
    public class ResultModel : ResultModel<dynamic>
    {
        public static new ResultModel Success(dynamic _Result = default)
        {
            var Ret = new ResultModel()
            {
                ErrorMessage = null,
                IsSuccess = true,
                Result = _Result,
            };
            return Ret;
        }

        public static new ResultModel Error(string _ErrorMessage)
        {
            var Ret = new ResultModel()
            {
                ErrorMessage = _ErrorMessage,
                IsSuccess = false,
                Result = default,
            };
            return Ret;
        }

        public static new ResultModel Error(IEnumerable<string> _ErrorMessage, string Separator = "，")
        {
            var Ret = Error(string.Join(Separator, _ErrorMessage));
            return Ret;
        }
    }
    public class ResultModel<TResult>
    {
        public string ErrorMessage { get; set; }
        public TResult Result { get; set; }
        public bool IsSuccess { get; set; }

        public static ResultModel<TResult> Success(TResult _Result = default)
        {
            var Ret = new ResultModel<TResult>()
            {
                ErrorMessage = null,
                IsSuccess = true,
                Result = _Result,
            };
            return Ret;
        }
        public static ResultModel<TResult> Error(string _ErrorMessage)
        {
            var Ret = new ResultModel<TResult>()
            {
                ErrorMessage = _ErrorMessage,
                IsSuccess = false,
                Result = default,
            };
            return Ret;
        }
        public static ResultModel<TResult> Error(IEnumerable<string> _ErrorMessage, string Separator = "，")
        {
            var Ret = Error(string.Join(Separator, _ErrorMessage));
            return Ret;
        }
    }
}
