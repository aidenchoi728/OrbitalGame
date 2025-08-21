using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using XCharts.Runtime;
using Slider = UnityEngine.UI.Slider;

public enum Plane { XY, YZ, XZ }

public class OrbitalManager : MonoBehaviour
{
    [SerializeField] private Slider transitionSlider;
    
    [Header("Chart")]
    [SerializeField] private LineChart mainChart; // Assign this in the Inspector
    [SerializeField] private LineChart psiChart;
    [SerializeField] private LineChart psiSqChart;
    [SerializeField] private LineChart psiSqRSqChart;
    [SerializeField] private RectTransform[] chartRectTransforms;
    [SerializeField] private RectTransform[] tooltips;

    [Header("Orbital Info")] 
    [SerializeField] private GameObject orbitalInfoPanel;
    [SerializeField] RectTransform layoutRoot; // The object with VerticalLayoutGroup (or its parent)
    //------------------//
    
    //Materials
    private Material phasePositiveMat;
    private Material phaseNegativeMat;
    private Material lineMat;       // LineRenderer
    
    //Orbital Mesh 
    private int gridSize;
    private float boundaryMargin;
    
    //Angular Node
    private GameObject nodePrefab;
    private Material nodeMat;
    
    //Line
    private float lineWidth;
    private Color lineColor;
    private int resolution;
    private int circleSegments;
    
    //Cross-Section
    private Color psiPositiveColor;
    private Color psiNegativeColor;
    private int crossSectionResolution;
    private float intensity;
    private float gamma; // Lower = brighter
    private float crossSectionMargin;
    private GameObject crossSectionQuadPrefab;
    
    //Chart
    private Color chartLineColor;
    private int sampleCount;
    private float chartMargin;
    private float cutYMargin;
    private float cutXMargin;
    
    //Wave
    private float length;     // Total length of the wave
    private float amplitude;   // Height of the wave
    private float period;      // Distance over which the wave repeats
    private int waveResolution;   // Number of points in the line
    private float waveSize;
    private float waveLength;
    private float waveLength3D;

    //Orbital Info 
    private  GameObject orbitalInfoPrefab;
    
    //------------------//
    private Vector2 lastMousePos;
    private bool wasInsideLastFrame;
    private Camera mainCamera;
    
    private List<GameObject> activeOverlaps = new List<GameObject>();
    private List<GameObject> activeRadialNodes = new List<GameObject>();
    private List<GameObject> activeAngularNodes = new List<GameObject>();
    private GameObject[] activeCrossSections = new GameObject[3];
    private List<GameObject>[] activeCSBoundaries = {new List<GameObject>(), new List<GameObject>(), new List<GameObject>()};
    private List<GameObject>[] activeCSRadialNodes = {new List<GameObject>(), new List<GameObject>(), new List<GameObject>()};
    private List<GameObject>[] activeCSAngularNodes = {new List<GameObject>(), new List<GameObject>(), new List<GameObject>()};
    private List<GameObject> activeOrbitalInfo = new List<GameObject>();
    private GameObject wave1DObject;
    private GameObject wave2DObject; // store the reference
    private GameObject prevRVisualizer = null;
    
    private float[,,] orbitalPrev;
    private float[,,] orbitalNew;
    private float thresholdPrev;
    private float thresholdNew;
    private float boxExtent;
    private float maxRadius = 0f;
    private bool isBillBoard = false;
    private bool isChart = false;
    private Plane rVisualizerPlane;

    private void Awake()
    {
        mainCamera = Camera.main;
        
        Init();
    }

    private void Init()
    {
        OrbitalSettings.Init();
        
        //Materials
        phasePositiveMat = OrbitalSettings.PhasePositiveMat;
        phaseNegativeMat = OrbitalSettings.PhaseNegativeMat;
        
        //Orbital Mesh 
        gridSize = OrbitalSettings.GridSize;
        boundaryMargin = OrbitalSettings.BoundaryMargin;
        
        //Angular Node
        nodePrefab = OrbitalSettings.NodePrefab;
        nodeMat = OrbitalSettings.NodeMat;
        
        //Line
        lineMat = OrbitalSettings.LineMat;
        lineWidth = OrbitalSettings.LineWidth;
        lineColor = OrbitalSettings.LineColor;
        resolution = OrbitalSettings.Resolution;
        circleSegments = OrbitalSettings.CircleSegments;
        
        //Cross-Section
        psiPositiveColor = OrbitalSettings.PsiPositiveColor;
        psiNegativeColor = OrbitalSettings.PsiNegativeColor;
        crossSectionResolution = OrbitalSettings.CrossSectionResolution;
        intensity = OrbitalSettings.Intensity;
        gamma = OrbitalSettings.Gamma;
        crossSectionMargin = OrbitalSettings.CrossSectionMargin;
        crossSectionQuadPrefab = OrbitalSettings.CrossSectionQuadPrefab;
        
        //Chart
        chartLineColor = OrbitalSettings.ChartLineColor;
        sampleCount = OrbitalSettings.SampleCount;
        chartMargin = OrbitalSettings.ChartMargin;
        cutYMargin = OrbitalSettings.CutYMargin;
        cutXMargin = OrbitalSettings.CutXMargin;
        
        //Wave
        length = OrbitalSettings.Length;
        amplitude = OrbitalSettings.Amplitude;
        period = OrbitalSettings.Period;
        waveResolution = OrbitalSettings.WaveResolution;
        waveSize = OrbitalSettings.WaveSize;
        waveLength = OrbitalSettings.WaveLength;
        waveLength3D = OrbitalSettings.WaveLength3D;

        //Orbital Info 
        orbitalInfoPrefab = OrbitalSettings.OrbitalInfoPrefab;
    }

    public void DestroyAll()
    {
        // Remove the mesh from the MeshFilter
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null && mf.mesh != null)
        {
            // Destroy the mesh asset to free memory
            Destroy(mf.mesh);
            mf.mesh = null;
        }
        
        for(int i = activeOverlaps.Count - 1; i >= 0; i--)
            if (activeOverlaps[i] != null)
            {
                Destroy(activeOverlaps[i]);
                activeOverlaps.RemoveAt(i);
            }
        
        for (int i = activeRadialNodes.Count - 1; i >= 0; i--)
            if (activeRadialNodes[i] != null)
            {
                Destroy(activeRadialNodes[i]);
                activeRadialNodes.RemoveAt(i);
            }
        
        for (int i = activeAngularNodes.Count - 1; i >= 0; i--)
            if (activeAngularNodes[i] != null)
            {
                Destroy(activeAngularNodes[i]);
                activeAngularNodes.RemoveAt(i);
            }

        foreach (GameObject cs in activeCrossSections) Destroy(cs);
        
        for (int i = 0; i < 3; i++) for(int j = activeCSBoundaries[i].Count - 1; j >= 0; j--)
            if (activeCSBoundaries[i][j] != null)
            {
                Destroy(activeCSBoundaries[i][j]);
                activeCSBoundaries[i].RemoveAt(j);
            }
        
        for(int i = 0; i < 3; i++) for (int j = activeCSRadialNodes[i].Count - 1; j >= 0; j--)
            if (activeCSRadialNodes[i][j] != null)
            {
                Destroy(activeCSRadialNodes[i][j]);
                activeCSRadialNodes[i].RemoveAt(j);
            }
        
        for(int i = 0; i < 3; i++) for (int j = activeCSAngularNodes[i].Count - 1; j >= 0; j--)
            if (activeCSAngularNodes[i][j] != null)
            {
                Destroy(activeCSAngularNodes[i][j]);
                activeCSAngularNodes[i].RemoveAt(j);
            }

        for(int i = activeOrbitalInfo.Count - 1; i >= 0; i--)
            if (activeOrbitalInfo[i] != null)
            {
                Destroy(activeOrbitalInfo[i]);
                activeOrbitalInfo.RemoveAt(i);
            }
        
