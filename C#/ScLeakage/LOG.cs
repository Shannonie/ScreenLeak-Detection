using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Windows.Forms;

using ScLeakageCmd;

namespace LOG_space
{
    class LOG 
    {
        static public string ResultLogPath;
        public bool OutLog(string RecordData)
        {
            string NowTime = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            string LogStr = NowTime + " [Log]:" + RecordData;
            try
            {
                using (StreamWriter sw = new StreamWriter(ResultLogPath, true))
                { sw.WriteLine(LogStr); }
            }
            catch (Exception) { return false; }
            return true;
        }
    }
}
