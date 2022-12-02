#region

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;

#endregion

namespace PoliNetworkBot_CSharp.Code.Config;

public static class NewConfig
{
    public static void NewConfigMethod(bool resetBot, bool resetUserBot, bool resetBotDisguisedAsUserBot,
        bool destroyDbAndRedoIt, bool alsoFillTablesFromJson)
    {
        if (resetBot) ResetBotMethod(BotTypeApi.REAL_BOT);

        if (resetUserBot) ResetUserbotMethod(BotTypeApi.USER_BOT);

        if (resetBotDisguisedAsUserBot) ResetBotDisguisedAsUserBotMethod(BotTypeApi.DISGUISED_BOT);

        if (destroyDbAndRedoIt)
            DestroyDB_And_Redo_it(alsoFillTablesFromJson);
    }

    private static void ResetBotDisguisedAsUserBotMethod(BotTypeApi b)
    {
        Reset(Paths.Info.ConfigBotDisguisedAsUserBotsInfo, b);
    }

    private static void ResetUserbotMethod(BotTypeApi b)
    {
        Reset(Paths.Info.ConfigUserBotsInfo, b);
    }

    private static void ResetBotMethod(BotTypeApi b)
    {
        Reset(Paths.Info.ConfigBotsInfo, b);
    }

    private static void Reset(string configBotsInfo, BotTypeApi b)
    {
        try
        {
            BotConfig t = new()
            {
                bots = new List<BotInfoAbstract>
                {
                    new()
                }
            };
            t.bots[0].botTypeApi = b;
            var j = JsonConvert.SerializeObject(t);
            File.WriteAllText(configBotsInfo, j);
        }
        catch (Exception e)
        {
            Logger.WriteLine(e.Message);
        }
    }

    private static void DestroyDB_And_Redo_it(bool alsoFillTablesFromJson)
    {
        DirectoryUtils.CreateDirectory("data");

        var dbPath = Paths.Db;
        dbPath = dbPath.Split('=')[1];
        try
        {
            File.WriteAllText(dbPath, "");
        }
        catch
        {
            // ignored
        }

        try
        {
            CleanDb();
        }
        catch (Exception e)
        {
            Logger.WriteLine(e);
            Logger.WriteLine("Skipping CleanDB");
        }

        Redo_DB(alsoFillTablesFromJson);
    }

    private static void CleanDb()
    {
        const string? s = "SELECT name FROM sqlite_master WHERE type='table'";
        var r1 = Database.ExecuteSelect(s, GlobalVariables.DbConfig);
        if (r1 == null)
            return;

        foreach (DataRow dr in r1.Rows)
        {
            var name = dr.ItemArray[0]?.ToString();
            if (name != null && name.StartsWith("sqlite_"))
                continue;

            var q = "DROP TABLE IF EXISTS " + name;
            Database.Execute(q, GlobalVariables.DbConfig);
        }
    }

