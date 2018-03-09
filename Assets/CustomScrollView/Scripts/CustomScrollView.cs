using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(ScrollRect))]
public class CustomScrollView : MonoBehaviour {

	public event Action<GameObject, int> UpdateItemEvent;

	#region Variables

	/// <summary>
	/// 스크롤 방향을 선택합니다.
	/// </summary>
	public enum Direction
	{
		Horizontal = 0,	// 가로
		Vertical = 1	// 세로
	};


	/// <summary>
	/// 아이템이 리스트에 정렬되는 방향.
	/// </summary>
	public Direction scrollDirection = Direction.Horizontal;

	/// <summary>
	/// 프리팹으로 생성해 둔 오브젝트. ( = 리스트에 표시할 아이템 )
	/// </summary>
	public GameObject itemPrefab;

	/// <summary>
	/// 아이템의 좌우 여백 크기.
	/// </summary>
	public float itemMarginWidth = 0f;

	/// <summary>
	/// 아이템의 상하 여백 크기.
	/// </summary>
	public float itemMarginHeight = 0f;

	/// <summary>
	/// 아이템에 대한 RectTransform 정보.
	/// </summary>
	private RectTransform itemRectTransform {
		get {
			if(itemPrefab == null) 	{ Debug.LogError("리스트에 표시할 아이템을 선택하지 않았습니다. itemPrefab 에 아이템을 추가해주세요."); return null; }
			else if(_itemRectTransform == null) {
				_itemRectTransform = itemPrefab.GetComponent<RectTransform>();
				if(_itemRectTransform == null) { Debug.LogError("아이템에서 RectTransform 정보를 찾을수 없습니다."); }
			}
			return _itemRectTransform;
		}	
	}
	private RectTransform _itemRectTransform;


	/// <summary>
	/// 아이템들을 가지는 리스트의 RectTransform 정보 ( Ex. 가로길이 == 화면상 보여질 리스트의 마스킹된 영역 / Mask - Show Mask Graphic 체크시 보이는 부분 ) 
	/// </summary>
	private RectTransform listRectTransform {
		get {
			if(_listRectTransform == null) _listRectTransform = this.GetComponent<RectTransform>();
			return _listRectTransform;
		}
	}
	private RectTransform _listRectTransform;


	/// <summary>
	/// ScrollRect에서 직접 이동시킬 Content의 RectTransform.
	/// - UGUI에서 ScrollView 생성시 Canvas/Scroll View/Viewport/Content 확인.
	/// </summary>
	private RectTransform contentRect {
		get {
			if(_contentRect == null) _contentRect = this.GetComponent<ScrollRect>().content;
			return _contentRect;
		}
	}
	private RectTransform _contentRect = null;

	/// <summary>
	/// 동일 오브젝트에 존재하는 ScrollRect에 대한 정보.
	/// - UGUI에서 ScrollView 생성시 Canvas/Scroll View 에 위치함.
	/// </summary>
	private ScrollRect scrollRect {
		get {
			if(_scrollRect == null) _scrollRect = this.GetComponent<ScrollRect>();
			return _scrollRect;
		}
	}
	private ScrollRect _scrollRect = null;


	private Vector3 preContentPos = Vector3.zero;


	/// <summary>
	/// 리스트에 표시할 아이템의 가로 길이. ( 여백 길이 포함 )
	/// </summary>
	/// <returns>가로 길이</returns>
	private float itemWidth {
		get {
			if(_itemWidth == 0f) {
				_itemWidth = itemRectTransform.rect.width + ( itemMarginWidth * 2 );
			}
			return _itemWidth;
		}
	}
	private float _itemWidth = 0f;


	/// <summary>
	/// 리스트에 표시할 아이템의 가로길이 절반 값. ( 여백 길이 포함 )
	/// </summary>
	/// <returns>가로길이의 절반 값</returns>
	private float itemHalfWidth {
		get {
			if(_itemHalfWidth == 0f) {
				_itemHalfWidth = ( itemRectTransform.rect.width * 0.5f ) + itemMarginWidth;
			}
			return _itemHalfWidth;
		}
	}
	private float _itemHalfWidth = 0f;


