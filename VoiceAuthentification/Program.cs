using Microsoft.Extensions.Options;
using VoiceAuthentification;
using VoiceAuthentification.Interface;
using VoiceAuthentification.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
    build => build.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
});
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IVoiceManager, VoiceManager>();

//builder.Services.Configure<BitrixConfiguration>(builder.Configuration.GetSection("BitrixConfiguration"));
//var bitrixConfig = app.Services.GetRequiredService<IOptions<BitrixConfiguration>>().Value;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigin");
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
