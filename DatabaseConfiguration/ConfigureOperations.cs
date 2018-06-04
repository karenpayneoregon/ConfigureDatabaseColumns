using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseConfiguration
{
    public class ConfigureOperations : BaseSqlServerConnections
    {
        public ConfigureOperations() 
        {
            DefaultCatalog = "NorthWindAzure";
        }

        public List<TableDetails> Tables()
        {
            mHasException = false;

            var tableDetailsList = new List<TableDetails>();

            var selectStatement = "SELECT id ,TableName FROM dbo.TableNames";

            using (var cn = new SqlConnection() {ConnectionString = ConnectionString})
            {
                using (var cmd = new SqlCommand() {Connection = cn})
                {
                    cmd.CommandText = selectStatement;
                    try
                    {
                        cn.Open();
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            tableDetailsList.Add(new TableDetails() { TableId = reader.GetInt32(0), Name = reader.GetString(1)});
                        }
                    }
                    catch (Exception e)
                    {
                        mHasException = true;
                        mLastException = e;
                    }
                }
            }

            return tableDetailsList;
        }

        public DataTable Columns()
        {
            var dt = new DataTable();
            var selectStatement = "SELECT id,TableId,ColumnName,ColumnHeaderText,Visible,OrdinalPosition " + 
                                  "FROM dbo.TableColumnInformation";

            using (var cn = new SqlConnection() { ConnectionString = ConnectionString })
            {
                using (var cmd = new SqlCommand() { Connection = cn })
                {
                    cmd.CommandText = selectStatement;
                    try
                    {
                        cn.Open();
                        dt.Load(cmd.ExecuteReader());

                        dt.Columns["id"].ColumnMapping = MappingType.Hidden;
                        dt.Columns["TableId"].ColumnMapping = MappingType.Hidden;
                        dt.Columns["OrdinalPosition"].ColumnMapping = MappingType.Hidden;
                    }
                    catch (Exception e)
                    {
                        mHasException = true;
                        mLastException = e;
                    }
                }
            }

            return dt;
        }
        /// <summary>
        /// Update one row for TableColumnInformation
        /// </summary>
        /// <param name="pColumnDetails">ColumnDetails instance populated</param>
        /// <returns>true when one row is updated and false if row failed to update</returns>
        public bool UpdateColumn(ColumnDetails pColumnDetails)
        {
            mHasException = false;
            var success = false;

            using (var cn = new SqlConnection() {ConnectionString = ConnectionString})
            {
                
                try
                {
                    cn.Open();

                    var cmd = PrepareUpdateColumnsSqlCommand(cn);

                    cmd.Parameters["@ColumnName"].Value = pColumnDetails.Name;
                    cmd.Parameters["@ColumnHeaderText"].Value = pColumnDetails.Title;
                    cmd.Parameters["@Visible"].Value = pColumnDetails.Visible;
                    cmd.Parameters["@OrdinalPosition"].Value = pColumnDetails.OrdinalPosition;
                    cmd.Parameters["@id"].Value = pColumnDetails.id = pColumnDetails.id;

                    success = ((int) cmd.ExecuteNonQuery() == 1);
                }
                catch (Exception e)
                {
                    mHasException = true;
                    mLastException = e;
                }
            }

            return true;
        }
        /// <summary>
        /// Prepare command object for UpdateColumn method
        /// </summary>
        /// <param name="cn"></param>
        /// <returns></returns>
        private SqlCommand PrepareUpdateColumnsSqlCommand(SqlConnection cn)
        {
            var updateStatement =
                "UPDATE [dbo].[TableColumnInformation] " +
                "SET [ColumnName] = @ColumnName,[ColumnHeaderText] = @ColumnHeaderText,[Visible] = @Visible,[OrdinalPosition] = @OrdinalPosition " +
                "WHERE id = @id";

            SqlCommand cmd = new SqlCommand() { CommandType = CommandType.Text };
            cmd.CommandText = updateStatement;
            cmd.Connection = cn;
            cmd.Parameters.Add("@ColumnName", SqlDbType.NVarChar, -1);           
            cmd.Parameters.Add("@ColumnHeaderText", SqlDbType.NVarChar, -1);
            cmd.Parameters.Add("@Visible", SqlDbType.Bit);
            cmd.Parameters.Add("@OrdinalPosition", SqlDbType.Int);
            cmd.Parameters.Add("@id", SqlDbType.Int);
            cmd.Prepare();

            return cmd;
        }
    }
}
