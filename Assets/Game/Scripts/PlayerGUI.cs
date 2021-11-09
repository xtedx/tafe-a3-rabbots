using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

namespace Game.Scripts
{
    public class PlayerGUI : NetworkBehaviour
    {
        [Serializable]
        public class PlayerGUIRendering
        {
            public Image avatar;
            public Text playerName;
            public Text hp;
            public Slider slider;

            public uint netId;
        }

        public List<PlayerGUIRendering> renders = new List<PlayerGUIRendering>();

            //todo: use dictionary instead of array?
        //private readonly Dictionary<uint, NetworkPlayer> players = new Dictionary<uint, NetworkPlayer>();
    
        public Text textTimer;
        public int playerIndex = 0;
        
        [ClientRpc]
        public void UpdateGUIcolour(uint key, Color value)
        {
            foreach (PlayerGUIRendering render in renders)
            {
                if (render.netId == key)
                {
                    Debug.Log($"in update gui render {render.avatar.name} for {key} color is {value}");
                    render.avatar.color = value;
                }
            }
        }
        
        [ClientRpc]
        public void UpdateGUIname(uint key, string value)
        {
            foreach (PlayerGUIRendering render in renders)
            {
                if (render.netId == key)
                {
                    Debug.Log($"in update gui render {render.playerName.text} for {key} color is {value}");
                    render.playerName.text = value;
                }
            }
        }
        
        // Start is called before the first frame update
        void Start()
        {
            playerIndex = (int)netId - 1;
            //ClearNetIds();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
