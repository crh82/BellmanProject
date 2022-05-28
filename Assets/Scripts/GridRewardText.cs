using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridRewardText : MonoBehaviour
{
    public GameObject rewardTextObject;
    public TextMeshProUGUI rewardText;
    public Canvas canvas;
    public float RewardFloat { get; set; }
    public int index;
    public Vector3Int coordinates;

    public TextMeshProUGUI RewardText
    {
        get => rewardText;
        set => rewardText = value;
    }

    private void Awake()
    {
        canvas.worldCamera = Camera.main;
        rewardText.text = "0";
        RewardFloat = 0;
    }

    // Update is called once per frame
    void Update()
    {
    
        
    }

    public void SetValue(string value)
    {
        try
        {
            RewardFloat = float.Parse(value);
            rewardText.text = value;
        }
        catch (Exception e)
        {
            Debug.Log(e.StackTrace);
            rewardText.text = "NaN";
        }
    }
}
