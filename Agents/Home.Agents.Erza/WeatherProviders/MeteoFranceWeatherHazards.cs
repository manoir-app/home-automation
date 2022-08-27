using Home.Graph.Common;
using Home.Common;
using Home.Common.Model;
using System.IO;
using System.IO.Compression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using System.Globalization;

namespace Home.Agents.Erza.WeatherProviders
{
    public class MeteoFranceWeatherHazards : ITask<Location>
    {
        private static string _lastMeteoFranceChecksum = "";


        public bool Run(TaskContextKind contextKind, string contextName, Location loc)
        {
            if (contextKind != TaskContextKind.Agent)
                return false;


            string country = loc.Country, zip = loc.ZipCode;

            if (country == null || zip == null)
                return false;
            List<WeatherHazard> lst = null;
            switch (country.ToUpperInvariant())
            {
                case "FRA":
                    lst = CheckMeteoFrance(country, zip);
                    break;
                default:
                    return false;
            }

            if (lst != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var cli = new MainApiAgentWebClient(contextName))
                        {
                            bool b = cli.UploadData<bool, List<WeatherHazard>>("v1.0/system/mesh/local/location/infos/weatherhazard", "POST", lst);
                            break;
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="country"></param>
        /// <param name="zip"></param>
        /// <returns>null si il n'y a rien à changer, la liste des nouveaux
        /// états (éventuellement vide si aucune alerte) si on a récupéré
        /// les infos</returns>
        internal static List<WeatherHazard> CheckMeteoFrance(string country, string zip)
        {
            if (!country.Equals("FRA"))
                return null;

            var ret = new List<WeatherHazard>();

            if (zip.Length > 2)
                zip = zip.Substring(0, 2);

            string data = null;
            using (var cli = new WebClient())
                data = cli.DownloadString("http://vigilance2019.meteofrance.com/data/vigilance_controle.txt");

            using (var rdr = new StringReader(data))
            {
                string s = rdr.ReadLine();
                if (!string.IsNullOrEmpty(s))
                {
                    // on pourra éventuellement parser la date
                }
                s = rdr.ReadLine();
                if (!string.IsNullOrEmpty(s))
                {
                    string[] prts = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (prts.Length == 3) // on doit avoir 3 parties avec le formulaire actuel
                    {
                        if (!prts[0].Equals(_lastMeteoFranceChecksum))
                        {
                            _lastMeteoFranceChecksum = prts[0];
                            DownloadAndParseMeteoFrance(zip, ret);
                        }
                        else
                            return null; // rien de nouveau, on n'a rien à remonter
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return ret;
        }

        private static void DownloadAndParseMeteoFrance(string zip, List<WeatherHazard> ret)
        {
            string pthTmp = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            string pthUnzip = Path.Combine(pthTmp, "unzip");
            try
            {
                if (!Directory.Exists(pthTmp))
                    Directory.CreateDirectory(pthTmp);

                using (var cli = new WebClient())
                    cli.DownloadFile("http://vigilance2019.meteofrance.com/data/vigilance.zip",
                        Path.Combine(pthTmp, "vigilance.zip"));

                ZipFile.ExtractToDirectory(Path.Combine(pthTmp, "vigilance.zip"), pthUnzip);

                foreach (string file in Directory.GetFiles(pthUnzip,
                    "NXFR*.*",
                    new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive }))
                {
                    string filename = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                    if (filename.StartsWith("nxfr33"))
                        ParseNxFR33(file, zip, ret);
                }

            }
            finally
            {
                if (Directory.Exists(pthTmp))
                {
                    try
                    {
                        Directory.Delete(pthTmp, true);
                    }
                    catch
                    {

                    }
                }
            }
        }

        private static void ParseNxFR33(string file, string zip, List<WeatherHazard> ret)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            DateTimeOffset dtDebut = DateTime.Now, dtFin = dtDebut.AddDays(1);

            XmlElement elmDate = doc.SelectSingleNode("/CV/EV") as XmlElement;
            if (elmDate != null)
            {
                dtDebut = DateTimeOffset.ParseExact(elmDate.GetAttribute("daterun"), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                dtFin = DateTimeOffset.ParseExact(elmDate.GetAttribute("dateprevue"), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            }

            foreach (XmlElement elm in doc.SelectNodes("/CV/DV[@dep='" + zip + "']"))
            {
                WeatherHazardSeverity hazardSeverity = WeatherHazardSeverity.Mild;
                int coul = 0;
                if (!int.TryParse(elm.GetAttribute("coul"), out coul))
                    coul = -1;

                switch (coul)
                {
                    case 1: // vert, pas de risques
                        break;
                    case 2:
                        hazardSeverity = WeatherHazardSeverity.Mild;
                        break;
                    case 3:
                        hazardSeverity = WeatherHazardSeverity.Moderate;
                        break;
                    case 4:
                        hazardSeverity = WeatherHazardSeverity.Important;
                        break;
                    default:
                        if (coul > 4)
                            hazardSeverity = WeatherHazardSeverity.Important;
                        else
                            hazardSeverity = WeatherHazardSeverity.Moderate;
                        break;
                }

                foreach (XmlElement risque in elm.SelectNodes("risque"))
                {
                    var hazard = new WeatherHazard()
                    {
                        DateDebut = dtDebut,
                        DateFin = dtFin,
                        Severity = hazardSeverity
                    };

                    switch (risque.GetAttribute("val"))
                    {
                        case "1":
                            hazard.Kind = WeatherHazardKind.Wind;
                            break;
                        case "2":
                            hazard.Kind = WeatherHazardKind.HardRain;
                            break;
                        case "3":
                            hazard.Kind = WeatherHazardKind.Storm;
                            break;
                        case "4":
                            hazard.Kind = WeatherHazardKind.Flood;
                            break;
                        case "5":
                            hazard.Kind = WeatherHazardKind.Snow;
                            break;
                        case "6":
                            hazard.Kind = WeatherHazardKind.HighTemperature;
                            break;
                        case "7":
                            hazard.Kind = WeatherHazardKind.LowTemperature;
                            break;
                        default:
                            hazard.Kind = WeatherHazardKind.Other;
                            break;
                    }

                    ret.Add(hazard);
                }
            }
        }
    }
}
