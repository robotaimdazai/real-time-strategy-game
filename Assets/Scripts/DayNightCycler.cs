using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycler : MonoBehaviour
{
    public GameParameters gameParameters;
    public Transform starsTransform;

    private float _starsRefreshRate;
    private float _rotationAngleStep;
    private Vector3 _rotationAxis;

    private void Awake()
    {
        // apply initial rotation on stars
        starsTransform.rotation = Quaternion.Euler(
            gameParameters.dayInitialRatio * 360f,
            -30f,
            0f
        );
        // compute relevant calculation parameters
        _starsRefreshRate = 0.1f;
        _rotationAxis = starsTransform.right;
        _rotationAngleStep = 360f * _starsRefreshRate / gameParameters.dayLengthInSeconds;
    }

    private void Start()
    {
        StartCoroutine(_UpdateStars());
    }

    private IEnumerator _UpdateStars()
    {
        while (true)
        {
            starsTransform.Rotate(_rotationAxis, _rotationAngleStep, Space.World);
            yield return new WaitForSeconds(_starsRefreshRate);
        }
    }
}
