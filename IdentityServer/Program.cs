using IdentityServer.Contracts;
using IdentityServer.Data;
using IdentityServer.Interfaces;
using IdentityServer.Interfaces.Database;
using IdentityServer.Services;
using Microsoft.EntityFrameworkCore;
using VoiceAuthentification;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
    build => build.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();

builder.Services.AddIdentityServer(options =>
{
    options.UserInteraction.LoginUrl = "/oauth2/v2/auth";
})
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryClients(Config.Clients)
    .AddTestUsers(Config.Users)
    .AddDeveloperSigningCredential(); //Не забыть убрать (только разработка)


//builder.Services.AddDbContext<ApplicationDbContext>(
//    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<VoiceConverterBase, WebmToWavConverter>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddHostedService<ReencryptionService>();

builder.Services.AddDbContext<ApplicationDbContext>(
        options => options.UseInMemoryDatabase("TestDb"));

string voiceUrl = builder.Configuration["VoiceUrl"];
builder.Services.AddHttpClient("VoiceAuth", client =>
{
    client.BaseAddress = new Uri(voiceUrl);
});

string imageUrl = builder.Configuration["ImageUrl"];
builder.Services.AddHttpClient("ImageAuth", client =>
{
    client.BaseAddress = new Uri(imageUrl);
});


builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigin");
app.UseStaticFiles();
app.UseRouting();

app.UseIdentityServer();
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseAuthorization();

//app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
