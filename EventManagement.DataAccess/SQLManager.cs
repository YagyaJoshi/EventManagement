using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace EventManagement.DataAccess
{
    public class SQLManager : IDisposable
    {
        public IConfiguration _configuration { get; }
        private string g_ConStr = string.Empty;
        private int g_Timeout = 300;

        public SQLManager(IConfiguration configuration, string p_ConStr = "", int p_Timeout = 0)
        {
            _configuration = configuration;

            try
            {
                //Prod
                if (p_ConStr.Trim().Length == 0)
                {
                    p_ConStr = _configuration.GetConnectionString("DefaultConnection").ToString();
                }
                if ((p_ConStr == null ? string.Empty : p_ConStr.Trim()).Length != 0)
                {
                    g_ConStr = (p_ConStr.Trim().Length == 0 ? p_ConStr : p_ConStr);
                }

                //Make Sure Connection Info ends properly.
                if (g_ConStr.Trim().Length > 0)
                {
                    g_ConStr = (g_ConStr + ";").Replace(";;", ";");
                }

                #region Custom Settings
                //Apply Custom Connection Timeout
                if (g_ConStr.Length > 0 && p_Timeout > 0)
                {
                    g_Timeout = p_Timeout;
                    if (g_ConStr.Contains("Connection Timeout=120;") == true)
                    {
                        g_ConStr = g_ConStr.Replace("Connection Timeout=120;", "");
                    }
                    if (g_ConStr.Contains("Connection Timeout=60;") == true)
                    {
                        g_ConStr = g_ConStr.Replace("Connection Timeout=60;", "");
                    }
                    g_ConStr += "Connection Timeout=" + p_Timeout.ToString() + ";";
                }

                //Apply default connection timeout if not exists
                if (g_ConStr.Length > 0 && g_ConStr.Contains("Connection Timeout") == false)
                {
                    g_ConStr += "Connection Timeout=300;";
                }

                //max pool size is 100 by default.
                if (g_ConStr.Length > 0 && g_ConStr.Contains("Max Pool") == false)
                {
                    g_ConStr += "Max Pool Size=50000;Pooling=True;";
                }
                #endregion
            }
            catch (System.Exception ex)
            {
                throw new Exception("DBM-DBMGR: " + ex.Message);
            }
            finally
            {

            }
        }

        public void Dispose()
        { 
        }

        public async Task<DataSet> GetDS(string p_StrQry)
        {
            DataSet DS = new DataSet();
            if (string.IsNullOrWhiteSpace(g_ConStr))
            {
                throw new Exception("Err-DBM: Connection string is not available.");
            }

            using (SqlConnection g_ObjCon = new SqlConnection(g_ConStr))
            {
                try
                {
                    await g_ObjCon.OpenAsync();

                    using (SqlCommand g_ObjCmd = new SqlCommand(p_StrQry, g_ObjCon))
                    {
                        g_ObjCmd.CommandTimeout = 300;

                        using (SqlDataReader reader = await g_ObjCmd.ExecuteReaderAsync())
                        {
                            DS.Load(reader, LoadOption.OverwriteChanges, DS.Tables.Add());
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    throw new Exception("DBM-GetDS-004: " + ex.Message, ex);
                }
            }

            return DS;
        }


        public async Task UpdateDB(string p_StrQry)
        {
            if (string.IsNullOrWhiteSpace(g_ConStr))
            {
                throw new Exception("Err-DBM: Connection string is not available.");
            }

            using (SqlConnection g_ObjCon = new SqlConnection(g_ConStr))
            {
                using (SqlCommand g_ObjSQLCmd = new SqlCommand(p_StrQry, g_ObjCon))
                {
                    SqlTransaction transaction = null;
                    try
                    {
                        g_ObjSQLCmd.CommandType = CommandType.Text;
                        g_ObjSQLCmd.CommandTimeout = g_Timeout;

                        await g_ObjCon.OpenAsync();
                        transaction = g_ObjCon.BeginTransaction();
                        g_ObjSQLCmd.Transaction = transaction;

                        await g_ObjSQLCmd.ExecuteNonQueryAsync();

                        transaction.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                        throw new Exception("DBM-UpdateDB-003: " + ex.Message, ex);
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            transaction.Dispose();
                        }
                    }
                }
            }
        }

        public async Task UpdateDB(SqlCommand p_objSQLCmd, bool p_IsStoredProcedure = false)
        {
            if (string.IsNullOrWhiteSpace(g_ConStr))
            {
                throw new Exception("Err-DBM: Connection string is not available.");
            }

            using (SqlConnection g_ObjCon = new SqlConnection(g_ConStr))
            {
                try
                {
                    p_objSQLCmd.Connection = g_ObjCon;
                    p_objSQLCmd.CommandTimeout = g_Timeout;
                    if (p_IsStoredProcedure)
                    {
                        p_objSQLCmd.CommandType = CommandType.StoredProcedure;
                    }

                    await g_ObjCon.OpenAsync();

                    using (SqlTransaction transaction = g_ObjCon.BeginTransaction())
                    {
                        p_objSQLCmd.Transaction = transaction;

                        try
                        {
                            await p_objSQLCmd.ExecuteNonQueryAsync();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception("DBM-UpdateDB-004i: " + ex.Message, ex);
                        }
                    }
                }
                finally
                {
                    if (g_ObjCon.State == ConnectionState.Open)
                    {
                        await g_ObjCon.CloseAsync();
                    }
                }
            }
        }

        public async Task<DataSet> FetchDB(string p_StrQry)
        {
            DataSet DS = new DataSet();
            if (string.IsNullOrWhiteSpace(g_ConStr))
            {
                throw new Exception("Err-DBM: Connection string is not available.");
            }

            using (SqlConnection g_ObjCon = new SqlConnection(g_ConStr))
            {
                try
                {
                    await g_ObjCon.OpenAsync();

                    using (SqlCommand g_ObjCmd = new SqlCommand(p_StrQry, g_ObjCon))
                    {
                        g_ObjCmd.CommandTimeout = g_Timeout;

                        using (SqlDataReader reader = await g_ObjCmd.ExecuteReaderAsync())
                        {
                            DS.Load(reader, LoadOption.OverwriteChanges, DS.Tables.Add());
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    throw new Exception("DBM-FetchDB-004: " + ex.Message, ex);
                }
            }

            return DS;
        }


        public async Task<DataTable> FetchDT(string p_StrQry)
        {
            DataTable l_DT = new DataTable();
            if (string.IsNullOrWhiteSpace(g_ConStr))
            {
                throw new Exception("Err-DBM: Connection string is not available.");
            }

            using (SqlConnection g_ObjCon = new SqlConnection(g_ConStr))
            {
                using (SqlCommand g_ObjSQLCmd = new SqlCommand(p_StrQry, g_ObjCon))
                {
                    try
                    {
                        g_ObjSQLCmd.CommandTimeout = g_Timeout;

                        await g_ObjCon.OpenAsync();

                        using (SqlTransaction transaction = g_ObjCon.BeginTransaction())
                        {
                            g_ObjSQLCmd.Transaction = transaction;

                            using (SqlDataReader l_DR = await g_ObjSQLCmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                            {
                                l_DT.Load(l_DR);
                            }

                            transaction.Commit();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (g_ObjSQLCmd != null && g_ObjSQLCmd.Transaction != null)
                        {
                            g_ObjSQLCmd.Transaction.Rollback();
                        }
                        throw new Exception("DBM-FetchDT-004i: " + ex.Message, ex);
                    }
                    finally
                    {
                        if (g_ObjCon.State == ConnectionState.Open)
                        {
                            g_ObjCon.Close();
                        }
                        g_ObjSQLCmd.Dispose();
                    }
                }
            }

            return l_DT;
        }


        public async Task<DataTable> FetchDT(SqlCommand p_objSQLCmd)
        {
            DataTable l_DT = new DataTable();
            if (string.IsNullOrWhiteSpace(g_ConStr))
            {
                throw new Exception("Err-DBM: Connection string is not available.");
            }

            using (SqlConnection g_ObjCon = new SqlConnection(g_ConStr))
            {
                try
                {
                    p_objSQLCmd.Connection = g_ObjCon;
                    p_objSQLCmd.CommandType = CommandType.StoredProcedure;
                    p_objSQLCmd.CommandTimeout = g_Timeout;

                    await g_ObjCon.OpenAsync();

                    using (SqlDataReader l_DR = await p_objSQLCmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        l_DT.Load(l_DR);
                    }
                }
                catch (System.Exception ex)
                {
                    throw new Exception("DBM-FetchDT-004i: " + ex.Message, ex);
                }
                finally
                {
                    if (g_ObjCon.State == ConnectionState.Open)
                    {
                        g_ObjCon.Close();
                    }
                    p_objSQLCmd.Dispose();
                }
            }

            return l_DT;
        }

        public async Task<string> FetchXML(SqlCommand p_objSQLCmd)
        {
            string xml = "";
            if (string.IsNullOrWhiteSpace(g_ConStr))
            {
                throw new Exception("Err-DBM: Connection string is not available.");
            }

            using (SqlConnection g_ObjCon = new SqlConnection(g_ConStr))
            {
                try
                {
                    p_objSQLCmd.Connection = g_ObjCon;
                    p_objSQLCmd.CommandType = CommandType.StoredProcedure;
                    p_objSQLCmd.CommandTimeout = g_Timeout;

                    await g_ObjCon.OpenAsync();

                    object result = await p_objSQLCmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        xml = result.ToString();
                    }
                }
                catch (System.Exception ex)
                {
                    throw new Exception("DBM-FetchXML-004i: " + ex.Message, ex);
                }
                finally
                {
                    if (g_ObjCon.State == ConnectionState.Open)
                    {
                        g_ObjCon.Close();
                    }
                    p_objSQLCmd.Dispose();
                }
            }

            return xml;
        }

        public async Task<string> Fetch(SqlCommand p_objSQLCmd, bool p_IsStoredProcedure)
        {
            string xml = "";
            if (string.IsNullOrWhiteSpace(g_ConStr))
            {
                throw new Exception("Err-DBM: Connection string is not available.");
            }

            using (SqlConnection g_ObjCon = new SqlConnection(g_ConStr))
            {
                try
                {
                    p_objSQLCmd.Connection = g_ObjCon;
                    if (p_IsStoredProcedure)
                    {
                        p_objSQLCmd.CommandType = CommandType.StoredProcedure;
                    }
                    p_objSQLCmd.CommandTimeout = g_Timeout;

                    await g_ObjCon.OpenAsync();

                    object result = await p_objSQLCmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        xml = result.ToString();
                    }
                }
                catch (System.Exception ex)
                {
                    throw new Exception("DBM-Fetch-004i: " + ex.Message, ex);
                }
                finally
                {
                    if (g_ObjCon.State == ConnectionState.Open)
                    {
                        g_ObjCon.Close();
                    }
                    p_objSQLCmd.Dispose();
                }
            }

            return xml;
        }


        public async Task<DataSet> FetchDB(SqlCommand p_objSQLCmd)
        {
            DataSet DS = new DataSet();
            if (string.IsNullOrWhiteSpace(g_ConStr))
            {
                throw new Exception("Err-DBM: Connection string is not available.");
            }

            using (SqlConnection g_ObjCon = new SqlConnection(g_ConStr))
            {
                try
                {
                    p_objSQLCmd.Connection = g_ObjCon;
                    p_objSQLCmd.CommandType = CommandType.StoredProcedure;

                    await g_ObjCon.OpenAsync();

                    SqlDataAdapter ObjSDA = new SqlDataAdapter(p_objSQLCmd);
                    ObjSDA.SelectCommand.CommandTimeout = g_Timeout;
                    ObjSDA.Fill(DS);

                    ObjSDA.Dispose();
                }
                catch (System.Exception ex)
                {
                    throw new Exception("DBM-FetchDB-004: " + ex.Message, ex);
                }
                finally
                {
                    if (g_ObjCon.State == ConnectionState.Open)
                    {
                        g_ObjCon.Close();
                    }
                    p_objSQLCmd.Dispose();
                }
            }

            return DS;
        }


        public async Task UpdateDB(DataTable p_DT, string p_StrQry)
        {
            if (string.IsNullOrWhiteSpace(g_ConStr))
            {
                throw new Exception("Err-DBM: Connection string is not available.");
            }

            using (SqlConnection g_ObjCon = new SqlConnection(g_ConStr))
            {
                SqlCommand g_ObjSQLCmd = new SqlCommand(p_StrQry, g_ObjCon);
                try
                {
                    SqlDataAdapter l_DA = new SqlDataAdapter(g_ObjSQLCmd);
                    g_ObjSQLCmd.CommandTimeout = g_Timeout;

                    await g_ObjCon.OpenAsync();

                    // Use SqlCommandBuilder to generate update commands
                    SqlCommandBuilder l_SCB = new SqlCommandBuilder(l_DA);
                    l_DA.UpdateCommand = l_SCB.GetUpdateCommand();

                    // Update DataTable using SqlDataAdapter
                    await Task.Run(() => {
                        l_DA.AcceptChangesDuringFill = l_DA.AcceptChangesDuringUpdate = true;
                        l_DA.Update(p_DT);
                        p_DT.AcceptChanges();
                    });

                    l_DA.Dispose();
                    l_SCB.Dispose();
                }
                catch (System.Exception ex)
                {
                    if (g_ObjSQLCmd.Transaction != null)
                    {
                        g_ObjSQLCmd.Transaction.Rollback();
                    }
                    throw new Exception("DBM-UpdateDB-005: " + ex.Message, ex);
                }
                finally
                {
                    g_ObjSQLCmd.Dispose();
                    if (g_ObjCon.State == ConnectionState.Open)
                    {
                        g_ObjCon.Close();
                    }
                }
            }
        }

    }
}
