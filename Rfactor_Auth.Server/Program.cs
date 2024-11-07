using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using Rfactor_Auth.Server.Interfaces;
using Rfactor_Auth.Server.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
    build => build.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
});

builder.Services.AddSingleton<VoiceConverterBase, WebmToWavConverter>();

string identityServerUrl = builder.Configuration["IdentityServer"];
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "IdentityServer";
    })
    .AddCookie()
    .AddOAuth("IdentityServer", options =>
    {
        options.ClientId = "react_client";
        options.ClientSecret = "mysecret";

        options.AuthorizationEndpoint = $"{identityServerUrl}oauth2/v2/auth";
        options.TokenEndpoint = $"{identityServerUrl}oauth2/v2/token";

        options.CallbackPath = new PathString("/");
        options.SaveTokens = true;
        options.Scope.Add("profile");
        options.Scope.Add("openid");

        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
    });


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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.UseRouting();

app.Run();
