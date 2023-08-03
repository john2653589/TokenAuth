using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Rugal.TokenAuth.WebApi.Database;
using Rugal.TokenAuth.WebApi.Model;
using System.Reflection;

namespace Rugal.TokenAuth.WebApi.Extention
{
    public static class StartupExtention
    {
        public static IServiceCollection AddAuthServerDI(this IServiceCollection Services, IConfiguration Configuration, AuthServerSetting Setting)
        {
            Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            Services.AddSingleton(Setting);
            Services.AddIdentity<IdentityUser, IdentityRole>(Options =>
            {
                Options.Password.RequireNonAlphanumeric = Setting.RequireNonAlphanumeric;
                Options.Password.RequireUppercase = Setting.RequireUppercase;
                Options.Password.RequireLowercase = Setting.RequireLowercase;
                Options.Password.RequireDigit = Setting.RequireDigit;
                Options.Password.RequiredLength = Setting.RequiredLength;
                Options.SignIn.RequireConfirmedAccount = Setting.RequireConfirmedAccount;
            }).AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

            if (Setting.PasswordResetTokenLifetime is not null)
            {
                Services.Configure<DataProtectionTokenProviderOptions>(Options =>
                {
                    Options.TokenLifespan = Setting.PasswordResetTokenLifetime.Value;
                });
            }

            var AuthServerSection = Configuration.GetSection("AuthServer");
            var DoubleEncodeKey = AuthServerSection?.GetValue<string>("DoubleEncodeKey");
            if (DoubleEncodeKey is not null)
                Setting.DoubleEncodeKey = DoubleEncodeKey;

            var DoubleEncodeIV = AuthServerSection?.GetValue<string>("DoubleEncodeIV");
            if (DoubleEncodeIV is not null)
                Setting.DoubleEncodeIV = DoubleEncodeIV;

            return Services;
        }
        public static IServiceCollection AddSwaggerToken(this IServiceCollection Services)
        {
            Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme,
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                var XmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var XmlPath = Path.Combine(AppContext.BaseDirectory, XmlFile);
                options.IncludeXmlComments(XmlPath);
            });
            return Services;
        }
    }
}