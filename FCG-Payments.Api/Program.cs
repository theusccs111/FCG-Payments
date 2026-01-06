using FCG_Payments.Application.Shared;
using FCG_Payments.Infrastructure.Shared;
using FCG_Payments.Infrastructure.Shared.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Reflection;

namespace FCG_Payments.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls("http://0.0.0.0:80");

            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddApplicationServices();

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Fgc.Payments.Api",
                    Version = "v1"
                });

                c.CustomSchemaIds(n => n.FullName);

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Digite seu token"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id   = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddHttpContextAccessor();

            // ===== CONFIGURAÇÃO DE AUTENTICAÇÃO MODIFICADA =====
            builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "APIM-or-JWT";
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddPolicyScheme("APIM-or-JWT", "APIM or JWT Authentication", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    // Verifica qual esquema usar baseado nos headers
                    if (context.Request.Headers.ContainsKey("X-User-Id"))
                    {
                        return "APIM"; // Tem headers APIM? Usa APIM
                    }

                    // Caso contrário, sempre tenta JWT (mesmo que não tenha Authorization header)
                    // O JWT handler vai retornar Fail se não houver token válido
                    return JwtBearerDefaults.AuthenticationScheme;
                };
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError($"? JWT validation failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var claims = string.Join(", ", context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? new List<string>());
                        logger.LogInformation($"? JWT validated! Claims: {claims}");
                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Convert.FromBase64String(builder.Configuration["Jwt:Key"]!))
                };
            })
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ApimAuthenticationHandler>(
        "APIM", null);

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("SomenteAdmin", policy =>
                    policy.RequireRole("Admin"));
            });

            var app = builder.Build();

            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var ex = exceptionHandlerPathFeature?.Error;

                    context.Response.ContentType = "application/problem+json";

                    var statusCode = ex switch
                    {
                        NotImplementedException => StatusCodes.Status501NotImplemented,
                        TimeoutException => StatusCodes.Status504GatewayTimeout,
                        InvalidOperationException => StatusCodes.Status502BadGateway,
                        _ => StatusCodes.Status500InternalServerError
                    };

                    context.Response.StatusCode = statusCode;

                    var problem = new ProblemDetails
                    {
                        Status = statusCode,
                        Title = "Erro interno",
                        Detail = "Ocorreu um erro inesperado. Tente novamente mais tarde."
                    };

                    await context.Response.WriteAsJsonAsync(problem);
                });
            });

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
                db.Database.Migrate();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
