using AdaptiveCards.Rendering;
using Home.Common;
using Home.Common.Model;
using MongoDB.Driver.Core.Servers;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Home.Graph.Common.Mails
{
    public class MailJetSendingService : IEmailSendingService
    {
        public class MailJetConfig
        {
            public string ApiKey { get; set; }
            public string ApiSecret { get; set; }
        }


        public MailSendResult Send(string agent, Account account, MailMessage message)
        {
            try
            {
                var config = account.ConvertMelConfigEnvoi<MailJetConfig>();

                var rep = EnvoyerParMailJet(config, message);
                return new MailSendResult() { ExternalId = rep.Messages[0].CustomID };
            }
            catch (MailSendException ex)
            {
                Console.Error.WriteLine(ex);
                return null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return null;
            }
        }


        public DataTemplateGenerique[] GetTemplates(string agent, MailPurpose purpose)
        {
            //try
            //{
            //    var rep = new MailJetBll(rjs_id).GetTemplates(purpose);
            //    return ConvertToGenericClass(rep);
            //}
            //catch (Exception ex)
            //{
            //    Console.Error.WriteLine(ex);
                return null;
            //}
        }


        //private static DataTemplateGenerique[] ConvertToGenericClass(TemplatesResponse mailJetClass)
        //{
        //    List<DataTemplateGenerique> liste = new List<DataTemplateGenerique>();

        //    foreach (var oldT in mailJetClass.Data)
        //    {
        //        DataTemplateGenerique newT = new DataTemplateGenerique()
        //        {
        //            Id = oldT.ID,
        //            Libelle = oldT.Name,
        //            Description = oldT.Description,
        //            UrlVisuModif = string.Format("https://app.mailjet.com/template/{0}/build", oldT.ID),
        //            Auteur = oldT.Author,
        //            CreatedAt = oldT.CreatedAt,
        //            LastUpdatedAt = oldT.LastUpdatedAt
        //        };
        //        List<string> listePurposes = new List<string>();
        //        foreach (var p in oldT.Purposes)
        //            listePurposes.Add(p);
        //        newT.Purpose = listePurposes.ToArray();

        //        liste.Add(newT);
        //    }

        //    return liste.ToArray();
        //}

        public MailSendResult Send(string agent, Account account, string idTemplate, object variables, MailAddress[] sendTo, MailAddress[] ccs)
        {
            if (string.IsNullOrEmpty(account.Email))
                throw new ArgumentNullException("L'expéditeur du message ne peut pas être vide");

            var config = account.ConvertMelConfigEnvoi<MailJetConfig>();

            try
            {
                var from = new MailAddress(account.Email);
                var rep = EnvoyerParMailJetAvecTemplate(config, idTemplate, variables, from, sendTo);
                string externalId = rep.Messages[0].CustomID;
                return new MailSendResult() { ExternalId = externalId };
            }
            catch (MailSendException ex)
            {
                Console.Error.WriteLine(ex);
                return null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return null;
            }
        }

        public bool ConfigureAccount(string agent, Account account, string user)
        {
            //if (account == null)
            //    throw new NullReferenceException("Le compte ne peut pas être null.");

            //if (string.IsNullOrEmpty(account.Email))
            //    throw new NullReferenceException("L'adresse mail ne peut pas être vide.");

            //MailJetBll bll = new MailJetBll(rjs_id);

            //try
            //{
            //    bool compteEstCree = false;
            //    string status = bll.GetStatusSenderMaiJet(account.Email);
            //    switch (status)
            //    {
            //        case "Deleted":
            //            compteEstCree = bll.ReactiverSenderMailJet(account.Email);
            //            break;

            //        case null:
            //            compteEstCree = bll.CreerSenderMailJet("bulk", false, account.NomPublic, account.Email);
            //            break;
            //    }

            //    if (compteEstCree)
            //    {
            //        ToDoBll toDoBll = new ToDoBll(rjs_id);
            //        string libelle = "Un compte MailJet est en attente de validation.";
            //        string details = $"Un mail vous a été envoyé. Veuillez vous rendre sur {account.Email} afin de valider son utilisation.";
            //        if (uxidUtilisateur == Guid.Empty)
            //            toDoBll.CreerTodoErreurSysteme(ToDoBll.tdoEmailAValiderChezPrestaMailJet, libelle, details);
            //        else
            //        {
            //            toDoBll.CreerTodoUtilisateur(uxidUtilisateur, ToDoBll.tdoEmailAValiderChezPrestaMailJet,
            //                null, Guid.Empty, libelle, "", false, DateTime.MinValue, details, null);
            //        }
            //        return true;
            //    }
            //    else
            //        return false;

            //}
            //catch (Exception ex)
            //{
            //    Console.Error.WriteLine(ex);
                return false;
            //}
        }

        public bool DeleteAccount(string agent, Account account, string user)
        {

            //if (account == null)
            //    throw new NullReferenceException("Le compte ne peut pas être null.");

            //if (string.IsNullOrEmpty(account.Email))
            //    throw new NullReferenceException("L'adresse mail ne peut pas être vide.");

            //MailJetBll bll = new MailJetBll(rjs_id);

            //try
            //{
            //    bll.SupprimerSenderMailJet(account.Email);
            //    return true;
            //}
            //catch (Exception ex)
            //{
                return false;
            //}
        }

        public string GetEditUrl(string idTemplate)
        {
            return $"https://app.mailjet.com/template/{idTemplate}/version-history";
        }




        public class WrapReponse
        {
            public List<MailJetReponse> Messages { get; set; }
        }

        public class MailJetReponse
        {
            public string CustomID { get; set; }
            public string Status { get; set; }
            public List<MailJetErreursData> Errors { get; set; }
            public List<MailJetReponseData> To { get; set; }
            public List<MailJetReponseData> Cc { get; set; }
            public List<MailJetReponseData> Bcc { get; set; }

        }
        public class MailJetReponseData
        {
            public string Email { get; set; }
            public string MessageUUID { get; set; }
            public long MessageID { get; set; }
            public string MessageHref { get; set; }
        }

        public class MailJetErreursData
        {
            public string ErrorIdentifier { get; set; }
            public string ErrorCode { get; set; }
            public int StatusCode { get; set; }
            public string ErrorMessage { get; set; }
            public List<string> ErrorRelatedTo { get; set; }
        }

        private class WrapMessages
        {
            public List<MessageData> Messages { get; set; }
            public bool SandboxMode { get; set; }
        }

        private class MessageData
        {
            public string CustomID { get; set; }
            public InterlocuteurInfo From { get; set; }
            public List<InterlocuteurInfo> To { get; set; }
            public List<InterlocuteurInfo> Cc { get; set; }
            public List<InterlocuteurInfo> Bcc { get; set; }
            public string Subject { get; set; }
            public string TextPart { get; set; }
            public string HTMLPart { get; set; }
            public List<PieceJointeData> Attachments { get; set; }
            public string CustomCampaign { get; set; }
            public int? TemplateID { get; set; }
            public object Variables { get; set; }
            public bool TemplateLanguage { get; set; }
        }

        private class PieceJointeData
        {
            public string ContentType { get; set; }
            public string Filename { get; set; }
            public string Base64Content { get; set; }
        }

        private class InterlocuteurInfo
        {
            public string Email { get; set; }
            public string Name { get; set; }
        }

        /// <summary>
        /// Envoi un <see cref="MailMessage"/> via le compte API connecté
        /// à MailJet
        /// </summary>
        /// <param name="m">Le message à envoyer</param>
        /// 
        /// <exception cref="MailSendException">Le message n'a pas pu être envoyé</exception>
        public WrapReponse EnvoyerParMailJet(MailJetConfig tk, MailMessage m)
        {
            return EnvoyerParMailJet(tk, m, null);
        }

        /// <summary>
        /// Envoi un <see cref="MailMessage"/> via le compte API connecté
        /// à MailJet
        /// </summary>
        /// <param name="m">Le message à envoyer</param>
        /// <param name="idCampagne">L'identifiant de la campagne pour stats dans MailJet</param>
        /// 
        /// <exception cref="MailSendException">Le message n'a pas pu être envoyé</exception> 
        public WrapReponse EnvoyerParMailJet(MailJetConfig tk , MailMessage m, string idCampagne)
        {
            var apiKey = tk.ApiKey;
            var secret = tk.ApiSecret;

            MessageData msg = new MessageData();
            InterlocuteurInfo exp = new InterlocuteurInfo();
            List<InterlocuteurInfo> to = new List<InterlocuteurInfo>();
            List<InterlocuteurInfo> cc = new List<InterlocuteurInfo>();
            List<InterlocuteurInfo> bcc = new List<InterlocuteurInfo>();
            List<PieceJointeData> pcJointe = new List<PieceJointeData>();
            msg.From = exp;
            msg.To = to;
            msg.Bcc = bcc;
            msg.Cc = cc;
            msg.Attachments = pcJointe;
            msg.TemplateID = null;

            msg.From.Email = m.From.Address;
            msg.From.Name = m.From.DisplayName;
            msg.HTMLPart = m.Body;

            foreach (var t in m.To)
            {
                InterlocuteurInfo z = new InterlocuteurInfo
                {
                    Email = t.Address,
                    Name = t.DisplayName,
                };

                msg.To.Add(z);
            }
            foreach (var t in m.CC)
            {
                InterlocuteurInfo z = new InterlocuteurInfo
                {
                    Email = t.Address,
                    Name = t.DisplayName,
                };

                msg.Cc.Add(z);
            }
            foreach (var t in m.Bcc)
            {
                InterlocuteurInfo z = new InterlocuteurInfo
                {
                    Email = t.Address,
                    Name = t.DisplayName,
                };

                msg.Bcc.Add(z);
            }

            msg.Subject = m.Subject;

            if (m.IsBodyHtml == true)
            {
                msg.HTMLPart = m.Body;
            }
            else
            {
                msg.TextPart = m.Body;
            }

            foreach (var t in m.Attachments)
            {
                PieceJointeData z = new PieceJointeData
                {
                    ContentType = t.ContentType.MediaType,
                };

                if (t.ContentDisposition.Inline == true)
                {
                    z.Filename = "cid:" + t.Name;
                }
                else
                {
                    z.Filename = t.Name;
                }

                byte[] bytes;
                using (var memoryStream = new MemoryStream())
                {
                    t.ContentStream.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }
                string base64Attachement = Convert.ToBase64String(bytes);
                z.Base64Content = base64Attachement;

                msg.Attachments.Add(z);
            }

            if (idCampagne != null)
                msg.CustomCampaign = idCampagne;


            WrapMessages data = new WrapMessages();
            List<MessageData> wrap = new List<MessageData>();
            data.Messages = wrap;
            data.Messages.Add(msg);
            data.Messages[0].CustomID = Guid.NewGuid().ToString();

#if DEBUG
            data.SandboxMode = true;
#endif

            var json = JsonConvert.SerializeObject(data);
            WrapReponse objReponse;

            using (WebClient cli = new WebClient())
            {
                cli.BaseAddress = "https://api.mailjet.com/v3.1/send";
                string creds = Convert.ToBase64String(Encoding.ASCII.GetBytes(apiKey + ":" + secret));
                cli.Headers.Add("Content-Type", "application/json");
                cli.Headers[HttpRequestHeader.Authorization] = "Basic " + creds;
                cli.Encoding = Encoding.UTF8;
                var resp = cli.UploadString(cli.BaseAddress, json);
                objReponse = JsonConvert.DeserializeObject<WrapReponse>(resp);
            }

            if (objReponse.Messages[0].Status == "error")
            {
                var errors = JsonConvert.SerializeObject(objReponse.Messages[0].Errors);
                string msgErreur = "L'envoi de mail a échoué : " + errors;
                throw new MailSendException(msgErreur);
            }

            return objReponse;
        }

        /// <summary>
        /// Envoi un <see cref="MailMessage"/> via le compte API connecté en utilisant un template
        /// </summary>
        /// <param name="idTemplate">Le template publié sur le compte MailJet</param>
        /// <param name="variables">Valeurs des variables à utiliser dans le template</param>
        /// <param name="expediteur">Adresse de l'expéditeur (doit être présente dans la liste des adresses du compte MailJet)</param>
        /// <param name="destinataires">Adresses des destinataires du mail à envoyer</param>
        /// <param name="subject">Objet du mail à envoyer (si null le sujet renseigné sur le template MailJet sera récupéré et utilisé en objet du mail)</param>
        /// 
        /// <exception cref="MailSendException">Le message n'a pas pu être envoyé</exception> 
        /// <exception cref="Exception">Veuillez renseigner au moins un destinataire</exception>
        /// <exception cref="Exception">Veuillez renseigner un expéditeur</exception>
        public WrapReponse EnvoyerParMailJetAvecTemplate(MailJetConfig tk, string idTemplate, object variables, MailAddress expediteur, MailAddress[] destinataires, string subject = null)
        {
            return EnvoyerParMailJetAvecTemplate(tk, idTemplate, variables, expediteur, destinataires, subject, null, null);
        }

        /// <summary>
        /// Envoi un <see cref="MailMessage"/> via le compte API connecté en utilisant un template. Vous avez la possibilité de choisir si les copies sont cachées ou non avec le booléen destinataireCaches.
        /// </summary>
        /// <param name="idTemplate">Le template publié sur le compte MailJet</param>
        /// <param name="variables">Valeurs des variables à utiliser dans le template</param>
        /// <param name="expediteur">Adresse de l'expéditeur (doit être présente dans la liste des adresses du compte MailJet)</param>
        /// <param name="destinataires">Adresses des destinataires du mail à envoyer</param>
        /// <param name="subject">Objet du mail à envoyer (si null le sujet renseigné sur le template MailJet sera récupéré et utilisé en objet du mail)</param>
        /// <param name="copiesDestinataires">Adresses des destinataires en copie</param>
        /// <param name="destinataireCaches">Détermine si les copies sont cachées</param>
        /// 
        /// <exception cref="MailSendException">Le message n'a pas pu être envoyé</exception> 
        /// <exception cref="Exception">Veuillez renseigner au moins un destinataire</exception>
        /// <exception cref="Exception">Veuillez renseigner un expéditeur</exception>
        public WrapReponse EnvoyerParMailJetAvecTemplate(MailJetConfig tk, string idTemplate, object variables, MailAddress expediteur, MailAddress[] destinataires, string subject, MailAddress[] copiesDestinataires, bool destinataireCaches)
        {
            if (destinataireCaches)
                return EnvoyerParMailJetAvecTemplate(tk, idTemplate, variables, expediteur, destinataires, subject, null, copiesDestinataires);
            else
                return EnvoyerParMailJetAvecTemplate(tk, idTemplate, variables, expediteur, destinataires, subject, copiesDestinataires, null);

        }

        /// <summary>
        /// Envoi un <see cref="MailMessage"/> via le compte API connecté en utilisant un template. Vous avez la possibilité renseigner des destinataires en copie cachées et des destinataires en copie non cachées.
        /// </summary>
        /// <param name="idTemplate"></param>
        /// <param name="variables"></param>
        /// <param name="expediteur"></param>
        /// <param name="destinataires"></param>
        /// <param name="subject"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// 
        /// <exception cref="MailSendException">Le message n'a pas pu être envoyé</exception>
        /// <exception cref="Exception">Le message n'a pas pu être envoyé</exception>
        /// <exception cref="Exception">Veuillez renseigner au moins un destinataire</exception>
        /// <exception cref="Exception">Veuillez renseigner un expéditeur</exception>
        /// <exception cref="Exception">Aucun identifiant n'a été trouvé en base</exception>
        internal static WrapReponse EnvoyerParMailJetAvecTemplate(MailJetConfig tk, string idTemplate, object variables, MailAddress expediteur, MailAddress[] destinataires, string subject, MailAddress[] cc, MailAddress[] bcc)
        {
            var apiKey = tk.ApiKey;
            var secret = tk.ApiSecret;

            MessageData msg = new MessageData();
            msg.From = new InterlocuteurInfo();
            msg.To = new List<InterlocuteurInfo>();

            msg.TemplateID = Int32.Parse(idTemplate);
            msg.Variables = variables;
            msg.From.Email = expediteur.Address;

            foreach (var d in destinataires)
            {
                InterlocuteurInfo z = new InterlocuteurInfo
                {
                    Email = d.Address,
                    Name = d.DisplayName,
                };
                msg.To.Add(z);
            }
            msg.From.Name = !string.IsNullOrEmpty(expediteur.DisplayName) ? expediteur.DisplayName : "";
            msg.Subject = subject;
            msg.TemplateLanguage = true;


            if (cc != null)
            {
                msg.Cc = new List<InterlocuteurInfo>();
                foreach (var t in cc)
                {
                    InterlocuteurInfo z = new InterlocuteurInfo
                    {
                        Email = t.Address,
                        Name = t.DisplayName,
                    };

                    msg.Cc.Add(z);
                }
            }

            if (bcc != null)
            {
                msg.Bcc = new List<InterlocuteurInfo>();
                foreach (var t in bcc)
                {
                    InterlocuteurInfo z = new InterlocuteurInfo
                    {
                        Email = t.Address,
                        Name = t.DisplayName,
                    };

                    msg.Bcc.Add(z);
                }
            }

            WrapMessages data = new WrapMessages();
            List<MessageData> wrap = new List<MessageData>();
            data.Messages = wrap;
            data.Messages.Add(msg);

#if DEBUG
            data.SandboxMode = true;
#endif


            data.Messages[0].CustomID = Guid.NewGuid().ToString();

            var json = JsonConvert.SerializeObject(data);
            WrapReponse objReponse;

            using (WebClient cli = new WebClient())
            {
                cli.BaseAddress = "https://api.mailjet.com/v3.1/send";
                string creds = Convert.ToBase64String(Encoding.ASCII.GetBytes(apiKey + ":" + secret));
                cli.Headers.Add("Content-Type", "application/json");
                cli.Headers[HttpRequestHeader.Authorization] = "Basic " + creds;
                cli.Encoding = Encoding.UTF8;
                var resp = cli.UploadString(cli.BaseAddress, json);
                objReponse = JsonConvert.DeserializeObject<WrapReponse>(resp);
            }

            if (objReponse.Messages[0].Status == "error")
            {
                var errors = JsonConvert.SerializeObject(objReponse.Messages[0].Errors);
                string msgErreur = "L'envoi de mail a échoué : " + errors;
                throw new MailSendException(msgErreur);
            }

            return objReponse;
        }

        private static ExternalToken GetToken()
        {
            throw new NotImplementedException();
        }
    }
}
