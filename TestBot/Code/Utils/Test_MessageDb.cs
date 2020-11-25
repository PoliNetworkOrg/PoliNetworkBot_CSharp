#region

using NUnit.Framework;

#endregion

namespace TestBot.Code.Utils
{
    public class MessageDb
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var r = PoliNetworkBot_CSharp.Code.Utils.MessageDb.GetMessageTypeClassById(1);
            if (r == null)
                Assert.Fail();
            else
                Assert.Pass();
        }
    }
}