using System;
using m2d;
using UnityEngine;

namespace XX
{
	public sealed class BlurScreen : HideScreen
	{
		protected override void Awake()
		{
			base.Awake();
			this.HIDE_MAXT = 20;
			base.gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
			this.M2D = M2DBase.Instance;
			this.Flags = new Flagger(delegate(FlaggerT<string> _Flg)
			{
				this.activate();
			}, delegate(FlaggerT<string> _Flg)
			{
				this.deactivate(false);
			});
			base.initMeshDrawer(true, true);
			base.Col = MTRX.ColWhite;
			this.Md.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false);
			this.Mrd.sharedMaterial = this.Md.getMaterial();
			this.Md.base_z = 0.1f;
			this.Mpb = new MProperty(this.Mrd, -1);
			this.Valot.Mpb = this.Mpb;
		}

		public override void OnDestroy()
		{
			BLIT.nDispose(this.TxBg);
			this.TxBg = null;
			base.OnDestroy();
		}

		public void addFlag(string s)
		{
			this.Flags.Add(s);
		}

		public void remFlag(string s)
		{
			this.Flags.Rem(s);
		}

		public override void activate()
		{
			if (!base.isActive() && (!this.temporary || this.TxBg == null))
			{
				this.need_recreate_img = true;
				this.Md.clear(false, false);
			}
			base.gameObject.SetActive(true);
			base.activate();
		}

		public override void deactivate(bool immediate = false)
		{
			this.temporary = false;
			base.deactivate(immediate);
			this.Flags.Clear();
		}

		protected override bool runIRD(float fcnt)
		{
			if (this.need_recreate_img)
			{
				this.createAnimateBg(1f);
				this.need_recreate_img = false;
			}
			return base.runIRD(fcnt);
		}

		private void createAnimateBg(float tz)
		{
			RenderTexture txBg = this.TxBg;
			RenderTexture finalizedTexture = this.M2D.Cam.getFinalizedTexture();
			if (finalizedTexture == null)
			{
				return;
			}
			this.TxBg = BLIT.Alloc(ref this.TxBg, (int)(IN.w * 0.3333f), (int)(IN.h * 0.3333f), false, RenderTextureFormat.ARGB32, 0);
			float num = ((this.M2D != null) ? X.Mn(1f, this.M2D.Cam.getScaleRev()) : 1f);
			float num2 = 1f + 0.2f * tz;
			float num3 = ((this.M2D != null) ? (-this.M2D.ui_shift_x / (IN.w + 16f) * num / num2) : 0f);
			BLIT.Blur(this.TxBg, finalizedTexture, X.IntR(6f * tz), X.IntR(6f * tz), IN.wh * 0.3333f, IN.hh * 0.3333f, 0.3333f, 1f, num3, 0f, 1f, 1f, 16777215U);
			RenderTexture.active = null;
			if (txBg != this.TxBg)
			{
				this.Mpb.SetTexture("_MainTex", this.TxBg, true);
			}
		}

		protected override void drawToMd(float alpha)
		{
			if (this.TxBg == null)
			{
				return;
			}
			this.Md.initForImg(this.TxBg);
			float num = ((this.M2D != null) ? X.Mx(1f, this.M2D.Cam.getScale(true)) : 1f);
			float num2 = 1f + 0.2f * alpha;
			this.Md.Col = this.Md.ColGrd.Set(base.Col).mulA(X.Mn(1f, 1.15f * alpha)).C;
			this.Md.DrawScaleGraph(0f, 0f, num / 0.3333f * num2, num / 0.3333f * num2, null);
		}

		private Flagger Flags;

		public bool immediate_flag;

		public bool need_recreate_img;

		public bool temporary;

		private const float blur_scale = 0.3333f;

		private M2DBase M2D;

		private MProperty Mpb;

		private RenderTexture TxBg;
	}
}
