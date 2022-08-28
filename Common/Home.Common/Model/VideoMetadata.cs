using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum VideoKind
    {
        Movie,
        TvShow,
        Social,
        Personal
    }

    public class VideoMetadata
    {
        public VideoKind Kind { get; set; }
        public string OfficialTitle { get; set; }
        public string LocalTitle { get; set; }

        public string Resolution { get; set; }


        public bool IsAdultContent { get; set; }

        public string Overview { get; set; }
        public DateTimeOffset? ReleaseDate { get; set; }

        public string Language { get; set; }
        public string OriginalLanguage { get; set; }

        public string MovieDbId { get; set; }
        public string ImbdId { get; set; }

        public string[] Directors { get; set; }
        public string[] Producers { get; set; }
        public string[] Studios { get; set; }
        public string[] Actors { get; set; }
        public string[] OtherCrew { get; set; }

        public string[] GenreCodes { get; set; }

        public string Status { get; set; }

        public SeasonDetails[] Seasons { get; set; }

        public string EpisodeId { get; set; }
    }

    public class SeasonDetails
    {
        public int SeasonNumber { get; set; }
        public string Name { get; set; }
        public DateTime StartAirTime { get; set; }
        public DateTime EndAirTime { get; set; }
        public int EpisodeCount { get; set; }
    }
}
