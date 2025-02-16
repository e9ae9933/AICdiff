using System;
using UnityEngine;
using XX;

namespace nel
{
	public class UiSummonerAnnounce : Designer
	{
		protected override void Awake()
		{
			base.Awake();
			this.animate_scaling_x = (this.animate_scaling_y = false);
			this.margin_in_lr = 40f;
			this.margin_in_tb = 14f;
			this.WH(490f, 80f);
			this.setPos(0f);
		}

		protected override void kadomaruRedraw(float _t, bool update_mesh = true)
		{
			base.kadomaruRedraw(_t, update_mesh);
			this.setPos(base.animating_alpha);
		}

		public void setPos(float tz)
		{
			Vector3 vector = default(Vector3);
			vector.z = -3.99f;
			vector.y = -1.875f;
			vector.x = (IN.wh + this.w * 0.5f + 4f - (this.w * 1.5f + 4f - this.radius) * ((this.animate_t < 0f) ? X.ZPOW(tz) : X.ZSIN2(tz))) * 0.015625f;
			base.transform.localPosition = vector;
		}

		public void activate(string t)
		{
		}
	}
}
