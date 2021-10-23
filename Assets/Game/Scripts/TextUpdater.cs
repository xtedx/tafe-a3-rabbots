using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class TextUpdater : MonoBehaviour
    {
        public void SetTextValue(float value)
        {
            var textBox = gameObject.GetComponent<Text>();
            if (textBox) textBox.text = value.ToString();
        }
    
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
