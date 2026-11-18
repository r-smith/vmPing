using System;
using System.Net;
using System.Net.Mail;
using vmPing.Configuration;
using vmPing.Properties;

namespace vmPing.Services
{
    internal static class Email
    {
        public static async void SendAlert(string status, string hostname, string alias)
        {
            try
            {
                using (MailMessage message = new MailMessage())
                {
                    string affectedShortName = string.IsNullOrWhiteSpace(alias) ? hostname : alias;
                    string affectedLongName = string.IsNullOrWhiteSpace(alias) ? hostname : $"{alias} ({hostname})";

                    message.Subject = $"Notice: {affectedShortName} {Strings.Email_Verb} {status}";
                    message.Body = $"{affectedLongName} {Strings.Email_Verb} {status}.{Environment.NewLine}" +
                        $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}";
                    message.From = new MailAddress(ApplicationOptions.EmailFromAddress, "vmPing");
                    message.To.Add(ApplicationOptions.EmailRecipient.Replace(";", ","));

                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.Host = ApplicationOptions.EmailServer;
                        smtp.EnableSsl = ApplicationOptions.IsEmailSslEnabled;
                        if (int.TryParse(ApplicationOptions.EmailPort, out int port) && port > 0)
                        {
                            smtp.Port = port;
                        }
                        if (ApplicationOptions.IsEmailAuthenticationRequired)
                        {
                            smtp.Credentials = new NetworkCredential(
                                ApplicationOptions.EmailUser,
                                ApplicationOptions.EmailPassword);
                        }

                        await smtp.SendMailAsync(message);
                    }
                }
            }
            catch
            {
                // Silently ignore errors.
            }
        }

        public static void SendTest(
            string server,
            string port,
            bool isSslEnabled,
            bool isAuthRequired,
            string username,
            System.Security.SecureString password,
            string fromAddress,
            string recipientAddress)
        {
            using (MailMessage message = new MailMessage())
            {
                message.Subject = "[vmPing] Test email notification";
                message.Body = $"This is a test email sent by vmPing on {DateTime.Now:F}";
                message.From = new MailAddress(fromAddress, "vmPing");
                message.To.Add(recipientAddress.Replace(";", ","));

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = server;
                    smtp.EnableSsl = isSslEnabled;
                    if (int.TryParse(port, out int portNumber) && portNumber > 0)
                    {
                        smtp.Port = portNumber;
                    }
                    if (isAuthRequired)
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(username, password);
                    }

                    smtp.Send(message);
                }
            }
        }
    }
}
