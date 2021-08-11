using Asp.Net_Core_Learn.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Asp.Net_Core_Learn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JWTController : ControllerBase
    {
        private readonly JwtHelpers jwt;

        public JWTController(JwtHelpers jwt)
        {
            this.jwt = jwt;
        }

        #region 登入
        [HttpPost("signin")]
        public ActionResult<string> SignIn(LoginViewModel login)
        {
            //假設帳號密碼驗證通過
            if (true)
            {
                //回傳JWT Token
                return jwt.GenerateToken(login.Username);
            }
            else
            {
                return BadRequest();
            }
        }
        #endregion

        #region 取得JWT資訊
        [HttpGet("claims")]
        public IActionResult GetClaims()
        {
            return Ok(User.Claims.Select(p => new { p.Type, p.Value }));
        }

        [HttpGet("username")]
        public IActionResult GetUserName()
        {
            return Ok(User.Identity.Name);
        }

        [HttpGet("jwtid")]
        public IActionResult GetUniqueId()
        {
            var jti = User.Claims.FirstOrDefault(p => p.Type == "jti");
            return Ok(jti.Value);
        }
        #endregion
    }

    #region 登入驗證Model
    public class LoginViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    #endregion
}
