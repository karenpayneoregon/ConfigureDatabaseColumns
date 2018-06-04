using System;
using DatabaseConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BackEndTestProject
{
    [TestClass]
    public class UnitTest1 : TestBase
    {

        [TestInitialize]
        public void Init()
        {
            var ops = new SetupOperations();
            ops.TruncateTableNamesTable();
            ops.PopulateTableNamesTable();
        }

        [TestMethod]
        public void PopulateTableNamesTable()
        {
            Assert.IsTrue(DatabaseTableCount(),
                "Expected TableNames row count to equal count of tables in database");
        }
        [TestMethod]
        public void TableNamesHasRecords_SetupOperations()
        {
            var ops = new SetupOperations();
            var recordCount = ops.TableNamesRecordCount();

            Assert.IsTrue(ops.HasException == false);
            Assert.IsTrue(recordCount >0);
        }
        /// <summary>
        /// Check to see if list count matches count from
        /// SELECT FROM INFORMATION_SCHEMA.TABLES
        /// </summary>
        [TestMethod]
        public void TableNamesHasRecords_ConfiigurationOperations()
        {
            var ops = new ConfigureOperations();
            Assert.IsTrue(DatabaseTableCount(ops.Tables().Count));
        }
        [TestMethod]
        public void TruncateTableNamesTable() 
        {
            var ops = new SetupOperations();
            var result = ops.TruncateTableNamesTable();

            Assert.IsTrue(ops.HasException == false);
            Assert.IsTrue(result,
                "Expected TableNames table to have been truncated");
        }
    }
}
