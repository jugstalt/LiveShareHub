using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Certes;
using FluffySpoon.AspNet.LetsEncrypt;
using FluffySpoon.AspNet.LetsEncrypt.Certes;
using LiveShareHub.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LiveShareHub.Core.Extensions.DependencyInjection;
using LiveShareHub.Core.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using LiveShareHub.Services;
using LiveShareHub.Middleware;

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

            #region FluffySpoon Letsencrypt

            if (Configuration["Letsencrypt:Use"]?.ToLower() == "true")
            {
                services.AddFluffySpoonLetsEncrypt(new LetsEncryptOptions()
                {
                    Email = Configuration["Letsencrypt:Email"],
                    UseStaging = false, //switch to true for testing
                    Domains = new[] { Configuration["Letsencrypt:Domain"] },
                    TimeUntilExpiryBeforeRenewal = TimeSpan.FromDays(30), //renew automatically 30 days before expiry
                    TimeAfterIssueDateBeforeRenewal = TimeSpan.FromDays(7), //renew automatically 7 days after the last certificate was issued
                    CertificateSigningRequest = new CsrInfo() //these are your certificate details
                    {
                        CountryName = Configuration["Letsencrypt:CrsInfo:CountryName"],
                        Locality = Configuration["Letsencrypt:CrsInfo:Locality"],
                        Organization = Configuration["Letsencrypt:CrsInfo:Organization"],
                        OrganizationUnit = Configuration["Letsencrypt:CrsInfo:OrganizationUnit"],
                        State = Configuration["Letsencrypt:CrsInfo:State"]
                    }
                });

                //the following line tells the library to persist the certificate to a file, so that if the server restarts, the certificate can be re-used without generating a new one.
                services.AddFluffySpoonLetsEncryptFileCertificatePersistence();

                //the following line tells the library to persist challenges in-memory. challenges are the "/.well-known" URL codes that LetsEncrypt will call.
                services.AddFluffySpoonLetsEncryptMemoryChallengePersistence();
            }

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (Configuration["Letsencrypt:Use"]?.ToLower() == "true")
            {
                app.UseFluffySpoonLetsEncrypt();
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
