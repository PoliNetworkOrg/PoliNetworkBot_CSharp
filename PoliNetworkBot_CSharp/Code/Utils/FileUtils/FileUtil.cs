using System.IO;
using System.Linq;

namespace PoliNetworkBot_CSharp.Code.Utils.FileUtils;

public static class FileUtil
{
    public static string? FindFile(string dbJson, string startingPath = "./", int count = 0)
    {
        while (count < 10)
        {
            var files = FindFiles(startingPath,  dbJson);
            if (!string.IsNullOrEmpty(files))
            {
                return files;
            }

            startingPath += "../";
            count += 1;
        }

        return null;
    }

    private static string? FindFiles(string startingPath, string dbJson)
    {
        try
        {
            var files = Directory.EnumerateFiles(startingPath).Where(x => x.EndsWith(dbJson)).ToList();
            if (files.Any())
                return files[0];
        }
        catch
        {
            return null;
        }

        try
        {
            var folders = Directory.EnumerateDirectories(startingPath);
            foreach (var f in folders)
            {
                try
                {
                    var x2 = FindFiles(f, dbJson);
                    if (!string.IsNullOrEmpty(x2))
                        return x2;
                }
                catch
                {
                    ;
                }
            }
        }
        catch
        {
            ;
        }

        return null;
    }
}