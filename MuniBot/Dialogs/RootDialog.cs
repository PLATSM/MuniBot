using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using MuniBot.Common.Cards;
using MuniBot.Data;
using MuniBot.Dialogs.CrearTramite;
using MuniBot.Dialogs.Qualification;
using MuniBot.Infraestructure.Luis;
using MuniBot.Infraestructure.SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MuniBot.Dialogs
{
    public class RootDialog: ComponentDialog
    {
        private readonly ILuisService _luisService;
        private readonly IDataBaseService _databaseService;
        private readonly ISendGridEmailService _sendGridEmailService;

        public RootDialog(ILuisService luisService, IDataBaseService databaseService,UserState userState, ISendGridEmailService sendGridEmailService)
        {
            _luisService = luisService;
            _databaseService = databaseService;
            _sendGridEmailService = sendGridEmailService;

            var waterfallSteps = new WaterfallStep[]
            {
                InitialProcess,
                FinalProcess
            };
            AddDialog(new QualificationDialog(_databaseService));
            AddDialog(new CrearTramiteDialog(_databaseService,userState, _sendGridEmailService));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog),waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisService._luisRecognizer.RecognizeAsync(stepContext.Context,cancellationToken);
            return await ManageIntentions(stepContext, luisResult, cancellationToken);

        }

        private async Task<DialogTurnResult> ManageIntentions(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            var topIntent = luisResult.GetTopScoringIntent();
            switch (topIntent.intent)
            {
                case "Saludar":
                    await IntentSaludar(stepContext, luisResult, cancellationToken);
                    break;
                case "Agradecer":
                    await IntentAgradecer(stepContext, luisResult, cancellationToken);
                    break;
                case "Despedir":
                    await IntentDespedir(stepContext, luisResult, cancellationToken);
                    break;
                case "VerOpciones":
                    await IntentVerOpciones(stepContext, luisResult, cancellationToken);
                    break;
                case "Contactar":
                    await IntentContactar(stepContext, luisResult, cancellationToken);
                    break;
                case "CalificarBot":
                    return await IntentCalificar(stepContext, luisResult, cancellationToken);
                case "SolicitarTramite":
                    return await IntentSolicitarTramite(stepContext, luisResult, cancellationToken);
                case "ConsultarTramite":
                    await IntentConsultarTramite(stepContext, luisResult, cancellationToken);
                    break;
                case "None":
                    await IntentNone(stepContext, luisResult, cancellationToken);
                    break;
                default:
                    break;

            }
            return await stepContext.NextAsync(cancellationToken:cancellationToken); // para que salte al siguiente método
        }

        #region IntentLuis;
        private async Task IntentConsultarTramite(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Un momento por favor...", cancellationToken: cancellationToken);
            await Task.Delay(1000);
            string idUser = stepContext.Context.Activity.From.Id;

            var tramiteData = _databaseService.Tramite.Where(x=> x.idUser == idUser).ToList();
            if (tramiteData.Count > 0)
            {
                var tramiteLista = tramiteData.Where(p => p.date >= DateTime.Now.Date).ToList();

                if (tramiteLista.Count > 0)
                {
                    await stepContext.Context.SendActivityAsync("Estas son tus trámites",cancellationToken:cancellationToken);
                    foreach (var item in tramiteLista)
                    {
                        await Task.Delay(1000);

                        if (item.date == DateTime.Now.Date && item.time < DateTime.Now.Hour)
                            continue;

                        var tramiteSummary = $"Fecha: {item.date.ToShortDateString()}" + $"{Environment.NewLine}Hora: {item.time}";

                        await stepContext.Context.SendActivityAsync(tramiteSummary,cancellationToken:cancellationToken);
                    }
                }
                else
                    await stepContext.Context.SendActivityAsync("No se encontró información", cancellationToken: cancellationToken);
            }
            else
                await stepContext.Context.SendActivityAsync("No se encontró información", cancellationToken: cancellationToken);
        }
        private async Task<DialogTurnResult> IntentSolicitarTramite(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(CrearTramiteDialog),cancellationToken:cancellationToken);
        }   
        private async Task<DialogTurnResult> IntentCalificar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(QualificationDialog), cancellationToken: cancellationToken);
        }

        private async Task IntentContactar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            string phoneDetail = $"Nuestro números de atención son los siguientes:{Environment.NewLine}"+
                $"+51 66778888{Environment.NewLine} +51 456789258";

            string addresDetail = $"Av.Saenz Peña 199, Callao, Lima";

            await stepContext.Context.SendActivityAsync(phoneDetail,cancellationToken:cancellationToken);
            await Task.Delay(1000);
            await stepContext.Context.SendActivityAsync(addresDetail, cancellationToken: cancellationToken);
            await Task.Delay(1000);
            await stepContext.Context.SendActivityAsync("En que más te puedo ayudar?", cancellationToken: cancellationToken);
        }

        private async Task IntentVerOpciones(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Aqui tengo mis opciones:", cancellationToken: cancellationToken);
            await MainOptionsCard.ToShow(stepContext, cancellationToken);
        }

        private async Task IntentSaludar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Hola, que gusto verte.",cancellationToken:cancellationToken);
        }

        private async Task IntentAgradecer(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("No te preocupes me gusta ayudar", cancellationToken: cancellationToken);
        }

        private async Task IntentDespedir(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Espero verte pronto.", cancellationToken: cancellationToken);
        }

        private async Task IntentNone(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("No entiendo lo que me dices.", cancellationToken: cancellationToken);
        }

        #endregion


        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }

}
