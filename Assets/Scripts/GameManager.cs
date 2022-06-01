using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static  GameManager         instance;
    
    // These are set to null unless a scene is sending one or of them between scenes.
    public         MDP                 currentMdp;
    public         Policy              currentPolicy;
    public         StateValueFunction  currentStateValueFunction;
    public         ActionValueFunction currentActionValueFunction;
    public bool                        sendMdp;
    public bool                        sendPolicy;
    public bool                        sendStateValueFunction;
    public bool                        sendActionValueFunction;
    
    private const  BellmanScenes       Title              = BellmanScenes.Title;
    private const  BellmanScenes       DynamicProgramming = BellmanScenes.DynamicProgramming;
    private const  BellmanScenes       MdpBuilder         = BellmanScenes.MdpBuilder;

    private void Awake()
    {
        // Generates the singleton architecture
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && SceneManager.GetActiveScene().name == "TitleMenuScene")
        {
            instance.SwitchScene(BellmanScenes.MdpBuilder);          
        }
        if (Input.GetKeyDown(KeyCode.L) && SceneManager.GetActiveScene().name == "TitleMenuScene")
        {
            instance.SwitchScene(BellmanScenes.DynamicProgramming);          
        }
    }

    public void ApplicationQuit() => Application.Quit();

    public void SwitchScene(BellmanScenes bellmanScene)
    {
        switch (bellmanScene)
        {
            case Title:
                SceneManager.LoadScene("Scenes/TitleMenuScene");
                break;
            case DynamicProgramming:
                SceneManager.LoadScene("Scenes/MarkovDecisionProcess");
                break;
            case MdpBuilder:
                SceneManager.LoadScene("Scenes/MdpBuilder");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(bellmanScene), bellmanScene, null);
        }        
    }
}

public enum BellmanScenes
{
    Title              = 0,
    DynamicProgramming = 1,
    MdpBuilder         = 2
}