using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;

/// <summary>
/// <para></para>
/// <para>
/// <b>Author:</b> <i>Christopher Howell</i>
/// </para>
/// <para>
/// <b>Citations:</b>
/// </para>
/// </summary>
public class UIController : MonoBehaviour
{
   // ╔════════════════════╗
   // ║ GENERAL OR UI WIDE ║
   // ╚════════════════════╝
   public TMP_Dropdown       mdpMenu;
   
   public GameObject         mdpGameObject;

   public string[]           currentPolicyString;
   
   public Canvas             mainUserInterface;
   
   private MdpManager        _mdpManager;
   
   private readonly int[]    _algorithmDelayValues = {1, 10, 100, 300, 500, 1000};

   public TooltipManager     mouseHoverHelper;

   private bool              _mouseHoverHelpOn;

   public TextMeshProUGUI    toolTipsOnText;
   

   // ╔════════════════════╗
   // ║ LEFT CONTROL PANEL ║
   // ╚════════════════════╝
   
   public HorizontalSelector uiMdpSelector;

   public Button             resetMdpButton;

   public GameObject         resetButtonContainer;

   
   public Slider             gammaSlider;
   
   public CanvasGroup        gammaSliderContainer;
   
   public TextMeshProUGUI    gammaValue;

   
   
   public TextMeshProUGUI    thetaValue;

   public Slider             thetaSlider;
   

   public Slider             maxIterationsSlider;

   public TextMeshProUGUI    maxIterationsValue;

   public GameObject         maxIterationsControlPanel;

   private bool              _gammaIsOn;
   
   private readonly float[]  _gammaValues = 
   {
      0.0f , 0.001f , 0.01f , 0.05f  , 
      0.1f , 0.15f  , 0.2f  , 0.25f  , 
      0.3f , 0.35f  , 0.4f  , 0.45f  , 
      0.5f , 0.55f  , 0.6f  , 0.65f  , 
      0.7f , 0.75f  , 0.8f  , 0.85f  , 
      0.9f , 0.95f  , 0.99f
   };
   
   // ╔═════════════════════╗
   // ║ RIGHT CONTROL PANEL ║
   // ╚═════════════════════╝

   public Slider             algorithmExecutionSpeedSlider;

   public TextMeshProUGUI    algorithmExecutionSpeedValue;

   public HorizontalSelector algorithmViewLevelSelector;

   public HorizontalSelector algorithmSelector;
   
   public SwitchManager      focusAndFollowToggle;

   public List<GameObject>   focusAndFollowUIObjects;
   
   public Button             runButton;

   public GameObject         runButtonContainer;

   private DropdownMultiSelect _settingsItemsDropdown;

   public GameObject         settingsItemsDropdownPanel;
   
   
   // ╔════════════════════════════════════╗
   // ║ CENTER CONTROL PANEL & INFORMATION ║
   // ╚════════════════════════════════════╝
   
   public TextMeshProUGUI    algorithmTitleText;
   
   public TextMeshProUGUI    maxDelta;
   
   public TextMeshProUGUI    numberOfIterationsDisplay;
   
   public GameObject         pauseBanner;
   
   public GameObject         progressBar;
   
   public ProgressBar        progressPercentageBar;
   
   
   // ╔══════════════════════════╗
   // ║ STATE INFORMATION WINDOW ║
   // ╚══════════════════════════╝
   public HorizontalSelector actionEdit;

   public GameObject         actionToEditPanel;
   
   private int               _currentStateToEdit;
   
   public TMP_InputField     rewardEditor;
   
   public ModalWindowManager stateInformationWindow;

   private const string      RegexRealNumber =
      @"/^(?:-(?:[1-9](?:\d{0,2}(?:,\d{3})+|\d*))|(?:0|(?:[1-9](?:\d{0,2}(?:,\d{3})+|\d*))))(?:.\d+|)$/";
   
   // ╔═════════════════════╗
   // ║ MAIN CAMERA CONTROL ║
   // ╚═════════════════════╝
   public CameraController              mainCameraController;
   
   public GameObject                    mainCameraRig;
   
