using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    internal class DB_Backup
    {
        public List<string> tableNames;
        public Dictionary<string, DataTable> tables; //indexed by names

        public DB_Backup()
        {
            this.tableNames = new List<string>();
            this.tables = new Dictionary<string, DataTable>();
        }
    }
}