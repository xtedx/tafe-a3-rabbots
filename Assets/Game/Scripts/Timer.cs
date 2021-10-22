using System;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class Timer : MonoBehaviour
    {
        public float startingTime = 10;
        public float tickInterval = 1;
        public float timeLeft;
        private int _counter;

        [Serializable]
        /// <summary>
        /// Function definition for a timer tick event, fired every interval it ticks
        /// </summary>
        public class TimerTickedEvent : UnityEvent<int> {}
        // Event delegates triggered on a timer tick
        [SerializeField]
        private TimerTickedEvent timerTickedEvent = new TimerTickedEvent();

        [Serializable]
        /// <summary>
        /// Function definition for a timer tick event, fired when the timer reaches zero
        /// </summary>
        public class TimerDoneEvent : UnityEvent {}
        // Event delegates triggered on a timer is done
        [SerializeField]
        private TimerDoneEvent timerDoneEvent = new TimerDoneEvent();
       
        /// <summary>
        /// UnityEvent that is triggered when the timer ticks.
        /// </summary>
        ///         ///<example>
        ///<code>
        /// using UnityEngine;
        /// using UnityEngine.UI;
        /// using System.Collections;
        ///
        /// public class ClickExample : MonoBehaviour
        /// {
        ///     public Button yourButton;
        ///
        ///     void Start()
        ///     {
        ///         Button btn = yourButton.GetComponent<Button>();
        ///         btn.onClick.AddListener(TaskOnClick);
        ///     }
        ///
        ///     void TaskOnClick()
        ///     {
        ///         Debug.Log("You have clicked the button!");
        ///     }
        /// }
        ///</code>
        ///</example>
        public TimerTickedEvent onTimerTick
        {
            get { return timerTickedEvent; }
            set { timerTickedEvent = value; }
        }
        
        /// <summary>
        /// UnityEvent that is triggered when the timer is done.
        /// </summary>
        public TimerDoneEvent onTimerDone
        {
            get { return timerDoneEvent; }
            set { timerDoneEvent = value; }
        }

        // Start is called before the first frame update
        private void Start()
        {
            Restart();
        }

        // Update is called once per frame
        private void Update()
        {
            CountDown();
        }

        public void CountDown()
        {
            //check and stop if timer is already over at the beginning
            if (_counter <= 0) return;
            
            //minus the time after one frame
            timeLeft -= Time.deltaTime;
            
            if (timeLeft < _counter)
            {
                _counter = (int) timeLeft;
                //fire interval has lapsed;
                //EventManager.Instance.TriggerTimer1SecondTick(_counter);
                onTimerTick.Invoke(_counter);
            }
            
            if (_counter <= 0)
            {
                //fire event timer is done
                //EventManager.Instance.TriggerTimerDone();
                onTimerDone.Invoke();
            } 
        }
        public void Restart()
        {
            timeLeft = startingTime;
            _counter = (int) (timeLeft + tickInterval);
        }
    }
}
