using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gawain_Footsteps : MonoBehaviour
{
    public AudioClip[] Footsteps;
    public AudioSource m_audioSource;
    public int counter=0;

    // Start is called before the first frame update
    void Start()
    {
      
    }


    public void sound_step()
    {
        if (Footsteps.Length > 0)
           {
            m_audioSource.PlayOneShot(Footsteps[counter]);
            counter++;
            if (counter > Footsteps.Length-1) counter = 0;
           }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
