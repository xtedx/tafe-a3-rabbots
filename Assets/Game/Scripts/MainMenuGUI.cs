using System;
using System.Collections.Generic;
using Mirror;
using TeddyToolKit.Core;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class MainMenuGUI : MonoBehaviour
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
        
        [Space]
        [Tooltip("the button group for map choice")]
        [SerializeField]
        private GameObject mapChoice;
        
        private int activeTabIndex; 
        
        /// <summary>
        /// Drag the Menu GUI GameObject here for the UIManager to manage
        /// </summary>
        [SerializeField]
        [Tooltip("Drag the Menu GUI GameObject here for the UIManager to manage")]
        private GameObject mainPanelGUI;
        [SerializeField]
        private GameObject topPanel;
        [SerializeField]
        private GameObject topTimerBlock;
        [SerializeField]
        private GameObject bottomPanel;
        
        /// <summary>
        /// toggles the display of menu
        /// </summary>
        public void ToggleMenu(GameObject gameObject)
        {
            var current = gameObject.activeSelf;
            gameObject.SetActive(!current);
        }
        
        /// <summary>
        /// toggles the display of the PanelMain menu
        /// </summary>
        public void ToggleMenu()
        {
            var current = mainPanelGUI.activeSelf;
            mainPanelGUI.SetActive(!current);
        }
        
        /// <summary>
        /// catches keypresses related for the UI, usually the main menu
        /// </summary>
        public void UIKeyPress()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenu(mainPanelGUI);
            }
        }

        private void Update()
        {
            UIKeyPress();
        }

        private void LateUpdate()
        {
            ShowNetworkStatus();
            //ShowMapChoiceButtons();
        }

        private void ShowMapChoiceButtons()
        {
            //if in online lobby
            var isHostInLobby = (GameManager.Instance.IsHost() && GameManager.Instance.IsInLobby());
            mapChoice.SetActive(isHostInLobby);
            //Debug.Log($"isInLobby {isInLobby} active {SceneManager.GetActiveScene().name} network {MyNetworkManager.Instance.onlineScene}");
        }

        private void ShowNetworkStatus()
        {
            txtNetworkStatus.text =
                $"{MyNetworkManager.Instance.mode.ToString()} :: {SceneManager.GetActiveScene().name}";
        }

        private void OnEnable()
        {
            RegisterListeners();
        }

        private void OnDisable()
        {
            DeregisterListeners();
        }

        /// <summary>
        /// set up the gui layout to show what is necessary for online mode after logging in
        /// main menu off, bottom bar on, to bar on
        /// </summary>
        public void OnStartOnline()
        {
            topPanel.SetActive(true);
            topTimerBlock.SetActive(false);
            mainPanelGUI.SetActive(false);
            bottomPanel.SetActive(true);
        }

        /// <summary>
        /// set up the gui layout to show what is necessary for online mode after starting a game
        /// main menu off, bottom bar on with players, to bar on, timer on
        /// </summary>
        public void OnStartGame()
        {
            topPanel.SetActive(true);
            topTimerBlock.SetActive(true);
            mainPanelGUI.SetActive(false);
            bottomPanel.SetActive(true);
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

        public void ButtonMapChoice(string sceneMapName)
        {
            GameManager.Instance.StartGame(sceneMapName);
        }
    }
}