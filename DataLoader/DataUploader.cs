
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernelPlayground.Models;

namespace SemanticKernelPlayground.DataLoader
{
#pragma warning disable SKEXP0001
    public class DataUploader(IVectorStore vectorStore, ITextEmbeddingGenerationService textEmbeddingGeneration)
    {
        public async Task UploadToVectorStore(string collectionName, IEnumerable<TextChunk> textChunk)
        {
            var collection = vectorStore.GetCollection<string, TextChunk>(collectionName);
            await collection.CreateCollectionIfNotExistsAsync();

            foreach (var chunk in textChunk)
            {
                Console.WriteLine($"Generated embeding for paragraph: {chunk.ParagraphId}");
                chunk.TextEmbeding = await textEmbeddingGeneration.GenerateEmbeddingAsync(chunk.Text);

                Console.WriteLine($"Upserting chunk to vector store: {chunk.Key}");
                await collection.UpsertAsync(chunk);
            }
        }
    }
}

#pragma warning restore SKEXP0001