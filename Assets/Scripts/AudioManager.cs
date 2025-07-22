using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip healthClip;
    [SerializeField] private AudioClip backgroundClip;
    [SerializeField] private AudioClip skillClip;
    [SerializeField] private AudioClip sliceClip;



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
    public void playSkillClip()
    {
        backgroundMusic.PlayOneShot(skillClip);
    }
    public void playSliceClip()
    {
        backgroundMusic.PlayOneShot(sliceClip);
    }
}
