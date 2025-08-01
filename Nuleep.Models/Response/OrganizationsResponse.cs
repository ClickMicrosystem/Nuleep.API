using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nuleep.Models.Response
{
    public class OrganizationsResponse
    {
        [JsonPropertyName("_id")]
        public int Id { get; set; }

        public string? name { get; set; }

        public List<string>? benefits { get; set; }

        public List<string>? perks { get; set; }

        public bool sendOwenerShip { get; set; } = false;
        public string orgCode { get; set; }

        public string about { get; set; }

        public string streetAddress { get; set; }

        public string countryRegion { get; set; }

        public string stateProvince { get; set; }

        public string zipPostal { get; set; }

        public string city { get; set; }

        public string email { get; set; }

        public string tel { get; set; }

        public string culture { get; set; }

        public string mission { get; set; }

        public string teamSize { get; set; }

        public bool verified { get; set; } = false;

        public MediaImage orgImage { get; set; }
    }



}
