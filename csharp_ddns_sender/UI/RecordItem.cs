using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace csharp_ddns_sender.UI
{
    public class RecordItem
    {
        public string Origin { get { return SubDomain + "." + Domain; } }
        public string SubDomain { get; set; }
        public string Domain { get; set; }
    }
}
