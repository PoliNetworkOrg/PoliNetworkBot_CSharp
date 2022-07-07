#region

using System.Data;
using PoliNetworkBot_CSharp.Code.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class ResultFixGroupsName
{
    private DataRow _dataRow;
    private GroupsFixLogUpdatedEnum _groupsFixLogUpdatedEnum;

    public ResultFixGroupsName(GroupsFixLogUpdatedEnum s1, DataRow groupsRow)
    {
        _groupsFixLogUpdatedEnum = s1;
        _dataRow = groupsRow;
    }
}