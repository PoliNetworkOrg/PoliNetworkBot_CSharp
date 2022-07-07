using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Objects;

public class UpdateGroupsResult
{
    private List<ResultFixGroupsName>? _list;
    public readonly Language Language;
    public UpdateGroupsResult(Language language, List<ResultFixGroupsName>? x1)
    {
        _list = x1;
        this.Language = language;
    }
}