   // ╔═══════════════════════╗
   // ║ CORNER CAMERA CONTROL ║
   // ╚═══════════════════════╝
   private CornerCameraController        _focusCam;

   public GameObject                     cornerCameraRig;
   
   public GameObject                     cursorTrailObject;
   
   public CursorTrail                    cursorTrail;
   
   public Camera                         solverMainCamera;
   
   public Camera                         solverTopDownCamera;

   // ╔═══════════════╗
   // ║ ASYNC RELATED ║
   // ╚═══════════════╝
   private CancellationTokenSource        _cancellationTokenSource;


   // ———————————————————————————————————————————————————————
   //                       ╔═════════╗
   //                       ║ Methods ║
   //                       ╚═════════╝
   // ———————————————————————————————————————————————————————
   
   private void Awake()
   {
                         _mdpManager = mdpGameObject.GetComponent<MdpManager>();
      _mdpManager.algorithmViewLevel = algorithmViewLevelSelector.defaultIndex;
            _cancellationTokenSource = new CancellationTokenSource();
                           _focusCam = cornerCameraRig.GetComponent<CornerCameraController>();
                mainCameraController = mainCameraRig.GetComponent<CameraController>();
                //    _mouseHoverHelpOn = false;
                // mouseHoverHelper.gameObject.SetActive(false);
                // toolTipsOnText.gameObject.SetActive(false);

                GameManager.instance.currentScene = (int) BellmanScenes.DynamicProgramming;
   }

   private void Update()
   {
      // if (Input.GetKeyDown(KeyCode.Escape))
      // {
      //    _mdpManager.SetMainLoopBoolConditionFalse();
      //    // Debug.Log("Escape key was pressed");
      // }
      //
      // if (Input.GetKeyDown(KeyCode.Alpha1))
      // {
      //    ToggleActionsVisuals("ActionObjects");
      // }
      // if (Input.GetKeyDown(KeyCode.Alpha2))
      // {
      //    ToggleActionsVisuals("ActionSprites");
      // }
      // if (Input.GetKeyDown(KeyCode.Alpha3))
      // {
      //    ToggleActionsVisuals("PreviousActionSprites");
      // }
      
      if (Input.GetKeyDown(KeyCode.Escape)) _mdpManager.SetMainLoopBoolConditionFalse();
        
      if (Input.GetKeyDown(KeyCode.Alpha1)) _mdpManager.Toggle("GridSquare");
       
      if (Input.GetKeyDown(KeyCode.Alpha2)) _mdpManager.Toggle("StateValue");
        
      if (Input.GetKeyDown(KeyCode.Alpha3)) _mdpManager.Toggle("StateValueText");
        
      if (Input.GetKeyDown(KeyCode.Alpha4)) _mdpManager.Toggle("ActionObjects");
        
      if (Input.GetKeyDown(KeyCode.Alpha5)) _mdpManager.Toggle("ActionSprites");

      if (Input.GetKeyDown(KeyCode.Alpha6)) _mdpManager.Toggle("PreviousActionSprites");
      
   }

   private void OnDisable()
   {
      _cancellationTokenSource.Cancel();
   }

   public void LoadMdpFromDropdown()
   {

      ResetGridWorldAsync();

      string mdpString;
      
      switch (uiMdpSelector.index)
      {
         case 0:
            Debug.Log("Selected Custom Gridworld.");
            break;
         case 1:
            mdpString = "Assets/Resources/TestMDPs/GrastiensWorld.json";
            _mdpManager.LoadMdpFromFilePath(mdpString);
            break;
         case 2:
            mdpString = "Assets/Resources/TestMDPs/FrozenLake4x4Test.json";
            _mdpManager.LoadMdpFromFilePath(mdpString);
            break;
         case 3:
            mdpString = "Assets/Resources/TestMDPs/LittleTestWorldTest.json";
            _mdpManager.LoadMdpFromFilePath(mdpString);
            break;
         case 4:
            mdpString = "Assets/Resources/TestMDPs/RussellNorvigGridworldTest.json";
            _mdpManager.LoadMdpFromFilePath(mdpString);
            break;
         case 5:
            mdpString = "Assets/Resources/TestMDPs/MonsterWorld.json";
            _mdpManager.LoadMdpFromFilePath(mdpString);
            break;
         case 6:
            mdpString = "Assets/Resources/TestMDPs/WidowMaker.json";
            _mdpManager.LoadMdpFromFilePath(mdpString);
            break;
         case 7:
            mdpString = "Assets/Resources/TestMDPs/BigRandomWalk.json";
            _mdpManager.LoadMdpFromFilePath(mdpString);
            break;
         case 8:
            mdpString = "Assets/Resources/TestMDPs/BloodMoon.json";
            _mdpManager.LoadMdpFromFilePath(mdpString);
            break;
         case 9:
            mdpString = "Assets/Resources/TestMDPs/TestFromGridEditor.json";
            _mdpManager.LoadMdpFromFilePath(mdpString);
            break;
         default:
            break;
      }
      
     InitializeSettingsPanel();
   }

