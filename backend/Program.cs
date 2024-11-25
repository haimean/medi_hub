using DashboardApi.Apis;
using DashboardApi.Application;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.DatabaseContext.AppDbcontext;
using DashboardApi.DatabaseContext.AppMongoDbContext;
using DashboardApi.DatabaseContext.DapperDbContext;
using DashboardApi.HttpConfig;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
#region Registger Services

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Dashboard API - 2024/10/21", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
   {
      {
         new OpenApiSecurityScheme
         {
            Reference = new OpenApiReference
            {
               Type=ReferenceType.SecurityScheme,
               Id="Bearer"
            }
         },
         new string[]{}
      }
   });
});

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
var conn = Environment.GetEnvironmentVariable("WORKERCONNECTIONSTRING");

builder.Services.AddDbContext<WorkerDbContext>(opt => opt.UseNpgsql(conn));

builder.Services.RegisterAppService();
builder.Services.AddSignalR();
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddSingleton<DigiCheckDapperContext>();
builder.Services.AddSingleton<WorkerDapperContext>();
builder.Services.AddSingleton<QaQcDapperContext>();
builder.Services.AddSingleton<ResourceCommonDapperContext>();
builder.Services.AddSingleton<AppMainDapperContext>();
builder.Services.AddSingleton<JotDapperContext>();
builder.Services.AddSingleton<DigiCheckDapperContext>();
builder.Services.AddSingleton<SafetyDbContext>();
builder.Services.AddSingleton<MaintenanceDbContext>();

builder.Services.AddScoped<IPermissionChecker, PermissionChecker>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
//builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

//builder.Services.AddAuthorization(options =>
//{
//   var permissions = new List<string>();
//   permissions.AddRange(new PermissionProvider().GetAll());
//   foreach (var permission in permissions)
//   {
//      options.AddPolicy(permission, policy => policy.Requirements.Add(new PermissionRequirement(permission)));
//   }
//});

#endregion

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
       builder =>
       {
           builder.WithOrigins("https://idd.wohhup.com", "https://test.bql-app.com",
           "http://localhost:3000");
       });
});

builder.Services.Configure<HttpEndpoint>(options => builder.Configuration.GetSection("Endpoint").Bind(options));


var app = builder.Build();
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "dashboard/swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/dashboard/swagger/v1/swagger.json", "BeCommon v1");
        c.RoutePrefix = "dashboard/swagger";
    });
}

app.UseHttpsRedirection();
app.Use((context, next) =>
{
    try
    {
        var token = context?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (token != null)
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); // symmetric
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

            var json = decoder.Decode(token, "wohhupiddapp2022", verify: false);

            dynamic jsonObj = JsonConvert.DeserializeObject(json);
            context.Items["User"] = jsonObj.email;
            context.Items["UserId"] = jsonObj.userId;
        }

    }
    catch (Exception e)
    {
        var m = e.Message;
        //
    }

    return next(context);
});

new QaQcApiV1().RegisterApi(app);
new QaQcApiV2().RegisterApi(app);
new QaQcJotApi().RegisterApi(app);
new QaQcDigicheckApi().RegisterApi(app);
new WorkerApi().RegisterApi(app);
new SafetyApi().RegisterApi(app);
new MaintenanceApi().RegisterApi(app);
new DigicheckApi().RegisterApi(app);

var configuration = builder.Configuration;
app.MapGet("dashboard/healthcheck", () => "9.20AM - 22.06.2023 Bim app api Ok!");

app.MapGet("dashboard/healthcheck2", () => new
{
    Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    DigiConnectionString = Environment.GetEnvironmentVariable("DIGICONNECTIONSTRING"),
    JotConnectionString = Environment.GetEnvironmentVariable("JOTCONNECTIONSTRING"),
    QaQcConnectionString = Environment.GetEnvironmentVariable("QAQCCONNECTIONSTRING"),
    WorkerConnectionString = Environment.GetEnvironmentVariable("WORKERCONNECTIONSTRING"),
    SafetyConnectionString = Environment.GetEnvironmentVariable("SAFETYCONNECTIONSTRING"),
    MaintenanceConnectionString = Environment.GetEnvironmentVariable("MAINTENANCE_CONNECTIONSTRING"),
    ResourceCommonConnectionString = Environment.GetEnvironmentVariable("RESOURCECOMMONCONNECTIONSTRING"),
});

app.UseCors(x => x
   .SetIsOriginAllowed(origin => true)
   .AllowAnyMethod()
   .AllowAnyHeader()
   .AllowCredentials()
   .WithExposedHeaders("Content-Disposition"));

app.UseCors(MyAllowSpecificOrigins);

app.Run();

