using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace MovieMaster.Helpers
{
    public static class JSONConverter
    {
        private static JavaScriptSerializer jss = new JavaScriptSerializer();

        public static T Convert<T>(string input)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Deserialize<T>(input);
        }

        //public static T Convert<T>(string input)
        //{
        //    T instance = Activator.CreateInstance<T>();

        //    string outermostClass = string 


        //    return instance;
        //}
    }
}
