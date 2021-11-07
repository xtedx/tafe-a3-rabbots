using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UIElements.Image;
using Slider = UnityEngine.UIElements.Slider;

namespace Game.Scripts
{
    public class PlayerGUI : NetworkBehaviour
    {
        //todo: use dictionary instead of array?
        //private readonly Dictionary<uint, NetworkPlayer> players = new Dictionary<uint, NetworkPlayer>();
    
        // private readonly SyncDictionary<uint, Image> imagePlayerAvatars = new SyncDictionary<uint, Image>();
        //private readonly SyncDictionary<uint, Text> textPlayerNames = new SyncDictionary<uint, Text>();
        [SerializeField]
        private Image[] imagePlayerAvatar;
        [SerializeField]
        private Text[] textPlayerName;
        [SerializeField]
        private Text[] textPlayerHP;
        [SerializeField]
        private Slider[] sliderPlayerHP;
        [SerializeField]
        private Text textTimer;
    
        [SyncVar(hook = nameof(OnSetTimerValue)), SerializeField] private int timer;
        [SyncVar(hook = nameof(OnSetPlayerHP)), SerializeField]
        private int playerHP;
        private int playerIndex = 0;
    
        private void OnSetTimerValue(int oldValue, int newValue)
        {
        
            //update the text value of timer
            textTimer.text = newValue.ToString("00");
            //timer = newValue;
        }
    
        private void OnSetPlayerHP(int oldValue, int newValue)
        {
            //update the text value and slider of hp
            textPlayerHP[playerIndex].text = newValue.ToString("00");
            sliderPlayerHP[playerIndex].value = newValue;
            //playerHP = newValue;
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
