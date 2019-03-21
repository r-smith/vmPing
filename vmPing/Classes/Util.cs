using System;
using System.Net;
using System.Net.Mail;

namespace vmPing.Classes
{
    class Util
    {
        public static void SendEmail(string hostStatus, string hostName)
        {
            var serverAddress = ApplicationOptions.EmailServer;
            var serverUser = ApplicationOptions.EmailUser;
            var serverPassword = ApplicationOptions.EmailPassword;
            var serverPort = ApplicationOptions.EmailPort;
            var mailFromAddress = ApplicationOptions.EmailFromAddress;
            var mailFromFriendly = "vmPing";
            var mailToAddress = ApplicationOptions.EmailRecipient;
            var mailSubject = $"[vmPing] {hostName} <> Host {hostStatus}";
            var mailBody =
                $"{hostName} is {hostStatus}.{Environment.NewLine}" +
                $"{DateTime.Now.ToLongDateString()}  {DateTime.Now.ToLongTimeString()}";

            var message = new MailMessage();

            try
            {
                var smtpClient = new SmtpClient();
                MailAddress fromAddress;
                if (mailFromFriendly.Length > 0)
                    fromAddress = new MailAddress(mailFromAddress, mailFromFriendly);
                else
                    fromAddress = new MailAddress(mailFromAddress);

                smtpClient.Host = serverAddress;

                if (ApplicationOptions.IsEmailAuthenticationRequired)
                {
                    smtpClient.Credentials = new NetworkCredential(serverUser, serverPassword);
                }

                if (serverPort.Length > 0)
                    smtpClient.Port = Int32.Parse(serverPort);

                message.From = fromAddress;
                message.Subject = mailSubject;
                message.Body = mailBody;

                message.To.Add(mailToAddress);

                //Send the email.
                smtpClient.Send(message);
            }
            catch
            {
                // There was an error sending Email.
            }
            finally
            {
                message.Dispose();
            }
        }
    }
}
