using OktaBackend.GraphQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OktaBackend.Data;
using OktaBackend.Grpc;
using OktaBackend.Services;
using OktaBackend.Settings;
using System.Text;
using OktaBackend.Soap;
using SoapCore;

namespace OktaBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddControllers()
                .AddXmlSerializerFormatters();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Unesi JWT token. Primjer: Bearer eyJhbGciOi..."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services
            .AddGraphQLServer()
            .AddQueryType<OktaPolicyQuery>()
            .AddMutationType<OktaPolicyMutation>();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<ApiSettings>(
                builder.Configuration.GetSection("ApiSettings"));

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("JwtSettings"));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                });
            });

            builder.Services.AddHttpClient();

            builder.Services.AddGrpc();
            builder.Services.AddHttpClient<WeatherGrpcService>();

            builder.Services.AddHttpClient<IOktaPolicyExternalService, OktaPolicyExternalService>();
            builder.Services.AddScoped<IOktaPolicySourceService, OktaPolicySourceService>();

            builder.Services.AddSoapCore();

            builder.Services.AddSingleton<IOktaPolicySoapService, OktaPolicySoapService>();

            var jwtSettings = builder.Configuration
                .GetSection("JwtSettings")
                .Get<JwtSettings>();

            if (jwtSettings == null)
            {
                throw new InvalidOperationException("JwtSettings nisu postavljeni u appsettings.json.");
            }

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                context.Database.Migrate();

                if (!context.Users.Any())
                {
                    context.Users.AddRange(
                        new OktaBackend.Models.User
                        {
                            Email = "readonly@test.com",
                            PasswordHash = OktaBackend.Controllers.AuthController.HashPassword("Test123!"),
                            Role = "ReadOnly"
                        },
                        new OktaBackend.Models.User
                        {
                            Email = "full@test.com",
                            PasswordHash = OktaBackend.Controllers.AuthController.HashPassword("Test123!"),
                            Role = "FullAccess"
                        }
                    );

                    context.SaveChanges();
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("FrontendPolicy");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseSoapEndpoint<IOktaPolicySoapService>(
                "/OktaPolicySoapService.asmx",
                new SoapEncoderOptions()
            );

            app.MapControllers();

            app.MapGraphQL().RequireAuthorization();

            app.MapGrpcService<WeatherGrpcService>();

            app.Run();
        }
    }
}