using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SevenArk;

public class Example : MonoBehaviour
{
    Timer timer;
    void Start()
    {
        timer = GetComponent<Timer>();

        timer.StartTimer(1, () => { Log("First"); });
        timer.StartTimerSequence(1, () => { Log("Second1"); }, () => { Log("Second2"); }, () => { Log("Second3"); });
        timer.StartTimerSequence(new TimeNode()
        {
            time = 2,
            callback = () => { Log("Three1"); }
        },
        new TimeNode()
        {
            time = 1,
            callback = () => { Log("Three2"); }
        },
        new TimeNode()
        {
            time = 3,
            callback = () => { Log("Three3"); }
        });

        timer.StartTimer(Test());
	
	timer.StartTimerRepeating(1, () => { Log("GO"); });
	}

    void Log(string text)
    {
        Debug.Log(text + " : " + Time.time);
    }

    IEnumerator<float> Test()
    {
        yield return 1;
        Debug.Log("Test1");
        yield return 1;
        Debug.Log("Test2");
        yield return 1;
        Debug.Log("Test3");
        yield return 1;
        Debug.Log("Test4");
    }
}
