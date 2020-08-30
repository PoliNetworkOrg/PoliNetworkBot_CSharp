namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class UsernameAndNameCheckResult
    {
        private readonly string _language;
        private readonly string _firstName;
        private readonly string _lastName;
        public readonly bool Name;
        private readonly int _userId;
        public readonly bool UsernameBool;
        private readonly string _usernameString;

        public UsernameAndNameCheckResult(in bool usernameBool, in bool name, string language,
            string usernameString, int userId, string firstName, string lastName)
        {
            UsernameBool = usernameBool;
            Name = name;
            _language = language;
            this._usernameString = usernameString;
            this._userId = userId;
            this._firstName = firstName;
            this._lastName = lastName;
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