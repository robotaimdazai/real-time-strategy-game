using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public AudioSource audioSource;
    public GameSoundParameters soundParameters;
    
    
    private void OnEnable()
    {
        EventManager.AddListener("PlaySoundByName", _OnPlaySoundByName);
    }
    
   private void OnDisable()
   {
       EventManager.RemoveListener("PlaySoundByName", _OnPlaySoundByName);
   }

   public void _OnPlaySoundByName(object data)
   {
       string clipName = data as string;
       var fields = typeof(GameSoundParameters).GetFields();
       AudioClip clip = null;
       foreach (var field in fields)
       {
           if (field.Name.Equals(clipName))
           {
               clip = field.GetValue(soundParameters) as AudioClip;
               break;
           }
       }
       
       if (clip == null)
       {
           Debug.LogWarning($"Unknown clip name: '{clipName}'");
           return;
       }

       // play the clip
       audioSource.PlayOneShot(clip);
   }
}
