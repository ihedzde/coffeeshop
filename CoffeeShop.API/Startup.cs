using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using AutoMapper;
using CoffeeShop.API.Domain.Repositories;
using CoffeeShop.API.Domain.Models;
using CoffeeShop.API.Persistance.Repositories;
using CoffeeShop.API.Persistance.Context;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.API.Helpers;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CoffeeShop.API.Domain.Services;
using CoffeeShop.API.Services;
using CoffeeShop.API.Domain.Services.Communication;
namespace CoffeeShop.API
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
            services.AddCors();
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x => 
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x => 
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ShopApi", Version = "v0.1" });
            });
             services.AddDbContext<AppDbContext>(options =>
               {
                   options.UseInMemoryDatabase("ShopAdo");
               }
            );
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<IRepository<User>, UserRepository>();
            services.AddScoped<IRepository<Coffee>, CoffeeRepository>();
            services.AddScoped<IRepository<Milk>, MilkRepository>();
            services.AddScoped<IRepository<Topping>, ToppingRepository>();
            services.AddScoped<IRepository<Tea>, TeaRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICRUDRepoService<User,UserResponse>, UserService>();
            services.AddScoped<ICRUDRepoService<Coffee,CoffeeResponse>, CoffeeService>();
            services.AddScoped<ICRUDRepoService<Milk,MilkResponse>, MilkService>();
            services.AddScoped<ICRUDRepoService<Topping, ToppingResponse>, ToppingService>();
            services.AddScoped<ICRUDRepoService<Tea, TeaResponse>, TeaService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(x =>
            {
                x.AllowAnyOrigin()
                    .AllowAnyMethod()
                        .AllowAnyHeader();
            });
            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwagger();
           app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My CoffeeShop API");
                c.RoutePrefix = string.Empty;
            });
        }
    }
}
