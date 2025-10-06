using System.IO;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using core8_astro_informix.Helpers;
using core8_astro_informix.Services;
using Microsoft.EntityFrameworkCore;
using IBM.Data.Db2;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//"InformixConnection": "Server=localhost;Port=9088;Database=core8;User=rey;Password=rey;"

// builder.Services.AddTransient<Func<IfxConnection>>(sp => () =>
// {
//     var configuration = sp.GetRequiredService<IConfiguration>();
//     var connectionString = configuration.GetConnectionString("InformixConnection");
//     return new IfxConnection(connectionString);
// });

// builder.Services.AddDbContext<InformixDbContext>(options =>
//     options.UseDb2(builder.Configuration.GetConnectionString("InformixConnection")));

// builder.Services.AddSingleton<IDbConnection>(sp => new IfxConnection(
//     builder.Configuration.GetConnectionString("InformixConnection")
// ));

//    "InformixConnection": "Server=localhost; Database=corey; Service=9088; User ID=rey; Password=rey;"    


// builder.Services.AddDbContext<InformixDbContext>(options =>
//     options.UseInformix(connectionString));


builder.Services.AddSingleton(builder.Configuration.GetConnectionString("InformixConnection")); 

// var connectionString = builder.Configuration.GetConnectionString("InformixConnection");
// builder.Services.AddDbContext<InformixDbContext>(options =>
// {
//     // The Action delegate starts here
//     options.UseDb2(connectionString, db2Options =>
//     {
//         // This is where you configure Db2-specific options
//         // For example, if needed: db2Options.ServerVersion(...);
//     });
// });

// builder.Services.AddDbContext<InformixDbContext>(options =>
//     options.UseInformix(builder.Configuration.GetConnectionString("InformixConnection")));

// builder.Services.AddHealthChecks()
//     .AddCheck<InformixHealthCheck>("Informix Database Check");

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "ADIDAS INC.", Description="RESTful API Documentation", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });

        c.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return new[] { api.GroupName };
                }
                throw new InvalidOperationException("Unable to determine tag for endpoint.");
            });
        c.DocInclusionPredicate((name, api) => true);        
    });


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJWTTokenServices, JWTServiceManage>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseCors( options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePages(async context =>
    {
        if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
        {
            if (!context.HttpContext.Response.ContentLength.HasValue || context.HttpContext.Response.ContentLength == 0)
            {
                // Change ContentType as json serialize
                context.HttpContext.Response.ContentType = "text/json";
                await context.HttpContext.Response.WriteAsJsonAsync(new {message = "Unauthorized Access, Please Login using your account."});
            }
        }
        else
        {
            // Ignore redirect
            context.HttpContext.Response.Redirect($"/error?code={context.HttpContext.Response.StatusCode}");
        }
    });

app.MapControllers();

app.Run();

