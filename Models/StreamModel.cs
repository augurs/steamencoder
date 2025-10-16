using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncoderApp.Models
{
    public class StreamModel
    {
        public string Name { get; set; }
        public string ServerType { get; set; }
        public string HostNameOrIP { get; set; }
        public int Port { get; set; }
        public string Mount { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AudioCodec { get; set; }
        public string SelectNWInterface { get; set; }
        public bool IsConnected { get; set; } = true;
    }


}
