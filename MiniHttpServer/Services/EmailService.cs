using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MiniHttpServer.Services
{
    internal class EmailService
    {
        private readonly List<SmtpSettings> _smtpList;

        public EmailService()
        {
            _smtpList = new List<SmtpSettings>
            {
                new SmtpSettings
                {
                    Name = "Gmail",
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Username = "your-gmail@gmail.com",
                    Password = "YOUR_GMAIL_APP_PASSWORD"
                },
                new SmtpSettings
                {
                    Name = "Yandex",
                    Host = "smtp.yandex.ru",
                    Port = 587,
                    EnableSsl = true,
                    Username = "your-account@yandex.ru",
                    Password = "YOUR_YANDEX_APP_PASSWORD"
                },
                new SmtpSettings
                {
                    Name = "Mail.ru",
                    Host = "smtp.mail.ru",
                    Port = 587,
                    EnableSsl = true,
                    Username = "your-account@inbox.ru",
                    Password = "YOUR_MAILRU_APP_PASSWORD"
                }
            };
        }

        public void SendEmail(string _to, string _title)
        {
            foreach (var smtpSettings in _smtpList) {
                try
                {
                    MailAddress from = new MailAddress(smtpSettings.Username, "Hotel Search");
                    // кому отправляем
                    MailAddress to = new MailAddress(_to);
                    // создаем объект сообщения
                    MailMessage m = new MailMessage(from, to);
                    // тема письма
                    m.Subject = _title;
                    // текст письма
                    m.Body = $@"
                            <html>
                                <body style='font-family: Arial, sans-serif; color: #333;'>
                                    <h2 style='color:#2e6c80;'>Здравствуйте!</h2>
                                    <p>Вы успешно подписались на рассылку!</p>
                                </body>
                            </html>";
                    // письмо представляет код html
                    m.IsBodyHtml = true;
                    // адрес smtp-сервера и порт, с которого будем отправлять письмо
                    SmtpClient smtp = new SmtpClient(smtpSettings.Host, smtpSettings.Port);
                    // логин и пароль
                    smtp.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);
                    smtp.EnableSsl = smtpSettings.EnableSsl;
                    smtp.Send(m);

                    Console.WriteLine("Письмо отправлено");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось через {smtpSettings.Name}: {ex.Message}");
                }
            }

            Console.WriteLine("Ни с одного почтового ящика не удалось отправить письмо");
        }
    }
}