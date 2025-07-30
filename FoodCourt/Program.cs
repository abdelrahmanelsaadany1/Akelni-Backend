using Domain.Contracts;
using Domain.Contracts.Item;
using Domain.Entities;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence.Data;
using Persistence.Mappers;
using Services.Abstractions.ICategoryService;
using Services.Abstractions.IServices;
using Services.Auth;
using Services.CategoryService;

using Sieve.Services;
using System.Text;

namespace FoodCourt
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Allow CORS -- 1
            string corsPolicyName = "AllowSpecificOrigins";
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add DbContexts
            builder.Services.AddDbContext<FoodCourtDbContext>((optionsbuilder =>
            {
                optionsbuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            }));

            builder.Services.AddDbContext<IdentityContext>((optionsbuilder =>
            {
                optionsbuilder.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
            }));

            // Add Identity
            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            // Add your services
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();
            // JWT
            builder.Services.AddScoped<JwtService>();
            // Email Service
            builder.Services.AddScoped<EmailService>();
            // Location Service
            builder.Services.AddScoped<LocationService>();

            builder.Services.AddScoped<IResturantService, ResturantService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IItemService, ItemService>();


            builder.Services.AddScoped<IAddOnService, AddOnService>();
            builder.Services.AddScoped<IComboService, ComboService>();
            builder.Services.AddScoped(typeof(IItemRepository<>), typeof(ItemRepository<>));
       



            //order 
            builder.Services.AddScoped(typeof(IExtendedRepository<>), typeof(OrderRepository<>));
            builder.Services.AddScoped<IExtendedRepository<ItemAddOn>, OrderRepository<ItemAddOn>>();
            builder.Services.AddScoped<IExtendedRepository<ItemCombo>, OrderRepository<ItemCombo>>();

            // OrderService registration (this is missing from your Program.cs)
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddHttpContextAccessor();
            // Authentication
            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration")))
                    };
                });

            //builder.Services.AddScoped<FacebookAuthService>();


            // Facebook and google
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
                });
                //.AddFacebook(options =>
                //{

                //    options.ClientId = builder.Configuration["Authentication:Facebook:AppId"] ?? "";
                //    options.ClientSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? "";
                //    options.Fields.Add("name");
                //});
            //.AddFacebook(options =>
            //{
            //    options.AppId = builder.Configuration["Authentication:Facebook:AppId"] ?? "";
            //    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? "";
            //    options.Scope.Add("email");
            //    options.Fields.Add("email");
            //});
            // Allow CORS --2
            builder.Services.AddCors(opt =>
            {
                opt.AddPolicy(corsPolicyName,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200", "https://akelny-front.vercel.app")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
            });

            //Sieve Filtering etc...
            builder.Services.AddScoped<SieveProcessor>();

            //AutoMapper
            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());



            var app = builder.Build();

            // Initialize database and roles
            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                await dbInitializer.InitializerIdentityAsync();
            }

            //Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
            }

            app.UseSwaggerUI();

            // Allow CORS -- 3 (moved before authentication)
            app.UseCors(corsPolicyName);

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/", () => " FoodCourt API — VERSION 1.7 ");
            app.MapControllers();

            app.Run();
        }
    }
}
