using System.Collections;
using System.Collections.Generic;

namespace TeddyToolKit.HighScore
{
    /// <summary>
    /// High score list using Insertion Sort
    /// worst case performance is O(n^2)
    /// Average is O(n^2)
    /// Best case is O(n)
    /// Very fast for small lists like around 10 elements
    /// slow for big lists
    /// useful for sorting high scores, or mostly already sorted lists
    /// stable as it does not change the existing order for equal values
    /// </summary>
    public class HighScoreList: IEnumerable<Score>
    {
        private List<Score> _scores = new List<Score>();

        /// <summary>
        /// Add a score to the highscore list
        /// </summary>
        /// <param name="score"></param>
        public void Add(Score score)
        {
            _scores.Add(score);
        }

        public int Count => _scores.Count;
        public void Clear() => _scores.Clear();
        
        /// <summary>
        /// Sort the highscore using insertion sort algorithm, instead of the default quicksort.
        /// </summary>
        /// <returns>null</returns>
        public void Sort(bool isAscending = false)
        {
            Score temp = null;
            //loop from 1 until the end, because 0 will be used as the first to compare
            for (int i = 1; i < _scores.Count; i++)
            {
                int j = i - 1;
                temp = _scores[i];
                //while j is bigger than the left most boundary
                if (isAscending)
                {
                    while (j >= 0 && (_scores[j] > temp))
                    {
                        //shift elements to the right
                        _scores[j + 1] = _scores[j];
                        //move to the left 
                        j--;
                    }
                }
                else
                {
                    while (j >= 0 && (_scores[j] < temp)) //reverse the sign to < so that it sorts big to small.
                    {
                        //shift elements to the right
                        _scores[j + 1] = _scores[j];
                        //move to the left 
                        j--;
                    }
                }

                //insert key to the right place
                _scores[j + 1] = temp;
                // yield return null;
            }
        }

        /// <summary>
        /// search for the score and return the rank in the list. this is done using sequential linear search because the list is small
        /// </summary>
        /// <param name="score">the score to search for</param>
        /// <returns>the rank of this score in the list, or -1 if not found</returns>
        public int getRank(Score score)
        {
            //sort before searching
            Sort(false);
            for (var i = 0; i < _scores.Count; i++)
            {
                if (_scores[i] == score)
                {
                    //return the rank, which is index +1 
                    return i + 1;
                }
            }

            return -1; //if not found in list
        } 

        /// <summary>
        /// returns the original list GetEnumerator
        /// </summary>
        /// <returns>IEnumerator Score</returns>
        public IEnumerator<Score> GetEnumerator()
        {
            return _scores.GetEnumerator();
        }

        /// <summary>
        /// returns the original get enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}