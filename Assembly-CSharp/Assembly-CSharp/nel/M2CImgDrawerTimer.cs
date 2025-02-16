using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerTimer : M2CImgDrawer
	{
		public M2CImgDrawerTimer(MeshDrawer Md, int lay, M2Puts _Cp)
			: base(Md, lay, _Cp, true)
		{
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			string text = base.Meta.GetSi(0, "hourglass");
			if (TX.valid(text))
			{
				this.LayRot = this.Cp.Img.SourceFrm.getLayerByName(text);
				if (this.LayRot == null)
				{
					X.de(string.Concat(new string[]
					{
						"砂時計のの回転体イメージ ",
						text,
						"が ",
						this.Cp.Img.SourceFrm.ToString(),
						" に見つかりません。"
					}), null);
				}
				else
				{
					this.AtlRot = base.M2D.IMGS.Atlas.getAtlasData(this.LayRot.Img);
				}
			}
			text = base.Meta.GetSi(1, "hourglass");
			if (TX.valid(text))
			{
				this.LayTop = this.Cp.Img.SourceFrm.getLayerByName(text);
				if (this.LayTop == null)
				{
					X.de(string.Concat(new string[]
					{
						"砂時計の上部イメージ ",
						text,
						"が ",
						this.Cp.Img.SourceFrm.ToString(),
						" に見つかりません。"
					}), null);
				}
				else
				{
					this.AtlTop = base.M2D.IMGS.Atlas.getAtlasData(this.LayTop.Img);
				}
			}
			this.SqAnim = this.Cp.Img.SourceFrm.pChar.getPoseByName("_anim_hourglass").getSequence(0);
			base.Set(true);
			this.draw();
			base.Set(false);
			return true;
		}

		private void draw()
		{
			Bench.P("1");
			PxlLayer sourceLayer = this.Cp.Img.SourceLayer;
			float num = base.Mp.map2meshx(this.Cp.mapcx);
			float num2 = base.Mp.map2meshy(this.Cp.mapcy);
			Color32 c = this.Cp.Lay.LayerColor.C;
			this.Md.base_x = (this.Md.base_y = 0f);
			Bench.Pend("1");
			if (this.AtlRot.valid)
			{
				Bench.P("2");
				float num3 = 0f;
				float num4 = 0f;
				int num5 = 0;
				float num6;
				if (this.t >= this.maxt)
				{
					this.Md.ColGrd.Set(4289903540U);
					num6 = 1f;
					this.maxt = 0f;
				}
				else if (this.t < 50f)
				{
					num6 = 1f;
					num4 = X.ZSIN(this.t, 8f) * 9f - X.ZSINV(this.t - 7f, 43f) * 199f;
					this.Md.ColGrd.Set(4289903540U).blend(4288801713U, X.ZPOW(this.t, 50f));
					this.refine_rotation = true;
				}
				else
				{
					float num7 = this.t - 50f;
					num4 = -10f + X.ZSIN(num7, 13f) * 15f - X.ZCOS(num7 - 10f, 25f) * 5f;
					if (num7 < 45f)
					{
						num4 += 7f * (1f - X.ZPOW(num7, 45f)) * X.COSI(base.Mp.floort, 17f);
					}
					float num8 = X.ZLINE(this.t - 50f, this.maxt - 50f);
					num3 = 1f - X.ZLINE(num8, 0.9375f);
					num6 = num8;
					this.Md.ColGrd.Set(4294914867U).blend(4288801713U, num3).blend(uint.MaxValue, 1f - X.ZLINE(num7, 40f));
					if (num8 >= 0.9375f)
					{
						num5 = 1 + (int)X.Mn(3f, 4f * (num8 - 0.9375f) / 0.0625f) * 3;
					}
					else
					{
						num5 = 1;
					}
					this.refine_rotation = true;
				}
				Bench.Pend("2");
				Bench.P("3");
				Color32 c2 = this.Md.ColGrd.multiply(0.5f, false).C;
				this.Md.Rotate(num4 / 180f * 3.1415927f, false).TranslateP(this.LayRot.x - sourceLayer.x, this.LayRot.y - sourceLayer.y, false).Scale((float)(this.Cp.flip ? (-1) : 1), 1f, false)
					.Rotate(this.Cp.draw_rotR, false)
					.TranslateP(num, num2, false);
				if (num3 == 0f)
				{
					this.Md.Col.a = 0;
					this.Md.RectBL(0f, 0f, 0f, 0f, false);
				}
				else
				{
					this.Md.Col = c2;
					this.drawMeter(num3, false);
				}
				Bench.Pend("3");
				Bench.P("4");
				if (num5 == 0)
				{
					this.Md.Col.a = 0;
					this.Md.RectBL(0f, 0f, 0f, 0f, false);
				}
				else
				{
					this.Md.Col = c2;
					PxlLayer layer = this.SqAnim.getFrame(X.ANM((int)base.Mp.floort, 3, 6f) + num5).getLayer(0);
					Rect atlasRect = base.M2D.IMGS.Atlas.getAtlasRect(layer.Img);
					this.Md.initForImg(base.M2D.MIchip.Tx, atlasRect, true);
					this.Md.RotaL(0f, 0f, layer, true, false, 0);
				}
				if (num6 == 0f)
				{
					this.Md.Col.a = 0;
					this.Md.RectBL(0f, 0f, 0f, 0f, false);
				}
				else
				{
					this.Md.Col = c2;
					this.drawMeter(num6, true);
				}
				Bench.Pend("4");
				this.Md.Col = c;
				this.AtlRot.initAtlasMd(this.Md, base.M2D.MIchip);
				this.Md.RotaGraph3(0f, 0f, 0.5f, 0.5f, 1f, 1f, 0f, null, false);
			}
			Bench.P("5");
			this.Md.Identity().Scale((float)(this.Cp.flip ? (-1) : 1), 1f, false).Rotate(this.Cp.draw_rotR, false)
				.TranslateP(num, num2, false);
			this.Md.RotaMesh(0f, 0f, 1f, 1f, 0f, this.Cp.Img.getSrcMesh(this.layer), false, false);
			if (this.AtlTop.valid)
			{
				this.AtlTop.initAtlasMd(this.Md, base.M2D.MIchip);
				this.Md.RotaL(-sourceLayer.x, sourceLayer.y, this.LayTop, true, false, 0);
			}
			this.Md.Identity();
			Bench.Pend("5");
		}

		private void drawMeter(float level, bool under)
		{
			PxlLayer layer = this.SqAnim.getFrame(0).getLayer(0);
			M2ImageAtlas.AtlasRect atlasData = base.M2D.IMGS.Atlas.getAtlasData(layer.Img);
			if (!atlasData.valid)
			{
				return;
			}
			atlasData.initAtlasMd(this.Md, base.M2D.MIchip);
			float num = -layer.y;
			float num2 = (float)layer.Img.height * 0.5f;
			float num3 = (float)layer.Img.width * 0.5f;
			if (under)
			{
				this.Md.RectBL(-num3, -num - num2, num3 * 2f, num2 * 2f * level, false);
				return;
			}
			this.Md.RectBL(-num3, num - num2, num3 * 2f, num2 * 2f * level, false);
		}

		public void activate(float _maxt)
		{
			this.maxt = _maxt;
			this.t = 0f;
			this.refine_rotation = true;
			string s = this.Cp.Img.Meta.GetS("ptcst");
			if (TX.valid(s))
			{
				base.Mp.PtcSTsetVar("cx", (double)this.Cp.mapcx).PtcSTsetVar("cy", (double)this.Cp.mapcy).PtcSTsetVar("rot_t", 50.0)
					.PtcSTsetVar("maxt", (double)this.maxt)
					.PtcST(s, null, PTCThread.StFollow.NO_FOLLOW);
			}
		}

		public void deactivate()
		{
			if (this.maxt > 0f)
			{
				this.maxt = 0f;
				this.t = 0f;
				this.refine_rotation = true;
			}
		}

		public override int redraw(float fcnt)
		{
			if (!this.refine_rotation)
			{
				return base.redraw(fcnt);
			}
			this.refine_rotation = false;
			if (this.t < this.maxt)
			{
				this.t += fcnt;
			}
			int vertexMax = this.Md.getVertexMax();
			int triMax = this.Md.getTriMax();
			base.revertVerAndTriIndexFirstSaved(false);
			this.draw();
			this.Md.revertVerAndTriIndex(vertexMax, triMax, false);
			return base.layer2update_flag;
		}

		private PxlLayer LayRot;

		private M2ImageAtlas.AtlasRect AtlRot;

		private PxlLayer LayTop;

		private M2ImageAtlas.AtlasRect AtlTop;

		private PxlSequence SqAnim;

		private bool refine_rotation;

		private float maxt;

		private float t;

		private const float ROT_T = 50f;
	}
}
