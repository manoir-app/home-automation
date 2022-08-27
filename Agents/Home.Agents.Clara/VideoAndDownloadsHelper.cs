using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Home.Agents.Clara
{
    public static class VideoAndDownloadsHelper
    {
        public static VideoMetadata GetMeta(string[] parts, bool includeAdult)
        {
            List<string> titleParts = GetTitleParts(parts);

            // on va essayer de trouver qq chose avec ces morceaux de titre
            var lst = SearchMovieDb(string.Join(' ', titleParts), includeAdult);
            if (lst.Count > 0)
                return Convert(lst[0]);

            return null;
        }


        private static Regex regExEpisode = new Regex(@"(?<episode>S\d{1,3}E\d{1,3})", RegexOptions.IgnoreCase);
    
        public static string SanitizeTitle(string fileTitle)
        {

            Match mEpisode = regExEpisode.Match(fileTitle);
            if (mEpisode != null && mEpisode.Success)
            {
                fileTitle = fileTitle.Replace(mEpisode.Value, "");
            }


            string[] parts = fileTitle.Split(new char[] { ' ', '.', ',', '-', '_', '[', '(', ')', ']' },
                    StringSplitOptions.RemoveEmptyEntries);

            var sant = GetTitleParts(parts);
            return string.Join(' ', sant);
        }


        private static List<string> GetTitleParts(string[] parts)
        {
            List<string> titleParts = new List<string>();

            foreach (var p in parts)
            {
                // ici il faudra mettre la partie ML, en attendant
                // un bon vieux switch :)
                int i = 0;
                string plow = p.ToLowerInvariant();
                if (int.TryParse(p, out i))
                {
                    if(i > 1900 && i<2200)
                        continue;
                }
                else if (plow.EndsWith('p') && int.TryParse(p.Substring(0, p.Length - 1), out i))
                {
                    // résolution type 720p
                    continue;
                }
                else if (plow.EndsWith('x') && int.TryParse(p.Substring(1), out i))
                {
                    // x264
                    continue;
                }
                else if (plow.EndsWith('h') && int.TryParse(p.Substring(1), out i))
                {
                    // h264, h265, etc.
                    continue;
                }
                else
                {
                    bool ignore = false;
                    switch (plow)
                    {
                        case "fullhd":
                        case "4k":
                            ignore = true;
                            break;
                        case "xvid":
                        case "hdlight":
                        case "dvdrip":
                        case "hdtv":
                        case "bdrip":
                        case "bluray":
                        case "webrip":
                        case "repack":
                        case "subforced":
                            ignore = true;
                            break;
                        case "fr":
                        case "french":
                        case "truefrench":
                            ignore = true;
                            break;
                        case "vostfr":
                        case "english":
                        case "german":
                        case "vo":
                            ignore = true;
                            break;
                        case "saison":
                            ignore = true;
                            break;
                        // les extensions de nom de domaine
                        // et de fichier habituelles
                        case "com":
                        case "cc":
                        case "avi":
                        case "mkv":
                        case "mp4":
                        case "mpeg4":
                        case "vid":
                        case "bz":
                        case "tv":
                            ignore = true;
                            break;
                        // et quelques sites :)
                        case "emule-island":
                        case "oxtorrent":
                        case "kj":
                        case "gktorrent":
                        case "torrent9":
                        case "cpasbien":
                            ignore = true;
                            break;
                    }
                    if (ignore)
                        continue;
                }
                titleParts.Add(p);
            }

            return titleParts;
        }

        private static VideoMetadata Convert(MovieDbSearchResult searchres)
        {
            var det = GetMovieDbDetails(searchres.id, searchres.media_type);
            if (det == null)
                return null;

            VideoMetadata ret = new VideoMetadata()
            {
                IsAdultContent = searchres.adult,
                MovieDbId = searchres.id.ToString(),
                OriginalLanguage = det.original_language,
                LocalTitle = det.title??det.original_name,
                OfficialTitle = det.original_title??det.original_name,
                Overview = det.overview,
                Status = det.status==null?"Released":det.status
            };
            ret.Actors = (from z in det.credits.cast
                          orderby z.order ascending
                          select z.name).ToArray();
            ret.Directors = (from z in det.credits.crew
                          where z.job.Equals("director", StringComparison.InvariantCultureIgnoreCase)
                          orderby z.order ascending
                          select z.name).ToArray();
            ret.Producers = (from z in det.credits.crew
                             where z.job.Equals("producer", StringComparison.InvariantCultureIgnoreCase)
                             orderby z.order ascending
                             select z.name).ToArray();
            ret.GenreCodes = (from z in det.genres
                              select z.name).ToArray();
            return ret;
        }

        private static string key = "52f9770b243c52a03276c4e9574904b3";
        private static string lang = "fr-FR";

        private static MovieDbGetDetailsResponse GetMovieDbDetails(int id, string type)
        {
            string url = $"https://api.themoviedb.org/3/{type}/{id}?api_key=52f9770b243c52a03276c4e9574904b3&language={lang}&append_to_response=credits";

            using (var cli = new WebClient())
            {
                var data = cli.DownloadString(url);
                var result = JsonConvert.DeserializeObject<MovieDbGetDetailsResponse>(data);
                return result;
            }

            return null;
        }


        private static List<MovieDbSearchResult> SearchMovieDb(string title, bool includeAdult)
        {
            if (string.IsNullOrEmpty(title))
                return new List<MovieDbSearchResult>();

            var qstring = Uri.EscapeDataString(title);
            string url = $"https://api.themoviedb.org/3/search/multi?api_key={key}&language={lang}&query={qstring}&page=1&include_adult={includeAdult}";

            using (var cli = new WebClient())
            {
                var data = cli.DownloadString(url);
                var result = JsonConvert.DeserializeObject<MovieDbSearchRootobject>(data);
                if (result != null && result.results != null && result.results.Length > 0)
                {
                    var items = (from z in result.results
                                 where z.media_type.Equals("movie") || z.media_type.Equals("tv")
                                 select z).ToList();
                    items = FilterResults(items, title);
                    return items;
                }
            }

            return new List<MovieDbSearchResult>();
        }

        private static List<MovieDbSearchResult> FilterResults(List<MovieDbSearchResult> items, string title)
        {
            var ret = new List<MovieDbSearchResult>();
            foreach (var item in items)
            {
                string ittitle = SanitizeFromMovieDb(item.title??item.name) ;
                string itsubti = SanitizeFromMovieDb(item.original_title??item.original_name);

                if (ittitle!=null && ittitle.Equals(title))
                    ret.Add(item);
                else if (itsubti!=null && itsubti.Equals(title))
                    ret.Add(item);
            }

            if (ret.Count > 0)
                return ret;

            // pour l'instant pas de recherche plus compliquées
            return ret;
        }

        private static string SanitizeFromMovieDb(string title)
        {
            if (title == null)
                return null;
            string[] parts = title.Split(new char[] { ' ', '.', ',', '-', '_', '[', '(', ')', ']' },
                StringSplitOptions.RemoveEmptyEntries);

            return string.Join(' ', parts);
        }

        internal static VideoMetadata GetMeta(string fileTitle, bool includeAdult)
        {
            string[] parts = fileTitle.Split(new char[] { ' ', '.', ',', '-', '_', '[', '(', ')', ']' },
                StringSplitOptions.RemoveEmptyEntries);
            return GetMeta(parts, includeAdult);
        }


        private class MovieDbSearchRootobject
        {
            public int page { get; set; }
            public MovieDbSearchResult[] results { get; set; }
            public int total_pages { get; set; }
            public int total_results { get; set; }
        }

        private class MovieDbSearchResult
        {
            public string backdrop_path { get; set; }
            public string first_air_date { get; set; }
            public int?[] genre_ids { get; set; }
            public int id { get; set; }
            public string media_type { get; set; }
            public string name { get; set; }
            public string[] origin_country { get; set; }
            public string original_language { get; set; }
            public string original_name { get; set; }
            public string overview { get; set; }
            public float popularity { get; set; }
            public string poster_path { get; set; }
            public float vote_average { get; set; }
            public int vote_count { get; set; }
            public bool adult { get; set; }
            public string original_title { get; set; }
            public string release_date { get; set; }
            public string title { get; set; }
            public bool video { get; set; }
            public int gender { get; set; }
            public string known_for_department { get; set; }
            public object profile_path { get; set; }
        }



        private class MovieDbGetDetailsResponse
        {



            public string backdrop_path { get; set; }
            public object[] created_by { get; set; }
            public object[] episode_run_time { get; set; }
            public string first_air_date { get; set; }
            public Genre[] genres { get; set; }
            public string homepage { get; set; }
            public int id { get; set; }
            public bool in_production { get; set; }
            public object[] languages { get; set; }
            public string last_air_date { get; set; }
            public Last_Episode_To_Air last_episode_to_air { get; set; }
            public string name { get; set; }
            public object next_episode_to_air { get; set; }
            public object[] networks { get; set; }
            public int? number_of_episodes { get; set; }
            public int? number_of_seasons { get; set; }
            public object[] origin_country { get; set; }
            public string original_language { get; set; }
            public string original_name { get; set; }
            public string overview { get; set; }
            public float? popularity { get; set; }
            public string poster_path { get; set; }
            public ProductionCompany[] production_companies { get; set; }
            public Country[] production_countries { get; set; }
            public Season[] seasons { get; set; }
            public Spoken_Languages[] spoken_languages { get; set; }
            public string status { get; set; }
            public string tagline { get; set; }
            public string type { get; set; }
            public float vote_average { get; set; }
            public int? vote_count { get; set; }
            public Credits credits { get; set; }


            public bool adult { get; set; }
            public object belongs_to_collection { get; set; }
            public int? budget { get; set; }
            public string imdb_id { get; set; }
            public string original_title { get; set; }
            public string release_date { get; set; }
            public int? revenue { get; set; }
            public int? runtime { get; set; }
            public string title { get; set; }
            public bool video { get; set; }
        }


        private class Country
        {
            public string iso_3166_1 { get; set; }
            public string name { get; set; }
        }


        private class ProductionCompany
        {
            public int? id { get; set; }
            public string logo_path { get; set; }
            public string name { get; set; }
            public string origin_country { get; set; }
        }


        private class Last_Episode_To_Air
        {
            public string air_date { get; set; }
            public int? episode_number { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public string overview { get; set; }
            public string production_code { get; set; }
            public int? season_number { get; set; }
            public string still_path { get; set; }
            public float vote_average { get; set; }
            public int? vote_count { get; set; }
        }


        private class Credits
        {
            public Cast[] cast { get; set; }
            public Crew[] crew { get; set; }
        }

        private class Cast
        {
            public bool adult { get; set; }
            public int? gender { get; set; }
            public int id { get; set; }
            public string known_for_department { get; set; }
            public string name { get; set; }
            public string original_name { get; set; }
            public float popularity { get; set; }
            public string profile_path { get; set; }
            public string character { get; set; }
            public string credit_id { get; set; }
            public int? order { get; set; }
        }

        private class Crew
        {
            public bool adult { get; set; }
            public int? gender { get; set; }
            public int id { get; set; }
            public string known_for_department { get; set; }
            public string name { get; set; }
            public string original_name { get; set; }
            public float popularity { get; set; }
            public string profile_path { get; set; }
            public string character { get; set; }
            public string credit_id { get; set; }
            public int? order { get; set; }
            public string job { get; set; }
        }


        private class Genre
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        private class Season
        {
            public string air_date { get; set; }
            public int? episode_count { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public string overview { get; set; }
            public string poster_path { get; set; }
            public int? season_number { get; set; }
        }

        private class Spoken_Languages
        {
            public string english_name { get; set; }
            public string iso_639_1 { get; set; }
            public string name { get; set; }
        }


    }
}
