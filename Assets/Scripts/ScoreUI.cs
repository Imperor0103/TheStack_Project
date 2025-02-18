using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreUI : BaseUI
{
    protected override UIState GetUIState()
    {
        return UIState.Score;
    }

    public override void Init(UIManager uiManager)
    {
        base.Init(uiManager);
    }
}
