using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using MuniBot.Common.Cards;
using MuniBot.Data;
using MuniBot.Dialogs.CrearTramite;
using MuniBot.Dialogs.Qualification;
using MuniBot.Infraestructure.Luis;
using MuniBot.Infraestructure.QnAMakerAI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MuniBot.Common;

namespace MuniBot.Dialogs
{

    public class RootDialog: ComponentDialog
    {
        private readonly ILuisService _luisService;
        private readonly IQnAMakerAIService _qnaMakerAIService;
        // private readonly IDataBaseService _databaseService;

        public RootDialog(ILuisService luisService, StateBotAccessors accessors, IQnAMakerAIService qnaMakerAIService)
        {
            _luisService = luisService;
            _qnaMakerAIService = qnaMakerAIService;
            // _databaseService = databaseService;

            var waterfallSteps = new WaterfallStep[]
            {
                InitialProcess,
                FinalProcess
            };
            //AddDialog(new QualificationDialog(_databaseService));
            //AddDialog(new TextPrompt(nameof(TextPrompt)));
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

            if (string.IsNullOrEmpty(stepContext.Context.Activity.Text) == false)
            {
                string activityText = stepContext.Context.Activity.Text.ToLower();

                if (activityText.IndexOf("requisito", 0) > -1)
                {
                    topIntent.score = 0;
                }
            }

            if (topIntent.score < 0.5)
            {
                await IntentNone(stepContext, luisResult, cancellationToken);
            }
            else
            {
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
                    case "CalificarBot":
                        return await IntentCalificar(stepContext, luisResult, cancellationToken);
                    case "RealizarTramite":
                        await IntentRealizarTramite(stepContext, luisResult, cancellationToken);
                        break;
                    case "RealizarTramiteLicencia":
                        await IntentRealizarTramiteLicencia(stepContext, luisResult, cancellationToken);
                        break;
                    case "RealizarTramiteAlcabala":
                        await IntentRealizarTramiteAlcabala(stepContext, luisResult, cancellationToken);
                        break;
                    case "RealizarTramiteVehicular":
                        await IntentRealizarTramiteVehicular(stepContext, luisResult, cancellationToken);
                        break;
                    case "ConsultarTramite":
                        await IntentConsultarTramite(stepContext, luisResult, cancellationToken);
                        break;
                    case "CrearCuenta":
                        await IntentCrearCuenta(stepContext, luisResult, cancellationToken);
                        break;
                    case "Login":
                        await IntentLogin(stepContext, luisResult, cancellationToken);
                        break;
                    case "CrearTramiteLicencia":
                        await IntentCrearTramiteLicencia(stepContext, luisResult, cancellationToken);
                        break;
                    case "None":
                        await IntentNone(stepContext, luisResult, cancellationToken);
                        break;
                    default:
                        break;

                }
            }

            return await stepContext.NextAsync(cancellationToken:cancellationToken); // para que salte al siguiente método
        }

