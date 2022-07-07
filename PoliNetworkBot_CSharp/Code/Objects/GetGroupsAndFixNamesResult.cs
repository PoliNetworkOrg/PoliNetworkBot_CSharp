using System.Collections.Generic;
using System.Data;

namespace PoliNetworkBot_CSharp.Code.Objects;

public class GetGroupsAndFixNamesResult
{
    private List<ResultFixGroupsName> _list;
    private DataTable? _dataTable;
    public GetGroupsAndFixNamesResult(List<ResultFixGroupsName> s1, DataTable? s2)
    {
        _list = s1;
        _dataTable = s2;
    }
}