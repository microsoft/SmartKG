// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using SmartKG.KGManagement.DataPersistance;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Logger;
using SmartKG.Common.Importer;
using SmartKG.Common.Data.Visulization;
using SmartKG.KGManagement.DataPersistence;

namespace SmartKG.KGManagement
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory loggerFactory, ILogger<Startup> _log)
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
            try
            {
                KGDataAccessor.initInstance(Configuration);
                log.Information("KGDataAccessor is initialized.");
            }
            catch (Exception e)
            {
                log.Error("Exception in MongoDB initializing.\n" + e.Message);
            }

            try
            {
                KGDataAccessor accessor = KGDataAccessor.GetInstance();
                accessor.Load(Configuration);

                List<Vertex> vList = accessor.GetVertexCollection();
                List<Edge> eList =  accessor.GetEdgeCollection();
                List<VisulizationConfig> vcList = accessor.GetVisulizationConfigs();

                DataPersistanceKGParser kgParser = new DataPersistanceKGParser(vList, eList, vcList);
                kgParser.ParseKG();

                log.Information("Knowledge Graph is parsed.");
                Console.WriteLine("Knowledge Graph is parsed.");               
            }
            catch (Exception e)
            {
                log.Error("Exception in KnowledgeGraph parsing.\n" + e.Message);
                Console.WriteLine("[Error]" + e.Message);
            }
        }
    }
}