    private static void Redo_DB(bool alsoFillTablesFromJson)
    {
        Database.Execute("CREATE TABLE GroupsTelegram (" +
                         "id BIGINT PRIMARY KEY, " +
                         "bot_id INT(12)," +
                         "valid CHAR(1)," +
                         "link VARCHAR(250)," +
                         "last_update_link DATETIME," +
                         "type VARCHAR(250)," +
                         "title VARCHAR(250)" +
                         ") ", GlobalVariables.DbConfig);

        if (alsoFillTablesFromJson)
            _ = FillGroups(0);

        Database.Execute("CREATE TABLE PeopleInEntities (" +
                         "id_entity INT(12)," +
                         "id_person INT(12)," +
                         "CONSTRAINT PK_Person PRIMARY KEY (id_entity,id_person)" +
                         ");", GlobalVariables.DbConfig);

        Database.Execute("CREATE TABLE Entities (" +
                         "id INT(12) PRIMARY KEY," +
                         "name VARCHAR(250)" +
                         ");", GlobalVariables.DbConfig);

        if (alsoFillTablesFromJson)
            _ = FillAssoc(GlobalVariables.DbConfig);

        Database.Execute("CREATE TABLE Messages (" +
                         "id INT(12) PRIMARY KEY," +
                         "from_id_person INT(12)," +
                         "from_id_entity INT(12)," +
                         "type INT(12)," +
                         "id_photo INT(12)," +
                         "id_video INT(12)," +
                         "id_file INT(12)," +
                         "id_voice INT(12)," +
                         "id_audio INT(12)," +
                         "message_text TEXT," +
                         "sent_date DATETIME," +
                         "has_been_sent BOOLEAN," +
                         "message_id_tg_from INT(12)," +
                         "message_id_tg_to INT(12)," +
                         "id_chat_sent_into BIGINT," +
                         "from_id_bot INT(12)," +
                         "type_chat_sent_into VARCHAR(250)" +
                         ");", GlobalVariables.DbConfig);

        Database.Execute("CREATE TABLE MessageTypes (" +
                         "id INT(12) PRIMARY KEY," +
                         "name VARCHAR(250)" +
                         ");", GlobalVariables.DbConfig);

        Database.Execute("CREATE TABLE Photos (" +
                         "id_photo INT(12) PRIMARY KEY," +
                         "file_id VARCHAR(2500)," +
                         "file_size INT(12)," +
                         "height INT(12)," +
                         "width INT(12)," +
                         "unique_id VARCHAR(250)" +
                         ");", GlobalVariables.DbConfig);

        Database.Execute("CREATE TABLE Videos (" +
                         "id_video INT(12) PRIMARY KEY," +
                         "file_id VARCHAR(2500)," +
                         "file_size INT(12)," +
                         "height INT(12)," +
                         "width INT(12)," +
                         "unique_id VARCHAR(250)," +
                         "duration INT," +
                         "mime VARCHAR(250)" +
                         ");", GlobalVariables.DbConfig);

        Database.Execute(@"CREATE TABLE IF NOT EXISTS `LogTable` (
  `log_id` int NOT NULL,
  `bot_id` bigint NOT NULL,
  `when_insert` datetime NOT NULL,
  `severity` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `stacktrace` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`log_id`,`bot_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
", GlobalVariables.DbConfig);


        Database.Execute(@"
DELIMITER //
CREATE PROCEDURE `insert_log`(
	IN `in_bot_id` BIGINT,
	IN `in_severity` VARCHAR(50),
	IN `in_content` TEXT,
	IN `in_stracktrace` TEXT
)
    MODIFIES SQL DATA
BEGIN
	SELECT COUNT(*)
	INTO @num_rows
	FROM Bot b 
	WHERE b.bot_id = in_bot_id;
	
	IF @num_rows IS NULL OR @num_rows < 1 THEN
		INSERT INTO Bot (bot_id, log_row) VALUES (in_bot_id, 0);
	END IF;
	
	SELECT MOD((b.log_row +1),10000)
	INTO @curr_count
	FROM Bot b 
	WHERE b.bot_id = in_bot_id;
	
	UPDATE Bot b
		SET b.log_row = @curr_count
		WHERE b.bot_id = in_bot_id;
	
	REPLACE INTO LogTable (bot_id, log_id, content, when_insert, severity, stacktrace) 
		VALUES (in_bot_id, @curr_count, in_content, NOW(), in_severity, in_stracktrace);
	

END//
DELIMITER ;


", GlobalVariables.DbConfig);

        Database.Execute(@"CREATE TABLE IF NOT EXISTS `Bot` (
  `bot_id` bigint NOT NULL,
  `log_row` int DEFAULT NULL,
  PRIMARY KEY (`bot_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

", GlobalVariables.DbConfig);
    }

    private static List<GroupAddedResult>? FillGroups(int botIdWhoInsertedThem)
    {
        var r15 = new List<GroupAddedResult>();

        //read groups from polinetwork python config file and fill db
        try
        {
            var s = File.ReadAllText("../../../Old/data/groups.json");
            var r = JsonConvert.DeserializeObject<JObject>(s);
            if (r == null)
                return null;

            var r2 = r.Children();
            foreach (var r3 in r2)
                if (r3 is JProperty { Name: "Gruppi" } r4)
                {
                    var r5 = r4.Children();
                    foreach (var r6 in r5)
                    {
                        var r7 = r6.Children();
                        foreach (var r8 in r7)
                            if (r8 is JObject r9)
                            {
                                var r10 = r9.Children();

                                var r11 = AddGroupToDb(r10, botIdWhoInsertedThem);
                                r15.Add(r11);
                            }
                    }
                }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e);
            Logger.WriteLine("Skipping Old bot groups import");
        }
        catch (DirectoryNotFoundException e)
        {
            Console.WriteLine(e);
            Logger.WriteLine("Skipping Old bot groups import");
        }

        return r15;
    }

