using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���� BaseUI�� ����� ���� �����Ƿ� abstractŬ������ �����
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
        gameObject.SetActive(GetUIState() == state);    // ������ UI���¿� state�� ������ Ȱ��ȭ, �ٸ��� ��Ȱ��ȭ
    }
}
