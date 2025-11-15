using UnityEngine;
using System.Collections;

public class ShakeEffect : MonoBehaviour
{
    public float shakeIntensity = 0.1f;
    public float shakeTime = 0.15f;

    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    public void PlayShake()
    {
        StopAllCoroutines();
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        float timer = 0f;

        while (timer < shakeTime)
        {
            timer += Time.deltaTime;
            transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * shakeIntensity;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
