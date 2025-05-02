using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class ProfileImage
    {
        [JsonPropertyName("_id")]
        public int Id { get; set; }

        public int ProfileId { get; set; }

        public string FileName { get; set; }

        public string BlobName { get; set; }

        public string FullUrl { get; set; }
    }

}
