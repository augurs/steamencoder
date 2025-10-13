using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncoderApp.Models
{
    public class AppItem
    {
        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public string IconPath { get; set; }

        public override string ToString()
        {
            return WindowTitle ?? ProcessName ?? base.ToString();
        }
    }

}
