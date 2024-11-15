
using System;
using UnityEngine;

public class MaxMovement : MonoBehaviour
{
    public Transform target;
    float maxSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        Vector3 myPos = transform.position;
        Vector3 targetPos = target.position;

        Vector3 offset = targetPos - myPos;
        Vector3 dir = offset.normalized;
        float magnitude = offset.magnitude;

        float sign = Mathf.Sign(magnitude);
        magnitude = Mathf.Abs(magnitude);

        magnitude = Mathf.Min(magnitude, maxSpeed * Time.deltaTime);
        Vector3 move = magnitude * dir;

        transform.position += move;
    }
}
