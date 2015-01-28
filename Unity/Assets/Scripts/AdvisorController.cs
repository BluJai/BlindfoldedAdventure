using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdvisorController : MonoBehaviour {

    public AudioClip Yes1;
    public AudioClip Yes2;
    public AudioClip No1;
    public AudioClip No2;
    public AudioClip Faster1;
    public AudioClip Faster2;
    public AudioClip Slower1;
    public AudioClip Slower2;

    private List<AudioClip> Yeses = new List<AudioClip>();
    private List<AudioClip> Nos = new List<AudioClip>();
    private List<AudioClip> Fasters = new List<AudioClip>();
    private List<AudioClip> Slowers = new List<AudioClip>();

	// Use this for initialization
    private void Start()
    {
        if (Yes1 != null) Yeses.Add(Yes1);
        if (Yes2 != null) Yeses.Add(Yes2);
        if (No1 != null) Nos.Add(No1);
        if (No2 != null) Nos.Add(No2);
        if (Faster1 != null) Fasters.Add(Faster1);
        if (Faster2 != null) Fasters.Add(Faster2);
        if (Slower1 != null) Slowers.Add(Slower1);
        if (Slower2 != null) Slowers.Add(Slower2);
        Debug.Log("Yes sounds: " + Yeses.Count);
        Debug.Log("No sounds: " + Nos.Count);
        Debug.Log("Faster sounds: " + Fasters.Count);
        Debug.Log("Slower sounds: " + Slowers.Count);
    }

    // Update is called once per frame
    private void Update()
    {
        Debug.Log("YesP2: " + Input.GetAxis("YesP2").ToString()
                  + " NoP2: " + Input.GetAxis("NoP2").ToString()
                  + " FasterP2: " + Input.GetAxis("FasterP2").ToString()
                  + " SlowerP2: " + Input.GetAxis("SlowerP2").ToString());
        if (_alreadyPlaying) return;
        float yes = Input.GetAxis("YesP2");
        float no = Input.GetAxis("NoP2");
        float faster = Input.GetAxis("FasterP2");
        float slower = Input.GetAxis("SlowerP2");
        if (yes > 0 || no > 0 || faster > 0 || slower > 0) {
            _alreadyPlaying = true;
        }
        if (yes > 0) {
            Debug.Log("Yes pressed");
            PlayRandomClip(Yeses);
        }
        else if (no> 0) {
            Debug.Log("No pressed");
            PlayRandomClip(Nos);
        }
        else if (faster > 0) {
            Debug.Log("Faster pressed");
            PlayRandomClip(Fasters);
        }
        else if (slower > 0) {
            Debug.Log("Slower pressed");
            PlayRandomClip(Slowers);
        }
    }

    private bool _alreadyPlaying = false;

    private void PlayRandomClip(List<AudioClip> clips)
    {
        AudioSource.PlayClipAtPoint(clips[UnityEngine.Random.Range(0, clips.Count - 1)], Vector3.zero);
        _alreadyPlaying = false;
    }
}
