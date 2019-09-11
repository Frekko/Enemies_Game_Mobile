using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupBase : UIScreenBase
{
	

	public override void Show()
	{
		base.Show();
		uiRoot.SetModal(true);
	}

	public override void Hide()
	{
		base.Hide();
		uiRoot.SetModal(false);
	}
}
