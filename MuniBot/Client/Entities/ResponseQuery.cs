using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuniBot.Client.Entities
{
    public class ResponseQuery
    {
        public int id_identity { get; set; }
        public int error_number { get; set; }
        public string error_message { get; set; }
    }
}
