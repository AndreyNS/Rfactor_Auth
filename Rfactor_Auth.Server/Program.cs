using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
