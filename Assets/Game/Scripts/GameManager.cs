using System.Collections.Generic;
using JetBrains.Annotations;
using Mirror;
using NetworkGame.Networking;
using TeddyToolKit.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts
{
    public class GameManager : MonoSingleton<GameManager>
    {
        #region constant definitions

        public const string MAP1_SCENE = "Map 1";
        public const string MAP2_SCENE = "Map 2";
        public const string OFFLINE_SCENE = "Offline Start Scene";
        public const string ONLINE_SCENE = "Online Lobby";
        
        #endregion

        public void LoadLocalScene(string scenename)
        {
            SceneManager.LoadScene(scenename, LoadSceneMode.Additive);
        }
        private void OnEnable()
        {
            FlagAsPersistant();
            RegisterListeners();
            LoadLocalScene("GUI");
            Debug.Log("loaded gui scene");
        }

        private void OnDisable()
        {
            DeregisterListeners();
        }
    
        /// <summary>
        /// listen to the event and call method when that happens
        /// </summary>
        private void RegisterListeners()
        {
            // EventManager.Instance.OnTimerDone += GameOver;
        }
        
        /// <summary>
        /// make sure to un-listen, always as a pair with the onenable
        /// </summary>
        private void DeregisterListeners()
        {
            // EventManager.Instance.OnTimerDone -= GameOver;
        }

        /// <summary>
        /// what happens when the gameover event occurs
        /// </summary>
        public void GameOver()
        {
            Debug.Log("Game Over");
        }

        /// <summary>
        /// Is this the host?
        /// </summary>
        /// <returns></returns>
        public bool IsHost()
        {
            return (MyNetworkManager.Instance.mode == NetworkManagerMode.Host);
        }

        /// <summary>
        /// are we in online lobby now?
        /// </summary>
        /// <returns></returns>
        public bool IsInLobby()
        {
            return (SceneManager.GetActiveScene().name == ONLINE_SCENE);
        }
        
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            
        }


        public void StartGame(string sceneMapName)
        {
            //if in online lobby
            if (IsInLobby() && IsHost())
            {
                //this should only be in the online lobby, by the host and not anytime in the game 
                GetComponent<NetworkSceneManager>().LoadNetworkScene(sceneMapName);
                Debug.Log($"loaded {sceneMapName}");
            }
            else
            {
                Debug.LogError($"not host and/or in lobby");
            }
        }
    }
}
