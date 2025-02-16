using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel.mgm.dojo
{
	public class DjFigure
	{
		public bool is_pr
		{
			get
			{
				return !this.is_en;
			}
		}

		public DjFigure(MgmDojo _DJ, bool _is_en)
		{
			this.is_en = _is_en;
			this.DJ = _DJ;
		}

		private void Init()
		{
			this.PF = this.ImageSq.getFrame(0);
		}

		public void destruct()
		{
			BLIT.nDispose(this.TxBlur);
			this.TxBlur = null;
		}

		internal void prepareBlur(MeshDrawer MdDest)
		{
			this.Init();
			PxlImage img = this.PF.getLayer(0).Img;
			this.TxBlur = BLIT.Blur(this.DJ.MI.Tx, 20, 20, 1f, 1f, img.RectIUv.x, img.RectIUv.y, img.RectIUv.width, img.RectIUv.height, 10855845U);
			MdDest.chooseSubMesh(this.is_en ? 2 : 1, false, true);
			MdDest.setMaterial(new Material(MTRX.ShaderGDT), true);
			MdDest.initForImgAndTexture(this.TxBlur);
		}

		public void initHitHand(bool failure, int hand_type)
		{
			this.hit_t = (float)X.MPF(!failure) * 30f;
			this.hand_type = (byte)hand_type;
			if (!failure && this.PF != null)
			{
				if (this.FD_EfHitHand == null)
				{
					this.FD_EfHitHand = new FnEffectRun(this.EfHitHand);
					this.DrRad = new RadiationDrawer
					{
						fine_intv = 0,
						fix_intv_randomize = 0.3f,
						count_ratio = 1.25f
					};
				}
				this.DrRad.ran0 = X.xors();
				this.DJ.GM.get_EF().setEffectWithSpecificFn("HitHand", this.drawx * 0.015625f, (DjFigure.btmy + this.PF.height * 0.5f * 0.4f) * 0.015625f, 0f, hand_type, 0, this.FD_EfHitHand);
			}
		}

		public DjFigure closeHitHand()
		{
			if (this.hit_t != 0f)
			{
				this.DJ.need_redraw = true;
				this.hit_t = 0f;
			}
			return this;
		}

		public DjFigure deactivate(bool reset_sq = false)
		{
			this.closeHitHand().closeHiding();
			if (reset_sq)
			{
				this.Init();
			}
			return this;
		}

		public DjFigure initHiding()
		{
			if (this.t_hide <= 0f)
			{
				this.DJ.need_redraw = true;
				this.t_hide = 1f;
			}
			return this;
		}

		public DjFigure closeHiding()
		{
			if (this.t_hide > 0f)
			{
				this.DJ.need_redraw = true;
				this.t_hide = -65f;
			}
			return this;
		}

		public void changeFigIdTo(int i)
		{
			this.PF = this.ImageSq.getFrame(i);
			this.DJ.need_redraw = true;
		}

		internal bool run(float fcnt)
		{
			bool flag = false;
			if (this.hit_t != 0f)
			{
				flag = true;
				this.hit_t = X.VALWALK(this.hit_t, 0f, fcnt);
			}
			if (!this.DJ.bgm_playing)
			{
				flag = true;
			}
			if (this.t_hide > 0f)
			{
				if (this.t_hide < 65f)
				{
					this.t_hide = X.VALWALK(this.t_hide, 65f, fcnt);
					flag = this.t_hide >= 40f || flag;
				}
			}
			else if (this.t_hide < 0f)
			{
				flag = true;
				this.t_hide = X.VALWALK(this.t_hide, 0f, fcnt);
			}
			return flag;
		}

		private PxlSequence ImageSq
		{
			get
			{
				return this.DJ.Pxc.getPoseByName(this.is_en ? "avator_en" : "avator_pr").getSequence(0);
			}
		}

		public void drawTo(MeshDrawer Md, float introz, float tz_beat, float alpha)
		{
			if (this.PF == null)
			{
				return;
			}
			float drawx = this.drawx;
			float btmy = DjFigure.btmy;
			float num = 0.4f;
			float num2 = 0.4f;
			PxlFrame pxlFrame = this.PF;
			alpha *= ((this.t_hide > 0f) ? (1f - X.ZLINE(this.t_hide - 40f, 25f)) : X.ZLINE(65f + this.t_hide, 65f));
			if (this.hit_t < 0f)
			{
				float num3 = X.ZLINE(30f + this.hit_t, 30f);
				float num4 = X.ZSIN(num3, 0.5f);
				float num5 = num3 * 3.1415927f * 1.7f;
				num *= X.NI(1f + 0.05f * X.Cos(num5), 1f, num4);
				num2 *= X.NI(1f + 0.038f * X.Sin(num5), 1f, num4);
			}
			else if (this.hit_t > 0f)
			{
				float num6 = X.ZLINE(30f - this.hit_t, 30f);
				num *= 1f + X.ZSIN(num6, 0.24f) * 0.12f - X.ZCOS(num6 - 0.14f, 0.33f) * 0.12f;
				num2 *= 1f - X.ZLINE(num6, 0.1f) * 0.05f + X.ZSIN2(num6 - 0.08f, 0.16f) * 0.12f - X.ZCOS(num6 - 0.1f, 0.35f) * 0.07f;
			}
			else if (!this.DJ.bgm_playing)
			{
				int num7 = (this.is_pr ? 0 : 1);
				num *= X.NI(0.993f, 1.013f, 0.5f + 0.5f * X.COSI(this.DJ.t_state + (float)(num7 * 78), (float)(430 + num7 * 80)));
				num2 *= X.NI(0.97f, 1.013f, 0.5f + 0.5f * X.COSI(this.DJ.t_state + (float)(num7 * 98), (float)(210 - num7 * 25)));
			}
			else if (tz_beat > 0f)
			{
				num *= X.NI(1f, 1.025f, tz_beat);
				num2 *= X.NI(1f, 0.985f, tz_beat);
			}
			if (this.hit_t != 0f && this.PF.pSq.countFrames() >= 5)
			{
				pxlFrame = this.PF.pSq.getFrame((int)(2 + this.hand_type));
			}
			float num8 = btmy + (float)this.PF.pSq.height * num2 * 0.5f;
			if (introz > 0f)
			{
				PxlLayer layer = this.PF.getLayer(0);
				Md.chooseSubMesh(this.is_en ? 2 : 1, false, true);
				Md.Col = Md.ColGrd.White().mulA(0.88f * X.ZLINE(introz, 0.5f) * alpha).C;
				Md.initForImg(this.TxBlur);
				Md.RotaGraph(drawx + layer.x * num, num8 - layer.y * num2, 0.4f, 0f, null, false);
				if (introz > 0.5f)
				{
					return;
				}
				Md.chooseSubMesh(0, false, false);
				Md.Col = Md.ColGrd.White().mulA(X.ZSIN(0.5f - introz, 0.5f) * alpha).C;
			}
			else
			{
				Md.Col = Md.ColGrd.White().mulA(alpha).C;
			}
			Md.RotaPF(drawx, num8, num, num2, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
		}

		public static float drawx_en
		{
			get
			{
				return IN.wh * 0.5f;
			}
		}

		public static float drawx_pr
		{
			get
			{
				return -IN.wh * 0.55f;
			}
		}

		public float drawx
		{
			get
			{
				if (!this.is_en)
				{
					return DjFigure.drawx_pr;
				}
				return DjFigure.drawx_en;
			}
		}

		public static float btmy
		{
			get
			{
				return -IN.hh * 0.85f;
			}
		}

		public float drawy
		{
			get
			{
				if (this.PF == null)
				{
					return 0f;
				}
				return DjFigure.btmy + 0.4f * (float)this.PF.pSq.height * 0.5f;
			}
		}

		public bool EfHitHand(EffectItem E)
		{
			float num = X.ZLINE(E.af, 28f);
			if (num >= 1f)
			{
				return false;
			}
			float num2 = X.ZLINE(E.af, 18f);
			if (num2 < 1f)
			{
				PxlFrame frame = this.PF.pSq.getFrame(2 + E.time);
				MeshDrawer meshImg = E.GetMeshImg("hithand", this.DJ.MI, BLEND.NORMALP3, false);
				meshImg.allocUv23(12, false);
				meshImg.Uv23(C32.d2c(7631988U), false);
				meshImg.Col = C32.MulA(uint.MaxValue, 1f - num2);
				float num3 = (1.125f + X.ZSIN(num2) * 0.125f) * 0.4f;
				meshImg.RotaPF(0f, 0f, num3, num3, 0f, frame, false, false, false, uint.MaxValue, false, 0);
				meshImg.allocUv23(0, true);
			}
			if (this.is_pr)
			{
				MeshDrawer mesh = E.GetMesh("hithand", uint.MaxValue, BLEND.NORMAL, false);
				mesh.base_x = (mesh.base_y = 0f);
				mesh.Col = C32.MulA(DjRPC.i2col(E.time), 1f - num);
				float num4 = X.NI(0.5f, 1.25f, X.ZSIN2(E.af, 5f) - X.ZCOS(E.af - 3f, 23f) * 0.23f);
				this.DrRad.min_len_ratio = 0.2f * num4;
				this.DrRad.max_len_ratio = 0.28f * num4;
				this.DrRad.drawTo(mesh, 0f, 0f, IN.w + 60f, IN.h + 80f, 12f * (1f - X.ZPOW(num)), 0f, false, 0.77f);
			}
			return true;
		}

		public readonly MgmDojo DJ;

		public readonly bool is_en;

		private const int blur_level = 20;

		private const float scale = 0.4f;

		private float hit_t;

		private byte hand_type;

		private const float MAXT_HIT = 30f;

		private float t_hide;

		private const float MAXT_HIDE = 65f;

		private const float MAXT_HIDE_CUTOFF = 40f;

		public const int FIG_ID_NORMAL = 0;

		public const int FIG_ID_DAMAGED = 1;

		public const int FIG_ID_ATTACK = 2;

		public const int FIG_ID_DEAD = 5;

		public const int FIG_ID_DEAD_NOBRA = 6;

		public const int FIG_ID_DEAD_NOPANTSU = 7;

		private PxlFrame PF;

		private FnEffectRun FD_EfHitHand;

		private RadiationDrawer DrRad;

		public PxlMeshDrawer MeshBlur;

		private RenderTexture TxBlur;
	}
}
