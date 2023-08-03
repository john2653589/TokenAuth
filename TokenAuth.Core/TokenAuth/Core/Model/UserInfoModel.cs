using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Rugal.TokenAuth.Common.Model
{
    public partial class UserInfoModel
    {
        internal List<Claim> Claims { get; set; }
        public string UserId => TryBaseGetValue("UserId", out var GetUserId) ? GetUserId : null;
        public string Token { get; set; }
        internal DateTime TokenExpiredDatetime => GetTokenExpiredDatetime();
        internal string GetString([CallerMemberName] string PropertyName = null)
        {
            if (TryBaseGetValue(PropertyName, out var Value))
                return Value;
            return null;
        }
        internal Guid GetGuid([CallerMemberName] string PropertyName = null)
        {
            if (TryBaseGetValue(PropertyName, out var Value))
            {
                if (Guid.TryParse(Value, out var ConvertValue))
                    return ConvertValue;
                return Guid.Empty;
            }
            return Guid.Empty;
        }
        internal Guid? GetGuidNull([CallerMemberName] string PropertyName = null)
        {
            if (TryBaseGetValue(PropertyName, out var Value))
            {
                if (Guid.TryParse(Value, out var ConvertValue))
                    return ConvertValue;
                return null;
            }
            return null;
        }
        internal bool GetBool([CallerMemberName] string PropertyName = null)
        {
            if (TryBaseGetValue(PropertyName, out var Value))
            {
                if (bool.TryParse(Value, out var ConvertValue))
                    return ConvertValue;
                return false;
            }
            return false;
        }
        internal int GetInt([CallerMemberName] string PropertyName = null)
        {
            if (TryBaseGetValue(PropertyName, out var Value))
            {
                if (int.TryParse(Value, out var ConvertValue))
                    return ConvertValue;
                return -1;
            }
            return -1;
        }

        internal List<TValue> GetArray<TValue>([CallerMemberName] string PropertyName = null)
        {
            if (TryBaseGetValue(PropertyName, out var Value))
            {
                var TryGetList = System.Text.Json.JsonSerializer.Deserialize<List<TValue>>(Value);
                return TryGetList;
            }
            return null;
        }
        internal DateTime GetTokenExpiredDatetime()
        {
            var GetExp = GetString("exp");
            if (long.TryParse(GetExp, out var Time))
                return DateTimeOffset.FromUnixTimeSeconds(Time).LocalDateTime;

            return DateTime.MinValue;
        }
        private bool TryBaseGetValue(string PropertyName, out string OutValue)
        {
            OutValue = null;
            var GetClaim = Claims.FirstOrDefault(Item => Item.Type == PropertyName);
            if (GetClaim is not null)
            {
                OutValue = GetClaim.Value;
                return true;
            }
            return false;
        }
        internal Dictionary<string, object> GetClaimsInfo()
        {
            var Ret = new Dictionary<string, object> { };
            foreach (var Item in Claims)
            {
                var GetType = Item.ValueType.ToLower();
                var GetValue = Item.Value;
                if (GetType == ClaimValueTypes.Boolean && bool.TryParse(GetValue, out var OutBoolean))
                    Ret.TryAdd(Item.Type, OutBoolean);
                else if (GetType == ClaimValueTypes.Integer && int.TryParse(GetValue, out var OutInt))
                    Ret.TryAdd(Item.Type, OutInt);
                else
                    Ret.TryAdd(Item.Type, GetValue);
            }
            return Ret;
        }
        public UserInfoModel AddClaims(string Type, object Value, ClaimsValueType ValueType)
        {
            var GetValueType = ValueType switch
            {
                ClaimsValueType.String => ClaimValueTypes.String,
                ClaimsValueType.Integer => ClaimValueTypes.Integer,
                ClaimsValueType.Boolean => ClaimValueTypes.Boolean,
                _ => ClaimValueTypes.String
            };
            Claims.Add(new Claim(Type, Value?.ToString() ?? "", GetValueType));
            return this;
        }
    }
    public enum ClaimsValueType
    {
        String,
        Integer,
        Boolean
    }
}