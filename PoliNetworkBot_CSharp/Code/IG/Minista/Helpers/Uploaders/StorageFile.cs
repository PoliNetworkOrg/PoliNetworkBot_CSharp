#region

using System;
using System.IO;
using System.Threading.Tasks;

#endregion

namespace Minista.Helpers
{
    internal class StorageFile
    {
        public StorageFile(string path)
        {
            Path = path;
        }

        public string Path { get; set; }

        public static async Task<StorageFile> GetFileFromPathAsync(string testJpg)
        {
            throw new NotImplementedException();
        }


        public Task<Stream> OpenAsync(FileAccessMode read)
        {
            switch (read)
            {
                case FileAccessMode.Read:
                    var x = File.OpenRead(Path);
                    return Task.FromResult<Stream>(x);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(read), read, null);
            }

            return Task.FromResult<Stream>(null);
        }
    }
}