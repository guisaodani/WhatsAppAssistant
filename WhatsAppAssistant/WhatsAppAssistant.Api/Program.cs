using Microsoft.SemanticKernel;
using WhatsAppAssistant.Application.Services;
using WhatsAppAssistant.Core.Entities;
using WhatsAppAssistant.Core.Interfaces;
using WhatsAppAssistant.Infrastructure.Calendar;
using WhatsAppAssistant.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAssistantService, AssistantService>();
builder.Services.AddScoped<IMessageService, MessageService>();
var googleClientId = builder.Configuration["Google:ClientId"] ?? "";
var googleClientSecret = builder.Configuration["Google:ClientSecret"] ?? "";
builder.Services.AddScoped<ICalendarService>(_ => new GoogleCalendarService(googleClientId, googleClientSecret));

var groqApiKey = builder.Configuration["Groq:ApiKey"] ?? "";
var groqModel = builder.Configuration["Groq:ModelId"] ?? "llama-3.3-70b-versatile";

var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.AddOpenAIChatCompletion(
    modelId: groqModel,
    apiKey: groqApiKey,
    httpClient: new HttpClient { BaseAddress = new Uri("https://api.groq.com/openai/v1/") }
);

// Supabase - repositorio de usuarios
var connectionString = builder.Configuration.GetConnectionString("Supabase") ?? "";
builder.Services.AddScoped<IUsuarioRepository>(_ => new UsuarioRepository(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();