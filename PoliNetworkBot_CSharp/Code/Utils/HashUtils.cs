#region

using System.Security.Cryptography;
using System.Text;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class HashUtils
{
    public static string? GetHashOf(string? message)
    {
        if (message == null) return null;
        var bytes = Encoding.Unicode.GetBytes(message);
        var hash = ByteArrayToString(MD5.Create().ComputeHash(bytes));
        return hash;
    }

    private static string ByteArrayToString(byte[] arrInput)
    {
        int i;
        var sOutput = new StringBuilder(arrInput.Length);
        for (i = 0; i < arrInput.Length - 1; i++)
            sOutput.Append(arrInput[i].ToString("X2"));
        return sOutput.ToString();
    }
}