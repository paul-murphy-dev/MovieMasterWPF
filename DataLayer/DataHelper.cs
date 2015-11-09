using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Collections;

namespace MovieMaster.DataLayer
{
    public static class DataHelper
    {
        public static LocalStore _movieData = new LocalStore();
        private static string _server;
        private static string _database;
        private static string _userName;
        private static string _password;
        private static SqlConnection _con;
        private static SqlCommand _com;
        //private static string _sqlCEConStr = @"Data Source={0}\moviedata.sdf;Persist Security Info=False;";
        private static string _sqlServerConStr = @"Server={0};Database={1};Trusted_Connection=True;";
        //private static string _mysqlConStr = "Server={0};Database={1};Uid={2};Pwd={3};Provider=MySQLProv";

        static DataHelper()
        {
            //NameValueCollection configSectionSettings = (NameValueCollection)ConfigurationManager.GetSection("DataConfig");
            //_server = configSectionSettings["server"];
            //_database = configSectionSettings["database"];

            _movieData.BeginInit();
            _movieData.EndInit();
        }

        public static SqlConnection GetConnection()
        {
            if (_con == null)
            {
                _con = new SqlConnection(string.Format(_sqlServerConStr, _server, _database));
                //_sqlCEConStr = string.Format(_sqlCEConStr, System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", string.Empty));
                //_con = new SqlConnection(_sqlCEConStr);
            }

            return _con;
        }

        public static SqlCommand GetCommand()
        {
            if (_com == null)
            {
                _com = new SqlCommand();
            }
            return _com;
        }

        public static string Sanitize(string sql)
        {
            return sql.Replace("'", "''");
        }

        public static List<T> ExecuteSQLReturnObjectList<T>(string sql)
        {
            List<T> returnValues = new List<T>();
            DataResult fetchResult = new DataResult();

            lock (new object())
            {
                var command = GetCommand();
                command.Connection = GetConnection();
                command.CommandText = sql;
                command.CommandType = System.Data.CommandType.Text;
                try
                {
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        command.Connection.Open();

                    var dr = _com.ExecuteReader();
                    fetchResult.Values = new object[dr.FieldCount];
                    string[] fieldNames = new string[dr.FieldCount];

                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        fieldNames[i] = dr.GetName(i);
                    }
                    fetchResult.FieldNames = fieldNames;

                    while (dr.Read())
                    {
                        T populateObj = Activator.CreateInstance<T>();
                        dr.GetValues(fetchResult.Values);
                        ((IPopulate)populateObj).Populate(fetchResult);
                        returnValues.Add(populateObj);
                    }

                    dr.Close();
                }
                catch (Exception ex)
                {
                    //throw;
                }
                finally
                {
                    try
                    {
                        command.Connection.Close();
                    }
                    catch { }
                }
            }
            return returnValues;
        }

        public static void ExecuteSQL(string sql)
        {
            lock (new object())
            {
                var command = GetCommand();
                command.Connection = GetConnection();
                command.CommandText = sql;
                command.CommandType = System.Data.CommandType.Text;
                try
                {
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        command.Connection.Open();

                    int rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    command.Connection.Close();
                }
            }
        }

        public static object ExecuteSQLReturnScalar(string sql)
        {            
            object scalar = null;
            lock (new object())
            {
                var command = GetCommand();
                command.Connection = GetConnection();
                command.CommandText = sql;
                command.CommandType = System.Data.CommandType.Text;
                try
                {
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        command.Connection.Open();

                    scalar = command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    command.Connection.Close();
                }
            }
            return scalar;
        }

        public static object ExecuteInsert(string sql)
        {
            object scalar = null;
            lock (new object())
            {
                var command = GetCommand();
                command.Connection = GetConnection();
                command.CommandText = sql;
                command.CommandType = System.Data.CommandType.Text;
                try
                {
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        command.Connection.Open();

                    command.ExecuteNonQuery();
                    command.CommandText = "select @@Identity";
                    scalar = command.ExecuteScalar();
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    try
                    {
                        command.Connection.Close();
                    }
                    catch { }
                }
            }
            return scalar;
        }

        public static void Delete<T>(T deleteIt)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("delete from ");
            string tableName = GetTableName(deleteIt);
            sql.Append(tableName);
            var primaryKey = (deleteIt as IPopulate).GetPrimaryKey();
            sql.AppendFormat(" where {0} = {1}", primaryKey.Name, primaryKey.GetValue(deleteIt, null));

