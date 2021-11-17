using System;
using System.Collections.Generic;
using Mirror;
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
                    var guiObjects = SceneManager.GetSceneByName(GameManager.GUI_SCENE).GetRootGameObjects();
                    //strange argument exception but it can get the objects fine. ignore
                    bool hasGui = false;
                    foreach (var go in guiObjects)
                    {
                        mainMenuGUI = go.GetComponent<MainMenuGUI>();
                        try
                        {
                            var testnull = mainMenuGUI.renders.Count;
                            //if no exception after this line, then all good
                            hasGui = true;
                            break;
                        }
                        catch (NullReferenceException)
                        {
                            continue;
                        }
                    }

                    //if after looping but still no gui then fatal error
                    if (!hasGui)
                    {
                        throw new NullReferenceException("need gui to continue, but not found");
                    }

                    return mainMenuGUI;
                }
            }
        }
        
        public Text textTimer;
        
        private bool hasAddedToGui = false;
        private bool hasChangedColour = false;
        private bool hasChangedName = false;

        // although the gui needs to display all players' name/colour/hp etc,
        // we dont need SyncList/SyncDict here, we'll use a list in the GUI as we only need them there.
        // each variables here per client will be managed by mirror
        
        [SyncVar(hook = nameof(OnSetPlayerColour)), SerializeField] private Color playerColour;
        [SyncVar, SerializeField] private string playerName;
        [SyncVar, SerializeField] private int playerHP;
        public SyncList<double> playerDashTimeList = new SyncList<double>();
        
        [Header("Game Settings")]
        [SerializeField] private int maxPlayerHP = 10;
        
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
            cachedMaterial = playerChildGameObject.GetComponent<MeshRenderer>().material;
            //the scarf colour
            cachedMaterial.color = newValue;
            //the lines/eyes colour
            cachedMaterial.SetColor("_EmissionColor", newValue);
        }
        
        private void Awake()
        {
            // This will run REGARDLESS if we are the local or remote player
        }

        private void Update()
        {
            //hacky way to avoid error when the ui is not ready, and keep calling from the update method.
            RegisterPlayerInGUI(netId);
            GetPlayerColourFromGUI(netId);
            GetPlayerNameFromGUI(netId);
            
            // First determine if this function is being run on the local player
            if(isLocalPlayer)
            {
                if(Input.GetKeyDown(KeyCode.Return))
                {
                    // Run a function that tells every client to change the colour of this gameObject
                    //CmdRandomColor();
                    //CmdRandomName();
                    //CmdTestToggleGUI();
                    //Debug.Log($"player {netId }colour changed");
                }

                if(Input.GetKeyDown(KeyCode.X))
                {
                    CmdSpawnEnemy();
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

        private void GetPlayerColourFromGUI(uint key)
        {
            //hacky way to avoid error when the ui is not ready, and keep calling from the update method.
            if(MainMenuGUI == null)
                return;

            if (hasChangedColour)
                return;
        
            foreach (PlayerGUIRendering render in MainMenuGUI.renders)
            {
                if (render.netId == key)
                {
                    Debug.Log($"in get player colour gui render {render.avatar.name} for {key} value is {render.avatar.color}");
                    playerColour = render.avatar.color;
                    hasChangedColour = true;
                }
            }
        }
        
        private void GetPlayerNameFromGUI(uint key)
        {
            //hacky way to avoid error when the ui is not ready, and keep calling from the update method.
            // if(MainMenuGUI == null)
            //     return;

            if (hasChangedName)
                return;
        
            foreach (PlayerGUIRendering render in MainMenuGUI.renders)
            {
                if (render.netId == key)
                {
                    Debug.Log($"in get player name gui render {render.avatar.name} for {key} value is {render.avatar.color}");
                    playerName = render.playerName.text;
                    playerID = UInt32.Parse(render.playerID.text);
                    hasChangedName = true;
                }
            }
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
        
        /// <summary>
        /// when player is hit, reduce hp by 1, then convert it to percentage 0-1 and pass it to update gui
        /// </summary>
        [Command]
        public void CmdPlayerIsHit()
        {
            //sanity check
            if (playerHP < 1) return;

            playerHP--;
            float hpPercent = playerHP / (float)maxPlayerHP;
            RpcUpdateGUIhp(netId, hpPercent);
        }
        
        /// <summary>
        /// when player is dashes, keep the time it started, and remove when done.
        /// this is used to compare who dashes last and it wins the hit, if both are dashing at the same time 
        /// </summary>
        [Command]
        public void CmdPlayerDashStart()
        {
            //sanity check
            if (playerDashTimeList.Count < 4) return;
            //store timestamp
            playerDashTimeList[GuiIndex] = NetworkTime.time;
        }
        [Command]
        public void CmdPlayerDashStop()
        {
            //sanity check
            if (playerDashTimeList.Count < 4) return;
            //reset timestamp
            playerDashTimeList[GuiIndex] = Double.MaxValue;
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
            playerName = DateTime.Now.ToString("mmss");
            playerName = $"Bot{playerName}";
            RpcUpdateGUIname(netId, playerName);
        }
        
        [ClientRpc]
        private void RpcUpdateGUIcolor(uint key, Color value)
        {
            Debug.Log($"RpcUpdateGUIcolor netid {netId}");
            foreach (PlayerGUIRendering render in MainMenuGUI.renders)
            {
                if (render.netId == key)
                {
                    Debug.Log($"in update gui render {render.avatar.name} for {key} value is {value}");
                    render.avatar.color = value;
                    render.hp.color = value;
                }
            }
        }

        /// <summary>
        /// update the fill amount for health bar
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value">fillamount takes a float 0-1, so convert the hp to a percentage first before passing</param>
        [ClientRpc]
        private void RpcUpdateGUIhp(uint key, float value)
        {
            Debug.Log($"RpcUpdateGUIhp netid {netId}");
            if (value < 0 || value >= 1) throw new Exception("invalid hp fillamount, expecting 0-1 only");
            foreach (PlayerGUIRendering render in MainMenuGUI.renders)
            {
                if (render.netId == key)
                {
                    Debug.Log($"in update gui render {render.hp.name} for {key} value is {value}");
                    render.hp.fillAmount = value;
                }
            }
        }
        
        [ClientRpc]
        private void RpcUpdateGUIname(uint key, string value)
        {
            Debug.Log($"RpcUpdateGUIname netid {netId}");
            foreach (PlayerGUIRendering render in MainMenuGUI.renders)
            {
                if (render.netId == key)
                {
                    Debug.Log($"in update gui render {render.playerName.name} for {key} value is {value}");
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
            var guiObjects = SceneManager.GetSceneByName(GameManager.GUI_SCENE).GetRootGameObjects();
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
            GameManager.Instance.LoadLocalScene(GameManager.GUI_SCENE);
            Debug.Log("loaded scene in network player");

            MyNetworkManager.AddPlayer(this);
            Debug.Log($"added player {netId} to network manager");

            //initialise player parameters
            playerHP = maxPlayerHP;
            playerName = $"StartClient{netId}";
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

            //for debugging
            playerID = netId;
        }

        public override void OnStopClient()
        {
            MyNetworkManager.RemovePlayer(this);
        }

        // This runs when the server starts... ON the server on all clients
        public override void OnStartServer()
        {
            //initialise array/list
            for (var i = 0; i < 4; i++)
            {
                playerDashTimeList.Add(Double.MaxValue);
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
//        foreach (PlayerGUIRendering render in FindObjectOfType<UiManager>().renders)
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
                    Debug.Log($"player colour should be from gui {render.avatar.color}");
                    hasAddedToGui = true;
                    break;
                }

                i++;
            }
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


    }
}