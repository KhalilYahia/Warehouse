
using Services.DependenyInjection;
using Warehouse.Model.ApplicationDbContext_DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext_Khalil(connectionString);
builder.Services.AddIdentityOptions_Khalil();


////// add auth and jwt
////builder.Services.AddAuthentication(x =>
////{
////    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
////    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
////}).AddJwtBearer(x =>
////{
////    x.RequireHttpsMetadata = false;
////    x.SaveToken = true;
    
////    x.TokenValidationParameters = new TokenValidationParameters
////    {
////        ValidateIssuerSigningKey = true,
////        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"])),
////        ValidateIssuer = true,
////        ValidateAudience = true,
////        ValidateLifetime = false,
////        ClockSkew = TimeSpan.Zero,
////        ValidIssuer = builder.Configuration["Jwt:Issuer"],
////        ValidAudience = builder.Configuration["Jwt:Audience"],
////    };
////});



// init Swagger for API test and make it fimilure with Bearer token
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    var security = new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer",new string[0]}
                };
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id="Bearer"
                            },
                            Scheme ="oauth2",
                            Name ="Bearer",
                            In = ParameterLocation.Header,
                        },new List<string>()
                    }
                });

    #region This to show the comments
    // Set the comments path for the Swagger JSON and UI.
    var xmlFile_WebApi = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath_WebApi = Path.Combine(AppContext.BaseDirectory, xmlFile_WebApi);
    c.IncludeXmlComments(xmlPath_WebApi);
    var xmlFile_Service = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    xmlFile_Service = xmlFile_Service.Replace("WebApi", "Services");
    var xmlPath_Service = Path.Combine(AppContext.BaseDirectory, xmlFile_Service);
    c.IncludeXmlComments(xmlPath_Service);
    #endregion

});



////builder.Services.AddDefaultIdentity<CustomUser>().AddRoles<CustomRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders(); ;

//builder.Services.Configure<IdentityOptions>(options =>
//{

//    options.ClaimsIdentity.RoleClaimType = System.Security.Claims.ClaimTypes.Role;
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});


    // Add services to the container.
    builder.Services.AddAuthorization();
builder.Services.AddControllers().AddFluentValidation(); 
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.SetDependencies();


//builder.Services.AddScoped<IPostsService, PostsService>();

var app = builder.Build();

app.CreateDb_IfNotExist();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//else
{   
    app.UseHsts();
}

app.UseHttpsRedirection();

////app.UseCors(
////        options => options.WithOrigins("http://alarabiacom.ru").AllowAnyMethod().AllowAnyHeader()
////        .AllowAnyOrigin()
////       .WithExposedHeaders("Custom-Header")
////       .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
////    );

////http://localhost:3000
///"http://khalilbroker-001-site1.ctempurl.com"

// In Configure method
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();



app.Run();
