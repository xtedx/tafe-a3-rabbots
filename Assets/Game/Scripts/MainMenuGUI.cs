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
    /// <summary>
    /// used to update the values on the gui
    /// </summary>
    [Serializable]
    public class PlayerGUIRendering
    {
        public Image avatar;
        public Text playerName;
        public Text playerID;
        public Image hp;

        /// <summary>
        /// used to check if this netid/player id is the same
        /// may not work if we have players disconnecting and reconnecting? as the index here is hard coded on inspector
        /// </summary>
        public uint netId;
    }
    
    /// <summary>
    /// this is not a networked class and only deals with the GUI
    /// </summary>
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
        [SerializeField]
        private Toggle tabControl;
        [SerializeField]
        private Toggle tabConnection;
        [SerializeField]
        private Toggle tabSetting;
        
        [Tooltip("the body for each tab")]
        [SerializeField]
        private List<GameObject> tabBody;
        
        [Space]
        [Tooltip("the button group for map choice")]
        [SerializeField]
        private GameObject mapChoice;

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
        
        [Space]
        [SerializeField]
        public Slider sliderMaxTime;
        [SerializeField]
        public Slider sliderMaxHP;
        [SerializeField]
        private Button buttonReady;
        [SerializeField]
        private Button buttonStartGame;
        
        [Space]
        [SerializeField] public List<PlayerGUIRendering> renders = new List<PlayerGUIRendering>(4);
        private int activeTabIndex;
        
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

        private void Awake()
        {

        }

        private void Start()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            switch (sceneName)
            {
                case MyNetworkManager.OFFLINE_SCENE:
                    OnStartOffline();
                    break;
                case MyNetworkManager.ONLINE_SCENE:
                    OnStartOnline();
                    break;
                default:
                    break;
            }
            //make sure only host can change this and start game
            sliderMaxTime.interactable = MyNetworkManager.Instance.IsHost;
            sliderMaxHP.interactable = MyNetworkManager.Instance.IsHost;
            buttonStartGame.interactable = MyNetworkManager.Instance.IsHost;
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
            //var isHostInLobby = (MyNetworkManager.Instance.IsHost && MyNetworkManager.Instance.IsInLobby());
            //mapChoice.SetActive(isHostInLobby);
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
            topTimerBlock.SetActive(true);
            mainPanelGUI.SetActive(true);
            tabSetting.isOn = true;
            bottomPanel.SetActive(true);
        }

        /// <summary>
        /// set up the gui layout to show what is necessary for online mode after starting a game
        /// main menu off, bottom bar on with players, to bar on, timer on
        /// </summary>
        public void OnStartOffline()
        {
            topPanel.SetActive(true);
            topTimerBlock.SetActive(false);
            mainPanelGUI.SetActive(true);
            tabConnection.isOn = true;
            bottomPanel.SetActive(false);
        }
        
        /// <summary>
        /// set up the gui layout to show what is necessary for online mode after starting a game
        /// main menu off, bottom bar on with players, to bar on, timer on
        /// </summary>
        public void OnStartGame()
        {
            topPanel.SetActive(true);
            topTimerBlock.SetActive(true);
            tabControl.isOn = true;
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
            //MyNetworkManager.Instance.StartGame(sceneMapName);
        }

        public void ButtonStartGame()
        {
            MyNetworkManager.LocalPlayer.LocalGameStart((int) sliderMaxTime.value, (int) sliderMaxHP.value);
        }

    }
}