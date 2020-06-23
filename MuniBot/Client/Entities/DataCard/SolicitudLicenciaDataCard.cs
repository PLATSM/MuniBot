using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuniBot.Client.Entities.DataCard
{
    public class SolicitudLicenciaDataCard
    {
        public string nu_solicitud_licencia { get; set; }
        public string no_contribuyente { get; set; }
        public string co_tipo_persona { get; set; }
        public string no_tipo_persona { get; set; }
        public string co_documento_identidad { get; set; }
        public string nu_documento_identidad { get; set; }
        public string no_nombres { get; set; }
        public string no_apellido_paterno { get; set; }
        public string no_apellido_materno { get; set; }
        public string no_razon_social { get; set; }
        public string nu_telefono { get; set; }
        public string no_direccion { get; set; }
        public string no_correo_electronico { get; set; }
    }
}
