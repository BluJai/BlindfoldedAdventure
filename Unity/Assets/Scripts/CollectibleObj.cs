using UnityEngine;
using System.Collections;

public class CollectibleObj : MonoBehaviour {

    private void OnTriggerEnter(Collider col)
    {
        Debug.Log("Reached collectible");
        GameManager.instance.ReachGoal();
    }
}
