using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Scripts
{
    /// <summary>
    /// A modular timer class that can be drag and dropped to unity as game object and specify what function to call when
    /// the timer ticks and/or done 
    /// </summary>
    public class Timer : MonoBehaviour
    {
        public float startingTime = 10;
        public int tickInterval = 1;
        public float timeLeft;
        private int _counter;

        #region UnityEvents
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
        ///<example>
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
        ///         Timer timer = yourButton.GetComponent&lt;Timer&gt;();
        ///         timer.onTimerTick.AddListener(TaskOnClick);
        ///     }
        ///
        ///     void TaskOnClick()
        ///     {
        ///         Debug.Log("Timer ticked!");
        ///     }
        /// }
        ///</code>
        ///</example>
        private TimerTickedEvent onTimerTick
        {
            get { return timerTickedEvent; }
            set { timerTickedEvent = value; }
        }
        
        /// <summary>
        /// UnityEvent that is triggered when the timer is done.
        /// </summary>
        private TimerDoneEvent onTimerDone
        {
            get { return timerDoneEvent; }
            set { timerDoneEvent = value; }
        }
        #endregion
        
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
            
            if (timeLeft <= (_counter - tickInterval) || timeLeft <= 0)
            {
                _counter = (int)Math.Ceiling(timeLeft);
                //fire interval has lapsed, sending the current counter value;
                onTimerTick.Invoke(_counter);
            }
            
            if (_counter <= 0)
            {
                //fire event timer is done
                onTimerDone.Invoke();
            } 
        }
        public void Restart()
        {
            timeLeft = startingTime;
            _counter = (int)timeLeft;
            //fire event, sending the current counter value;
            onTimerTick.Invoke(_counter);
            //need to add one interval so it skips the first countdown method (analogy: making it count from 1 instead of 0)
            _counter += tickInterval;
        }
    }
}
