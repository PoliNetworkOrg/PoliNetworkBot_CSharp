using Bot.Enums;
using PoliNetworkBot_CSharp.Code.Bots.Materials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Global;
using Telegram.Bot.Types.ReplyMarkups;

namespace PoliNetworkBot_CSharp.Code.Bots.Materials
{
    static public class Keyboards
    {
        static public List<List<KeyboardButton>> getKeyboard(string[] keyboardList)
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

            List<List<KeyboardButton>> replyKeyboard = new List<List<KeyboardButton>>();

            foreach (var l2 in keyboadToArray)
            {
                List<KeyboardButton> x2 = new List<KeyboardButton>();
                foreach (string l3 in l2)
                {   
                    string[] path = l3.Split(@"/");
                    var len = path.Length;
                    x2.Add(new KeyboardButton(path[len-1]));
                }
                replyKeyboard.Add(x2);
            }
            return replyKeyboard;
        }

        internal static List<List<KeyboardButton>> getKeyboardCorsi(string scuola)
        {
            List<List<KeyboardButton>> r = new List<List<KeyboardButton>>();
            string testo = "";
            foreach (var corso in Navigator.ScuoleCorso[scuola])
            {
                r.Add(new List<KeyboardButton>() { new KeyboardButton() { Text = corso } });
            }
            r.Add(new List<KeyboardButton>() { new KeyboardButton() { Text = "🔙 back" } });
            return r;
        }

        internal static String[] getDir(long id)
        {
            string corso = Program.UsersConversations[id].getcorso();
            if (string.IsNullOrEmpty(corso))
                return null;
            corso = corso.ToLower();
            string root = Program.Config + corso;
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
                subdirectoryEntries = removeGit(subdirectoryEntries);
            }
            return subdirectoryEntries;
        }
        internal static List<List<KeyboardButton>> getPathsKeyboard(long id)
        {
            string[] subdirectoryEntries = getDir(id);
            string percorso = Program.UsersConversations[id].getPercorso();
            List<List<KeyboardButton>> k  =  Keyboards.getKeyboard(subdirectoryEntries);
            if (k == null) { k = new List<List<KeyboardButton>>(); } 
            if (percorso == null)
            {
                k.Insert(0, new List<KeyboardButton>() {
                new KeyboardButton(){  Text = "🔙 back"}
                });
                return k;
            }
            k.Insert(0, new List<KeyboardButton>() { 
                new KeyboardButton(){  Text = "🔙 back"},
                new KeyboardButton(){  Text = "🆗 Cartella Corrente"},
                new KeyboardButton(){  Text = "🆕 New Folder"}
            });
            return k;
        }

        private static string[] removeGit(string[] subdirectoryEntries)
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

        internal static List<List<InlineKeyboardButton>> GetKeyboardSchools()
        {
            List<List<InlineKeyboardButton>> r = new List<List<InlineKeyboardButton>>();
            foreach (var v in Navigator.ScuoleCorso.Keys)
            {
                r.Add(new List<InlineKeyboardButton> { new(text: v ) });
            }
            return r;
        }
    }
}
