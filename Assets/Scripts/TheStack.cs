using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheStack : MonoBehaviour
{
    private const float BoundSize = 3.5f;   // ����� ������
    private const float MovingBoundSize = 3f;   // �̵��ϴ� ��
    private const float StackMovingSpeed = 5.0f;    // ������ �̵� ���ǵ�
    private const float BlockMovingSpeed = 3.5f;    // ����� �̵� ���ǵ�
    private const float ErrorMargin = 0.1f;     // �������� ����� ����

    public GameObject originBlock = null;   // ������ ����

    private Vector3 prevBlockPosition;  // ���� ��� ��ġ
    private Vector3 desiredPosition;    // �̵��ؾ��ϴ� ��ġ
    private Vector3 stackBounds = new Vector2(BoundSize, BoundSize);    // ������ ����� ������

    // ���ο� ����� �����ϱ� ���� ������
    Transform lastBlock = null;
    float blockTransition = 0f;
    float secondaryPosition = 0f;

    int stackCount = -1;    // ���ÿ� ���� ����, �����ϸ鼭 +1 �ϸ� ����� ���̹Ƿ� -1�� �ʱ�ȭ
    int comboCount = 0;

    public Color prevColor;    // ���� ��� ����
    public Color nextColor;    // ���Ӱ� �����Ǵ� ����� ����


    // Start is called before the first frame update
    void Start()
    {
        if (originBlock == null)
        {
            Debug.Log("OriginBlock is NULL");
            return;
        }

        // ó������ �����ϰ� ���� ���Ѵ�
        prevColor = GetRandomColor();
        nextColor = GetRandomColor();

        // stackCount�� -1�� ���¿��� �����ϹǷ�, 1�� �װ� �����Ѵ�
        prevBlockPosition = Vector3.down;

        Spawn_Block();  // �ϳ� ����
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Spawn_Block();  // �ϳ� ����
        }

        // ����� ���� ��ŭ ������ �Ʒ��� �̵��ϴµ�, �������� ���� ��ó�� �ε巴�� �̵�
        transform.position = Vector3.Lerp(transform.position, desiredPosition, StackMovingSpeed * Time.deltaTime);
    }

    bool Spawn_Block()
    {
        if (lastBlock != null)
            prevBlockPosition = lastBlock.localPosition;    // TheStack�� ������Ʈ�� ������ ���̹Ƿ� TheStack�ȿ����� ��ǥ�� ����ϱ� ���ؼ��̴�
        // TheStack�� �ڽ����� ������Ʈ�� ����

        GameObject newBlock = null;
        Transform newTrans = null;

        newBlock = Instantiate(originBlock);    // ���ӿ�����Ʈ ����(�θ� ���� ���·� ����)

        if (newBlock == null)
        {
            Debug.Log("NewBlock Instantiate Failed");
            return false;
        }
        // newBlock�� �����Ǿ����� ������ �ٲ۴�
        ColorChange(newBlock);

        // ��� ����
        newTrans = newBlock.transform;
        newTrans.parent = this.transform;   // ���� ������ ������Ʈ�� �θ� this(TheStack)���� ����
        newTrans.localPosition = prevBlockPosition + Vector3.up;    // �θ� �������� �̵�, y�� 1�̹Ƿ� 1�� �ø��� ����ϴ�
        newTrans.localRotation = Quaternion.identity;   // �ʱⰪ�� Quaternion.identity(ȸ���� ���� ����)
        newTrans.localScale = new Vector3(stackBounds.x, 1, stackBounds.y); // stackBounds: ���Ӱ� �����Ǵ� ����� ������

        stackCount++;

        desiredPosition = Vector3.down * stackCount;    // ������ ���̸�, TheStack�� �׸�ŭ �Ʒ��� ������ ���� ���߿� ���� ��(top)�� ��ġ�� ȭ�� �߾ӿ� �����Ѵ�
        blockTransition = 0f;   // �̵��� ���� ó���� �ϱ� ���� ���ذ�

        lastBlock = newTrans;   // ���� ���� ���� last���

        return true;
    }

    Color GetRandomColor()
    {
        float r = Random.Range(100f, 250f) / 255f;
        float g = Random.Range(100f, 250f) / 255f;
        float b = Random.Range(100f, 250f) / 255f;

        return new Color(r, g, b);
    }

    void ColorChange(GameObject go)
    {
        Color applyColor = Color.Lerp(prevColor, nextColor, (stackCount % 11) / 10f);
        // 0~11������ ���� ��ȯ�ϸ� �̸� �ٽ� 10���� ������ 0���� 1 ������ ���� ���´�

        Renderer rn = go.GetComponent<Renderer>();

        if (rn == null)
        {
            Debug.Log("Renderer is NULL");
            return;
        }

        rn.material.color = applyColor; // material�� ������ �ٲ۴�
        Camera.main.backgroundColor = applyColor - new Color(0.1f, 0.1f, 0.1f); // ��ϰ� ī�޶��� ������ �����ϸ� ���е��� �ʱ⶧���� �̼��ϰ� �������ش�

        if (applyColor.Equals(nextColor) == true)   // (stackCount % 11)�� 10�� ��쿡 applyColor�� nextColor�� �����ϴ�(lerp�� ���� 1�̸� nextColor)
        {
            prevColor = nextColor;
            nextColor = GetRandomColor();   // GetRandomColor�� ���ο� ������ �����´�
        }
    }


}
