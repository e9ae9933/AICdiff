using System;
using UnityEngine;
using XX;

public class SandDsn : MonoBehaviour
{
	public void Start()
	{
	}

	public void LateUpdate()
	{
		if (!this.initted && MTRX.prepared)
		{
			this.initted = true;
			LabeledInputField labeledInputField = base.gameObject.AddComponent<LabeledInputField>();
			labeledInputField.text = this.txu;
			labeledInputField.label_top = true;
			labeledInputField.multi_line = 4;
			labeledInputField.setLabel("label");
		}
	}

	private bool initted;

	private string txu = "undertext";

	private MeshDrawer Md;
}
