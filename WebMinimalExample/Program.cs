using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;
using WebMinimalExample.Data;
using WebMinimalExample.Endpoints;
using WebMinimalExample.Models;
using WebMinimalExample.Models.DTOs;
using WebMinimalExample.Models.DTOs.Item;
using WebMinimalExample.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

//---------------------Scaller UI-----------------------------------------------------------------------
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "WebMinimalExample API",
            Version = "v1",
            Description = "Minimal API with JWT Bearer Authentication"
        };

        var components = document.Components ??= new OpenApiComponents();
        var schemes = components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        schemes["Bearer"] = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token in the format: Bearer {your token}"
        };

        var security = document.Security ??= new List<OpenApiSecurityRequirement>();
        security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

//---------------------Db Connection and Jwt Token -----------------------------------------------------------------------


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddValidation();
builder.Services.AddProblemDetails();
var secretKey = builder.Configuration["ApiSettings:Secret"]
                ?? throw new InvalidOperationException("ApiSettings:Secret is not configured.");
var issuer = builder.Configuration["ApiSettings:Issuer"];
var audience = builder.Configuration["ApiSettings:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ICategoryService, CategoryService>();


//---------------------Auto MapperConfig  For Category  -----------------------------------------------------------------------

builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<CategoryCreateDTO, Category>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.AddedDate, opt => opt.Ignore());

    cfg.CreateMap<CategoryUpdateDTO, Category>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.AddedDate, opt => opt.Ignore());

    cfg.CreateMap<Category, CategoryDTO>().ReverseMap();

});



//---------------------Auto MapperConfig  For Menu Items  -----------------------------------------------------------------------


builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<MenuItem, MenuItemDTO>()
         .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
    cfg.CreateMap<MenuItemCreateDTO, MenuItem>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
        .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
        .ForMember(dest => dest.Category, opt => opt.Ignore());
    cfg.CreateMap<MenuItemUpdateDTO, MenuItem>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
        .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) // Image updated only via upload endpoint
        .ForMember(dest => dest.Category, opt => opt.Ignore());

});
//-----------------------------Build-----------------------------------------------------------------------


var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapOpenApi();
app.MapScalarApiReference();



app.UseAuthentication();
app.UseAuthorization();

app.MapCategoryEndpoints();
app.MapAuthEndpoints();




app.UseHttpsRedirection();


app.Run();

