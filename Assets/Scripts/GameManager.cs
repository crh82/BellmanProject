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

    private MdpManager                 _mdpManager;
    
    private const  BellmanScenes       Title              = BellmanScenes.Title;
    private const  BellmanScenes       DynamicProgramming = BellmanScenes.DynamicProgramming;
    private const  BellmanScenes       MdpBuilder         = BellmanScenes.MdpBuilder;
    private const  BellmanScenes       DP2                = BellmanScenes.Dp2;

    public int currentScene;

    public GameObject  cursorTrailObject;
    public CursorTrail cursorTrail;
    public Camera solverMainCamera;
    public Camera solverTopDownCamera;

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
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            currentScene += 1;

            if (currentScene > 3) currentScene = 0;
           
            instance.SwitchScene((BellmanScenes) currentScene);          
        }

        // if (Input.GetKeyDown(KeyCode.L) && SceneManager.GetActiveScene().name == "TitleMenuScene")
        // {
        //     instance.SwitchScene(BellmanScenes.DynamicProgramming);          
        // }
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
            case DP2:
                SceneManager.LoadScene(3);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(bellmanScene), bellmanScene, null);
        }        
    }

    public void SetMdpManager(MdpManager mdpManager) => _mdpManager = mdpManager;

    public UIController GetUIController() => _mdpManager.uiController;
}

public enum BellmanScenes
{
    Title              = 0,
    DynamicProgramming = 1,
    MdpBuilder         = 2,
    Dp2 = 3
}