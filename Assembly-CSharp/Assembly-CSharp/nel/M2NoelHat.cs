using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2NoelHat : M2NoelDropCloth
	{
		public void initImage(PxlImage[] _AImg)
		{
			this.ImgFooted = _AImg[1];
		}

		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
			base.Size(13f, 13f, ALIGN.CENTER, ALIGNY.MIDDLE, false);
			base.base_gravity = 0.12f;
			this.Phy.sound = "prko_ground";
		}

		public override void initObject(NoelAnimator _PrAnm, PxlImage _BaseImg, float mover_scale)
		{
			base.initObject(_PrAnm, _BaseImg, mover_scale);
			if (_BaseImg != this.ImgFooted)
			{
				this.changefooted = false;
			}
		}

		public override void runPre()
		{
			base.runPre();
			if (!this.changefooted && base.hasFoot())
			{
				this.initObject(this.PrAnm, this.ImgFooted, 1f);
				base.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				this.changefooted = true;
			}
		}

		private PxlImage ImgFooted;

		private bool changefooted;
	}
}
