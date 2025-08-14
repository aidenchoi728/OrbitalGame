using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using XCharts.Runtime;
public class ParticleGameManager : MonoBehaviour
{
    [Header("Control Settings")]
    [SerializeField] private Slider tempSlider;
    [SerializeField] private TMP_Text tempText;
    [SerializeField] private Slider particleSlider;
    [SerializeField] private TMP_Text particleText;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeText;
    [SerializeField] private Slider massSlider;
    [SerializeField] private TMP_Text massText;
    
    [Header("Piston Settings")]
    [SerializeField] private Transform piston;
    [SerializeField] private int pistonMin, pistonMax;
    
    [Header("Spawner Settings")]
    [SerializeField] GameObject particlePrefab;
    [SerializeField] Vector3 spawnLocation = Vector3.zero;

    [Header("Visualization")]
    [SerializeField] int numBins = 10; // Number of bars in the histogram
    [SerializeField] float maxSpeed = 2000f; // Adjust as appropriate
    [SerializeField] BarChart barChart; // Your BarChart component here

    private IdealGasCalculator[] particleCalculators;
    private GameObject[] particles;

    private int prevParticleNum;
    private int particleNum;
    private int temp;
    private int mass;
    
    void Start()
    {
        prevParticleNum = 0;
        particleNum = (int)particleSlider.value;
        temp = (int)tempSlider.value;
        mass = (int)massSlider.value;
        
        particleCalculators = new IdealGasCalculator[(int)particleSlider.maxValue];
        particles = new GameObject[(int)particleSlider.maxValue];
        
        tempText.text = $"{temp}K";
        particleText.text = $"{particleNum}";
        massText.text = $"{mass} amu";
        
        SpawnParticles();
    }

    void SpawnParticles()
    {
        if (particleNum > prevParticleNum)
        {
            for (int i = prevParticleNum; i < particleNum; i++)
            {
                particles[i] = Instantiate(particlePrefab, spawnLocation, Quaternion.identity);
                particleCalculators[i] = particles[i].GetComponent<IdealGasCalculator>();
                particleCalculators[i].initialTemp = temp;
                particleCalculators[i].m = mass / 6.022e26;
            }
        }
        else if (particleNum < prevParticleNum)
        {
            for (int i = particleNum; i < prevParticleNum; i++)
            {
                Destroy(particles[i]);
            }
        }
        
        StartCoroutine(UpdateHistogramNextFrame());
    }

    IEnumerator UpdateHistogramNextFrame()
    {
        yield return null; // Wait one frame
        UpdateHistogram();
    }


    public void UpdateTemp()
    {
        temp = (int)tempSlider.value;
        tempText.text = $"{temp}K";
        for (int i = 0; i < particleNum; i++)
            particleCalculators[i].SetMaxwellSpeed(temp);
        UpdateHistogram();
    }

    public void UpdateParticle()
    {
        prevParticleNum = particleNum;
        particleNum = (int)particleSlider.value;
        particleText.text = $"{particleNum}";
        SpawnParticles();
    }

    public void UpdateVolume()
    {
        piston.position = new Vector3(pistonMax * volumeSlider.value + pistonMin * (1 - volumeSlider.value), piston.position.y, piston.position.z);
    }

    public void UpdateMass()
    {
        mass = (int)massSlider.value;
        massText.text = $"{mass} amu";
        
        for (int i = 0; i < particleNum; i++)
        {
            particleCalculators[i].m = mass / 6.022e26;
            particleCalculators[i].SetMaxwellSpeed(temp);
        }
        UpdateHistogram();
    }

    // Build a histogram and update the bar chart
    void UpdateHistogram()
    {
        float[] binCounts = new float[numBins];
        float binSize = maxSpeed / numBins;

        for (int i = 0; i < particleNum; i++)
        {
            float speed = (float)particleCalculators[i].speed;
            int bin = Mathf.Clamp((int)(speed / binSize), 0, numBins - 1);
            binCounts[bin]++;
        }

        if (barChart != null)
        {
            // Set X axis labels
            var xAxis = barChart.EnsureChartComponent<XAxis>();
            xAxis.ClearData();
            for (int i = 0; i < numBins; i++)
            {
                xAxis.AddData($"(i + 0.5) * binSize");
            }

            barChart.ClearData();
            for (int bin = 0; bin < numBins; bin++)
            {
                float center = (bin + 0.5f) * binSize; // Center of the bin
                barChart.AddData("Speeds", center, binCounts[bin]);
            }
            barChart.RefreshChart();
        }
    }
    
}
