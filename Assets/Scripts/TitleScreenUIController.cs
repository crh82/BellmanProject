using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;

public class TitleScreenUIController : MonoBehaviour
{
    public ModalWindowManager helpWindow;

    public ModalWindowManager aboutWindow;
    
    private void Start()
    {
        GameManager.instance.currentScene = (int) BellmanScenes.Title;
    }

    
    public void LoadGridBuilderScene() => GameManager.instance.SwitchScene(BellmanScenes.MdpBuilder);

    public void LoadSolver() => GameManager.instance.SwitchScene(BellmanScenes.MdpSolver);

    public void OpenHelpWindow() => helpWindow.OpenWindow();
    
    public void OpenAboutWindow() => aboutWindow.OpenWindow();

    public void ApplicationQuit() => GameManager.instance.ApplicationQuit();
}
