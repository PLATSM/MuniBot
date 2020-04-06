using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MuniBot.Common.Cards
{
    public class MainOptionsCard
    {
        public static async Task ToShow(DialogContext stepContext,CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(activity: CreateCarousel(), cancellationToken);
        }

        private static Activity CreateCarousel()
        {
            var cardImpuestoAlcabala = new HeroCard
            {
                Title = "Impuesto Alcabala",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://munibotstorage.blob.core.windows.net/images/01_ImpuestoAlcabala.png") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Realizar Trámite",Value="Realizar Trámite",Type=ActionTypes.ImBack},
                    new CardAction(){Title = "Ver Trámite",Value="Realizar Trámite",Type=ActionTypes.ImBack}
                }
            };

            var cardLicenciaFuncionamiento = new HeroCard
            {
                Title = "Licencia de Funcionamiento",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://munibotstorage.blob.core.windows.net/images/02_LicenciaFuncionamiento.png") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Realizar Trámite",Value="Realizar Trámite",Type=ActionTypes.ImBack},
                    new CardAction(){Title = "Ver Trámite",Value="Realizar Trámite",Type=ActionTypes.ImBack}
                }
            };

            var cardImpuestoVehicular = new HeroCard
            {
                Title = "Impuesto Vehicular",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://munibotstorage.blob.core.windows.net/images/03_ImpuestoVehicular.png") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Realizar Trámite",Value="Realizar Trámite",Type=ActionTypes.ImBack},
                    new CardAction(){Title = "Ver Trámite",Value="Realizar Trámite",Type=ActionTypes.ImBack}
                }
            };

            var cardEstadoCuenta = new HeroCard
            {
                Title = "Estado de Cuenta",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://munibotstorage.blob.core.windows.net/images/04_EstadoCuenta.png") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Realizar Trámite",Value="Realizar Trámite",Type=ActionTypes.ImBack},
                    new CardAction(){Title = "Ver Trámite",Value="Realizar Trámite",Type=ActionTypes.ImBack}
                }
            };

            var cardCentroContacto = new HeroCard
            {
                Title = "Centro de Contactos",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://munibotstorage.blob.core.windows.net/images/05_CentroContactos.png") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Contactos",Value="Centro de Contactos",Type=ActionTypes.ImBack},
                    new CardAction(){Title = "Sitio Web",Value="http://www.municallao.gob.pe/index.php/locales-municipales",Type=ActionTypes.OpenUrl}
                }
            };

            var optionAttachments = new List<Attachment>()
            {
                cardImpuestoAlcabala.ToAttachment(),
                cardLicenciaFuncionamiento.ToAttachment(),
                cardImpuestoVehicular.ToAttachment(),
                cardEstadoCuenta.ToAttachment(),
                cardCentroContacto.ToAttachment()
            };
            var reply = MessageFactory.Attachment(optionAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }
    }
}
