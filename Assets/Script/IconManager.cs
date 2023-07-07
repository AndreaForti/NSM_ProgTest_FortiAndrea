using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IconManager : MonoBehaviour
{
	[SerializeField] private List<SpriteRenderer> IconSlots = new List<SpriteRenderer>();

	[Header("Icons")]
	[SerializeField] private Sprite MonsterSprite;
	[SerializeField] private Sprite TeleporterSprite;
	[SerializeField] private Sprite WellSprite;

	private int iconPointer = 0;

	public void AddIcon(IconType iconType)
	{
		switch (iconType)
		{
			case IconType.Monster:
				IconSlots[iconPointer].sprite = MonsterSprite;
				break;
			case IconType.Telporter:
				IconSlots[iconPointer].sprite = TeleporterSprite;
				break;
			case IconType.Well:
				IconSlots[iconPointer].sprite = WellSprite;
				break;
		}
		IconSlots[iconPointer].gameObject.SetActive(true);
		iconPointer++;
	}

	public void Clear()
	{
		iconPointer = 0;
		IconSlots.ForEach(x => x.gameObject.SetActive(false));
	}

	public enum IconType
	{
		Monster,
		Telporter,
		Well
	}

}
