// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EmptyBot v4.6.2

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MuniBot
{
    static class Globales
    {
        public static bool OnSesion = false;
        public static int id_empresa = 1;
        public static int id_contribuyente = 0;
        public static string no_token = string.Empty;
        public static string no_contribuyente = string.Empty;
        public static string idCard = string.Empty;
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
