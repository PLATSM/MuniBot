// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EmptyBot v4.6.2

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MuniBot.Common;
using MuniBot.Common.Models;
using MuniBot.Data;
using MuniBot.Dialogs;
using MuniBot.Infraestructure.Luis;
using MuniBot.Infraestructure.QnAMakerAI;

namespace MuniBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Create conversation and user state with in-memory storage provider.
            IStorage storage = new MemoryStorage();
            ConversationState conversationState = new ConversationState(storage);
            UserState userState = new UserState(storage);

            // Create and register state accessors.
            // Accessors created here are passed into the IBot-derived class on every turn.
            services.AddSingleton<StateBotAccessors>(sp =>
            {
                // Create the custom state accessor.
                return new StateBotAccessors(conversationState, userState)
                {
                    ConversationDataAccessor = conversationState.CreateProperty<ConversationData>(StateBotAccessors.ConversationDataName),
                    UserProfileAccessor = userState.CreateProperty<UserProfile>(StateBotAccessors.UserProfileName),
                };
            });

            /*
                        var storage = new AzureBlobStorage(
                            Configuration.GetSection("StorageConnectionString").Value,
                            Configuration.GetSection("StorageContainer").Value
                            );
                        var userState = new UserState(storage);

                        services.AddSingleton(userState);

                        var conversationState = new ConversationState(storage);
                        services.AddSingleton(conversationState);
            */
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

/*
            services.AddDbContext<DataBaseService>(options =>
            {
                options.UseCosmos(
                    Configuration["CosmosEndPoint"],
                    Configuration["CosmosKey"],
                    Configuration["CosmosDataBase"]
                    );
            });
            services.AddScoped<IDataBaseService, DataBaseService>();
*/

            services.AddSingleton<ILuisService, LuisService>();
            services.AddSingleton<IQnAMakerAIService, QnAMakerAIService>();
            services.AddTransient<RootDialog>(); // porque vamos a utilizar dialogos externos


            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, MuniBot<RootDialog>>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
