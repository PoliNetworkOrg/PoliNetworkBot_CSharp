using System;
using System.IO;

namespace PoliNetworkBot_CSharp.Code.Utils.FileUtils;

public static class FileUtil
{
    public static string? FindFile(string dbJson, string startingPath = "./", int count = 0)
    {
        while (count < 10)
        {
  


            string?[] files = Directory.GetFiles(startingPath, "*" + dbJson, SearchOption.AllDirectories);
            if (files.Length > 1)
            {
                return files[0];
            }

            startingPath += "../";
            count += 1;
        }

        return null;
    }
}