	/// <summary>
	/// 리스트에 표시할 아이템의 세로길이. ( 여백 길이 포함 )
	/// </summary>
	/// <returns>세로 길이</returns>
	private float itemHeight {
		get {
			if(_itemHeight == 0f) {
				_itemHeight = itemRectTransform.rect.height + ( itemMarginHeight * 2 );
			}
			return _itemHeight;
		}
	}
	private float _itemHeight = 0f;


	/// <summary>
	/// 리스트에 표시할 아이템의 세로길이 절반 값. ( 여백 길이 포함 )
	/// </summary>
	/// <returns>세로길이의 절반 값</returns>
	private float itemHalfHeight {
		get {
			if(_itemHalfHeight == 0f) {
				_itemHalfHeight = ( itemRectTransform.rect.height * 0.5f ) + itemMarginHeight;
			}
			return _itemHalfHeight;
		}
	}
	private float _itemHalfHeight = 0f;


	/// <summary>
	/// 리스트 풀에 생성할 실제 아이템의 갯수.
	/// </summary>
	private int generateItemCount = 0;

	/// <summary>
	/// 사용자가 다룰 아이템의 전체 갯수.
	/// </summary>
	private int totalItemCount = 0;


	/// <summary>
	/// 현재 보여지고 있는 아이템들의 대한 인덱스 정보를 저장할 리스트.
	/// </summary>
	[HideInInspector]
	public List<int> itemlistIndex = new List<int>();


	/// <summary>
	/// 리스트에 담겨진 아이템 목록.
	/// </summary>
	public List<GameObject> itemlistPool = new List<GameObject>();




	#endregion


	#region CORE


	public void Initialize(int _totalItemCount)
	{
		// 사용자가 다룰 아이템의 전체 갯수.
		// cf.	generateItemCount : 뷰 상에 실제 생성할 아이템 갯수. ( 풀링으로 재활용. )
		//		totalItemCount : 실제로 사용자가 다룰 아이템의 전체 갯수.
		totalItemCount = _totalItemCount;

		// 스크롤 방향에 따른 아이템 초기화 방식 분기.
		switch(scrollDirection)
		{
			// 가로형 스크롤.
			case Direction.Horizontal :

				/// 아이템의 가로 길이 / 스크롤 리스트의 가로 길이 = 리스트상에 보여지는 아이템 갯수
				/// 리스트 상의 보여지는 아이템 갯수에서 + 4이상을 해주어, 화면비율로 인해 화면이 늘어났을때 공백이 보이는 것을 방지.
				generateItemCount = Mathf.RoundToInt(listRectTransform.rect.width / itemRectTransform.rect.width) + 4;

				// 아이템들의 인덱스 정보를 저장할 리스트 초기화.
				for(int i = 0 ; i < generateItemCount ; i++) { itemlistIndex.Add(i); }

				// content 의 너비값. ( x : 아이템의 가로길이 x 총 아이템의 갯수 , y : 아이템의 세로길이  /// Margin 값 포함.)
				contentRect.sizeDelta = new Vector2(itemWidth * totalItemCount, itemWidth);
		
				// ScrollRect의 현재 포지션 리셋
				scrollRect.horizontalNormalizedPosition = 0f;

				// content의 위치값 초기화.
				preContentPos = new Vector3(contentRect.localPosition.x - listRectTransform.rect.width * 0.5f, contentRect.localPosition.y, contentRect.localPosition.z);

			break;

			// 세로형 스크롤.
			case Direction.Vertical :

				/// 아이템의 세로 길이  / 스크롤 리스트의 세로 길이 = 리스트상에 보여지는 아이템 갯수
				/// 리스트 상의 보여지는 아이템 갯수에서 + 4이상을 해주어, 화면비율로 인해 화면이 늘어났을때 공백이 보이는 것을 방지.
				generateItemCount = Mathf.RoundToInt(listRectTransform.rect.height / itemRectTransform.rect.height) + 4;

				for(int i = 0 ; i <generateItemCount ; i++) { itemlistIndex.Add(i); }

				// content의 높이값 ( x : 아이템의 가로길이 , y : 아이템의 세로길이 x 총 아이템의 갯수  /// Margin 값 포함.)
				contentRect.sizeDelta = new Vector2(itemHeight, itemHeight * totalItemCount);
		
				// ScrollRect의 현재 포지션 리셋
				scrollRect.verticalNormalizedPosition = 1f;

				// content의 위치값 초기화.
				preContentPos = new Vector3(contentRect.localPosition.x, contentRect.localPosition.y + listRectTransform.rect.height * 0.5f, contentRect.localPosition.z);

			break;
		}

		// 아이템 오브젝트 풀 생성.
		if(itemlistPool.Count == 0)
		{
			for(int i = 0 ; i < generateItemCount ; i++)
			{
				// 새로운 오브젝트 추가.
				AddItemObjectInPool(i);

				// 오브젝트 위치 정렬.
				SetItemPosition(itemlistPool[i].transform , itemlistIndex[i]);
			}
		}

		// 이미 생성해 둔 오브젝트 풀이 존재할 경우...
		else {
			for(int i = 0 ; i < generateItemCount ; i++)
			{
				// 오브젝트 정보 읽어옴.
				GameObject obj = contentRect.GetChild(i).gameObject;

				obj.name = itemlistIndex[i].ToString();	// 오브젝트 이름 인덱스로 초기화.
				// obj.transform.SetParent(contentRect.transform); // 이미 content에 생성이 되어있는 상태이므로 생략.

				// 오브젝트 위치 정렬.
				SetItemPosition(itemlistPool[i].transform , itemlistIndex[i]);
			}
		}

		// 생성된 오브젝트들의 인덱스에 따라 내부 데이터 초기화.
		UpdateAllItems();
		
	}

