using Rugal.TokenAuth.Common.Service;

namespace Rugal.TokenAuth.Common.Model
{
    public class TokenAuthSetting
    {
        public List<Func<string, IServiceProvider, bool>> BlackTokenCheckList { get; set; }
        public TokenAuthSetting()
        {
            BlackTokenCheckList = new List<Func<string, IServiceProvider, bool>> { };
        }
        public void AddBlackTokenCheck(Func<string, IServiceProvider, bool> BlackTokenCheck)
        {
            BlackTokenCheckList.Add(BlackTokenCheck);
        }
    }
}