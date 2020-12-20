using System;
using DiabNet.Configuration;
using DiabNet.Domain.Services;
using DiabNet.ElasticSearch;
using DiabNet.Features;
using DiabNet.HostedServices;
using DiabNet.Nightscout;
using DiabNet.Sync;
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

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _nightscoutUrl = configuration["NIGHTSCOUT_URL"];
            _elasticUrl = configuration["ELASTIC_URL"];
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<INightscoutApi, NightscoutApi>(client =>
            {
                client.BaseAddress = new Uri(_nightscoutUrl);
            });
            services.AddSingleton(new ElasticClient(new Uri(_elasticUrl)));
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
