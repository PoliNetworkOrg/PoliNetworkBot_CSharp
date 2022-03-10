using Bot.Enums;
using PoliNetworkBot_CSharp.Code.Bots.Materials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Global;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.ReplyMarkups;

namespace PoliNetworkBot_CSharp.Code.Bots.Materials
{
    static public class Keyboards
    {
        static public List<List<InlineKeyboardButton>> GetKeyboard(string[] keyboardList)
        { 
            int i = 0;

            if (keyboardList == null)
            {
                return null;
            }
            List<string> keyboardToList = keyboardList.ToList();

            List<List<string>> keyboadToArray = KeyboardMarkup.ArrayToMatrixString(keyboardToList);

     
            if (keyboadToArray == null || keyboadToArray.Count == 0)
                return null;

            List<List<InlineKeyboardButton>> replyKeyboard = new List<List<InlineKeyboardButton>>();

            foreach (var l2 in keyboadToArray)
            {
                List<InlineKeyboardButton> x2 = new List<InlineKeyboardButton>();
                foreach (string l3 in l2)
                {   
                    string[] path = l3.Split(@"/");
                    var len = path.Length;
                    x2.Add(new InlineKeyboardButton(path[len-1]));
                }
                replyKeyboard.Add(x2);
            }
            return replyKeyboard;
        }

        internal static List<List<Language>> GetKeyboardCorsi(string scuola)
        {
            var r = new List<List<InlineKeyboardButton>>();
            string testo = "";
            var options2 = new List<Language> ();
            foreach (var corso in Navigator.ScuoleCorso[scuola])
            {
                options2.Add(new(new Dictionary<string, string>
                {
                    { "it", corso },
                    { "en", corso }
                }));
            }
            options2.Add(new(new Dictionary<string, string>
            {
                { "it", "🔙 Indietro" },
                { "en", "🔙 Back" }
            }));
            return Code.Utils.KeyboardMarkup.ArrayToMatrixString(options2);
        }

        internal static String[] GetDir(long id)
        {
            string corso = Program.UsersConversations[id].getcorso();
            if (string.IsNullOrEmpty(corso))
                return null;
            corso = corso.ToLower();
            string root = Program.Config.RootDir + corso;
            string percorso = Program.UsersConversations[id].getPercorso();
            if (!string.IsNullOrEmpty(percorso))
            {
                root += @"/" + percorso;
            }
            string[] subdirectoryEntries = null;
            if (Program.UsersConversations[id].getStato() != stati.newCartella)
            {
                subdirectoryEntries = Directory.GetDirectories(root);
            }
            if (subdirectoryEntries != null)
            {
                subdirectoryEntries = RemoveGit(subdirectoryEntries);
            }
            return subdirectoryEntries;
        }
        
        internal static List<List<Language>> GetPathsKeyboard(long id)
        {
            var options2 = new List<Language> ();
            string[] subdirectoryEntries = GetDir(id);
            string percorso = Program.UsersConversations[id].getPercorso();
            Logger.WriteLine("Trying to get path: " + percorso);
            if (percorso == null)
            {
                options2.Add(new(new Dictionary<string, string>
                {
                    { "it", "🔙 back" },
                    { "en", "🔙 back" }
                }));
                return Code.Utils.KeyboardMarkup.ArrayToMatrixString(options2);
            }
            foreach (var v in subdirectoryEntries)
            {
                options2.Add(new Language(new Dictionary<string, string>
                {
                    { "it", v },
                    { "en", v }
                }));
            }
            options2.Add(new(new Dictionary<string, string>
            {
                { "it", "🔙 Indietro" },
                { "en", "🔙 Back" }
            }));
            options2.Add(new(new Dictionary<string, string>
            {
                { "it", "🆗 Cartella Corrente" },
                { "en", "🆗 Current Folder" }
            }));
            options2.Add(new(new Dictionary<string, string>
            {
                { "it", "🆕 Nuova Cartella" },
                { "en", "🆕 New Folder" }
            }));
            return Code.Utils.KeyboardMarkup.ArrayToMatrixString(options2);
        }

        private static string[] RemoveGit(string[] subdirectoryEntries)
        {
            List<String> listadir = subdirectoryEntries.ToList();
            for (int i = 0; i < listadir.Count(); i++)
            {
                if (listadir[i].Contains(".git"))
                {
                    listadir.Remove(listadir[i]);
                    i--;
                }
            }
            return listadir.ToArray();
        }

        internal static List<List<Language>> GetKeyboardSchools()
        {
            var options2 = new List<Language> ();
            foreach (var v in Navigator.ScuoleCorso.Keys)
            {
                options2.Add(new(new Dictionary<string, string>
                {
                    { "it", v },
                    { "en", v }
                }));
                //r.Add(new List<InlineKeyboardButton> { new(text: v ) });
            }
            return Code.Utils.KeyboardMarkup.ArrayToMatrixString(options2);
        }
    }
}
