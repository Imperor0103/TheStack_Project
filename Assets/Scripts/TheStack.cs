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
    public int Score { get { return stackCount; } }

    int comboCount = 0;
    public int Combo { get { return comboCount; } }

    private int maxCombo = 0;
    public int MaxCombo { get => maxCombo; }    // lambda�� �ۼ�


    public Color prevColor;    // ���� ��� ����
    public Color nextColor;    // ���Ӱ� �����Ǵ� ����� ����

    bool isMovingX = true;  // x�� �̵�

    int bestScore = 0;
    public int BestScore { get => bestScore; }

    int bestCombo = 0;
    public int BestCombo { get => bestCombo; }

    // PlayerPrefs�� ����� �� �ʿ��� key��
    private const string BestScoreKey = "BestScore";
    private const string BestComboKey = "BestCombo";

    private bool isGameOver = true;    // ���ӿ����� ������ ����(ó������ �������� �����Ƿ� true)


    // Start is called before the first frame update
    void Start()
    {
        if (originBlock == null)
        {
            Debug.Log("OriginBlock is NULL");
            return;
        }

        /// PlayerPrefs�� ����� ������ �ִٸ� �ҷ��´�
        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        bestCombo = PlayerPrefs.GetInt(BestComboKey, 0);

        // ó������ �����ϰ� ���� ���Ѵ�
        prevColor = GetRandomColor();
        nextColor = GetRandomColor();

        // stackCount�� -1�� ���¿��� �����ϹǷ�, 1�� �װ� �����Ѵ�
        prevBlockPosition = Vector3.down;

        Spawn_Block();  // ó����� ����
        Spawn_Block();  // �̵���� ����
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceBlock())   // ����� �� �����ٸ�
            {
                Spawn_Block();  // �ϳ� ����
            }
            else
            {
                // ���� ����
                Debug.Log("Game Over");
                UpdateScore();  // ���� ���� ����
                isGameOver = true;
                GameOverEffect();
            }
        }

        MoveBlock();

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

        isMovingX = !isMovingX; // block�� ���� �����Ǿ��ٸ� bool���� �ݴ��

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

    void MoveBlock()
    {
        blockTransition += Time.deltaTime * BlockMovingSpeed;

        // Mathf.PingPong(blockTransition, BoundSize) �� ����� �������� �����ϴ� �Լ���
        float movePosition = Mathf.PingPong(blockTransition, BoundSize) - BoundSize / 2;  // BoundSize�� ���ݸ�ŭ ������ sin�Լ����� -+ �����Ѵ�
        // movePosition�� ����� �������. ����� �����ŭ �̵��ϱ� ���ؼ��̴�

        if (isMovingX)
        {
            // x�� �̵�
            lastBlock.localPosition = new Vector3(
                movePosition * MovingBoundSize, stackCount, secondaryPosition); // y�� stackCount�� ������, ���̴� ������ �ʱ� ����
        }
        else
        {
            // z�� �̵�
            lastBlock.localPosition = new Vector3(
                secondaryPosition, stackCount, movePosition * MovingBoundSize);
        }
    }

    bool PlaceBlock()
    {
        Vector3 lastPosition = lastBlock.localPosition;
        if (isMovingX)
        {
            // x�� �̵��� �� ����
            // deltaX: �߷������� �ϴ� ũ��
            float deltaX = prevBlockPosition.x - lastPosition.x;
            // top�� �ִ� ���(prevBlock)�� ���� ���� ���(last)�� �߽� ���̰� ��� �����̴�

            // Rubble(����)�� �����ϴ� ������ �����Ѵ�
            // ����� �����ϴ� ���⿡�� ������������? �����ϰ� �ٰ����� ���⿡�� ������������?
            bool isNegativeNum = (deltaX < 0) ? true : false;



            deltaX = Mathf.Abs(deltaX); // ������ ��쿡 ���밪
            if (deltaX > ErrorMargin) // �������� �����
            {
                stackBounds.x -= deltaX;    // ������ ������ ����� ����� �پ���
                if (stackBounds.x <= 0)
                {
                    return false;
                }
                // ������ ����� ������ �� �ִ� ��� �߽��� �� ����� �߽��� ����
                float middle = (prevBlockPosition.x + lastPosition.x) / 2;
                lastBlock.localScale = new Vector3(stackBounds.x, 1, stackBounds.y); // ũ��

                Vector3 tempPosition = lastBlock.localPosition;
                tempPosition.x = middle;
                lastBlock.localPosition = lastPosition = tempPosition;  // �����ʺ��� ����, lastBlock�� ��ġ�� �ٲٴ� ����

                // x���� ���� ����
                float rubbleHalfScale = deltaX / 2f;
                CreateRubble(
                    new Vector3(isNegativeNum
                    ? lastPosition.x + stackBounds.x / 2 + rubbleHalfScale  // ���ο� �߽���: �߸� �κ��� �߽ɰ� �߷����� �κ��� �߽��� ���
                    : lastPosition.x - stackBounds.x / 2 - rubbleHalfScale
                    , lastPosition.y
                    , lastPosition.z),
                    new Vector3(deltaX, 1, stackBounds.y)
                );

                comboCount = 0; // �޺� �ʱ�ȭ
            }
            else
            {
                ComboCheck();   // �޺� �߰�
                lastBlock.localPosition = prevBlockPosition + Vector3.up;
            }
        }
        else
        {
            // z�� �̵��� �� ����
            float deltaZ = prevBlockPosition.z - lastPosition.z;    // top�� �ִ� ���(prevBlock)�� ���� ���� ���(last)�� �߽� ���̰� ��� �����̴�
            bool isNegativeNum = (deltaZ < 0) ? true : false;

            deltaZ = Mathf.Abs(deltaZ); // ������ ��쿡 ���밪
            if (deltaZ > ErrorMargin) // �������� �����
            {
                stackBounds.y -= deltaZ;    // ������ ������ ����� ����� �پ���
                if (stackBounds.y <= 0)
                {
                    return false;
                }
                // ������ ����� ������ �� �ִ� ��� �߽��� �� ����� �߽��� ����
                float middle = (prevBlockPosition.z + lastPosition.z) / 2f;
                lastBlock.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);    // ũ��

                Vector3 tempPosition = lastBlock.localPosition;
                tempPosition.z = middle;
                lastBlock.localPosition = lastPosition = tempPosition;  // �����ʺ��� ����, lastBlock�� ��ġ�� �ٲٴ� ����

                float rubbleHalfScale = deltaZ / 2f;
                CreateRubble(
                    new Vector3(
                        lastPosition.x
                        , lastPosition.y
                        , isNegativeNum
                            ? lastPosition.z + stackBounds.y / 2 + rubbleHalfScale
                            : lastPosition.z - stackBounds.y / 2 - rubbleHalfScale),
                    new Vector3(stackBounds.x, 1, deltaZ)
                );

                comboCount = 0;
            }
            else
            {
                ComboCheck();
                lastBlock.localPosition = prevBlockPosition + Vector3.up;
            }
        }

        secondaryPosition = (isMovingX) ? lastBlock.localPosition.x : lastBlock.localPosition.z;
        // ��� ����
        // secondaryPosition�� MoveBlock���� ����ϰ� �ִ�
        // ���� ����� ��ġ�� ��� �ٲ�� ������, �߽��� 0�� ��ġ�� ��� ����� �� ����
        // �̵���Ų ����� x�� �̵��� ��� x��, z�� �̵��� ��� z���� �����ߴٰ�
        // MoveBlock���� ����ϰ� �ִ� ���̴�

        return true;
    }

    // ����(�߷����� ���) �����
    void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = Instantiate(lastBlock.gameObject);
        go.transform.parent = this.transform;   // �θ�� TheStack

        // �ʱ�ȭ
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.transform.localRotation = Quaternion.identity;    // ȸ���� ����

        // ������ �ٴڿ� ���������Ѵ�
        go.AddComponent<Rigidbody>();
        go.name = "Rubble";

    }

    void ComboCheck()
    {
        comboCount++;

        if (comboCount > maxCombo)
            maxCombo = comboCount;

        if ((comboCount % 5) == 0)
        {
            Debug.Log("5Combo Success!");
            stackBounds += new Vector3(0.5f, 0.5f);
            // 5�޺� �޼��ϸ� size�� ���ݾ� �ø���! (�������̸� �ʰ������� ����)
            stackBounds.x =
                (stackBounds.x > BoundSize) ? BoundSize : stackBounds.x;
            stackBounds.y =
                (stackBounds.y > BoundSize) ? BoundSize : stackBounds.y;
        }
    }

    void UpdateScore()
    {
        if (bestScore < stackCount)
        {
            Debug.Log("�ְ� ���� ����");
            bestScore = stackCount;
            bestCombo = maxCombo;

            // ����
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
            PlayerPrefs.SetInt(BestComboKey, bestCombo);
        }
    }

    void GameOverEffect()
    {
        int childCount = this.transform.childCount; // this.transform�� ������ �ִ� ������Ʈ ����
        // This�� TheStack�̹Ƿ�, �� ������ �ִ� ��ϵ�� Rubble(����)���� ����

        // ���� 20���� effect ����
        for (int i = 1; i < 20; i++)
        {
            // childCount�� 20������ ū ��쿡 ���� 20���� ������ �� �ִ�
            if (childCount < i) break;  // �Ʒ��� childCount - i�� ���ϰ� �Ǿ� �ε������� �����

            GameObject go =
                this.transform.GetChild(childCount - i).gameObject;

            // ������ ��쿡�� ȿ���� �������� �ʴ´�
            if (go.name.Equals("Rubble")) continue;

            Rigidbody rigid = go.AddComponent<Rigidbody>();

            // ���� �����ؼ� ���� ����������
            rigid.AddForce(
                (Vector3.up * Random.Range(0, 10f) + Vector3.right * (Random.Range(0, 10f) - 5f))
                * 100f
            );
        }
    }
    public void Restart()
    {
        // ������� ���� ��� �ʱ�ȭ

        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        isGameOver = false;

        lastBlock = null;
        desiredPosition = Vector3.zero;
        stackBounds = new Vector3(BoundSize, BoundSize);

        stackCount = -1;
        isMovingX = true;
        blockTransition = 0f;
        secondaryPosition = 0f;

        comboCount = 0;
        maxCombo = 0;

        prevBlockPosition = Vector3.down;

        prevColor = GetRandomColor();
        nextColor = GetRandomColor();

        Spawn_Block();  // ó����� ����
        Spawn_Block();  // �̵���� ����
    }

}
