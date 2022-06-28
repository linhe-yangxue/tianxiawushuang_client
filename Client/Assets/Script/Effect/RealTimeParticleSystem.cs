using UnityEngine;


public class RealTimeParticleSystem : MonoBehaviour
{
    private ParticleSystem[] particleSystems;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    private void Update()
    {
        foreach (var system in particleSystems)
        {
            if (system != null)
                system.Simulate(TimeSetting.realDeltaTime, false, false);
        }
    }
}