            ExecuteSQL(sql.ToString());
        }

        public static void Update<T>(T updateIt)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("update ");
            string tableName = GetTableName(updateIt);
            sql.Append(tableName);
            sql.Append(" set ");

            PropertyInfo[] props = updateIt.GetType().GetProperties();
            int idx = 0;
            int lastIdx = props.Length - 1;
            foreach (PropertyInfo p in props)
            {
                object[] customAttributes = p.GetCustomAttributes(false);
                if (p.PropertyType.Name == typeof(System.Collections.Generic.List<T>).Name ||
                    customAttributes.Any(attribute => attribute is PrimaryKey) ||
                    customAttributes.Any(attribute => attribute is Ignore))
                {
                    //we don't handle these types
                    idx++;
                    continue;
                }
                else
                {
                    string fieldName = string.Empty;
                    if (customAttributes != null && customAttributes.Any())
                    {
                        object columnAttribute = customAttributes.Where(att => att is ColumnMapping).FirstOrDefault();
                        if (columnAttribute != null && columnAttribute is ColumnMapping)
                        {
                            fieldName = ((ColumnMapping)columnAttribute).Name;
                        }
                        else
                        {
                            fieldName = p.Name;
                        }
                    }
                    else
                    {
                        fieldName = p.Name;
                    }
                    string expr = " {0} = {1}";
                    if (p.PropertyType.Name == "String")
                        expr = " {0} = '{1}'";

                    if (idx < lastIdx)
                        expr += ", ";
                    else
                        expr += " ";

                    sql.AppendFormat(expr, fieldName, Sanitize(p.GetValue(updateIt, null).ToString()));
                    idx++;
                }
            }

            string sqlStatement = sql.ToString();
            if (sqlStatement.Trim().EndsWith(","))
            {
                //replace the last , with nothing
                sqlStatement = sqlStatement.Substring(0, sqlStatement.LastIndexOf(","));
            }

            var primaryKey = (updateIt as IPopulate).GetPrimaryKey();
            sqlStatement += string.Format(" where {0} = {1}", primaryKey.Name, primaryKey.GetValue(updateIt, null));

            ExecuteSQL(sqlStatement);
        }

        public static void Create<T>(T createIt)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("insert into ");
            string tableName = GetTableName(createIt);
            sql.Append(tableName);            
            sql.Append(" (");

            PropertyInfo[] props = createIt.GetType().GetProperties();
            int idx = 0;
            int lastIdx = props.Length - 1;
            List<Tuple<string, object>> fields = new List<Tuple<string, object>>();

            foreach (PropertyInfo p in props)
            {
                object[] customAttributes = p.GetCustomAttributes(false);
                if (p.PropertyType.Name == typeof(System.Collections.Generic.List<T>).Name ||
                    customAttributes.Any(attribute => attribute is PrimaryKey) ||
                    customAttributes.Any(attribute => attribute is Ignore))
                {
                    //we don't handle these types
                    idx++;
                    continue;
                }
                else
                {
                    string fieldName = string.Empty;
                    if (customAttributes != null && customAttributes.Any())
                    {
                        object columnAttribute = customAttributes.Where(att => att is ColumnMapping).FirstOrDefault();
                        if (columnAttribute != null && columnAttribute is ColumnMapping)
                        {
                            fieldName = ((ColumnMapping)columnAttribute).Name;
                        }
                        else
                        {
                            fieldName = p.Name;
                        }
                    }
                    else
                    {
                        fieldName = p.Name;
                    }
                    fields.Add(new Tuple<string, object>(fieldName, p.GetValue(createIt, null)));
                    idx++;
                }
            }

            string fieldNames = string.Join(", ", fields.Select(thing => thing.Item1).ToArray());

            sql.Append(fieldNames);
            sql.Append(") values (");

            foreach (var thing in fields)
            {
                if (thing.Item2 is string)
                {
                    sql.AppendFormat("'{0}', ", Sanitize(thing.Item2.ToString()));
                }
                else
                {
                    sql.AppendFormat("{0}, ", thing.Item2);
                }
            }


            string sqlStatement = sql.ToString();
            if (sqlStatement.Trim().EndsWith(","))
            {
                //replace the last , with nothing
                sqlStatement = sqlStatement.Substring(0, sqlStatement.LastIndexOf(","));
            }

            sqlStatement += ")";
            object returned = ExecuteInsert(sqlStatement);

            PropertyInfo primaryKeyField = ((IPopulate)createIt).GetPrimaryKey();
            if (returned != DBNull.Value)//can't set the value if nothing was returned
            {
                if (primaryKeyField != null)
                    primaryKeyField.SetValue(createIt, Convert.ToInt32(returned), null);
            }
        }

        public static T Fetch<T>(T fetchIt) where T : class
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("select * from ");
            string tableName = GetTableName(fetchIt);
            sql.Append(tableName);
            sql.Append(" where ");

            var primaryKey = ((IPopulate)fetchIt).GetPrimaryKey();            
            if (primaryKey != null)
            {
                sql.Append(primaryKey.Name);
            }
            else
            {
                throw new ApplicationException(string.Format("No primary key declared (via attributes) on type {0}.", fetchIt.GetType().Name));
            }

            sql.Append(string.Format(" = {0}", primaryKey.GetValue(fetchIt, null)));

            var foundThings = ExecuteSQLReturnObjectList<T>(sql.ToString());

            if (foundThings != null)
                return foundThings.FirstOrDefault();

            return null;
        }

        private static string GetTableName(object fetchIt)
        {
            var customAtts = fetchIt.GetType().GetCustomAttributes(false);
            if (customAtts != null && customAtts.Any())
            {
                var tableAtt = customAtts.FirstOrDefault(att => att is TableMapping);
                if (tableAtt != null)
                {
                    return (tableAtt as TableMapping).Name;
                }
            }
            return fetchIt.GetType().Name;
        }        
    }
}
