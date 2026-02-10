using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using smrp;
using smrp.Services;
using smrp.Utils;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Swagger SMRP API",
        Version = "v1",
        Description = "SMRP API"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
    });
    //c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    //{
    //    [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
    //});
    c.OperationFilter<AuthorizationOperationFilter>();
});
builder.Services.AddScoped<DefaultConnection>();
builder.Services.AddScoped<RsConnection>();
builder.Services.AddSingleton<IMongoClient>(s => new MongoClient(builder.Configuration.GetConnectionString("MongoDbConnection")));
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
    };
});

var app = builder.Build();

string basePath = "smrp";
app.UsePathBase($"/{basePath}");
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"/{basePath}/swagger/v1/swagger.json", "SMRP API V1");
    c.DocumentTitle = "Swagger SMRP API";
    c.InjectStylesheet("/css/theme-flattop.css");
    //c.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
    c.EnablePersistAuthorization();
});

app.MapControllers();
app.UseStaticFiles();

app.Run();
