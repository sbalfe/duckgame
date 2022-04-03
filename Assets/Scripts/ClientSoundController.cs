using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientSoundController : NetworkBehaviour
{
    [SerializeField] List<AudioClip> soundClips;
    [SerializeField] AudioSource audioSource;

    [ClientRpc]
    public void PlaySoundClientRpc(int soundIndex)
    {
        if (soundIndex < soundClips.Count)
        {
            audioSource.PlayOneShot(soundClips[soundIndex]);
        }
    }

    public void PlayLocalSound(int soundIndex)
    {
        if (soundIndex < soundClips.Count)
        {
            audioSource.PlayOneShot(soundClips[soundIndex]);
        }
    }
}
