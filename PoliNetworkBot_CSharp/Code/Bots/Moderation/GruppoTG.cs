using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    public class GruppoTG
    {
        public string idLink;
        public DateTime? LastUpdateInviteLinkTimeDateTime;
        public string LastUpdateInviteLinkTimeString;
        public string newLink;
        public string nome;
        public List<string> oldLinks;
        public long? permanentId;

        public GruppoTG(JToken idLink, JToken nome, JToken id, JToken LastUpdateInviteLinkTime)
        {
            this.idLink = idLink.ToString();
            oldLinks = new List<string> {this.idLink};
            this.nome = nome.ToString();

            try
            {
                permanentId = Convert.ToInt64(id.ToString());
            }
            catch
            {
                ;
            }

            try
            {
                LastUpdateInviteLinkTimeString = LastUpdateInviteLinkTime.ToString();
                if (LastUpdateInviteLinkTimeString.Contains(" "))
                {
                    var s1 = LastUpdateInviteLinkTimeString.Split(' ');
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

                                LastUpdateInviteLinkTimeDateTime = new DateTime(
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
            permanentId = value;
        }

        internal void UpdateNewLink(string link)
        {
            newLink = link;
        }
    }
}