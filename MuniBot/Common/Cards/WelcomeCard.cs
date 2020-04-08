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
    public class WelcomeCard
    {
        public static async Task ToShow(DialogContext stepContext, CancellationToken cancellationToken)
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

            var optionAttachments = new List<Attachment>()
            {
                cardImpuestoAlcabala.ToAttachment()
            };
            var reply = MessageFactory.Attachment(optionAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }

    }
}
