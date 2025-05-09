using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartBtn : Button
{
    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        switch (state)
        {
            case SelectionState.Normal:
                transform.localScale = new Vector3(.5f, .5f, .5f);
                break;
            case SelectionState.Highlighted:
                transform.localScale = new Vector3(.6f, .6f, .6f);
                break;
            case SelectionState.Pressed:
                break;
            case SelectionState.Selected:
                break;
            case SelectionState.Disabled:
                break;
            default:
                break;
        }
    }
}
