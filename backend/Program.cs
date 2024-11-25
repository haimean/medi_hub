using AutoMapper;
using DashboardApi.Application;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.DatabaseContext.DapperDbContext;
using DashboardApi.HttpConfig;
using HttpConfig;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using QAQC.Profiles;
using QAQCApi.Aws.Dtos;
using QAQCApi.DatabaseContext.AppDbcontext;

var builder = WebApplication.CreateBuilder(args);
string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

#region Registger Services

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "QAQC Entry App API - 13.11.2024", Version = "v1" });
    option.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer",
        }
    );
    option.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        }
    );

    // config base
});

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var conn = Environment.GetEnvironmentVariable("MediHubConnectionString");
builder.Services.AddDbContext<MediHubContext>(opt => opt.UseNpgsql(conn));

builder.Services.RegisterAppService();
builder.Services.AddSignalR();

builder.Services.AddSingleton<MediHubDapperContext>();
builder.Services.AddSingleton<MediHubDapperContext>();

builder.Services.AddScoped<IPermissionChecker, PermissionChecker>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddTransient<HttpStatusCodeFilterMiddleware>();

#endregion
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: MyAllowSpecificOrigins,
        builder =>
        {
            builder.WithOrigins(
                "https://idd.wohhup.com",
                "https://test.bql-app.com",
                "http://localhost:3000"
            );
        }
    );
});

builder.Services.Configure<HttpEndpoint>(options =>
    builder.Configuration.GetSection("Endpoint").Bind(options)
);
builder.Services.Configure<AwsOptions>(options =>
    builder.Configuration.GetSection("AwsOptions").Bind(options)
);

builder.Services.AddEndpointsApiExplorer();

// add mapper
var autoMapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});
IMapper mapper = autoMapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

var app = builder.Build();
app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "qaqc/swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/qaqc/swagger/v1/swagger.json", "BeCommon v1");
        c.RoutePrefix = "qaqc/swagger";
    });
}

app.UseHttpsRedirection();
app.Use(
    (context, next) =>
    {
        try
        {
            var token = context
                ?.Request.Headers["Authorization"]
                .FirstOrDefault()
                ?.Split(" ")
                .Last();
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
    }
);

var configuration = builder.Configuration;
app.MapGet("qaqc/healthcheck", () => "Ver 29.06.2023 - Bim app api Ok!");

app.MapGet(
    "qaqc/healthcheck2",
    () =>
        new
        {
            Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            DigiConnectionString = Environment.GetEnvironmentVariable("DIGICONNECTIONSTRING"),
            JotConnectionString = Environment.GetEnvironmentVariable("JOTCONNECTIONSTRING"),
            QaQcConnectionString = Environment.GetEnvironmentVariable("QAQCCONNECTIONSTRING"),
            WorkerConnectionString = Environment.GetEnvironmentVariable("WORKERCONNECTIONSTRING"),
            MaintenanceConnectionString = Environment.GetEnvironmentVariable(
                "MAINTENANCECONNECTIONSTRING"
            ),
        }
);

app.UseCors(x =>
    x.SetIsOriginAllowed(origin => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .WithExposedHeaders("Content-Disposition")
);

app.UseCors(MyAllowSpecificOrigins);
app.MapControllers();

app.UseMiddleware<HttpStatusCodeFilterMiddleware>();

await using var scope = app.Services.CreateAsyncScope();
using var db = scope.ServiceProvider.GetService<MediHubContext>();
await db.Database.MigrateAsync();

app.Run();
