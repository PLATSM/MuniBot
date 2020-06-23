using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using MuniBot.Client.Entities;
using MuniBot.Client.Entities.DataCard;
using Newtonsoft.Json;

namespace MuniBot.Client
{
    public class ContribuyenteClient
    {

        public ResponseQuery InsertAsync(ContribuyenteDTO contribuyente)
        {

            ResponseQuery responseQuery = new ResponseQuery();

            var json = JsonConvert.SerializeObject(contribuyente);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:4020/api/");
                var responseTask = client.PostAsync("Contribuyente/InsertAsync", data);
                responseTask.Wait();

                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<ResponseQuery>();
                readTask.Wait();
                responseQuery = readTask.Result;
            }
            return responseQuery;
        }

        public ResponseQuery UpdatetAsync(ContribuyenteDTO contribuyente)
        {

            ResponseQuery responseQuery = new ResponseQuery();

            var json = JsonConvert.SerializeObject(contribuyente);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:4020/api/");
                var responseTask = client.PutAsync("Contribuyente/UpdateAsync", data);
                responseTask.Wait();

                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<ResponseQuery>();
                readTask.Wait();
                responseQuery = readTask.Result;
            }
            return responseQuery;
        }

        public ResponseQuery DeletetAsync(ContribuyenteDTO contribuyente)
        {

            ResponseQuery responseQuery = new ResponseQuery();

            var json = JsonConvert.SerializeObject(contribuyente);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:4020/api/");
                var responseTask = client.PutAsync("Contribuyente/DeleteAsync", data);
                responseTask.Wait();

                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<ResponseQuery>();
                readTask.Wait();
                responseQuery = readTask.Result;
            }
            return responseQuery;
        }

        public Response<ContribuyenteDTO> GetLoginAsync(string co_documento_identidad, string nu_documento_identidad, string no_contrasena)
        {
            var response = new Response<ContribuyenteDTO>();

            ContribuyenteDTO contribuyente = new ContribuyenteDTO
            {
                co_documento_identidad = co_documento_identidad,
                nu_documento_identidad = nu_documento_identidad,
                no_contrasena = no_contrasena
            };

            var json = JsonConvert.SerializeObject(contribuyente);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:4020/api/");
                var responseTask = client.PostAsync("Contribuyente/GetLoginAsync", data);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<Response<ContribuyenteDTO>>();
                    readTask.Wait();

                    response = readTask.Result;
                }
                else
                {
                    var readTask = result.Content.ReadAsAsync<Response<ContribuyenteDTO>>();
                    readTask.Wait();

                    response = readTask.Result;
                }
            }
            return response;
        }

        public Response<ContribuyenteDTO> GetAsync(int id_contribuyente, string no_token)
        {
            var response = new Response<ContribuyenteDTO>();

            ContribuyenteDTO contribuyente = new ContribuyenteDTO
            {
                id_contribuyente = id_contribuyente,
                no_token = no_token
            };

            var json = JsonConvert.SerializeObject(contribuyente);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:4020/api/");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + contribuyente.no_token);
                var responseTask = client.PostAsync("Contribuyente/GetAsync", data);
                responseTask.Wait();

                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<Response<ContribuyenteDTO>>();
                readTask.Wait();
                response = readTask.Result;

            }
            return response;
        }
        public Response<DataJsonDTO> GetJsonAsync(int id_contribuyente, string no_token)
        {
            var response = new Response<DataJsonDTO>();

            ContribuyenteDTO contribuyente = new ContribuyenteDTO
            {
                id_contribuyente = id_contribuyente,
                no_token = no_token
            };

            var json = JsonConvert.SerializeObject(contribuyente);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:4020/api/");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + contribuyente.no_token);
                var responseTask = client.PostAsync("Contribuyente/GetJsonAsync", data);
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
