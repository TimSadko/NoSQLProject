using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;
using System.Collections.Generic;
using NoSQLProject.Services;
using Microsoft.AspNetCore.Authentication;
using NoSQLProject.Other.Security;
using Microsoft.OpenApi.Models;

namespace NoSQLProject
{
    public class Program
    {
        public static void Main(string[] args)
        {           

            DotNetEnv.Env.TraversePath().Load(); // Load .env before building configuration so env vars are available

			var builder = WebApplication.CreateBuilder(args);

            Hasher.SetSalt(builder.Configuration.GetSection("Salt").Value); // Get salt from appsetting.json file and give it to hasher (used for hashing passwords)

            //Console.WriteLine(Hasher.GetHashedString("123")); // Use if you need to hash the password

            // 1) Register MongoClient as a SINGLETON (one shared instance for the whole app)
            // WHY: MongoClient is thread-safe and internally manages a connection pool.
            // Reusing one instance is fast and efficient. Creating many clients would waste resources.
            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                // Read the connection string from configuration (env var via .env)
                var conn = builder.Configuration["Mongo:ConnectionString"];
                if (string.IsNullOrWhiteSpace(conn))
                    throw new InvalidOperationException("Mongo:ConnectionString is not configured. Did you set it in .env?");

                // Optional: tweak settings (timeouts, etc.)
                var settings = MongoClientSettings.FromConnectionString(conn);
                // settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);

                return new MongoClient(settings);
            });
            // 2) Register IMongoDatabase as SCOPED (new per HTTP request)
            // WHY: Fits the ASP.NET request lifecycle and keeps each request cleanly separated.
            builder.Services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();

                var dbName = builder.Configuration["Mongo:Database"]; // from appsettings.json
                if (string.IsNullOrWhiteSpace(dbName))
                    throw new InvalidOperationException("Mongo:Database is not configured in appsettings.json.");

                return client.GetDatabase(dbName);
            });

            // Adding Repositories
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<ITicketRepository, TicketRepository>();
            builder.Services.AddScoped<ITicketRequestRepository, TicketRequestRepository>();

			// Adding Services
			builder.Services.AddScoped<EmployeeService>();
			builder.Services.AddScoped<IServiceDeskEmployeeService, ServiceDeskEmployeeService>();
			builder.Services.AddScoped<ITicketRequestService, TicketRequestService>();

            // Register password reset service
            builder.Services.AddScoped<PasswordResetService>();

			// Add services to the container.
			builder.Services.AddControllersWithViews();

            // Swagger + endpoints explorer for testing
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "NoSQLProject API",
                    Version = "v1"
                });

                // Define HTTP Basic security scheme
                c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Basic authentication using the Authorization header."
                });

                // Require Basic auth for operations that have [Authorize]
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "basic"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Basic Authentication registration (only used where explicitly required)
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme =
                    BasicAuthenticationHandler.SchemeName; // not enforced globally unless [Authorize] is applied
                options.DefaultChallengeScheme = BasicAuthenticationHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicAuthenticationHandler.SchemeName,
                null);

            builder.Services.AddAuthorization();

            builder.Services.AddSession(options => // Configure sessions
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession(); // Enable sessions

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // BsonClassMap registration
            if (!BsonClassMap.IsClassMapRegistered(typeof(Employee)))
            {
                BsonClassMap.RegisterClassMap<Employee>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIsRootClass(true);
                    cm.AddKnownType(typeof(ServiceDeskEmployee));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(ServiceDeskEmployee)))
            {
                BsonClassMap.RegisterClassMap<ServiceDeskEmployee>(cm => cm.AutoMap());
            }

            app.Run();
        }
    }
}