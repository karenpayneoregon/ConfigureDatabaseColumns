using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BackEndTestProject
{
    /// <summary>
    /// Helper methods for test methods
    /// </summary>
    public class TestBase : BaseSqlServerConnections
    {
        public TestBase()
        {
            DefaultCatalog = "NorthWindAzure";
        }

        protected TestContext TestContextInstance;
        public TestContext TestContext
        {
            get => TestContextInstance;
            set => TestContextInstance = value;
        }

        protected bool DatabaseTableCount()
        {
            int actualtableCount = 0;
            int rowCountInTableNames = 0;

            var selectStatement = "SELECT COUNT(TABLE_NAME) " +
                                  "FROM INFORMATION_SCHEMA.TABLES " + 
                                  "WHERE TABLE_TYPE = \'BASE TABLE\' AND TABLE_NAME != \'sysdiagrams\'  AND " +
                                  "TABLE_NAME NOT IN (\'TableNames\',\'TableColumnInformation\')";

            using (var cn = new SqlConnection() {ConnectionString = ConnectionString})
            {
                using (var cmd = new SqlCommand() {Connection = cn})
                {
                    cmd.CommandText = selectStatement;
                    cn.Open();
                    actualtableCount = (int) cmd.ExecuteScalar();

                    selectStatement = "SELECT COUNT(id) FROM dbo.TableNames";
                    cmd.CommandText = selectStatement;
                    rowCountInTableNames = (int)cmd.ExecuteScalar();


                }
            }

            return actualtableCount == rowCountInTableNames;

        }
        /// <summary>
        /// Checks record count from INFORMATION_SCHEMA.TABLES
        /// to pCount, if the same count return true, otherwise
        /// return false.
        /// </summary>
        /// <param name="pCount">Value to check against</param>
        /// <returns></returns>
        protected bool DatabaseTableCount(int pCount)
        {
            int actualtableCount = 0;

            var selectStatement = "SELECT COUNT(TABLE_NAME) " +
                                  "FROM INFORMATION_SCHEMA.TABLES " +
                                  "WHERE TABLE_TYPE = \'BASE TABLE\' AND TABLE_NAME != \'sysdiagrams\'  AND " +
                                  "TABLE_NAME NOT IN (\'TableNames\',\'TableColumnInformation\')";

            using (var cn = new SqlConnection() { ConnectionString = ConnectionString })
            {
                using (var cmd = new SqlCommand() { Connection = cn })
                {
                    cmd.CommandText = selectStatement;
                    cn.Open();
                    actualtableCount = (int)cmd.ExecuteScalar();
                }
            }

            return actualtableCount == pCount;

        }
    }
}
