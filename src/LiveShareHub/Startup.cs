using LiveShareHub.Core.Extensions.DependencyInjection;
using LiveShareHub.Core.Services;
using LiveShareHub.Hubs;
using LiveShareHub.Middleware;
using LiveShareHub.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LiveShareHub
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddControllers();
            services.AddSignalR();

            services.AddDistributedMemoryCache();

            //services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
            services.AddHttpContextAccessor();
            services.AddScoped<RoutingEndPointReflectionService>();  // Scoped => one Instance per Http Request!!


            services.AddGroupProviderService<DefaultGroupProvider, DefaultGroupProviderOptions>(config =>
            {
                config.EncryptionPassword = Configuration["Crypto:KeyPassword"];
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            //app.UseAuthorization();
            app.UseMiddleware<AuthorizeAccessMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<SignalRHub>("/signalrhub");
            });
        }
    }
}
