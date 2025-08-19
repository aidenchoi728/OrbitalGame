public static class Settings
{
    private static bool axes = true;
    private static bool nucleus = true;
    private static float bgm = 0.5f;
    private static float sfx = 0.5f;
    
    public static bool Axes { get => axes; set => axes = value; }
    public static bool Nucleus { get => nucleus; set => nucleus = value; }
    public static float Bgm { get => bgm; set => bgm = value; }
    public static float Sfx { get => sfx; set => sfx = value; }
}