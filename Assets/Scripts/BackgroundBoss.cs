using UnityEngine;

public class BackgroundBoss : MonoBehaviour
{
    public AudioSource audioSource;
    void Start()
    {
        // Get the Button component and add a listener to the onClick even
        PlaySound();
    }
    public void PlaySound()
    {
        // Play the sound effect
        audioSource.Play();
    }
}
