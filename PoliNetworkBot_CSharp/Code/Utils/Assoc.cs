#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class Assoc
    {
        internal static async Task<int?> GetIdEntityFromPersonAsync(int id, Language question,
            TelegramBotAbstract sender, string lang, string username)
        {
            const string q =
                "SELECT Entities.id, Entities.name FROM (SELECT * FROM PeopleInEntities WHERE id_person = @idp) AS T1, Entities WHERE T1.id_entity = Entities.id";
            var r = SqLite.ExecuteSelect(q, new Dictionary<string, object> {{"@idp", id}});
            if (r == null || r.Rows.Count == 0) return null;

            if (r.Rows.Count == 1) return Convert.ToInt32(r.Rows[0].ItemArray[0]);

            var l = new Dictionary<string, int>();
            foreach (DataRow dr in r.Rows)
            {
                var s = dr.ItemArray[1].ToString();
                if (!string.IsNullOrEmpty(s)) l[s] = Convert.ToInt32(dr.ItemArray[0]);
            }

            var l3 = l.Keys.Select(
                l2 => new Language(
                    new Dictionary<string, string>
                    {
                        {"en", l2}
                    })
            ).ToList();

            var options = KeyboardMarkup.ArrayToMatrixString(l3);
            var r2 = await AskUser.AskBetweenRangeAsync(id, question, sender, lang,
                options, username);

            return l[r2];
        }

        public static async Task<bool> Assoc_SendAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var replyTo = e.Message.ReplyToMessage;

            var languageList = new Language(new Dictionary<string, string>
            {
                {"it", "Scegli l'entità per il quale stai componendo il messaggio"},
                {"en", "Choose the entity you are writing this message for"}
            });

            var messageFromIdEntity = await GetIdEntityFromPersonAsync(e.Message.From.Id, languageList,
                sender, e.Message.From.LanguageCode, e.Message.From.Username);

            if (messageFromIdEntity == null)
            {
                var languageList3 = new Language(new Dictionary<string, string>
                {
                    {
                        "en",
                        "We can't find the entity you want to post from. Are you sure you are a member of some entity allowed to post?"
                    },
                    {
                        "it",
                        "Non riusciamo a trovare l'organizzazione per la quale vuoi postare. Sei sicuro di essere un membro di qualche organizzazione autorizzata a postare?"
                    }
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, languageList3, ChatType.Private, default,
                    ParseMode.Default, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username);
                return false;
            }

            var hasThisEntityAlreadyReachedItsLimit = CheckIfEntityReachedItsMaxLimit(messageFromIdEntity.Value);
            if (hasThisEntityAlreadyReachedItsLimit)
            {
                var languageList4 = new Language(new Dictionary<string, string>
                {
                    {"it", "Spiacente! In questo periodo hai inviato troppi messaggi"},
                    {"en", "I'm sorry! In this period you have sent too many messages"}
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, languageList4, ChatType.Private, default,
                    ParseMode.Default, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username);
                return false;
            }

            var languageList2 = new Language(new Dictionary<string, string>
                {
                    {"it", "Data di pubblicazione?"},
                    {"en", "Date of pubblication?"}
                }
            );

            var opt1 = new Language(new Dictionary<string, string> {{"it", "Metti in coda"}, {"en", "Place in queue"}});
            var opt2 = new Language(
                new Dictionary<string, string> {{"it", "Scegli la data"}, {"en", "Choose the date"}});
            var options = new List<List<Language>>
            {
                new List<Language> {opt1, opt2}
            };

            var queueOrPreciseDate = await AskUser.AskBetweenRangeAsync(e.Message.From.Id,
                languageList2, sender, e.Message.From.LanguageCode, options, e.Message.From.Username);

            DateTime? sentDate = null;
            if (Language.EqualsLang(queueOrPreciseDate, options[0][0], e.Message.From.LanguageCode))
                sentDate = null;
            else
                sentDate = await DateTimeClass.AskDateAsync(e.Message.From.Id, e.Message.Text,
                    e.Message.From.LanguageCode, sender, e.Message.From.Username);


            const long idChatSentInto = Channels.PoliAssociazioni;

            if (replyTo.Photo != null)
            {
                var photoLarge = UtilsPhoto.GetLargest(replyTo.Photo);
                var photoIdDb = UtilsPhoto.AddPhotoToDb(photoLarge);
                if (photoIdDb == null)
                    return false;

                MessageDb.AddMessage(MessageType.Photo,
                    replyTo.Caption, e.Message.From.Id,
                    messageFromIdEntity, photoIdDb.Value,
                    idChatSentInto, sentDate, false,
                    sender.GetId(), replyTo.MessageId);
            }
            else
            {
                var lang2 = new Language(new Dictionary<string, string>
                {
                    {"en", "You have to attach something! (A photo, for example)"},
                    {"it", "Devi allegare qualcosa! (Una foto, ad esempio)"}
                });
                await sender.SendTextMessageAsync(e.Message.From.Id,
                    lang2,
                    ChatType.Private, e.Message.From.LanguageCode,
                    ParseMode.Default,
                    new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username);
                return false;
            }

            var lang3 = new Language(new Dictionary<string, string>
            {
                {"en", "The message has been submitted correctly"},
                {"it", "Il messaggio è stato inviato correttamente"}
            });
            await sender.SendTextMessageAsync(e.Message.From.Id, lang3,
                ChatType.Private, e.Message.From.LanguageCode,
                ParseMode.Default, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                e.Message.From.Username);
            return true;
        }

        private static bool CheckIfEntityReachedItsMaxLimit(int messageFromIdEntity)
        {
            return false; //todo: do a sql query to check how many messages have been already queued by that entity
        }
    }
}