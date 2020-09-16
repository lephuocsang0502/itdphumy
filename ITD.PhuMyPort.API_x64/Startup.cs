using ITD.PhuMyPort.API.Models;
using ITD.PhuMyPort.API.Services;
using ITD.PhuMyPort.DataAccess;
using ITD.PhuMyPort.DataAccess.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace ITD.PhuMyPort.API
{
    public class Startup
    {
        static string path = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
        static string dbname = Path.Combine(path, "DB", "ConfigWebDb.db");
        string connectionString = @"Data Source=" + dbname + ";";
        public const string CookieScheme = "AccountScheme";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            LicensePlateRecogEx.IsInit = LicensePlateRecogEx.Init();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ConfigWebContext>(options => options.UseSqlite(connectionString));
            services.AddAuthentication(CookieScheme) // Sets the default scheme to cookies
               .AddCookie(CookieScheme, options =>
               {
                   options.LoginPath = "/Account/Index";
                   options.Cookie.Name = "AccountCookie";
               });
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 2147483648;
            });
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
            });
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });
            services.Configure<PLCSettings>(Configuration.GetSection("PLCSettings"));
            services.AddSingleton(typeof(PLCServices));
            services.AddHostedService<PLCBackgroundService>();
            services.AddControllers(); 
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();    
            
            //Adding static file middleware
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Index}/{id?}");
            });
        }
    }
}
