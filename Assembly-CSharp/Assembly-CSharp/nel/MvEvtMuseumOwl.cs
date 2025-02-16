using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class MvEvtMuseumOwl : M2AttackableEventManipulatable
	{
		public override string get_parent_name()
		{
			return "owl";
		}

		public override void appear(Map2d Mp)
		{
			this.replace_tecon = false;
			base.appear(Mp);
			this.EvSphere = Mp.getMoverByName("sphere", false) as M2EventItem;
			this.LpSp = Mp.getLabelPoint("Ev_sphere");
			this.ParEv = this.Par as M2EventItem;
		}

		public override void destruct()
		{
			if (this.Ef != null)
			{
				this.Ef.destruct();
				this.Ef = null;
			}
			base.destruct();
		}

		public override void EvtRead(StringHolder rER)
		{
			string _ = rER._2;
			if (_ != null)
			{
				if (_ == "LIG_INIT")
				{
					this.t_lig_expand = 0f;
					this.Lig = this.ParEv.setLight(4285885548U, 0.01f);
					return;
				}
				if (_ == "LIG_SPHERE")
				{
					this.t_lig_sphere = 0f;
					this.LigSp = this.EvSphere.setLight(4283450436U, 0.01f);
					return;
				}
				if (!(_ == "SPHERE_FLASH") && !(_ == "SPHERE_FLASH_OUT"))
				{
					if (!(_ == "FIRST_PROJECTION"))
					{
						return;
					}
					if (this.Ef != null)
					{
						this.Ef.destruct();
					}
					SND.Ui.play("museum_dynamo", false);
					try
					{
						M2ChipImage m2ChipImage = base.M2D.IMGS.Get("grazia_museum/pic_pvv010_pn");
						if (m2ChipImage == null)
						{
							X.de("不明なイメージ grazia_museum/pic_pvv010_pn", null);
						}
						else
						{
							this.LpHalo = new EfParticleLooper("lp_museum_fproj_halo");
							this.PicAtlas = base.M2D.IMGS.Atlas.getAtlasData(m2ChipImage.SourceLayer.Img);
							this.Ef = this.Mp.getEffect().setEffectWithSpecificFn("first_projection", this.LpSp.mapfocx, this.LpSp.mapfocy, 0f, 0, 0, delegate(EffectItem Ef)
							{
								if (!this.fnDrawFirstProjection(Ef))
								{
									this.Ef = null;
									return false;
								}
								return true;
							});
						}
					}
					catch (Exception ex)
					{
						rER.tError("FIRST_PROJECTION:Error " + ex.ToString());
					}
				}
				else
				{
					if (this.Ef != null)
					{
						this.Ef.destruct();
					}
					this.Ef = this.Mp.getEffect().setEffectWithSpecificFn("sphere_flash", this.LpSp.mapfocx, this.LpSp.mapfocy, (float)((rER._2 == "SPHERE_FLASH_OUT") ? 1 : 0), 0, 0, delegate(EffectItem Ef)
					{
						if (!this.fnDrawSphereFlash(Ef, Ef.z == 1f))
						{
							this.Ef = null;
							return false;
						}
						return true;
					});
					this.Mp.PtcSTsetVar("cx", (double)this.LpSp.mapfocx).PtcSTsetVar("cy", (double)this.LpSp.mapfocy);
					if (rER._2 == "SPHERE_FLASH")
					{
						SND.Ui.play("ev_bright_aura", false);
						this.Mp.PtcST("lp_museum_sphere_flash", null, PTCThread.StFollow.NO_FOLLOW);
						return;
					}
					this.Mp.PtcST("lp_museum_sphere_flash_out", null, PTCThread.StFollow.NO_FOLLOW);
					return;
				}
			}
		}

		public override void runPre()
		{
			base.runPre();
			if (this.t_lig_expand >= 0f && this.t_lig_expand <= 90f)
			{
				this.t_lig_expand += this.TS;
				this.Lig.radius = X.NI(0.01f, 55f, X.ZSIN(this.t_lig_expand, 90f));
			}
			if (this.t_lig_sphere >= 0f && this.t_lig_sphere <= 90f)
			{
				this.t_lig_sphere += this.TS;
				this.LigSp.radius = X.NI(0.01f, 55f, X.ZSIN(this.t_lig_sphere, 90f));
			}
		}

		private bool fnDrawSphereFlash(EffectItem Ef, bool is_out)
		{
			if (this.Mp == null || base.destructed)
			{
				return false;
			}
			Vector3 posMainTransform = base.M2D.Cam.PosMainTransform;
			if (!is_out)
			{
				MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
				meshImg.ColGrd.Set(4290678157U);
				float num = 0.5f * X.COSI(Ef.af, 11.7f) + 0.25f * X.COSI(Ef.af, 3.19f);
				meshImg.Col = meshImg.ColGrd.mulA(X.saturate(X.ZPOW(Ef.af, 80f) + 0.08f + 0.06f * num) * (1f + num * 0.4f)).C;
				meshImg.initForImg(MTRX.EffBlurCircle245, 0);
				float num2 = 390f * X.ZPOW(Ef.af, 120f) * (1f + num * X.ZSIN(Ef.af, 20f) * 0.14f) + 400f * X.ZPOW(Ef.af - 30f, 200f);
				meshImg.Rect(0f, 0f, num2, num2, false);
				if (Ef.af >= 100f)
				{
					MeshDrawer mesh = Ef.GetMesh("", MTRX.MtrMeshAdd, false);
					mesh.Col = mesh.ColGrd.Set(4290678157U).mulA(X.ZLINE(Ef.af, 60f)).C;
					mesh.Rect(0f, 0f, IN.w + 60f, IN.h + 60f, false);
				}
			}
			else
			{
				float num3 = 1f - X.ZLINE(Ef.af, 170f);
				if (num3 <= 0f)
				{
					return false;
				}
				MeshDrawer mesh2 = Ef.GetMesh("", MTRX.MtrMeshAdd, false);
				mesh2.Col = mesh2.ColGrd.Set(4290678157U).mulA(num3).C;
				mesh2.Rect(0f, 0f, IN.w + 60f, IN.h + 60f, false);
			}
			return true;
		}

		private bool fnDrawFirstProjection(EffectItem Ef)
		{
			float num = (0.3f * X.COSI(Ef.af, 11.7f) + 0.2f * X.COSI(Ef.af, 3.19f)) * 0.6f + 0.5f;
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
			meshImg.initForImg(MTRX.EffBlurCircle245, 0);
			meshImg.Col = C32.MulA(4289823175U, num);
			meshImg.Rect(0f, 0f, 24f, 24f, false);
			meshImg.Col = C32.MulA(4289823175U, num * 0.5f);
			meshImg.Rect(0f, 0f, 54f, 54f, false);
			this.LpHalo.Draw(Ef, this.LpHalo.total_delay);
			MeshDrawer meshImg2 = Ef.GetMeshImg("", base.M2D.MIchip, BLEND.ADD, false);
			this.PicAtlas.initAtlasMd(meshImg2, base.M2D.IMGS.Atlas.getTexture());
			meshImg2.Col = C32.MulA(uint.MaxValue, num);
			meshImg2.RotaGraph(0.35f * this.Mp.CLENB, 90f, this.Mp.base_scale * 2f, 0f, null, false);
			return true;
		}

		private float t_lig_expand = -1f;

		private M2EventItem ParEv;

		private M2Light Lig;

		private M2Light LigSp;

		private M2LabelPoint LpSp;

		private M2EventItem EvSphere;

		private EffectItem Ef;

		private EfParticleLooper LpHalo;

		private M2ImageAtlas.AtlasRect PicAtlas;

		private const float final_lig_len = 55f;

		private float t_lig_sphere = -1f;

		private const string first_projection_src = "grazia_museum/pic_pvv010_pn";
	}
}
