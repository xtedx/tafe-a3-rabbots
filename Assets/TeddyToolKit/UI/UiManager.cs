using TeddyToolKit.Core;
using UnityEngine;

namespace TeddyToolKit.UI
{
    /// <summary>
    /// Essential Game UI methods
    /// </summary>
    public class UiManager: MonoSingleton<UiManager>
    {
        /// <summary>
        /// Drag the Menu GUI GameObject here for the UIManager to manage
        /// </summary>
        [SerializeField] 
        [Tooltip("Drag the Menu GUI GameObject here for the UIManager to manage")]
        private GameObject menuGUI;
        
        /// <summary>
        /// toggles the display of menu
        /// </summary>
        public void ToggleMenu(GameObject gameObject)
        {
            var current = gameObject.activeSelf;
            gameObject.SetActive(!current);
        }
        
        /// <summary>
        /// catches keypresses related for the UI, usually the main menu
        /// </summary>
        public void UIKeyPress()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenu(menuGUI);
            }
        }
    }
}
