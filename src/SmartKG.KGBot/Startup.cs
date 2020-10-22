// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SmartKG.Common.Logger;
using SmartKG.Common.ContextStore;
using Microsoft.OpenApi.Models;
using SmartKG.Common.DataStoreMgmt;

namespace SmartKG.KGBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }        

        public Startup(IConfiguration configuration)
        {                        
            Configuration = configuration;            
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).Enrich.FromLogContext().CreateLogger();
            
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {            
            services.AddMvc().AddSessionStateTempDataProvider();
            services.AddSession();
            services.AddSingleton<IConfiguration>(Configuration);

            //注册Swagger生成器，定义一个和多个Swagger 文档
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("smartkg", new OpenApiInfo { Title = "SmartKG", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ILogger<Startup> _log)
        {           
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(builder =>
            {
                builder.AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin();
            });

            app.UseHttpsRedirection();
            app.UseSession();            
            app.UseMvcWithDefaultRoute();
                        
            loggerFactory.AddSerilog();

            Serilog.ILogger log = Log.Logger.ForContext<Startup>().Here();
                    
            DataStoreManager.initInstance(Configuration);
            DataStoreManager.GetInstance().LoadDataStores();
            log.Information("KG and NLU Data is initialized and loaded.");
            
            ContextAccessor.initInstance(Configuration);
            log.Information("Context Data is initialized.");

            //启用中间件服务生成Swagger作为JSON终结点
            app.UseSwagger();
            //启用中间件服务对swagger-ui，指定Swagger JSON终结点
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/smartkg/swagger.json", "SmartKG V1");
            });

        }
    }
}
