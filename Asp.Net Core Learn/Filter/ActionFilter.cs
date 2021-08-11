using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Asp.Net_Core_Learn.Filter
{
    public class ActionFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Startup
        /// </summary>
        private readonly IConfiguration Configuration;

        #region ctor
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="configuration"></param>
        public ActionFilter(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        #endregion

        /// <summary>
        /// OnActionExecuting(參考網址：https://docs.microsoft.com/zh-tw/aspnet/core/mvc/controllers/filters?view=aspnetcore-5.0)
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            StringValues sAuthorizationToken;

            //取得不須驗證的Path項目
            var AIngore = Configuration.GetSection("IngoreAuth").GetChildren().ToList();

            //進行JWT Token驗證

            #region 檢查是否需要驗證
            bool bReturn = true;
            if (AIngore != null)
            {
                foreach (var Ingore in AIngore)
                {
                    if (context.HttpContext.Request.Path.HasValue && context.HttpContext.Request.Path == Ingore.Value)
                    {
                        bReturn = false;//代表不用進行驗證
                        break;
                    }
                }
            }
            #endregion

            #region 執行Token驗證
            if (bReturn)
            {
                //HTTP Header的Key必須是「Authorization」，Value必須是「Bearer 」開頭，後面要一個空白
                //「Bearer 」可視為前贅字
                if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out sAuthorizationToken) || !sAuthorizationToken.ToString().Contains("Bearer "))
                {
                    context.Result = new BadRequestObjectResult("並無驗證碼!");
                }
                else
                {
                    try
                    {
                        //移除「Bearer 」前贅字
                        var jwtEncodedString = sAuthorizationToken[0].Replace("Bearer ", "");

                        var handler = new JwtSecurityTokenHandler();

                        //讀取Token
                        var jwtSecurityToken = handler.ReadJwtToken(jwtEncodedString);

                        //取得claims元素
                        var jti = jwtSecurityToken.Claims.First(claim => claim.Type == "exp").Value;

                        if (string.IsNullOrEmpty(jti))
                        {
                            context.Result = new BadRequestObjectResult("Token驗證資料異常!");
                        }
                        else
                        {
                            //驗證時效
                            if (IsTokenExpired(jti.ToString()))
                            {
                                context.Result = new BadRequestObjectResult("驗證期限已過請重新取得Token!");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Result = new BadRequestObjectResult(ex.Message);
                    }
                }
            }
            #endregion

            base.OnActionExecuting(context);
        }

        #region 驗證時效方法
        public bool IsTokenExpired(string dateTime)
        {
            long timesec = Convert.ToInt64(dateTime);
            bool bRetuen = true;
            //TimeSpan ts1 = new TimeSpan(Convert.ToDateTime(timesec).Ticks);
            //double dAllSec = ts1.TotalSeconds;//转换为总秒数
            if (timesec > Convert.ToInt64(DateTimeOffset.UtcNow.AddMinutes(0).ToUnixTimeSeconds()))
            {
                bRetuen = false;
            }
            return bRetuen;
        }
        #endregion
    }
}
