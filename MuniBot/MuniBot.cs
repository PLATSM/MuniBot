// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using AdaptiveCards;
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
using AdaptiveCards.Templating;
using MuniBot.Client.Entities.DataCard;
using MuniBot.Client.Entities;
using MuniBot.Common.Models;

namespace MuniBot
{
    public class MuniBot<T> : ActivityHandler where T : Dialog
    {
        private readonly StateBotAccessors _accessors;
        // private readonly BotState _userState;
        // private readonly BotState _conversationState;
        private readonly Dialog _dialog;
        // private readonly IDataBaseService _databaseService;
/*
        public MuniBot(UserState userState, ConversationState conversationState, T dialog, IDataBaseService databaseService)
        {

            _userState = userState;
            _conversationState = conversationState;
            _dialog = dialog;
            _databaseService = databaseService;
        }
*/
        public MuniBot(StateBotAccessors accessors, T dialog)
        {
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
            _dialog = dialog;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    Globales.OnSesion = false;
                    Globales.id_contribuyente = 0;
                    Globales.no_token = string.Empty;
                    Globales.no_contribuyente = string.Empty;

                    AdaptiveCardList adaptiveCard = new AdaptiveCardList();
                    var nameCard = adaptiveCard.CreateAttachment(0, "");
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(nameCard), cancellationToken);
                    await Task.Delay(500);

                    await buttonsInicio(turnContext, cancellationToken, $"En que te puedo ayudar?\n\n puedes utilizar los botones de la parte inferior.");
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {

            var activityText = turnContext.Activity.Text;
            var activityId = turnContext.Activity.Id;

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Get the state properties from the turn context.
                UserProfile userProfile = await _accessors.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
                ConversationData conversationData = await _accessors.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());

                if (string.IsNullOrEmpty(userProfile.Name))
                {
                    // First time around this is set to false, so we will prompt user for name.
                    if (conversationData.PromptedUserForName)
                    {
                        // Set the name to what the user provided.
                        userProfile.Name = turnContext.Activity.Text?.Trim();

                        // Acknowledge that we got their name.
                        await turnContext.SendActivityAsync($"Thanks {userProfile.Name}.");

                        // Reset the flag to allow the bot to go though the cycle again.
                        conversationData.PromptedUserForName = false;
                    }
                    else
                    {
                        // Prompt the user for their name.
                        await turnContext.SendActivityAsync($"What is your name?");

                        // Set the flag to true, so we don't prompt in the next turn.
                        conversationData.PromptedUserForName = true;
                    }

                    // Save user state and save changes.
                    await _accessors.UserProfileAccessor.SetAsync(turnContext, userProfile);
                    await _accessors.UserState.SaveChangesAsync(turnContext);
                }
                else
                {
                    // Add message details to the conversation data.
                    conversationData.Timestamp = turnContext.Activity.Timestamp.ToString();
                    conversationData.ChannelId = turnContext.Activity.ChannelId.ToString();

                    // Display state data.
                    await turnContext.SendActivityAsync($"{userProfile.Name} sent: {turnContext.Activity.Text}");
                    await turnContext.SendActivityAsync($"Message received at: {conversationData.Timestamp}");
                    await turnContext.SendActivityAsync($"Message received from: {conversationData.ChannelId}");
                }

