﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Fortune_Teller_Service.Models;
using Microsoft.EntityFrameworkCore;
using Pivotal.Extensions.Configuration;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Pivotal.Discovery.Client;

namespace Fortune_Teller_Service
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                // Lab07 Start
                .AddConfigServer(env)
                // Lab07 End
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;
        }

        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Lab08 add
            if (Environment.IsDevelopment())
            {
                // Lab06 Start
                services.AddEntityFramework()
                        .AddDbContext<FortuneContext>(options => options.UseInMemoryDatabase());
            } else
            {
                // Lab08 add
                services.AddEntityFramework()
                     .AddDbContext<FortuneContext>(options => options.UseMySql(Configuration));
            }

            services.AddSingleton<IFortuneRepository, FortuneRepository>();
            // Lab06 End

            // Lab09 Start
            services.AddDiscoveryClient(Configuration);
            // Lab09 End

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            app.UseMvc();

            // Lab09 Start
            app.UseDiscoveryClient();
            // Lab09 End

            // Lab06 Start
            SampleData.InitializeFortunesAsync(app.ApplicationServices).Wait();
            // Lab06 End
        }
    }
}
