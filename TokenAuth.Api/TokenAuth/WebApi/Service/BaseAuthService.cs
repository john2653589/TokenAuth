using Microsoft.AspNetCore.Identity;
using Rugal.TokenAuth.Common.Model;
using Rugal.TokenAuth.Common.Service;
using Rugal.TokenAuth.WebApi.Model;
using System.Runtime.CompilerServices;

namespace Rugal.TokenAuth.WebApi.Service
{
    public abstract partial class BaseAuthService
    {
        private List<Func<IdentityUser, AuthResultModel>> SignInCheckFunc { get; set; }
        private List<Action<IdentityUser, BaseSignInModel>> SignInSuccessFunc { get; set; }
        private List<Action<IdentityUser>> ResetPasswordSuccessFunc { get; set; }
        public virtual string Message_NoUser { get; set; } = "查無使用者";
        public virtual string Message_PasswordErorr { get; set; } = "密碼錯誤";
        public virtual string Message_AccountLock { get; set; } = "帳號已鎖定";
        public virtual bool IsAlertLockTimeEnd { get; set; } = true;
        public virtual PasswordErrorLockType PasswordErrorLock { get; set; } = PasswordErrorLockType.LockUntilTime;
        public virtual TimeSpan Time_PasswordLock { get; set; } = new TimeSpan(0, 15, 0);

        internal readonly UserManager<IdentityUser> UserManager;
        internal readonly RoleManager<IdentityRole> RoleManager;
        internal readonly TokenService TokenService;
        internal readonly AuthServerSetting AuthServerSetting;
        protected BaseAuthService(
            UserManager<IdentityUser> _UserManager,
            RoleManager<IdentityRole> _RoleManager,
            TokenService _TokenService,
            AuthServerSetting _AuthServerSetting
            )
        {
            UserManager = _UserManager;
            TokenService = _TokenService;
            RoleManager = _RoleManager;
            AuthServerSetting = _AuthServerSetting;
            SignInCheckFunc = new List<Func<IdentityUser, AuthResultModel>> { };
            SignInSuccessFunc = new List<Action<IdentityUser, BaseSignInModel>> { };
            ResetPasswordSuccessFunc = new List<Action<IdentityUser>> { };
        }

        internal abstract Dictionary<string, object> GetCreateTokenClaims(IdentityUser User, [CallerMemberName] string SignType = null);
        internal virtual string GetRegisterRole(string UserName, [CallerMemberName] string CallerName = null)
        {
            var Ret = CallerName == "base" ? "Admin" : CallerName;
            return Ret;
        }
        public virtual void LoginLog(bool IsSuccess, string UserName, LoginStatusType LoginStatusTypeId) { }
        public virtual async Task<AuthResultModel> SignIn(BaseSignInModel Model, string SignInType = "base")
        {
            var User = await UserManager.FindByNameAsync(Model.UserName);
            if (User is null)
            {
                LoginLog(false, Model.UserName, LoginStatusType.NoUser);
                return AuthResultModel.Error(Message_NoUser, LoginStatusType.NoUser);
            }

            if (!CheckAccountStatus(Model, User, out var Message))
            {
                LoginLog(false, Model.UserName, LoginStatusType.AccountLock);
                return AuthResultModel.Error(Message, LoginStatusType.AccountLock);
            }

            if (!await UserManager.CheckPasswordAsync(User, Model.Password))
            {
                LoginLog(false, Model.UserName, LoginStatusType.PasswordErorr);
                var PasswordErrorResult = AuthResultModel.Error(Message_PasswordErorr, LoginStatusType.PasswordErorr);

                if (PasswordErrorLock == PasswordErrorLockType.None || !AddErrorCount(User))
                    return PasswordErrorResult;

                var MessageList = new List<string>
                {
                    Message_PasswordErorr,
                    Message_AccountLock,
                };

                if (IsAlertLockTimeEnd && PasswordErrorLock == PasswordErrorLockType.LockUntilTime)
                    MessageList.Add(GetLockTimeEnd(User));

                var LockMessage = string.Join("、", MessageList);
                return AuthResultModel.Error(LockMessage, LoginStatusType.PasswordErorrAndLock);
            }

            var SignInCheckResult = OnSignInCheck(User);
            if (!SignInCheckResult.IsSuccess)
                return SignInCheckResult;

            var GetRole = await UserManager.GetRolesAsync(User);
            var ClaimsDic = GetCreateTokenClaims(User, SignInType);
            var Token = TokenService.CreateToken(User, GetRole, ClaimsDic);

            OnSignInSuccess(User, Model);
            LoginLog(true, Model.UserName, LoginStatusType.Success);
            ResetErrorCount(User);

            return AuthResultModel.Success(Token);
        }
        public virtual AuthResultModel OnSignInCheck(IdentityUser User)
        {
            foreach (var CheckFunc in SignInCheckFunc)
            {
                var CheckResult = CheckFunc?.Invoke(User);
                if (!CheckResult.IsSuccess)
                    return CheckResult;
            }
            return AuthResultModel.Success();
        }
        public virtual void OnSignInSuccess(IdentityUser User, BaseSignInModel Model)
        {
            foreach (var SuccessFunc in SignInSuccessFunc)
                SuccessFunc?.Invoke(User, Model);
        }

