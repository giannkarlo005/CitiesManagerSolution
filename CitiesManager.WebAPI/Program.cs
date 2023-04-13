using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Core.ServiceImplementation;
using CitiesManager.Infrastructure.DatabaseContext;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json"));
    options.Filters.Add(new ConsumesAttribute("application/json"));

    //Authorization Policy - Global Policy
    //Applies Authorize filter for all Controllers
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddXmlSerializerFormatters();

builder.Services.AddTransient<IJwtService, JwtService>();

//Enable versioning in Web API controllers
builder.Services.AddApiVersioning(config =>
{
    //Reads version number from request url at "apiVersion" constraint
    config.ApiVersionReader = new UrlSegmentApiVersionReader();

    //Reads version number from query string called "api-version"
    //config.ApiVersionReader = new QueryStringApiVersionReader("api-version");
    //config.DefaultApiVersion = new ApiVersion(1,0);
    //config.AssumeDefaultVersionWhenUnspecified = true;

    ////Reads version number from request header called "api-version"
    //config.ApiVersionReader = new HeaderApiVersionReader();
    //config.DefaultApiVersion = new ApiVersion(1,0);
    //config.AssumeDefaultVersionWhenUnspecified = true;
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CitiesManagerConnString"));
});

//Swagger
builder.Services.AddEndpointsApiExplorer(); //Generates description for all enpoints
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));

    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
    {
        Title = "Cities Web API",
        Version = "1.0"
    });

    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo()
    {
        Title = "Cities Web API",
        Version = "2.0"
    });
}); //Generates OpnAPI Specification
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

//CORS: localhost:
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>())
                     .WithHeaders("Authorization", "origin", "accept", "content-type")
                     .WithMethods("GET", "POST", "PUT", "DELETE");
    });

    options.AddPolicy("4100Client", policyBuilder =>
    {
        policyBuilder.WithOrigins(builder.Configuration.GetSection("AllowedOrigins2").Get<string[]>())
                     .WithHeaders("Authorization", "origin", "accept")
                     .WithMethods("GET");
    });
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
}).AddEntityFrameworkStores<ApplicationDbContext>()
  .AddDefaultTokenProviders()
  .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
  .AddRoleStore <RoleStore<ApplicationRole, ApplicationDbContext, Guid>>()
  ;

builder.Services.AddAuthentication(options =>
{
    //DefaultAuthenticateScheme = Cookie Authentication by Default
    //Validate client request using JwtBearerDefaults.AuthenticationScheme
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //If above fails, application will use the DefaultChallengeScheme
    //If use is not authenticated, should redirect to CookieAuthentication Scheme
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization(options => {
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger(); //creates endpoint for swagger.json
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0");
}); //Creates Swagger UI for testing all web endpoints / action methods

app.UseRouting();
app.UseCors();

app.UseHsts();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
