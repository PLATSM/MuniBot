using Microsoft.AspNetCore.Mvc;
using MuniBot.Client.Entities;
using MuniBot.Client.Entities.Reniec;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace MuniBot.Client
{
    public class ReniecClient
    {
        public ReniecResponse GetReniecAsync(string nuDNI)
        {
            var responseReniec = new ReniecResponse();

            ReniecDNI reniecDNI = new ReniecDNI
            {
                dni = nuDNI
            };

            var json = JsonConvert.SerializeObject(reniecDNI);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            //    var content = new FormUrlEncodedContent(new[]
            //    {
            //     new KeyValuePair<string, string>("dni", "25629432")
            //});

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://api.municallao.gob.pe/pide/public/v1/consulta-dni");
                var result = client.PostAsync(client.BaseAddress, data).Result;
                //var result = client.PostAsync("", content).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;

                JObject DNISearch = JObject.Parse(resultContent);

                // Obtener la propiedades result en una lista 
                //IList<JToken> results = DNISearch["consultarResponse"]["return"]["datosPersona"].Children().ToList();

                ReniecDatosPersona persona = new ReniecDatosPersona();

                if (resultContent.IndexOf("prenombres") >= 0)
                    persona.prenombres = DNISearch["consultarResponse"]["return"]["datosPersona"]["prenombres"].ToString();

                if (resultContent.IndexOf("apPrimer") >= 0)
                    persona.apPrimer = DNISearch["consultarResponse"]["return"]["datosPersona"]["apPrimer"].ToString();

                if (resultContent.IndexOf("apSegundo") >= 0)
                    persona.apSegundo = DNISearch["consultarResponse"]["return"]["datosPersona"]["apSegundo"].ToString();

                if (resultContent.IndexOf("direccion") >= 0)
                    persona.direccion = DNISearch["consultarResponse"]["return"]["datosPersona"]["direccion"].ToString();

                if (resultContent.IndexOf("estadoCivil") >= 0)
                    persona.estadoCivil = DNISearch["consultarResponse"]["return"]["datosPersona"]["estadoCivil"].ToString();

                if (resultContent.IndexOf("foto") >= 0)
                    persona.foto = DNISearch["consultarResponse"]["return"]["datosPersona"]["foto"].ToString();

                if (resultContent.IndexOf("restriccion") >= 0)
                    persona.restriccion = DNISearch["consultarResponse"]["return"]["datosPersona"]["restriccion"].ToString();

                if (resultContent.IndexOf("ubigeo") >= 0)
                    persona.ubigeo = DNISearch["consultarResponse"]["return"]["datosPersona"]["ubigeo"].ToString();

                responseReniec.datosPersona = persona;
                responseReniec.coResultado = DNISearch["consultarResponse"]["return"]["coResultado"].ToString();
                responseReniec.deResultado = DNISearch["consultarResponse"]["return"]["deResultado"].ToString();

            }
            return responseReniec;
        }
        public Response<ReniecDTO> GetLocalAsync(string nuDNI, string no_token)
        {
            var response = new Response<ReniecDTO>();

            ReniecDTO reniec = new ReniecDTO
            {
                nuDNI = nuDNI,
                no_token = no_token
            };

            var json = JsonConvert.SerializeObject(reniec);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:4020/api/");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + reniec.no_token);
                var responseTask = client.PostAsync("Reniec/GetAsync", data);
                responseTask.Wait();

                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<Response<ReniecDTO>>();
                readTask.Wait();
                response = readTask.Result;
            }
            return response;
        }

    }
}