        #region IntentLuis;
        private async Task IntentLogin(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            if (Globales.OnSesion)
            {
                await buttonsInicio(stepContext, cancellationToken, $"Su sesión ya fue iniciada como {Globales.no_contribuyente}");
            }
            else
            {
                AdaptiveCardList adaptiveCard = new AdaptiveCardList();
                var nameCard = adaptiveCard.CreateAttachment(2,"");
                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(nameCard), cancellationToken);
                await Task.Delay(500);
            }
            await buttonsInicio(stepContext, cancellationToken, "");
        }
        private async Task IntentCrearCuenta(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await buttonsTipoPersona(stepContext, cancellationToken);
        }
        private async Task IntentCrearTramiteLicencia(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            AdaptiveCardList adaptiveCard = new AdaptiveCardList();
            var nameCard = adaptiveCard.CreateAttachment(3,"");
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(nameCard), cancellationToken);
            await Task.Delay(500);
            await buttonsInicio(stepContext, cancellationToken, "");
        }
        private async Task IntentConsultarTramite(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await buttonsTramite(stepContext, cancellationToken);
        }
        private async Task IntentRealizarTramite(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await buttonsTramite(stepContext, cancellationToken);
        }
        private async Task IntentRealizarTramiteLicencia(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await buttonsLicenciaFuncionamiento(stepContext, cancellationToken);
        }
        private async Task IntentRealizarTramiteAlcabala(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await buttonsImpuestoAlcabala(stepContext, cancellationToken);
        }
        private async Task IntentRealizarTramiteVehicular(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await buttonsImpuestoVehicular(stepContext, cancellationToken);
        }
        private async Task<DialogTurnResult> IntentCalificar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(QualificationDialog), cancellationToken: cancellationToken);
        }

        private async Task IntentSaludar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await buttonsInicio(stepContext, cancellationToken, $"Hola, que gusto verte, en que te puedo ayudar?");
        }

        private async Task IntentAgradecer(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await buttonsInicio(stepContext, cancellationToken, $"gracias a ti, en que te puedo ayudar?");
        }

        private async Task IntentDespedir(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await buttonsInicio(stepContext, cancellationToken, $"Espero verte pronto {Globales.no_contribuyente}.");
        }

        private async Task IntentNone(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            var resultQnA = await _qnaMakerAIService._qnaMakerResult.GetAnswersAsync(stepContext.Context);

            var score = resultQnA.FirstOrDefault()?.Score; // Capturo el puntaje
            string response = resultQnA.FirstOrDefault()?.Answer; // Capturo la respuesta que devuelve Qna Maker

            if (score >= 0.5)
            {
                await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
                await buttonsInicio(stepContext, cancellationToken, $"En que te puedo ayudar?");
            }
            else
            {
                await Task.Delay(500);
                await buttonsInicio(stepContext, cancellationToken, $"No entiendo lo que me dices, puedes utilizar los botones de la parte inferior.");
            }
        }

        #endregion


        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static async Task buttonsInicio(WaterfallStepContext stepContext, CancellationToken cancellationToken, string message)
        {
            var reply = MessageFactory.Text($"{message}");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Trámites", Type = ActionTypes.ImBack, Value = "Seleccionar Trámite" },
                    new CardAction() { Title = "Iniciar Sesion", Type = ActionTypes.ImBack, Value = "Iniciar Sesion" },
                    new CardAction() { Title = "Crear una cuenta", Type = ActionTypes.ImBack, Value = "Crear una cuenta" },
                    new CardAction() { Title = "Contactos", Type = ActionTypes.ImBack, Value = "Contactos" },
                },
            };
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
        }
        private static async Task buttonsTramite(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"Que trámite deseas realizar?");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Inicio", Type = ActionTypes.ImBack, Value = "Inicio" },
                    new CardAction() { Title = "Licencia Funcionamiento", Type = ActionTypes.ImBack, Value = "Trámite Licencia de Funcionamiento" },
                    new CardAction() { Title = "Impuesto Alcabala", Type = ActionTypes.ImBack, Value = "Trámite Impuesto de Alcabala" },
                    new CardAction() { Title = "Impuesto Vehicular", Type = ActionTypes.ImBack, Value = "Trámite Impuesto Vehicular"},
                },
            };
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
        }
        private static async Task buttonsLicenciaFuncionamiento(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"Selecciona una opción");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Inicio", Type = ActionTypes.ImBack, Value = "Inicio" },
                    new CardAction() { Title = "Nueva Licencia", Type = ActionTypes.ImBack, Value = "Nuevo Trámite Licencia de Funcionamiento" },
                    new CardAction() { Title = "Consultar Licencias", Type = ActionTypes.ImBack, Value = "Consultar Licencias de Funcionamiento" },
                    new CardAction() { Title = "Requisitos", Type = ActionTypes.ImBack, Value = "Requisitos Licencia de Funcionamiento" },
                },
            };
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
        }
        private static async Task buttonsImpuestoAlcabala(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"Selecciona una opción");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Inicio", Type = ActionTypes.ImBack, Value = "Inicio" },
                    new CardAction() { Title = "Nuevo Trámite", Type = ActionTypes.ImBack, Value = "Nuevo Trámite Impuesto de Alcabala" },
                    new CardAction() { Title = "Consultar Trámites", Type = ActionTypes.ImBack, Value = "Consultar Trámites Impuesto de Alcabala" },
                    new CardAction() { Title = "Requisitos", Type = ActionTypes.ImBack, Value = "Requisitos Impuesto de Alcabala" },
                },
            };
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
        }
        private static async Task buttonsImpuestoVehicular(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"Selecciona una opción");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Inicio", Type = ActionTypes.ImBack, Value = "Inicio" },
                    new CardAction() { Title = "Nuevo Trámite", Type = ActionTypes.ImBack, Value = "Nuevo Trámite Impuesto Vehicular" },
                    new CardAction() { Title = "Consultar Trámites", Type = ActionTypes.ImBack, Value = "Consultar Trámites Impuesto Vehicular" },
                    new CardAction() { Title = "Requisitos", Type = ActionTypes.ImBack, Value = "Requisitos Impuesto Vehicular" },
                },
            };
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
        }
        private static async Task buttonsTipoPersona(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"Selecciona un tipo de persona");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Inicio", Type = ActionTypes.ImBack, Value = "Inicio" },
                    new CardAction() { Title = "Persona Natural", Type = ActionTypes.ImBack, Value = "Cuenta Persona Natural" },
                    new CardAction() { Title = "Persona Juridica", Type = ActionTypes.ImBack, Value = "Cuenta Persona Juridica" },
                },
            };
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
        }


    }
}
