using DevDay.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevDay.Test
{
    [TestClass]
    public class MailHelperTest
    {
        [TestMethod]
        public void ShouldSendEmail()
        {
            Assert.IsTrue(MailHelper.Send("renato.saito@live.com", "teste"));
        }
    }
}
