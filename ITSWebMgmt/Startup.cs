using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Controllers;
using ITSWebMgmt.Connectors;
using ITSWebMgmt.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace ITSWebMgmt
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether userController consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            SCCM.Init();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "group",
                    template: "{controller=Group}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "computer",
                    template: "{controller=Computer}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "user",
                    template: "{controller=User}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "defendpointchallengeresponse",
                    template: "{controller=DefendpointChallengeResponse}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "createworkitem",
                    template: "{controller=CreateWorkItem}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "aauredirector",
                    template: "{controller=AAURedirector}/{action=Index}/{id?}");
            });
        }
    }
}
