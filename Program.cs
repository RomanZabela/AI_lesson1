using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernelPlayground.DataLoader;
using SemanticKernelPlayground.Models;
using SemanticKernelPlayground.Plugins;

#pragma warning disable SKEXP0010 
#pragma warning disable SKEXP0001

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
    .Build();

var modelName = configuration["ModelName"] ?? throw new ApplicationException("ModelName not found");
var endpoint = configuration["Endpoint"] ?? throw new ApplicationException("Endpoint not found");
var apiKey = configuration["ApiKey"] ?? throw new ApplicationException("ApiKey not found");
var embedding = configuration["EmbeddingModel"] ?? throw new ApplicationException("ModelName not found");

var builder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(modelName, endpoint, apiKey)
    .AddAzureOpenAITextEmbeddingGeneration(embedding, endpoint, apiKey)
    .AddInMemoryVectorStore();

var kernel = builder.Build();

List<string> fileList = new()
{
    "..\\..\\..\\DataLoader\\DataUploader.cs",
    "..\\..\\..\\DataLoader\\FileReader.cs",
    "..\\..\\..\\Plugins\\GitCommand.cs"
};

var vectorStore = kernel.GetRequiredService<IVectorStore>();
var textEmbedding = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

for (int i = 0 ; i < fileList.Count; i++)
{
    var textChunk = FileReader.ParseFile(fileList[i]);
    var dataUploader = new DataUploader(vectorStore, textEmbedding);
    await dataUploader.UploadToVectorStore("my_code", textChunk);
}

var collection = vectorStore.GetCollection<string, TextChunk>("my_code");

var stringMapper = new TextChunkStringMapper();
var resultMapper = new TextChunkResultMapper();

var textSearch = new VectorStoreTextSearch<TextChunk>(collection, textEmbedding, stringMapper, resultMapper);

var searchPlugin = textSearch.CreateWithGetSearchResults("MyCodePluginSearch");

kernel.Plugins.Add(searchPlugin);
kernel.Plugins.AddFromType<GitCommand>("Commands");

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

AzureOpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var history = new ChatHistory();

do
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Me > ");
    Console.ResetColor();

    var userInput = Console.ReadLine();
    if (userInput == "exit")
    {
        break;
    }

    history.AddUserMessage(userInput!);

    var streamingResponse =
        chatCompletionService.GetStreamingChatMessageContentsAsync(
            history,
            openAiPromptExecutionSettings,
            kernel);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Agent > ");
    Console.ResetColor();

    var fullResponse = "";
    await foreach (var chunk in streamingResponse)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(chunk.Content);
        Console.ResetColor();
        fullResponse += chunk.Content;
    }
    Console.WriteLine();

    history.AddMessage(AuthorRole.Assistant, fullResponse);


} while (true);

#pragma warning restore SKEXP0010
#pragma warning restore SKEXP0001