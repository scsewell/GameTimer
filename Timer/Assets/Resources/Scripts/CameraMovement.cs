using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Timer timer;
    
    void Update()
    {
        transform.position = timer.GetIntensity() * 2 * new Vector3(Mathf.Sin(Time.time * 1.05f), Mathf.Sin(Time.time * 1.17f), Mathf.Sin(Time.time * 0.91f));
        transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.forward + timer.GetIntensity() * 0.035f * new Vector3(Mathf.Sin(Time.time * 2.01f), Mathf.Sin((Time.time + 1.5f) * 1.97f), Mathf.Sin(Time.time * 2.09f)));
    }
}
