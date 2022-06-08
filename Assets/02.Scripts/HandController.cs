using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    //현재 장착된 Hand형 타입(무기)
    [SerializeField]
    private Hand currentHand;

    //공격중?
    private bool isAttack = false;
    private bool isSwing = false;

    private RaycastHit hitInfo; //Raycast에 닿은 정보를 hitInfo에 저장하는 변수

    void Update()
    {
        TryAttack();
    }

    private void TryAttack()
    { //왼쪽 버튼을 누를 경우 코루틴이 실행
        if(Input.GetButton("Fire1")) //마우스를 누르고 있는 경우에도 효과 지속
        {
            if (!isAttack)
            {
                StartCoroutine(AttackCoroutine());
            }
        }
    }
//마우스 좌클릭을 하는 순간 StartCoroutine(AttackCoroutine() 코루틴이 실행되고,
//바로 isAttack = true가 되면서 중복 실행이 막아진다!
//마지막에 isAttack = false를 줘서 실행시키기...!
    IEnumerator AttackCoroutine()
    {
        isAttack = true;
        currentHand.anim.SetTrigger("Attack"); //Attack애니메이션 실행

        yield return new WaitForSeconds(currentHand.attackDelayA);//currentHand.attackDelayA 만큼 대기시간 주기...
        isSwing = true; //공격 들어감 true가 된 순간 공격이 적중했는지 구분하는 함수(코루틴)

        //적중 여부를 판단할 수 있는 코루틴 반복 실행...
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentHand.attackDelayB); //또 대기시간 주기
        isSwing = false; //일정 시간이 지나면 false 가 되어 HitCoroutine이 꺼짐

        //공격할 수 있게 대기...
        //딱 attackDelay 만큼 쉴 수 있게 그 전의 A,B값을 빼줌...
        yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB);
        isAttack = false; //false를 줘서 재공격할 수 있도록 만들었다...
    }

//공격 적중을 알아보는 코루틴
//Delay A,B사이에 계속 닿은것이 있는지를 체크...
    IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            if (CheckObject())
            {
                isSwing = false; //하나 적중했을 경우 다시 실행하지 않도록...
                //충돌함/충돌한 것의 하이어라키 이름 받아오기
                Debug.Log(hitInfo.transform.name);
            }
            yield return null; //while문이 한 번 돌 동안 1프레임 대기
        }
    }

    private bool CheckObject()
    {
        if(Physics.Raycast(transform.position, transform.forward, out hitInfo, currentHand.range))
        {
            return true; //충돌한 게 있음
        }
        return false; //충돌한 게 없음
    }
}