        public virtual string GetLockTimeEnd(IdentityUser User)
        {
            var LockTime = User.LockoutEnd;
            if (LockTime is null)
                return "";

            var Message = $"鎖定至:{LockTime:yyyy/MM/dd HH:mm:ss}";
            return Message;
        }
        internal virtual bool CheckAccountStatus(BaseSignInModel Model, IdentityUser User, out string Message)
        {
            var LockEndTime = User.LockoutEnd;
            if (LockEndTime is not null && LockEndTime.Value > DateTime.Now)
            {
                var Diff = LockEndTime.Value - DateTime.Now;
                Message = $"帳號已被鎖定，剩餘{(int)Diff.TotalMinutes}分鐘";
                return false;
            }

            Message = null;
            return true;
        }
        public virtual bool LockAccountUntilTime(string UserName, TimeSpan LockTime)
        {
            var User = UserManager.FindByNameAsync(UserName).Result;
            return LockAccountUntilTime(User, LockTime);
        }
        public virtual bool LockAccountUntilTime(IdentityUser User, TimeSpan LockTime)
        {
            var LockEndTime = DateTime.Now.ToLocalTime().Add(LockTime);
            var Reuslt = UserManager.SetLockoutEndDateAsync(User, new DateTimeOffset(LockEndTime)).Result;
            return Reuslt.Succeeded;
        }
        public virtual bool LockAccount(IdentityUser User) => false;

        public virtual bool AddErrorCount(string UserName)
        {
            var User = UserManager.FindByNameAsync(UserName).Result;
            return AddErrorCount(User);
        }
        public virtual bool AddErrorCount(IdentityUser User)
        {
            if (User is null)
                return false;

            _ = UserManager.AccessFailedAsync(User).Result;
            if (User.AccessFailedCount < AuthServerSetting.AccountLockForPasswordError)
                return false;

            ResetErrorCount(User);
            var IsLock = PasswordErrorLock switch
            {
                PasswordErrorLockType.Lock => LockAccount(User),
                PasswordErrorLockType.LockUntilTime => LockAccountUntilTime(User, Time_PasswordLock),
                _ => false
            };
            return IsLock;
        }
        public virtual bool ResetErrorCount(string UserName)
        {
            var User = UserManager.FindByNameAsync(UserName).Result;
            return ResetErrorCount(User);
        }
        public virtual bool ResetErrorCount(IdentityUser User)
        {
            if (User is null)
                return false;

            _ = UserManager.ResetAccessFailedCountAsync(User).Result;
            return true;
        }
        public virtual void WithSignInCheck(Func<IdentityUser, AuthResultModel> CheckFunc)
            => SignInCheckFunc.Add(CheckFunc);
        public virtual void WithSignInSuccess(Action<IdentityUser, BaseSignInModel> CheckFunc)
            => SignInSuccessFunc.Add(CheckFunc);
        public virtual void WithResetPasswordSuccess(Action<IdentityUser> SuccessFunc)
            => ResetPasswordSuccessFunc.Add(SuccessFunc);

