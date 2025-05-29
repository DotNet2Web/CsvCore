using System.IO;

namespace CsvCore.Specs.Helpers;

public static class FileHelper
{
    public static void DeleteTestFile(string filePath)
    {
        if(File.Exists(filePath) && !IsFileLocked(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static bool IsFileLocked(string filePath)
    {
        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }
}
