using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Utils;

namespace PoliNetworkBot_CSharp.Code.Objects;

public class TelegramFileContent
{
 public readonly string? FileContent;
 private readonly string? _caption;
 public TelegramFileContent(string? fileContent, string? caption)
 {
  this.FileContent = fileContent;
  this._caption = caption;
 }

 public  async Task<List<MessageSentResult>?> Send(TelegramBotAbstract sender, int loopNumber, string? langCode, long? replyToMessageId2, MessageEventArgs? messageEventArgs)
 {


  if (string.IsNullOrEmpty(this.FileContent) && string.IsNullOrEmpty(this._caption))
  {
   return null;
  }

  if (string.IsNullOrEmpty(this.FileContent))
  {
   var text1 = new Language(new Dictionary<string, string?>
   {
    { "it", "Eccezione! " + this._caption },
    { "en", "Exception! " + this._caption }
   });

   var r11 = await Utils.NotifyUtil.NotifyOwners7(text1, sender, langCode, replyToMessageId2, messageEventArgs, this.FileContent);
   return r11;
  }


  if (string.IsNullOrEmpty(this._caption))
  {
   await NotifyUtil.SendString(
    this.FileContent, messageEventArgs, sender, 
    "ex.json", "", replyToMessageId2
    );
  }
  
  var text = new Language(new Dictionary<string, string?>
  {
   { "it", "Eccezione! " + this._caption },
   { "en", "Exception! " + this._caption }
  });

  var r1 = await Utils.NotifyUtil.NotifyOwners7(text, sender, langCode, replyToMessageId2, messageEventArgs);
  return r1;
 }
}