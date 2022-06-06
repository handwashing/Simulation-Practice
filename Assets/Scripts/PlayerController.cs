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
    private float applySpeed; //움직임 제어 / 이것 하나만 있어도 대입만 하면 됨...(여러개의 함수 쓸 필요없다)

    //상태 변수
    private bool isRun = false; //뛰고있는지를 확인 false가 기본값임(안 써도 ok)

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
        
        //리지드바디 컴퍼넌트를 마이리지드 변수에 넣겠다
        myRigid = GetComponent<Rigidbody>();
        applySpeed = walkSpeed; //달리기 전까지는 걷는 상태
    }

    void Update()
    {
        TryRun(); //뛰거나 걷는것을 구분하는 함수(판단 후 움직임 제어 / 순서주의)
        Move(); //키입력에 따라 움직임이 실시간으로 이루어지게하는 처리
        CameraRotation(); 
        CharacterRotation();
    }

    private void TryRun()
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

    private void Running()
    {
        isRun = true; 
        applySpeed = runSpeed; //스피드가 RunSpeed로 바뀜
    }

    private void RunningCancel()
    {
        isRun = true; 
        applySpeed = walkSpeed; //걷는 속도
    }
    private void Move()
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
