using Asp.Net_Core_Learn.Filter;
using Asp.Net_Core_Learn.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Asp.Net_Core_Learn
{
    public class Startup
    {
        /// <summary>
        /// CSRF的命名
        /// </summary>
        private readonly string sCSRF = "CorsIpAccess";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        #region 注入服務
        /// <summary>
        /// 注入服務
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            //注入JWT方法
            services.AddSingleton<JwtHelpers>();

            #region 注入CSRF規則
            services.AddCors(options =>
            {
                options.AddPolicy(sCSRF,
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader();
                });
            });
            #endregion

            //注入Controllers
            services.AddControllers();

            #region OpenAP
            //注入 OpenAPI 文件
            services.AddOpenApiDocument();

            //OpenAPI設定
            services.AddOpenApiDocument(config =>
            {
                // 設定文件名稱 (重要) (預設值: v1)
                config.DocumentName = "v2";

                // 設定文件或 API 版本資訊
                config.Version = "0.0.1";

                // 設定文件標題 (當顯示 Swagger/ReDoc UI 的時候會顯示在畫面上)
                config.Title = "CoreApiDemo";

                // 設定文件簡要說明
                config.Description = "Core版本範例測試";
            });
            #endregion

            //註冊記憶體快取
            services.AddMemoryCache();

            #region ActionFilter JWT驗證
            services.AddMvc(config =>
            {
                config.Filters.Add(new ActionFilter(Configuration));//註冊ActionFilter來讓jwt可以全域驗證            
            });
            #endregion

            #region Dapper
            //Scoped：注入的物件在同一Request中，參考的都是相同物件(你在Controller、View中注入的IDbConnection指向相同參考)
            services.AddScoped<IDbConnection, SqlConnection>(serviceProvider => {
                SqlConnection conn = new SqlConnection();
                //指派連線字串
                conn.ConnectionString = Configuration.GetConnectionString("DatabaseString");
                return conn;
            });
            #endregion
            //第一個泛型為注入的類型建議用 Interface 來包裝，這樣在才能把相依關係拆除。
            //第二個泛型為實做的類別
            services.AddTransient<ISampleTransient, DISample>();//Transient每次注入時，都重新 new 一個新的實例。
            services.AddScoped<ISampleScoped, DISample>();//Scoped 每個 Request 都重新 new 一個新的實例，同一個 Request 不管經過多少個 Pipeline 都是用同一個實例。上例所使用的就是 Scoped。
            services.AddSingleton<ISampleSingleton, DISample>();//Singleton被實例化後就不會消失，程式運行期間只會有一個實例。
        }
        #endregion

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //CSRF 不可隨意變動位子放在UseEndpoints之後會失效
            app.UseCors(sCSRF);

            #region 環境判斷
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            #endregion

            app.UseRouting();

            #region JWT先驗證再授權
            //微軟內建驗證
            app.UseAuthentication();

            //微軟內建授權
            app.UseAuthorization();
            #endregion

            app.UseHttpsRedirection();

            #region Swagger Api文件產出
            app.UseOpenApi(config =>
            {
                // 這裡的 Path 用來設定 OpenAPI 文件的路由 (網址路徑) (一定要以 / 斜線開頭)
                config.Path = "/swagger/v2/swagger.json";

                // 這裡的 DocumentName 必須跟 services.AddOpenApiDocument() 的時候設定的 DocumentName 一致！
                config.DocumentName = "v2";

                config.PostProcess = (document, http) =>
                {
                    if (env.IsDevelopment())
                    {
                        #region 開發環境
                        document.Info.Title += " (開發環境)";
                        document.Info.Version += "-dev";
                        document.Info.Description = "API開發文件規範";
                        document.Info.Contact = new NSwag.OpenApiContact
                        {
                            //Name = "",
                            //Email = "",
                            //Url = ""
                        };
                        #endregion
                    }
                    else
                    {
                        #region 非開發環境
                        //document.Info.TermsOfService = "https://go.microsoft.com/fwlink/?LinkID=206977";

                        //document.Info.Contact = new NSwag.OpenApiContact
                        //{
                        //    Name = "",
                        //    Email = "",
                        //    Url = ""
                        //};
                        #endregion
                    }

                    document.Info.License = new NSwag.OpenApiLicense
                    {
                        Name = "",
                        Url = ""
                    };
                };
            });

            app.UseSwaggerUi3(config =>
            {
                // 這裡的 Path 用來設定 Swagger UI 的路由 (網址路徑) (一定要以 / 斜線開頭)
                config.Path = "/swagger";

                // 這裡的 DocumentPath 用來設定 OpenAPI 文件的網址路徑 (一定要以 / 斜線開頭)
                config.DocumentPath = "/swagger/v2/swagger.json";

                // 這裡的 DocExpansion 用來設定 Swagger UI 是否要展開文件 (可設定為 none, list, full，預設: none)
                config.DocExpansion = "list";
            });

            app.UseReDoc(config =>
            {
                // 這裡的 Path 用來設定 ReDoc UI 的路由 (網址路徑) (一定要以 / 斜線開頭)
                config.Path = "/redoc";

                // 這裡的 DocumentPath 用來設定 OpenAPI 文件的網址路徑 (一定要以 / 斜線開頭)
                config.DocumentPath = "/swagger/v2/swagger.json";
            });
            #endregion

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
