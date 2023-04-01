using CsvHelper;
using Home.Agents.Clara.SaaS;
using Home.Common;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Home.Agents.Clara.Mails
{
    public class MailHelper
    {
        private static Regex rxIsHtml = new Regex(@"\<html\>(.|\n)*\<\/html\>");
        private static Regex rxJsonLD = new Regex(@"\<script(\n|.)*\>(\n|\w)*(?<json>(\n|.)*)(\n|\w)*\<\/script\>");











        public void ParseMail(string userID, string content)
        {
            if (rxIsHtml.Match(content).Success)
            {
                ParseHtmlMail(userID, content);
            }
        }


        public class RootJsonLd
        {
            [JsonProperty("@context")]
            public string context { get; set; }

            [JsonProperty("@type")]
            public string type { get; set; }
        }


        private void ParseHtmlMail(string userID, string content)
        {
            var matchJsonLD = rxJsonLD.Match(content);
            if (matchJsonLD.Success)
            {
                var jsonLD = matchJsonLD.Groups["json"]?.Value;
                if (!string.IsNullOrEmpty(jsonLD))
                {
                    try
                    {
                        object c = JsonConvert.DeserializeObject(jsonLD);
                        if (c != null && c is IEnumerable)
                        {
                            var root = JsonConvert.DeserializeObject<RootJsonLd[]>(jsonLD);
                            foreach (var r in root)
                            {
                               
                            }
                        }
                        else
                        {

                            var root = JsonConvert.DeserializeObject<RootJsonLd>(jsonLD);
                            ParseObject(userID, jsonLD, root);
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        private void ParseObject(string userID, string jsonLD, RootJsonLd root)
        {
            if (root != null && !string.IsNullOrEmpty(root.type))
            {
                switch (root.type.ToLowerInvariant())
                {
                    case "trainreservation":
                    case "flightreservation":
                    case "rentalcarreservation":
                    case "lodgingreservation":
                    case "foodestablishmentreservation":
                    case "busreservation":
                    case "eventreservation":
                        ParseReservation(userID, jsonLD);
                        break;
                }
            }
        }

        public class Reservation : RootJsonLd
        {
            public string reservationNumber { get; set; }
            public string reservationStatus { get; set; }
            public Undername underName { get; set; }
            public Reservationfor reservationFor { get; set; }

            // pour hotel
            public DateTime? checkinDate { get; set; }
            public DateTime? checkoutDate { get; set; }

            // pour avion
            public DateTime? departureTime { get; set; }
            public DateTime? arrivalTime { get; set; }

            // pour reservation auto
            public DateTime? pickupTime { get; set; }
            public DateTime? dropoffTime { get; set; }

        }

        public class Undername
        {
            [JsonProperty("@type")]
            public string type { get; set; }
            public string name { get; set; }
        }

        public class Reservationfor
        {
            [JsonProperty("@type")]
            public string type { get; set; }
            public string name { get; set; }
            public string model { get; set; } // car
            public string flightNumber { get; set; } // avion
            public AirLine airline { get; set; } // avion
            public Address address { get; set; }
            public string telephone { get; set; }
        }


        public class AirLine
        {
            [JsonProperty("@type")]
            public string type { get; set; }
            public string name { get; set; }
            public string iataCode { get; set; }
        }


        public class Address
        {
            [JsonProperty("@type")]
            public string type { get; set; }
            public string streetAddress { get; set; }
            public string addressLocality { get; set; }
            public string addressRegion { get; set; }
            public string postalCode { get; set; }
            public string addressCountry { get; set; }
        }


        private void ParseReservation(string userID, string jsonLD)
        {

        }

        internal static void CheckForNewMails()
        {
            List<MailServiceItem> mails = new List<MailServiceItem>();
            try
            {
                var svcs = GetServices();
                foreach (var svc in svcs)
                {
                    try
                    {
                        var tmp = svc.CheckForMails();
                        if (tmp != null && tmp.Count > 0)
                            mails.AddRange(tmp);
                    }
                    catch (NotImplementedException)
                    {
                        try
                        {
                            //svc.CheckForMails();
                        }
                        catch (NotImplementedException)
                        {

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("MailHelper-CheckMail error : " + ex.ToString());
            }


            foreach(var mailItem in mails)
            {

            }


        }

        private static List<IMailService> GetServices()
        {
            // pour l'instant en dur à partir des tokens

            List<IMailService> ret = new List<IMailService>();

            List<ExternalToken> tokens = new List<ExternalToken>();
            var usrs = AgentHelper.GetMainUsers("clara");
            foreach(var usr in usrs)
            {
                var tk = AgentHelper.GetUserExternalToken("clara", usr.Id, "azuread");
                if (tk != null)
                {
                    tokens.Add(tk);
                    ret.Add(new Microsoft365MailService(usr.Id, tk));
                }
            }

            return ret;
        }

    }
}
