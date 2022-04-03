using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientAudio : NetworkBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> audioClips;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    [ClientRpc]
    public void PlaySoundClientRpc(int index)
    {
        if (index >= audioClips.Count)
        {
            Debug.LogError("Audio clip index out of range");
            return;
        }
        audioSource.PlayOneShot(audioClips[index]);
    }
}