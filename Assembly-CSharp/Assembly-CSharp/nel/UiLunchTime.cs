using System;
using evt;
using UnityEngine;
using XX;

namespace nel
{
	public class UiLunchTime : UiLunchTimeBase
	{
		protected override ItemStorage AwakeLunch()
		{
			this.DrSeet = new RollSeetDrawer();
			this.DrSeet.double_roll = true;
			this.DrSeet.block_count_x = 5;
			this.DrSeet.block_count_y = 3;
			this.DrSeet.ImgFront = MTR.SqNelCuteLine.getImage(3, 0);
			this.DrSeet.ImgBack = MTR.SqNelCuteLine.getImage(4, 0);
			this.MdSeet = MeshDrawer.prepareMeshRenderer(base.gameObject, MTRX.MIicon.getMtr(BLEND.NORMAL, -1), 0f, -1, null, false, false);
			this.Img = EV.Pics.getPic("lunch_cutin/noel_lunch", true, true);
			this.ImgLoader = EV.Pics.cacheReadFor(this.Img);
			this.t_noel = -120f;
			return base.AwakeLunch();
		}

		public override void OnDestroy()
		{
			if (this.MdSeet != null)
			{
				this.MdSeet.destruct();
			}
			base.OnDestroy();
		}

		public override UiBoxDesignerFamily deactivate(bool immediate = false)
		{
			base.deactivate(immediate);
			if (this.t_noel > -100f)
			{
				this.t_noel = X.Mx(this.t_noel, 54f);
			}
			return this;
		}

		protected override bool runEffect(MeshDrawer MdEf, float af_effect)
		{
			return UiLunchTime.runEffectS(this, MdEf, af_effect, this.SMtrNoel, this.PtcUseFood);
		}

		public static bool runEffectS(UiLunchTimeBase Con, MeshDrawer MdEf, float af_effect, Material SMtrNoel, EfParticleOnce PtcUseFood)
		{
			Con.syncNoelPos2GobEfPos();
			if ((PtcUseFood == null || !PtcUseFood.drawTo(MdEf, af_effect, 0f)) && af_effect >= 28f)
			{
				if (SMtrNoel != null)
				{
					SMtrNoel.SetColor("_AddColor", C32.d2c(0U));
				}
				return false;
			}
			if (SMtrNoel != null)
			{
				SMtrNoel.SetColor("_AddColor", MTRX.colb.Set(13219479).blend(0U, X.ZSIN(af_effect, 18f)).C);
			}
			return true;
		}

