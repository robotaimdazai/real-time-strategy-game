using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sound Parameters", menuName = "Scriptable Objects/Game Sound Parameters", order = 11)]
public class GameSoundParameters : GameParameters
{
    public override string GetParametersName() => "Sound";
    
    [Header("Ambient sounds")]
    public AudioClip onDayStartSound;
    public AudioClip onNightStartSound;
    public AudioClip onBuildingPlacedSound;
    
    [Range(0, 100)]
    public int musicVolume;

    [Range(0, 100)]
    public int sfxVolume;
}
