using System;
using System.Security;
using DiabNet.Configuration;
using DiabNet.Domain.Services;
using DiabNet.ElasticSearch;
using DiabNet.Features;
using DiabNet.HostedServices;
using DiabNet.Nightscout;
using DiabNet.Sync;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Nest;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace DiabNet
{
    public class Startup
    {
        private readonly string _nightscoutUrl;
        private readonly string _elasticUrl;
        private readonly string? _elasticUser;
        private readonly SecureString? _elasticPassword;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _nightscoutUrl = configuration["Nightscout:Url"];
            _elasticUrl = configuration["Elastic:Url"];
            _elasticUser = configuration["Elastic:Username"];
            _elasticPassword = configuration["Elastic:Password"]?.CreateSecureString();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<INightscoutApi, NightscoutApi>(client =>
            {
                client.BaseAddress = new Uri(_nightscoutUrl);
            });
            services.AddSingleton(BuildElasticClient());
            services.AddSingleton<ISearchService, ElasticSearchService>();
            services.AddSingleton<ISgvSyncService, SgvSyncService>();
            // Add Quartz services
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddTransient<SyncSgvJob>();
            // Add Job schedule
            services.AddSingleton(new JobSchedule(typeof(SyncSgvJob), "0 0 * ? * * *"));

            services.AddHostedService<QuartzHostedService>();

            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "DiabNet", Version = "v1"}); });
        }

        private ElasticClient BuildElasticClient()
        {
            var node = new SingleNodeConnectionPool(new Uri(_elasticUrl));
            var settings = new ConnectionSettings(node);
            if (!string.IsNullOrEmpty(_elasticUser) && !string.IsNullOrEmpty(_elasticPassword.CreateString()))
            {
                settings = settings.BasicAuthentication(_elasticUser, _elasticPassword);
            }

            return new ElasticClient(settings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISearchService searchService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DiabNet v1"));

           // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            if (searchService is ElasticSearchService elastic)
            {
                await elastic.EnsureInitialized();
            }
        }
    }
}
