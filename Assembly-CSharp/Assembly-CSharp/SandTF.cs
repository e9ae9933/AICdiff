using System;
using UnityEngine;
using XX;

[RequireComponent(typeof(aBtn))]
public class SandTF : MonoBehaviour
{
	public void Start()
	{
		this.MMRD = base.gameObject.AddComponent<MultiMeshRenderer>();
		this.Md0 = this.MMRD.Make(MTRX.cola.Set(4287823797U).C, BLEND.NORMAL, null, null);
		this.Md1 = this.MMRD.Make(MTRX.cola.Set(4290875903U).C, BLEND.NORMAL, null, null);
		this.MdCur = this.Md1;
	}

	public void Update()
	{
		if (this.fine_flag <= 0)
		{
			TFKEY tfkey = TFKEY.SC_STARDROP;
			this.Tf = new TransFader(tfkey, 95f, IN.w, IN.h, 1f);
			this.Tf.oneitem_maxt_level = 0.6f;
			this.Tf.resetAnim(TFANIM.R2L, false, 0.5f, 0);
			this.MdCur = ((this.MdCur == this.Md1) ? this.Md0 : this.Md1);
			this.MdCur.base_z = this.base_z;
			this.base_z -= 7E-06f;
			this.fine_flag = 30;
		}
		if (this.Tf != null && !this.Tf.redraw(this.MdCur, 1f))
		{
			this.fine_flag--;
		}
	}

	private MultiMeshRenderer MMRD;

	private TransFader Tf;

	private MeshDrawer MdCur;

	private MeshDrawer Md0;

	private MeshDrawer Md1;

	private float base_z;

	private int fine_flag;
}
