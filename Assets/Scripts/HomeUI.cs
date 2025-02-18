using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeUI : BaseUI
{

    Button startButton;
    Button exitButton;


    protected override UIState GetUIState()
    {
        return UIState.Home;
    }
    public override void Init(UIManager uiManager)
    {
        base.Init(uiManager);

        // Find: GetChild와는 달리 path를 가져온다
        startButton = transform.Find("StartButton").GetComponent<Button>();
        exitButton = transform.Find("ExitButton").GetComponent<Button>();

        /// 에디터의 OnClick을 이용하지 않고 코드를 이용하는 방법
        startButton.onClick.AddListener(OnClickStartButton);
        exitButton.onClick.AddListener(OnClickExitButton);
    }
    void OnClickStartButton()
    {
        uiManager.OnClickStart();
    }

    void OnClickExitButton()
    {
        uiManager.OnClickExit();
    }

}
