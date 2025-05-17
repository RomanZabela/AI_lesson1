using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using SemanticKernelPlayground.Models;

namespace SemanticKernelPlayground.Models
{
    public class TextChunk
    {
        [VectorStoreRecordKey]
        public required string Key { get; init; }

        [VectorStoreRecordData]
        public required string DocumentName { get; init; }

        [VectorStoreRecordData]
        public required int ParagraphId { get; init; }

        [VectorStoreRecordData]
        public required string Text { get; init; }

        [VectorStoreRecordVector(1536)]
        public ReadOnlyMemory<float> TextEmbeding { get; set; }
    }
}

sealed class TextChunkStringMapper : ITextSearchStringMapper
{
    public string MapFromResultToString(object result)
    {
        if (result is TextChunk dataModel)
        {
            return dataModel.Text;
        }

        throw new ArgumentException("Invalid type result!");
    }
}

sealed class TextChunkResultMapper : ITextSearchResultMapper
{
    public TextSearchResult MapFromResultToTextSearchResult(object result)
    {
        if (result is TextChunk dataModel)
        {
            return new(value: dataModel.Text)
            {
                Name = dataModel.Key,
                Link = dataModel.DocumentName
            };
        }

        throw new ArgumentException("Invalid type result!");
    }
}
