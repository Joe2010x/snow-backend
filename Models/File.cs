namespace SnowBackend.Models;

public class FileTool
{
    public static async Task<List<DataRow>?> ReadFile(IFormFile file)
    {
        var fileInfo = new List<DataRow>();

        using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var parts = line.Split(':');
                        if (parts.Length < 3)
                        {
                            return null;
                        }
                        fileInfo.Add(new DataRow(parts[0].Substring(1),parts[1],int.Parse(parts[2])));
                    }
            }
        return fileInfo;
    }
}