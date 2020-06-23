using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using MuniBot.Common.Models.BotState;
using MuniBot.Common.Models.Tramite;
using MuniBot.Common.Models.User;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MuniBot.Dialogs.CrearTramite
{
    public class CrearTramiteDialog : ComponentDialog
    {
        public static UserModel newUserModel = new UserModel();
        public static TramiteModel tramiteModel = new TramiteModel();

        private readonly IStatePropertyAccessor<BotStateModel> _userState;


        public CrearTramiteDialog(UserState userState)
        {
            _userState = userState.CreateProperty<BotStateModel>(nameof(BotStateModel));

            var waterfallSteps = new WaterfallStep[]
            {
                SetPhone,
                SetFullName,
                SetEmail,
                SetDate,
                SetTime,
                SetConfirmation,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

        }

        private async Task<DialogTurnResult> SetPhone(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());
            if (userStateModel.tramiteData)
            {
                return await stepContext.NextAsync(cancellationToken:cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa tu número de teléfono:") },
                    cancellationToken
                    );
            }
        }

        private async Task<DialogTurnResult> SetFullName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());
            if (userStateModel.tramiteData)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var userPhone = stepContext.Context.Activity.Text;
                newUserModel.phone = userPhone;

                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("Ahora ingresa tu nombre completo") },
                    cancellationToken
                    );
            }
        }

        private async Task<DialogTurnResult> SetEmail(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());
            if (userStateModel.tramiteData)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var fullName = stepContext.Context.Activity.Text;
                newUserModel.fullName = fullName;

                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("Ahora ingresa tu correo") },
                    cancellationToken
                    );
            }
        }

        private async Task<DialogTurnResult> SetDate(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userMail = stepContext.Context.Activity.Text;
            newUserModel.email = userMail;

            string text = $"Ahora necesito la fecha del tramite con el siguiente formato: " + $"{Environment.NewLine}dd/mm/yyyy";

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text(text) },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> SetTime(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tramiteDate = stepContext.Context.Activity.Text;
            tramiteModel.date = Convert.ToDateTime(tramiteDate);

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = CreateButtonsTime() },
                cancellationToken
                );
        }

        private async Task<DialogTurnResult> SetConfirmation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tramiteTime = stepContext.Context.Activity.Text;
            tramiteModel.time = int.Parse(tramiteTime);

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = CreateButtonConfirmation()},
                cancellationToken
                );

        }


        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userConfirmation = stepContext.Context.Activity.Text;
            return await stepContext.ContinueDialogAsync(cancellationToken:cancellationToken);
        }
        private Activity CreateButtonsTime()
        {
            var reply = MessageFactory.Text("Ahora selecciona la hora");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){Title="9", Value="9", Type=ActionTypes.ImBack},
                    new CardAction(){Title="10", Value="10", Type=ActionTypes.ImBack},
                    new CardAction(){Title="11", Value="11", Type=ActionTypes.ImBack},
                    new CardAction(){Title="15", Value="15", Type=ActionTypes.ImBack},
                    new CardAction(){Title="16", Value="16", Type=ActionTypes.ImBack},
                    new CardAction(){Title="17", Value="17", Type=ActionTypes.ImBack},
                    new CardAction(){Title="18", Value="18", Type=ActionTypes.ImBack}
                }

            };
            return reply as Activity;
        }
        private Activity CreateButtonConfirmation()
        {
            var reply = MessageFactory.Text("Confirmar la creación del trámite");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){Title="Si", Value="Si", Type=ActionTypes.ImBack},
                    new CardAction(){Title="No", Value="No", Type=ActionTypes.ImBack}
                }

            };
            return reply as Activity;
        }

    }
}
