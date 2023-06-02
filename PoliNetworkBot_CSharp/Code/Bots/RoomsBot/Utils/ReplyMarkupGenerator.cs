using System;
using System.Collections.Generic;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Bots.RoomsBot.Utils;

using Telegram.Bot.Types.ReplyMarkups;

public static class ReplyMarkupGenerator
{
    public static ReplyMarkupObject MainKeyboard(string langCode)
    {
        // filter out the main menu options that are not available in the current language
        var choices = langCode == "it" ? new List<string>(Data.Enums.MainMenuOptionsToFunction.Keys) : new List<string>(Data.Enums.MainMenuOptionsToStateEn.Keys);
        return new ReplyMarkupObject(new ReplyMarkupOptions(GenerateKeyboardMarkup(choices, false)), false);
    } 

    public static ReplyMarkupObject CampusKeyboard(string langCode, bool withBackButton = true)
    {
        var campuses = Data.Enums.Campuses.Keys;
        var choices = new List<string>(campuses);
        return new ReplyMarkupObject(new ReplyMarkupOptions(GenerateKeyboardMarkup(choices, withBackButton)), false);
    }

    public const int DaysAmount = 30;
    public static ReplyMarkupObject DateKeyboard()
    {
        // Generate an array of dates from today to 30 days from now in day/month/year format
        var dates = Enumerable.Range(0, DaysAmount).Select(i =>
            DateTime.Now.AddDays(i).ToString("dd/MM/yyyy")).ToArray();
        var choices = new List<string>(dates);
        return new ReplyMarkupObject(new ReplyMarkupOptions(GenerateKeyboardMarkup(choices, true)), false);
    }

    public static ReplyMarkupObject ClassroomsKeyboard(IEnumerable<string> classrooms)
    {
        var choices = new List<string>(classrooms);
        return new ReplyMarkupObject(new ReplyMarkupOptions(GenerateKeyboardMarkup(choices, true)), false);
    }


    public static ReplyMarkupObject HourSelector(int firstHour, int lastHour)
    {
        var hours = Enumerable.Range(firstHour, lastHour-firstHour).ToArray();
        var choices = new List<string>(hours.Select(i => i.ToString()).ToArray());
        var options = new ReplyMarkupOptions(GenerateKeyboardMarkup(choices, 1, true));
        return new ReplyMarkupObject(options, false);
    }

    public static ReplyMarkupObject BackButton()
    {
        return new ReplyMarkupObject(new ReplyMarkupOptions(GenerateKeyboardMarkup(new List<string>(), true)), false);
    }


    /// <summary>
    /// Generates a ReplyKeyboardMarkup dynamically by having a maximum of characters per row.
    /// <br/> The maximum items per row are 3.
    /// </summary>
    /// <param name="items">List of texts to put in buttons.</param>
    /// <param name="hasBackButton">true if markup needs to be generated with a back button; otherwise false.</param>
    /// <returns>Generated ReplyKeyboardMarkup.</returns>
    private static List<List<KeyboardButton?>> GenerateKeyboardMarkup(List<string> items, bool hasBackButton)
    {
        const int maxCharsPerRow = 40;
        const int maxItemsPerRow = 3;
        var buttons = new List<List<KeyboardButton?>>();
        if (hasBackButton)
            buttons.Add(new List<KeyboardButton?> { new("back") });
        var row = new List<KeyboardButton?>();
        var characterCount = 0;
        for (var i = 0; i < items.Count; i++)
        {
            characterCount += items[i].Length;
            if (characterCount > maxCharsPerRow || row.Count >= maxItemsPerRow)
            {
                buttons.Add(row);
                characterCount = items[i].Length;
                row = new List<KeyboardButton?> { new(items[i]) };
            }
            else
            {
                row.Add(new KeyboardButton(items[i]));
            }

            if (i == items.Count - 1)
                buttons.Add(row);
        }

        return buttons;
    }

    /// <summary>
    /// Generates a ReplyKeyboardMarkUp with the requested amount of items per row.
    /// </summary>
    /// <param name="items">List of texts to put in buttons.</param>
    /// <param name="itemsPerRow">Max amount of items in one row.</param>
    /// <param name="hasBackButton">true if markup needs to be generated with a back button; otherwise false.</param>
    /// <returns>Generated ReplyKeyboardMarkup.</returns>
    private static List<List<KeyboardButton?>> GenerateKeyboardMarkup(List<string> items, int itemsPerRow,
        bool hasBackButton)
    {
        var buttons = new List<List<KeyboardButton?>>();
        if (hasBackButton)
            buttons.Add(new List<KeyboardButton?> { new("back") });
        var row = new List<KeyboardButton?>();
        for (var i = 0; i < items.Count; i++)
        {
            row.Add(new KeyboardButton(items[i]));
            if ((i + 1) % itemsPerRow != 0 && i != items.Count - 1)
                continue;
            buttons.Add(row);
            row = new List<KeyboardButton?>();
        }

        return buttons;
    }
}