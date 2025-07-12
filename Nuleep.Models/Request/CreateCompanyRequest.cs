using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class CreateCompanyRequest
    {
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAbout { get; set; } = string.Empty;
        public string CompanyCulture { get; set; } = string.Empty;
        public string CompanyMission { get; set; } = string.Empty;
        public List<string> CompanyBenefits { get; set; } = new();
        public List<string> CompanyPerks { get; set; } = new();
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanyStreet { get; set; } = string.Empty;
        public string ZipPostal { get; set; } = string.Empty;
    }
}
