using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseConfiguration
{
    /// <summary>
    /// Responsible for setting up TableNames and TableColumnInformation tables
    /// with detail information e.g. one row in TableNames for each table in
    /// the current catalog as set in DefaultCatalog propery in the new constructor
    /// of this class, for each row in TableNames the table TableColumnInformation
    /// will have one record for each field in a table in TableNames.
    /// </summary>
    /// <remarks>
    /// TableNames has a rule for cascading deletes which means
    /// when removing a record from TableNames associated column records
    /// are removed from TableColumnInformation
    /// </remarks>
    public class SetupOperations : BaseSqlServerConnections
    {
        public SetupOperations()
        {
            DefaultCatalog = "NorthWindAzure";
        }

        public bool PopulateTableNamesTable()
        {

            mHasException = false;
            var success = false;

            /*
             * Get all table name excluding schema
             * and the two configuration tables
             */
            var selectStatement = "SELECT  TABLE_NAME AS tb " + 
                                  "FROM INFORMATION_SCHEMA.TABLES " +
                                  "WHERE TABLE_TYPE = \'BASE TABLE\' AND TABLE_NAME != \'sysdiagrams\' AND " + 
                                  "TABLE_NAME NOT IN (\'TableNames\',\'TableColumnInformation\') " + 
                                  "ORDER BY TABLE_NAME;";

            var insertIntoTableNames = "INSERT INTO dbo.TableNames (TableName) VALUES (@TableName);" + 
                                       "SELECT CAST(scope_identity() AS int);";

            var tableDetailsList = new List<TableDetails>();

            using (var cn = new SqlConnection() {ConnectionString = ConnectionString})
            {
                using (var cmd = new SqlCommand() {Connection = cn})
                {
                    cmd.CommandText = selectStatement;

                    try
                    {

                        cn.Open();

                        var reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {

                            while (reader.Read())
                            {
                                tableDetailsList.Add(new TableDetails() {Name = reader.GetString(0)});
                            }

                            reader.Close();

                            cmd.CommandText = insertIntoTableNames;

                            cmd.Parameters.Add(new SqlParameter()
                            {
                                ParameterName = "@TableName",
                                SqlDbType = SqlDbType.NVarChar
                            });
                            
                            foreach (var tableDetail in tableDetailsList)
                            {
                                cmd.Parameters["@TableName"].Value = tableDetail.Name;
                                tableDetail.TableId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            success = PopulateTableColumnInformation(cn, tableDetailsList);

                        }
                        else
                        {
                            // no tables in database
                            success = false;
                        }
                        
                    }
                    catch (Exception e)
                    {
                        mHasException = true;
                        mLastException = e;
                    }
                }
            }

            return success;

        }
        /// <summary>
        /// Setup TableColumnInformation with default values. 
        /// Note ColumnHeaderText is set to the same as the column name so
        /// when traversing column information in a application that value is
        /// used unless the value is changes, for instance if the value of 
        /// ColumnName is ContactName and ColumnHeaderText value when initialized
        /// is also ContactName then changed to Contact name then that is what
        /// will be used in the presentation layer of the current application.
        /// </summary>
        /// <param name="cn">Valid open connection to the targeted database</param>
        /// <param name="pTableDetails"></param>
        /// <returns></returns>
        private bool PopulateTableColumnInformation(SqlConnection cn, List<TableDetails> pTableDetails)
        {
            var success = false;
            var insertStatement = "INSERT INTO dbo.TableColumnInformation (TableId,ColumnName,Visible,OrdinalPosition,ColumnHeaderText) " +
                                  "VALUES (@TableId,@ColumnName,1,@OrdinalPosition,@ColumnName);SELECT CAST(scope_identity() AS int);";

            using (var cmd = new SqlCommand() {Connection = cn})
            {
                cmd.CommandText = insertStatement;

                cmd.Parameters.Add(new SqlParameter() {ParameterName = "@TableId",SqlDbType = SqlDbType.Int});
                cmd.Parameters.Add(new SqlParameter() {ParameterName = "@ColumnName",SqlDbType = SqlDbType.NVarChar});
                cmd.Parameters.Add(new SqlParameter() {ParameterName = "@OrdinalPosition",SqlDbType = SqlDbType.Int});

                try
                {
                    foreach (var details in pTableDetails)
                    {
                        var columnDataList = GetColumnsForTable(cn, details.Name);
                        if (columnDataList != null)
                        {
                            foreach (var col in columnDataList)
                            {
                                cmd.Parameters[0].Value = details.TableId;
                                cmd.Parameters[1].Value = col.Name;
                                cmd.Parameters[2].Value = col.OrdinalPosition;
                                col.id = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }
                    }

                    success = true;

                }
                catch (Exception e)
                {
                    mHasException = true;
                    mLastException = e;
                }
            }

            return success;

        }
        /// <summary>
        /// Responsible for obtaining column details for the table pTableName
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="pTableName">Existing table in current database</param>
        /// <returns></returns>
        private List<ColumnDetails> GetColumnsForTable(SqlConnection cn, string pTableName)
        {
            List<ColumnDetails> columnDetailsList = null;

            var selectStatement = "SELECT COLUMN_NAME,ORDINAL_POSITION " + 
                                  "FROM INFORMATION_SCHEMA.COLUMNS " + 
                                  "WHERE TABLE_NAME = @TableName; ";

            using (var cmd = new SqlCommand() {Connection = cn})
            {
                cmd.CommandText = selectStatement;
                cmd.Parameters.AddWithValue("@TableName", pTableName);
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    columnDetailsList = new List<ColumnDetails>();
                    while (reader.Read())
                    {

                        columnDetailsList.Add(new ColumnDetails()
                        {
                            Name = reader.GetString(0),
                            OrdinalPosition = reader.GetInt32(1)
                        });

                    }

                    reader.Close();
                }

            }

            return columnDetailsList;

        }
        /// <summary>
        /// Remove all records from TableNames and TableColumnInformation tables
        /// via the DELETE statement, cascading delete rule. Followed by resetting
        /// the primary key for both tables.
        /// </summary>
        /// <returns></returns>
        public bool TruncateTableNamesTable()
        {
            mHasException = false;
            var success = false;


            var selectStatement = "DELETE FROM TableNames;" + 
                                  "DBCC CHECKIDENT (TableNames, RESEED, 0);" +
                                  "DBCC CHECKIDENT (TableColumnInformation, RESEED, 0);";

            try
            {
                using (var cn = new SqlConnection() { ConnectionString = ConnectionString })
                {
                    using (var cmd = new SqlCommand() { Connection = cn, CommandText = selectStatement })
                    {

                        cn.Open();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "SELECT COUNT(Id) as id FROM dbo.TableNames";
                        success = ((int)cmd.ExecuteScalar() == 0);
                    }
                }
            }
            catch (Exception e)
            {
                mHasException = true;
                mLastException = e;
            }

            return success;

        }
        /// <summary>
        /// Get total record count for the table TableNames
        /// </summary>
        /// <returns></returns>
        public int TableNamesRecordCount()
        {
            mHasException = false;
            int recordCount = 0;
            var selectStatement = "SELECT COUNT(Id) FROM dbo.TableNames";

            try
            {
                using (var cn = new SqlConnection() { ConnectionString = ConnectionString })
                {
                    using (var cmd = new SqlCommand() { Connection = cn, CommandText = selectStatement })
                    {

                        cn.Open();
                        recordCount = (int) cmd.ExecuteScalar();
                    }
                }

            }
            catch (Exception e)
            {
                mHasException = true;
                mLastException = e;
            }

            return recordCount;
        }
    }
}
