using System;
using System.Threading.Tasks;
using DiabNet.Features.Search;
using DiabNet.Features.Synchronization.Nightscout;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Nest;

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
            services.AddHttpClient("nightscout", c => { c.BaseAddress = new Uri(_nightscoutUrl); });
            services.AddTransient<NightscoutApi>();
            services.AddSingleton(new ElasticClient(new Uri(_elasticUrl)));
            services.AddSingleton<ISearchService, ElasticSearchService>();
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "DiabNet", Version = "v1"}); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async Task Configure(IApplicationBuilder app, IWebHostEnvironment env, ElasticSearchService searchService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DiabNet v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            await searchService.EnsureInitialized();

        }
    }
}