﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Swashbuckle.AspNetCore.Swagger;
using TobinTaxer.Authorization;
using TobinTaxer.Clients;
using TobinTaxer.DB;
using TobinTaxer.OptionModels;

namespace TobinTaxer
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvcCore().AddAuthorization().AddJsonFormatters();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });

            SetupDatabase(services, _env);

            services.Configure<Services>(Configuration.GetSection(nameof(Services)));
            services.Configure<TaxInfo>(Configuration.GetSection(nameof(TaxInfo)));
            services.Configure<RabbitMqOptions>(Configuration.GetSection(nameof(RabbitMqOptions)));

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options =>
            //    {
            //        options.Authority = Configuration["IdentityServerBaseAddress"];
            //        options.Audience = "BankingService";
            //    });

            //services.AddAuthorization(options =>
            //{
            //    //options.AddPolicy("BankingService.UserActions", policy =>
            //    //    policy.Requirements.Add(new HasScopeRequirement("BankingService.UserActions", Configuration["IdentityServerBaseAddress"])));
            //    //options.AddPolicy("BankingService.broker&taxer", policy =>
            //    //    policy.Requirements.Add(new HasScopeRequirement("BankingService.broker&taxer", Configuration["IdentityServerBaseAddress"])));
            //});

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
            services.AddSingleton<IRabbitMqClient, RabbitMqClient>();

            services.AddHealthChecks().AddDbContextCheck<TobinTaxerContext>(tags: new[] { "ready" });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, TobinTaxerContext context)
        {
            app.UseAuthentication();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                context.Database.Migrate();
            }

            SetupReadyAndLiveHealthChecks(app);

            app.UseMetricServer();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void SetupDatabase(IServiceCollection services, IHostingEnvironment env)
        {
            if(env.IsDevelopment())
            {
                //services.AddDbContext<TobinTaxerContext>(options => options.UseInMemoryDatabase("InMemoryDbForTesting"));
                services.AddDbContext<TobinTaxerContext>
                    (options => options.UseSqlServer(Configuration.GetConnectionString("TobinTaxerDatabase")));
            }
            else
            {
                services.AddDbContext<TobinTaxerContext>
                    (options => options.UseSqlServer(Configuration.GetConnectionString("TobinTaxerDatabase")));
            }
        }

        private static void SetupReadyAndLiveHealthChecks(IApplicationBuilder app)
        {
            // The readiness check uses all registered checks with the 'ready' tag.
            app.UseHealthChecks("/health/ready", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("ready"),
            });
            app.UseHealthChecks("/health/live", new HealthCheckOptions()
            {
                // Exclude all checks and return a 200-Ok.
                Predicate = (_) => false
            });
        }
    }
}
