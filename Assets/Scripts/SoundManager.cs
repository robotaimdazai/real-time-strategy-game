using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{

    public AudioSource audioSource;
    public GameSoundParameters soundParameters;
    public AudioMixerSnapshot paused;
    public AudioMixerSnapshot unpaused;
    public AudioMixer masterMixer;
    
    
    private void OnEnable()
    {
        EventManager.AddListener("PlaySoundByName", _OnPlaySoundByName);
        EventManager.AddListener("PauseGame", _OnPauseGame);
        EventManager.AddListener("ResumeGame", _OnResumeGame);
        EventManager.AddListener("UpdateGameParameter:musicVolume", _OnUpdateMusicVolume);
        EventManager.AddListener("UpdateGameParameter:sfxVolume", _OnUpdateSfxVolume);
    }
    
   private void OnDisable()
   {
       EventManager.RemoveListener("PlaySoundByName", _OnPlaySoundByName);
       EventManager.RemoveListener("PauseGame", _OnPauseGame);
       EventManager.RemoveListener("ResumeGame", _OnResumeGame);
       EventManager.RemoveListener("UpdateGameParameter:musicVolume", _OnUpdateMusicVolume);
       EventManager.RemoveListener("UpdateGameParameter:sfxVolume", _OnUpdateSfxVolume);
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
   
   private void _OnPauseGame()
   {
       paused.TransitionTo(0.01f);
   }

   private void _OnResumeGame()
   {
       unpaused.TransitionTo(0.01f);
   }
   
   private void _OnUpdateMusicVolume(object data)
   {
       float volume = (float)data;
       masterMixer.SetFloat("musicVol", volume);
   }

   private void _OnUpdateSfxVolume(object data)
   {
       float volume = (float)data;
       masterMixer.SetFloat("sfxVol", volume);
   }
}
