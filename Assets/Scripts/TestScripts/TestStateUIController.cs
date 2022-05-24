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
        
    }
}
