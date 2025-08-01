using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Nuleep.Models.Request
{

    public class GetBlogByIdRequest
    {
        [JsonPropertyName("_id")]
        public int Id { get; set; }
    }

}
