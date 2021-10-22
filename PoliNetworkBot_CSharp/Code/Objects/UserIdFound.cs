namespace PoliNetworkBot_CSharp.Code.Objects
{
    internal class UserIdFound
    {
        private readonly int? i;
        private readonly string v;

        public UserIdFound(int? i)
        {
            this.i = i;
        }

        public UserIdFound(int? i, string v) : this(i)
        {
            this.v = v;
        }

        internal int? GetID()
        {
            return i;
        }

        internal string getError()
        {
            return v;
        }
    }
}