using Microsoft.Extensions.Options;
using System.Net.Security;
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

builder.Services.AddTransient<IVoiceManager, VoiceManager>();
builder.Services.AddSingleton<IRecognition, SpeechRecognition>();
builder.Services.AddSingleton<EncryptionService>();

string recognitionUrl = builder.Configuration["RecognitionUrl"];
builder.Services.AddHttpClient("RecognitionSpeech", client =>
{
    client.BaseAddress = new Uri(recognitionUrl);
});

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
