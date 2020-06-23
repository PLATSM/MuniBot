using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using MuniBot.Client.Entities;
using Newtonsoft.Json;

namespace MuniBot.Client
{
    public class SolicitudLicenciaClient
    {

        public ResponseQuery InsertAsync(SolicitudLicenciaDTO solicitudLicencia)
        {

            ResponseQuery responseQuery = new ResponseQuery();

            var json = JsonConvert.SerializeObject(solicitudLicencia);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:4020/api/");
                var responseTask = client.PostAsync("SolicitudLicencia/InsertAsync", data);
                responseTask.Wait();

                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<ResponseQuery>();
                readTask.Wait();
                responseQuery = readTask.Result;
            }
            return responseQuery;
        }

        public ResponseQuery UpdatetAsync(SolicitudLicenciaDTO solicitudLicencia)
        {

            ResponseQuery responseQuery = new ResponseQuery();

            var json = JsonConvert.SerializeObject(solicitudLicencia);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:4020/api/");
                var responseTask = client.PutAsync("SolicitudLicencia/UpdateAsync", data);
                responseTask.Wait();

                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<ResponseQuery>();
                readTask.Wait();
                responseQuery = readTask.Result;
            }
            return responseQuery;
        }

        public ResponseQuery DeletetAsync(SolicitudLicenciaDTO solicitudLicencia)
        {

            ResponseQuery responseQuery = new ResponseQuery();

            var json = JsonConvert.SerializeObject(solicitudLicencia);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:4020/api/");
                var responseTask = client.PutAsync("SolicitudLicencia/DeleteAsync", data);
                responseTask.Wait();

                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<ResponseQuery>();
                readTask.Wait();
                responseQuery = readTask.Result;
            }
            return responseQuery;
        }

        public Response<SolicitudLicenciaDTO> GetAsync(SolicitudLicenciaDTO solicitudLicencia)
        {

            var response = new Response<SolicitudLicenciaDTO>();

            var json = JsonConvert.SerializeObject(solicitudLicencia);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:4020/api/");
                var responseTask = client.PostAsync("SolicitudLicencia/GetAsync", data);
                responseTask.Wait();

                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<Response<SolicitudLicenciaDTO>>();
                readTask.Wait();
                response = readTask.Result;

            }
            return response;
        }

    }
}