    private static GroupAddedResult AddGroupToDb(JEnumerable<JToken> r1, int botIdWhoInsertedThem)
    {
        ChatJson? chat = null;
        DateTime? lastUpdateLinkTime = null;
        bool? weAreAdmin = null;

        var exceptions = new List<Exception>();
        foreach (var r2 in r1)
        {
            if (r2 is not JProperty r3) continue;
            switch (r3.Name)
            {
                case "Chat":
                    chat = GetChatFromJson(r3);
                    break;

                case "LastUpdateInviteLinkTime":
                {
                    var d1 = GetLastUpdateLinkTimeFromJson(r3);
                    if (d1 != null && d1.HasValue())
                    {
                        lastUpdateLinkTime = d1.GetValue();
                    }
                    else
                    {
                        var e2 = d1?.GetExceptions();
                        if (e2 != null)
                            exceptions.AddRange(e2!);
                    }

                    break;
                }
                case "we_are_admin":
                    weAreAdmin = GetWeAreAdminFromJson(r3);
                    break;
            }
        }

        var d2 = AddGroupToDb2(chat, lastUpdateLinkTime, weAreAdmin, botIdWhoInsertedThem);
        return new GroupAddedResult(d2, exceptions);
    }

    private static bool AddGroupToDb2(ChatJson? chat, DateTime? lastUpdateLinkTime, bool? weAreAdmin,
        int botIdWhoInsertedThem)
    {
        try
        {
            const string? q1 = "INSERT INTO GroupsTelegram (id, bot_id, type, title, link, last_update_link, valid) " +
                               " VALUES " +
                               " (@id, @botid, @type, @title, @link, @lul, @valid)";
            if (chat != null)
                Database.Execute(q1, GlobalVariables.DbConfig, new Dictionary<string, object?>
                {
                    { "@id", chat.id },
                    { "@botid", botIdWhoInsertedThem },
                    { "@type", chat.type },
                    { "@title", chat.title },
                    { "@link", chat.invite_link },
                    { "@lul", InviteLinks.GetDateTimeLastUpdateLinkFormattedString(lastUpdateLinkTime) },
                    { "@valid", weAreAdmin }
                });
        }
        catch
        {
            return false;
        }

        return true;
    }

    private static bool? GetWeAreAdminFromJson(JToken r3)
    {
        var r4 = r3.First;
        return r4 is not JValue r5 ? null : r5.Value == null ? null : r5.Value.ToString()?.ToLower() == "true";
    }

    private static ValueWithException<DateTime?>? GetLastUpdateLinkTimeFromJson(JToken r3)
    {
        var r4 = r3.First;
        return r4 is not JValue r5 ? new ValueWithException<DateTime?>(null, new JsonDateTimeNotFound()) :
            r5.Value == null ? null : DateTimeClass.GetFromString(r5.Value.ToString());
    }

