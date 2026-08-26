using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioSource audioSource;

    public void PlayClickSound()
    {
        audioSource.Play();
    }
}