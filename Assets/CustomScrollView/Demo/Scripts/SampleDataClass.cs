using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/****************************************

해당 클래스는 예제를 위한 샘플용으로 스크롤 기능에 사용되지 않습니다.

/*****************************************/

/// <summary>
/// 예제) 사용자의 아이템 데이터를 가지고 있는 클래스.
/// </summary>
public class SampleDataClass : MonoBehaviour {
	
	public Texture2D 	itemThumbnail;
	public string 		itemName;
	public int			itemPrice;

	public SampleDataClass(Texture2D _texture, string name, int price)
	{
		itemThumbnail = _texture;
		itemName = name;
		itemPrice = price;
	}


}
