
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class MixerSnapshotAutomation : MonoBehaviour
{
    public AudioMixer audioMixer;
    public AudioMixerSnapshot[] mixerSnapshots;

    [Min(0)] public float fadingTime = 0.1f;

    public void FadeToSnapshot(string name)
    {
        if (audioMixer)
        {
            var snapshot = mixerSnapshots.FirstOrDefault(x => x.name == name);

            if (snapshot)
            {
                var weights = new float[mixerSnapshots.Length];
                
                for (int i = 0; i < weights.Length; ++i)
                    weights[i] = (mixerSnapshots[i] == snapshot) ? 1f : 0f;

                audioMixer.TransitionToSnapshots(mixerSnapshots, weights, fadingTime);
            }
        }
    }
}

