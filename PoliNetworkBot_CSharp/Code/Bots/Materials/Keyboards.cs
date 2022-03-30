#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Enums;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Global;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials;

public static class Keyboards
{
    internal static IEnumerable<List<Language>> GetKeyboardCorsi(string scuola)
    {
        var options2 = new List<Language>();
        if (Navigator.ScuoleCorso[scuola] != null)
            options2.AddRange(Navigator.ScuoleCorso[scuola].Select(corso =>
                new Language(new Dictionary<string, string> { { "it", corso }, { "en", corso } })));
        options2.Add(new Language(new Dictionary<string, string>
        {
            { "it", "🔙 Indietro" },
            { "en", "🔙 Back" }
        }));
        return Code.Utils.KeyboardMarkup.ArrayToMatrixString(options2);
    }

    internal static string[] GetDir(long id)
    {
        var corso = Program.UsersConversations[id].GetCourse();
        if (string.IsNullOrEmpty(corso))
            return null;
        corso = corso.ToLower();
        var root = Program.Config.RootDir + corso;
        var percorso = Program.UsersConversations[id].GetPath();
        if (!string.IsNullOrEmpty(percorso)) root += @"/" + percorso;
        string[] subdirectoryEntries = null;
        if (Program.UsersConversations[id].GetState() != UserState.NEW_FOLDER)
            subdirectoryEntries = Directory.GetDirectories(root);
        if (subdirectoryEntries != null) subdirectoryEntries = RemoveGit(subdirectoryEntries);
        return subdirectoryEntries;
    }

    internal static List<List<Language>> GetPathsKeyboard(long id)
    {
        var subdirectoryEntries = GetDir(id);
        var percorso = Program.UsersConversations[id].GetPath();
        Logger.WriteLine("User " + id + " trying to get path: " + percorso + " SubDir: " +
                         subdirectoryEntries.Aggregate("", (current, s) => current + s + ";"));
        var options2 = subdirectoryEntries.Select(v => new Language(new Dictionary<string, string>
            { { "it", v.Split("/").Last() }, { "en", v.Split("/").Last() } })).ToList();
        if (percorso == null)
        {
            options2.Add(new Language(new Dictionary<string, string>
            {
                { "it", "🔙 back" },
                { "en", "🔙 back" }
            }));
            return Code.Utils.KeyboardMarkup.ArrayToMatrixString(options2);
        }

        options2.Add(new Language(new Dictionary<string, string>
        {
            { "it", "🔙 Indietro" },
            { "en", "🔙 Back" }
        }));
        options2.Add(new Language(new Dictionary<string, string>
        {
            { "it", "🆗 Cartella Corrente" },
            { "en", "🆗 Current Folder" }
        }));
        options2.Add(new Language(new Dictionary<string, string>
        {
            { "it", "🆕 Nuova Cartella" },
            { "en", "🆕 New Folder" }
        }));
        return Code.Utils.KeyboardMarkup.ArrayToMatrixString(options2);
    }

    private static string[] RemoveGit(IEnumerable<string> subdirectoryEntries)
    {
        var listadir = subdirectoryEntries.ToList();
        for (var i = 0; i < listadir.Count; i++)
            if (listadir[i].Contains(".git"))
            {
                listadir.Remove(listadir[i]);
                i--;
            }

        return listadir.ToArray();
    }

    internal static IEnumerable<List<Language>> GetKeyboardSchools()
    {
        var options2 = Navigator.ScuoleCorso.Keys
            .Select(v => new Language(new Dictionary<string, string> { { "it", v }, { "en", v } })).ToList();
        //r.Add(new List<InlineKeyboardButton> { new(text: v ) });
        return Code.Utils.KeyboardMarkup.ArrayToMatrixString(options2);
    }
}