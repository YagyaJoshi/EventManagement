using EventManagement.API.Controllers.v1;
using EventManagement.API.Extensions;
using EventManagement.BusinessLogic.Helpers;
using EventManagement.DataAccess.ViewModels;
using EventManagement.Utilities.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors();
//Dependency Injection
builder.Services.RegisterDI();
builder.Services.AddMemoryCache();

builder.Services.AddHttpContextAccessor();




builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = actionContext =>
        {
            var modelState = actionContext.ModelState.Values;   
            return new BadRequestObjectResult(new Response { Message = modelState.LastOrDefault()?.Errors?.LastOrDefault()?.ErrorMessage });
            //String.Join(",", modelState.Select(e => e.Errors.LastOrDefault().ErrorMessage))
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    });

builder.Services.AddLocalization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swagger =>
{
    //This is to generate the Default UI of Swagger Documentation
    swagger.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Event Management"
    });
    // To Enable authorization using Swagger (JWT)

    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    swagger.AddSecurityDefinition("OrganizationId", new OpenApiSecurityScheme
    {
        Name = "OrganizationId",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "OrganizationId"
    });

    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        },

         {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "OrganizationId"
                }
            },
            new string[] {}
        }
    });
});


//Jwt Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
    };
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
});

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseMiddleware<JwtMiddleware>();
// Configure the HTTP request pipeline.

HttpContextHelper.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseRouting();
app.UseCors(x => x
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

//Custom middleware to handle OPTIONS requests
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
    }
    else
    {
        await next();
    }
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "";
    c.SwaggerEndpoint("swagger/v1/swagger.json", "EventManagement Api");
    c.InjectStylesheet("/documentation.css");
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
