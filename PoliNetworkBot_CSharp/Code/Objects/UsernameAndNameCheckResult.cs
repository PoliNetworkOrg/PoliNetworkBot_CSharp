namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class UsernameAndNameCheckResult
    {
        public readonly bool UsernameBool;
        public readonly bool Name;
        private readonly string _language;
        private readonly string usernameString;
        private readonly int userId;
        private readonly string firstName;
        private readonly string lastName;
        
        public UsernameAndNameCheckResult(in bool usernameBool, in bool name, string language, 
            string usernameString, int userId, string firstName, string lastName)
        {
            this.UsernameBool = usernameBool;
            this.Name = name;
            this._language = language;
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
            return this.usernameString;
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