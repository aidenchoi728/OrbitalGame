using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class IdealGasCalculator : MonoBehaviour
{
    [Header("Physics Parameters")]

    [SerializeField] private double kB = 1.380649e-23;      // Boltzmann constant
    [SerializeField] private float velocityMultiplier = 1f; // Multiplier for velocity magnitude

    public int initialTemp;
    public double speed;
    public double m;
    
    private Rigidbody rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Set initial speed in a random direction
        Vector3 initialDirection = UnityEngine.Random.onUnitSphere;
        SetMaxwellSpeed(initialTemp, initialDirection);
    }

    // Overload to allow optional direction input
    public void SetMaxwellSpeed(int temp)
    {
        // Use current direction, or random if nearly zero
        Vector3 dir = rb.linearVelocity.sqrMagnitude > 1e-8f ? rb.linearVelocity.normalized : UnityEngine.Random.onUnitSphere;
        SetMaxwellSpeed(temp, dir);
    }

    public void SetMaxwellSpeed(int temp, Vector3 direction)
    {
        double randomU = UnityEngine.Random.value;
        speed = InverseCDF(randomU, m, kB, temp);
        float finalSpeed = (float)(speed * velocityMultiplier);

        rb.linearVelocity = direction.normalized * finalSpeed;
    }

    // Inverse CDF via binary search
    double InverseCDF(double targetCDF, double m, double kB, int temp)
    {
        double lower = 0.0;
        double upper = 4000.0;
        double tolerance = 1e-6;
        double mid = 0.0;

        for (int i = 0; i < 100; i++)
        {
            mid = 0.5 * (lower + upper);
            double cdf = MaxwellCDF(mid, m, kB, temp);
            if (Math.Abs(cdf - targetCDF) < tolerance)
                return mid;
            if (cdf < targetCDF)
                lower = mid;
            else
                upper = mid;
        }
        return mid; // fallback
    }

    // Maxwell–Boltzmann cumulative distribution function (CDF) with embedded erf approximation
    double MaxwellCDF(double x, double m, double kB, int temp)
    {
        double a = m / (2.0 * kB * temp);
        double y = x * Math.Sqrt(a);

        // Erf approximation (Abramowitz & Stegun 7.1.26)
        double sign = y >= 0.0 ? 1.0 : -1.0;
        double absY = Math.Abs(y);
        double t = 1.0 / (1.0 + 0.3275911 * absY);
        double tau = t * (0.254829592 +
                          t * (-0.284496736 +
                               t * (1.421413741 +
                                    t * (-1.453152027 +
                                         t * 1.061405429))));
        double erfY = sign * (1.0 - tau * Math.Exp(-absY * absY));

        return erfY - (2.0 / Math.Sqrt(Math.PI)) * y * Math.Exp(-a * x * x);
    }
}
