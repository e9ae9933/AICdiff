using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class BurstSelector : ActiveSelector
	{
		public BurstSelector(PR _Pr, MagicSelector _MagSel)
			: base("BURST", null)
		{
			this.Pr = _Pr;
			this.MagSel = _MagSel;
			this.CenterPos = new Vector3(0f, 0.78125f, -4.5f);
		}

		public bool fineBurstMagic()
		{
			this.MKBurst = this.MagSel.getBurst();
			return this.MKBurst != null;
		}

		public void clearExecuteCount()
		{
			this.execute_count = (this.fainted_ratio = 0f);
		}

		public void newGame()
		{
			this.deactivate();
			this.clearExecuteCount();
			this.MKBurst = null;
		}

		public override void deactivate()
		{
			base.deactivate();
			this.stop_sink = false;
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
			this.fineFaintedRatio();
			this.Tx.alpha = 0f;
		}

		public void fineFaintedRatio()
		{
			this.fainted_ratio = MDAT.calcBurstFaintedRatio(this.Pr, this.MKBurst, this.execute_count, this.Pr.Ser.burstConsumeRatio());
			if (this.Tx != null)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.AddTxA("BurstSel_fainted_ratio", false).TxRpl(X.IntC(this.fainted_ratio * 100f));
					this.Tx.Txt(stb);
				}
			}
		}

		protected override bool drawEd(EffectItem Ef, M2DrawBinder Ed)
		{
			if (!base.drawEd(Ef, Ed) || this.MKBurst == null)
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
			bool flag = false;
			float num4 = 0f;
			float num5;
			if (Ed.t < 65536f)
			{
				num2 = Ed.t / 32768f;
				num5 = X.ZLINE((float)(IN.totalframe - Ed.f0), 11f);
				if (num2 >= 0.25f)
				{
					float num6;
					num = (num6 = X.ZLINE(num2 - 0.25f, 0.75f));
					num3 = X.ZPOW(num6) * 10f * (0.85f * X.COSIT(X.NI(17.6f, 5.9f, num6)) + 0.15f * X.COSIT(X.NI(11.6f, 3.7f, num6)));
				}
				flag = !this.stop_sink;
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
			MeshDrawer meshImg = Ef.GetMeshImg("burstsel", MTR.MIiconL, BLEND.NORMAL, false);
			float num8 = 1f / this.Pr.M2D.Cam.getScale(true);
			Vector3 vector = this.Pr.M2D.Cam.PosMainTransform + this.CenterPos * num8;
			if (mesh.getTriMax() == 0)
			{
				mesh.base_z -= 1.2f;
				meshImg.base_z -= 1.215f;
			}
			if (Ed.t < 65536f && this.Tx != null)
			{
				this.Tx.transform.position = new Vector3(this.Pr.M2D.ui_shift_x * 0.015625f + this.CenterPos.x, -256f * num7 * 0.44f * 0.015625f + this.CenterPos.y, 25f);
				this.Tx.alpha = num5;
			}
			mesh.base_x = (meshImg.base_x = vector.x);
			mesh.base_y = (meshImg.base_y = vector.y);
			meshImg.ColGrd.White().blend(4294901760U, X.ZPOW(this.fainted_ratio - 0.125f, 0.875f));
			meshImg.Col = mesh.ColGrd.Set(meshImg.ColGrd).mulA(num5).C;
			mesh.Col = mesh.ColGrd.mulA(X.ZPOW(num2)).C;
			meshImg.Scale(num8, num8, false);
			mesh.Scale(num8, num8, false);
			mesh.ColGrd.mulA(0f);
			if (num2 > 0f)
			{
				mesh.BlurPoly2(num3, num4, 256f * num7 * 0.55f, 0f, 4, 256f * num7 * 0.25f, 256f * num7 * 1.25f, mesh.ColGrd, mesh.ColGrd);
			}
			mesh.Col = meshImg.Col;
			mesh.Daia3(num3, num4, 256f * num7, 256f * num7, 4f, 4f, false);
			meshImg.RotaPF(num3, num4, num7, num7, 0f, MTR.AMagicIconL[this.MKBurst.icon_index], false, false, false, uint.MaxValue, false, 0);
			if (flag)
			{
				float num9 = 153.6f;
				mesh.Line(0f + num9, num4 + num9, 0f - num9, num4 - num9, 4f, false, 0f, 0f);
			}
			if (num > 0f)
			{
				mesh.Col = mesh.ColGrd.Set(meshImg.ColGrd).mulA(num * 4f).C;
				float num10 = X.NI(1, 12, num);
				float num11 = X.NI(2.25f, 1f, num) * num7 * 256f;
				mesh.Daia3(num3 * 0.3f, num4, num11, num11, num10, num10, false);
			}
			if (num2 == 2f)
			{
				mesh.StripedDaia(num3, num4, num7 * 256f, X.ANMPT(12, 1f), 0.33f * num5, 14f, false);
			}
			return true;
		}

		public bool runActivating(ref float t, float t_prefix, float t_error_time, bool can_progress_burst)
		{
			bool flag = false;
			if (this.EdDraw == null || this.EdDraw.t >= 65536f)
			{
				if (this.MKBurst == null && (t >= t_prefix + t_error_time || !this.fineBurstMagic()))
				{
					t = t_prefix + t_error_time;
					this.runDeactivating(false);
					return false;
				}
				this.prepareTx();
				this.deactivateEffect(true);
				base.selectInit(this.Pr.Mp.setEDT("magic_selector", this.FD_drawEd, 0f), 10f, -1f, 0.16666667f);
			}
			if (can_progress_burst)
			{
				UIStatus.showHold(20, false);
				this.Pr.getSkillManager().mana_drain_lock_t = 5f;
				float num = (((this.Pr.isBreatheStop(false, false) || this.Pr.isBurstFastState()) && this.fainted_ratio < 0.25f) ? 0.16666667f : (0.071428575f * (this.Pr.isFacingEnemy() ? 1f : 0.66f)));
				if (this.PeSlow != null)
				{
					this.PeSlow.x = 1f - num;
				}
				t += num;
				this.stop_sink = true;
				if (t >= t_prefix + 5f + 0f)
				{
					this.EdDraw.t = 65536f;
					this.EdDraw = null;
					flag = true;
					this.execute_count = MDAT.progressBurstExecuteCount(this.execute_count);
					this.runDeactivating(true);
				}
				else
				{
					if (this.EdDraw != null)
					{
						this.EdDraw.t = X.ZLINE((t - t_prefix) / 5f) * 32768f;
					}
					if (t >= t_prefix + 5f - num * 60f)
					{
						PR.PunchDecline(15, false);
						if (this.PreSndBD == null)
						{
							base.playSnd(ref this.PreSndBD, ref this.pre_snd_cue_bd, "burst_prepare_basedrop");
						}
					}
					if (this.PreSnd == null)
					{
						base.playSnd(ref this.PreSnd, ref this.pre_snd_cue, "burst_prepare_ui");
					}
				}
			}
			else
			{
				if (this.PreSnd != null)
				{
					this.PreSnd.getPlayerInstance().StopWithoutReleaseTime();
				}
				if (this.PreSndBD != null)
				{
					this.PreSndBD.getPlayerInstance().StopWithoutReleaseTime();
				}
				base.playSnd(ref this.PreSnd, ref this.pre_snd_cue, null);
				base.playSnd(ref this.PreSndBD, ref this.pre_snd_cue_bd, null);
				this.stop_sink = false;
				t = t_prefix;
				if (this.EdDraw != null)
				{
					this.EdDraw.t = t - t_prefix;
				}
				if (this.PeSlow != null)
				{
					this.PeSlow.x = 0f;
				}
			}
			base.runPE();
			return flag;
		}

		public void runDeactivating(bool strong = false)
		{
			if (this.EdDraw != null)
			{
				if (this.stop_sink)
				{
					this.EdDraw.t = 65551f - X.ZLINE((float)(IN.totalframe - this.EdDraw.f0), 11f) * 15f;
					this.EdDraw = null;
				}
				strong = true;
			}
			if (strong)
			{
				this.deactivate();
			}
			this.stop_sink = false;
		}

		public void reduceBurstExecute(float TS)
		{
			if (this.execute_count > 0f)
			{
				this.execute_count = X.Mx(this.execute_count - TS * 0.0027777778f, 0f);
			}
		}

		public bool isActive()
		{
			return this.EdDraw != null;
		}

		private const float DEBUG_MARGIN = 0f;

		private const float BURST_HOLD_TIME = 5f;

		private const float BURST_SLOW_FAST = 0.16666667f;

		private const float BURST_SLOW_LATE = 0.071428575f;

		private const float EFFECT_FIX_T = 32768f;

		private const float BURST_SLOW_DISABLED = 1f;

		public const float DEF_WH = 256f;

		public readonly PR Pr;

		private readonly MagicSelector MagSel;

		private const float FADE_T = 15f;

		private const float FADEIN_T = 11f;

		public bool stop_sink;

		public float fainted_ratio;

		public float execute_count;

		private M2SoundPlayerItem PreSndBD;

		private string pre_snd_cue_bd;

		private MagicSelector.KindData MKBurst;

		private Vector3 CenterPos;

		private TextRenderer Tx;

		private const float Z_TX = 25f;
	}
}
