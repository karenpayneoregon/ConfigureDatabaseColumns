using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseConfiguration
{
    public class UpdateConfiguration : BaseSqlServerConnections
    {
        public UpdateConfiguration()
        {
            DefaultCatalog = "NorthWindAzure";
            ExistingTablesInClause();
        }

        private string ExistingTablesInClause()
        {
            mHasException = false;
            var knownTableNamesList = new List<string>();
            var allTableNamesInDatabase = new List<string>();

            var selectStatement = "SELECT TableName  FROM dbo.TableNames";
            using (var cn = new SqlConnection() {ConnectionString = ConnectionString})
            {
                using (var cmd = new SqlCommand() {Connection = cn})
                {
                    cmd.CommandText = selectStatement;
                    try
                    {
                        cn.Open();

                        // get table names in dbo.TableNames to compare against all tables in the current catalog
                        var reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                knownTableNamesList.Add(reader.GetString(0));
                            }

                            reader.Close();
                        }
                        else
                        {
                            // setup has not executed yet
                        }

                        selectStatement = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES " + 
                                          "WHERE TABLE_TYPE = \'BASE TABLE\' AND TABLE_NAME != \'sysdiagrams\'  " + 
                                          "AND TABLE_NAME NOT IN (\'TableNames\',\'TableColumnInformation\') " + 
                                          "ORDER BY TABLE_NAME;";

                        cmd.CommandText = selectStatement;

                        // get all tables in current catalog to compare against table names in dbo.TableNames
                        reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                allTableNamesInDatabase.Add(reader.GetString(0));
                            }

                            reader.Close();

                        }

                    }
                    catch (Exception e)
                    {
                        mHasException = true;
                        mLastException = e;
                    }
                }
            }

            var test = string.Join(",",allTableNamesInDatabase.Except(knownTableNamesList).ToArray());

            var items = allTableNamesInDatabase.Except(knownTableNamesList).ToArray();
            if (items == null || items.Length == 0)
            {
                // no tables not in dbo.TableNames
            }
            else
            {
                var testItems = $"({string.Join(",",items)})";
                Console.WriteLine();
            }
            

            return "";
        }
    }
}
