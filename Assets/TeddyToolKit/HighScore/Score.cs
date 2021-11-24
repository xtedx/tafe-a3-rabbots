using System;
using UnityEngine;

namespace TeddyToolKit.HighScore
{
    ///<summary>
    /// A score contains a value, name and the datetime
    /// it has an integer value, a string name and datetime timestamp
    /// by default, all operators are to calculate the value, keeping the name and date intact
    /// usage:
    /// use the score class in a list and run Sort() to have it sorted by the highest score at the top.
    ///</summary>
    [Serializable]
    public class Score : IComparable<Score>
    {
        [SerializeField] public int _value = 0;
        [SerializeField] public string _name = "";
        [SerializeField] private DateTime _dateTime = DateTime.Now;

        ///<summary>constructor with int value and name</summary>
        /// <param name="value">the value of this score</param>
        /// <param name="name">name of the achiever of this score. defaults to "AAA"</param>
        public Score(int value, string name = "AAA")
        {
            _value = value;
            _name = name;
            _dateTime = DateTime.Now;
        }

        #region overloading

        /// <summary>
        /// implementation of iComparable for sorting purpose
        /// </summary>
        /// <param name="other">the other score to compare to</param>
        /// <returns>1 if other is greater, -1 if less, 0 if equal</returns>
        public int CompareTo(Score other)
        {
            /* lambda version, shorter but harder to read for me
             * public int CompareTo(HighScore other) => _other - this;
             * normal version is used below
             */ 
            if (other._value > _value)
            {
                return 1;
            }
            else if (other._value < _value)
            {
                return -1;
            }

            return 0;
        }

        public override string ToString()
        {
            return $"Score: {_value}\tName: {_name}\tTime: {_dateTime}";
        }

        ///<summary>expose internal value as int publicly</summary>
        public static implicit operator int(Score score) => score._value;

        ///<summary>override operators in lambda notation</summary>
        /// <param name="score">this score</param>
        /// <param name="other">the integer to add</param>
        public static Score operator +(Score score, int other)
            => new Score(score._value + other);

        ///<summary>override operators in lambda notation</summary>
        /// <param name="score">this score</param>
        /// <param name="other">the integer to add</param>
        public static Score operator -(Score score, int other)
            => new Score(score._value - other);

        ///<summary>override operators in lambda notation
        ///logical operator must be done in pairs because they need to know if it is false?</summary>
        /// <param name="score">this score</param>
        /// <param name="other">the other to compare for sorting</param>
        public static bool operator >(Score score, Score other)
            => score._value > other._value;

        ///<summary>override operators in lambda notation
        ///logical operator must be done in pairs because they need to know if it is false?</summary>
        /// <param name="score">this score</param>
        /// <param name="other">the other to compare for sorting</param>
        public static bool operator <(Score score, Score other)
            => score._value < other._value;

        #endregion
    }
}