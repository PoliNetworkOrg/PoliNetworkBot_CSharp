namespace PoliNetworkBot_CSharp.Code.Objects;

public class UsernameAndNameCheckResult
{
    private readonly string _firstName;
    private readonly string _language;
    private readonly string _lastName;
    private readonly long? _userId;
    private readonly string _usernameString;
    private readonly long? messageId;
    public readonly bool Name;
    public readonly bool UsernameBool;

    public UsernameAndNameCheckResult(in bool usernameBool, in bool name, string language,
        string usernameString, long? userId, string firstName, string lastName, long? messageId)
    {
        UsernameBool = usernameBool;
        Name = name;
        _language = language;
        _usernameString = usernameString;
        _userId = userId;
        _firstName = firstName;
        _lastName = lastName;
        this.messageId = messageId;
    }

    public string GetLanguage()
    {
        return _language;
    }

    public string GetUsername()
    {
        return _usernameString;
    }

    public long? GetUserId()
    {
        return _userId;
    }

    public string GetFirstName()
    {
        return _firstName;
    }

    public string GetLastName()
    {
        return _lastName;
    }

    internal long? GetMessageId()
    {
        return messageId;
    }
}