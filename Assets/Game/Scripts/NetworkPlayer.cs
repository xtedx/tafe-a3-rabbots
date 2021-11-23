using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using NetworkGame.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.Scripts
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(CameraMovement))]
    public class NetworkPlayer : NetworkBehaviour
    {
        /// <summary>
        /// for debugging only to show at the inspector window
        /// </summary>
        public uint playerID;
        /// <summary>
        /// this is used as the index in the GUI elements (gui renderer class PlayerGUIRendering)
        /// because netid starts from 1, but csharp arrays starts from 0
        /// </summary>
        /// <exception cref="Exception"></exception>
        public int GuiIndex
        {
            get
            {
                var index = netId - 1;
                if (index >= 0)
                {
                    return (int) index;
                }
                else
                {
                    throw new Exception("player id used for index cannot be less than 0");
                }
            }
        }
        /// <summary>
        /// this is to contain the main menu gui, but because this is in a different scene, it will be set in code using the public getter
        /// </summary>
        private MainMenuGUI mainMenuGUI;
        /// <summary>
        /// this is to prevent null on the game object, because unity game object cannot accurately be compared with == null
        /// until a property is used, and thats why we need the try catch to check the count of renders element
        /// otherwise for other objects, using the object?.property syntax would be easier.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public MainMenuGUI MainMenuGUI
        {
            get
            {
                if (mainMenuGUI != null)
                {
                    return mainMenuGUI;
                }
                else
                {
                    //initialise the GUI
                    mainMenuGUI = GameObject.FindObjectOfType<Scripts.MainMenuGUI>();
                    return mainMenuGUI;
                }
            }
        }
        
        public Text textTimer;
        
        private bool hasAddedToGui = false;

        // although the gui needs to display all players' name/colour/hp etc,
        // we dont need SyncList/SyncDict here, we'll use a list in the GUI as we only need them there.
        // each variables here per client will be managed by mirror
        [SyncVar(hook = nameof(OnSetPlayerColour)), SerializeField] private Color playerColour;
        [SyncVar(hook = nameof(OnSetPlayerName)), SerializeField] private string playerName1;
        [SyncVar(hook = nameof(OnSetPlayerHP)), SerializeField] private int playerHP;
        [SyncVar(hook = nameof(OnTimerTick)), SerializeField] private int gameTimer;
        [SyncVar, SerializeField] private double playerDashTime;
        
        [Header("Game Settings")]
        [SyncVar(hook = nameof(OnSetMaxHP)), SerializeField] private int maxPlayerHP = 10;
        [SyncVar(hook = nameof(OnSetMaxTime)), SerializeField] private int maxGameTime = 99;
        
        
        /// <summary>
        /// to change the object colour
        /// </summary>
        private Material cachedMaterial;

        [Tooltip("this is the real player model object, in the child of the root player prefab")]
        [SerializeField] public GameObject playerChildGameObject;

        // Typical naming convention for SyncVarHooks is OnSet<VariableName>
        
        /// <summary>
        /// when player colour is changed, this function is called and update the player object's material
        /// then update the gui with rpc
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        private void OnSetPlayerColour(Color oldValue, Color newValue)
        {
            // cachedMaterial = playerChildGameObject.GetComponent<MeshRenderer>().material;
            var renderers = playerChildGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var r in renderers)
            {
                cachedMaterial = r.material;
                //the scarf colour
                cachedMaterial.color = newValue;
                //the lines/eyes colour
                cachedMaterial.SetColor("_EmissionColor", newValue);
            }
        }

        private void OnSetPlayerHP(int oldValue, int newValue)
        {
            UpdateGUIhp(netId, newValue);
        }
        private void OnSetMaxHP(int oldValue, int newValue)
        {
            UpdateGUImaxHp(netId, newValue);
        }
        private void OnSetMaxTime(int oldValue, int newValue)
        {
            GetComponent<Timer>().startingTime = newValue;
            UpdateGUImaxTime(netId, newValue);
        }
        
        private void OnSetPlayerName(string oldValue, string newValue)
        {
            UpdateGUIname(netId, newValue);
        }
        
        private void OnTimerTick(int oldValue, int newValue)
        {
            mainMenuGUI.UpdateTimerText(newValue);
        }

        private void Update()
        {
            //hacky way to avoid error when the ui is not ready, and keep calling from the update method.
            RegisterPlayerInGUI(netId);
            
            // First determine if this function is being run on the local player
            if(isLocalPlayer)
            {
                if(Input.GetKeyDown(KeyCode.Return))
                {
                    // Run a function that tells every client to change the colour of this gameObject
                    //CmdRandomColor();
                    CmdRandomName();
                    //CmdTestToggleGUI();
                    //Debug.Log($"player {netId }colour changed");
                }
            }

            if (isLocalPlayer && MyNetworkManager.Instance.IsHost)
            {
                if(Input.GetKeyDown(KeyCode.X))
                {
                    CmdTimerStart();
                }
            }
        }

        [Command]
        public void CmdSpawnEnemy()
        {
            // NetworkServer.Spawn requires an instance of the object in the server's scene to be present
            // so if the object being spawned is a prefab, instantiate needs to be called first
            // GameObject newEnemy = Instantiate(enemyToSpawn);
            // NetworkServer.Spawn(newEnemy);
        }

        /// <summary>
        /// RULES FOR COMMANDS:
        /// 1. Cannot return anything
        /// 2. Must follow the correct naming convention: The function name MUST start with 'Cmd' exactly like that
        /// 3. The function must have the attribute [Command] found in Mirror namespace
        /// 4. Can only be certain serializable types (see Command in the documentation)
        /// This is called by the client, and being run in the server
        /// then to propagate the change to all other cliets, call an RPC function to deal with the change
        /// in this case update gui
        /// </summary>
        [Command]
        public void CmdRandomColor()
        {
            // SyncVar MUST be set on the server, otherwise it won't be synced between clients
            playerColour = Random.ColorHSV(0, 1, 1, 1, 1, 1);
            
            RpcUpdateGUIcolor(netId, playerColour);
        }

        [Command]
        public void CmdTimerTick(int value)
        {
            gameTimer = value;
        }
        
        [Command]
        public void CmdTimerDone()
        {
            Debug.Log("GAME OVER");
            //decides who wins by checking the hp of each player
            MyNetworkManager.LocalPlayer.ServerGameOver();
        }

        [Server]
        public void ServerGameOver()
        {
            string summary = "";
            foreach (var pair in MyNetworkManager.Instance.players)
            {
                summary += $"{pair.Value.playerName1} remaining HP: {pair.Value.playerHP}\n";
            }

            summary += "biggest HP wins!";
            RpcGameOver(summary);
        }

        [ClientRpc]
        public void RpcGameOver(string summmary)
        {
            MainMenuGUI.OnGameOver();
            MainMenuGUI.textGameOverSummary.text = summmary;
        }
        
        [Command]
        public void CmdTimerStart()
        {
            if (MyNetworkManager.Instance.IsHost)
            {
                GetComponent<Timer>().Restart();
            }
        }
        
        /// <summary>
        /// when player is hit, reduce hp by 1, then convert it to percentage 0-1 and pass it to update gui
        /// </summary>
        [Command]
        public void CmdPlayerIsHit()
        {
            //sanity check
            if (playerHP < 1) return;
            
            playerHP--;
            RpcUpdateGUIhp(netId, playerHP);
        }
        
        /// <summary>
        /// when player collides, if not dashing it will definitely lose, otherwise 50% chance of losing hp.
        /// why can't this call another command? so i had to copy paste.
        /// </summary>
        [Command]
        public void CmdDecidePlayerCollision(bool isDashing)
        {
            if (!isDashing)
            {
                //sanity check
                if (playerHP < 1) return;
            
                playerHP--;
                RpcUpdateGUIhp(netId, playerHP);
                //CmdPlayerIsHit();
            }
            else
            {
                if (Random.value < 0.4f)
                {
                    //sanity check
                    if (playerHP < 1) return;
            
                    playerHP--;
                    RpcUpdateGUIhp(netId, playerHP);
                    //CmdPlayerIsHit();
                }
            }
        }
        
        [Command]
        public void CmdPlayerAddHP()
        {
            //sanity check
            if (playerHP > 1) return;

            playerHP++;
            RpcUpdateGUIhp(netId, playerHP);
        }
        
        /// <summary>
        /// this is used to compare who dashes last and it wins the hit, if both are dashing at the same time
 		/// dash rule: on dash, a time snapshot is taken, the number of seconds from game start.
        /// the number gets bigger over time, so the bigger number means the later start to dash
        /// the character that dashes last will win the collision battle
        /// when player is dashes, keep the time it started. this does not need to be reset because the smaller number loses anyway 

        /// </summary>
        [Command]
        public void CmdPlayerDashStart()
        {
            //store timestamp
            playerDashTime = NetworkTime.time;
        }
        
        /// <summary>
        /// RULES FOR CLIENT RPC:
        /// 1. Cannot return anything
        /// 2. Must follow the correct naming convention: The function name MUST start with 'Rpc' exactly like that
        /// 3. The function must have the attribute [ClientRpc] found in Mirror namespace
        /// 4. Can only be certain serializable types (see Command in the documentation)
        ///
        /// this is run on the client
        ///
        /// <example>
        /// [ClientRpc]
        /// public void RpcRandomColor(float _hue)
        /// {
        ///     // This is running on every instance of the same object that the client was calling from.
        ///     // i.e. Red GO on Red Client runs Cmd, Red GO on Red, Green and Blue client's run Rpc
        ///     MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();
        ///     rend.material.color = Color.HSVToRGB(_hue, 1, 1);
        /// }
        /// </example>
        /// </summary>
        [ClientRpc]
        public void RpcRandomColor()
        {
            
        }

        [Command]
        public void CmdRandomName()
        {
            // SyncVar MUST be set on the server, otherwise it won't be synced between clients
            playerName1 = DateTime.Now.ToString("mmss");
            playerName1 = $"Bot{playerName1}";
            RpcUpdateGUIname(netId, playerName1);
        }

        [Command]
        public void CmdUpdateGUIcolor(uint key, Color value)
        {
            RpcUpdateGUIcolor(key, value);
        }

        [ClientRpc]
        private void RpcUpdateGUIcolor(uint key, Color value)
        {
            UpdateGUIcolor(key, value);
        }
        
        private void UpdateGUIcolor(uint key, Color value)
        {
            // Debug.Log($"RpcUpdateGUIcolor netid {netId}");
            foreach (PlayerGUIRendering render in MainMenuGUI.renders)
            {
                if (render.netId == key)
                {
                    // Debug.Log($"in update gui render {render.avatar.name} for {key} value is {value}");
                    render.avatar.color = value;
                    render.hp.color = value;
                }
            }
        }

        [Command]
        private void CmdUpdateGUIhp(uint key, int value)
        {
            RpcUpdateGUIhp(key, value);
        }

        /// <summary>
        /// update the fill amount for health bar
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value">fillamount takes a float 0-1, so convert the hp to a percentage first before passing</param>
        [ClientRpc]
        private void RpcUpdateGUIhp(uint key, int value)
        {
            UpdateGUIhp(key, value);
        }
        
        private void UpdateGUIhp(uint key, int value)
        {
            // Debug.Log($"RpcUpdateGUIhp netid {netId}");
            float hpPercent = value / (float)maxPlayerHP;

            if (hpPercent < 0 || hpPercent > 1) throw new Exception($"invalid hp fillamount, expecting 0-1 only. key {key} % {hpPercent} {value}/{maxPlayerHP}");
            foreach (PlayerGUIRendering render in MainMenuGUI.renders)
            {
                if (render.netId == key)
                {
                    // Debug.Log($"Login update gui render {render.hp.name} for {key} value is {value}");
                    render.hp.fillAmount = hpPercent;
                }
            }
        }

        [Command]
        private void CmdUpdateGUImaxHp(uint key, int value)
        {
            RpcUpdateGUImaxHp(key, value);
        }
        
        [ClientRpc]
        private void RpcUpdateGUImaxHp(uint key, int value)
        {
            UpdateGUImaxHp(key, value);
        }
        
        private void UpdateGUImaxHp(uint key, int value)
        {
            // Debug.Log($"UpdateGUImaxHp netid {netId}");
            MainMenuGUI.sliderMaxHP.SetValueWithoutNotify((float)value);
            MainMenuGUI.textMaxHP.GetComponent<TextUpdater>().SetTextValue(value);

        }
        
        [Command]
        private void CmdUpdateGUImaxTime(uint key, int value)
        {
            RpcUpdateGUImaxTime(key, value);
        }
        
        [ClientRpc]
        private void RpcUpdateGUImaxTime(uint key, int value)
        {
            UpdateGUImaxTime(key, value);
        }
        
        private void UpdateGUImaxTime(uint key, int value)
        {
            // Debug.Log($"UpdateGUImaxTime netid {netId}");
            MainMenuGUI.sliderMaxTime.SetValueWithoutNotify((float)value);
            MainMenuGUI.textMaxTime.GetComponent<TextUpdater>().SetTextValue(value);
        }

        [Command]
        private void CmdUpdateGUIname(uint key, string value)
        {
            RpcUpdateGUIname(key, value);
        }

        [ClientRpc]
        private void RpcUpdateGUIname(uint key, string value)
        {
            UpdateGUIname(key, value);
        }
        
        private void UpdateGUIname(uint key, string value)
        {
            // Debug.Log($"RpcUpdateGUIname netid {netId}");
            foreach (PlayerGUIRendering render in MainMenuGUI.renders)
            {
                if (render.netId == key)
                {
                    // Debug.Log($"in update gui render {render.playerName.name} for {key} value is {value}");
                    render.playerName.text = value;
                    render.playerID.text = key.ToString();
                }
            }
        }
        
        #region test gui code
        /// <summary>
        ///test if a client can toggle a server gui and shown on every client
        ///if this works it means the gui doesn't need to be in player prefab
        ///this works by togling the menu in the server only.
        ///the second client doesn't see anything.
        ///now test if rpc can do this?
        ///yes rpc can do this, the command just need to call the RPC
        ///so it means both command and rpc need to be called for it to be the same on all
        ///LocalTestToggleGUI();
        /// </summary>
        [Command]
        public void CmdTestToggleGUI()
        {
            RpcTestToggleGUI();
        }

        public void LocalTestToggleGUI()
        {
            RpcTestToggleGUI();
        }
        
        [ClientRpc]
        public void RpcTestToggleGUI()
        {
            var guiObjects = SceneManager.GetSceneByName(MyNetworkManager.GUI_SCENE).GetRootGameObjects();
            foreach (var go in guiObjects)
            {
                var menu = go.GetComponent<MainMenuGUI>();
                try
                {
                    menu.ToggleMenu();
                    break;
                }
                catch (NullReferenceException)
                {
                    continue;
                }
            }
        }
        #endregion
        
        // This is run via the network starting and the player connecting...
        // NOT Unity
        // It is run when the object is spawned via the networking system NOT when Unity
        // instantiates the object
        public override void OnStartLocalPlayer()
        {
            // This is run if we are the local player and NOT a remote player
            // MyNetworkManager.Instance.UnLoadLocalScene(MyNetworkManager.GUI_SCENE);
            // MyNetworkManager.Instance.LoadLocalScene(MyNetworkManager.GUI_SCENE);
            // Debug.Log("loaded scene in network player");
            
            //RegisterPlayerInGUI(netid) should be in the update because of some execution issue, should use sceneloadedevent next time.
        }
        
        /// <summary>
        /// This is run via the network starting and the player connecting...
        /// NOT Unity
        /// It is run when the object is spawned via the networking system NOT when Unity
        /// instantiates the object
        /// This will run REGARDLESS if we are the local or remote player
        /// isLocalPlayer is true if this object is the client's local player otherwise it's false
        /// </summary>
        public override void OnStartClient()
        {
            PlayerController controller = gameObject.GetComponent<PlayerController>();
            controller.enabled = isLocalPlayer;
            
            MyNetworkManager.AddPlayer(this);
            Debug.Log($"added player {netId} to network manager");

            //for debugging
            playerID = netId;
        }

        public override void OnStopClient()
        {
            MyNetworkManager.RemovePlayer(this);
        }

        /// <summary>
        /// This runs when the server starts... ON the server on all clients
        /// set some default values
        /// </summary>
        public override void OnStartServer()
        {
            playerHP = maxPlayerHP;
            switch (netId%4)
            {
                case 1:
                    playerColour = Color.red;
                    playerName1 = "Redy";
                    break;
                case 2:
                    playerColour = Color.blue;
                    playerName1 = "Bluey";
                    break;
                case 3:
                    playerColour = Color.yellow;
                    playerName1 = "Yellowy";
                    break;
                case 4:
                    playerColour = Color.green;
                    playerName1 = "Greeny";
                    break;
            }
        }
        
        /// <summary>
        /// registers the player in the gui slots. only take the first slot available and breaks out
        /// empty slot is when the netid is gui is 0
        /// </summary>
        /// <param name="key"></param>
        private void RegisterPlayerInGUI(uint key)
        {
            //hacky way to avoid error when the ui is not ready, and keep calling from the update method.
            if(MainMenuGUI == null)
                return;

            if (hasAddedToGui)
                return;
        
            var i = 0;
            foreach (PlayerGUIRendering render in MainMenuGUI.renders)
            {
                if (render.netId == key)
                {
                    Debug.Log($"RegisterPlayerInGUI trying to register the same ID {key}");
                    return;
                }
                if (render.netId == 0) //0 means empty slot
                {
                    Debug.Log($"registered player {key} in the gui slots {i}");
                    render.netId = key;
                    // Debug.Log($"player colour should be from gui {render.avatar.color}");
                    
                    UpdateGUI(key);
                    hasAddedToGui = true;
                    break;
                }

                i++;
            }
        }

        /// <summary>
        ///set default values here ,or get values from server trigger rpc calls to update ui
        /// </summary>
        /// <param name="key"></param>
        public void UpdateGUI(uint key)
        {
            UpdateGUIcolor(key, playerColour);
            UpdateGUIhp(key, playerHP);
            UpdateGUIname(key, playerName1);
        }

        /// <summary>
        /// registers the player in the gui slots. only take the first slot available and breaks out
        /// empty slot is when the netid is gui is 0
        /// </summary>
        /// <param name="key"></param>
        private void UnRegisterPlayerInGUI(uint key)
        {
            foreach (PlayerGUIRendering render in MainMenuGUI.renders)
            {
                if (render.netId == key)
                {
                    Debug.Log($"unregistered player {key} in the gui slots");
                    render.netId = 0; //0 means empty slot
                    break;
                }
            }
        }

        public void LocalMaxTimeChanged(int maxTime)
        {
            if (isLocalPlayer)
            {
                CmdUpdateGUImaxTime(netId, maxTime);
            }
        }
        
        public void LocalMaxHPChanged(int maxHP)
        {
            if (isLocalPlayer)
            {
                CmdUpdateGUImaxHp(netId, maxHP);
            }
        }
        
        public void LocalGameStart(int maxTime, int maxHP)
        {
            if (isLocalPlayer)
            {
                CmdGameStart(maxTime, maxHP);
            }
        }

        [Command]
        public void CmdGameStart(int maxTime, int maxHP)
        {
            RpcGameStart(maxTime, maxHP);
            ServerGameStart();
        }

        [ClientRpc]
        public void RpcGameStart(int maxTime, int maxHP)
        {
            maxGameTime = maxTime;
            playerHP = maxPlayerHP = maxHP;
            //update settings
            UpdateGUI(netId);
        }
        
        [SyncVar(hook = nameof(OnReceivedGameStarted))]
        public bool gameStarted = false;
        
        private void OnReceivedGameStarted(bool _old, bool _new)
        {
            // If you want a countdown or some sort of match starting
            // indicator, replace the contents of this function
            // with that and then call the Unload
			
            if(_new)
            {
                //SceneManager.UnloadSceneAsync("Lobby");
                GameObject.FindObjectOfType<MainMenuGUI>().OnStartGame();
                if (hasAuthority) CmdTimerStart();

            }
        }
        
        [Server]
        public void ServerGameStart()
        {
            gameStarted = true;
        }
        
    }
}