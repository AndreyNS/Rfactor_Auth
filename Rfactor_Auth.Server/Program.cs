using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
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


builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001"; // URL вашего IdentityServer4
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1.read");
    });
});



builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "RfactorVoice";
    })
    .AddOAuth("RfactorVoice", options =>
    {
        options.ClientId = "your-client-id"; 
        options.ClientSecret = "your-client-secret"; 

        options.AuthorizationEndpoint = "https://your-voice-service/connect/authorize"; 
        options.TokenEndpoint = "https://your-voice-service/connect/token"; 

        options.CallbackPath = new PathString("/signin-oauth"); 
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
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers()
        .RequireAuthorization("ApiScope");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.UseRouting();

app.Run();