                // Update conversation state and save changes.
                await _accessors.ConversationDataAccessor.SetAsync(turnContext, conversationData);
                await _accessors.ConversationState.SaveChangesAsync(turnContext);
            }

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
                            await buttonsInicio(turnContext, cancellationToken, $"En que te puedo ayudar?");
                        }
                    }
                }

                if (turnContext.Activity.Value != null)
                {
                    String value = turnContext.Activity.Value.ToString();
                    JObject results = JObject.Parse(value);

                    // Get type from input field
                    String nameCard = results.GetValue("id").ToString();

                    // Crear Cuenta Persona Natural
                    if (nameCard == "PersonaNaturalNewCard")
                    {
                        if
                        (
                            string.IsNullOrEmpty(results.GetValue("txtNombres").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtApellidoPaterno").ToString()) ||
                            // string.IsNullOrEmpty(results.GetValue("txtApellidoMaterno").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtFechaNacimiento").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("cboSexo").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("cboTipoDocumento").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtDocumentoIdentidad").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtCorreoElectronico").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtTelefono").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtDireccion").ToString()) ||
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
                                contribuyenteDTO.fe_nacimiento = results.GetValue("txtFechaNacimiento").ToString();
                                contribuyenteDTO.co_sexo = results.GetValue("cboSexo").ToString();
                                contribuyenteDTO.co_documento_identidad = results.GetValue("cboTipoDocumento").ToString();
                                contribuyenteDTO.nu_documento_identidad = results.GetValue("txtDocumentoIdentidad").ToString();
                                contribuyenteDTO.no_correo_electronico = results.GetValue("txtCorreoElectronico").ToString();
                                contribuyenteDTO.nu_telefono = results.GetValue("txtTelefono").ToString();
                                contribuyenteDTO.no_direccion = results.GetValue("txtDireccion").ToString();
                                contribuyenteDTO.no_contrasena = results.GetValue("txtContrasena").ToString();
                                contribuyenteDTO.no_contrasena_sha256 = Funciones.GetSHA256(results.GetValue("txtContrasena").ToString());
                                contribuyenteDTO.id_usuario_creacion = 2; // (2=Bot)

                                // Verificar Informacion en RENIEC
                                //var numero = VerificarDNI_Local(contribuyenteDTO);
                                var numero = VerificarDNI_RENIEC(contribuyenteDTO);

                                switch (numero)
                                {
                                    case 0:
                                        ContribuyenteClient contribuyenteClient = new ContribuyenteClient();
                                        var result = contribuyenteClient.InsertAsync(contribuyenteDTO);

                                        if (result.error_number == 0)
                                            await buttonsInicio(turnContext, cancellationToken, "Se ha creado su cuenta exitosamente.");
                                        else
                                            await buttonsInicio(turnContext, cancellationToken, $"{result.error_message}");
                                        break;

                                    case -1:
                                        await buttonsInicio(turnContext, cancellationToken, "Los datos ingresados no coinciden con la información de RENIEC.");
                                        break;

                                    case 999:
                                        await buttonsInicio(turnContext, cancellationToken, "No se ha encontrado información para el número de DNI.");
                                        break;

                                    case 1000:
                                        await buttonsInicio(turnContext, cancellationToken, "Uno o más datos de la petición no son válidos.");
                                        break;

                                    case 1001:
                                        await buttonsInicio(turnContext, cancellationToken, "El DNI, RUC y contraseña no corresponden a un usuario válido.");
                                        break;

                                    case 1002:
                                        await buttonsInicio(turnContext, cancellationToken, "La contraseña para el DNI y RUC está caducada.");
                                        break;

                                    case 1003:
                                        await buttonsInicio(turnContext, cancellationToken, "Se ha alcanzado el límite de consultas permitidas por día.");
                                        break;

                                    case 1999:
                                        await buttonsInicio(turnContext, cancellationToken, "Error desconocido / inesperado.");
                                        break;

                                    default:
                                        await buttonsInicio(turnContext, cancellationToken, "Sucedió un problema, intente otra vez.");
                                        break;
                                }
                            }
                        }
                    }

                    // Crear Cuenta Persona Juridica
                    if (nameCard == "PersonaJuridicaNewCard")
                    {
                        if
                        (
                            string.IsNullOrEmpty(results.GetValue("txtRazonSocial").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtRepresentanteLegal").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("cboTipoDocumento").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtDocumentoIdentidad").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtCorreoElectronico").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtTelefono").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtDireccion").ToString()) ||
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
                                contribuyenteDTO.no_razon_social = results.GetValue("txtRazonSocial").ToString();
                                contribuyenteDTO.no_representante_legal = results.GetValue("txtRepresentanteLegal").ToString();
                                contribuyenteDTO.co_documento_identidad = results.GetValue("cboTipoDocumento").ToString();
                                contribuyenteDTO.nu_documento_identidad = results.GetValue("txtDocumentoIdentidad").ToString();
                                contribuyenteDTO.no_correo_electronico = results.GetValue("txtCorreoElectronico").ToString();
                                contribuyenteDTO.nu_telefono = results.GetValue("txtTelefono").ToString();
                                contribuyenteDTO.no_direccion = results.GetValue("txtDireccion").ToString();
                                contribuyenteDTO.no_contrasena = results.GetValue("txtContrasena").ToString();
                                contribuyenteDTO.no_contrasena_sha256 = Funciones.GetSHA256(results.GetValue("txtContrasena").ToString());
                                contribuyenteDTO.id_usuario_creacion = 2; // (2=Bot)

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
                    if (nameCard == "LoginCard")
                    {
                        if (Globales.OnSesion)
                        {
                            await buttonsInicio(turnContext, cancellationToken, $"Su sesión ya fue iniciada como {Globales.no_contribuyente}");
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
                                var no_contrasena = Funciones.GetSHA256(results.GetValue("txtContrasena").ToString());

                                ContribuyenteClient contribuyenteClient = new ContribuyenteClient();
                                var result = contribuyenteClient.GetLoginAsync(co_documento_identidad, nu_documento_identidad, no_contrasena);

                                if (result.error_number == 0)
                                {
                                    Globales.OnSesion = true;
                                    Globales.id_contribuyente = result.Data.id_contribuyente;
                                    if (result.Data.co_tipo_persona == "0002") // 0002=Persona Juridica
                                    {
                                        Globales.no_contribuyente = result.Data.no_razon_social;
                                    }
                                    else
                                    {
                                        Globales.no_contribuyente = result.Data.no_nombres + ' ' + result.Data.no_apellido_paterno + ' ' + result.Data.no_apellido_materno;
                                    }

                                    await buttonsInicio(turnContext, cancellationToken, $"Hola {Globales.no_contribuyente}, en que te puedo ayudar?");

                                }
                                else
                                {
                                    Globales.OnSesion = false;
                                    Globales.id_contribuyente = 0;
                                    Globales.no_token = string.Empty;
                                    Globales.no_contribuyente = string.Empty;
                                    await buttonsInicio(turnContext, cancellationToken, $"{result.error_message}");
                                }
                            }

                        }
                    }

                    // Crear Solicitud Licencia
                    if (nameCard == "SolicitudLicenciaNewCard")
                    {
                        if
                        (
                            string.IsNullOrEmpty(results.GetValue("cboTipoLicencia").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtNombreComercial").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("cboClase").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("cboSubClase").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("cboCategoria").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txtDireccionEstablecimiento").ToString()) ||
                            string.IsNullOrEmpty(results.GetValue("txArea").ToString())
                        )
                        {
                            await buttonsInicio(turnContext, cancellationToken, "Todos los campos son obligatorios.");
                        }
                        else
                        {
                            SolicitudLicenciaDTO solicitudLicenciaDTO = new SolicitudLicenciaDTO();
                            solicitudLicenciaDTO.id_contribuyente = Globales.id_contribuyente;
                            solicitudLicenciaDTO.id_empresa = Globales.id_empresa;
                            solicitudLicenciaDTO.co_tipo_licencia = results.GetValue("cboTipoLicencia").ToString();
                            solicitudLicenciaDTO.no_comercial = results.GetValue("txtNombreComercial").ToString();
                            solicitudLicenciaDTO.co_clase = results.GetValue("cboClase").ToString();
                            solicitudLicenciaDTO.co_clase = results.GetValue("cboClase").ToString();
                            solicitudLicenciaDTO.co_subclase = results.GetValue("cboSubClase").ToString();
                            solicitudLicenciaDTO.co_categoria = results.GetValue("cboCategoria").ToString();
                            solicitudLicenciaDTO.no_direccion_solicitud = results.GetValue("txtDireccionEstablecimiento").ToString();
                            solicitudLicenciaDTO.nu_area = results.GetValue("txArea").ToString();
                            solicitudLicenciaDTO.id_usuario_creacion = 2;// (2=Bot)

                            SolicitudLicenciaClient solicitudLicenciaClient = new SolicitudLicenciaClient();
                            var result = solicitudLicenciaClient.InsertAsync(solicitudLicenciaDTO);

                            if (result.error_number == 0)
                                await buttonsInicio(turnContext, cancellationToken, $"Se ha creado su solicitud exitosamente.\n\n su número de solicitud es: **{DateTime.Now.Date.Year.ToString("0000-")}{result.id_identity.ToString("000000")}**\n\n se encuentra en estado **Pendiente de Aprobación**.");
                            else
                                await buttonsInicio(turnContext, cancellationToken, $"{result.error_message}");
                        }
                    }
                    // Consultar Solicitud Licencia por Numero de Licencia
                    if (nameCard == "ConsultarIdSolicitudLicencia")
                    {
                        if
                        (
                            string.IsNullOrEmpty(results.GetValue("txtNumeroSolicitud").ToString())
                        )
                        {
                            await buttonsInicio(turnContext, cancellationToken, "Debe ingresar un número de la lista.");
                        }
                        else
                        {
                            SolicitudLicenciaDTO solicitudLicenciaDTO = new SolicitudLicenciaDTO();
                            solicitudLicenciaDTO.id_solicitud_licencia = 0;
                            solicitudLicenciaDTO.id_contribuyente = Globales.id_contribuyente;
                            solicitudLicenciaDTO.nu_solicitud_licencia = results.GetValue("txtNumeroSolicitud").ToString();

                            SolicitudLicenciaClient solicitudLicenciaClient = new SolicitudLicenciaClient();
                            var result = solicitudLicenciaClient.GetAsync(solicitudLicenciaDTO);

                            if (result.error_number == 0)
                            {
                                var dataJson = JsonConvert.SerializeObject(result.Data);
                                AdaptiveCardList adaptiveCardLicencia = new AdaptiveCardList();
                                var LicenciaCard = adaptiveCardLicencia.CreateAttachment(6, dataJson);
                                await turnContext.SendActivityAsync(MessageFactory.Attachment(LicenciaCard), cancellationToken);

                                await Task.Delay(500);
                                await buttonsInicio(turnContext, cancellationToken, "");

                            }
                            else
                                await buttonsInicio(turnContext, cancellationToken, $"{result.error_message}");
                        }
                    }

                }
            }

            await base.OnTurnAsync(turnContext, cancellationToken);
            // await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
            // await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var GoRootDialog = true;
            var activityText = turnContext.Activity.Text;
            var activityId = turnContext.Activity.Id;


            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (turnContext.Activity.Value == null)
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
                            activityText == "Seleccionar Trámite" ||
                            activityText == "Iniciar Sesion" ||
                            activityText == "Crear una cuenta" ||
                            activityText == "Cuenta Persona Natural" ||
                            activityText == "Cuenta Persona Juridica" ||
                            activityText == "Cerrar Sesion" ||
                            activityText == "Foto" ||
                            activityText == "ADD Licencia"
                        )
                    {
                        GoRootDialog = false;

                        switch (activityText)
                        {
                            case "Trámite Licencia de Funcionamiento":
                                await buttonsLicenciaFuncionamiento(turnContext, cancellationToken);
                                break;
                            case "Nuevo Trámite Licencia de Funcionamiento":
                                if (Globales.OnSesion)
                                {
                                    // Obtiene la información del contribuyente
                                    ContribuyenteClient contribuyenteClient = new ContribuyenteClient();
                                    var result = contribuyenteClient.GetAsync(Globales.id_contribuyente, Globales.no_token);

                                    if (result.error_number == 0)
                                    {
                                        SolicitudLicenciaDataCard licenciaFuncionamientoData = new SolicitudLicenciaDataCard()
                                        {
                                            no_contribuyente = Globales.no_contribuyente,
                                            co_tipo_persona = result.Data.co_tipo_persona,
                                            co_documento_identidad = result.Data.co_documento_identidad,
                                            nu_documento_identidad = result.Data.nu_documento_identidad,
                                            no_nombres = result.Data.no_nombres,
                                            no_apellido_paterno = result.Data.no_apellido_paterno,
                                            no_apellido_materno = result.Data.no_apellido_materno,
                                            no_razon_social = result.Data.no_razon_social,
                                            nu_telefono = result.Data.nu_telefono,
                                            no_direccion = result.Data.no_direccion,
                                            no_correo_electronico = result.Data.no_correo_electronico
                                        };
                                        var dataJson = JsonConvert.SerializeObject(licenciaFuncionamientoData);
                                        AdaptiveCardList adaptiveCardLicencia = new AdaptiveCardList();
                                        var LicenciaCard = adaptiveCardLicencia.CreateAttachment(3, dataJson);
                                        await turnContext.SendActivityAsync(MessageFactory.Attachment(LicenciaCard), cancellationToken);

                                        await Task.Delay(500);
                                        await buttonsInicio(turnContext, cancellationToken, "");
                                    }
                                    else
                                    {
                                        await Task.Delay(500);
                                        await buttonsInicio(turnContext, cancellationToken, "Sucedió un error inesperado, elija otra opción");
                                    }
                                }
                                break;
                            case "Consultar Licencias de Funcionamiento":
                                if (Globales.OnSesion)
                                {
                                    // Obtiene la información del contribuyente
                                    ContribuyenteClient contribuyenteClient = new ContribuyenteClient();
                                    var result = contribuyenteClient.GetJsonAsync(Globales.id_contribuyente, Globales.no_token);

                                    if (result.error_number == 0)
                                    {
                                        AdaptiveCardList adaptiveCardLicencia = new AdaptiveCardList();
                                        var LicenciaCard = adaptiveCardLicencia.CreateAttachment(5, result.Data.no_data_json);
                                        await turnContext.SendActivityAsync(MessageFactory.Attachment(LicenciaCard), cancellationToken);

                                        await Task.Delay(500);
                                        await buttonsInicio(turnContext, cancellationToken, "");
                                    }
                                    else
                                    {
                                        await Task.Delay(500);
                                        await buttonsInicio(turnContext, cancellationToken, result.error_message);
                                    }
                                }
                                break;
                            case "Trámite Impuesto de Alcabala":
                                await buttonsImpuestoAlcabala(turnContext, cancellationToken);
                                break;
                            case "Nuevo Trámite Impuesto de Alcabala":
                                if (Globales.OnSesion)
                                    await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar?");
                                break;
                            case "Consultar Trámites Impuesto de Alcabala":
                                if (Globales.OnSesion)
                                    await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar?");
                                break;
                            case "Trámite Impuesto Vehicular":
                                await buttonsImpuestoVehicular(turnContext, cancellationToken);
                                break;
                            case "Nuevo Trámite Impuesto Vehicular":
                                if (Globales.OnSesion)
                                    await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar?");
                                break;
                            case "Consultar Trámites Impuesto Vehicular":
                                if (Globales.OnSesion)
                                    await buttonsInicio(turnContext, cancellationToken, $"Estamos en proceso de implementar este trámite en forma virtual, pronto estará funcionando, en que más te puedo ayudar?");
                                break;
                            case "Inicio":
                                await buttonsInicio(turnContext, cancellationToken, $"En que te puedo ayudar?");
                                break;
                            case "Seleccionar Trámite":
                                await buttonsSeleccionarTramite(turnContext, cancellationToken);
                                break;
                            case "Iniciar Sesion":
                                if (Globales.OnSesion)
                                {
                                    await buttonsInicio(turnContext, cancellationToken, $"Su sesión ya fue iniciada como {Globales.no_contribuyente}");
                                }
                                else
                                {
                                    AdaptiveCardList adaptiveCardLogin = new AdaptiveCardList();
                                    var loginCard = adaptiveCardLogin.CreateAttachment(2, "");
                                    await turnContext.SendActivityAsync(MessageFactory.Attachment(loginCard), cancellationToken);
                                    await Task.Delay(500);
                                }
                                await buttonsInicio(turnContext, cancellationToken, "");
                                break;
                            case "Crear una cuenta":
                                await buttonsTipoPersona(turnContext, cancellationToken);
                                break;

                            case "Cuenta Persona Natural":
                                AdaptiveCardList adaptiveCardNatural = new AdaptiveCardList();
                                var PersonaNaturalCard = adaptiveCardNatural.CreateAttachment(1, "");
                                await turnContext.SendActivityAsync(MessageFactory.Attachment(PersonaNaturalCard), cancellationToken);
                                await Task.Delay(500);
                                await buttonsInicio(turnContext, cancellationToken, "");
                                break;
                            case "Cuenta Persona Juridica":
                                AdaptiveCardList adaptiveCardJuridica = new AdaptiveCardList();
                                var PersonaJuridicaCard = adaptiveCardJuridica.CreateAttachment(4, "");
                                await turnContext.SendActivityAsync(MessageFactory.Attachment(PersonaJuridicaCard), cancellationToken);
                                await Task.Delay(500);
                                await buttonsInicio(turnContext, cancellationToken, "");
                                break;
                            case "Cerrar Sesion":
                                break;
                            case "Foto":
                                if (Globales.OnSesion)
                                {
                                    // Obtiene la información del contribuyente
                                    ContribuyenteClient contribuyenteClient = new ContribuyenteClient();
                                    var result = contribuyenteClient.GetAsync(Globales.id_contribuyente, Globales.no_token);

                                    if (result.error_number == 0)
                                    {
                                        var DataJson = JsonConvert.SerializeObject(result.Data);

                                        AdaptiveCardList adaptiveCardLicencia = new AdaptiveCardList();
                                        var ContribuyenteCard = adaptiveCardLicencia.CreateAttachment(8, DataJson);
                                        await turnContext.SendActivityAsync(MessageFactory.Attachment(ContribuyenteCard), cancellationToken);

                                        await Task.Delay(500);
                                        await buttonsInicio(turnContext, cancellationToken, "");
                                    }
                                    else
                                    {
                                        await Task.Delay(500);
                                        await buttonsInicio(turnContext, cancellationToken, result.error_message);
                                    }
                                }
                                break;
                            case "ADD Licencia":
                                if (Globales.OnSesion)
                                {
                                    ClaseEstablecimientoDTO claseEstablecimientoDTO = new ClaseEstablecimientoDTO()
                                    {
                                        fl_inactivo = "0"
                                    };

                                    // Obtiene la información del contribuyente
                                    ClaseEstablecimientoClient claseEstablecimientoClient = new ClaseEstablecimientoClient();
                                    var result = claseEstablecimientoClient.GetListJsonAsync(claseEstablecimientoDTO);

                                    if (result.error_number == 0)
                                    {
                                        AdaptiveCardList adaptiveCardLicencia = new AdaptiveCardList();
                                        var LicenciaCard = adaptiveCardLicencia.CreateAttachment(7, result.Data.no_data_json);
                                        await turnContext.SendActivityAsync(MessageFactory.Attachment(LicenciaCard), cancellationToken);

                                        await Task.Delay(500);
                                        await buttonsInicio(turnContext, cancellationToken, "");
                                    }
                                    else
                                    {
                                        await Task.Delay(500);
                                        await buttonsInicio(turnContext, cancellationToken, result.error_message);
                                    }
                                }
                                break;
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
                    if
                    (
                        nameCard == "LoginCard" ||
                        nameCard == "ContribuyenteNewCard" ||
                        nameCard == "SolicitudLicenciaNewCard" ||
                        nameCard == "PersonaNaturalNewCard" ||
                        nameCard == "PersonaJuridicaNewCard" ||
                        nameCard == "ConsultarIdSolicitudLicencia"
                    )
                    {
                        GoRootDialog = false;
                    }
                }
            }


            if (GoRootDialog)
            {
                // await SaveUser(turnContext);
                // await _dialog.RunAsync(turnContext,_conversationState.CreateProperty<DialogState>(nameof(DialogState)),cancellationToken);

                await _dialog.RunAsync(turnContext, _accessors.ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            }

        }

