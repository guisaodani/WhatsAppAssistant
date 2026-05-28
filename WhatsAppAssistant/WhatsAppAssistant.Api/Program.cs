using Microsoft.SemanticKernel;
using WhatsAppAssistant.Application.Services;
using WhatsAppAssistant.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAssistantService, AssistantService>();
builder.Services.AddScoped<IMessageService, MessageService>();

var groqApiKey = builder.Configuration["Groq:ApiKey"] ?? "";
var groqModel = builder.Configuration["Groq:ModelId"] ?? "llama-3.3-70b-versatile";

var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.AddOpenAIChatCompletion(
    modelId: groqModel,
    apiKey: groqApiKey,
    httpClient: new HttpClient { BaseAddress = new Uri("https://api.groq.com/openai/v1/") }
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();