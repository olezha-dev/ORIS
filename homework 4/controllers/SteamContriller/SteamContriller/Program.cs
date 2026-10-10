using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using CustomHttpServer.Framework;
using CustomHttpServer.Framework.Attributes;

namespace CustomHttpServer.Controllers
{
    [HttpController("steam")]
    public class SteamController : ControllerBase
    {
        //Get http://localhost:2323/steam/send?to=test@test.ru&hero=
        SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587);
        [Get("send")]
        public void Send([FromQuery] string to, string hero)
        {
            MailAddress from = new MailAddress("Oleg.Cher.Hello@yandex.ru", "AFHDK");
            MailAddress toAdress = new MailAddress(to);
            MailMessage message = new MailMessage(from, toAdress);
            message.Subject = "text";
            message.Body = "<h2> письмо</h2>";
            message.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 465);
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("Oleg.Cher.Hello@yandex.ru", "a1133v12354");
            smtp.Send(message);
            message.Dispose();
            smtp.Dispose();
        }
    }
}