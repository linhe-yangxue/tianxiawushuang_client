using UnityEngine;
using System.Collections;

public class PlayParticleOnEnable : MonoBehaviour
{
    public float delay = 0f;

    private ParticleSystem[] particleSystems;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    private void OnEnable()
    {
        foreach (var system in particleSystems)
        {
            if (system == null)
                continue;
            system.Stop();
            system.Clear();
        }

        StartCoroutine(PlayDelayed(delay));
    }

    private IEnumerator PlayDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var system in particleSystems)
        {
            if (system == null)
                continue;
            system.Play();
        }
    }
}