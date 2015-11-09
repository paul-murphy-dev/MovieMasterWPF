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
    public static class LocalDataStore
    {
        

        public static LocalStore MovieData
        {
            get
            {
                return _movieData;
            }
        }

        static LocalDataStore()
        {
            _movieData.BeginInit();
            _movieData.EndInit();
        }

        public static string Sanitize(string sql)
        {
            return sql.Replace("'", "''");
        }
    }
}


