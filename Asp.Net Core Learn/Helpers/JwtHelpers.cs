using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Asp.Net_Core_Learn.Helpers
{
    public class JwtHelpers
    {
        /// <summary>
        /// Startup
        /// </summary>
        private readonly IConfiguration Configuration;

        /// <summary>
        /// 記憶體快取
        /// </summary>
        private IMemoryCache _cache;

        #region ctor
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="Memorycatch"></param>
        public JwtHelpers(IConfiguration configuration, IMemoryCache Memorycatch)
        {
            this.Configuration = configuration;
            this._cache = Memorycatch;
        }
        #endregion

        /// <summary>
        /// 產生JWT Token
        /// </summary>
        /// <param name="userName">Token使用者</param>
        /// <param name="expireMinutes">Token過期時間-單位分鐘</param>
        /// <returns></returns>
        public string GenerateToken(string userName, int expireMinutes = 30)
        {
            #region 取得Appsettings設定
            //JWT發布者
            var issuer = Configuration.GetValue<string>("JwtSettings:Issuer");

            //JWT私鑰
            var signKey = Configuration.GetValue<string>("JwtSettings:SignKey");

            //是否開啟快取(memory機制)
            var Catch = Configuration.GetValue<string>("JwtSettings:Catch");
            #endregion

            #region JWT中間段設定(必要時加密處裡)
            //在 RFC 7519 規格中(Section#4)，總共定義了 7 個預設的 Claims
            //設定要加入到 JWT Token 中的聲明資訊(Claims)，JWT中間段
            var claims = new List<Claim>();

            //JWT發布者
            //claims.Add(new Claim(JwtRegisteredClaimNames.Iss, issuer));

            //Token使用者
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userName));

            //未知
            //claims.Add(new Claim(JwtRegisteredClaimNames.Aud, "The Audience"));

            //Token過期時間(單位秒)
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(expireMinutes).ToUnixTimeSeconds().ToString()));

            //未知-必須為數字
            //claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));

            //未知-必須為數字
            //claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));

            //JWT ID
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            //網路上常看到的這個 NameId 設定是多餘的
            //claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userName));

            //這個 Claim 也以直接被 JwtRegisteredClaimNames.Sub 取代，所以也是多餘的
            //claims.Add(new Claim(ClaimTypes.Name, userName));

            //你可以自行擴充 "roles" 加入登入者該有的角色
            //claims.Add(new Claim("roles", "Admin"));
            //claims.Add(new Claim("roles", "Users"));

            //產生JWT中間段
            var userClaimsIdentity = new ClaimsIdentity(claims);
            #endregion

            #region 建立一組對稱式加密的金鑰，主要用於 JWT 簽章之用(JWT第三段)
            if (Catch == "1")
            {
                //JWT加密的私鑰
                var Rkey = Guid.NewGuid().ToString();
                signKey = Rkey;

                //放進cache中
                _cache.Set(userName, Rkey, TimeSpan.FromMinutes(expireMinutes));
            }

            //執行加密
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey));

            //HmacSha256 有要求必須要大於 128 bits，所以 key 不能太短，至少要 16 字元以上
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            #endregion

            #region 產生JWT Token
            // 建立 SecurityTokenDescriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,//發行者
                //Audience = issuer, // 由於你的 API 受眾通常沒有區分特別對象，因此通常不太需要設定，也不太需要驗證
                //NotBefore = DateTime.Now, // 預設值就是 DateTime.Now
                //IssuedAt = DateTime.Now, // 預設值就是 DateTime.Now
                Subject = userClaimsIdentity,
                Expires = DateTime.Now.AddMinutes(expireMinutes),
                SigningCredentials = signingCredentials
            };

            // 產出所需要的 JWT securityToken 物件，並取得序列化後的 Token 結果(字串格式)
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var serializeToken = tokenHandler.WriteToken(securityToken);
            #endregion

            return serializeToken;
        }
    }
}