		protected override void drawBackground(float af, bool force = false)
		{
			if (af > 64f)
			{
				return;
			}
			if (force)
			{
				af = 100f;
			}
			if (af >= 0f)
			{
				this.MdSeet.clear(false, false);
				float num = X.ZLINE(af, 60f);
				float num2 = X.ZPOW(af, 23f);
				float num3 = X.ZLINE(af, 15f);
				float num4 = 1f - num2 + X.BOUNCE(af - 23f, 13f) * 0.18f + X.BOUNCE(af - 23f - 13f, 7f) * 0.03f;
				float num5 = (-24f + 36f * num2 - 12f * X.ZPOW(af - 22f, 13f)) * 1.5f;
				float num6 = (1f + num4 * num4 * 2f) * 180f;
				float num7 = X.ZLINE(af - 20f, 40f);
				Matrix4x4 matrix4x = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 10f + 13f * X.ZCOS(af, 30f))) * Matrix4x4.Rotate(Quaternion.Euler(-5f, 0f, 0f)) * Matrix4x4.Rotate(Quaternion.Euler(0f, num5, 0f));
				this.DrSeet.dark_col = this.MdSeet.ColGrd.Set(4281348144U).blend(4291085508U, num2).mulA(num3)
					.rgba;
				this.DrSeet.bright_col = this.MdSeet.ColGrd.Set(4286216826U).blend(uint.MaxValue, num2).mulA(num3)
					.rgba;
				this.DrSeet.drawTo(this.MdSeet, num7, num6, matrix4x, false);
				if (num < 0.875f)
				{
					this.DrSeet.drawTo(this.MdSeet, num7, num6, matrix4x, true);
				}
				this.MdSeet.updateForMeshRenderer(true);
				return;
			}
			Color32[] colorArray = this.MdSeet.getColorArray();
			byte b = (byte)(255f * X.ZLINE(30f + af, 30f));
			for (int i = this.MdSeet.getVertexMax() - 1; i >= 0; i--)
			{
				Color32 color = colorArray[i];
				color.a = b;
				colorArray[i] = color;
			}
			this.MdSeet.updateForMeshRenderer(true);
		}

		public static bool runNoelImgS(MeshDrawer MdNoel, ref float t_noel, float thresh_t, EvImg Img, ref EvPerson.EvPxlsLoader ImgLoader, ref Material SMtrNoel)
		{
			if (ImgLoader != null && ImgLoader.preparePxlImage(false))
			{
				t_noel = thresh_t - 4f;
				return false;
			}
			ImgLoader = null;
			MdNoel.chooseSubMesh(1, false, false);
			SMtrNoel = MTRX.newMtr(MTRX.getMI(Img.PF).getMtr(BLEND.NORMAL, -1));
			SMtrNoel.SetFloat("_UseAddColor", 1f);
			SMtrNoel.EnableKeyword("_USEADDCOLOR_ON");
			MdNoel.setMaterial(SMtrNoel, true);
			MdNoel.chooseSubMesh(0, false, false);
			t_noel = 0f;
			return true;
		}

		protected override void runNoelImg(MeshDrawer MdNoel, ref bool need_redraw_noel)
		{
			if (this.t_noel >= -100f && this.t_noel < 0f)
			{
				if (!UiLunchTime.runNoelImgS(MdNoel, ref this.t_noel, -100f, this.Img, ref this.ImgLoader, ref this.SMtrNoel))
				{
					return;
				}
				need_redraw_noel = true;
			}
			if (this.t_noel >= 0f && !need_redraw_noel)
			{
				byte b = (byte)X.ANM((int)this.t_noel, 2, 30f);
				if (b != this.noel_id || this.t_noel < 58f)
				{
					this.noel_id = b;
					need_redraw_noel = true;
				}
			}
		}

		protected override bool drawNoelImg(MeshDrawer MdNoel, float alpha, ref bool need_redraw_noel)
		{
			MdNoel.clear(false, false);
			need_redraw_noel = false;
			MdNoel.chooseSubMesh(0, false, true);
			MdNoel.Col = MdNoel.ColGrd.White().mulA(alpha).C;
			MdNoel.base_px_x = -IN.wh * 0.6f;
			MdNoel.base_px_y = 10f + 900f * (base.isActiveL() ? (1f - X.ZSIN(this.t_noel, 50f)) : 0f);
			int vertexMax = MdNoel.getVertexMax();
			if (this.noel_id == 0)
			{
				MdNoel.Rotate(-0.052359883f, true);
				base.MK(327f, 63f).MK(230f, 106f).MK(170f, 122f)
					.MK(180f, 214f)
					.MK(162f, 309f)
					.MK(221f, 394f)
					.MK(219f, 534f)
					.MK(252f, 574f)
					.MK(155f, 685f)
					.MK(63f, 782f)
					.MK(140f, 879f)
					.MK(454f, 862f)
					.MK(641f, 874f)
					.MK(683f, 737f)
					.MK(501f, 628f)
					.MK(471f, 576f)
					.MK(511f, 499f)
					.MK(491f, 432f)
					.MK(565f, 304f)
					.MK(528f, 180f)
					.MK(524f, 120f)
					.MK(473f, 96f);
			}
			else
			{
				MdNoel.Rotate(0.03490659f, true);
				base.MK(349f, 66f).MK(237f, 103f).MK(177f, 131f)
					.MK(176f, 209f)
					.MK(149f, 302f)
					.MK(217f, 398f)
					.MK(228f, 516f)
					.MK(253f, 560f)
					.MK(138f, 714f)
					.MK(59f, 769f)
					.MK(92f, 871f)
					.MK(453f, 863f)
					.MK(636f, 873f)
					.MK(693f, 745f)
					.MK(568f, 668f)
					.MK(500f, 568f)
					.MK(494f, 435f)
					.MK(515f, 383f)
					.MK(569f, 340f)
					.MK(541f, 195f)
					.MK(524f, 126f)
					.MK(466f, 94f);
			}
			base.MKTri(vertexMax);
			MdNoel.chooseSubMesh(1, false, true);
			MdNoel.RotaPF(0f, 0f, this.noel_scale, this.noel_scale, 0f, this.Img.PF, false, false, false, uint.MaxValue, false, 0);
			MdNoel.Identity();
			return true;
		}

		protected override void executeEatAfter()
		{
			if (this.PtcUseFood == null)
			{
				this.PtcUseFood = new EfParticleOnce("ui_use_food", EFCON_TYPE.UI);
				return;
			}
			this.PtcUseFood.shuffle();
		}

		private EvImg Img;

		private EvPerson.EvPxlsLoader ImgLoader;

		private Material SMtrNoel;

		private RollSeetDrawer DrSeet;

		private MeshDrawer MdSeet;

		private EfParticleOnce PtcUseFood;

		private byte noel_id;

		protected const int noel_start_maxt = 50;
	}
}
