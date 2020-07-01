// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SmartKG.KGBot.StorageAccessor;
using SmartKG.Common.Logger;
using SmartKG.Common.DataPersistence;

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
            

            try
            {
                DataLoader.initInstance(Configuration);
                log.Information("KG Data is initialized.");
            }
            catch (Exception e)
            {
                log.Error("Exception in KG Data initializing.\n" + e.Message);
            }

            try
            {
                DataLoader accessor = DataLoader.GetInstance();
                accessor.Load(Configuration);

                /*List<Vertex> vList = accessor.GetVertexCollection();
                List<Edge> eList = accessor.GetEdgeCollection();
                List<VisulizationConfig> vcList = accessor.GetVisulizationConfigs();


                DataPersistanceKGParser kgParser = new DataPersistanceKGParser(vList, eList, vcList);
                kgParser.ParseKG();

                log.Information("Knowledge Graph is parsed.");
                Console.WriteLine("Knowledge Graph is parsed.");*/
            }
            catch (Exception e)
            {
                log.Error("Exception in KnowledgeGraph parsing.\n" + e.Message);
                Console.WriteLine("[Error]" + e.Message);
            }

           /* try
            {
                DataLoader.initInstance(Configuration);
                log.Information("NLU Data is initialized.");
            }
            catch (Exception e)
            {
                log.Error("Exception in NLU Data initializing.\n" + e.Message);
            }

            try
            {
                List<ScenarioSetting> settings = Configuration.GetSection("Scenarios").Get<List<ScenarioSetting>>();

                DataLoader loader = DataLoader.GetInstance();
                loader.Load(Configuration);

                DataPersistanceNLUParser nluParser = new DataPersistanceNLUParser(loader.GetIntentCollection(), loader.GetEntityCollection(), loader.GetEntityAttributeCollection());
                nluParser.Parse();

                List<Vertex> roots = KnowledgeGraphStore.GetInstance().GetAllVertexes();
                nluParser.ParseScenarioSettings(settings, roots);

                log.Information("NLU materials is parsed.");
                Console.WriteLine("NLU materials is parsed.");
            }
            catch (Exception e)
            {
                log.Error("Exception in NLU materials parsing.\n" + e.Message);
            }
            */

            try
            {
                ContextAccessController.initInstance(Configuration);
                log.Information("Context Data is initialized.");
            }
            catch (Exception e)
            {
                log.Error("Exception in Context Data initializing.\n" + e.Message);
            }
        }
    }
}
