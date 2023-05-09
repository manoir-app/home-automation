using Home.Common;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsForAgents.Common
{
    [TestClass]
    public class AgentHelperTests
    {
        [TestInitialize]
        public static void TestInitialize(TestContext testContext)
        {
            MainApiAgentWebClient.ClearTestHandlers();
        }


        [TestMethod]
        public void RedirectionDesAppelsApi()
        {
            MainApiAgentWebClient.RegisterTestHandler("v1.0/users/main", new Func<List<User>>( () =>
            {
                return new List<User>();
            }));
            var usrs = AgentHelper.GetMainUsers("tests");
            Assert.IsTrue(usrs.Count == 0);

            MainApiAgentWebClient.RegisterTestHandler("v1.0/users/main", new Func<List<User>>(() =>
            {
                return new List<User>(new User[]
                {
                    new User { Id = "test", Name = "TestNom", FirstName="TestPrenom"},
                    new User { Id = "test2", Name = "TestNom2", FirstName="TestPrenom2"},
                });
            }));
            
            usrs = AgentHelper.GetMainUsers("tests");
            Assert.IsTrue(usrs.Count == 2);
            Assert.AreEqual<string>("test", usrs[0].Id);
            Assert.AreEqual<string>("test2", usrs[1].Id);
        }
    }
}
