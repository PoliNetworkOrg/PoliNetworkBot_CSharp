#region

using System;
using System.IO;
using System.Threading.Tasks;

#endregion

namespace Minista.Helpers;

internal class StorageFile
{
    private StorageFile(string path)
    {
        Path = path;
    }

    public string Path { get; set; }

    public static Task<StorageFile> GetFileFromPathAsync(string testJpg)
    {
        return Task.FromResult(new StorageFile(testJpg));
    }

    public Task<Stream> OpenAsync(FileAccessMode read)
    {
        switch (read)
        {
            case FileAccessMode.Read:
                var x = File.OpenRead(Path);
                return Task.FromResult<Stream>(x);

            default:
                throw new ArgumentOutOfRangeException(nameof(read), read, null);
        }
    }
}