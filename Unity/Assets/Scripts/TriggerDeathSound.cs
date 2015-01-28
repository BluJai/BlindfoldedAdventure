using UnityEngine;
using System.Collections;

public class TriggerDeathSound : MonoBehaviour {
    public AudioClip DeathSound;

    private void OnTriggerEnter(Collider col)
    {
        AudioSource.PlayClipAtPoint(DeathSound, Vector3.zero);
    }
}
