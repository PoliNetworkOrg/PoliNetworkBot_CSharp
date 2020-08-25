using NUnit.Framework;

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
            PoliNetworkBot_CSharp.Code.MainProgram.NewConfig.NewConfigMethod(true,true);
            
            var r = PoliNetworkBot_CSharp.Code.Utils.MessageDb.GetMessageTypeClassById(1);
            if (r == null)
                Assert.Fail();
            
            Assert.Pass();
        }
    }
}