   private async void ResetGridWorldAsync()
   {
      var existingStateSpace = GameObject.FindGameObjectWithTag("State Space");

      if (existingStateSpace != null)
      {
         await StopAlgorithmAsync();
         
         _mdpManager.ResetPolicy();
         _mdpManager.ResetCurrentStateValueFunction();
         _mdpManager.ResetStateQuadDictionary();
         
         Destroy(existingStateSpace);
         
         await UpdateNumberOfIterationsAsync(0);
      }
   }

   // ┌──────────────────────┐
   // │ Central Area Related │
   // └──────────────────────┘
   public void SetAlgorithmTitleText(string algorithmTitle) => algorithmTitleText.text = algorithmTitle;

   // ┌────────────────┐
   // │ Policy Related │
   // └────────────────┘
   
   public void BuildPolicyFromTestString()
   {
      string textInput = GameObject.FindGameObjectWithTag("Policy Input").GetComponent<TMP_InputField>().text.Replace(" ", "");

      if (textInput.Length != _mdpManager.Mdp.StateCount) throw new ArgumentNullException("textInput", "String of Actions does not equal state space of MDP.");
      
      var newPolicy = new Policy(_mdpManager.Mdp.StateCount);
      
      for (var index = 0; index < textInput.Length; index++)
      {
         string action = textInput[index].ToString().ToLower();
         newPolicy.SetAction(index, action);
      }
      
      var policyDisplay = GameObject.Find("ActualPolicy");
      
      policyDisplay.GetComponent<TextMeshProUGUI>().text = newPolicy.StringRepresentationOfPolicy();

      newPolicy.PrintPolicyToDebugLog();
      
      currentPolicyString = newPolicy.PolicyToStringArray(_mdpManager.Mdp.States);

      _mdpManager.currentPolicy = newPolicy;

      numberOfIterationsDisplay.text = "0";
   }
   
   public void ShowActionSpritesAtopStateValueVisuals()
   {
      _mdpManager.ShowActionSpritesAtopStateValueVisuals();
   }
   
   // ┌───────────────────────┐
   // │ Corner Camera Methods │
   // └───────────────────────┘

   public Task FocusCornerCamera(int stateIndex)
   {
      cornerCameraRig.SetActive(true);
      _focusCam.FocusOn(_mdpManager.StateQuads[stateIndex]);
      return Task.CompletedTask;
   }

   // ┌───────────────────────────┐
   // │ Algorithm Control Methods │
   // └───────────────────────────┘
   

   // Controls the execution of policy evaluation by state space sweep, individual state, or individual transition.
   public void SendLevelInformationToMdp()
   {
      _mdpManager.algorithmViewLevel = algorithmViewLevelSelector.index;
   }
   
   public void UpdateAlgorithmExecutionSpeedValue()
   {

      int algorithmDelay = _algorithmDelayValues[(int) algorithmExecutionSpeedSlider.value];
      
      algorithmExecutionSpeedValue.text = $"<color=#FFFFFF>{algorithmDelay}</color>ms";
      
      if (_mdpManager != null)
      {
         _mdpManager.playSpeed = algorithmDelay;
      }
   }

