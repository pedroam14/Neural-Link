using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    
    public AudioClip swordHit, swordSwing, swordBlock, dying;
    public AudioSource audioSource;
    public void PlaySound(int clip)
    {
        switch (clip)
        {
            case 1:
                audioSource.PlayOneShot(swordSwing);
                break;
            case 2:
                audioSource.PlayOneShot(swordHit);
                break;
            case 3:
                audioSource.PlayOneShot(swordBlock);
                break;
            case 4:
                audioSource.PlayOneShot(dying);
                break;
        }
    }
}
