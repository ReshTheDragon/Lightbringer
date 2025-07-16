using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip healthClip;
    [SerializeField] private AudioClip backgroundClip;
    

    public void playShootSound()
    {
      backgroundMusic.PlayOneShot(shootClip);
    }
    public void playHealthSound()
    {
        backgroundMusic.PlayOneShot(healthClip);
    }
    public void playBackGroundClip()
    {
        backgroundMusic.PlayOneShot(backgroundClip);
    }
    //public void playExplosionSound()
    //{
    //    backgroundMusic.PlayOneShot(explosionClip);
    //}
}
