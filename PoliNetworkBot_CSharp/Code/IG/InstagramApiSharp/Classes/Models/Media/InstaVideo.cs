#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaVideo
{
    public InstaVideo()
    {
    }

    public InstaVideo(string url, int width, int height, int type = 3)
    {
        Uri = url;
        Width = width;
        Height = height;
        Type = type;
    }

    public string Uri { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public int Type { get; set; }

    internal string UploadId { get; set; }

    public double Length { get; set; } = 0;

    [JsonIgnore] public byte[] VideoBytes { get; set; }
}