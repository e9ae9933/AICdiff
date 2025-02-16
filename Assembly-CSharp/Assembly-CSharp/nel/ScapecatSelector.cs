using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class ScapecatSelector : ActiveSelector
	{
		public ScapecatSelector(PR _Pr, int _itm_grade, NelItem _TargetItem, ItemStorage _InvItemFrom)
			: base("SCAPECAT", "scapecat_activated")
		{
			this.Pr = _Pr;
			this.TargetItem = _TargetItem;
			this.InvItemFrom = _InvItemFrom;
			this.CenterPos = new Vector3(0f, 0.78125f, -4.5f);
			this.itm_grade = _itm_grade;
			this.M2D.loadMaterialSnd("mg_scapecat_burst");
		}

		public override void deactivate()
		{
			base.deactivate();
			base.playSnd(ref this.PreSndBD, ref this.pre_snd_cue_bd, null);
			if (this.Tx != null)
			{
				this.Tx.alpha = 0f;
				this.Tx.gameObject.SetActive(false);
			}
		}

		private void prepareTx()
		{
			if (this.Tx == null)
			{
				this.Tx = new GameObject("Tx-BurstDesc").AddComponent<TextRenderer>();
				this.Tx.html_mode = true;
				this.Tx.size = 20f;
				this.Tx.alignx = ALIGN.CENTER;
				this.Tx.BorderCol(4278190080U);
				this.Tx.Col(MTRX.ColWhite);
			}
			this.Tx.gameObject.layer = IN.gui_layer;
			this.Tx.use_valotile = true;
			this.Tx.gameObject.SetActive(true);
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddTxA("Item_cmd_respawn", false).Add("    ");
				NelItem.getGradeMeshTxTo(stb, this.itm_grade, 0, 34);
				this.Tx.Txt(stb);
			}
			this.Tx.alpha = 0f;
		}

		protected override bool drawEd(EffectItem Ef, M2DrawBinder Ed)
		{
			if (!base.drawEd(Ef, Ed))
			{
				if (Ed == this.EdDraw)
				{
					this.EdDraw = null;
				}
				return false;
			}
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5;
			if (Ed.t < 65536f)
			{
				num2 = Ed.t / 130f;
				num5 = X.ZLINE((float)(IN.totalframe - Ed.f0), 11f);
				if (num2 >= 0.25f)
				{
					float num6;
					num = (num6 = X.ZLINE(num2 - 0.25f, 0.75f));
					num3 = X.ZPOW(num6) * 10f * (0.85f * X.COSIT(X.NI(17.6f, 5.9f, num6)) + 0.15f * X.COSIT(X.NI(11.6f, 3.7f, num6)));
				}
				num4 = -(1f - num5) * 60f;
			}
			else
			{
				if (this.Pr.isBurstState())
				{
					num2 = 2f;
					num5 = 1f - X.ZLINE(Ed.t - 65536f, 30f);
				}
				else
				{
					num5 = 1f - X.ZLINE(Ed.t - 65536f, 15f);
					num4 = -(1f - num5) * 60f;
				}
				if (num5 <= 0f)
				{
					if (Ed == this.EdDraw)
					{
						this.EdDraw = null;
					}
					return false;
				}
			}
			float num7 = X.NIL(1f, 1.5f, num2, 1f);
			MeshDrawer mesh = Ef.GetMesh("burstsel", MTRX.MtrMeshAdd, false);
			MeshDrawer meshImg = Ef.GetMeshImg("burstsel", MTRX.MIicon, BLEND.NORMAL, false);
			float num8 = 1f / this.Pr.M2D.Cam.getScale(true);
			Vector3 vector = this.Pr.M2D.Cam.PosMainTransform + this.CenterPos * num8;
			if (mesh.getTriMax() == 0)
			{
				mesh.base_z -= 1.2f;
				meshImg.base_z -= 1.215f;
			}
			if (Ed.t < 65536f && this.Tx != null)
			{
				this.Tx.transform.position = new Vector3(this.Pr.M2D.ui_shift_x * 0.015625f + this.CenterPos.x, -128f * num7 * 0.015625f + this.CenterPos.y, 25f);
				this.Tx.alpha = num5;
			}
			mesh.base_x = (meshImg.base_x = vector.x);
			mesh.base_y = (meshImg.base_y = vector.y);
			meshImg.Col = meshImg.ColGrd.Black().mulA(0.5f * num5).C;
			float num9 = X.NI(140, 220, num5);
			meshImg.initForImg(MTRX.EffBlurCircle245, 0).Rect(num3, num4, num9, num9, false);
			meshImg.ColGrd.White();
			meshImg.Col = mesh.ColGrd.Set(meshImg.ColGrd).mulA(num5).C;
			mesh.Col = mesh.ColGrd.mulA(X.ZPOW(num2)).C;
			meshImg.Scale(num8, num8, false);
			mesh.Scale(num8, num8, false);
			mesh.ColGrd.mulA(0f);
			if (num2 > 0f)
			{
				mesh.BlurPoly2(num3, num4, 128f * num7 * 0.55f, 0f, 24, 128f * num7 * 0.25f, 128f * num7 * 1.25f, mesh.ColGrd, mesh.ColGrd);
			}
			mesh.Col = meshImg.Col;
			mesh.Poly(num3, num4, 128f * num7, 0f, 24, 2f, false, 0f, 0f);
			meshImg.RotaPF(num3, num4, num7 * 4f, num7 * 4f, 0f, MTR.AItemIcon[65], false, false, false, uint.MaxValue, false, 0);
			if (num > 0f)
			{
				mesh.Col = mesh.ColGrd.Set(meshImg.ColGrd).mulA(num * 4f).C;
				float num10 = X.NI(1, 12, num);
				float num11 = X.NI(2.25f, 1f, num) * num7 * 128f;
				mesh.Poly(num3 * 0.3f, num4, num11, 0f, 24, num10, false, 0f, 0f);
			}
			if (num2 == 2f)
			{
				mesh.StripedCircle(num3, num4, num7 * 128f, X.ANMPT(12, 1f), 0.33f * num5, 14f, false);
			}
			return true;
		}

		public bool runActivating()
		{
			bool flag = false;
			if (this.EdDraw == null || this.EdDraw.t >= 65536f)
			{
				this.deactivateEffect(true);
				if (this.decided)
				{
					return true;
				}
				this.prepareTx();
				base.selectInit(this.Pr.Mp.setEDT("scapecat_selector", this.FD_drawEd, 0f), 0f, 130f, 0.03846154f);
			}
			UIStatus.showHold(20, false);
			if (this.EdDraw.t >= 130f)
			{
				this.EdDraw.t = 65536f;
				this.EdDraw = null;
				flag = true;
				this.decided = true;
				this.runDeactivating(true);
			}
			else if (this.EdDraw.t >= 70f && this.PreSndBD == null)
			{
				base.playSnd(ref this.PreSndBD, ref this.pre_snd_cue_bd, "scapecat_clock");
			}
			base.runPE();
			return flag;
		}

		public void runDeactivating(bool strong = false)
		{
			if (this.EdDraw != null)
			{
				this.EdDraw.t = 65551f - X.ZLINE((float)(IN.totalframe - this.EdDraw.f0), 11f) * 15f;
				this.EdDraw = null;
				strong = true;
			}
			if (strong)
			{
				this.deactivate();
			}
		}

		public bool isActive()
		{
			return this.EdDraw != null;
		}

		public NelM2DBase M2D
		{
			get
			{
				return this.Pr.NM2D;
			}
		}

		private const float DEBUG_MARGIN = 0f;

		private const float BURST_SLOW_LATE_R = 26f;

		private const float BURST_SLOW_LATE = 0.03846154f;

		private const float BURST_HOLD_TIME = 130f;

		public const float DEF_WH = 128f;

		public readonly PR Pr;

		public int itm_grade;

		public readonly NelItem TargetItem;

		public readonly ItemStorage InvItemFrom;

		private const float FADE_T = 15f;

		private const float FADEIN_T = 11f;

		private M2SoundPlayerItem PreSndBD;

		private string pre_snd_cue_bd;

		public int add_danger;

		private Vector3 CenterPos;

		private TextRenderer Tx;

		public bool decided;

		private const float Z_TX = 25f;
	}
}
