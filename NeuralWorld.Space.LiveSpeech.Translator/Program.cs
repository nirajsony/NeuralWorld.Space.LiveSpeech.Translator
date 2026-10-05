using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.FileProviders;
using NeuralWorld.Space.LiveSpeech.Translator.Hubs;
using NeuralWorld.Space.LiveSpeech.Translator.Interfaces;
using NeuralWorld.Space.LiveSpeech.Translator.Services;
using OllamaSharp;
using OpenAI;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
})
.AddMessagePackProtocol();


var translationConfig = builder.Configuration.GetSection("Translation");
var provider = translationConfig["Provider"] ?? "Ollama";

if (provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
{
    var ollamaEndpoint = translationConfig["Endpoint"] ?? "http://localhost:11434";
    var ollamaModel = translationConfig["Model"] ?? "qwen3-translator";
    builder.Services.AddSingleton<IChatClient>(_ => new OllamaApiClient(new Uri(ollamaEndpoint), ollamaModel));
}
else
{
    // Uses Microsoft.Extensions.AI with any OpenAI-compatible cloud provider (Groq, OpenAI, etc.)
    // Requires NuGet packages: OpenAI (v2.0+) and Microsoft.Extensions.AI.OpenAI
    var apiKey = translationConfig["ApiKey"] ?? string.Empty;
    var endpoint = translationConfig["Endpoint"];
    var model = translationConfig["Model"] ?? "gpt-4o-mini";

    var clientOptions = new OpenAI.OpenAIClientOptions();
    if (!string.IsNullOrEmpty(endpoint))
    {
        clientOptions.Endpoint = new Uri(endpoint);
    }

    var openAiClient = new OpenAI.OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey), clientOptions);

    // The .AsChatClient() extension method wires the native OpenAIClient up to the IChatClient abstraction
    builder.Services.AddSingleton<IChatClient>(_ => openAiClient.GetChatClient(model).AsIChatClient());
}


//// Ollama configuration
//var ollamaEndpoint = builder.Configuration["Ollama:Endpoint"]
//    ?? throw new InvalidOperationException(
//        "Ollama:Endpoint is not configured.");

//var ollamaModel = builder.Configuration["Ollama:Model"]
//    ?? throw new InvalidOperationException(
//        "Ollama:Model is not configured.");

// Register Qwen3 through IChatClient
//builder.Services.AddSingleton<IChatClient>(_ =>
//    new OllamaApiClient(
//        new Uri(ollamaEndpoint),
//        ollamaModel));

// Translation service
builder.Services.AddSingleton<ITranslationService, TranslationService>();
// Speech-to-text and text-to-speech services
//builder.Services.AddSingleton<ISpeechToTextService, SpeechToTextService>();
builder.Services.AddHttpClient<ISpeechToTextService, SpeechToTextService>();

// Text-to-speech service
//builder.Services.AddSingleton<ITextToSpeechService, TextToSpeechService>();
builder.Services.AddHttpClient<ITextToSpeechService, TextToSpeechService>();


// co-ordinate between TTS, STT and Translation service
builder.Services.AddSingleton<ISpeechSessionService, SpeechSessionService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Scalar reads the OpenAPI document above
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();
// 2. Set up the MIME type for Android APKs
var providers = new FileExtensionContentTypeProvider();
providers.Mappings[".apk"] = "application/vnd.android.package-archive";

// 3. Expose the external "apk" folder to the web
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "apk")),
    RequestPath = "/apk",
    ContentTypeProvider = providers
});

app.MapControllers();

app.MapHub<TranslationHub>("/hubs/translation");

app.Run();
