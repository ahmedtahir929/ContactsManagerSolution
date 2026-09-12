using OfficeOpenXml;
using Rotativa.AspNetCore;
using Serilog;
using CRUDExample;
using CRUDExample.Middleware;

var builder = WebApplication.CreateBuilder(args);

//Serilog
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
  //Read configurations from built-in IConfiguration i.e from, appsettings.json
  loggerConfiguration
  .ReadFrom.Configuration(context.Configuration)
  .ReadFrom.Services(services); //makes our service collection available to serilog
});

builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

ExcelPackage.License.SetNonCommercialPersonal("Ahmed Tahir");

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
}
else
{
  app.UseExceptionHandler("/Error");
  app.UseExceptionHandlingMiddleware();
}

if (!builder.Environment.IsEnvironment("Test"))
{
  RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");
}

app.UseSerilogRequestLogging(); //Enable Serilog request logging
app.UseHttpLogging();
app.UseStaticFiles();
app.UseRouting(); //Idenitifying action method based on route
app.UseAuthentication(); //Reading Identity Cookie
app.UseAuthorization(); //Validates access permissions of the user
app.MapControllers(); //Executing the filter pipeline (action + filter)

app.Run();

public partial class Program { } //make the auto-generated Program class accessible programmatically