        public virtual async Task<AuthResultModel> Register(BaseRegisterModel Model, string RegistType = "base")
        {
            var User = await UserManager.FindByNameAsync(Model.UserName);
            if (User != null)
                return AuthResultModel.Error("帳號已存在");

            User = new IdentityUser()
            {
                UserName = Model.UserName,
            };
            var CreateUser = await UserManager.CreateAsync(User, Model.Password);
            if (!CreateUser.Succeeded)
                return AuthResultModel.Error(CreateUser.Errors.Select(Item => Item.Description));

            var AddRole = GetRegisterRole(User.UserName, RegistType);
            var CreateRole = await UserManager.AddToRoleAsync(User, AddRole);
            if (!CreateRole.Succeeded)
                return AuthResultModel.Error(CreateRole.Errors.Select(Item => Item.Description));

            return AuthResultModel.Success(User.Id);
        }
        public virtual async Task<AuthResultModel> ResetPassword(BaseResetPasswordModel Model)
        {
            if (Model.Password != Model.RePassword)
                return AuthResultModel.Error("密碼與重複密碼不一致");

            var User = Model.ResetBy switch
            {
                ResetByType.UserId => await UserManager.FindByIdAsync(Model.UserId.ToString()),
                ResetByType.UserName => await UserManager.FindByNameAsync(Model.UserName.ToString()),
                _ => null
            };

            if (User is null)
                return AuthResultModel.Error("帳號不存在");

            var ResetToken = Model.Token;
            if (Model.IsIgnoreToken)
                ResetToken = await UserManager.GeneratePasswordResetTokenAsync(User);

            if (string.IsNullOrWhiteSpace(ResetToken))
                return AuthResultModel.Error("ResetToken cannot be null or empty");

            if (Model.IsLastPasswordHashSameCheck)
            {
                var Hasher = UserManager.PasswordHasher;
                var LastPassworHash = GetLastHashRecord(User, Model.IsLastPasswordHashSameCount);
                var IsSameOne = LastPassworHash
                    .Any(Item => Hasher.VerifyHashedPassword(User, Item, Model.Password) == PasswordVerificationResult.Success);
                if (IsSameOne)
                    return AuthResultModel.Error($"密碼不得與前「{Model.IsLastPasswordHashSameCount}」次密碼一樣");
            }

            if (Model.IsIgnoreRequire)
            {
                UserManager.Options.Password.RequireNonAlphanumeric = false;
                UserManager.Options.Password.RequireUppercase = false;
                UserManager.Options.Password.RequireLowercase = false;
                UserManager.Options.Password.RequireDigit = false;
                UserManager.Options.Password.RequiredLength = 3;
            }

            var ResetRet = await UserManager.ResetPasswordAsync(User, ResetToken, Model.Password);
            if (!ResetRet.Succeeded)
                return AuthResultModel.Error(ResetRet.Errors.Select(Item =>
                {
                    var ChtMessage = Item.Code switch
                    {
                        "PasswordRequiresNonAlphanumeric" => "密碼至少要包含一個非字母數字字元",
                        "PasswordTooShort" => $"密碼長度至少要{AuthServerSetting.RequiredLength}碼",
                        "PasswordRequiresLower" => "密碼至少要有一個小寫英文字母 (a-z)",
                        "PasswordRequiresUpper" => "密碼至少要有一個大寫字母 (A-Z)",
                        "PasswordRequiresDigit" => "密碼必須至少包含一個數字(0-9)",
                        _ => Item.Description,
                    };
                    return ChtMessage;
                }));

            ResetErrorCount(User);
            OnResetPasswordSuccess(User);

            if (Model.IsResetUserName && !string.IsNullOrWhiteSpace(Model.UserName))
            {
                ResetRet = await UserManager.SetUserNameAsync(User, Model.UserName);
                if (!ResetRet.Succeeded)
                    return AuthResultModel.Error(ResetRet.Errors.Select(Item => Item.Description));
            }

            if (Model.Roles != null && Model.Roles.Any())
            {
                var GetRoles = await UserManager.GetRolesAsync(User);
                await UserManager.RemoveFromRolesAsync(User, GetRoles);
                await UserManager.AddToRolesAsync(User, Model.Roles);
            }

            return AuthResultModel.Success(User.Id);
        }
        public virtual void OnResetPasswordSuccess(IdentityUser User)
        {
            foreach (var Func in ResetPasswordSuccessFunc)
            {
                Func?.Invoke(User);
            }
        }
        public virtual IEnumerable<string> GetLastHashRecord(IdentityUser User, int TakeCount) => Array.Empty<string>();


        public virtual async Task<AuthResultModel> DeleteUser(string UserId)
        {
            var GetUser = await UserManager.FindByIdAsync(UserId);
            if (GetUser is null)
                return AuthResultModel.Error("無此帳號");

            var DeleteRet = await UserManager.DeleteAsync(GetUser);
            if (!DeleteRet.Succeeded)
                return AuthResultModel.Error(DeleteRet.Errors.Select(Item => Item.Description));

            return AuthResultModel.Success(GetUser.Id);
        }
        public virtual async Task<AuthResultModel> RoleInit(params string[] Roles)
        {
            foreach (var Role in Roles)
            {
                if (!await RoleManager.RoleExistsAsync(Role))
                    await RoleManager.CreateAsync(new IdentityRole(Role));
            }
            return AuthResultModel.Success();
        }
    }
}