   public void RunAlgorithm()
   {
      if (!_mdpManager.mdpLoaded) return;
      
      var cancellationToken = _cancellationTokenSource.Token;
      
      switch (algorithmSelector.index)
      {
         case 0:
            if (_mdpManager.focusAndFollowMode) EvaluatePolicyFullControlAsync(cancellationToken);
            else EvaluatePolicyNoDelay(cancellationToken);
            break;
         case 1:
            ImprovePolicyFullControlAsync(cancellationToken);
            break;
         case 2:
            PolicyIterateFullControllAsync(cancellationToken);
            break;
         case 3:
            ValueIterateFullControlAsync(cancellationToken);
            break;
      }
   }

   public void AdvanceStep() => _mdpManager.stepped = true;

   public void PauseAlgorithm()
   {
      if (_mdpManager.paused)
      {
         _mdpManager.algorithmViewLevel = algorithmViewLevelSelector.index;
         _mdpManager.paused = false;
         pauseBanner.SetActive(false);
      }
      else
      {
         _mdpManager.paused = true;
         _mdpManager.algorithmViewLevel = -1;
         pauseBanner.SetActive(true);
      }
   }
   
   public void StopAlgorithm()
   {
      _mdpManager.DisableRabbit();
      _mdpManager.SetMainLoopBoolConditionFalse();
      _cancellationTokenSource.Cancel();
      _cancellationTokenSource.Dispose();
      _cancellationTokenSource = new CancellationTokenSource();

      if (_mdpManager.paused) // If stopped while paused this unpauses
      {
         PauseAlgorithm();
      }
      
      SetRunFeaturesActive();
   }
   
   public Task StopAlgorithmAsync()
   {
      _mdpManager.DisableRabbit();
      _mdpManager.SetMainLoopBoolConditionFalse();
      _cancellationTokenSource.Cancel();
      _cancellationTokenSource.Dispose();
      _cancellationTokenSource = new CancellationTokenSource();

      if (_mdpManager.paused) // If stopped while paused this unpauses
      {
         PauseAlgorithm();
      }
      SetRunFeaturesActive();
      return Task.CompletedTask;
   }

   public void EnableFocusAndFollow() => _mdpManager.focusAndFollowMode = true;

   public void DisableFocusAndFollow() => _mdpManager.focusAndFollowMode = false;

   /// <summary>
   /// <para>
   /// Disables any features in the UI that can crash the system if clicked during algorithm execution.
   /// </para>
   /// </summary>
   public async void DisableRunFeatures()
   {
      await Task.Delay(100);
      await ButtonVisibility(runButtonContainer,0.3f);
      await SetButtonInteractableOff(runButton); // OFF
      
      progressBar.SetActive(true);
      progressPercentageBar.isOn = true;
      progressPercentageBar.currentPercent = 0f;
      
      await ButtonVisibility(resetButtonContainer,0.3f);
      await SetButtonInteractableOff(resetMdpButton); // OFF
   
   }

   public async void SetRunFeaturesActive()
   {
      await Task.Delay(100);
      await ButtonVisibility(runButtonContainer, 1f);
      await SetButtonInteractableOn(runButton); // ON
      progressBar.SetActive(false);
      progressPercentageBar.isOn = false;
      
      await ButtonVisibility(resetButtonContainer, 1f);
      await SetButtonInteractableOn(resetMdpButton); // ON
   }

   private Task ButtonVisibility(GameObject buttonContainer, float alpha)
   {
      buttonContainer.GetComponent<CanvasGroup>().alpha = alpha;
      return Task.CompletedTask;
   }

   private Task SetButtonInteractableOff(Button button)
   {
      button.interactable = false;
      return Task.CompletedTask;
   }
   
   private Task SetButtonInteractableOn(Button button)
   {
      button.interactable = true;
      return Task.CompletedTask;
   }

   private async void EvaluatePolicyNoDelay(CancellationToken cancellationToken)
   {
      try
      {
         await _mdpManager.PolicyEvaluationNoDelay(cancellationToken);
      }
      catch (TaskCanceledException)
      {
         Debug.Log("Policy evaluation execution cancelled with cancellation token.");
      }
   }

