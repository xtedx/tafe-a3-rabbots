using System;
using System.Collections.Generic;
using Mirror;
using TeddyToolKit.Core;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class UiManager : MonoSingleton<UiManager>
    {
        [Tooltip("the Text UI element for Timer")]
        [SerializeField]
        private Text txtTimer;
        [SerializeField]
        private Text txtNetworkStatus;
        
        [Space]
        [Tooltip("the toggle group used for the tab heads")]
        [SerializeField]
        private ToggleGroup tabGroup;
        [Tooltip("the body for each tab")]
        [SerializeField]
        private List<GameObject> tabBody;
        private int activeTabIndex; 
        
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

        private void Update()
        {
            UIKeyPress();
            NetworkStatus();
        }

        private void NetworkStatus()
        {
            txtNetworkStatus.text = NetworkManager.singleton.mode.ToString();
        }

        private void OnEnable()
        {
            RegisterListeners();
        }

        private void OnDisable()
        {
            DeregisterListeners();
        }

        #region events related

        /// <summary>
        /// listen to the event and call method when that happens
        /// </summary>
        private void RegisterListeners()
        {
            // EventManager.Instance.OnTimer1SecondTick += UpdateTimerText;
        }
        
        /// <summary>
        /// make sure to un-listen, always as a pair with the onenable
        /// </summary>
        private void DeregisterListeners()
        {
            // EventManager.Instance.OnTimer1SecondTick -= UpdateTimerText;
        }

        #endregion

        /// <summary>
        /// updates the text on the ui element
        /// </summary>
        /// <param name="value"></param>
        public void UpdateTimerText(int value)
        {
            if (txtTimer) txtTimer.text = value.ToString("00");
        }

        /// <summary>
        /// called everytime the active tab head is changed and show the corresponding body
        /// the tabs are indexed in the object name using .int from 1 and above 
        /// </summary>
        public void showSelectedTabBody()
        {
            //get the index of active tab head from the tab group and break out of loop
            var t = tabGroup.GetFirstActiveToggle();
            //split by the delimiter . in the name
            var s = t.name.Split('.');
            //then get the integer in the last element
            int.TryParse(s[(s.Length - 1)], out activeTabIndex);
            // exit if nothing is active or invalid tab
            if (activeTabIndex < 0) return;
            
            //disables all tab bodies and enable the active one
            int bodyIndex;
            foreach (var body in tabBody)
            {
                s = body.name.Split('.');
                int.TryParse(s[(s.Length - 1)], out bodyIndex);
                body.SetActive(bodyIndex == activeTabIndex);
            }
        }
    }
}