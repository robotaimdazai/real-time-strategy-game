using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sound Parameters", menuName = "Scriptable Objects/Game Sound Parameters", order = 11)]
public class GameSoundParameters : ScriptableObject
{
    [Header("Ambient sounds")]
    public AudioClip onDayStartSound;
    public AudioClip onNightStartSound;
    public AudioClip onBuildingPlacedSound;
}
