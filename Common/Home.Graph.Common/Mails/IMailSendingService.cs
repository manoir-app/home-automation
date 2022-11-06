using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Xml;

namespace Home.Graph.Common.Mails
{
    [Serializable]
    public class MailSendException : ApplicationException
    {
        public MailSendException() { }
        public MailSendException(string message) : base(message) { }
        public MailSendException(string message, Exception inner) : base(message, inner) { }
        protected MailSendException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class MailSendResult
    {
        public string ExternalId { get; set; }
    }

    public class DataTemplateGenerique
    {
        public string Id { get; set; } // string plutot
        public string Libelle { get; set; }
        public string Description { get; set; }
        public string UrlVisuModif { get; set; }
        public string Auteur { get; set; }
        public string[] Purpose { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    public enum MailPurpose { Newsletter, Transactional, None }

    public interface IEmailSendingService
    {
        MailSendResult Send(string agent, Account account, string idTemplate, object variables, MailAddress[] sendTo, MailAddress[] ccs);
        MailSendResult Send(string agent, Account account, MailMessage message);
        DataTemplateGenerique[] GetTemplates(string agent, MailPurpose purpose);
        bool ConfigureAccount(string agent, Account account, string userId);
        bool DeleteAccount(string agent, Account account, string userId);
        string GetEditUrl(string idTemplate);

    }


    public class Account
    {
        public string MelConfigEnvoi { get; set; }
        public string Email { get; set; } //adresse mail
        public string NomPublic { get; set; }

        public T ConvertMelConfigEnvoi<T>()
        {
            return JsonConvert.DeserializeObject<T>(MelConfigEnvoi);
        }
    }
}
