public class LocationData
{
    public string id { get; set; }
    public string name { get; set; }
    public string background { get; set; }
    public string dialogue { get; set; }
    public string[] exits { get; set; }
    public bool dialoguePlayed { get; set; } = false;
    public string[] inspectables { get; set; }
    public string backgroundAfterPickup { get; set; }
}
