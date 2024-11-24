using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MISA.ApplicationCore.Interfaces;
using MISA.ApplicationCore.Interfaces.Repository;
using MISA.ApplicationCore.Interfaces.Service;
using MISA.ApplicationCore.Service;
using MISA.Infastructure;
using MISA.Infrastructure;

namespace MISA.AMIS.Web
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MISA.AMIS.Web", Version = "v1" });
            });

            // Add scoped for base repository
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));

            // Add scoped for employee repository
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeService, EmployeeService>();

            // Add scoped for department repository
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IDepartmentService, DepartmentService>();

            // Config services access with http
            services.AddCors(options =>
            {
                options.AddPolicy("AllowMyOrigin",
                builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MISA.AMIS.Web v1"));
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            // Confid access http
            app.UseCors("AllowMyOrigin");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
