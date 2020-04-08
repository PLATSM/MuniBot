using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuniBot.Common.Models.Tramite
{
    public class TramiteModel
    {
        public string id { get; set; }
        public string idUser { get; set; }
        public DateTime date { get; set; }
        public int time { get; set; }
    }
}
