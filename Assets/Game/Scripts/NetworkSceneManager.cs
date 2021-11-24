using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetworkGame.Networking
{
    public delegate void SceneLoadedDelegate(Scene _scene);

    public class NetworkSceneManager : NetworkBehaviour
    {
        public void LoadNetworkScene(string _scene)
        {
            // Debug.Log($"Clicked {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            // Debug.Log($"isLocalPlayer {isLocalPlayer}");
            if(isLocalPlayer) CmdLoadNetworkScene(_scene);
        }

        [Command]
        public void CmdLoadNetworkScene(string _scene) => RpcLoadNetworkScene(_scene);

        [ClientRpc]
        public void RpcLoadNetworkScene(string _scene)
        {
            LoadScene(_scene, _loadedScene => SceneManager.SetActiveScene(_loadedScene));
        }

        public void LoadScene(string _sceneName, SceneLoadedDelegate _onSceneLoaded = null)
        {
            StartCoroutine(LoadScene_CR(_sceneName, _onSceneLoaded));
        }

        private IEnumerator LoadScene_CR(string _sceneName, SceneLoadedDelegate _onSceneLoaded = null)
        {
            yield return SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
            _onSceneLoaded?.Invoke(SceneManager.GetSceneByName(_sceneName));
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