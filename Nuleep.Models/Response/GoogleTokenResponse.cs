using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nuleep.Models.Response
{
    public class GoogleTokenResponse
    {
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }


}
