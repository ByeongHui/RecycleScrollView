using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/****************************************

해당 클래스는 예제를 위한 샘플용으로 스크롤 기능에 사용되지 않습니다.

/*****************************************/

/// <summary>
/// 예제) 아이템 정보를 가지고 있는 클래스.
/// 해당 아이템의 이미지 및 버튼 이벤트 처리등을 포함해서 한번에 처리 하고 있습니다.
/// </summary>
public class SampleItem : MonoBehaviour {
	
	public Texture2D 	itemThumbnail;
	public string 		itemName;
	public int			itemPrice;


	public RawImage rawImage;
	public Text textName;
	public Text textPrice;


	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		if(rawImage == null) rawImage = this.transform.Find("thumbnail").GetComponent<RawImage>();
		if(rawImage == null) textName = this.transform.Find("name").GetComponent<Text>();
		if(rawImage == null) textName = this.transform.Find("price").GetComponent<Text>();
	}

	public void SetItemData(Texture2D _texture, string name, int price)
	{
		itemThumbnail = _texture;
		itemName = name;
		itemPrice = price;

		rawImage.texture = itemThumbnail;
		textName.text = itemName;
		textPrice.text = itemPrice.ToString();
	}

	// Buy 버튼에 대한 동작처리.
	public void BtnProcess()
	{
		// 아이템을 구매한다고 치면, SetItemData을 통해 설정된 아이템 정보를 기반으로 구매에 대한 루틴 처리.
		Debug.Log("Buy " + itemName );

		//...
	}


}
