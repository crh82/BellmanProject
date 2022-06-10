using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionButtonWithDisplay : MonoBehaviour
{
    public GameObject       actionImageObject;
    public Image            actionImage;
    public const float      DisplayLeft  = 90f;
    public const float      DisplayDown  = 180f;
    public const float      DisplayRight = 270f;
    public const float      DisplayUp    = 0f;
    public const GridAction Left    = GridAction.Left;
    public const GridAction Down    = GridAction.Down;
    public const GridAction Right   = GridAction.Right;
    public const GridAction Up      = GridAction.Up;
    
    public void SetActionImage(float direction)
    {
        actionImage.gameObject.SetActive(true);
        actionImage.transform.rotation = Quaternion.Euler(new Vector3(0,0,direction));
    }
}
