using System.Collections.Generic;
using UnityEngine;

namespace SevenArk
{
    /// <summary>
    /// Time and action pairing
    /// </summary>
    public struct TimeNode
    {
        public float time;
        public System.Action callback;
    }
    /// <summary>
    /// Instance class returned from starting a timer
    /// </summary>
    public struct TimerInstance
    {
        public long id;
    }
    
    public class Timer : MonoBehaviour
    {
        private struct TimeInfo
        {
            public TimeNode[] nodes;
        }
        private struct TimeInfoIndexed
        {
            public int currentIndex;
            public TimeInfo info;
            public bool repeat;

            public bool useSequence;
            public float optionalSequenceTime;
            public IEnumerator<float> optionalSequence;
        }
        private Dictionary<long, TimeInfoIndexed> timers = new Dictionary<long, TimeInfoIndexed>();
        private static long idCurrent = 0;

        /// <summary>
        /// Delays for a time, then runs the action
        /// </summary>
        /// <param name="delay">Time to wait</param>
        /// <param name="action">Action to run</param>
        /// <returns>An instance of the timer</returns>
        public TimerInstance StartTimer(float delay, System.Action action)
        {
            TimerInstance instance = new TimerInstance()
            {
                id = idCurrent++
            };
            TimeNode[] timeNodes = new[] { new TimeNode() { time = delay, callback = action } };
            timers.Add(instance.id, new TimeInfoIndexed()
            {
                currentIndex = 0,
                info = new TimeInfo() { nodes = timeNodes }
            });

            return instance;
        }

        /// <summary>
        /// Delays for a time, then runs the action. Repeats.
        /// </summary>
        /// <param name="delay">Time to wait</param>
        /// <param name="action">Action to run</param>
        /// <returns>An instance of the timer</returns>
        public TimerInstance StartTimerRepeating(float delay, System.Action action)
        {
            TimerInstance instance = new TimerInstance()
            {
                id = idCurrent++
            };
            TimeNode[] timeNodes = new[] { new TimeNode() { time = delay, callback = action } };
            timers.Add(instance.id, new TimeInfoIndexed()
            {
                repeat = true,
                currentIndex = 0,
                info = new TimeInfo() { nodes = timeNodes }
            });

            return instance;
        }

        /// <summary>
        /// Runs multiple actions at a uniform delay time 
        /// </summary>
        /// <param name="uniformDelay">Time to wait between each action</param>
        /// <param name="actions">Actions to run</param>
        /// <returns>An instance of the timer</returns>
        public TimerInstance StartTimerSequence(float uniformDelay, params System.Action[] actions)
        {
            TimerInstance instance = new TimerInstance()
            {
                id = idCurrent++
            };
            TimeNode[] times = new TimeNode[actions.Length];
            for (int i = 0; i < times.Length; i++)
            {
                times[i] = new TimeNode()
                {
                    time = uniformDelay,
                    callback = actions[i]
                };
            }
            timers.Add(instance.id, new TimeInfoIndexed()
            {
                currentIndex = 0,
                info = new TimeInfo() { nodes = times }
            });

            return instance;
        }

        /// <summary>
        /// A series of nodes with times and actions to create a sequence
        /// </summary>
        /// <param name="sequences">The nodes for each action and time pair</param>
        /// <returns>An instance of the timer</returns>
        public TimerInstance StartTimerSequence(params TimeNode[] sequences)
        {
            TimerInstance instance = new TimerInstance()
            {
                id = idCurrent++
            };
            timers.Add(instance.id, new TimeInfoIndexed()
            {
                currentIndex = 0,
                info = new TimeInfo() { nodes = sequences }
            });

            return instance;
        }

        /// <summary>
        /// Runs an IEnumerator, where each yield return float is the time to wait between actions
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns>An instance of the timer</returns>
        public TimerInstance StartTimer(IEnumerator<float> sequence)
        {
            TimerInstance instance = new TimerInstance()
            {
                id = idCurrent++
            };
            timers.Add(instance.id, new TimeInfoIndexed()
            {
                useSequence = true,
                optionalSequenceTime = sequence.Current,
                optionalSequence = sequence
            });

            return instance;
        }

        /// <summary>
        /// Stops a timer from a given TimerInstance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>If the timer was found and stopped</returns>
        public bool StopTimer(TimerInstance instance)
        {
            if(timers.ContainsKey(instance.id))
            {
                timers.Remove(instance.id);
                return true;
            }
            return false;
        }

        private void Update()
        {
            List<long> keys = new List<long>(timers.Keys);
            //Go through all the timers set
            foreach(long key in keys)
            {
                if(timers[key].useSequence == false)
                {
                    //Whatever the current index is, reduce its time by deltaTime
                    timers[key].info.nodes[timers[key].currentIndex].time -= Time.deltaTime;

                    //If the timer has run out
                    if (timers[key].info.nodes[timers[key].currentIndex].time < 0)
                    {
                        //Run the callback
                        timers[key].info.nodes[timers[key].currentIndex].callback();

                        //Increase the index, if it's over the amount of nodes we have, delete it.
                        TimeInfoIndexed val = timers[key];
                        val.currentIndex++;
                        if (val.currentIndex >= val.info.nodes.Length)
                        {
                            if(val.repeat)
                            {
                                val.currentIndex = 0;
                                timers[key] = val;
                            }
                            else
                            {
                                timers.Remove(key);
                            }
                        }
                        else
                        {
                            timers[key] = val;
                        }
                    }
                }
                else
                {
                    //Decrease the time
                    TimeInfoIndexed val = timers[key];
                    val.optionalSequenceTime -= Time.deltaTime;

                    if(val.optionalSequenceTime < 0)
                    {
                        //If its time to move to the next step, do so and set the new time
                        if(val.optionalSequence.MoveNext())
                        {
                            val.optionalSequenceTime = val.optionalSequence.Current;
                            timers[key] = val;
                        }
                        else
                        {
                            timers.Remove(key);
                        }
                    }
                    else
                    {
                        timers[key] = val;
                    }
                }
            }
        }
    }
}
