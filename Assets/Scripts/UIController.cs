using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
   public TMP_Dropdown        mdpMenu;
   
   public GameObject          mdpGameObject;
   
   // public Policy CurrentPolicy;

   public Canvas             mainUserInterface;

   public string[]           CurrentPolicy;

   private MdpManager        _mdpManager;

   public HorizontalSelector uiMdpSelector;

   
   public RadialSlider       gammaSlider;
   
   
   public TextMeshProUGUI    thetaValue;

   public Slider             thetaSlider;
   

   public Slider             maxIterationsSlider;

   public TextMeshProUGUI    maxIterationsValue;

   public GameObject         maxIterationsControlPanel;
   
   

   // ╔═════════╗
   // ║ Methods ║
   // ╚═════════╝

   private void Awake()
   {
      _mdpManager = mdpGameObject.GetComponent<MdpManager>();
   }

   public void LoadMdpFromDropdown()
   {

      var existingStateSpace = GameObject.FindGameObjectWithTag("State Space");

      if (existingStateSpace != null)
      {
         Destroy(existingStateSpace);
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
         string action = textInput[index].ToString().ToLower() ?? throw new ArgumentNullException("textInput[index].ToString().ToUpper()");
         newPolicy.SetAction(index, action);
      }
      
      var policyDisplay = GameObject.Find("ActualPolicy");
      
      policyDisplay.GetComponent<TextMeshProUGUI>().text = newPolicy.StringRepresentationOfPolicy();

      newPolicy.PrintPolicyToDebugLog();
      
      CurrentPolicy = newPolicy.PolicyToStringArray(_mdpManager.mdp.States);

      _mdpManager.CurrentPolicy = newPolicy;
   }

   public void EvaluatePolicy()
   {
      _mdpManager.EvaluateAndVisualizeStateValues();
   }

   public void ShowActionSpritesAtopStateValueVisuals()
   {
      StartCoroutine(_mdpManager.ShowActionSpritesAtopStateValueVisuals());
   }
   
   
   // ┌───────────────────────┐
   // │ MDP Parameter Related │
   // └───────────────────────┘
   public void UpdateGamma()
   {
      // if (GammaInputField != null)
      // {
      //    string uiGamma = GammaInputField.text;
      //    _mdpManager.gamma = float.Parse(uiGamma);
      // }
      // else
      // {
      //    _mdpManager.gamma = 1f;
      // }
      _mdpManager.gamma = gammaSlider.currentValue;
   }

   public void UpdateTheta()
   {
      thetaValue.text = $"1<color=#FFFFFF><sup>-{thetaSlider.value}</sup></color>";
      
      if (_mdpManager != null)
      {
         _mdpManager.theta = (1 / thetaSlider.value);
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

}
