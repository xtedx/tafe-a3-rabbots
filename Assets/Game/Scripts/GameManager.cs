using System.Collections.Generic;
using JetBrains.Annotations;
using TeddyToolKit.Core;
using UnityEngine;

namespace Game.Scripts
{
    public class GameManager : MonoSingleton<GameManager>
    {
        /// <summary> The dictionary of all connected players using their NetID as the key. </summary>
        private readonly Dictionary<uint, NetworkPlayer> players = new Dictionary<uint, NetworkPlayer>();

        /// <summary> Adds a player to the dictionary. </summary>
        public void AddPlayer([NotNull] NetworkPlayer player)
        {
            players.Add(player.netId, player);
        }
        
        /// <summary> Removes a player from the dictionary. </summary>
        public void RemovePlayer([NotNull] NetworkPlayer player) => players.Remove(player.netId);
        
        private void OnEnable()
        {
            RegisterListeners();
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
