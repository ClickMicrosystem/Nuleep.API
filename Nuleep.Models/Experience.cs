namespace Nuleep.Models
{
    public class Experience
    {
        public string Title { get; set; }
        public string Company { get; set; }
        public List<string> Description { get; set; }
        public string Location { get; set; }
        public string DescriptionC { get; set; }
        public List<string> Impact { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool Current { get; set; }
    }

}
