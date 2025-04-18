using System.IO;

namespace CsvCore.Specs.Helpers;

public static class FileHelper
{
    public static void DeleteTestFile(string filePath)
    {
        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
