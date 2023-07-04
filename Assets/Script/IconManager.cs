using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IconManager : MonoBehaviour
{
	[SerializeField] private SpriteRenderer[] BossIcons = new SpriteRenderer[4];
	[SerializeField] private SpriteRenderer[] TeleportIcons = new SpriteRenderer[4];
	[SerializeField] private SpriteRenderer[] WellIcons = new SpriteRenderer[4];

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
}
