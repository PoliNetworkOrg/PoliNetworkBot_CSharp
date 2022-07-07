using System.Data;
using PoliNetworkBot_CSharp.Code.Enums;

namespace PoliNetworkBot_CSharp.Code.Objects;

public class ResultFixGroupsName
{
    private GroupsFixLogUpdatedEnum _groupsFixLogUpdatedEnum;
    private DataRow _dataRow;
    public ResultFixGroupsName(GroupsFixLogUpdatedEnum s1, DataRow groupsRow)
    {
        this._groupsFixLogUpdatedEnum = s1;
        this._dataRow = groupsRow;
    }
}