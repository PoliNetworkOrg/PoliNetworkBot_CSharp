using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Data;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class BanUnbanAllResult
    {
        private List<DataRow> done;
        private List<DataRow> failed;

        public BanUnbanAllResult(List<DataRow> done, List<DataRow> failed)
        {
            this.done = done;
            this.failed = failed;
        }

        internal Language GetLanguage(bool ban_true_unban_false, string target)
        {
            if (ban_true_unban_false == false)
            {
                var text2 = new Language(new Dictionary<string, string>
                {
                    {"en", "Target "+target+" unbanned from " + done.Count + " groups" + "\n" + "not unbanned from " + failed.Count + " groups"},
                    {"it", "Target "+target+" sbannato da " + done.Count + " gruppi" + "\n" + "non sbannato da " + failed.Count + " gruppi" }
                });

                return text2;
            }
            else
            {
                var text2 = new Language(new Dictionary<string, string>
                    {
                        {"en", "Target "+target+" banned from " + done.Count + " groups" + "\n" + "not banned from " + failed.Count + " groups"},
                        {"it", "Target "+target+" bannato da " + done.Count + " gruppi"+ "\n" + "non bannato da " + failed.Count + " gruppi"}
                    });

                return text2;
            }

        }
    }
}