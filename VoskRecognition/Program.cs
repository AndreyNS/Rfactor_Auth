using VoskRecognition.Interfaces;
using VoskRecognition.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(option =>
{
    option.ListenAnyIP(10221);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IRecogtion, RecognitionManager>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
