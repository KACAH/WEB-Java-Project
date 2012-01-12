using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Java_Project.Utilities
{
    public static class CultureHelper
    {
        private static readonly Dictionary<String, bool> _cultures = new Dictionary<string, bool> 
        {
            {"en-US", true},
            {"ru-RU", true},
            {"lv-LV", true}
        };

        public static string GetValidCulture(string name)
        {
            if (string.IsNullOrEmpty(name))
                return GetDefaultCulture();

            if (_cultures.ContainsKey(name))
                return name;

           foreach (var c in _cultures.Keys)
                if (c.StartsWith(name.Substring(0, 2)))
                    return c;
   
            return GetDefaultCulture();
        }

        public static string GetDefaultCulture()
        {
            return _cultures.Keys.ElementAt(0); // return Default culture

        }

        public static bool IsViewSeparate(string name)
        {
            return _cultures[name];
        }

    }
}