        isChart = false;
        
        // If it exists, destroy it
        if (wave1DObject != null)
        {
            Destroy(wave1DObject);
            wave1DObject = null;
        }
        
        if (wave2DObject != null)
        {
            Destroy(wave2DObject);
            wave2DObject = null; // clear reference
        }
        
        // Remove all child objects (the spheres)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void Update()
    {
        if(isChart) ChartRVisualizer();
    }

    public void CreateOrbitalInfo(int n, int l, int ml)
    {
        GameObject orbitalInfo = Instantiate(orbitalInfoPrefab, orbitalInfoPanel.transform);
        orbitalInfo.transform.SetSiblingIndex(orbitalInfoPanel.transform.childCount - 2);
        activeOrbitalInfo.Add(orbitalInfo);
        UpdateOrbitalInfo(n, l, ml, activeOrbitalInfo.Count - 1);
    }
    
    public void Orbital(int n, int l, int ml, bool isOverlap)
    {
        GameObject orbitalInfo = Instantiate(orbitalInfoPrefab, orbitalInfoPanel.transform);
        orbitalInfo.transform.SetSiblingIndex(orbitalInfoPanel.transform.childCount - 2);
        activeOrbitalInfo.Add(orbitalInfo);
        UpdateOrbitalInfo(n, l, ml, activeOrbitalInfo.Count - 1);
        
        // 1. Compute threshold and cutoff radius (physical size of the orbital)
        (float threshold, float rCutoff) = ComputePsiCutoff(n, l, ml);

        // 2. Physical box size for this orbital
        rCutoff *= boundaryMargin;
        float[,,] orbitalMap = new float[gridSize, gridSize, gridSize];
        float step = (2f * rCutoff) / (gridSize - 1);

        // 3. Fill grid as usual
        for (int x = 0; x < gridSize; x++)
        for (int y = 0; y < gridSize; y++)
        for (int z = 0; z < gridSize; z++) 
            orbitalMap[x, y, z] = GetPsi(n, l, ml, x * step - rCutoff, y * step - rCutoff, z * step - rCutoff);

        CreateAndApplyMesh(orbitalMap, threshold, isOverlap);

        // 4. Make the mesh's physical size independent of gridSize:
        //    The mesh vertices span 0..gridSize-1, so we normalize the scale.
        // This ensures physical size is always 2*rCutoff*scale
        
        if(isOverlap) activeOverlaps[activeOverlaps.Count - 1].transform.localScale = Vector3.one * (2f * rCutoff / gridSize);
        else transform.localScale = Vector3.one * (2f * rCutoff / gridSize);
    }
    
    public void DestroyOrbital()
    {
        // Remove the mesh from the MeshFilter
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null && mf.mesh != null)
        {
            // Destroy the mesh asset to free memory
            Destroy(mf.mesh);
            mf.mesh = null;
        }
        
        for(int i = activeOverlaps.Count - 1; i >= 0; i--) if(activeOverlaps[i] != null) Destroy(activeOverlaps[i]);
        