	/// <summary>
	/// itemPrefab에 지정 해둔 item 오브젝트를 생성합니다.
	/// </summary>
	/// <param name="index">생성시 해당 오브젝트가 가지게 될 인덱스 정보/param>
	void AddItemObjectInPool(int index)
	{
		if(itemlistIndex == null || itemlistIndex.Count <= index) { Debug.LogError("itemlistIndex가 초기화되지 않았습니다."); return; }

		// 오브젝트 생성.
		GameObject obj = Instantiate(itemPrefab) as GameObject;
		
		obj.name = itemlistIndex[index].ToString();		// 오브젝트명을 인덱스로 대체.
		obj.transform.SetParent(contentRect.transform); // content의 하위 오브젝트로 이동.
		obj.transform.localPosition = Vector3.zero;		// 좌표값 초기화.
		obj.transform.localScale = Vector3.one;			// 스케일 초기화.

		// 아이템 오브젝트 풀에 추가.
		itemlistPool.Add(obj);
	}

	/// <summary>
	/// 생성되어 있는 아이템 정보를 모두 갱신합니다.
	/// </summary>
	void UpdateAllItems()
	{
		for(int i = 0 ; i < generateItemCount ; i++ )
		{
			if(UpdateItemEvent == null) { Debug.LogError("아이템의 데이터를 셋팅하는 이벤트가 지정되지 않았습니다."); return; }
			UpdateItemEvent(itemlistPool[i], itemlistIndex[i]);
		}
	}

	/// <summary>
	/// 아이템의 좌표값 지정.
	/// </summary>
	/// <param name="objTransform">아이템의 Transform</param>
	/// <param name="index">해당 아이템의 인덱스 ( content에 담긴 아이템 리스트 전체 기준 )</param>
	private void SetItemPosition(Transform objTransform, int index)
	{
		// 방향에 따라 x, y 값으로 사용.
		float contentMiddlePosition = 0f;		// content의 가운데 좌표 값.
		float itemPosition = 0f;				// index 번째 아이템의 좌표 값.
		float resultPosition = 0f;				// 최종 산출된 좌표 값.

		// 스크롤 방향에 따른 분기 처리.
		switch (scrollDirection)
		{
			// 가로로 정렬.
			case Direction.Horizontal :

				// 0을 기준으로 아이템의 길이를 이용하여 해당 아이템의 위치값을 산출. ( 가로 or 세로 길이인지는 scrollDirection 설정값에 따라 달라짐 )
				// content의 절반 길이 ( = content의 중심좌표 )
				contentMiddlePosition = contentRect.rect.width * 0.5f;

				// index 번째 아이템의 위치
				itemPosition = itemWidth * index;

				// content의 가운데 위치 - ( n번째 아이템의 위치 + 아이템의 절반 길이 )   => 좌표기준점이 아이템의 좌상단 기준이므로, 절반길이만큼 더해줌으로써 원하는 지점의 중앙에 위치하도록 함.
				resultPosition = contentMiddlePosition - (itemPosition + itemHalfWidth);

				//resultPosition = (itemPosition + itemHalfWidth);

				// 좌표 갱신. ( => content의 오른쪽(-) 가장 자리부터 순차적으로 위치하게 됨. ) 
				objTransform.localPosition = new Vector3(-resultPosition, objTransform.localPosition.y, objTransform.localPosition.z);

			break;


			// 세로로 정렬.
			case Direction.Vertical :

				// 0을 기준으로 아이템의 길이를 이용하여 해당 아이템의 위치값을 산출. ( 가로 or 세로 길이인지는 scrollDirection 설정값에 따라 달라짐 )
				// content의 절반 길이 ( = content의 중심좌표 )
				contentMiddlePosition = contentRect.rect.height * 0.5f;

				// index번째 아이템의 위치
				itemPosition = itemHeight * index;

				// content의 가운데 위치 - ( n번째 아이템의 위치 + 아이템의 절반 길이 )   => 좌표기준점이 아이템의 좌상단 기준이므로, 절반길이만큼 더해줌으로써 원하는 지점의 중앙에 위치하도록 함.
				resultPosition = contentMiddlePosition - (itemPosition + itemHalfHeight);

				//resultPosition = (itemPosition + itemHalfHeight);

				// 좌표 갱신. ( => content의 가장 위부터 순차적으로 위치하게 됨. ) 
				objTransform.localPosition = new Vector3(objTransform.localPosition.x, resultPosition, objTransform.localPosition.z);

			break;
		}
	}


