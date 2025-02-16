using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class AnimateSyncCutin : AnimateCutin
	{
		public void Init(PR _Pr, UIPicture _Con, Vector3 _PosPx, Vector3 _BaseSlidePx, int _appear_dir_aim = -1, float _appear_len = 0f)
		{
			this.Con = _Con;
			bool flag = this.Con.getBodyData() is UIPictureBodySpine;
			this.Pr = _Pr;
			if (!flag)
			{
				return;
			}
			this.PosPx = _PosPx;
			this.BaseSlidePx = _BaseSlidePx;
			if (this.FD_fnDrawACDefault == null)
			{
				this.FD_fnDrawACDefault = new AnimateCutin.FnDrawAC(this.fnDrawACDefault);
			}
			this.appear_dir_aim = -1001;
			string currentFadeKey = this.Con.getCurrentFadeKey(true);
			if (!base.gameObject.activeSelf || this.Spw == null)
			{
				base.Init(this.Pr.Mp, currentFadeKey, this.FD_fnDrawACDefault);
			}
			this.fader_key = currentFadeKey;
			this.position_consider_basepos = false;
			this.appear_dir_aim = _appear_dir_aim;
			this.appear_len = _appear_len;
			this.Sync();
		}

		public override void deactivate(bool not_remrunner = false)
		{
			base.deactivate(not_remrunner);
			if (this.appear_dir_aim != -1001)
			{
				this.fader_key = null;
			}
		}

		public void Sync()
		{
			if (!base.isActive())
			{
				return;
			}
			UIPictureBodySpine uipictureBodySpine = this.Con.getBodyData() as UIPictureBodySpine;
			if (uipictureBodySpine == null || this.Con.getCurrentFadeKey(true) != this.fader_key)
			{
				this.fader_key = null;
				this.deactivate(false);
				return;
			}
			base.initUIPictBody(this.fader_key, uipictureBodySpine, this.Pr, this.Con.getCurrentState());
			this.Con.PosSyncSlide = new Vector3(this.BaseSlidePx.x * 0.015625f, this.BaseSlidePx.y * 0.015625f, this.BaseSlidePx.z);
		}

		protected override void PictBodyFinalize(BetobetoManager.SvTexture CurSvt)
		{
			base.PictBodyFinalize(CurSvt);
		}

		protected override void animRandomize(SpineViewer Spv, UIPictureBase.EMSTATE st, out UIPictureBase.EMSTATE current_state)
		{
			UIPictureBodySpine uipictureBodySpine = this.Con.getBodyData() as UIPictureBodySpine;
			current_state = this.Con.getCurrentState();
			if (uipictureBodySpine == null || this.Con.getCurrentFadeKey(true) != this.fader_key)
			{
				this.deactivate(false);
				return;
			}
			this.Spw.copyAnimationFrom(uipictureBodySpine.getViewer(), true);
		}

		private bool fnDrawACDefault(AnimateCutin Cti, Map2d Mp, MeshDrawer Md, MeshDrawer MdT, float t, float anim_t, ref bool update_meshdrawer)
		{
			UIPictureBodySpine uipictureBodySpine = this.Con.getBodyData() as UIPictureBodySpine;
			if (uipictureBodySpine == null || this.Con.getCurrentFadeKey(true) != this.fader_key)
			{
				return false;
			}
			X.ZSIN(anim_t, 25f);
			float num = ((this.appear_dir_aim < 0) ? 0f : this.appear_len);
			Cti.setBase(this.PosPx.x + num * (float)CAim._XD(this.appear_dir_aim, 1), this.PosPx.y + num * (float)CAim._YD(this.appear_dir_aim, 1), this.PosPx.z);
			this.Spw.setTimePositionAll(uipictureBodySpine.getViewer());
			this.Spw.setColor(this.Con.getColorTe());
			return true;
		}

		private AnimateCutin.FnDrawAC FD_fnDrawACDefault;

		private Vector3 PosPx;

		private Vector3 BaseSlidePx;

		private PR Pr;

		public int appear_dir_aim = -1;

		public float appear_len;

		private UIPicture Con;
	}
}
