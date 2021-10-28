using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    /// <summary>
    /// very simple class to set a text value of a UI Text object from the inspector events
    /// </summary>
    public class TextUpdater : MonoBehaviour
    {
        
        /// <summary>
        /// the the text property of the text component of this game object
        /// </summary>
        /// <param name="value"></param>
        public void SetTextValue(float value)
        {
            var textBox = gameObject.GetComponent<Text>();
            if (textBox) textBox.text = value.ToString();
        }
    }
}