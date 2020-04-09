using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuniBot.Infraestructure.SendGrid
{
    public interface ISendGridEmailService
    {
        Task<bool> Execute
        (
            string fromEmail,
            string fromName,
            string toEmail,
            string toName,
            string subject,
            string plainTextContent,
            string htmlContent
        );
    }
}
