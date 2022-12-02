using System;
using System.Data;

namespace PoliNetworkBot_CSharp.Code.Objects.DbObject;

public class Colonna
{
    public readonly string Name;
    public readonly Type DataType;

    public Colonna(string name, Type dataType)
    {
        this.Name = name;
        this.DataType = dataType;
    }
}