using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionTrail : MonoBehaviour
{
    public GameObject target;

    public float time;

    public bool releaseTrail;

    // Update is called once per frame
    void Update()
    {
        if (releaseTrail) SendTransitionTrailFromSuccessorStateToVsOrQsa(target.transform.position);
    }

    public void SendTransitionTrailFromSuccessorStateToVsOrQsa(Vector3 targetPosition)
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime*50);
    }

    public void TrailShot()
    {
        SendTransitionTrailFromSuccessorStateToVsOrQsa(target.transform.position);
    }
}
