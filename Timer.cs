using System.Collections.Generic;
using UnityEngine;

namespace SevenArk.Utilities
{
    /// <summary>
    /// Time and action pairing
    /// </summary>
    public class TimeNode
    {
        public float time;
        public System.Action callback;
    }
    public struct TimeInfo
    {
        public TimeNode[] nodes;
    }
    public class TimeInfoIndexed
    {
        public int currentIndex;
        public TimeInfo info;
        public bool repeat;
        public float delaySavedTime;

        public bool useSequence;
        public float optionalSequenceTime;
        public IEnumerator<float> optionalSequence;
    }

    public class Timer : MonoBehaviour
    {
        private List<TimeInfoIndexed> timers = new List<TimeInfoIndexed>();
        Queue<TimeInfoIndexed> added = new Queue<TimeInfoIndexed>();
        Queue<TimeInfoIndexed> removed = new Queue<TimeInfoIndexed>();
        private static long idCurrent = 0;

        /// <summary>
        /// Delays for a time, then runs the action
        /// </summary>
        /// <param name="delay">Time to wait</param>
        /// <param name="action">Action to run</param>
        /// <returns>An instance of the timer</returns>
        public TimeInfoIndexed StartTimer(float delay, System.Action action)
        {
            TimeNode[] timeNodes = new[] { new TimeNode() { time = delay, callback = action } };
            TimeInfoIndexed instance = new TimeInfoIndexed()
            {
                currentIndex = 0,
                info = new TimeInfo() { nodes = timeNodes }
            };
            added.Enqueue(instance);

            return instance;
        }

        /// <summary>
        /// Delays for a time, then runs the action. Repeats.
        /// </summary>
        /// <param name="delay">Time to wait</param>
        /// <param name="action">Action to run</param>
        /// <returns>An instance of the timer</returns>
        public TimeInfoIndexed StartTimerRepeating(float delay, System.Action action)
        {
            TimeNode[] timeNodes = new[] { new TimeNode() { time = delay, callback = action } };
            TimeInfoIndexed instance = new TimeInfoIndexed()
            {
                repeat = true,
                delaySavedTime = delay,
                currentIndex = 0,
                info = new TimeInfo() { nodes = timeNodes }
            };
            added.Enqueue(instance);

            return instance;
        }

        /// <summary>
        /// Runs multiple actions at a uniform delay time 
        /// </summary>
        /// <param name="uniformDelay">Time to wait between each action</param>
        /// <param name="actions">Actions to run</param>
        /// <returns>An instance of the timer</returns>
        public TimeInfoIndexed StartTimerSequence(float uniformDelay, params System.Action[] actions)
        {
            TimeNode[] times = new TimeNode[actions.Length];
            for (int i = 0; i < times.Length; i++)
            {
                times[i] = new TimeNode()
                {
                    time = uniformDelay,
                    callback = actions[i]
                };
            }
            TimeInfoIndexed instance = new TimeInfoIndexed()
            {
                currentIndex = 0,
                info = new TimeInfo() { nodes = times }
            };
            added.Enqueue(instance);

            return instance;
        }

        /// <summary>
        /// A series of nodes with times and actions to create a sequence
        /// </summary>
        /// <param name="sequences">The nodes for each action and time pair</param>
        /// <returns>An instance of the timer</returns>
        public TimeInfoIndexed StartTimerSequence(params TimeNode[] sequences)
        {
            TimeInfoIndexed instance = new TimeInfoIndexed()
            {
                currentIndex = 0,
                info = new TimeInfo() { nodes = sequences }
            };
            added.Enqueue(instance);

            return instance;
        }

        /// <summary>
        /// Runs an IEnumerator, where each yield return float is the time to wait between actions
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns>An instance of the timer</returns>
        public TimeInfoIndexed StartTimer(IEnumerator<float> sequence)
        {
            TimeInfoIndexed instance = new TimeInfoIndexed()
            {
                useSequence = true,
                optionalSequenceTime = sequence.Current,
                optionalSequence = sequence
            };
            added.Enqueue(instance);

            return instance;
        }

        /// <summary>
        /// Stops a timer from a given TimerInstance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>If the timer was found and stopped</returns>
        public bool StopTimer(TimeInfoIndexed instance)
        {
            if(timers.Contains(instance) && !removed.Contains(instance))
            {
                removed.Enqueue(instance);
                return true;
            }
            return false;
        }

        private void Update()
        {
            while(added.Count > 0)
            {
                timers.Add(added.Dequeue());
            }

            while(removed.Count > 0)
            {
                timers.Remove(removed.Dequeue());
            }

            for (int i = timers.Count - 1; i >= 0; i--)
            {
                TimeInfoIndexed currentTimer = timers[i];
                if (currentTimer.useSequence == false)
                {
                    TimeNode currentNode = currentTimer.info.nodes[currentTimer.currentIndex];
                    //Whatever the current index is, reduce its time by deltaTime
                    currentNode.time -= Time.deltaTime;

                    //If the timer has run out
                    if (currentNode.time <= 0)
                    {
                        //Run the callback
                        currentNode.callback();

                        //Increase the index, if it's over the amount of nodes we have, delete it.
                        if (currentTimer.repeat)
                        {
                            currentNode.time = currentTimer.delaySavedTime;
                        }
                        currentTimer.currentIndex++;
                        if (currentTimer.currentIndex >= currentTimer.info.nodes.Length)
                        {
                            if (currentTimer.repeat)
                            {
                                currentTimer.currentIndex = 0;
                            }
                            else
                            {
                                timers.RemoveAt(i);
                            }
                        }
                    }
                }
                else
                {
                    //Decrease the time
                    currentTimer.optionalSequenceTime -= Time.deltaTime;

                    if (currentTimer.optionalSequenceTime <= 0)
                    {
                        //If its time to move to the next step, do so and set the new time
                        if (currentTimer.optionalSequence.MoveNext())
                        {
                            currentTimer.optionalSequenceTime = currentTimer.optionalSequence.Current;
                        }
                        else
                        {
                            timers.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}

