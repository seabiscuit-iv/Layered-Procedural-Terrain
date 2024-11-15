using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxRotate : MonoBehaviour
{
    public Transform target;
    float maxSpeed = 5f;


    // Update is called once per frame
    private void Update()
    {
        Quaternion myRot = transform.rotation;
        Quaternion targetRot = target.rotation;

        
        transform.rotation = Quaternion.RotateTowards(myRot, targetRot, maxSpeed * Time.deltaTime);
        
    }
}
