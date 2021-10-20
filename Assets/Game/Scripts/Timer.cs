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
        public float timeLeft;

        private int _counter;

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
            if (_counter <= 0) return;
            timeLeft -= Time.deltaTime;
            if (timeLeft < _counter)
            {
                _counter = (int) timeLeft;
                //fire 1second has lapsed;
                EventManager.Instance.TriggerTimer1SecondTick(_counter);
            }
            if (_counter == 0)
            {
                //fire event timer is done
                EventManager.Instance.TriggerTimerDone();
            } 
        }
        public void Restart()
        {
            timeLeft = startingTime; 
            _counter = (int) timeLeft;
        }
    }
}
