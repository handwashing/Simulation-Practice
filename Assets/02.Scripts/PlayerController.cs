using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //스피드 조정 변수
    [SerializeField] //SerializeField/ private상태지만 인스펙터 창에서 수정 가능
    private float walkSpeed; //움직임 제어
    [SerializeField] 
    private float runSpeed; //달리기
    [SerializeField] 
    private float crouchSpeed; //앉기
    private float applySpeed; //움직임 제어 / 이것 하나만 있어도 대입만 하면 됨...(여러개의 함수 쓸 필요없다)

    [SerializeField]
    private float jumpForce; //순간적으로 얼마만큼(의 힘으로) 위로 치솟게 할지...

    //상태 변수
    private bool isRun = false; //뛰고있는지를 확인 false가 기본값임(안 써도 ok)
    private bool isCrouch = false; //앉았는지...
    private bool isGround = true; //땅이 있는지...

    //앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField] 
    private float crouchPosY; //y값을 감소시켜 숙인것처럼...
    private float originPosY; //숙였다가 다시 원래 대로 돌아갈 값...
    private float applyCrouchPosY; //위에있는 각각의 값을 넣어줘서 사용...

    //땅 착지 여부
    private CapsuleCollider capsuleCollider; //캡슐 콜라이더가 Mesh콜라이더와 맞닿아 있을 경우가 true임(지상)...

    //민감도
    [SerializeField]
    private float lookSensitivity; //카메라 민감도

    //카메라의 한계
    [SerializeField]
    private float cameraRotationLimit; //고개를 들었을 때 각도 제한
    private float currentCameraRotationX = 0f; //0을 줘서 정면 보도록(생략해도 ok)

    //필요한 컴퍼넌트
    [SerializeField]
    private Camera theCamera; //camera component

    //플레이어의 실제 육체(몸)
    //콜라이더로 충돌 영역 설정, 리지드바디로 콜라이더에 물리적 기능 추가
    private Rigidbody myRigid;

    void Start()
    {
        //하이어라키의 객체를 뒤져서 카메라 컴퍼넌트가 있다면 theCamera에 
        //찾아서 넣어주기 -> theCamera = FindObjectOfType<Camera>(); 
        //카메라가 여래개일 수 있으니 프로젝트창에 직접 드래그했음...
        
        capsuleCollider = GetComponent<CapsuleCollider>(); //플레이어가 캡슐 콜라이더를 통제할 수 있도록 가져오기...
        //리지드바디 컴퍼넌트를 마이리지드 변수에 넣겠다
        myRigid = GetComponent<Rigidbody>();
        applySpeed = walkSpeed; //달리기 전까지는 걷는 상태
        //초기화
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY; //기본 서있는 상태로 초기화
    }

    void Update()
    {
        IsGround();
        TryJump();
        TryRun(); //뛰거나 걷는것을 구분하는 함수(판단 후 움직임 제어 / 순서주의)
        TryCouch(); //앉으려고 시도
        Move(); //키입력에 따라 움직임이 실시간으로 이루어지게하는 처리
        CameraRotation(); 
        CharacterRotation();
    }

    private void TryCouch() //앉기 시도
    {
        //좌측에 있는 crtl키를 눌러야 발동
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }

    }

    private void Crouch() //앉기 동작
    {//isCrouch가 실행될때마다 반전시키기
        isCrouch = !isCrouch;
        //if (isCrouch) //isCrouch가 true면 false로 바꿔주기
        //     isCrouch = false;
        // else
        //     isCrouch = true; //그렇지 않으면 true 
        //이렇게도 쓸 수 있다!

        if (isCrouch) //isCrouch가 트루면 앉는 모션으로...
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {//아니라면...
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        StartCoroutine(CrouchCoroutine());
        //벡터값은 카메라의 현재 x,z값 그리고 바뀔 Y값을 가지게한다...
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
    }
    private void TryJump() //점프 시도
    {//스페이스바를 한 번 눌렀을 경우 / 땅위에 있을 경우에...
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }

    private void Jump() //점프
    {
        if(isCrouch)  //앉은 상태에서 점프시 앉은 상태 해제
            Crouch(); //앉아있다 점프 했을 때...  플레이어를 일어난 상태로...
    //벨로서티 (현재 어느방향, 속도로 움직이는지...)를 변경해
    //jumpForce만큼 순간적으로 위로 향하게 만들기...
        myRigid.velocity = transform.up * jumpForce;
    }

    IEnumerator CrouchCoroutine() //부드러운 앉기 동작(카메라 이동 처리...)
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while(_posY != applyCrouchPosY) //posY가 원하는 값이 되면 벗어나도록(아니면 반복...)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f); //보관하기 _posY부터applyCrouchPosY까지 0.3f씩 증가
            theCamera.transform.localPosition = new Vector3(0, _posY, 0); //변경된 포스값을 카메라에 적용...
            if (count > 15)
                break; //무한반복 방지위해 보관 범위를 지정해줌...
            yield return null; //1프레임마다 쉬기 / _posY가 목적지까지 가면 while문에서 빠져 나옴...
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
    }
    private void IsGround() //지면 체크
    {//고정된 좌표를 향해 y 반절의 거리만큼 (아래방향으로) 레이저 쏘기
    //-> 지면과 닿게 됨...isGround는 true를 반환해 점프할 수 있는 상태가 됨...
    //지면의 경사에 따라 오차가 생기는 것을 방지하기 위해 여유주기 /+0.1f/
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider .bounds.extents.y + 0.1f);
    }


    private void TryRun() //달리기 시도
    {//shitf키를 누르면 달릴 수 있도록...
        if(Input.GetKey(KeyCode.LeftShift)) //LeftShift 를 누르게 되면
        {
            Running();
        }
        if(Input.GetKeyUp(KeyCode.LeftShift)) //LeftShift에서 손을 떼면
        {
            RunningCancel();
        }
    }

    private void Running() //달리기 실행
    {
        if(isCrouch)  //앉은 상태에서 달릴때 앉은 상태 해제
            Crouch(); 
        isRun = true; 
        applySpeed = runSpeed; //스피드가 RunSpeed로 바뀜
    }

    private void RunningCancel() //달리기 취소
    {
        isRun = true; 
        applySpeed = walkSpeed; //걷는 속도
    }
    private void Move() //움직임 실행
    {//상하좌우...move...
     
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        //벡터값을 이용해 실제 움직이도록...
        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;
        //normalized를 써서 x와 z값의 합이 1이 나오도록 한다
        //계산을 편하게 하기 위해 합이 1이 나오도록 정규화 하는 것이 나음...

        //나온 값을 myRigid에 무브포지션 메서드를 이용해 움직이도록 구현
        //현위치에서 velocity만큼 움직임 / 순간이동하듯 움직이는 것을 방지하기 위해
        //타임.델타타임으로 쪼개준다
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    private void CharacterRotation()
    {   //좌우 캐릭터 회전
        float _yRotation = Input.GetAxisRaw("Mouse X"); //마우스가 좌우로 움직이는 경우
        //민감도 - 위아래 - 좌우 똑같이 설정
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY)); //unity는 회전을 내부적으로 Quaternion 사용 
        //Debug.Log(myRigid.rotation);
        //Debug.Log(myRigid.rotation.eulerAngles);
    }
    private void CameraRotation()
    {   //상하 카메라 회전

        //마우스는 2차원임 x,y만 존재~
        float _xRotation = Input.GetAxisRaw("Mouse Y"); //위 아래로 고개를 드는 것
        float _cameraRotationX = _xRotation * lookSensitivity; //순식간에 움직이는 것을 방지 / 천천히 움직이도록...
        //currentCameraRotationX += _cameraRotationX;   <이렇게 하면 마우스 방향과 화면이 반전된다...
        currentCameraRotationX -= _cameraRotationX; 
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit); 
        //limit 만큼만 움직이게 가두기...(currentCameraRotationX가 -cameraRotationLimit, cameraRotationLimit 사이의 값으로 고정되게...)

        //currentCameraRotationX값을 theCamera (실제 카메라)에 적용시키기...
        //카메라의 위치(로테이션)정보 / 마우스가 좌우로 움직이지 않게
        theCamera.transform.localEulerAngles  = new Vector3(currentCameraRotationX, 0f, 0f);
    }

}
