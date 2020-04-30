using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MuniBot.Common
{
    public class AdaptiveCard
    {
        //var cardId = Guid.NewGuid().ToString();
        // public static string idCard { get; set; } = string.Empty;

        private readonly string[] _path =
        {
            Path.Combine(".", "Common/json", "welcomeCard.json"),
            Path.Combine(".", "Common/json", "createUserCard.json"),
            Path.Combine(".", "Common/json", "loginCard.json"),
        };

        public Attachment Create(string cardResourcePath)
        {
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

        public Attachment CreateAttachment(int Index)
        {
            var adaptiveCardJson = File.ReadAllText(_path[Index]);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return adaptiveCardAttachment;
        }


    }
}
