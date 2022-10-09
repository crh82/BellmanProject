using Michsky.UI.ModernUIPack;
using UnityEngine;

namespace TestScripts
{
    public class TestStateUIController : MonoBehaviour
    {
        public ModalWindowManager wind;
        public GameObject stateObject;
        private State _state;

        public WindowDragger window;

        private void Awake()
        {
            _state = gameObject.GetComponentInParent<State>();
        }

        private void OnMouseDown()
        {
            stateObject.SetActive(true);
            if (_state == null)
            {
                Debug.Log("Null");
            }
        
            Debug.Log("Clicked State test");
            
            // wind.gameObject.SetActive(true);
        }
        
        public async void LoadMdpFromFilePath(string filepath)
        {
            // TODO Deal with saving and loading more effectively later.
            // string path = Path.Combine(Application.dataPath, "Resources/TestMDPs");
            // string fullPath = Path.Combine(path, filepath);
            // // string mdpJsonRepresentation = File.ReadAllText(fullPath);
            //
            // // String mdpJsonRepresentation = Resources.Load<String>(filepath);
            // TextAsset mdpFromFile = Resources.Load<TextAsset>(filepath);
            // // string mdpFromFileString = mdpFromFile.text;
            // string mdpJsonRepresentation = mdpFromFile.text;
            // // string mdpJsonRepresentation = File.ReadAllText(filepath);
            //
            // Mdp = CreateFromJson(mdpJsonRepresentation);
            //
            // _mdpForReset = mdpJsonRepresentation;
            //
            // Mdp = await InstantiateMdpVisualisationAsync(CreateFromJson(mdpJsonRepresentation));
            //
            // mdpLoaded = true;
            //
            // uiController.SetRunFeaturesActive();
        }
        
    }
}