        for(int i = activeOrbitalInfo.Count - 1; i >= 0; i--) if(activeOrbitalInfo[i] != null) Destroy(activeOrbitalInfo[i]);
    }

    public void TransitionOrbital(int nPrev, int nNew, int lPrev, int lNew, int mlPrev, int mlNew)
    {
        transitionSlider.value = 0;
        
        float rCutoffPrev, rCutoffNew;
        (thresholdPrev, rCutoffPrev) = ComputePsiCutoff(nPrev, lPrev, mlPrev);
        (thresholdNew, rCutoffNew) = ComputePsiCutoff(nNew, lNew, mlNew);

        // Find which is larger
        bool prevIsLarge = rCutoffPrev >= rCutoffNew;
        // Use the large orbital's box for both
        boxExtent = Mathf.Max(rCutoffPrev * boundaryMargin, rCutoffNew * boundaryMargin);

        orbitalPrev = new float[gridSize, gridSize, gridSize];
        orbitalNew  = new float[gridSize, gridSize, gridSize];

        float step = (2f * boxExtent) / (gridSize - 1);

        // Both orbitals are sampled in the *same* large box, with no scaling.
        for (int x = 0; x < gridSize; x++)
        for (int y = 0; y < gridSize; y++)
        for (int z = 0; z < gridSize; z++)
        {
            float physX = x * step - boxExtent;
            float physY = y * step - boxExtent;
            float physZ = z * step - boxExtent;

            if (prevIsLarge)
            {
                // prev is large, new is small
                orbitalPrev[x, y, z] = GetPsi(nPrev, lPrev, mlPrev, physX, physY, physZ);
                orbitalNew[x, y, z]  = GetPsi(nNew,  lNew,  mlNew,  physX, physY, physZ);
            }
            else
            {
                // new is large, prev is small
                orbitalPrev[x, y, z] = GetPsi(nPrev, lPrev, mlPrev, physX, physY, physZ);
                orbitalNew[x, y, z]  = GetPsi(nNew,  lNew,  mlNew,  physX, physY, physZ);
            }
            // Note: Both use the same grid/box, so the small orbital appears as a small blob in the large box!
        }
    }
    
    public void TransitionAnimation()
    {
        float t = transitionSlider.value;

        // Allocate result
        float[,,] orbitalMap = new float[gridSize, gridSize, gridSize];

        // Blend maps
        for (int x = 0; x < gridSize; x++)
        for (int y = 0; y < gridSize; y++)
        for (int z = 0; z < gridSize; z++)
        {
            orbitalMap[x, y, z] = Mathf.Lerp(orbitalPrev[x, y, z], orbitalNew[x, y, z], t);
        }

        // Blend thresholds
        float threshold = Mathf.Lerp(thresholdPrev, thresholdNew, t);

        // Generate mesh
        CreateAndApplyMesh(orbitalMap, threshold, false);

        // Set the physical scale (independent of grid size)
        transform.localScale = Vector3.one * (2f * boxExtent / gridSize);
    }

    public void RadialNode(int n, int l, int ml, float dr = 0.01f)
    {
        List<float> nodes = new List<float>();

        (_, float rMax) = ComputePsiCutoff(n, l, ml);
        rMax *= boundaryMargin;

        Func<float, (float, float, float)> radialVec = r =>
        {
            if (l == 1)
            {
                if (ml == 0) return (0, 0, r);
                if (ml == 1) return (r, 0, 0);
                if (ml == -1) return (0, r, 0);
            }
            return (r, 0, 0);
        };

        float prevPsi = GetPsi(n, l, ml, radialVec(0).Item1, radialVec(0).Item2, radialVec(0).Item3);

        for (float r = dr; r <= rMax && nodes.Count < n - l - 1; r += dr)
        {
            var vec = radialVec(r);
            float psi = GetPsi(n, l, ml, vec.Item1, vec.Item2, vec.Item3);

            if (prevPsi * psi < 0) nodes.Add(r - dr / 2);
            prevPsi = psi;
            r += 0.5f;
        }

        foreach (float rNode in nodes)
        {
            GameObject go = new GameObject("Radial Node");
            GameObject ring = DrawCircle(rNode, Plane.XY, "Radial Node Circle");
            ring.transform.parent = go.transform;  // <-- make it a child!

            go.AddComponent<SimpleBillboard>();
            activeRadialNodes.Add(go);
        }
    }

    public void DestroyRadialNode()
    {
        for (int i = activeRadialNodes.Count - 1; i >= 0; i--)
            if (activeRadialNodes[i] != null)
            {
                Destroy(activeRadialNodes[i]);
                activeRadialNodes.RemoveAt(i);
            }
    }

    public void AngularNode(int n, int l, int ml)
    {
        switch (l)
        {
            case 1:
                switch (ml)
                {
                    case -1:
                        activeAngularNodes.Add(Instantiate(nodePrefab));
                        break;
                    case 0:
                        GameObject xy = Instantiate(nodePrefab);
                        xy.GetComponent<Transform>().rotation = Quaternion.Euler(90f, 0f, 0f);
                        activeAngularNodes.Add(xy);
                        break;
                    case 1:
                        GameObject yz = Instantiate(nodePrefab);
                        yz.GetComponent<Transform>().rotation = Quaternion.Euler(0f, 0f, 90f);
                        activeAngularNodes.Add(yz);
                        break;
                }
                break;
            case 2:
                switch (ml)
                {
                    case -2:
                        activeAngularNodes.Add(Instantiate(nodePrefab));
                        GameObject yz = Instantiate(nodePrefab);
                        yz.GetComponent<Transform>().rotation = Quaternion.Euler(0f, 0f, 90f);
                        activeAngularNodes.Add(yz);
                        break;
                    case -1:
                        activeAngularNodes.Add(Instantiate(nodePrefab));
                        GameObject xy = Instantiate(nodePrefab);
                        xy.GetComponent<Transform>().rotation = Quaternion.Euler(90f, 0f, 0f);
                        activeAngularNodes.Add(xy);
                        break;
                    case 0:
                        int psiResolution = 100;
                        float coneHeight = 8f;
                        float bestAngle = 0f;
                        float prevPsi = 0f;

                        // Find angle θ (from z-axis) where Psi is closest to zero
                        for (int i = 0; i <= psiResolution; i++)
                        {
                            float theta = Mathf.PI * i / psiResolution;
                            float psi = GetPsi(n, l, ml, Mathf.Sin(theta), 0f, Mathf.Cos(theta));

                            if (prevPsi * psi < 0)
                            {
                                bestAngle = theta;
                                break;
                            }
                            prevPsi = psi;
                        }

                        float bestRadius = coneHeight * Mathf.Tan(bestAngle);
                        int segments = 64;

                        for (int j = 0; j < 2; j++)
                        {
                            GameObject cone = new GameObject("Angular Node");
                            MeshFilter mf = cone.AddComponent<MeshFilter>();
                            MeshRenderer mr = cone.AddComponent<MeshRenderer>();
                            Mesh mesh = new Mesh();

                            Vector3[] vertices = new Vector3[segments + 2];
                            int[] triangles = new int[segments * 3];

                            // Vertex at origin (tip of the cone)
                            vertices[0] = Vector3.zero;

                            // Base circle points along +Z or -Z
                            Vector3 baseCenter = Vector3.forward * coneHeight * (j == 0 ? 1f : -1f);

                            for (int i = 0; i <= segments; i++)
                            {
                                float angle = 2 * Mathf.PI * i / segments;
                                float x = bestRadius * Mathf.Cos(angle);
                                float y = bestRadius * Mathf.Sin(angle);
                                vertices[i + 1] = baseCenter + new Vector3(x, y, 0);
                            }

                            for (int i = 0; i < segments; i++)
                            {
                                triangles[i * 3] = 0;
                                triangles[i * 3 + 1] = i + 1;
                                triangles[i * 3 + 2] = (i + 2 > segments) ? 1 : i + 2;
                            }

                            mesh.vertices = vertices;
                            mesh.triangles = triangles;
                            mesh.RecalculateNormals();
                            mesh.RecalculateBounds();

                            mf.mesh = mesh;
                            mr.material = nodeMat;

                            cone.transform.position = Vector3.zero;
                            cone.transform.rotation = Quaternion.identity;

                            activeAngularNodes.Add(cone);
                        }

                        break;
                    case 1:
                        GameObject yx = Instantiate(nodePrefab);
                        yx.GetComponent<Transform>().rotation = Quaternion.Euler(90f, 0f, 0f);
                        activeAngularNodes.Add(yx);
                        GameObject zy = Instantiate(nodePrefab);
                        zy.GetComponent<Transform>().rotation = Quaternion.Euler(0f, 0f, 90f);
                        activeAngularNodes.Add(zy);
                        break;
                    case 2:
                        GameObject x2y2 = Instantiate(nodePrefab);
                        x2y2.GetComponent<Transform>().rotation = Quaternion.Euler(0f, 0f, 45f);
                        activeAngularNodes.Add(x2y2);
                        GameObject y2x2 = Instantiate(nodePrefab);
                        y2x2.GetComponent<Transform>().rotation = Quaternion.Euler(0f, 0f, -45f);
                        activeAngularNodes.Add(y2x2);
                        break;
                }
                break;
        }
        

        foreach (GameObject node in activeAngularNodes)
            node.GetComponent<MeshRenderer>().material = nodeMat;
    }

    public void DestroyAngularNode()
    {
        for (int i = activeAngularNodes.Count - 1; i >= 0; i--)
            if (activeAngularNodes[i] != null)
            {
                Destroy(activeAngularNodes[i]);
                activeAngularNodes.RemoveAt(i);
            }
    }
    
    public void CrossSection(int n, int l, int ml, Plane plane)
    {
        rVisualizerPlane = plane;
        
        GameObject orbitalInfo = Instantiate(orbitalInfoPrefab, orbitalInfoPanel.transform);
        orbitalInfo.transform.SetSiblingIndex(orbitalInfoPanel.transform.childCount - 2);
        activeOrbitalInfo.Add(orbitalInfo);
        UpdateOrbitalInfo(n, l, ml, activeOrbitalInfo.Count - 1);
        
        GameObject quad = Instantiate(crossSectionQuadPrefab);
        
        switch (plane)
        {
            case Plane.XY:
                activeCrossSections[0] = quad;
                quad.transform.localRotation = Quaternion.identity;
                break;
            case Plane.XZ:
                activeCrossSections[1] = quad;
                quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                break;
            case Plane.YZ:
                activeCrossSections[2] = quad;
                quad.transform.localRotation = Quaternion.Euler(0f, 90, 90f);
                break;
        }
        
        Texture2D tex = new Texture2D(crossSectionResolution, crossSectionResolution, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        (_, float rCutoff) = ComputePsiCutoff(n, l, ml);
        rCutoff *= crossSectionMargin;

        // Helper for grid-to-world mapping
        Vector3 GetWorldPos(int x, int y)
        {
            float u = Mathf.Lerp(-rCutoff, rCutoff, (float)x / (crossSectionResolution - 1));
            float v = Mathf.Lerp(-rCutoff, rCutoff, (float)y / (crossSectionResolution - 1));
            switch (plane)
            {
                case Plane.XY: return new Vector3(u, v, 0f);
                case Plane.YZ: return new Vector3(0f, u, v);
                case Plane.XZ: return new Vector3(u, 0f, v);
                default: return Vector3.zero;
            }
        }

        float maxPsi2 = 0f;
        float[,] psiGrid = new float[crossSectionResolution, crossSectionResolution];

        // 1st pass: compute psi, psi^2 and cache for color
        for (int x = 0; x < crossSectionResolution; x++)
        for (int y = 0; y < crossSectionResolution; y++)
        {
            Vector3 pos = GetWorldPos(x, y);
            float psi = GetPsi(n, l, ml, pos.x, pos.y, pos.z);
            psiGrid[x, y] = psi;
            if (psiGrid[x, y] * psiGrid[x, y] > maxPsi2) maxPsi2 = psiGrid[x, y] * psiGrid[x, y];
        }

        // 2nd pass: assign pixels, normalize and tint
        for (int x = 0; x < crossSectionResolution; x++)
        for (int y = 0; y < crossSectionResolution; y++)
        {
            float adjusted = Mathf.Pow((maxPsi2 > 0f) ? psiGrid[x, y] * psiGrid[x, y] / maxPsi2 : 0f, gamma) * intensity;
            float finalIntensity = Mathf.Clamp01(adjusted);

            Color c = (psiGrid[x, y] >= 0f)
                ? new Color(psiPositiveColor.r, psiPositiveColor.g, psiPositiveColor.b, finalIntensity)
                : new Color(psiNegativeColor.r, psiNegativeColor.g, psiNegativeColor.b, finalIntensity);

            tex.SetPixel(x, y, c);
        }

        tex.Apply();

        // Assign to quad’s material
        quad.GetComponent<MeshRenderer>().material.mainTexture = tex;

        // Unity quad is 1x1 unit, so scale to 2*rCutoff
        quad.transform.localScale = new Vector3(2 * rCutoff, 2 * rCutoff, 1);
    }

    public void DestroyCrossSection(bool isALl, int num = 0)
    {
        if (isALl) foreach (GameObject cs in activeCrossSections) if(cs != null) Destroy(cs);
        else Destroy(activeCrossSections[num]);
        
        for(int i = activeOrbitalInfo.Count - 1; i >= 0; i--) if(activeOrbitalInfo[i] != null) Destroy(activeOrbitalInfo[i]);
    }
    
    public void CrossSectionBoundary(int n, int l, int ml, Plane plane)
    {
        (float threshold, float rCutoff) = ComputePsiCutoff(n, l, ml);
        rCutoff *= crossSectionMargin;

        Vector3 right = Vector3.right;
        Vector3 up = Vector3.up;

        switch (plane)
        {
            case Plane.XY: break;
            case Plane.XZ:
                up = Vector3.forward;
                break;
            case Plane.YZ:
                right = Vector3.forward;
                break;
        }

        float[,] values = new float[resolution, resolution];

        for (int y = 0; y < resolution; y++)
        for (int x = 0; x < resolution; x++)
        {
            Vector3 worldPos = Vector3.zero + Mathf.Lerp(-rCutoff, rCutoff, (float)x / (resolution - 1)) * right 
                                            + Mathf.Lerp(-rCutoff, rCutoff, (float)y / (resolution - 1)) * up;
            float psi = GetPsi(n, l, ml, worldPos.x, worldPos.y, worldPos.z);
            values[x, y] = psi * psi;
        }

        List<Vector3> segments = GenerateIsolineSegments(values, threshold, Vector3.zero, right, up, rCutoff);
        
        int num = 0;
        
        switch (plane)
        {
            case Plane.XY:
                num = 0; 
                break;
            case Plane.XZ:
                num = 1; 
                break;
            case Plane.YZ: 
                num = 2; 
                break;
        }
        
        for (int i = 0; i < segments.Count; i += 2)
        {
            GameObject go = new GameObject($"Boundary Segment {i / 2}");
            activeCSBoundaries[num].Add(go);
            
            var lr = go.AddComponent<LineRenderer>();

            lr.positionCount = 2;
            lr.SetPosition(0, segments[i]);
            lr.SetPosition(1, segments[i + 1]);

            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.material = lineMat;
            lr.material.color = lineColor;
            lr.useWorldSpace = true;
            lr.loop = false;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.numCapVertices = 4; // Optional: makes line ends smoother
        }
    }

    public void DestroyCSBoundary(bool isAll, int num = 0)
    {
        if (isAll)
        {
            for (int i = 0; i < 3; i++)
            for (int j = activeCSBoundaries[i].Count - 1; j >= 0; j--)
                if (activeCSBoundaries[i][j] != null)
                {
                    Destroy(activeCSBoundaries[i][j]);
                    activeCSBoundaries[i].RemoveAt(j);
                }
        }
        else for(int i = activeCSBoundaries[num].Count - 1; i >= 0; i--)
            if (activeCSBoundaries[num][i] != null)
            {
                Destroy(activeCSBoundaries[num][i]);
                activeCSBoundaries[num].RemoveAt(i);
            }
    }
    
    public void CrossSectionRadialNode(int n, int l, int ml, Plane plane, float rMax = 10f, float dr = 0.01f)
    {
        List<float> nodes = new List<float>();

        Func<float, (float, float, float)> radialVec = r =>
        {
            if (l == 1)
            {
                if (ml == 0) return (0, 0, r);
                if (ml == 1) return (r, 0, 0);
                if (ml == -1) return (0, r, 0);
            }
            return (r, 0, 0);
        };

        float prevPsi = GetPsi(n, l, ml, radialVec(0).Item1, radialVec(0).Item2, radialVec(0).Item3);

        for (float r = dr; r <= rMax && nodes.Count < n - l - 1; r += dr)
        {
            var vec = radialVec(r);
            float psi = GetPsi(n, l, ml, vec.Item1, vec.Item2, vec.Item3);

            if (prevPsi * psi < 0)
            {
                float nodeR = r - dr / 2;
                nodes.Add(nodeR);
                r += 0.5f;
            }
            prevPsi = psi;
        }

        int num = 0;
        switch (plane)
        {
            case Plane.XY:
                num = 0;
                break;
            case Plane.XZ:
                num = 1;
                break;
            case Plane.YZ:
                num = 2;
                break;
        }

        foreach (float rNode in nodes)
        {
            GameObject go = DrawCircle(rNode, plane, "Cross Section Radial Node");
            activeCSRadialNodes[num].Add(go);
        }
    }
    
    public void DestroyCSRadialNode(bool isAll, int num = 0)
    {
        if (isAll)
        {
            for (int i = 0; i < 3; i++)
            for (int j = activeCSRadialNodes[i].Count - 1; j >= 0; j--)
                if (activeCSRadialNodes[i][j] != null)
                {
                    Destroy(activeCSRadialNodes[i][j]);
                    activeCSRadialNodes[i].RemoveAt(j);
                }
        }
        else for (int i = activeCSRadialNodes[num].Count - 1; i >= 0; i--)
            if (activeCSRadialNodes[num][i] != null)
            {
                Destroy(activeCSRadialNodes[num][i]);
                activeCSRadialNodes[num].RemoveAt(i);
            }
    }
    
    public void CrossSectionAngularNode(int n, int l, int ml, Plane plane)
    {
        (_, float lineLength) = ComputePsiCutoff(n, l, ml);

        List<float> detectedAngles = new();

        for (int i = 0; i < 360; i++)
        {
            float angle = i * Mathf.Deg2Rad;
            float x = 0, y = 0, z = 0;

            switch (plane)
            {
                case Plane.XY:
                    x = Mathf.Cos(angle);
                    y = Mathf.Sin(angle);
                    break;
                case Plane.YZ:
                    y = Mathf.Cos(angle);
                    z = Mathf.Sin(angle);
                    break;
                case Plane.XZ:
                    x = Mathf.Cos(angle);
                    z = Mathf.Sin(angle);
                    break;
            }

            float psi = Mathf.Abs(GetPsi(n, l, ml, x, y, z));
            if (psi < 1e-4f)
            {
                if (!detectedAngles.Any(a => Mathf.Abs(Mathf.DeltaAngle(a * Mathf.Rad2Deg, angle * Mathf.Rad2Deg)) < 5f))
                {
                    detectedAngles.Add(angle);
                    if (detectedAngles.Count == l) break;
                }
            }
        }
        
        int num = 0;
        switch (plane)
        {
            case Plane.XY:
                num = 0;
                break;
            case Plane.XZ:
                num = 1;
                break;
            case Plane.YZ:
                num = 2;
                break;
        }

        foreach (float angle in detectedAngles)
        {
            Vector3 dir = plane switch
            {
                Plane.XY => new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0),
                Plane.YZ => new Vector3(0, Mathf.Cos(angle), Mathf.Sin(angle)),
                Plane.XZ => new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)),
                _ => Vector3.forward
            };

            GameObject lineObj = new GameObject("Angular Node");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, -dir * lineLength);
            lr.SetPosition(1, dir * lineLength);
            lr.startWidth = lr.endWidth = lineWidth;
            lr.material = lineMat;
            lr.startColor = lr.endColor = lineColor;
            lr.useWorldSpace = true;
            
            activeCSAngularNodes[num].Add(lineObj);
        }
    }
    
    public void DestroyCSAngularNode(bool isAll, int num = 0)
    {
        if (isAll)
        {
            for (int i = 0; i < 3; i++)
            for (int j = activeCSAngularNodes[i].Count - 1; j >= 0; j--)
                if (activeCSAngularNodes[i] != null)
                    Destroy(activeCSAngularNodes[i][j]);
        }
        else for (int i = activeCSAngularNodes[num].Count - 1; i >= 0; i--) if(activeCSAngularNodes[num][i] != null)
            Destroy(activeCSAngularNodes[num][i]);
    }

    public void Psi(int n, int l, int ml, bool isOverlap = false, bool isSeparate = false, int num = 0)
    {
        LineChart chart;

        if (isSeparate) chart = psiChart;
        else chart = mainChart;
        
        // Remove all existing series & data
        if (isOverlap)
            chart.AddSerie<Line>($"R vs r ({num})");
        else
        {
            chart.RemoveData();
            chart.AddSerie<Line>("R vs r");
        }

        chart.GetChartComponent<Title>().text = "R (Ψ Real) vs Radius";

        var xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Category;
        xAxis.boundaryGap = true;

        var yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;

        float psiMax = 0f;
        float psiMin = 0f;

        (_, maxRadius) = ComputePsiCutoff(n, l, ml);
        maxRadius *= chartMargin;

        // Populate data
        for (int i = 0; i <= sampleCount; i++)
        {
            float r = i * (maxRadius / sampleCount);
            chart.AddXAxisData(r.ToString("F2"));
            float psi = GetPsi(n, l, ml, r, 0f, 0f);
            if(psi > psiMax) psiMax = psi;
            if(psi < psiMin) psiMin = psi;
            chart.AddData(0, psi);
        }

        // Configure the series styling
        var serie = chart.series[0] as Serie;
        serie.symbol.show = false;

        xAxis.axisName.name = "Radius";
        yAxis.axisName.name = "R";

        yAxis.max = psiMax;
        yAxis.min = psiMin;

        serie.lineStyle.color = chartLineColor;
        serie.clip = true;
        
        chart.RefreshChart();
        isChart = true;
    }
    
    public void PsiSquared(int n, int l, int ml, bool isOverlap = false, bool isSeparate = false, int num = 0)
    {
        LineChart chart;

        if (isSeparate) chart = psiSqChart;
        else chart = mainChart;
        
        // Remove all existing series & data
        if (isOverlap)
            chart.AddSerie<Line>($"Ψ² vs r ({num})");
        else
        {
            chart.RemoveData();
            chart.AddSerie<Line>("Ψ² vs r");
        }
        
        chart.GetChartComponent<Title>().text = "Ψ² vs Radius";

        var xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Category;
        xAxis.boundaryGap = true;

        var yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;

        float psiMax = 0f;

        (_, maxRadius) = ComputePsiCutoff(n, l, ml);
        maxRadius *= chartMargin;

        // Populate data
        for (int i = 0; i <= sampleCount; i++)
        {
            float r = i * (maxRadius / sampleCount);
            chart.AddXAxisData(r.ToString("F2"));
            float psi = GetPsi(n, l, ml, r, 0f, 0f);
            if(Math.Abs(psi) > psiMax) psiMax = Math.Abs(psi);
            chart.AddData(0, psi * psi);
        }

        // Configure the series styling
        var serie = chart.series[0] as Serie;
        serie.symbol.show = false;

        xAxis.axisName.name = "Radius";
        yAxis.axisName.name = "Ψ²";

        yAxis.max = psiMax * cutYMargin;
        float cutXMax = GetPsi(n, l, ml, maxRadius * (1 - cutXMargin), 0f, 0f);
        cutXMax *= cutXMax;
        if(cutXMax > yAxis.max) yAxis.max = cutXMax;

        serie.lineStyle.color = chartLineColor;
        serie.clip = true;
        
        chart.RefreshChart();
        isChart = true;
    }
    
    public void PsiSquaredRSquared(int n, int l, int ml, bool  isOverlap = false, bool isSeparate = false, int num = 0)
    {
        LineChart chart;

        if (isSeparate) chart = psiSqRSqChart;
        else chart = mainChart;
        
        // Remove all existing series & data
        if (isOverlap)
            chart.AddSerie<Line>($"r²Ψ² vs r ({num})");
        else
        {
            chart.RemoveData();
            chart.AddSerie<Line>("r²Ψ² vs r");
        }
        
        chart.GetChartComponent<Title>().text = "Ψ²r² vs Radius";
        
        var xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Category;
        xAxis.boundaryGap = true;

        var yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;

        float maxValue = 0f;

        int thetaSteps = 36; // 10° steps
        int phiSteps = 72;   // 5° steps
        float dTheta = Mathf.PI / thetaSteps;
        float dPhi = 2 * Mathf.PI / phiSteps;
        
        (_, maxRadius) = ComputePsiCutoff(n, l, ml);
        maxRadius *= chartMargin;

        for (int i = 0; i <= sampleCount; i++)
        {
            float r = i * (maxRadius / sampleCount);
            float sum = 0f;

            for (int t = 0; t < thetaSteps; t++)
            {
                float theta = t * dTheta + dTheta / 2; // center of step
                float sinTheta = Mathf.Sin(theta);

                for (int p = 0; p < phiSteps; p++)
                {
                    float phi = p * dPhi + dPhi / 2;

                    // Convert spherical to Cartesian coordinates
                    float x = r * sinTheta * Mathf.Cos(phi);
                    float y = r * sinTheta * Mathf.Sin(phi);
                    float z = r * Mathf.Cos(theta);

                    float psi = GetPsi(n, l, ml, x, y, z);
                    float value = psi * psi * sinTheta * dTheta * dPhi; // spherical integration element

                    sum += value;
                }
            }

            float result = r * r * sum; // r² * ∫ψ² dΩ
            if (float.IsNaN(result) || float.IsInfinity(result))
                result = 0f; // or skip this point entirely
            chart.AddXAxisData(r.ToString("F2"));
            chart.AddData(0, result);
            if (result > maxValue) maxValue = result;
        }

        var serie = chart.series[0] as Serie;
        serie.symbol.show = false;
        serie.lineStyle.color = chartLineColor;
        serie.clip = true;

        xAxis.axisName.name = "Radius";
        yAxis.axisName.name = "Ψ²r²";

        yAxis.max = maxValue;

        serie.lineStyle.color = chartLineColor;
        serie.clip = true;
        
        chart.RefreshChart();
        isChart = true;
    }
    
    private void ChartRVisualizer()
    {
        if (lastMousePos != (Vector2)Input.mousePosition)
        {
            Vector2 localMousePos;

            if (wasInsideLastFrame)
            {
                Destroy(prevRVisualizer);
                lastMousePos = Input.mousePosition;
            }

            foreach (RectTransform chartRectTransform in chartRectTransforms)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(chartRectTransform, 
                        Input.mousePosition, mainCamera, out localMousePos) 
                    && chartRectTransform.rect.Contains(localMousePos))
                {
                    wasInsideLastFrame = true;
                    lastMousePos = Input.mousePosition;

                    float chartWidth = chartRectTransform.rect.width;
                    float normalizedX = Mathf.InverseLerp(-chartWidth / 2, chartWidth / 2, localMousePos.x);
                    normalizedX = Mathf.Clamp01(normalizedX);
                    float r = normalizedX * maxRadius;

                    if(!isBillBoard) prevRVisualizer = DrawCircle(r, rVisualizerPlane, $"R Visualizer");
                    else
                    {
                        prevRVisualizer = new GameObject("R Visualizer");
                        GameObject ring = DrawCircle(r, Plane.XY, "R Visualizer Circle");
                        ring.transform.parent = prevRVisualizer.transform;
                        prevRVisualizer.AddComponent<SimpleBillboard>();
                    }

                    // === Move Tooltip to mouse X ===
                    foreach (RectTransform tooltip in tooltips)
                    {
                        tooltip.gameObject.SetActive(true);
                        if (!tooltip.gameObject.activeInHierarchy) return;
                        if (!tooltip.gameObject.activeInHierarchy) return;
                        tooltip.anchoredPosition = new Vector2(localMousePos.x + 134, 0f);
                    }

                    return;
                }
            }
            
            wasInsideLastFrame = false;
            foreach (RectTransform tooltip in tooltips) tooltip.gameObject.SetActive(false);
        }
    }

    
    public void Wave1D()
    {
        GameObject go = new GameObject("Wave");
        LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
        
        lineRenderer.material = lineMat;
        lineRenderer.material.color = lineColor;
        lineRenderer.widthMultiplier = lineWidth;
        
        lineRenderer.positionCount = waveResolution;
        float step = length / (waveResolution - 1);
        float startX = -length / 2f;

        for (int i = 0; i < waveResolution; i++)
        {
            float x = startX + i * step;
            float z = Mathf.Sin((x / period) * 2 * Mathf.PI) * amplitude;
            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
        }

        wave1DObject = go;
    }
    
    public void DestroyWave1D()
    {
        // If it exists, destroy it
        if (wave1DObject != null)
        {
            Destroy(wave1DObject);
            wave1DObject = null;
        }
    }
    

    public void Wave2D()
    {
        Mesh mesh = new Mesh();

        int vertCount = waveResolution * waveResolution;
        Vector3[] vertices = new Vector3[vertCount];
        int[] triangles = new int[(waveResolution - 1) * (waveResolution - 1) * 6];
        Vector2[] uvs = new Vector2[vertCount];

        float halfSize = waveSize / 2f;
        float step = waveSize / (waveResolution - 1);

        // Generate vertices
        for (int y = 0; y < waveResolution; y++)
        {
            for (int x = 0; x < waveResolution; x++)
            {
                int i = x + y * waveResolution;
                float xPos = -halfSize + x * step;
                float zPos = -halfSize + y * step;
                float radius = Mathf.Sqrt(xPos * xPos + zPos * zPos);
                float yPos = Mathf.Sin((radius / waveLength) * 2f * Mathf.PI) * amplitude;

                vertices[i] = new Vector3(xPos, yPos, zPos);
                uvs[i] = new Vector2((float)x / (waveResolution - 1), (float)y / (waveResolution - 1));
            }
        }

        // Generate triangles
        int t = 0;
        for (int y = 0; y < waveResolution - 1; y++)
        {
            for (int x = 0; x < waveResolution - 1; x++)
            {
                int i = x + y * waveResolution;

                triangles[t++] = i;
                triangles[t++] = i + waveResolution;
                triangles[t++] = i + 1;

                triangles[t++] = i + 1;
                triangles[t++] = i + waveResolution;
                triangles[t++] = i + waveResolution + 1;
            }
        }

        // Assign to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        // Create object and store reference
        wave2DObject = new GameObject("Wave2D");
        wave2DObject.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer mr = wave2DObject.AddComponent<MeshRenderer>();
        mr.material = phaseNegativeMat;
    }

    public void DestroyWave2D()
    {
        if (wave2DObject != null)
        {
            Destroy(wave2DObject);
            wave2DObject = null; // clear reference
        }
    }


    public void Wave3D()
    {
        // Clean up previous spheres
        foreach (Transform child in transform)
            DestroyImmediate(child.gameObject);

        Vector3 center = transform.position;
        float twoPi = Mathf.PI * 2f;
        float waveStep = twoPi / waveLength3D;
        int numWaves = Mathf.FloorToInt(length / waveStep);

        // Get the shared material from this object's renderer
        Material sharedMaterial = phaseNegativeMat;

        for (int i = 0; i < numWaves; i++)
        {
            float dist = i * waveStep;

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform);
            sphere.transform.position = center;
            sphere.transform.localScale = Vector3.one * (dist * 2f); // Diameter
            sphere.name = $"Wave Sphere {i}";

            // Use shared material
            Renderer rend = sphere.GetComponent<Renderer>();
            rend.material = sharedMaterial;
        }
    }
    
    public void DestroyWave3D()
    {
        // Remove all child objects (the spheres)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }


    private void UpdateOrbitalInfo(int n, int l, int ml, int num)
    {
        char lName = new char();
        string mlName = new string("");
        
        switch (l)
        {
            case 0:
                lName = 's';
                mlName = "";
                break;
            case 1:
                lName = 'p';
                switch (ml)
                {
                    case -1:
                        mlName = "y";
                        break;
                    case 0:
                        mlName = "z";
                        break;
                    case 1:
                        mlName = "x";
                        break;
                }
                break;
            case 2:
                lName = 'd';
                switch (ml)
                {
                    case -2:
                        mlName = "xy";
                        break;
                    case -1:
                        mlName = "yz";
                        break;
                    case 0:
                        mlName = "z<sup>2</sup>";
                        break;
                    case 1:
                        mlName = "xz";
                        break;
                    case 2:
                        mlName = "x<sup>2</sup>-y<sup>2</sup>";
                        break;
                }
                break;
        }

        activeOrbitalInfo[num].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            $"{n}{lName}<sub>{mlName}</sub>";
        activeOrbitalInfo[num].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"n = {n}";
        activeOrbitalInfo[num].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"l = {l}";
        activeOrbitalInfo[num].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"m<sub>l</sub> = {ml}";
        
        RefreshLayoutNow();
        
    }
    

    private float GetPsi(int n, int l, int ml, float x, float y, float z)
    {
        float r = Mathf.Sqrt(x * x + y * y + z * z);

        // Avoid division by zero / NaN at the origin.
        // For hydrogenic orbitals: Y_lm(0) is only finite for l=0; for l>0 it should go to 0.
        if (r < 1e-5f)
        {
            // Angular part: only s (l=0) survives at r=0
            float Y0 = 0.282095f; // Y_00
            float rnl0 = n switch
            {
                1 when l == 0 => (float)(2f * Math.Exp(-0f)),
                2 when l == 0 => (float)(0.353553f * (2 - 0f) * Math.Exp(-0f / 2)),
                3 when l == 0 => (float)(0.0142556f * (27 - 0f + 0f) * Math.Exp(-0f / 3)),
                _ => 0f
            };
            return l == 0 ? Y0 * rnl0 : 0f;
        }

        // Use safe inverses
        float invR = 1f / r;
        float invR2 = invR * invR;

        float Y = 0f, rnl = 0f;

        // --- Angular (real harmonics) ---
        switch (l)
        {
            case 0: Y = 0.282095f; break;
            case 1:
                if (ml == -1) Y = 0.488603f * y * invR;
                else if (ml == 0) Y = 0.488603f * z * invR;
                else if (ml == 1) Y = 0.488603f * x * invR;
                break;
            case 2:
                if (ml == -2) Y = 0.546274f * x * y * invR2;
                else if (ml == -1) Y = 1.092548f * y * z * invR2;
                else if (ml == 0)  Y = 0.315392f * (2 * z * z - x * x - y * y) * invR2;
                else if (ml == 1) Y = 1.092548f * x * z * invR2;
                else if (ml == 2) Y = 0.546274f * (x * x - y * y) * invR2;
                break;
        }

        // --- Radial (your approximations) ---
        switch (l)
        {
            case 0:
                if (n == 1) rnl = (float)(2f * Math.Exp(-r));
                else if (n == 2) rnl = (float)(0.353553f * (2 - r) * Math.Exp(-r / 2));
                else if (n == 3) rnl = (float)(0.0142556f * (27 - 18 * r + 2 * r * r) * Math.Exp(-r / 3));
                break;
            case 1:
                if (n == 2) rnl = (float)(0.204124f * r * Math.Exp(-r / 2));
                else if (n == 3) rnl = (float)(0.0201604f * (6 * r - r * r) * Math.Exp(-r / 3));
                break;
            case 2:
                rnl = (float)(0.00901601f * r * r * Math.Exp(-r / 3));
                break;
        }

        return Y * rnl;
    }


    (float threshold, float rCutoff) ComputePsiCutoff(int n, int l, int ml,
        float rMax = 20f, float dx = 0.5f, float targetFraction = 0.9f)
    {
        List<(float psiSq, float weightedVol, float r)> data = new();
        float dV = dx * dx * dx;
        float rMaxSq = rMax * rMax;

        for (float x = -rMax; x <= rMax; x += dx)
        for (float y = -rMax; y <= rMax; y += dx)
        for (float z = -rMax; z <= rMax; z += dx)
        {
            if (x * x + y * y + z * z > rMaxSq) continue;

            float psi = GetPsi(n, l, ml, x, y, z);
            if (float.IsNaN(psi) || float.IsInfinity(psi)) continue;

            float psiSq = psi * psi;
            data.Add((psiSq, psiSq * dV, Mathf.Sqrt(x * x + y * y + z * z)));
        }

        if (data.Count == 0)
            return (0f, 1f);

        float totalProb = data.Sum(d => d.weightedVol);
        float targetProb = totalProb * targetFraction;

        var sorted = data.OrderByDescending(d => d.weightedVol).ToList();

        float cum = 0f;
        float maxR = 0f;
        foreach (var (psiSq, vol, r) in sorted)
        {
            cum += vol;
            maxR = Mathf.Max(maxR, r);
            if (cum >= targetProb)
                return (psiSq, maxR);
        }

        return (sorted.Last().psiSq, sorted.Last().r);
    }
    
    private void CreateAndApplyMesh(float[,,] targetData, float targetThreshold, bool isOverlap)
    {
        Mesh mesh0 = CreateMeshForPhase(0, targetData, targetThreshold);
        Mesh mesh1 = CreateMeshForPhase(1, targetData, targetThreshold);

        List<Vector3> vertices = new(mesh0.vertexCount + mesh1.vertexCount);
        List<int> submesh0 = new(mesh0.triangles);
        List<int> submesh1 = new();

        vertices.AddRange(mesh0.vertices);
        int offsetIndex = vertices.Count;
        vertices.AddRange(mesh1.vertices);
        submesh1.AddRange(mesh1.triangles.Select(i => i + offsetIndex));

        Mesh mesh = new()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        mesh.SetVertices(vertices);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(submesh0, 0);
        mesh.SetTriangles(submesh1, 1);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.Optimize();

        if (isOverlap)
        {
            GameObject go = new GameObject("Orbital Overlap");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.materials = new Material[] { phasePositiveMat, phaseNegativeMat };
            mf.mesh = mesh;
            activeOverlaps.Add(go);
        }
        else GetComponent<MeshFilter>().mesh = mesh;
    }
    
    private Mesh CreateMeshForPhase(int targetPhase, float[,,] targetData, float targetThreshold)
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        Vector3 centerOffset = new((gridSize - 1) * 0.5f, (gridSize - 1) * 0.5f, (gridSize - 1) * 0.5f);

        float[] cube = new float[8];
        Vector3[] corner = new Vector3[8];
        int[] cornerPhase = new int[8];
        Vector3[] edgeVertex = new Vector3[12];
        int[] edgePhase = new int[12];
        bool[] edgeUsed = new bool[12];

        for (int x = 0; x < gridSize - 1; x++)
        for (int y = 0; y < gridSize - 1; y++)
        for (int z = 0; z < gridSize - 1; z++)
        {
            Vector3Int pos = new(x, y, z);
            bool skipCube = false;

            for (int i = 0; i < 8; i++)
            {
                Vector3Int cp = pos + MarchingCubesTables.CornerTable[i];
                float val = targetData[cp.x, cp.y, cp.z];
                if (!float.IsFinite(val)) { skipCube = true; break; }

                cube[i] = val * val;
                corner[i] = new Vector3(cp.x, cp.y, cp.z) - centerOffset;
                cornerPhase[i] = val >= 0f ? 1 : 0;
            }

            if (skipCube) continue;

            int configIndex = 0;
            for (int i = 0; i < 8; i++)
                if (cube[i] >= targetThreshold) configIndex |= 1 << i;

            int edgeFlags = MarchingCubesTables.EdgeTable[configIndex];
            if (edgeFlags == 0) continue;

            Array.Clear(edgeUsed, 0, 12);

            for (int i = 0; i < 12; i++)
            {
                if ((edgeFlags & (1 << i)) == 0) continue;

                int a = MarchingCubesTables.EdgeConnection[i, 0];
                int b = MarchingCubesTables.EdgeConnection[i, 1];
                float denom = cube[b] - cube[a];

                if (Math.Abs(denom) < 1e-7f) continue;

                float t = (targetThreshold - cube[a]) / denom;
                t = t < 0f ? 0f : (t > 1f ? 1f : t);

                edgeVertex[i] = corner[a] + t * (corner[b] - corner[a]);
                edgePhase[i] = Math.Abs(t - 0.5f) <= 0.5f ? cornerPhase[a] : cornerPhase[b];
                edgeUsed[i] = true;
            }

            for (int i = 0; MarchingCubesTables.TriangleTable[configIndex, i] != -1; i += 3)
            {
                int i0 = MarchingCubesTables.TriangleTable[configIndex, i];
                int i1 = MarchingCubesTables.TriangleTable[configIndex, i + 1];
                int i2 = MarchingCubesTables.TriangleTable[configIndex, i + 2];

                if (!edgeUsed[i0] || !edgeUsed[i1] || !edgeUsed[i2]) continue;
                if (edgePhase[i0] != targetPhase || edgePhase[i1] != targetPhase || edgePhase[i2] != targetPhase)
                    continue;

                int idx = vertices.Count;
                vertices.Add(edgeVertex[i0]);
                vertices.Add(edgeVertex[i1]);
                vertices.Add(edgeVertex[i2]);
                triangles.Add(idx);
                triangles.Add(idx + 1);
                triangles.Add(idx + 2);
            }
        }

        Mesh mesh = new()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.Optimize();
        
        return mesh;
    }

