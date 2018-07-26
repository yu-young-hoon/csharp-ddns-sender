using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ddns_setting.Module
{
    public class BaseSingleton<T> where T : class, new()
    {
        protected static readonly Lazy<T> instance = new Lazy<T>(() => new T());
        public static T GetInstance
        {
            get
            {
                return instance.Value;
            }
        }
    }
}