	/// <summary>
	/// 매 프레임마다 content의 이동상태를 파악하여, 리스트에 생성된 오브젝트의 재배치 및 데이터 갱신 여부를 결정합니다.
	/// </summary>
	private void OnUpdatePosition()
	{
		switch(scrollDirection)
		{
			// 가로형 스크롤.
			case Direction.Horizontal :
				// content가 scrollView의 가로 길이만큼 오른쪽으로 이동한 상황이라면...
				if (contentRect.localPosition.x > preContentPos.x + itemHalfWidth) {
					// 리스트에서 마지막에 있는 아이템 오브젝트를 첫번째로 이동 후 값 갱신. 
					TailToHead(); 
				}
				// content가 scrollView의 가로 길이만큼 왼쪽으로 이동한 상황이라면...
				else if (contentRect.localPosition.x < preContentPos.x - itemHalfWidth) { 
					// 리스트에서 첫번째에 있는 아이템 오브젝트를 맨 뒤로 이동 시킨 후 값 갱신.
					HeadToTail(); 
				}
			break;

			// 세로형 스크롤.
			case Direction.Vertical :

				Debug.Log(contentRect.localPosition.y + " / " + (preContentPos.y + itemHalfHeight));
				// content가 scrollView의 세로 길이 만큼 위로 이동한 상황이라면...
				if (contentRect.localPosition.y > preContentPos.y + itemHalfHeight) {
					Debug.Log("HeadToTail()");
					// 리스트에서 첫번째에 있는 아이템 오브젝트를 맨 뒤로 이동 시킨 후 값 갱신.
					HeadToTail();
				}
				// content가 scrollView의 세로 길이만큼 아래로 이동한 상황이라면...
				else if (contentRect.localPosition.y < preContentPos.y - itemHalfHeight) { 
					Debug.Log("TailToHead()");
					// 리스트에서 마지막에 있는 아이템 오브젝트를 첫번째로 이동 후 값 갱신. 
					TailToHead();  
				}

			break;
		}
	}


