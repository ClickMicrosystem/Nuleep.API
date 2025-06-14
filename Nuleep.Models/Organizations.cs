﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class Organization
    {
        [JsonPropertyName("_id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        [Required, MaxLength(255)]
        public string? Name { get; set; }

        public List<string>? Benefits { get; set; }

        public List<string>? Perks { get; set; }

        public bool SendOwnership { get; set; } = false;
        public string OrgCode { get; set; }

        public string About { get; set; }

        public string StreetAddress { get; set; }

        public string CountryRegion { get; set; }

        public string StateProvince { get; set; }

        public string ZipPostal { get; set; }

        public string City { get; set; }

        [JsonPropertyName("email")]
        [Required, MaxLength(255), EmailAddress]
        public string Email { get; set; }

        public string Tel { get; set; }

        public string Culture { get; set; }

        public string Mission { get; set; }

        public string TeamSize { get; set; }

        public bool Verified { get; set; } = false;


        public string OrgImageFileName { get; set; }

        public string OrgImageBlobName { get; set; }

        public string OrgImageFullUrl { get; set; }
    }



}