/*
        private async Task SaveUser(ITurnContext<IMessageActivity> turnContext)
        {
            var userModel = new UserModel();
            userModel.id = turnContext.Activity.From.Id;
            userModel.userNameChannel = turnContext.Activity.From.Name;
            userModel.channel = turnContext.Activity.ChannelId;
            userModel.registerDate = DateTime.Now.Date;

            var user = await _databaseService.User.FirstOrDefaultAsync(x => x.id == turnContext.Activity.From.Id);

            if (user == null)
            {
                await _databaseService.User.AddAsync(userModel);
                await _databaseService.SaveAsync();
            }
         }
 */
        private static async Task buttonsInicio(ITurnContext turnContext, CancellationToken cancellationToken, string message)
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
                    new CardAction() { Title = "Foto", Type = ActionTypes.ImBack, Value = "Foto" },
                    new CardAction() { Title = "ADD Licencia", Type = ActionTypes.ImBack, Value = "ADD Licencia" },
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
        private static async Task buttonsSeleccionarTramite(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text($"Selecciona un tipo de trámite.");

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
        private static async Task buttonsTipoPersona(ITurnContext turnContext, CancellationToken cancellationToken)
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
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private int VerificarDNI_Local(ContribuyenteDTO contribuyenteDTO)
        {
            ReniecDTO reniec = new ReniecDTO();

            ReniecClient reniecClient = new ReniecClient();
            var result = reniecClient.GetLocalAsync(contribuyenteDTO.nu_documento_identidad, contribuyenteDTO.no_token);

            if (result.error_number == 0)
            {
                reniec = result.Data;

                if 
                (
                    contribuyenteDTO.no_nombres.ToUpper() != reniec.prenombres.ToUpper() ||
                    contribuyenteDTO.no_apellido_paterno.ToUpper() != reniec.apPrimer.ToUpper() ||
                    contribuyenteDTO.no_apellido_materno.ToUpper() != reniec.apSegundo.ToUpper()
                )
                {
                    return -1;
                }
            }
            else
            {
                return result.error_number;
            }
            return 0;

        }
        private int VerificarDNI_RENIEC(ContribuyenteDTO contribuyenteDTO)
        {
            //ReniecDatosPersonas reniec = new ReniecDatosPersonas();

            ReniecClient reniecClient = new ReniecClient();
            var result = reniecClient.GetReniecAsync(contribuyenteDTO.nu_documento_identidad);

            int error_number = 0;

            switch (result.coResultado)
            {
                case "0000":
                    
                    if (result.datosPersona.prenombres != null)
                    {
                        if(contribuyenteDTO.no_nombres.ToUpper() != result.datosPersona.prenombres.ToUpper())
                            error_number = -1;
                    }

                    if (result.datosPersona.apPrimer != null)
                    {
                        if (contribuyenteDTO.no_apellido_paterno.ToUpper() != result.datosPersona.apPrimer.ToUpper())
                            error_number = -1;
                    }

                    if (result.datosPersona.apSegundo != null)
                    {
                        if (contribuyenteDTO.no_apellido_materno.ToUpper() != result.datosPersona.apSegundo.ToUpper())
                            error_number = -1;
                    }

                    if(error_number == 0)
                    {
                        if (result.datosPersona.foto != null)
                            contribuyenteDTO.foto = result.datosPersona.foto;
                    }
                    break;

                case "0001":
                    error_number = 1;
                    break;

                case "0999":
                    error_number = 999;
                    break;

                case "1000":
                    error_number = 1000;
                    break;

                case "1001":
                    error_number = 1001;
                    break;

                case "1002":
                    error_number = 1002;
                    break;

                case "1003":
                    error_number = 1003;
                    break;

                case "1999":
                    error_number = 1999;
                    break;
            };

            return error_number;

        }
    }

}

