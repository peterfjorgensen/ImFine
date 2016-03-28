using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace IoTHubServiceBackend
{
    public class EmailController
    {
        #region Private fields
        SmtpClient client;
        #endregion

        #region Properties
        private string host;
        public string Host
        {
            get { return host; }
            set { host = value; }
        }

        private int port;
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        private string domain;
        public string Domain
        {
            get { return domain; }
            set { domain = value; }
        }

        private bool useSSL;
        public bool UseSSL
        {
            get { return useSSL; }
            set { useSSL = value; }
        }

        private bool useDefaultCredentials;
        public bool UseDefaultCredentials
        {
            get { return useDefaultCredentials; }
            set { useDefaultCredentials = value; }
        }

        private string subject;
        public string MailSubject
        {
            get { return subject; }
            set { subject = value; }
        }

        private string userName;
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        private string userPassword;
        public string UserPassword
        {
            get { return userPassword; }
            set { userPassword = value; }
        }

        private string from;
        public string From
        {
            get { return from; }
            set { from = value; }
        }

        private List<Attachment> attachments;
        public List<Attachment> Attachments
        {
            get { return attachments; }
            //            set { attachments = value; }
        }

        #endregion

        #region Construction
        public EmailController()
        {
            this.client = new SmtpClient();
            this.domain = "";
            this.host = "";
            this.port = 25;
            this.useSSL = false;

            this.useDefaultCredentials = false;
            this.userName = "";
            this.userPassword = "";

            this.from = "";
            this.subject = "";

            this.attachments = new List<Attachment>();
        }

        public EmailController(string Username, string Password)
        {
            this.client = new SmtpClient();
            this.domain = "";
            this.host = "";
            this.port = 25;
            this.useSSL = false;

            this.useDefaultCredentials = false;
            this.userName = Username;
            this.userPassword = Password;

            this.from = "";
            this.subject = "";

            this.attachments = new List<Attachment>();
        }

        public EmailController(string Host, int Port, string Username, string Password, string From)
        {
            this.client = new SmtpClient();
            this.domain = "";
            this.host = Host;
            this.port = Port;
            this.useSSL = false;

            this.useDefaultCredentials = false;
            this.userName = Username;
            this.userPassword = Password;

            this.from = From;
            this.subject = "";

            this.attachments = new List<Attachment>();
        }
        #endregion

        #region Sending emails
        public string SendEmail(string EmailAddress, string Message)
        {
            return SendEmail(EmailAddress, Message, this.Attachments);
        }

        public string SendEmail(string EmailAddress, string Subject, string Message)
        {
            this.MailSubject = Subject;
            return SendEmail(EmailAddress, Message, this.Attachments);
        }

        public string SendEmail(string EmailAddress, string Message, List<Attachment> Attachments)
        {
            string result = "";
            if (IsValidReceiver(EmailAddress))
            {
                // Create email client
                this.client.Host = this.host;
                this.client.Port = this.port;
                this.client.UseDefaultCredentials = this.useDefaultCredentials;
                this.client.EnableSsl = this.useSSL;
                this.client.SendCompleted += new SendCompletedEventHandler(client_SendCompleted);

                // Set user credentials
                System.Net.NetworkCredential credentials;
                if (!this.client.UseDefaultCredentials)
                {
                    credentials = new System.Net.NetworkCredential(this.userName, this.userPassword);
                    if (this.domain != "")
                        credentials.Domain = this.domain;
                    this.client.Credentials = credentials;
                }

                //// Create sender and receivers
                string displayName = "ImFine";
                MailAddress sender = new MailAddress(this.from, displayName);
                MailAddress receiver = new MailAddress(EmailAddress);

                // Create email message
                MailMessage email = new MailMessage();
                email.BodyEncoding = UTF8Encoding.UTF8;
                email.IsBodyHtml = true;
                email.Priority = MailPriority.Normal;

                email.To.Add(receiver);
                email.From = sender;

                email.Subject = this.MailSubject;
                email.Body = Message;

                foreach (Attachment attachment in this.attachments)
                {
                    email.Attachments.Add(attachment);
                }

                try
                {
                    this.client.SendAsync(email, EmailAddress);
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }
            else
            {
                // TODO logging
            }
            return result;
        }

        void client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            string error = "";
            try
            {
                string s = (string)e.UserState;
                if (e.Error != null)
                {
                    error = e.Error.Message;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
        }
        #endregion

        #region Private helper methods
        private static bool IsValidReceiver(string EmailAddress)
        {
            // Dummy validation
            return true;

            // TODO Implement real validation of the email address
        }
        #endregion

    }
}
