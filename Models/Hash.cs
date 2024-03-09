namespace SnowBackend.Models;

public class HashTool
{
    public static string? CalculateHash (IFormFile file)
    {
        try
        {
        using (var stream = file.OpenReadStream())
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    var hashBytes = md5.ComputeHash(stream);
                    var md5Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    
                    return md5Hash;
                }
            }
                    }
        catch (System.Exception)
        {
            return null;
        }
    }

    public static bool HealthyCheck (IFormFile file, string hash)
    {
        var calculatedHash = CalculateHash(file);

        return hash == calculatedHash;
    }
}