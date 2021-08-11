using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asp.Net_Core_Learn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly IDbConnection _conn;

        public NewsController(IDbConnection conn)
        {
            this._conn = conn;
        }


        /// <summary>
        /// 取得消息
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public IEnumerable<VMNews> Get()
        {
            //Dapper查詢資料，注意不能用IEnumerable<DataRow>來接結果
            string sSql = @"SELECT Pk_News
                                  ,News_Title
                                  ,News_State
                                  ,News_Content
                                  ,News_CreateDate
                                  ,News_CreateIp
                                  ,News_CreateCode
                                  ,News_EditDate
                                  ,News_EditIp
                                  ,News_EditCode
                          FROM Coredb.dbo.News"; 

            using (IDbConnection conn = _conn)
            {
                var result = conn.Query<VMNews>(
                    sSql).ToList();
               return result;
            }
        }

        /// <summary>
        /// 新增消息
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [Route("")]
        [HttpPost]
        public ResultViewModel AddAccount([FromBody] VMNews parameter)
        {
            #region 資料檢查
            //a、所有欄位皆須輸入。
            if (string.IsNullOrWhiteSpace(parameter.News_Title) ||
                string.IsNullOrWhiteSpace(parameter.News_State) ||
                string.IsNullOrWhiteSpace(parameter.News_Content))
            {
                throw new Exception("請檢查輸入欄位，缺一不可！");
            }

            #endregion
            #region 資料寫入
            var result = 0;
            var parameters = new DynamicParameters();
            Dictionary<string, object> dCon = new Dictionary<string, object>();
            dCon.Add("Pk_News", Guid.NewGuid());
            dCon.Add("News_Title", parameter.News_Title);
            dCon.Add("News_State", parameter.News_State);
            dCon.Add("News_Content", parameter.News_Content);
            dCon.Add("News_CreateCode", "System");
            dCon.Add("News_CreateDate", DateTime.Now);
            dCon.Add("News_CreateIp", "127.0.0.1");
            dCon.Add("News_EditCode", "System");
            dCon.Add("News_EditDate", DateTime.Now);
            dCon.Add("News_EditIp", "127.0.0.1");
            string sSql = @"Insert News(";
            if (dCon != null && dCon.Count > 0)
            {
                List<string> lColname = new List<string>() { };
                List<object> lValue = new List<object>() { };
                foreach (KeyValuePair<string, object> item in dCon)
                {
                    lColname.Add(item.Key);
                    lValue.Add("@" + item.Key);
                    parameters.Add("@" + item.Key, item.Value);
                }
                sSql += string.Join(",", lColname.ToArray());
                sSql += ") VALUES(";
                sSql += string.Join(",", lValue.ToArray());
                sSql += ")";
            }

            using (IDbConnection conn = _conn)
            {
                conn.Open();
                try
                {
                    using (var tran = conn.BeginTransaction())
                    {
                        result = conn.Execute(sSql, parameters, tran);
                        tran.Commit();
                    }
                }
                catch (Exception e)
                {

                }
                finally
                {
                    conn.Close();
                }
            }
            return new ResultViewModel
            {
                Success = result > 0 ? true:false,
                Message = result > 0 ? "新增成功" : "新增失敗"
            };
            #endregion
        }

        /// <summary>
        /// 更新消息
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [Route("")]
        [HttpPatch]
        public ResultViewModel UpdateAccount([FromBody] VMNews parameter)
        {
            #region 資料檢查
            //a、所有欄位皆須輸入。
            if (string.IsNullOrWhiteSpace(parameter.News_Title) ||
                string.IsNullOrWhiteSpace(parameter.News_State) ||
                string.IsNullOrWhiteSpace(parameter.News_Content)||
                parameter.Pk_News==Guid.Empty)
            {
                throw new Exception("請檢查輸入欄位，缺一不可！");
            }

            #endregion
            #region 資料寫入
            var result = 0;
            var parameters = new DynamicParameters();
            Dictionary<string, object> dCon = new Dictionary<string, object>();
            dCon.Add("News_Title", parameter.News_Title);
            dCon.Add("News_State", parameter.News_State);
            dCon.Add("News_Content", parameter.News_Content);
            dCon.Add("News_EditCode", "System");
            dCon.Add("News_EditDate", DateTime.Now);
            dCon.Add("News_EditIp", "127.0.0.1");
            Dictionary<string, object> dWhere = new Dictionary<string, object>();
            dWhere.Add("Pk_News", parameter.Pk_News);
            string sSql = @"Update News 
                            Set ";
            if (dCon != null && dCon.Count > 0)
            {
                List<string> lColname = new List<string>() { };
                List<object> lValue = new List<object>() { };
                foreach (KeyValuePair<string, object> item in dCon)
                {
                    lColname.Add(item.Key+"=@"+ item.Key);
                    parameters.Add("@" + item.Key, item.Value);
                }
                sSql += string.Join(",", lColname.ToArray());
            }
            if (dWhere != null && dWhere.Count > 0)
            {
                sSql += " Where ";
                List<string> lWhere = new List<string>() { };
                foreach (KeyValuePair<string, object> item in dWhere)
                {
                    lWhere.Add(item.Key + "=@" + item.Key);
                    parameters.Add("@" + item.Key, item.Value);
                }
                sSql += string.Join(",", lWhere.ToArray());
            }
            using (IDbConnection conn = _conn)
            {
                conn.Open();
                try
                {
                    using (var tran = conn.BeginTransaction())
                    {
                        result = conn.Execute(sSql, parameters, tran);
                        tran.Commit();
                    }
                }
                catch (Exception e)
                {

                }
                finally
                {
                    conn.Close();
                }
            }
            return new ResultViewModel
            {
                Success = result > 0 ? true : false,
                Message = result > 0 ? "更新成功" : "更新失敗"
            };
            #endregion
        }

        /// <summary>
        /// 刪除消息
        /// </summary>
        /// <param name="gPK"></param>
        /// <returns></returns>
        [Route("{gPK}")]
        [HttpDelete]
        public ResultViewModel RemoveAccount(Guid gPK)
        {
            var result = 0;
            string sSql = @"Delete News
                            WHERE Pk_News = @Pk_News";

            if (gPK == Guid.Empty)
            {
                throw new Exception("資料錯誤");
            }

            var parameter = new DynamicParameters();
            parameter.Add("@Pk_News", gPK, DbType.Guid);

            using (IDbConnection conn = _conn)
            {
                conn.Open();
                try
                {
                    using (var tran = conn.BeginTransaction())
                    {
                        result = conn.Execute(sSql, parameter, tran);
                        tran.Commit();
                    }
                }
                catch (Exception e)
                {

                }
                finally
                {
                    conn.Close();
                }
            }
            return new ResultViewModel
            {
                Success = result > 0 ? true : false,
                Message = result > 0 ? "刪除成功" : "刪除失敗"
            };
        }
    }
}
