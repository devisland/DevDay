using System.Net;
using System.Net.Mail;

namespace DevDay.Helpers
{
    public static class MailHelper
    {
        public static bool Send(string userEmail, string password)
        {
            int port;
            int.TryParse(System.Configuration.ConfigurationManager.AppSettings["MailSmtpPort"], out port);

            var client = new SmtpClient(System.Configuration.ConfigurationManager.AppSettings["MailSmtpAddress"])
                             {
                                 Port = port,
                                 Credentials =
                                     new NetworkCredential(
                                     System.Configuration.ConfigurationManager.AppSettings["MailUsername"],
                                     System.Configuration.ConfigurationManager.AppSettings["MailPassword"])
                             };

            var from = new MailAddress(System.Configuration.ConfigurationManager.AppSettings["MailAddress"]);

            var to = new MailAddress(userEmail);
            var message = new MailMessage(from, to)
                              {
                                  Body = password,
                                  BodyEncoding = System.Text.Encoding.UTF8,
                                  Subject = "Senha de acesso",
                                  SubjectEncoding = System.Text.Encoding.UTF8
                              };

            try
            {
                client.Send(message);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}