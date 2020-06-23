using AdaptiveCards.Templating;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MuniBot.Common
{
    public class AdaptiveCardList
    {
        //var cardId = Guid.NewGuid().ToString();
        // public static string idCard { get; set; } = string.Empty;

        private readonly string[] _path =
        {
            Path.Combine(".", "Common/json", "WelcomeCard.json"),
            Path.Combine(".", "Common/json", "PersonaNaturalNewCard.json"),
            Path.Combine(".", "Common/json", "LoginCard.json"),
            Path.Combine(".", "Common/json", "SolicitudLicenciaNewCard.json"),
            Path.Combine(".", "Common/json", "PersonaJuridicaNewCard.json"),
            Path.Combine(".", "Common/json", "SolicitudLicenciaList.json"),
            Path.Combine(".", "Common/json", "SolicitudLicenciaGetCard.json"),
            Path.Combine(".", "Common/json", "SolicitudLicenciaAddCard.json"),
            Path.Combine(".", "Common/json", "ContribuyenteGetCard.json"),
        };

        //public Attachment Create(string cardResourcePath)
        //{
        //    using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
        //    {
        //        using (var reader = new StreamReader(stream))
        //        {
        //            var adaptiveCard = reader.ReadToEnd();
        //            return new Attachment()
        //            {
        //                ContentType = "application/vnd.microsoft.card.adaptive",
        //                Content = JsonConvert.DeserializeObject(adaptiveCard),
        //            };
        //        }
        //    }
        //}

        //public Attachment CreateAttachment(string filePath)
        //{
        //    var adaptiveCardJson = File.ReadAllText(filePath);
        //    var adaptiveCardAttachment = new Attachment()
        //    {
        //        ContentType = "application/vnd.microsoft.card.adaptive",
        //        Content = JsonConvert.DeserializeObject(adaptiveCardJson),
        //    };
        //    return adaptiveCardAttachment;
        //}

        //public Attachment CreateAttachment(int Index)
        //{
        //    var adaptiveCardJson = File.ReadAllText(_path[Index]);
        //    var adaptiveCardAttachment = new Attachment()
        //    {
        //        ContentType = "application/vnd.microsoft.card.adaptive",
        //        Content = JsonConvert.DeserializeObject(adaptiveCardJson),
        //    };

        //    return adaptiveCardAttachment;
        //}

        public Attachment CreateAttachment(int Index,string dataJson)
        {


            if (string.IsNullOrEmpty(dataJson))
            {
                //string adaptiveCardJson;

                //switch (Index)
                //{
                //    case 0:
                //        adaptiveCardJson = File.ReadAllText(https://munibotstorage.blob.core.windows.net/cards/WelcomeCard.json);
                //        break;
                //    default:
                //        adaptiveCardJson = "";
                //        break;
                //}

                var adaptiveCardJson = File.ReadAllText(_path[Index]);
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(adaptiveCardJson),
                };
                return adaptiveCardAttachment;

            }
            else
            {
                var CardJson = File.ReadAllText(_path[Index]);

                var transformer = new AdaptiveTransformer();
                var cardJson = transformer.Transform(CardJson, dataJson);

                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(cardJson),
                };
                return adaptiveCardAttachment;
            }
            
        }


    }
}
