using FluentValidation;
using FluentValidation.AspNetCore;
using labs_lntu_web.DbContexts;
using labs_lntu_web.Services;
using labs_lntu_web.Tasks;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace labs_lntu_web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddControllersWithViews();
            
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();
            builder.Services.AddFluentValidationRulesToSwagger();

            builder.Services.AddTransient<IPinger, Pinger>();
            builder.Services.AddSingleton<HostsResultsTempStorage>();
            
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            builder.Services.AddSingleton<PingWorker>();
            builder.Services.AddHostedService(sp => sp.GetRequiredService<PingWorker>());

            builder.Services.AddDbContext<ApplicationDbContext>(o=> {
                o.UseSqlite("Data Source=application.db");
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options=> {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                options.SupportNonNullableReferenceTypes();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            if ( app.Environment.IsDevelopment() ) {
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pinger API V1");
                    c.SwaggerEndpoint("/swagger/v1/swagger.yaml", "Pinger API V1");
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapControllers();
            app.Run();
        }
    }
}
