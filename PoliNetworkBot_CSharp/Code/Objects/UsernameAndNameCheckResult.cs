namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class UsernameAndNameCheckResult
    {
        private readonly string _firstName;
        private readonly string _language;
        private readonly string _lastName;
        private readonly int _userId;
        private readonly string _usernameString;
        public readonly bool Name;
        public readonly bool UsernameBool;

        public UsernameAndNameCheckResult(in bool usernameBool, in bool name, string language,
            string usernameString, int userId, string firstName, string lastName)
        {
            UsernameBool = usernameBool;
            Name = name;
            _language = language;
            _usernameString = usernameString;
            _userId = userId;
            _firstName = firstName;
            _lastName = lastName;
        }

        public string GetLanguage()
        {
            return _language;
        }

        public string GetUsername()
        {
            return _usernameString;
        }

        public int GetUserId()
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
    }
}