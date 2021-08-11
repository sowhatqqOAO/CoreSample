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
        /// CSRF���R�W
        /// </summary>
        private readonly string sCSRF = "CorsIpAccess";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        #region �`�J�A��
        /// <summary>
        /// �`�J�A��
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            //�`�JJWT��k
            services.AddSingleton<JwtHelpers>();

            #region �`�JCSRF�W�h
            services.AddCors(options =>
            {
                options.AddPolicy(sCSRF,
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader();
                });
            });
            #endregion

            //�`�JControllers
            services.AddControllers();

            #region OpenAP
            //�`�J OpenAPI ���
            services.AddOpenApiDocument();

            //OpenAPI�]�w
            services.AddOpenApiDocument(config =>
            {
                // �]�w���W�� (���n) (�w�]��: v1)
                config.DocumentName = "v2";

                // �]�w���� API ������T
                config.Version = "0.0.1";

                // �]�w�����D (����� Swagger/ReDoc UI ���ɭԷ|��ܦb�e���W)
                config.Title = "CoreApiDemo";

                // �]�w���²�n����
                config.Description = "Core�����d�Ҵ���";
            });
            #endregion

            //���U�O����֨�
            services.AddMemoryCache();

            #region ActionFilter JWT����
            services.AddMvc(config =>
            {
                config.Filters.Add(new ActionFilter(Configuration));//���UActionFilter����jwt�i�H��������            
            });
            #endregion

            #region Dapper
            //Scoped�G�`�J������b�P�@Request���A�ѦҪ����O�ۦP����(�A�bController�BView���`�J��IDbConnection���V�ۦP�Ѧ�)
            services.AddScoped<IDbConnection, SqlConnection>(serviceProvider => {
                SqlConnection conn = new SqlConnection();
                //�����s�u�r��
                conn.ConnectionString = Configuration.GetConnectionString("DatabaseString");
                return conn;
            });
            #endregion
            //�Ĥ@�Ӫx�����`�J��������ĳ�� Interface �ӥ]�ˡA�o�˦b�~���ۨ����Y��C
            //�ĤG�Ӫx�����갵�����O
            services.AddTransient<ISampleTransient, DISample>();//Transient�C���`�J�ɡA�����s new �@�ӷs����ҡC
            services.AddScoped<ISampleScoped, DISample>();//Scoped �C�� Request �����s new �@�ӷs����ҡA�P�@�� Request ���޸g�L�h�֭� Pipeline ���O�ΦP�@�ӹ�ҡC�W�ҩҨϥΪ��N�O Scoped�C
            services.AddSingleton<ISampleSingleton, DISample>();//Singleton�Q��Ҥƫ�N���|�����A�{���B������u�|���@�ӹ�ҡC
        }
        #endregion

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //CSRF ���i�H�N�ܰʦ�l��bUseEndpoints����|����
            app.UseCors(sCSRF);

            #region ���ҧP�_
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            #endregion

            app.UseRouting();

            #region JWT�����ҦA���v
            //�L�n��������
            app.UseAuthentication();

            //�L�n���ر��v
            app.UseAuthorization();
            #endregion

            app.UseHttpsRedirection();

            #region Swagger Api��󲣥X
            app.UseOpenApi(config =>
            {
                // �o�̪� Path �Ψӳ]�w OpenAPI ��󪺸��� (���}���|) (�@�w�n�H / �׽u�}�Y)
                config.Path = "/swagger/v2/swagger.json";

                // �o�̪� DocumentName ������ services.AddOpenApiDocument() ���ɭԳ]�w�� DocumentName �@�P�I
                config.DocumentName = "v2";

                config.PostProcess = (document, http) =>
                {
                    if (env.IsDevelopment())
                    {
                        #region �}�o����
                        document.Info.Title += " (�}�o����)";
                        document.Info.Version += "-dev";
                        document.Info.Description = "API�}�o���W�d";
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
                        #region �D�}�o����
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
                // �o�̪� Path �Ψӳ]�w Swagger UI ������ (���}���|) (�@�w�n�H / �׽u�}�Y)
                config.Path = "/swagger";

                // �o�̪� DocumentPath �Ψӳ]�w OpenAPI ��󪺺��}���| (�@�w�n�H / �׽u�}�Y)
                config.DocumentPath = "/swagger/v2/swagger.json";

                // �o�̪� DocExpansion �Ψӳ]�w Swagger UI �O�_�n�i�}��� (�i�]�w�� none, list, full�A�w�]: none)
                config.DocExpansion = "list";
            });

            app.UseReDoc(config =>
            {
                // �o�̪� Path �Ψӳ]�w ReDoc UI ������ (���}���|) (�@�w�n�H / �׽u�}�Y)
                config.Path = "/redoc";

                // �o�̪� DocumentPath �Ψӳ]�w OpenAPI ��󪺺��}���| (�@�w�n�H / �׽u�}�Y)
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
