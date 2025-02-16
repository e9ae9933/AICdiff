using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2NoelCane : M2NoelDropCloth
	{
		public void initCane(NoelAnimator _PrAnm, PxlImage _BaseImg, float mover_scale, bool head_sphere = true)
		{
			this.initObject(_PrAnm, _BaseImg, mover_scale);
			if (!(this.CC is M2MvColliderCreatorNoelCane))
			{
				this.CC = new M2MvColliderCreatorNoelCane(this, mover_scale);
			}
			M2MvColliderCreatorNoelCane m2MvColliderCreatorNoelCane = this.CC as M2MvColliderCreatorNoelCane;
			m2MvColliderCreatorNoelCane.is_sphere = head_sphere;
			m2MvColliderCreatorNoelCane.BaseImg = _BaseImg;
		}

		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
			this.Phy.sound = "cane_drop";
		}

		public Vector2 getSphereMapPos()
		{
			float num = base.transform.localEulerAngles.z / 180f * 3.1415927f;
			Vector2 vector = new Vector2(base.x, base.y);
			if (this.BaseImg == null)
			{
				return vector;
			}
			float num2 = (float)(-(float)(this.BaseImg.width / 2 - this.BaseImg.height / 2)) / base.CLENM;
			vector.x += X.Cos(num) * num2;
			vector.y -= X.Sin(num) * num2;
			return vector;
		}

		protected override void OnCollisionEnter2D(Collision2D col)
		{
			base.OnCollisionEnter2D(col);
			string text = ((this.Mp != null) ? this.Mp.getTag(col.gameObject) : null);
			if (!this.PrAnm.cane_broken && this.appear_t >= 60f && ((text == "MoverEn" && X.XORSP() < 0.125f) || text == "MoverPr" || X.XORSP() < 0.0625f))
			{
				this.PrAnm.checkBreakEffect(true);
			}
		}

		public const float break_ratio_pr = 0.0625f;
	}
}
