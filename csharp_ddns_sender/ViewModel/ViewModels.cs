using ddns_setting.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ddns_setting.ViewModel
{
    public class UserData : BaseViewModel
    {
        public string User { get; set; }
        public string Auth { get; set; }
    }

    public class Config
    {
        public string GetUser { get; set; }
        public string SetUser { get; set; }
        public string AuthUser { get; set; }
    }

    public class Origins
    {
        public List<Origin> OriginData { get; set; }
    }

    public class Origin
    {
        public string Domain { get; set; }
        public string Record { get; set; }
    }
}