   private async void EvaluatePolicyFullControlAsync(CancellationToken cancellationToken)
   {
      // _mdpManager.EnsureMdpAndPolicyAreNotNull();
      
      try
      {
         await _mdpManager.PolicyEvaluationControlAsync(cancellationToken);
      }
      catch (TaskCanceledException)
      {
         Debug.Log("Policy evaluation execution cancelled with cancellation token.");
      }
   }

   private async void ImprovePolicyFullControlAsync(CancellationToken cancellationToken)
   {
      try
      {
         await _mdpManager.PolicyImprovementControlledAsync(cancellationToken);
      }
      catch (TaskCanceledException)
      {
         Debug.Log("Policy improvement execution cancelled with cancellation token.");
      }
   }

   private async void PolicyIterateFullControllAsync(CancellationToken cancellationToken)
   {

      try
      {
         await _mdpManager.PolicyIterationControlledAsync(cancellationToken);
      }
      catch (TaskCanceledException)
      {
         Debug.Log("Policy iteration execution cancelled with cancellation token.");
      }
   }

   private async void ValueIterateFullControlAsync(CancellationToken cancellationToken)
   {
      try
      {
         await _mdpManager.ValueIterationControlledAsync(cancellationToken);
      }
      catch (TaskCanceledException)
      {
         Debug.Log("Value iteration execution cancelled with cancellation token.");
      }
   }
   
   // ┌───────────────────────┐
   // │ MDP Parameter Methods │
   // └───────────────────────┘

   public void RandomizeStateValues() => _mdpManager.GenerateRandomStateValueFunction();

   public void RandomizePolicy() => _mdpManager.GenerateRandomPolicy();

   public void SwitchGammaOn()
   {
      _gammaIsOn = true;
      gammaSliderContainer.alpha = 1;
   }

   public void SwitchGammaOff()
   {
      _gammaIsOn = false;
      gammaSliderContainer.alpha = 0.25f;
      _mdpManager.Gamma = 1f;
   }

   public void UpdateGamma()
   {
      if (_gammaIsOn) _mdpManager.Gamma = _gammaValues[(int) gammaSlider.value];
      UpdateGameDisplay();
      // _mdpManager.gamma = gammaSlider.currentValue;
   }
   
   private void UpdateGameDisplay()
   {
      gammaValue.text =  $"<color=#FFFFFF>{_gammaValues[(int) gammaSlider.value]}</color>";
   }

   public void UpdateTheta()
   {
      thetaValue.text = $"1<color=#FFFFFF><sup>-{thetaSlider.value}</sup></color>";
      
      if (_mdpManager != null)
      {
         _mdpManager.theta = (float) (1 / Math.Pow(10, thetaSlider.value));
      }
   }

   public void MaxIterationsOn()
   {
      _mdpManager.boundIterations = true;
      // maxIterationsControlPanel.SetActive(true);
   }

   public void MaxIterationsOff()
   {
      _mdpManager.boundIterations = false;
      // maxIterationsControlPanel.SetActive(false);
   }

   public void UpdateMaxIterations()
   {
      var maxIterationsExponent = (double) maxIterationsSlider.value;
      
      maxIterationsValue.text = $"10<color=#FFFFFF><sup>{(int) maxIterationsExponent}</sup></color>";
      
      if (_mdpManager != null)
      {
         _mdpManager.maximumIterations = (int) Math.Pow(10, maxIterationsExponent);
         
         Debug.Log($"Max Iter: {_mdpManager.maximumIterations}");
      }
      
   }

   public void UpdateNumberOfIterations(int iterations)
   {
      numberOfIterationsDisplay.text = $"{iterations}";
   }

   public Task UpdateNumberOfIterationsAsync(int iterations)
   {
      numberOfIterationsDisplay.text = $"{iterations}";

      return Task.CompletedTask;
   }

   public void SetProgressBarPercentage(float value)
   {
      progressPercentageBar.currentPercent = value;
   }

   public void SetMaxDeltaText(float value)
   {
      maxDelta.text = $"{value}";
   }

