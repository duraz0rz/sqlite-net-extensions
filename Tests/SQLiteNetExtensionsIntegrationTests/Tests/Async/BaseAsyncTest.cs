using System.Threading.Tasks;
using NUnit.Framework;
using SQLite;

namespace SQLiteNetExtensionsIntegrationTests.Tests.Async
{
    public class BaseAsyncTest
    {
        protected SQLiteAsyncConnection Connection;

        [OneTimeSetUp]
        public void InitialSetup()
        {
            SQLitePCL.Batteries_V2.Init();
        }

        [SetUp]
        public void SetupConnection()
        {
            Connection = new SQLiteAsyncConnection(":memory:");
        }

        [TearDown]
        public async Task TearDownConnection()
        {
            await Connection.CloseAsync();
        }
    }
}