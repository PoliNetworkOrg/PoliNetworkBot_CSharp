#region

using System.Collections.Generic;
using System.Data;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class GetGroupsAndFixNamesResult
{
    private DataTable? _dataTable;
    private List<ResultFixGroupsName> _list;

    public GetGroupsAndFixNamesResult(List<ResultFixGroupsName> s1, DataTable? s2)
    {
        _list = s1;
        _dataTable = s2;
    }
}