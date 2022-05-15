using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleMove : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var position = transform.position;
        position = Vector3.Lerp(position, new Vector3(0, 0, 1000), Time.deltaTime/2);
        transform.position = position;
    }
}
