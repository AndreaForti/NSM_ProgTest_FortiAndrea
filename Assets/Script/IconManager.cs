using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IconManager : MonoBehaviour
{
	[SerializeField] private SpriteRenderer[] BossIcons = new SpriteRenderer[4];
	[SerializeField] private SpriteRenderer[] TeleportIcons = new SpriteRenderer[4];
	[SerializeField] private SpriteRenderer[] WellIcons = new SpriteRenderer[4];
	[SerializeField] private SpriteRenderer[] IconSlots = new SpriteRenderer[3];


	[Header("Icons")]
	[SerializeField] private Sprite MonsterSprite;
	[SerializeField] private Sprite TeleporterSprite;
	[SerializeField] private Sprite WellSprite;

	private int iconPointer = 0;


	public void ShowBossIcons()
	{
		BossIcons.ToList().ForEach(x => x.gameObject.SetActive(true));
	}
	public void ShowTeleportIcons()
	{
		TeleportIcons.ToList().ForEach(x => x.gameObject.SetActive(true));
	}
	public void ShowWellIcons()
	{
		WellIcons.ToList().ForEach(x => x.gameObject.SetActive(true));
	}

	public void AddIcon(IconType iconType)
	{
		switch (iconType)
		{
			case IconType.Monster:
				IconSlots[iconPointer].sprite = MonsterSprite;
				IconSlots[iconPointer].gameObject.SetActive(true);
				iconPointer++;
				break;
			case IconType.Telporter:
				IconSlots[iconPointer].sprite = TeleporterSprite;
				IconSlots[iconPointer].gameObject.SetActive(true);
				iconPointer++;
				break;
			case IconType.Well:
				IconSlots[iconPointer].sprite = WellSprite;
				IconSlots[iconPointer].gameObject.SetActive(true);
				iconPointer++;
				break;
		}
	}

	public enum IconType
	{
		Monster,
		Telporter,
		Well
	}

}
