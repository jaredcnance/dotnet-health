﻿using DotnetStatus.Services;
using DotnetStatus.Services.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetStatus
{
    public class Startup
    {
        public readonly IConfiguration Config;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Config = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole();
            services.AddSingleton<ILoggerFactory>(loggerFactory);

            services.AddMvc();
            services.AddCors();
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();

            if (env.IsDevelopment())
                app.UseCors(builder => builder.WithOrigins("http://localhost:4200"));

            app.UseMvc(routes =>
            {
                routes.MapRoute("status", "api/status/gh/{*path}",
                        defaults: new { controller = "GitHubPackageStatus", action = "GetGithub" });
            });
        }
    }
}