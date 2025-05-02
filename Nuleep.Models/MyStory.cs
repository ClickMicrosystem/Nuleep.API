namespace Nuleep.Models
{
    public class MyStory
    {
        public string Header { get; set; }
        public string Summary { get; set; }
        public List<Activity> Activities { get; set; } = new();
    }

}
