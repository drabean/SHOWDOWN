using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Vector3 camOriginPos;

    private void Awake()
    {
        camOriginPos = transform.localPosition;
    }

    public void shakeCam(float duration, float power)
    {
        StartCoroutine(CameraShake(duration, power));
    }
    IEnumerator CameraShake(float duration, float magnitude)
    {
        float timer = 0;

        while(timer <= duration)
        {
            transform.localPosition = Random.insideUnitSphere * magnitude + camOriginPos;

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition =  camOriginPos;
    }
}
