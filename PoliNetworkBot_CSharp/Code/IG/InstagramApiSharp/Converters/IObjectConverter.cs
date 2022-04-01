namespace InstagramApiSharp.Converters;

internal interface IObjectConverter<out T, TT>
{
    T Convert();
}