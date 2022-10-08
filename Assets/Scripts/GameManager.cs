using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static  GameManager         instance;
    
    // These are set to null unless a scene is sending one or of them between scenes.
    public MDP                 currentMdp;
    public Policy              currentPolicy;
    public StateValueFunction  currentStateValueFunction;
    public ActionValueFunction currentActionValueFunction;
    public bool                sendMdp;
    public bool                sendPolicy;
    public bool                sendStateValueFunction;
    public bool                sendActionValueFunction;

    private MdpManager         _mdpManager;
    
    private const  BellmanScenes  Title       = BellmanScenes.Title;
    private const  BellmanScenes  MdpSolver   = BellmanScenes.MdpSolver;
    private const  BellmanScenes  MdpBuilder  = BellmanScenes.MdpBuilder;
    private const  BellmanScenes  DP2         = BellmanScenes.Dp2;

    public int currentScene;

    public GameObject  cursorTrailObject;
    public CursorTrail cursorTrail;
    public Camera      solverMainCamera;
    public Camera      solverTopDownCamera;


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

            if (currentScene > 2) currentScene = 0;
           
            instance.SwitchScene((BellmanScenes) currentScene);          
        }
    }

    public void ApplicationQuit() => Application.Quit();

    /// <summary>
    /// The SwitchScene method loads the scene specified by the bellmanScene parameter. The currentScene variable is
    /// used to keep track of which scene is currently loaded.
    /// </summary>
    ///
    /// <param name="bellmanScene"> The scene to switch to</param>
    public void SwitchScene(BellmanScenes bellmanScene)
    {
        switch (bellmanScene)
        {
            case Title:
                currentScene = 0;
                SceneManager.LoadScene(0);
                break;
            case MdpSolver:
                currentScene = 1;
                SceneManager.LoadScene(1);
                break;
            case MdpBuilder:
                currentScene = 2;
                SceneManager.LoadScene(2);
                break;
            case DP2:
                currentScene = 3;
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
    MdpSolver          = 1,
    MdpBuilder         = 2,
    Dp2                = 3
}