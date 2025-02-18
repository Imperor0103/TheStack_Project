using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheStack : MonoBehaviour
{
    private const float BoundSize = 3.5f;   // 블록의 사이즈
    private const float MovingBoundSize = 3f;   // 이동하는 양
    private const float StackMovingSpeed = 5.0f;    // 스택의 이동 스피드
    private const float BlockMovingSpeed = 3.5f;    // 블록의 이동 스피드
    private const float ErrorMargin = 0.1f;     // 성공으로 취급할 기준

    public GameObject originBlock = null;   // 프리팹 연결

    private Vector3 prevBlockPosition;  // 이전 블록 위치
    private Vector3 desiredPosition;    // 이동해야하는 위치
    private Vector3 stackBounds = new Vector2(BoundSize, BoundSize);    // 생성할 블록의 사이즈

    // 새로운 블록을 생성하기 위한 변수들
    Transform lastBlock = null;
    float blockTransition = 0f;
    float secondaryPosition = 0f;

    int stackCount = -1;    // 스택에 쌓인 개수, 시작하면서 +1 하며 사용할 것이므로 -1로 초기화
    int comboCount = 0;

    public Color prevColor;    // 이전 블록 색깔
    public Color nextColor;    // 새롭게 생성되는 블록의 색깔

    bool isMovingX = true;  // x축 이동



    // Start is called before the first frame update
    void Start()
    {
        if (originBlock == null)
        {
            Debug.Log("OriginBlock is NULL");
            return;
        }

        // 처음에는 랜덤하게 색깔 정한다
        prevColor = GetRandomColor();
        nextColor = GetRandomColor();

        // stackCount가 -1인 상태에서 시작하므로, 1개 쌓고 시작한다
        prevBlockPosition = Vector3.down;

        Spawn_Block();  // 하나 생성
        Spawn_Block();  // 하나 생성
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceBlock())   // 블록이 잘 놓였다면
            {
                Spawn_Block();  // 하나 생성
            }
            else
            {
                // 게임 오버
                Debug.Log("Game Over");
            }
        }

        MoveBlock();

        // 블록이 쌓인 만큼 스택을 아래로 이동하는데, 끊어지지 않은 것처럼 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, desiredPosition, StackMovingSpeed * Time.deltaTime);
    }

    bool Spawn_Block()
    {
        if (lastBlock != null)
            prevBlockPosition = lastBlock.localPosition;    // TheStack에 오브젝트를 생성할 것이므로 TheStack안에서의 좌표를 사용하기 위해서이다
        // TheStack의 자식으로 오브젝트를 생성

        GameObject newBlock = null;
        Transform newTrans = null;

        newBlock = Instantiate(originBlock);    // 게임오브젝트 생성(부모가 없는 상태로 생성)

        if (newBlock == null)
        {
            Debug.Log("NewBlock Instantiate Failed");
            return false;
        }
        // newBlock이 생성되었으면 색깔을 바꾼다
        ColorChange(newBlock);

        // 블록 생성
        newTrans = newBlock.transform;
        newTrans.parent = this.transform;   // 새로 생성된 오브젝트의 부모를 this(TheStack)으로 설정
        newTrans.localPosition = prevBlockPosition + Vector3.up;    // 부모를 기준으로 이동, y가 1이므로 1만 올리면 충분하다
        newTrans.localRotation = Quaternion.identity;   // 초기값은 Quaternion.identity(회전이 없는 상태)
        newTrans.localScale = new Vector3(stackBounds.x, 1, stackBounds.y); // stackBounds: 새롭게 생성되는 블록의 사이즈

        stackCount++;

        desiredPosition = Vector3.down * stackCount;    // 스택이 쌓이면, TheStack을 그만큼 아래로 내려서 가장 나중에 쌓인 것(top)의 위치를 화면 중앙에 고정한다
        blockTransition = 0f;   // 이동에 대한 처리를 하기 위한 기준값

        lastBlock = newTrans;   // 새로 만든 것이 last블록

        isMovingX = !isMovingX; // block이 새로 생성되었다면 bool값을 반대로

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
        // 0~11까지의 값이 순환하며 이를 다시 10으로 나누니 0부터 1 사이의 값이 나온다

        Renderer rn = go.GetComponent<Renderer>();

        if (rn == null)
        {
            Debug.Log("Renderer is NULL");
            return;
        }

        rn.material.color = applyColor; // material의 색깔을 바꾼다
        Camera.main.backgroundColor = applyColor - new Color(0.1f, 0.1f, 0.1f); // 블록과 카메라의 배경색이 동일하면 구분되지 않기때문에 미세하게 구분해준다

        if (applyColor.Equals(nextColor) == true)   // (stackCount % 11)가 10인 경우에 applyColor와 nextColor이 동일하다(lerp에 의해 1이면 nextColor)
        {
            prevColor = nextColor;
            nextColor = GetRandomColor();   // GetRandomColor로 새로운 색깔을 가져온다
        }
    }

    void MoveBlock()
    {
        blockTransition += Time.deltaTime * BlockMovingSpeed;

        // Mathf.PingPong(blockTransition, BoundSize) 는 양수인 범위에서 진동하는 함수다
        float movePosition = Mathf.PingPong(blockTransition, BoundSize) - BoundSize / 2;  // BoundSize의 절반만큼 빼야지 sin함수같이 -+ 진동한다
        // movePosition은 블록의 사이즈다. 블록의 사이즈만큼 이동하기 위해서이다

        if (isMovingX)
        {
            // x축 이동
            lastBlock.localPosition = new Vector3(
                movePosition * MovingBoundSize, stackCount, secondaryPosition); // y가 stackCount인 이유는, 높이는 변하지 않기 때문
        }
        else
        {
            // z축 이동
            lastBlock.localPosition = new Vector3(
                secondaryPosition, stackCount, movePosition * MovingBoundSize);
        }
    }

    bool PlaceBlock()
    {
        Vector3 lastPosition = lastBlock.localPosition;
        if (isMovingX)
        {
            // x축 이동할 때 놓기
            // deltaX: 잘려나가야 하는 크기
            float deltaX = prevBlockPosition.x - lastPosition.x;
            // top에 있는 블록(prevBlock)과 새로 쌓은 블록(last)의 중심 차이가 벗어난 정도이다

            // Rubble(파편)을 생성하는 방향을 지정한다
            // 블록이 진행하는 방향에서 떨어져야할지? 진행하고 다가오는 방향에서 떨어져야할지?
            //bool isNegativeNum = (deltaX < 0) ? true : false;



            deltaX = Mathf.Abs(deltaX); // 음수인 경우에 절대값
            if (deltaX > ErrorMargin) // 오차범위 벗어나면
            {
                stackBounds.x -= deltaX;    // 다음에 생성할 블록의 사이즈가 줄어든다
                if (stackBounds.x <= 0)
                {
                    return false;
                }
                // 다음에 블록을 생성할 수 있는 경우 중심은 두 블록의 중심의 중점
                float middle = (prevBlockPosition.x + lastPosition.x) / 2;
                lastBlock.localScale = new Vector3(stackBounds.x, 1, stackBounds.y); // 크기

                Vector3 tempPosition = lastBlock.localPosition;
                tempPosition.x = middle;
                lastBlock.localPosition = lastPosition = tempPosition;  // 오른쪽부터 대입, lastBlock의 위치를 바꾸는 과정

                //// x방향 파편 생성
                //float rubbleHalfScale = deltaX / 2f;
                //CreateRubble(
                //    new Vector3(isNegativeNum
                //    ? lastPosition.x + stackBounds.x / 2 + rubbleHalfScale
                //    : lastPosition.x - stackBounds.x / 2 - rubbleHalfScale
                //    , lastPosition.y
                //    , lastPosition.z),
                //    new Vector3(deltaX, 1, stackBounds.y)
                //);

            }
            else
            {
                lastBlock.localPosition = prevBlockPosition + Vector3.up;
            }
        }
        else
        {
            // z축 이동할 때 놓기
            float deltaZ = prevBlockPosition.z - lastPosition.z;    // top에 있는 블록(prevBlock)과 새로 쌓은 블록(last)의 중심 차이가 벗어난 정도이다

            deltaZ = Mathf.Abs(deltaZ); // 음수인 경우에 절대값
            if (deltaZ > ErrorMargin) // 오차범위 벗어나면
            {
                stackBounds.y -= deltaZ;    // 다음에 생성할 블록의 사이즈가 줄어든다
                if (stackBounds.y <= 0)
                {
                    return false;
                }
                // 다음에 블록을 생성할 수 있는 경우 중심은 두 블록의 중심의 중점
                float middle = (prevBlockPosition.z + lastPosition.z) / 2f;
                lastBlock.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);    // 크기

                Vector3 tempPosition = lastBlock.localPosition;
                tempPosition.z = middle;
                lastBlock.localPosition = lastPosition = tempPosition;  // 오른쪽부터 대입, lastBlock의 위치를 바꾸는 과정
            }
            else
            {
                lastBlock.localPosition = prevBlockPosition + Vector3.up;
            }
        }

        secondaryPosition = (isMovingX) ? lastBlock.localPosition.x : lastBlock.localPosition.z;
        // 사용 이유
        // secondaryPosition을 MoveBlock에서 사용하고 있다
        // 이전 블록의 위치가 계속 바뀌기 때문에, 중심인 0의 위치를 계속 사용할 수 없다
        // 이동시킨 블록이 x축 이동인 경우 x값, z축 이동인 경우 z값을 저장했다가
        // MoveBlock에서 사용하고 있는 것이다

        return true;
    }

    // 파편(잘려나간 블록) 만든다
    void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = Instantiate(lastBlock.gameObject);
        go.transform.parent = this.transform;   // 부모는 TheStack

        // 초기화
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.transform.localRotation = Quaternion.identity;    // 회전은 없다

        // 파편은 바닥에 딸어져야한다
        go.AddComponent<Rigidbody>();
        go.name = "Rubble";

    }

}
