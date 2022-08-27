using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace Home.Agents.Clara.Commute
{
    internal static class TransportDataGouvFrProvider
    {
        public const string KnowUrlTransvilleValenciennes = "https://transport.data.gouv.fr/api/datasets/5e5640aa634f412ab4f8353e";


        public static List<ResourceFile> GetResourceFiles(string feedUrl, string roleNeeded)
        {
            DataSetGouvFeedDesc tmp = null;
            using (HttpClient cli = new HttpClient())
            {
                var res = cli.GetAsync(feedUrl).Result;
                if(res.IsSuccessStatusCode)
                {
                    tmp = res.Content.ReadFromJsonAsync<DataSetGouvFeedDesc>().Result;
                }
                else
                {
                    throw new ApplicationException($"Unable to get dataset from {feedUrl} : err {res.StatusCode}");
                }
            }

            var resourceFiles = (from z in tmp.resources
                          where z.features != null && z.features.Contains(roleNeeded, StringComparer.InvariantCultureIgnoreCase)
                          select new ResourceFile()
                          {
                              Format = z.format,
                              Roles = z.features,
                              Url = z.url,
                              UpdatedAt = z.updated
                          }).ToList();
            return resourceFiles;
        }


        public class ResourceFile
        {
            public string Format { get; set; }
            public string Url { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
            public string[] Roles { get; set; }
        }


        private class DataSetGouvFeedDesc
        {
            public Aom aom { get; set; }
            public object[] community_resources { get; set; }
            public Covered_Area covered_area { get; set; }
            public string created_at { get; set; }
            public string datagouv_id { get; set; }
            public History[] history { get; set; }
            public string id { get; set; }
            public string page_url { get; set; }
            public Publisher publisher { get; set; }
            public Resource[] resources { get; set; }
            public string slug { get; set; }
            public string title { get; set; }
            public string type { get; set; }
            public DateTime updated { get; set; }
        }

        private class Aom
        {
            public string name { get; set; }
            public string siren { get; set; }
        }

        private class Covered_Area
        {
            public Aom aom { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }


        private class Publisher
        {
            public string name { get; set; }
            public string type { get; set; }
        }

        private class History
        {
            public int resource_id { get; set; }
            public Payload payload { get; set; }
            public DateTime? last_up_to_date_at { get; set; }
            public DateTime inserted_at { get; set; }
            public DateTime updated_at { get; set; }
        }

        private class Payload
        {
            public int dataset_id { get; set; }
            public DateTime download_datetime { get; set; }
            public string filename { get; set; }
            public string[] filenames { get; set; }
            public int filesize { get; set; }
            public string format { get; set; }
            public Http_Headers http_headers { get; set; }
            public object latest_schema_version_to_date { get; set; }
            public string permanent_url { get; set; }
            public Resource_Metadata resource_metadata { get; set; }
            public object schema_name { get; set; }
            public object schema_version { get; set; }
            public string title { get; set; }
            public int total_compressed_size { get; set; }
            public int total_uncompressed_size { get; set; }
            public string uuid { get; set; }
            public Zip_Metadata[] zip_metadata { get; set; }
            public bool from_old_system { get; set; }
            public string old_href { get; set; }
            public Old_Payload old_payload { get; set; }
        }

        private class Http_Headers
        {
            public string contentlength { get; set; }
            public string contenttype { get; set; }
            public string etag { get; set; }
            public string lastmodified { get; set; }
        }

        private class Resource_Metadata
        {
            public string end_date { get; set; }
            public bool has_fares { get; set; }
            public bool has_pathways { get; set; }
            public bool has_shapes { get; set; }
            public Issues_Count issues_count { get; set; }
            public int lines_count { get; set; }
            public int lines_with_custom_color_count { get; set; }
            public string[] modes { get; set; }
            public string[] networks { get; set; }
            public bool some_stops_need_phone_agency { get; set; }
            public bool some_stops_need_phone_driver { get; set; }
            public string start_date { get; set; }
            public int stop_areas_count { get; set; }
            public int stop_points_count { get; set; }
            public string validator_version { get; set; }
        }

        private class Issues_Count
        {
            public int DuplicateStops { get; set; }
            public int ExcessiveSpeed { get; set; }
            public int NullDuration { get; set; }
            public int UnusedStop { get; set; }
        }

        private class Old_Payload
        {
            public string dataset_datagouv_id { get; set; }
            public string href { get; set; }
            public bool is_current { get; set; }
            public DateTime last_modified { get; set; }
            public Metadata metadata { get; set; }
            public string name { get; set; }
        }

        private class Metadata
        {
            public string contenthash { get; set; }
            public string end { get; set; }
            public string format { get; set; }
            public string start { get; set; }
            public string title { get; set; }
            public object updatedat { get; set; }
            public string url { get; set; }
        }

        private class Zip_Metadata
        {
            public int compressed_size { get; set; }
            public string file_name { get; set; }
            public DateTime last_modified_datetime { get; set; }
            public string sha256 { get; set; }
            public int uncompressed_size { get; set; }
        }

        private class Resource
        {
            public string content_hash { get; set; }
            public string datagouv_id { get; set; }
            public string end_calendar_validity { get; set; }
            public string[] features { get; set; }
            public string format { get; set; }
            public ResourceMetadata metadata { get; set; }
            public string[] modes { get; set; }
            public string original_url { get; set; }
            public string start_calendar_validity { get; set; }
            public string title { get; set; }
            public string type { get; set; }
            public DateTimeOffset updated { get; set; }
            public string url { get; set; }
        }

        private class ResourceMetadata
        {
            public string end_date { get; set; }
            public bool has_fares { get; set; }
            public bool has_pathways { get; set; }
            public bool has_shapes { get; set; }
            public Issues_Count1 issues_count { get; set; }
            public int lines_count { get; set; }
            public int lines_with_custom_color_count { get; set; }
            public string[] modes { get; set; }
            public string[] networks { get; set; }
            public bool some_stops_need_phone_agency { get; set; }
            public bool some_stops_need_phone_driver { get; set; }
            public string start_date { get; set; }
            public int stop_areas_count { get; set; }
            public int stop_points_count { get; set; }
            public string validator_version { get; set; }
            public Entities_Last_Seen entities_last_seen { get; set; }
            public Validation validation { get; set; }
        }

        private class Issues_Count1
        {
            public int DuplicateStops { get; set; }
            public int ExcessiveSpeed { get; set; }
            public int NullDuration { get; set; }
        }

        private class Entities_Last_Seen
        {
            public string trip_updates { get; set; }
            public string vehicle_positions { get; set; }
            public string service_alerts { get; set; }
        }

        private class Validation
        {
            public string datetime { get; set; }
            public Error[] errors { get; set; }
            public int errors_count { get; set; }
            public Files files { get; set; }
            public bool has_errors { get; set; }
            public string max_severity { get; set; }
            public string uuid { get; set; }
            public string validator { get; set; }
            public int warnings_count { get; set; }
        }

        private class Files
        {
            public string gtfs_permanent_url { get; set; }
            public string gtfs_resource_history_uuid { get; set; }
            public string gtfs_rt_filename { get; set; }
            public string gtfs_rt_permanent_url { get; set; }
        }

        private class Error
        {
            public string description { get; set; }
            public string error_id { get; set; }
            public string[] errors { get; set; }
            public int errors_count { get; set; }
            public string severity { get; set; }
            public string title { get; set; }
        }


    }
}
