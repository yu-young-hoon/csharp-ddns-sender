using ddns_setting.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ddns_setting.Control
{
    public class Logger : BaseSingleton<Logger>
    {
        Mutex mutex;
        public Logger(){
            mutex = new Mutex();
        }

        public void AppendErrorLog(Exception ex)
        {
            mutex.WaitOne();
            string logFile = ".\\DDNS-ERROR-" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            System.IO.StreamWriter file = new System.IO.StreamWriter(logFile, true);
            file.WriteLine(ex.StackTrace);
            file.WriteLine(ex.GetType().FullName + ex.Message);
            file.Close();
            mutex.ReleaseMutex();
        }
    }
}
