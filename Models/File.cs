namespace SnowBackend.Models;

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class FileTool
{
    public static async Task<List<DataRow>?> ReadFile(IFormFile file)
    {
        var fileInfo = new List<DataRow>();

        try
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split(':');
                    if (parts.Length < 3)
                    {
                        // If line doesn't contain enough parts, log error and skip this line
                        Console.WriteLine($"Invalid line format: {line}");
                        continue;
                    }

                    if (!int.TryParse(parts[2], out int value))
                    {
                        // If the third part is not a valid integer, log error and skip this line
                        Console.WriteLine($"Invalid integer value: {parts[2]}");
                        continue;
                    }

                    fileInfo.Add(new DataRow(parts[0].Substring(1), parts[1], value));
                }
            }
        }
        catch (Exception ex)
        {
            // Log any exception occurred during file reading
            Console.WriteLine($"Error reading file: {ex.Message}");
            return null;
        }

        return fileInfo;
    }
}
