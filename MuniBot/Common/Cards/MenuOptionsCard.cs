using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MuniBot.Common.Cards
{
    public class MenuOptionsCard
    {
        public static async Task ToShow(DialogContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(activity: CreateMenuOptions(), cancellationToken);
        }

        private static Activity CreateMenuOptions()
        {
            var cardMenuPrincipal = new HeroCard
            {
                Title = "Menu Principal",
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Licencia de Funcionamiento",Value="Licencia de Funcionamiento",Type=ActionTypes.ImBack},
                    new CardAction(){Title = "Impuesto Alcabala",Value="Impuesto Alcabala",Type=ActionTypes.ImBack},
                    new CardAction(){Title = "Declaración Jurada Vehicular",Value="Declaración Jurada Vehicular",Type=ActionTypes.ImBack},
                    new CardAction(){Title = "Estado de Cuenta",Value="Estado de Cuenta",Type=ActionTypes.ImBack},
                    new CardAction(){Title = "Registrar Ciudadano",Value="Registrar Ciudadano",Type=ActionTypes.ImBack},
                    new CardAction(){Title = "Contactos",Value="Contactos",Type=ActionTypes.ImBack}
                }
            };


            var optionAttachments = new List<Attachment>()
            {
                cardMenuPrincipal.ToAttachment(),
            };
            var reply = Microsoft.Bot.Builder.MessageFactory.Attachment(optionAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }

    }
}
