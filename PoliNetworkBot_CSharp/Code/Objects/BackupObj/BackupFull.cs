using System;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Objects.DbObject;

namespace PoliNetworkBot_CSharp.Code.Objects.BackupObj;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class BackupFull
{
    public DB_Backup? DbFull;
    public DbBackupDdl? DbFullDdl;

    public BackupFull(DB_Backup? dbFull, DbBackupDdl? dbFullDdl)
    {
        DbFull = dbFull;
        DbFullDdl = dbFullDdl;
    }
}