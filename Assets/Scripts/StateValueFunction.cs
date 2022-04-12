using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateValueFunction : MonoBehaviour
{
    public float stateValue;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Value(float valuePrime)
    {
        
        Vector3 current = transform.localScale;
        transform.localScale = new Vector3(current.x, (current.y + valuePrime), current.z);
        
    }
}
