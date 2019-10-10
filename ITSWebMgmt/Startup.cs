using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ITSWebMgmt.Models.Log;

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
            services.AddControllersWithViews();
            services.AddMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            var connection = Configuration.GetConnectionString("MyConnectionString");
            services.AddDbContext<LogEntryContext>(options => options.UseSqlServer(connection));
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
                app.UseExceptionHandler("/Error/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            SCCM.Init();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("group", "{controller=Group}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("computer", "{controller=Computer}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("user", "{controller=User}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("log", "{controller=Log}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("defendpointchallengeresponse", "{controller=DefendpointChallengeResponse}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("createworkitem", "{controller=CreateWorkItem}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("aauredirector", "{controller=AAURedirector}/{action=Index}/{id?}");
            });
        }
    }
}
