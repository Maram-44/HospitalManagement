using HosbitalManagement.API;
using HospitalManagement.BussinessLogic;
using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.BussinessLogic.Services;
using HospitalManagement.BussinessLogic.Tools;
using HospitalManagement.DataAccess.Data;
using HospitalManagement.DataAccess.Entities;
using HospitalManagement.DataAccess.Repositories.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddBusinessLayer();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // ≈⁄œ«œ«  «Œ Ì«—Ì… · ”ÂÌ· «·œŒÊ· »œÊ‰  ⁄ﬁÌœ«  ﬂ·„… «·„—Ê— Õ«·Ì«
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // ≈⁄œ«œ «·‹ Provider «·Œ«’ »«·‹ OTP („Â„ Ãœ« ··‹ 6 √—ﬁ«„)
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<AppDbContext>() // —»ÿ Identity »ﬁ«⁄œ… »Ì«‰« ﬂ
.AddDefaultTokenProviders(); // Â–« «·”ÿ— Ì”„Õ » Ê·Ìœ «·√ﬂÊ«œ (OTP)

// 3. ≈÷«›… «·‹ Authentication Ê «·‹ Authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddAuthentication().AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
    };
});

// Â–« «·”ÿ— Ì„‰⁄ ASP.NET „‰  ÕÊÌ· √”„«¡ «·‹ Claims ≈·Ï —Ê«»ÿ XML ÿÊÌ·…
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Stripe Payment
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Value;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // √÷Ì›Ì Â–« «·”ÿ—
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPulseApp",
        policy => policy
            .WithOrigins("https://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowPulseApp");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
