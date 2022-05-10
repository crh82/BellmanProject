using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEditor;
using UnityEngine.Android;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
   public TMP_Dropdown       mdpMenu;
   
   public GameObject         mdpGameObject;

   public string[]           currentPolicyString;
   
   public Canvas             mainUserInterface;
   
   private MdpManager        _mdpManager;

   
   
   private readonly int[]  _playSpeedValues = new int[] {1, 10, 100, 500, 1000, 2000};

   // ╔════════════════════╗
   // ║ LEFT CONTROL PANEL ║
   // ╚════════════════════╝
   
   public HorizontalSelector uiMdpSelector;

   
   // public RadialSlider       gammaSlider;
   
   public Slider             gammaSlider;
   
   public CanvasGroup        gammaSliderContainer;
   
   public TextMeshProUGUI    gammaValue;

   
   
   public TextMeshProUGUI    thetaValue;

   public Slider             thetaSlider;
   

   public Slider             maxIterationsSlider;

   public TextMeshProUGUI    maxIterationsValue;

   public GameObject         maxIterationsControlPanel;

   private bool              _gammaIsOn;
   
   private readonly float[]  _gammaValues = new float[] 
   {
      0.0f , 0.001f , 0.01f , 0.05f  , 
      0.1f , 0.15f  , 0.2f  , 0.25f  , 
      0.3f , 0.35f  , 0.4f  , 0.45f  , 
      0.5f , 0.55f  , 0.6f  , 0.65f  , 
      0.7f , 0.75f  , 0.8f  , 0.85f  , 
      0.9f , 0.95f  , 0.99f , 0.999f
   };
   
   // ╔═════════════════════╗
   // ║ RIGHT CONTROL PANEL ║
   // ╚═════════════════════╝

   public Slider             algorithmExecutionSpeedSlider;

   public TextMeshProUGUI    algorithmExecutionSpeedValue;

   public TextMeshProUGUI    numberOfIterationsDisplay;

   public HorizontalSelector algorithmViewLevelSelector;

   public HorizontalSelector algorithmSelector;
   
   // ╔═════════════════════════════════════════════════╗
   // ║ CENTER CONTROL PANEL — STATE INFORMATION WINDOW ║
   // ╚═════════════════════════════════════════════════╝
   public ModalWindowManager stateInformationWindow;
   
   public HorizontalSelector actionEdit;

   public GameObject         actionToEditPanel;

   private int               _currentStateToEdit;

   public TMP_InputField     rewardEditor;
   
   // ╔═══════════════╗
   // ║ ASYNC RELATED ║
   // ╚═══════════════╝
   private CancellationTokenSource  _cancellationTokenSource;

   // ╔═════════╗
   // ║ Methods ║
   // ╚═════════╝

   private void Awake()
   {
      _mdpManager = mdpGameObject.GetComponent<MdpManager>();
      _mdpManager.algorithmViewLevel = algorithmViewLevelSelector.defaultIndex;
      _cancellationTokenSource = new CancellationTokenSource();
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Escape))
      {
         _mdpManager.SetKeepGoingFalse();
         Debug.Log("Escape key was pressed");
      }
      
      if (Input.GetKeyDown(KeyCode.KeypadPlus))
      {
         _mdpManager.playSpeed += 10;
      }

      if (Input.GetKeyDown(KeyCode.KeypadMultiply))
      {
         _mdpManager.playSpeed += 100;
      }
        
      if (Input.GetKeyDown(KeyCode.KeypadMinus))
      { 
         if (_mdpManager.playSpeed > 11) _mdpManager.playSpeed -= 10;
      } 
        
      if (Input.GetKeyDown(KeyCode.KeypadDivide))
      {
         if (_mdpManager.playSpeed > 110) _mdpManager.playSpeed -= 100;
      }
      if (Input.GetKeyDown(KeyCode.Keypad4))
      {
         if (_mdpManager.cat > 0) _mdpManager.cat--;
      }
      if (Input.GetKeyDown(KeyCode.Keypad6))
      {
         if (_mdpManager.cat < 3) _mdpManager.cat++;
      }
   }

   private void OnDisable()
   {
      _cancellationTokenSource.Cancel();
   }

   public void LoadMdpFromDropdown()
   {

      var existingStateSpace = GameObject.FindGameObjectWithTag("State Space");

      if (existingStateSpace != null)
      {
         Destroy(existingStateSpace);
         _mdpManager.ResetPolicy();
         _mdpManager.ResetCurrentStateValueFunction();
      }
      
      string mdpString;
      
      switch (uiMdpSelector.index)
      {
         case 0:
            Debug.Log("Selected Custom Gridworld.");
            break;
         case 1:
            mdpString = "Assets/Resources/TestMDPs/DrunkBonanza4x4Test.json";
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
         default:
            break;
      }
   }

   // ┌────────────────┐
   // │ Policy Related │
   // └────────────────┘
   
   public void BuildPolicyFromTestString()
   {
      string textInput = GameObject.FindGameObjectWithTag("Policy Input").GetComponent<TMP_InputField>().text.Replace(" ", "");

      if (textInput.Length != _mdpManager.mdp.StateCount) throw new ArgumentNullException("textInput", "String of Actions does not equal state space of MDP.");
      
      var newPolicy = new Policy(_mdpManager.mdp.StateCount);
      
      for (var index = 0; index < textInput.Length; index++)
      {
         string action = textInput[index].ToString().ToLower();
         newPolicy.SetAction(index, action);
      }
      
      var policyDisplay = GameObject.Find("ActualPolicy");
      
      policyDisplay.GetComponent<TextMeshProUGUI>().text = newPolicy.StringRepresentationOfPolicy();

      newPolicy.PrintPolicyToDebugLog();
      
      currentPolicyString = newPolicy.PolicyToStringArray(_mdpManager.mdp.States);

      _mdpManager.CurrentPolicy = newPolicy;

      numberOfIterationsDisplay.text = "0";
   }

   public void EvaluatePolicy()
   {
      _mdpManager.EvaluateAndVisualizeStateValues();
   }

   public void ShowActionSpritesAtopStateValueVisuals()
   {
      _mdpManager.ShowActionSpritesAtopStateValueVisuals();
   }
   

   // ╔═══════════════════════════╗
   // ║ Algorithm Control Related ║
   // ╚═══════════════════════════╝
   

   // Controls the execution of policy evaluation by state space sweep, individual state, or individual transition.
   public void SendLevelInformationToMdp()
   {
      _mdpManager.algorithmViewLevel = algorithmViewLevelSelector.index;
      Debug.Log(_mdpManager.algorithmViewLevel);
   }
   
   public void UpdateAlgorithmExecutionSpeedValue()
   {
      var algSpeed = (int) algorithmExecutionSpeedSlider.value;
      
      algorithmExecutionSpeedValue.text = $"<color=#FFFFFF>{algSpeed}</color>ms";
      
      if (_mdpManager != null)
      {
         // _mdpManager.algorithmExecutionSpeed = algSpeed;
         _mdpManager.playSpeed = algSpeed;
      }
   }

   public void RunAlgorithm()
   {
      var cancellationToken = _cancellationTokenSource.Token;
      
      switch (algorithmSelector.index)
      {
         case 0:
            EvaluatePolicyFullControlAsync(cancellationToken);
            break;
         case 1:
            ImprovePolicyFullControlAsync(cancellationToken);
            break;

      }
   }

   public void PauseAlgorithm()
   {
      if (_mdpManager.paused)
      {
         _mdpManager.algorithmViewLevel = algorithmViewLevelSelector.index;
         _mdpManager.paused = false;
      }
      else
      {
         _mdpManager.paused = true;
         _mdpManager.algorithmViewLevel = -1;
      }
   }
   
   public void StopPolicyEvaluation()
   {
      _cancellationTokenSource.Cancel();
      _cancellationTokenSource.Dispose();
      _cancellationTokenSource = new CancellationTokenSource();
   }
   
   
   // ┌───────────────────────────┐
   // │ Policy Evaluation Control │
   // └───────────────────────────┘

   private async void EvaluatePolicyFullControlAsync(CancellationToken cancellationToken)
   {

      _mdpManager.EnsureMdpAndPolicyAreNotNull();
      
      await _mdpManager.ShowActionSpritesAtopStateValueVisuals();
      
      try
      {
         await _mdpManager.PolicyEvaluationControlAsync(cancellationToken);
      }
      catch
      {
         Debug.Log("Algorithm execution cancelled with cancellation token.");
      }
   }

   private async void ImprovePolicyFullControlAsync(CancellationToken cancellationToken)
   {
      try
      {
         await _mdpManager.PolicyImprovementControlledAsync(cancellationToken);
      }
      catch
      {
         Debug.Log("Algorithm execution cancelled with cancellation token.");
      }
   }

   

   
   // ┌───────────────────────┐
   // │ MDP Parameter Related │
   // └───────────────────────┘
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
   
   // ┌───────────────────────────────────┐
   // │ Display and Edit State and Policy │
   // └───────────────────────────────────┘

   public async void OpenStateInformationEditorAndDisplay(int stateIndex)
   {
      stateInformationWindow.useCustomValues = true;
      
      stateInformationWindow.OpenWindow();
      
      var stateInfoFromManager = _mdpManager.GetStateAndActionInformationForDisplayAndEdit(stateIndex);
      
      stateInformationWindow.windowTitle.text = stateInfoFromManager["state"] + " Information";
      
      var displayInfo = new StringBuilder(string.Join("\n", stateInfoFromManager.Values));
      
      stateInformationWindow.windowDescription.text = displayInfo.ToString();
      
      await Task.Yield();
   }
   
   public void SetStateToEdit(int stateIndex)
   {
      _currentStateToEdit = stateIndex;
      Debug.Log(_currentStateToEdit);
   }

   public void EditPolicyActionInSelectedState()
   {
      if (_mdpManager.CurrentPolicy == null) return;

      actionEdit.defaultIndex = (int) _mdpManager.CurrentPolicy.GetAction(_currentStateToEdit);
      
      actionToEditPanel.SetActive(true);
   }

   public void CommitPolicyEdit()
   {
      _mdpManager.EditCurrentPolicy(_currentStateToEdit, actionEdit.index);
      _mdpManager.SetActionImage(_currentStateToEdit, actionEdit.index);
      actionToEditPanel.SetActive(false);
   }

   public void EditReward()
   {
      float newReward = float.Parse(rewardEditor.text);
      _mdpManager.EditRewardOfState(_currentStateToEdit, newReward);
   }
}