    private static ChatJson GetChatFromJson(JToken r3)
    {
        var r4 = r3.Children();
        var r5 = r4.Children();

        long? id = null;
        string? type = null;
        string? title = null;
        string? inviteLink = null;

        foreach (var r6 in r5)
            if (r6 is JProperty r7)
                switch (r7.Name)
                {
                    case "id":
                        id = GetIdFromJson(r7);
                        break;

                    case "type":
                        type = GetTypeFromJson(r7);
                        break;

                    case "title":
                        title = GetTitleFromJson(r7);
                        break;

                    case "invite_link":
                        inviteLink = GetInviteLinkFromJson(r7);
                        break;

                    default:
                        Logger.WriteLine(r7);
                        break;
                }

        return new ChatJson(id, type, title, inviteLink);
    }

    private static string? GetInviteLinkFromJson(JToken r7)
    {
        var r8 = r7.First;
        if (r8 is JValue r9) return r9.Value?.ToString();

        return null;
    }

    private static string? GetTitleFromJson(JToken r7)
    {
        var r8 = r7.First;
        return r8 is not JValue r9 ? null : r9.Value?.ToString();
    }

    private static string? GetTypeFromJson(JToken r7)
    {
        var r8 = r7.First;
        return r8 is not JValue r9 ? null : r9.Value?.ToString();
    }

    private static long? GetIdFromJson(JToken r7)
    {
        var r8 = r7.First;

        if (r8 is not JValue r9) return null;
        try
        {
            return Convert.ToInt64(r9.Value);
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static List<bool>? FillAssoc(DbConfigConnection? dbConfig)
    {
        var r10 = new List<bool>();
        try
        {
            //read assoc from polinetwork python config file and fill db
            var s = File.ReadAllText("../../../Old/config/assoc.json");
            var r = JsonConvert.DeserializeObject<JObject>(s);
            if (r == null) return null;
            var r2 = r.Children();
            foreach (var r3 in r2)
                if (r3 is JProperty r4)
                {
                    var name = r4.Name;
                    var r5 = r4.Value;
                    var users = GetUsersFromAssocJson(r5);
                    var r6 = AddAssocToDb(name, users, dbConfig);
                    r10.Add(r6);
                }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e);
            Logger.WriteLine("Skipping assoc.json import");
        }
        catch (DirectoryNotFoundException e)
        {
            Console.WriteLine(e);
            Logger.WriteLine("Skipping assoc.json import");
        }

        return r10;
    }

    private static bool AddAssocToDb(string? name, IReadOnlyCollection<long>? users, DbConfigConnection? dbConfig)
    {
        const string? q1 = "INSERT INTO Entities (Name) VALUES (@name)";
        _ = Database.Execute(q1, GlobalVariables.DbConfig, new Dictionary<string, object?> { { "@name", name } });

        Tables.FixIdTable("Entities", "id", "name", dbConfig);

        const string? q2 = "SELECT id FROM Entities WHERE Name = @name";
        var r2 = Database.ExecuteSelect(q2, GlobalVariables.DbConfig,
            new Dictionary<string, object?> { { "@name", name } });

        var r3 = Database.GetFirstValueFromDataTable(r2);
        long? r4 = null;
        try
        {
            r4 = Convert.ToInt64(r3);
        }
        catch
        {
            // ignored
        }

        if (r4 == null)
            return false;

        if (users == null)
            return true;

        if (users.Count == 0)
            return true;

        foreach (var u in users)
        {
            const string? q3 = "INSERT INTO PeopleInEntities (id_entity, id_person) VALUES (@ide, @idp)";
            _ = Database.Execute(q3, GlobalVariables.DbConfig,
                new Dictionary<string, object?> { { "@ide", r4.Value }, { "@idp", u } });
        }

        return true;
    }

    private static List<long>? GetUsersFromAssocJson(JToken r1)
    {
        var r2 = r1.Children();
        foreach (var r3 in r2)
            if (r3 is JProperty { Name: "users" } r4)
            {
                var r5 = r4.Value;
                if (r5 is not JArray r6)
                    continue;

                var users = new List<long>();
                foreach (var r7 in r6)
                    if (r7 is JValue r8)
                        users.Add(Convert.ToInt64(r8.Value));
                return users;
            }

        return null;
    }
}