public class IntroData
{
    public IntroSlide[] slides { get; set; }
}

public class IntroSlide
{
    public string text { get; set; }
    public string image { get; set; }
    public bool fadeOut { get; set; }
    public bool fadeNoise { get; set; }
    public string sfx { get; set; }
    public bool fadeMusic { get; set; }
    public bool stopNoise { get; set; }
}
