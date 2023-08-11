using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PCShop.Models.Common
{
    
    public class SettingsHelper
    {
        private static ApplicationDbContext _context = new ApplicationDbContext();

        public static string GetValue(string key)
        {
            var item = _context.systemSettings.Where(x => x.settingKey == key).FirstOrDefault();
            if (item != null)
            {
                return item.settingValue;
            }
            return null;
        }
    }
}