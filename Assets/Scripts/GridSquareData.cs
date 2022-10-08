using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridSquareData : MonoBehaviour
{
    public Canvas dataCanvas;
    public TextMeshProUGUI gsCoordinate;
    public TextMeshProUGUI gsIndex;

    public void SetGridSquareVisualInformation(int x, int y, int index)
    {
        gsCoordinate.text = $"({x}, {y})";
        gsIndex.text = $"{index}";
    }

    public void ToggleVisibility() => dataCanvas.gameObject.SetActive(!dataCanvas.gameObject.activeSelf);
    
    public void ToggleOn() => dataCanvas.gameObject.SetActive(true);
    
    public void ToggleOff() => dataCanvas.gameObject.SetActive(false);
}
