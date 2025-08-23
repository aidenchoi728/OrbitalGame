using UnityEngine;
using XCharts.Runtime;

public static class OrbitalSettings
{
    //Materials
    private static Material phasePositiveMat;
    private static Material phaseNegativeMat;
    private static Material lineMat;       // LineRenderer
    
    //Orbital Mesh 
    private static int gridSize = 45;
    private static float boundaryMargin = 1.1f;
    
    //Angular Node
    private static GameObject nodePrefab;
    private static Material nodeMat;
    
    //Line
    private static float lineWidth = 0.1f;
    private static Color lineColor = Color.white;
    private static int resolution = 1024;
    private static int circleSegments = 128;
    
    //Cross Section
    private static Color psiPositiveColor = new (1f, 0.1803922f, 0.7058824f, 1f);
    private static Color psiNegativeColor = new (0f, 0.6980392f, 1f, 1f);
    private static int crossSectionResolution = 256;
    private static float intensity = 5.0f;
    private static float gamma = 0.8f; // Lower = brighter
    private static float crossSectionMargin = 2f;
    private static GameObject crossSectionQuadPrefab;
    
    //Chart
    private static Color chartLineColor = new (0.227451f, 0.7372549f, 0.9568627f, 1f);
    private static int sampleCount = 1000;
    private static float chartMargin = 1.3f;
    private static float cutYMargin = 0.08f;
    private static float cutXMargin = 0.95f;
    
    //Wave
    private static float length = 120f;     // Total length of the wave
    private static float amplitude = 1f;   // Height of the wave
    private static float period = 20f;      // Distance over which the wave repeats
    private static int waveResolution = 120;   // Number of points in the line
    private static float waveSize = 120f;
    private static float waveLength = 10f;
    private static float waveLength3D = 0.35f;

    //Orbital Info 
    private static GameObject orbitalInfoPrefab;

    public static void Init()
    {
        phasePositiveMat = Resources.Load<Material>("Materials/Positive Boundary Material");
        phaseNegativeMat = Resources.Load<Material>("Materials/Negative Boundary Material");
        nodeMat =  Resources.Load<Material>("Materials/Node Material");
        lineMat = Resources.Load<Material>("Materials/Line Material");
        
        nodePrefab = Resources.Load<GameObject>("Prefabs/Node Prefab");
        orbitalInfoPrefab = Resources.Load<GameObject>("Prefabs/Orbital Info Prefab");
        crossSectionQuadPrefab = Resources.Load<GameObject>("Prefabs/Cross Section Quad Prefab");
    }

    public static Material PhasePositiveMat
    {
        get => phasePositiveMat;
    }

    public static Material PhaseNegativeMat
    {
        get => phaseNegativeMat;
    }

    public static Material LineMat
    {
        get => lineMat;
    }

    public static int GridSize
    {
        get => gridSize;
        set => gridSize = value;
    }

    public static float BoundaryMargin
    {
        get => boundaryMargin;
    }

    public static GameObject NodePrefab
    {
        get => nodePrefab;
    }

    public static Material NodeMat
    {
        get => nodeMat;
    }

    public static float LineWidth
    {
        get => lineWidth;
    }

    public static Color LineColor
    {
        get => lineColor;
    }

    public static int Resolution
    {
        get => resolution;
    }

    public static int CircleSegments
    {
        get => circleSegments;
    }

    public static Color PsiPositiveColor
    {
        get => psiPositiveColor;
    }

    public static Color PsiNegativeColor
    {
        get => psiNegativeColor;
    }

    public static int CrossSectionResolution
    {
        get => crossSectionResolution;
    }

    public static float Intensity
    {
        get => intensity;
    }

    public static float Gamma
    {
        get => gamma;
    }

    public static float CrossSectionMargin
    {
        get => crossSectionMargin;
    }

    public static GameObject CrossSectionQuadPrefab
    {
        get => crossSectionQuadPrefab;
    }

    public static Color ChartLineColor
    {
        get => chartLineColor;
    }

    public static int SampleCount
    {
        get => sampleCount;
    }

    public static float ChartMargin
    {
        get => chartMargin;
    }

    public static float CutYMargin
    {
        get => cutYMargin;
    }

    public static float CutXMargin
    {
        get => cutXMargin;
    }

    public static float Length
    {
        get => length;
    }

    public static float Amplitude
    {
        get => amplitude;
    }

    public static float Period
    {
        get => period;
    }

    public static int WaveResolution
    {
        get => waveResolution;
    }

    public static float WaveSize
    {
        get => waveSize;
    }

    public static float WaveLength
    {
        get => waveLength;
    }

    public static float WaveLength3D
    {
        get => waveLength3D;
    }

    public static GameObject OrbitalInfoPrefab
    {
        get => orbitalInfoPrefab;
    }
}