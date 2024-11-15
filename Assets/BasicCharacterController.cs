using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCharacterController : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        rb.velocity = new Vector3(8f*x, rb.velocity.y, 8f*y);

        if(Input.GetKeyDown(KeyCode.Space)) {
            rb.velocity += new Vector3(0, 10f, 0);
        }
    }
}
