using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CloudflareDNS
{
    public class CloudflareRecord
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("proxied")]
        public bool Proxied { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

    }

    public class CloudflareMessage
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class CloudflareAPI
    {
        private readonly string _apiToken;
        private readonly HttpClient _httpClient;

        public CloudflareAPI(string apiToken)
        {
            _apiToken = apiToken;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(@"https://api.cloudflare.com/client/v4/"),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _apiToken),
                    Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") },
                    UserAgent = { ProductInfoHeaderValue.Parse("AppName/1.0") }
                }
            };
        }

        public async Task<List<CloudflareRecord>> GetDnsRecordsAsync(string zoneId)
        {
            string url = $"zones/{zoneId}/dns_records";
            string response = await _httpClient.GetStringAsync(url);

            CloudflareApiDNSResponse result = JsonSerializer.Deserialize<CloudflareApiDNSResponse>(response);

            return result?.Result ?? new List<CloudflareRecord>();
        }

        public async Task<bool> ValidateTokenAsync()
        {
            try
            {
                string url = $"user/tokens/verify";
                string response = await _httpClient.GetStringAsync(url);

                CloudflareApiResponse result = JsonSerializer.Deserialize<CloudflareApiResponse>(response);

                return result.Result.Status == "active";
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<bool> ValidateZoneIDAsync(string zoneId)
        {
            try
            {
                string url = $"zones/{zoneId}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                return response.IsSuccessStatusCode;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<bool> ValidateRecordIDAsync(string zoneId, string recordId)
        {
            try
            {
                //Override record id if it's empty.
                if(recordId == "")
                {
                    recordId = "null";
                }

                string url = $"zones/{zoneId}/dns_records/{recordId}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                return response.IsSuccessStatusCode;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<HttpResponseMessage> UpdateRecordAsync(string zoneId, string recordId, IPAddress publicIP)
        {
            try
            {
                string url = $"zones/{zoneId}/dns_records/{recordId}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                //Get record.
                string jsonResponse = await response.Content.ReadAsStringAsync();
                CloudflareApiDNSResponseSingleRecord record = JsonSerializer.Deserialize<CloudflareApiDNSResponseSingleRecord>(jsonResponse);

                //Update record.
                record.Result.Content = publicIP.ToString();
                record.Result.Comment = "Updated by PIN0L33KZ on: " + DateTime.Now;
                string jsonRequest = JsonSerializer.Serialize<CloudflareRecord>(record.Result);
                response = await _httpClient.PutAsync(url, new StringContent(jsonRequest, Encoding.Default, "application/json"));

                return response;
            }
            catch(Exception)
            {
                return null;
            }
        }

        public class CloudflareApiDNSResponse
        {
            [JsonPropertyName("result")]
            public List<CloudflareRecord> Result { get; set; }
        }

        public class CloudflareApiDNSResponseSingleRecord
        {
            [JsonPropertyName("result")]
            public CloudflareRecord Result { get; set; }
        }

        public class CloudflareApiResponse
        {
            [JsonPropertyName("result")]
            public CloudflareMessage Result { get; set; }
        }
    }
}