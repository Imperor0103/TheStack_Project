using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 따로 BaseUI를 사용할 것은 없으므로 abstract클래스로 만든다
public abstract class BaseUI : MonoBehaviour
{
    protected UIManager uiManager;

    public virtual void Init(UIManager uiManager)
    {
        this.uiManager = uiManager;
    }

    protected abstract UIState GetUIState();
    public void SetActive(UIState state)
    {
        gameObject.SetActive(GetUIState() == state);    // 가져온 UI상태와 state가 같으면 활성화, 다르면 비활성화
    }
}
