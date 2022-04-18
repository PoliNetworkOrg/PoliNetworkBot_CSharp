namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Media;

public class InstaPosition
{
    public InstaPosition(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double X { get; set; }
    public double Y { get; set; }
}