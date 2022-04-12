using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string jsonFile;
    // Start is called before the first frame update
    void Start()
    {
        // TestJson dude = gameObject.AddComponent<TestJson>();
        // dude.Name = "Larry";
        //
        // string message = dude.SaveToString();
        // Debug.Log(message);
        // string anotherMessage = JsonUtility.ToJson(dude);
        //
        // Debug.Log(anotherMessage);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class TestJson : MonoBehaviour
{
    public string Name { get; set; }
    
    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }

    public static TestJson CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<TestJson>(jsonString);
    }
}