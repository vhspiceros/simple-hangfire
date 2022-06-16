using System;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using simple_hangfire.Jobs.IJobs;
using simple_hangfire.Jobs.Jobs;

namespace simple_hangfire {
    public class Startup {

        private Microsoft.Extensions.Logging.ILogger _Logger;
        public IConfiguration _configuration { set; get; }

        public Startup(IConfiguration configuration) {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            ConfigureSerilog();
            ConfigureDependecies(services);
            ConfigureJobsDependecies(services);
            ConfigureHangfire(services);
        }

        private void ConfigureJobsDependecies(IServiceCollection services) {
            services.AddTransient<ISimpleJob, SimpleJob>();
        }

        protected virtual void ConfigureHangfire(IServiceCollection services) {
            services.AddHangfire(opt => {
                opt.UseMemoryStorage();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapGet("/", async context => {
                    await context.Response.WriteAsync("Hello World!");
                });
            });


            StartHangFireJobs(app, serviceProvider);
        }


        private void ConfigureDependecies(IServiceCollection services) {
            services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger), _Logger);
            services.AddSingleton(typeof(IConfiguration), _configuration);
        }

        private void ConfigureSerilog() {

            var serilog = new LoggerConfiguration()
                                  .ReadFrom.Configuration(_configuration)
                                  .Enrich.FromLogContext()
                                  .CreateLogger();

            var loggerFactory = new LoggerFactory().AddSerilog(serilog);
            _Logger = loggerFactory.CreateLogger("Logger");
            _Logger.LogInformation("Starting...");
        }

        /// <summary>
        /// schedules hangfire jobs
        /// </summary>
        /// <param name="app"></param>
        /// <param name="serviceProvider"></param>
        protected virtual void StartHangFireJobs(IApplicationBuilder app, IServiceProvider serviceProvider) {
            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions {
                //Authorization = new[] { new HangfireDashboardAuthorizationFilter() },
                StatsPollingInterval = 30000,
                // no back link
                AppPath = null
            });
            var minutesCronExpresion = Hangfire.Cron.Minutely();
            RecurringJob.AddOrUpdate(() => serviceProvider.GetService<ISimpleJob>().Execute(), minutesCronExpresion);


        }
    }
}
