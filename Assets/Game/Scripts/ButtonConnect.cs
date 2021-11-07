using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    /// <summary>
    /// attached to the template button to connect to the server.
    /// TODO: need to close the menu after connecting 
    /// </summary>
    public class ButtonConnect : MonoBehaviour
    {
        private Text _text;
        // Start is called before the first frame update
        void Start()
        {
            _text = GetComponentInChildren<Text>();
        }

        public void ConnectToServer()
        {
            if (_text)
            {
                MyNetworkManager.Instance.networkAddress = _text.text;
                MyNetworkManager.Instance.StartClient();
            }
        }
    
        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
