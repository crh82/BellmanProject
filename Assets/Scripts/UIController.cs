using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class UIController : MonoBehaviour
{
   public TMP_Dropdown mdpMenu;
   
   public GameObject mdpGameObject;

   public TextMeshPro actualPolicy; // Todo improve, currently just for testing.

   // public Policy CurrentPolicy;

   public Canvas mainUserInterface;

   public string[] CurrentPolicy;

   private MdpManager _mdpManager;

   public CustomDropdown uiMdpSelector;

   public TMP_InputField GammaInputField;
   
   

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
      Debug.Log(mdpMenu.value);
   }

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
      _mdpManager.VisualizeStateValues();
   }

   public void UpdateGamma()
   {
      if (GammaInputField != null)
      {
         string uiGamma = GammaInputField.text;
         _mdpManager.gamma = float.Parse(uiGamma);
      }
      else
      {
         _mdpManager.gamma = 1f;
      }
   }

}
