// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using MuniBot.Common.Models.User;
using MuniBot.Data;
using Newtonsoft.Json;

namespace MuniBot
{
    public class MuniBot<T> : ActivityHandler where T:Dialog
    {
        private readonly BotState _userState;
        private readonly BotState _conversationState;
        private readonly Dialog _dialog;
        private readonly IDataBaseService _databaseService;

        private const string WelcomeMessage = "Bienvenido al asistente virtual de la Municipalidad!";

        public MuniBot(UserState userState,ConversationState conversationState, T dialog, IDataBaseService databaseService) {
            _userState = userState;
            _conversationState = conversationState;
            _dialog = dialog;
            _databaseService = databaseService;
        }
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // await turnContext.SendActivityAsync(MessageFactory.Text($"Bienvenido al asistente virtual de la Municipalidad!"), cancellationToken);
                    // await turnContext.SendActivityAsync(WelcomeMessage, cancellationToken: cancellationToken);

                    var welcomeCard = CreateWelcomeCard();
                    var responseWelcome = MessageFactory.Attachment(welcomeCard, ssml: "Bienvenido!");
                    await turnContext.SendActivityAsync(responseWelcome, cancellationToken);
                    await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                    await Task.Delay(500);

                    //var menuCard = CreateMenuCard();
                    //var responseMenu = MessageFactory.Attachment(menuCard, ssml: "Menu");
                    //await turnContext.SendActivityAsync(responseMenu, cancellationToken);
                    //await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                    //await Task.Delay(500);

                    await turnContext.SendActivityAsync("En que te puedo ayudar?", cancellationToken: cancellationToken);

                    //await MainOptionsCard.ToShow(_conversationState., cancellationToken);

                    //return await _dialog.BeginDialogAsync(nameof(RootDialog), rootDialog, cancellationToken);

                }
            }
        }

        // Load attachment from embedded resource.
        private Attachment CreateWelcomeCard()
        {
            var cardResourcePath = "MuniBot.Common.json.welcomeCard.json";

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();
                    return new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JsonConvert.DeserializeObject(adaptiveCard),
                    };
                }
            }
        }

        private Attachment CreateMenuCard()
        {
            var cardResourcePath = "MuniBot.Common.json.menuCard.json";

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();
                    return new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JsonConvert.DeserializeObject(adaptiveCard),
                    };
                }
            }

        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _userState.SaveChangesAsync(turnContext,false,cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //var userMessage = turnContext.Activity.Text;
            //await turnContext.SendActivityAsync($"User: {userMessage}",cancellationToken:cancellationToken);

            await SaveUser(turnContext);

            await _dialog.RunAsync(
                turnContext,
                _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                cancellationToken
                );
        }

        private async Task SaveUser(ITurnContext<IMessageActivity> turnContext)
        {
            var userModel = new UserModel();
            userModel.id = turnContext.Activity.From.Id;
            userModel.userNameChannel= turnContext.Activity.From.Name;
            userModel.channel = turnContext.Activity.ChannelId;
            userModel.registerDate = DateTime.Now.Date;

            var user = await _databaseService.User.FirstOrDefaultAsync(x => x.id == turnContext.Activity.From.Id);

            if (user == null)
            {
                await _databaseService.User.AddAsync(userModel);
                await _databaseService.SaveAsync();
            }
        }
    }
}
