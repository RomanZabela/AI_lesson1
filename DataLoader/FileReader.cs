using SemanticKernelPlayground.Models;

namespace SemanticKernelPlayground.DataLoader
{
    public class FileReader
    {
        public static IEnumerable<TextChunk> ParseFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var fileName = Path.GetFileName(filePath);

            var chunk = new List<TextChunk>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (string.IsNullOrWhiteSpace(line)) continue;

                var paragraphId = i + 1;

                var key = $"{fileName}_{paragraphId}";

                chunk.Add(new()
                {
                    Key = key,
                    DocumentName = fileName,
                    ParagraphId = paragraphId,
                    Text = line,
                    TextEmbeding = ReadOnlyMemory<float>.Empty
                });
            }

            return chunk;
        }
    }
}
