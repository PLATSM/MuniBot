using MuniBot.Client.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

namespace MuniBot.Client
{
    public class ClaseEstablecimientoClient
    {
        public Response<DataJsonDTO> GetListJsonAsync(ClaseEstablecimientoDTO claseEstablecimientoDTO)
        {
            var response = new Response<DataJsonDTO>();

            var json = JsonConvert.SerializeObject(claseEstablecimientoDTO);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:4020/api/");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + claseEstablecimientoDTO.no_token);
                var responseTask = client.PostAsync("ClaseEstablecimiento/GetListJsonAsync", data);
                responseTask.Wait();

                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<Response<DataJsonDTO>>();
                readTask.Wait();
                response = readTask.Result;

            }
            return response;
        }

    }
}
