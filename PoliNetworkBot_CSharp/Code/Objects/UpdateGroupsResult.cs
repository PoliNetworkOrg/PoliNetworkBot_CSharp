#region

using System.Collections.Generic;
using SampleNuGet.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class UpdateGroupsResult
{
    public readonly Language Language;
    private List<ResultFixGroupsName>? _list;

    public UpdateGroupsResult(Language language, List<ResultFixGroupsName>? x1)
    {
        _list = x1;
        Language = language;
    }
}