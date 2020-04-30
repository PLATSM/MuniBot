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
using MuniBot.Client;
using MuniBot.Common;
using MuniBot.Common.Cards;
using MuniBot.Common.Models.User;
using MuniBot.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MuniBot
{
    public class MuniBot<T> : ActivityHandler where T:Dialog
    {
        private readonly BotState _userState;
        private readonly BotState _conversationState;
        private readonly Dialog _dialog;
        private readonly IDataBaseService _databaseService;

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
                    Globales.OnSesion = false;
                    Globales.co_documento_identidad = string.Empty;
                    Globales.nu_documento_identidad = string.Empty;
                    Globales.no_nombres = string.Empty;
                    Globales.no_apellido_paterno = string.Empty;

                    AdaptiveCard adaptiveCard = new AdaptiveCard();
                    var welcomeCard = adaptiveCard.CreateAttachment(0);
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(welcomeCard), cancellationToken);
                    await Task.Delay(500);

                    await buttonsInicio(turnContext, cancellationToken,$"En que te puedo ayudar?\n\n puedes utilizar los botones de la parte inferior.");
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {

            var activityText = turnContext.Activity.Text;
            var activityId = turnContext.Activity.Id;

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (turnContext.Activity.Value == null)
                {

                    //var newActivity = MessageFactory.Text("The new text for the activity");
                    //newActivity.Id = activityId;
                    //await turnContext.UpdateActivityAsync(newActivity, cancellationToken);

                    if (
                            //activityText == "Trámite Licencia de Funcionamiento" ||
                            activityText == "Nuevo Trámite Licencia de Funcionamiento" ||
                            activityText == "Consultar Licencias de Funcionamiento" ||
                            //activityText == "Trámite Impuesto de Alcabala" ||
                            activityText == "Nuevo Trámite Impuesto de Alcabala" ||
                            activityText == "Consultar Trámites Impuesto de Alcabala" ||
                            //activityText == "Trámite Impuesto Vehicular" ||
                            activityText == "Nuevo Trámite Impuesto Vehicular" ||
                            activityText == "Consultar Trámites Impuesto Vehicular" ||
                            activityText == "Estado de Cuenta"
                        ) 
                    {
                        //if (Globales.idCard != String.Empty)
                        //{
                        //    await turnContext.DeleteActivityAsync(Globales.idCard, cancellationToken);
                        //    Globales.idCard = string.Empty;
                        //}

                        if (Globales.OnSesion == false)
                        {
                            await turnContext.SendActivityAsync($"Para utilizar la opcion **{activityText}** debes **Iniciar Sesión** con tu documento de identidad y contraseña, si no estás registrado, debes crear una cuenta de acceso con tus datos personales.", cancellationToken: cancellationToken);
                            await buttonsInicio(turnContext, cancellationToken, $"En que te puedo ayudar {Globales.no_nombres}?");
                        }
                    }
                }

                if (turnContext.Activity.Value != null)
                {
                    String value = turnContext.Activity.Value.ToString();
                    JObject results = JObject.Parse(value);

                    // Get type from input field
                    String nameCard = results.GetValue("id").ToString();

                    // Crear Cuenta
                    if (nameCard == "createUserCard")
                    {
                        if 
                        (
                            string.IsNullOrEmpty(results.GetValue("txtNombres").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtApellidoPaterno").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtApellidoMaterno").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("cboTipoDocumento").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtDocumentoIdentidad").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtCorreoElectronico").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtContrasena").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtVerificarContrasena").ToString())
                        )
                        {
                            await buttonsInicio(turnContext, cancellationToken, "Todos los campos son obligatorios.");
                        }
                        else
                        {
                            if (results.GetValue("txtContrasena").ToString() != results.GetValue("txtVerificarContrasena").ToString())
                            {
                                await buttonsInicio(turnContext, cancellationToken, "Contraseña y Verificar Contraseña deben ser iguales.");
                            }
                            else
                            {
                                ContribuyenteDTO contribuyenteDTO = new ContribuyenteDTO();
                                contribuyenteDTO.id_empresa = 1;
                                contribuyenteDTO.no_nombres = results.GetValue("txtNombres").ToString();
                                contribuyenteDTO.no_apellido_paterno = results.GetValue("txtApellidoPaterno").ToString();
                                contribuyenteDTO.no_apellido_materno = results.GetValue("txtApellidoMaterno").ToString();
                                contribuyenteDTO.co_documento_identidad = results.GetValue("cboTipoDocumento").ToString();
                                contribuyenteDTO.nu_documento_identidad = results.GetValue("txtDocumentoIdentidad").ToString();
                                contribuyenteDTO.no_correo_electronico = results.GetValue("txtCorreoElectronico").ToString();
                                contribuyenteDTO.no_contrasena = results.GetValue("txtContrasena").ToString();

                                ContribuyenteClient contribuyenteClient = new ContribuyenteClient();
                                var result = contribuyenteClient.InsertAsync(contribuyenteDTO);

                                if (result.error_number == 0)
                                    await buttonsInicio(turnContext, cancellationToken, "Se ha creado su cuenta exitosamente.");
                                else
                                    await buttonsInicio(turnContext, cancellationToken, $"{result.error_message}");
                            }
                        }                       
                    }

                    // Login
                    if (nameCard == "loginCard") 
                    {
                        if (Globales.OnSesion)
                        {
                            await buttonsInicio(turnContext, cancellationToken, $"Su sesión ya fue iniciada como {Globales.no_nombres} {Globales.no_apellido_paterno}");
                        }
                        else
                        {
                            if (
                                string.IsNullOrEmpty(results.GetValue("cboTipoDocumento").ToString()) ||
                                string.IsNullOrEmpty(results.GetValue("txtNumeroDocumento").ToString()) ||
                                string.IsNullOrEmpty(results.GetValue("txtContrasena").ToString()))
                            {
                                await buttonsInicio(turnContext, cancellationToken, "Ingrese documento de identidad/contraseña");
                            }
                            else
                            {
                                var co_documento_identidad = results.GetValue("cboTipoDocumento").ToString();
                                var nu_documento_identidad = results.GetValue("txtNumeroDocumento").ToString();
                                var no_contrasena = results.GetValue("txtContrasena").ToString();

                                ContribuyenteClient contribuyenteClient = new ContribuyenteClient();
                                var result = contribuyenteClient.GetLoginAsync(co_documento_identidad, nu_documento_identidad, no_contrasena);

                                if (result.error_number == 0)
                                {
                                    Globales.OnSesion = true;
                                    Globales.co_documento_identidad = result.Data.co_documento_identidad;
                                    Globales.nu_documento_identidad = result.Data.nu_documento_identidad;
                                    Globales.no_nombres = result.Data.no_nombres;
                                    Globales.no_apellido_paterno = result.Data.no_apellido_paterno;
                                    await buttonsInicio(turnContext, cancellationToken, $"Hola {result.Data.no_nombres}, en que te puedo ayudar?");

                                }
                                else
                                {
                                    Globales.OnSesion = false;
                                    Globales.co_documento_identidad = string.Empty;
                                    Globales.nu_documento_identidad = string.Empty;
                                    Globales.no_nombres = string.Empty;
                                    Globales.no_apellido_paterno = string.Empty;
                                    await buttonsInicio(turnContext, cancellationToken, $"{result.error_message}");
                                }
                            }

                        }
                    }       
                }
            }

            await base.OnTurnAsync(turnContext, cancellationToken);
            //await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
            //await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var GoRootDialog = true;
            var activityText = turnContext.Activity.Text;
            var activityId = turnContext.Activity.Id;


            if (turnContext.Activity.Type == ActivityTypes.Message && turnContext.Activity.Value == null)
            {
                if (
                        activityText == "Trámite Licencia de Funcionamiento" ||
                        activityText == "Nuevo Trámite Licencia de Funcionamiento" ||
                        activityText == "Consultar Licencias de Funcionamiento" ||
                        activityText == "Trámite Impuesto de Alcabala" ||
                        activityText == "Nuevo Trámite Impuesto de Alcabala" ||
                        activityText == "Consultar Trámites Impuesto de Alcabala" ||
                        activityText == "Trámite Impuesto Vehicular" ||
                        activityText == "Nuevo Trámite Impuesto Vehicular" ||
                        activityText == "Consultar Trámites Impuesto Vehicular" ||
                        activityText == "Estado de Cuenta" ||
                        activityText == "Inicio" ||
                        activityText == "Seleccionar Trámite"
                    )
                {
                    GoRootDialog = false;

                    switch (activityText)
                    {
                        case "Trámite Licencia de Funcionamiento":
                            await buttonsLicenciaFuncionamiento(turnContext, cancellationToken);
                            //if (Globales.OnSesion)
                            //    await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar {Globales.no_nombres}?");
                            break;
                        case "Nuevo Trámite Licencia de Funcionamiento":
                            if (Globales.OnSesion)
                                await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar {Globales.no_nombres}?");
                            break;
                        case "Consultar Licencias de Funcionamiento":
                            if (Globales.OnSesion)
                                await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar {Globales.no_nombres}?");
                            break;
                        case "Trámite Impuesto de Alcabala":
                            await buttonsImpuestoAlcabala(turnContext, cancellationToken);
                            //if (Globales.OnSesion)
                            //    await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar {Globales.no_nombres}?");
                            break;
                        case "Nuevo Trámite Impuesto de Alcabala":
                            if (Globales.OnSesion)
                                await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar {Globales.no_nombres}?");
                            break;
                        case "Consultar Trámites Impuesto de Alcabala":
                            if (Globales.OnSesion)
                                await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar {Globales.no_nombres}?");
                            break;
                        case "Trámite Impuesto Vehicular":
                            await buttonsImpuestoVehicular(turnContext, cancellationToken);
                            //if (Globales.OnSesion)
                            //    await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar {Globales.no_nombres}?");
                            break;
                        case "Nuevo Trámite Impuesto Vehicular":
                            if (Globales.OnSesion)
                                await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar {Globales.no_nombres}?");
                            break;
                        case "Consultar Trámites Impuesto Vehicular":
                            if (Globales.OnSesion)
                                await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar {Globales.no_nombres}?");
                            break;
                        case "Inicio":
                            await buttonsInicio(turnContext, cancellationToken, $"En que te puedo ayudar {Globales.no_nombres}?");
                            break;
                        case "Seleccionar Trámite":
                            await buttonsSeleccionarTramite(turnContext, cancellationToken);
                            break;
                    }
                }
            }
            
            
            if (GoRootDialog)
            {
                //await SaveUser(turnContext);

                await _dialog.RunAsync(
                turnContext,
                _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                cancellationToken
                );
            }
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
        private static async Task buttonsInicio(ITurnContext turnContext, CancellationToken cancellationToken, string message)
        {
            var reply = MessageFactory.Text($"{message}");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Trámites", Type = ActionTypes.ImBack, Value = "Seleccionar Trámite" },
                    new CardAction() { Title = "Iniciar Sesion", Type = ActionTypes.ImBack, Value = "Iniciar Sesion" },
                    new CardAction() { Title = "Crear una cuenta", Type = ActionTypes.ImBack, Value = "Crear un cuenta" },
                    new CardAction() { Title = "Contactos", Type = ActionTypes.ImBack, Value = "Contactos" },
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
        private static async Task buttonsSeleccionarTramite(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"Selecciona un tipo de trámite {Globales.no_nombres}.");

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
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
        private static async Task buttonsLicenciaFuncionamiento(ITurnContext turnContext, CancellationToken cancellationToken)
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
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
        private static async Task buttonsImpuestoAlcabala(ITurnContext turnContext, CancellationToken cancellationToken)
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
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
        private static async Task buttonsImpuestoVehicular(ITurnContext turnContext, CancellationToken cancellationToken)
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
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
