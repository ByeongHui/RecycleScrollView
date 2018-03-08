using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/****************************************

해당 클래스는 예제를 위한 샘플용으로 스크롤 기능에 사용되지 않습니다.

/*****************************************/

/// <summary>
/// 예제) 스크롤기능이 포함된 뷰( == 창 )를 관리하는 클래스
/// 사용자의 커스텀 데이터를 스크롤 뷰에 셋팅하는 부분이 포함됩니다.
/// </summary>
public class SampleUIManager : MonoBehaviour {

    public CustomScrollView customScrollView;                       // 1. CustomScrollView 선언.   ********************


    /// <summary>
    /// 사용자가 스크롤 뷰에 표현할 데이터를 갖고 있는 리스트.
    /// - 테스트용 데이터(SampleDataClass) 는 하나의 텍스쳐와 '이름' , '가격' 을 표시할 2개의 문자열로 되어있습니다.
    /// </summary>
    /// <returns></returns>
    public List<SampleDataClass> datalist = new List<SampleDataClass>();

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if(customScrollView == null) {                              // 2. CustomScrollView 초기화.   ********************
            customScrollView = GameObject.Find("Canvas/Scroll View").GetComponent<CustomScrollView>();
        }

        InitSampleData();
    }


    /// <summary>
    /// 예제) 테스트용 데이터 초기화.
    /// </summary>
    void InitSampleData()
    {
        // 더미로 쓸 데이터 초기화. ====
        Texture2D sampleTex = Resources.Load("icons") as Texture2D;
        for(int i = 0 ; i < 100 ; i++)
        {
            SampleDataClass data = new SampleDataClass(sampleTex, "item_" + i, i*100);
            datalist.Add(data);
        }
        // 끝 ====
        
        customScrollView.UpdateItemEvent += SetItemData;        // 3. 리스트에 있는 아이템에 데이터를 셋팅하는 함수 등록.  *************  
        customScrollView.Initialize(datalist.Count);            // 4. 내 데이터의 전체 갯수만큼 인자로 넘겨준뒤 초기화.   **************
    }








    /// <summary>
    /// CustomScrollView 에서 생성한 리스트 상에 표기되는 오브젝트와 인덱스에 대한 정보가 넘어옵니다.
    /// 이벤트용 함수로 정해진 함수명이 있는게 아닙니다. 본인이 원하시는 함수명으로 작성하시되,
    /// GameObject와 index 두개의 인자를 받는 함수를 생성한다음, CustomScrollView의 UpdateItemEvent에 연결해주면 됩니다.
    /// ( 36번째 줄 참고. )
    /// 해당 함수는 리스트의 아이템 오브젝트가 갱신될때마다 호출되면서 데이터를 동기화 시켜줍니다.
    /// </summary>
    /// <param name="_obj">리스트에 생성된 오브젝트중 하나</param>
    /// <param name="_index">해당 오브젝트가 가리키는 인덱스</param>
    void SetItemData(GameObject _obj, int _index)
    {
        // 해당 아이템에 있는 아이템 관리 클래스에 접근..
        SampleItem item = _obj.GetComponent<SampleItem>();
        // 데이터를 셋팅해줌.
        item.SetItemData(datalist[_index].itemThumbnail, datalist[_index].itemName, datalist[_index].itemPrice );
    }
	
}
