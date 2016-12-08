using UnityEngine;
using System.Collections;

public class Cubes : MonoBehaviour
{
    public Timer timer;

    private ParticleSystem particles;

    // Use this for initialization
	void Start ()
    {
        particles = gameObject.GetComponent<ParticleSystem>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (timer.GetIntensity() < 0.025f)
        {
            particles.Clear();
        }
        
        particles.emissionRate = timer.GetIntensity() * 25;
        particles.startSpeed = timer.GetIntensity() * 25;

        particles.GetComponent<Renderer>().material.SetFloat("_Spook", Mathf.Clamp01(2 * (timer.GetIntensity() - 0.5f)));
    }
}
