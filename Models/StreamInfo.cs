using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncoderApp.Models
{
    public class StreamInfo
    {
        public string Name { get; set; }       
        public string Mount { get; set; }        
        public bool IsConnected { get; set; }
        public bool? IsError { get; set; } = false;
    }
}
