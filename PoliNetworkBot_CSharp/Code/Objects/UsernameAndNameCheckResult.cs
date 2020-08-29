namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class UsernameAndNameCheckResult
    {
        private readonly string _language;
        private readonly string firstName;
        private readonly string lastName;
        public readonly bool Name;
        private readonly int userId;
        public readonly bool UsernameBool;
        private readonly string usernameString;

        public UsernameAndNameCheckResult(in bool usernameBool, in bool name, string language,
            string usernameString, int userId, string firstName, string lastName)
        {
            UsernameBool = usernameBool;
            Name = name;
            _language = language;
            this.usernameString = usernameString;
            this.userId = userId;
            this.firstName = firstName;
            this.lastName = lastName;
        }

        public string GetLanguage()
        {
            return _language;
        }

        public string GetUsername()
        {
            return usernameString;
        }

        public int GetUserId()
        {
            return userId;
        }

        public string GetFirstName()
        {
            return firstName;
        }

        public string GetLastName()
        {
            return lastName;
        }
    }
}