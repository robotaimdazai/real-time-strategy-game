using System;
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


    private void Start()
    {
        masterMixer.SetFloat("musicVol", soundParameters.musicVolume);
        masterMixer.SetFloat("sfxVol", soundParameters.sfxVolume);
    }

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
       StartCoroutine(_TransitioningVolume(
           "musicVol",
           soundParameters.musicVolume,
           soundParameters.musicVolume - 6,
           0.5f
       ));
       StartCoroutine(_TransitioningVolume(
           "sfxVol",
           soundParameters.sfxVolume,
           -80,
           0.5f
       ));
   }

   private void _OnResumeGame()
   {
       StartCoroutine(_TransitioningVolume(
           "musicVol",
           soundParameters.musicVolume - 6,
           soundParameters.musicVolume,
           0.5f
       ));
       StartCoroutine(_TransitioningVolume(
           "sfxVol",
           -80,
           soundParameters.sfxVolume,
           0.5f
       ));
   }
   
   private void _OnUpdateMusicVolume(object data)
   {
       float volume = (float)data;
       masterMixer.SetFloat("musicVol", volume);
   }

   private void _OnUpdateSfxVolume(object data)
   {
       if (GameManager.instance.gameIsPaused) return;
       float volume = (float)data;
       masterMixer.SetFloat("sfxVol", volume);
   }
   
   private IEnumerator _TransitioningVolume(string volumeParameter, float from, float to, float delay)
   {
       float t = 0;
       while (t < delay)
       {
           masterMixer.SetFloat(volumeParameter, Mathf.Lerp(from, to, t / delay));
           t += Time.unscaledDeltaTime;
           yield return null;
       }
       masterMixer.SetFloat(volumeParameter, to);
   }
}