	/// <summary>
	/// 리스트에서 가장 먼저 저장된 오브젝트 및 데이터를 리스트의 맨 뒤로 보냅니다.
	/// </summary>
	void HeadToTail()
	{
		// 셋팅할 아이템의 인덱스가 리스트의 갯수와 같거나 벗어날 경우 리턴
		if (totalItemCount <= itemlistIndex[itemlistIndex.Count - 1] + 1) return;

		// 오브젝트 풀 셋팅.
		itemlistPool.Add(itemlistPool[0]); 	// 가장 앞의 있는 데이터를 똑같이 복사한 다음,
		itemlistPool.RemoveAt(0);			// 앞의 데이터는 삭제.

		// 아이템 인덱스 셋팅 
		itemlistIndex.Add(itemlistIndex[itemlistIndex.Count - 1] + 1); 	// 인덱스 리스트도 위와 동일한 방식으로 변경.
		itemlistIndex.RemoveAt(0);

		// 위치 저장
		if(scrollDirection == Direction.Horizontal) preContentPos.Set(preContentPos.x - itemWidth, preContentPos.y, preContentPos.z);
		else										preContentPos.Set(preContentPos.x, preContentPos.y + itemHeight, preContentPos.z);
		
		// 오브젝트 셋팅
		int lastIndex = itemlistPool.Count - 1;
		itemlistPool[lastIndex].name = itemlistIndex[lastIndex].ToString();				// 오브젝트 이름 인덱스로 재설정.
		SetItemPosition(itemlistPool[lastIndex].transform, itemlistIndex[lastIndex]);	// 오브젝트 좌표 재설정.

		// ***** 사용자의 데이터를 item에 설정하기 위한 이벤트 호출.
		// Demo - Scripts / SampleUIManager.SetItemData(GameObject, int) 참고.
		if(UpdateItemEvent == null) { Debug.LogError("아이템의 데이터를 설정하는 이벤트가 지정되지 않았습니다."); return; }
		UpdateItemEvent(itemlistPool[lastIndex], itemlistIndex[lastIndex]);
	}

	/// <summary>
	/// 리스트에서 가장 나중에 추가 오브젝트 및 데이터를 리스트의 맨 앞으로 보냅니다.
	/// </summary>
	void TailToHead()
	{
		// 셋팅할 아이템의 인덱스가 0보다 작을 경우 리턴
		if (0 >= itemlistIndex[0]) return;

		// 오브젝트 풀 셋팅
		itemlistPool.Insert(0, itemlistPool[itemlistPool.Count - 1]); 	// 리스트의 가장 뒤에 있는 오브젝트를 리스트 처음 자리(0번 인덱스) 에 복사.
		itemlistPool.RemoveAt(itemlistPool.Count - 1);					// 가장 뒤에 있던 오브젝트는 삭제.

		// 아이템 인덱스 셋팅 (맨 뒤를 0번과 바꿈)
		itemlistIndex.Insert(0, itemlistIndex[0] - 1);					// 인덱스 리스트도 위와 동일한 방식으로 변경.
		itemlistIndex.RemoveAt(itemlistIndex.Count - 1);

		// 위치 저장
		if(scrollDirection == Direction.Horizontal) preContentPos.Set(preContentPos.x + itemWidth, preContentPos.y, preContentPos.z);
		else										preContentPos.Set(preContentPos.x, preContentPos.y - itemHeight, preContentPos.z);

		// 오브젝트 셋팅 ( 해당 인덱스의 데이터를 오브젝트에 설정. )
		itemlistPool[0].name = itemlistIndex[0].ToString();				// 오브젝트 이름 인덱스로 재설정.
		SetItemPosition(itemlistPool[0].transform, itemlistIndex[0]);	// 오브젝트 좌표 재설정.

		// ***** 사용자의 데이터를 item에 설정하기 위한 이벤트 호출.
		// Demo - Scripts / SampleUIManager.SetItemData(GameObject, int) 참고.
		if(UpdateItemEvent == null) { Debug.LogError("아이템의 데이터를 설정하는 이벤트가 지정되지 않았습니다."); return; }
		UpdateItemEvent(itemlistPool[0], itemlistIndex[0]);
	}


	#endregion

	#region UNITY_FUNCTIONS

	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		// ScrollRect 의 셋팅값을 CustomScrollView의 값을 기준으로 하게 만든다.
		// ( ScrollRect 에 종속된 스크롤 바와 같은 기능들 활용. )
		if(scrollDirection == Direction.Horizontal){
			scrollRect.horizontal = true;
			scrollRect.vertical = false;
		}
		else {
			scrollRect.horizontal = false;
			scrollRect.vertical = true;
		}

		// Content 의 기준점을 초기화.
		contentRect.localPosition = Vector3.zero;
		contentRect.anchorMin = new Vector2(0.5f, 0.5f);
		contentRect.anchorMax = new Vector2(0.5f, 0.5f);
		contentRect.pivot = new Vector2(0.5f, 0.5f);
	}

	/// <summary>
	/// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
	/// </summary>
	void FixedUpdate()
	{
		OnUpdatePosition();
	}

	#endregion

}
