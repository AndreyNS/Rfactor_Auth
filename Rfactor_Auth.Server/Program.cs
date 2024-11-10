using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Rfactor_Auth.Data;
using Rfactor_Auth.Server.Contracts;
using Rfactor_Auth.Server.Interfaces;
using Rfactor_Auth.Server.Interfaces.Database;
using Rfactor_Auth.Server.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
    build => build.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
});

builder.Services.AddSingleton<VoiceConverterBase, WebmToWavConverter>();
builder.Services.AddSingleton<ICrypto, CryptoMethod>();
builder.Services.AddSingleton<ICache, BlacklistService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


//builder.Services.AddDbContext<ApplicationDbContext>(
//    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<ApplicationDbContext>(
        options => options.UseInMemoryDatabase("TestDb"));

string identityServerUrl = builder.Configuration["IdentityServer"];
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Authority = identityServerUrl;
        options.ClientId = "react_client";
        options.ClientSecret = "mysecret";

        options.ResponseType = "code";
        options.UsePkce = true;
        options.RequireHttpsMetadata = false; 

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("api1.read");

        options.SaveTokens = true;

        options.GetClaimsFromUserInfoEndpoint = true;
        options.RequireHttpsMetadata = false;
        options.CallbackPath = new PathString("/callback"); 

        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                if (context.Properties.Items.TryGetValue("acr_values", out var acrValues))
                {
                    context.ProtocolMessage.AcrValues = acrValues;
                }
                if (context.Properties.Items.TryGetValue("username", out var username))
                {
                    context.ProtocolMessage.SetParameter("username", username);
                }

                context.Properties.RedirectUri = "/api/protected/callback";
                return Task.CompletedTask;

            },
            OnRemoteFailure = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Failure.Message}");
                context.HandleResponse();
                context.Response.Redirect("/error");
                return Task.CompletedTask;
            }
        };
    });


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
});


var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };

        //options.Events = new JwtBearerEvents
        //{
        //    OnTokenValidated = async context =>
        //    {
        //        var blacklistService = context.HttpContext.RequestServices.GetRequiredService<ICache>();
        //        var token = context.SecurityToken; //as JwtSecurityToken;

        //        if (token != null)
        //        {
        //            //var tokenId = token.RawData;
        //            if (blacklistService.IsTokenBlacklisted(token.Id))
        //            {
        //                context.Fail("Токен в чс");
        //            }
        //        }
        //    },
        //    OnAuthenticationFailed = context =>
        //    {
        //        Console.WriteLine("Ошибка аутентификации: " + context.Exception.Message);
        //        return Task.CompletedTask;
        //    }
        //};
    });

builder.Services.AddAuthorization();


builder.Services.AddHttpClient("VoiceAuth", client =>
{
    client.BaseAddress = new Uri("https://localhost:7117/api/");
});

builder.Services.AddHttpClient("ImageAuth", client =>
{
    client.BaseAddress = new Uri("https://localhost:7117/api/");
});

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();
app.MapFallbackToFile("/index.html");


app.Run();
