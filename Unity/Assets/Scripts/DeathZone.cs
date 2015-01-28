using UnityEngine;
using System.Collections;

public class DeathZone : MonoBehaviour {

    private void OnTriggerEnter(Collider col)
    {
        Debug.Log("Hit the death trigger");
        GameManager.instance.LoseLife();
    }
}
