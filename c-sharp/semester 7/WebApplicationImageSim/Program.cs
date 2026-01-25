using WebApplicationImageSim.Data;
using System.IO;
using WebApplicationImageSim;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                             "Models",
                             "arcfaceresnet100-8.onnx");
builder.Services.AddSingleton(new SimilarityService(modelPath));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
