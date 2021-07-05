using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    public class GruppoTG
    {
        public string idLink;
        public string nome;
        public long? permanentId;
        public string newLink;
        public List<string> oldLinks;
        public DateTime? LastUpdateInviteLinkTimeDateTime;
        public string LastUpdateInviteLinkTimeString;

        public GruppoTG(JToken idLink, JToken nome, JToken id, JToken LastUpdateInviteLinkTime)
        {
            this.idLink = idLink.ToString();
            this.oldLinks = new List<string>() { this.idLink };
            this.nome = nome.ToString();

            try
            {
                this.permanentId = Convert.ToInt64(id.ToString());
            }
            catch
            {
                ;
            }

            try
            {
                this.LastUpdateInviteLinkTimeString = LastUpdateInviteLinkTime.ToString();
                if (this.LastUpdateInviteLinkTimeString.Contains(" "))
                {
                    var s1 = this.LastUpdateInviteLinkTimeString.Split(' ');
                    var s2 = s1[0]; //2021-06-30
                    var s3 = s1[1]; //22:12:06.399

                    if (s2.Contains("-"))
                    {
                        var s4 = s2.Split('-');
                        var year = s4[0];
                        var month = s4[1];
                        var day = s4[2];

                        if (s3.Contains("."))
                        {
                            var s5 = s3.Split(".");
                            var s6 = s5[0]; //22:12:06
                            var millisecond = s5[1];

                            if (s6.Contains(":"))
                            {
                                var s7 = s6.Split(':');

                                var hour = s7[0];
                                var minute = s7[1];
                                var second = s7[2];

                                this.LastUpdateInviteLinkTimeDateTime = new DateTime(
                                    Convert.ToInt32(year),
                                    Convert.ToInt32(month),
                                    Convert.ToInt32(day),
                                    Convert.ToInt32(hour),
                                    Convert.ToInt32(minute),
                                    Convert.ToInt32(second),
                                    Convert.ToInt32(millisecond));
                            }
                        }
                    }
                }
            }
            catch
            {
                ;
            }
        }

        internal void UpdateID(long value)
        {
            this.permanentId = value;
        }

        internal void UpdateNewLink(string link)
        {
            this.newLink = link;
        }
    }
}