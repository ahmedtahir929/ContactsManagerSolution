using ContactsManager.Core.Domain.IdentityEntities;
using CRUDExample.Filters.ActionFilters;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts.CountriesServiceContracts;
using ServiceContracts.PersonsServiceContracts;
using Services.CountriesService;
using Services.PersonsService;

namespace CRUDExample
{
  public static class ConfigureServicesExtension
  {
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
      services.AddControllersWithViews(options =>
       {
         //options.Filters.Add<ResponseHeaderActionFilter>();

         var logger = services.BuildServiceProvider().GetService<ILogger<ResponseHeaderActionFilter>>();

         options.Filters.Add(new ResponseHeaderActionFilter(logger, "My-Key-From-Global", "My-Value-From-Global", 2));
       });

      services.AddHttpLogging();
      //Add services into IoC container
      //CountriesService
      services.AddScoped<ICountriesAdderService, CountriesAdderService>();
      services.AddScoped<ICountriesGetterService, CountriesGetterService>();
      services.AddScoped<ICountriesUploaderService, CountriesUploaderService>();
      //PersonsService
      services.AddScoped<IPersonsAdderService, PersonsAdderService>();
      services.AddKeyedScoped<IPersonsGetterService, PersonsGetterService>("original");
      services.AddScoped<IPersonsGetterService>(sp =>
      {
        var original =
            sp.GetRequiredKeyedService<IPersonsGetterService>("original");

        return new PersonsGetterServiceWithFewExcelFields(original);
      });
      services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();
      services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();
      services.AddScoped<IPersonsSorterService, PersonsSorterService>();
      //Repository injection
      services.AddScoped<ICountriesRepository, CountriesRepository>();
      services.AddScoped<IPersonsRepository, PersonsRepository>();

      if (!webHostEnvironment.IsEnvironment("Test"))
      {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
          //options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]);
          options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
          //both works, but the second one is more common and recommended as it provides better error handling if the connection string is missing or misconfigured.
        });
      }

      services
        .AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
          options.Password.RequiredLength = 8;
          options.Password.RequireNonAlphanumeric = true;
          options.Password.RequireUppercase = true;
          options.Password.RequireLowercase = true;
          options.Password.RequireDigit = true;
          options.Password.RequiredUniqueChars = 3;
        })
        //Configure DbContext for Identity
        .AddEntityFrameworkStores<ApplicationDbContext>()
        //Configure default token providers for Identity i.e, password reset, email confirmation, etc.
        .AddDefaultTokenProviders()
        //Configure UserStore to use custom ApplicationUser and ApplicationRole with Guid as primary key type
        .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
        //Configure RoleStore to use custom ApplicationRole with Guid as primary key type
        .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>()
        ;

      services.AddAuthorization(options =>
      {
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); //enforces authorization policy (user must be authenticated) for all the action methods.
      });

      services.ConfigureApplicationCookie(options =>
      {
        options.LoginPath = "/Account/Login";
      });

      return services;
    }
  }
}