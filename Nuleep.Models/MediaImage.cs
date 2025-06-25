using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class MediaImage
    {
        [JsonPropertyName("_id")]
        public int? Id { get; set; }

        public int? ProfileId { get; set; }

        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("blobName")]
        public string? BlobName { get; set; }

        [JsonPropertyName("fullUrl")]
        public string? FullUrl { get; set; }
    }

}
