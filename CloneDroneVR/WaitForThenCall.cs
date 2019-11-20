using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLibrary;
using UnityEngine;
using ModLibrary.YieldInstructions;

public class WaitForThenCall : Singleton<WaitForThenCall>
{
    public static void Schedule(Action action, Func<bool> whatToWaitFor)
    {
        if (WaitForThenCall.Instance == null)
        {
            new GameObject("WaitForThenCallManager").AddComponent<WaitForThenCall>();
        }
        Tuple<Action, Func<bool>> tuple = new Tuple<Action, Func<bool>>(action, whatToWaitFor);
        WaitForThenCall.Instance._scheduledActions.Add(tuple);
    }

    void Update()
    {
        for(int i = 0; i<_scheduledActions.Count; i++)
        {
            Tuple<Action, Func<bool>> actions = _scheduledActions[i];
            if (actions.Item2())
            {
                actions.Item1();
                _scheduledActions.RemoveAt(i);
                i--;
            }
        }
    }

    List<Tuple<Action, Func<bool>>> _scheduledActions = new List<Tuple<Action, Func<bool>>>();
}
