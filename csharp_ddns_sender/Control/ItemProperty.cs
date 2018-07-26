using csharp_ddns_sender.UI;
using ddns_setting.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ddns_setting.Control
{
    public class ItemProperty : BaseSingleton<ItemProperty>
    {
        public List<LogItem> logItems { get; set; }
        public List<RecordItem> AllOriginItems { get; set; }
        public List<RecordItem> ChoiceOriginItems { get; set; }

        public ItemProperty()
        {
            logItems = new List<LogItem>();
            AllOriginItems = new List<RecordItem>();
            ChoiceOriginItems = new List<RecordItem>();
            logItems.Add(new LogItem() { Log = "프로그램이 시작되었습니다.", Time = DateTime.Now });
        }

        public void ChoiceOrigin(RecordItem item)
        {
            if (AllOriginItems.Contains(item))
            {
                AllOriginItems.Remove(item);
                ChoiceOriginItems.Add(item);
            }
        }

        public void UnChoiceOrigin(RecordItem item)
        {
            if (ChoiceOriginItems.Contains(item))
            {
                ChoiceOriginItems.Remove(item);
                AllOriginItems.Add(item);
            }
        }
    }
}