/*
    private Mesh CreateMeshForPhase(int targetPhase, float[,,] targetData, float targetThreshold)
    {
        List<Vector3> vertices = new();
        List<int> lineIndices = new();
        Vector3 centerOffset = new((gridSize - 1) * 0.5f, (gridSize - 1) * 0.5f, (gridSize - 1) * 0.5f);

        float[] cube = new float[8];
        Vector3[] corner = new Vector3[8];
        int[] cornerPhase = new int[8];
        Vector3[] edgeVertex = new Vector3[12];
        int[] edgePhase = new int[12];
        bool[] edgeUsed = new bool[12];

        for (int x = 0; x < gridSize - 1; x++)
        for (int y = 0; y < gridSize - 1; y++)
        for (int z = 0; z < gridSize - 1; z++)
        {
            Vector3Int pos = new(x, y, z);
            bool skipCube = false;

            for (int i = 0; i < 8; i++)
            {
                Vector3Int cp = pos + MarchingCubesTables.CornerTable[i];
                float val = targetData[cp.x, cp.y, cp.z];
                if (!float.IsFinite(val)) { skipCube = true; break; }

                cube[i] = val * val;
                corner[i] = new Vector3(cp.x, cp.y, cp.z) - centerOffset;
                cornerPhase[i] = val >= 0f ? 1 : 0;
            }

            if (skipCube) continue;

            int configIndex = 0;
            for (int i = 0; i < 8; i++)
                if (cube[i] >= targetThreshold) configIndex |= 1 << i;

            int edgeFlags = MarchingCubesTables.EdgeTable[configIndex];
            if (edgeFlags == 0) continue;

            Array.Clear(edgeUsed, 0, 12);

            for (int i = 0; i < 12; i++)
            {
                if ((edgeFlags & (1 << i)) == 0) continue;

                int a = MarchingCubesTables.EdgeConnection[i, 0];
                int b = MarchingCubesTables.EdgeConnection[i, 1];
                float denom = cube[b] - cube[a];

                if (Math.Abs(denom) < 1e-7f) continue;

                float t = (targetThreshold - cube[a]) / denom;
                t = Mathf.Clamp01(t);

                edgeVertex[i] = corner[a] + t * (corner[b] - corner[a]);
                edgePhase[i] = Mathf.Abs(t - 0.5f) <= 0.5f ? cornerPhase[a] : cornerPhase[b];
                edgeUsed[i] = true;
            }

            for (int i = 0; MarchingCubesTables.TriangleTable[configIndex, i] != -1; i += 3)
            {
                int i0 = MarchingCubesTables.TriangleTable[configIndex, i];
                int i1 = MarchingCubesTables.TriangleTable[configIndex, i + 1];
                int i2 = MarchingCubesTables.TriangleTable[configIndex, i + 2];

                if (!edgeUsed[i0] || !edgeUsed[i1] || !edgeUsed[i2]) continue;
                if (edgePhase[i0] != targetPhase || edgePhase[i1] != targetPhase || edgePhase[i2] != targetPhase)
                    continue;

                int vIndex = vertices.Count;
                Vector3 v0 = edgeVertex[i0];
                Vector3 v1 = edgeVertex[i1];
                Vector3 v2 = edgeVertex[i2];

                vertices.Add(v0); vertices.Add(v1); lineIndices.Add(vIndex); lineIndices.Add(vIndex + 1);
                vertices.Add(v1); vertices.Add(v2); lineIndices.Add(vIndex + 2); lineIndices.Add(vIndex + 3);
                vertices.Add(v2); vertices.Add(v0); lineIndices.Add(vIndex + 4); lineIndices.Add(vIndex + 5);
            }
        }

        Mesh mesh = new()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        mesh.SetVertices(vertices);
        mesh.SetIndices(lineIndices, MeshTopology.Lines, 0);
        return mesh;
    }
    private void CreateAndApplyMesh(float[,,] targetData, float targetThreshold)
    {
        Mesh mesh0 = CreateMeshForPhase(0, targetData, targetThreshold);
        Mesh mesh1 = CreateMeshForPhase(1, targetData, targetThreshold);

        List<Vector3> vertices = new(mesh0.vertexCount + mesh1.vertexCount);
        List<int> indices = new(mesh0.GetIndices(0));

        vertices.AddRange(mesh0.vertices);

        int offset = vertices.Count;
        vertices.AddRange(mesh1.vertices);

        indices.AddRange(mesh1.GetIndices(0).Select(i => i + offset));

        Mesh combinedMesh = new()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        combinedMesh.SetVertices(vertices);
        combinedMesh.SetIndices(indices, MeshTopology.Lines, 0);

        GetComponent<MeshFilter>().mesh = combinedMesh;
    }
*/
    
    private GameObject DrawCircle(float radius, Plane plane, string name)
    {
        if (!float.IsFinite(radius) || radius <= 0f) return null;

        Vector3[] points = new Vector3[circleSegments + 1];
        for (int j = 0; j <= circleSegments; j++)
        {
            float angle = 2 * Mathf.PI * j / circleSegments;
            float x = 0, y = 0, z = 0;

            switch (plane)
            {
                case Plane.XY:
                    x = radius * Mathf.Cos(angle);
                    y = radius * Mathf.Sin(angle);
                    z = 0;
                    break;
                case Plane.XZ:
                    x = radius * Mathf.Cos(angle);
                    y = 0;
                    z = radius * Mathf.Sin(angle);
                    break;
                case Plane.YZ:
                    x = 0;
                    y = radius * Mathf.Cos(angle);
                    z = radius * Mathf.Sin(angle);
                    break;
            }
            points[j] = new Vector3(x, y, z);
        }
        
        // Additional safety: Remove the whole node if any point is invalid
        bool allFinite = true;
        foreach (var p in points)
            if (!float.IsFinite(p.x) || !float.IsFinite(p.y) || !float.IsFinite(p.z))
                allFinite = false;

        if (!allFinite) return null;

        GameObject go = new GameObject(name);
        
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = points.Length;
        lr.SetPositions(points);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = lineMat;
        lr.material.color = lineColor;
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.numCapVertices = 4;
        
        return go;
    }
    
    private static readonly (int, int)[][] MarchingSquaresTable = new (int, int)[][]
    {
        new (int, int)[] {},                         // 0
        new (int, int)[] {(3, 0)},                  // 1
        new (int, int)[] {(0, 1)},                  // 2
        new (int, int)[] {(3, 1)},                  // 3
        new (int, int)[] {(1, 2)},                  // 4
        new (int, int)[] {(3, 0), (1, 2)},          // 5
        new (int, int)[] {(0, 2)},                  // 6
        new (int, int)[] {(3, 2)},                  // 7
        new (int, int)[] {(2, 3)},                  // 8
        new (int, int)[] {(0, 2)},                  // 9
        new (int, int)[] {(0, 1), (2, 3)},          // 10
        new (int, int)[] {(1, 3)},                  // 11
        new (int, int)[] {(2, 3)},                  // 12
        new (int, int)[] {(0, 1)},                  // 13
        new (int, int)[] {(3, 0)},                  // 14
        new (int, int)[] {}                         // 15
    };
    
    private List<Vector3> GenerateIsolineSegments(float[,] values, float threshold, Vector3 origin, Vector3 right, Vector3 up, float rMax)
    {
        List<Vector3> segments = new();
        int w = values.GetLength(0);
        int h = values.GetLength(1);

        for (int y = 0; y < h - 1; y++)
        for (int x = 0; x < w - 1; x++)
        {
            float v0 = values[x, y];
            float v1 = values[x + 1, y];
            float v2 = values[x + 1, y + 1];
            float v3 = values[x, y + 1];

            int index = 0;
            if (v0 > threshold) index |= 1;
            if (v1 > threshold) index |= 2;
            if (v2 > threshold) index |= 4;
            if (v3 > threshold) index |= 8;

            foreach (var edge in MarchingSquaresTable[index])
            {
                Vector2 p1 = InterpolateEdge(x, y, edge.Item1, v0, v1, v2, v3, threshold);
                Vector2 p2 = InterpolateEdge(x, y, edge.Item2, v0, v1, v2, v3, threshold);

                float px1 = Mathf.Lerp(-rMax, rMax, p1.x / (w - 1));
                float py1 = Mathf.Lerp(-rMax, rMax, p1.y / (h - 1));
                float px2 = Mathf.Lerp(-rMax, rMax, p2.x / (w - 1));
                float py2 = Mathf.Lerp(-rMax, rMax, p2.y / (h - 1));

                Vector3 world1 = origin + px1 * right + py1 * up;
                Vector3 world2 = origin + px2 * right + py2 * up;

                segments.Add(world1);
                segments.Add(world2);
            }
        }

        return segments;
    }
    
    private Vector2 InterpolateEdge(int x, int y, int edge, float v0, float v1, float v2, float v3, float threshold)
    {
        return edge switch
        {
            0 => new Vector2(x + Mathf.InverseLerp(v0, v1, threshold), y),
            1 => new Vector2(x + 1, y + Mathf.InverseLerp(v1, v2, threshold)),
            2 => new Vector2(x + Mathf.InverseLerp(v3, v2, threshold), y + 1),
            3 => new Vector2(x, y + Mathf.InverseLerp(v0, v3, threshold)),
            _ => Vector2.zero
        };
    }

    public void RefreshLayoutNow()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        Canvas.ForceUpdateCanvases();
    }
    
    public bool IsChart { get => isChart; set => isChart = value; }

    public bool IsBillBoard { get => isBillBoard; set => isBillBoard = value; }
    
    public int GridSize { get => gridSize; set => gridSize = value; }
}