   public Task SetMaxDeltaTextAsync(float value)
   {
      maxDelta.text = $"{value}";
      return Task.CompletedTask;
   }
   
   // ┌───────────────────────────────────────────┐
   // │ Display and Edit State and Policy Methods │
   // └───────────────────────────────────────────┘

   public async void OpenStateInformationEditorAndDisplay(int stateIndex)
   {
      _mdpManager.ToggleStateHighlight(stateIndex);
      
      stateInformationWindow.useCustomValues = true;

      // stateInformationWindow.transform.position = Input.mousePosition;
      
      stateInformationWindow.OpenWindow();
      
      SetStateInformationWindowText(stateIndex);

      await Task.Yield();
   }

   private void SetStateInformationWindowText(int stateIndex)
   {
      var stateInfoFromManager = _mdpManager.GetStateAndActionInformationForDisplayAndEdit(stateIndex);

      stateInformationWindow.windowTitle.text = stateInfoFromManager["state"] + " Information";

      var displayInfo = new StringBuilder(string.Join("\n", stateInfoFromManager.Values));

      stateInformationWindow.windowDescription.text = displayInfo.ToString();
   }

   public void SetStateToEdit(int stateIndex) => _currentStateToEdit = stateIndex;

   public void EditPolicyActionInSelectedState()
   {
      if (_mdpManager.currentPolicy == null) return;

      actionEdit.defaultIndex = (int) _mdpManager.currentPolicy.GetAction(_currentStateToEdit);
      
      actionToEditPanel.SetActive(true);
   }

   public void CommitPolicyEdit()
   {
      _mdpManager.EditCurrentPolicy(_currentStateToEdit, actionEdit.index);
      _mdpManager.SetActionImage(_currentStateToEdit, actionEdit.index);
      actionToEditPanel.SetActive(false);
   }

   public void SetEditRewardTextRedIfDirty()
   {
      if (!Regex.IsMatch(rewardEditor.textComponent.text, RegexRealNumber))
         rewardEditor.textComponent.color = Color.red;
      else
         rewardEditor.textComponent.color = Color.HSVToRGB(152, 98, 75);
   }
   
   public void EditReward()
   {
      // if (!Regex.IsMatch(rewardEditor.text, RegexRealNumber)) return;
      
      float newReward = float.Parse(rewardEditor.text);
      _mdpManager.EditRewardOfState(_currentStateToEdit, newReward);
      SetStateInformationWindowText(_currentStateToEdit);
   }

   public void SetRewardTextInEditStatePanelToCurrentEditableStateReward()
   {
      float currentReward = _mdpManager.GetStateFromCurrentMdp(_currentStateToEdit).Reward;
      rewardEditor.text = $"{currentReward}";
   }

   // ┌──────────────────┐
   // │ Settings Methods │
   // └──────────────────┘

   public void InitializeSettingsPanel()
   {
      settingsItemsDropdownPanel.SetActive(true);

      if (_mdpManager.Mdp != null)
      {
         _settingsItemsDropdown = settingsItemsDropdownPanel.GetComponent<DropdownMultiSelect>();
         _settingsItemsDropdown.SetupDropdown();
      }
   }

   public void ToggleFocusAndFollowObjectsInRightUIControlPanel(bool toggleState)
   {
      _mdpManager.focusAndFollowMode = toggleState;
      
      foreach (var uiObject in focusAndFollowUIObjects)
      {
         uiObject.SetActive(!uiObject.activeSelf);
      }
   }
   
   // public void ToggleActionsVisuals(string toBeToggled) => _mdpManager.Toggle(toBeToggled);

   public void ApplicationQuit()
   {
      GameManager.instance.ApplicationQuit();
   }
   
   public void ToggleMouseHoverHelp()
   {
      _mouseHoverHelpOn = !_mouseHoverHelpOn;

      mouseHoverHelper.gameObject.SetActive(_mouseHoverHelpOn);
      
      toolTipsOnText.gameObject.SetActive(_mouseHoverHelpOn);
      
   }

   public void NavigateToMainMenuScreen() => GameManager.instance.SwitchScene(BellmanScenes.Title);
}
