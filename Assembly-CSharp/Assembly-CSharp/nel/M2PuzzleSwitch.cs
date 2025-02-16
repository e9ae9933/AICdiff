using System;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2PuzzleSwitch : M2PuzzleGimmick
	{
		public void initPuzzleSwitch(M2DBase M2D, NelChipPuzzleSwitch _Cp, int _puzzle_id, M2LabelPointSwitchContainer LpArea = null)
		{
			if (this.EdC != null)
			{
				this.remEf();
			}
			this.t = -27f;
			this.Cp = _Cp;
			this.CpSwitch = _Cp;
			this.Cp.light_alpha = 0f;
			this.puzzle_id = _puzzle_id;
			if (this.Cp.rotation % 2 == 0)
			{
				base.Size((float)this.Cp.iwidth, X.Mx(40f, 10f + (float)this.Cp.iheight), ALIGN.CENTER, ALIGNY.MIDDLE, false);
			}
			else
			{
				base.Size(X.Mx(40f, (float)this.Cp.iwidth + 10f), (float)this.Cp.iheight, ALIGN.CENTER, ALIGNY.MIDDLE, false);
			}
			PxlCharacter pxlM2DGeneral = MTR.PxlM2DGeneral;
			this.AnimSqCrystal = pxlM2DGeneral.getPoseByName("_anim_crystal").getSequence(0);
			PxlPose poseByName = pxlM2DGeneral.getPoseByName(this.Cp.getMeta().GetS("animsq_base"));
			if (poseByName != null)
			{
				this.AnimSq = poseByName.getSequence(0);
				string[] array = this.Cp.getMeta().Get("animsq_position");
				if (array != null)
				{
					this.Aanm_sq_pos = new float[array.Length];
					for (int i = array.Length - 1; i >= 0; i--)
					{
						string text = array[i];
						float num;
						if (i != 0)
						{
							if (i != 1)
							{
								num = 0f;
							}
							else
							{
								num = (float)this.Cp.iheight * 0.5f;
							}
						}
						else
						{
							num = (float)this.Cp.iwidth * 0.5f;
						}
						float[] aanm_sq_pos = this.Aanm_sq_pos;
						int num2 = i;
						aanm_sq_pos[num2] = X.Nm(text, num, false);
					}
				}
			}
			this.pos_shifty = this.Cp.getMeta().GetNm("pos_shifty", (float)(-(float)this.Cp.Img.iheight) * 0.5f - 10f, 0);
			this.crystal_float_y = this.Cp.getMeta().GetNm("crystal_float_y", 25f, 0);
			this.crystal_agR = this.Cp.getMeta().GetNm("crystal_ag360", 0f, 0) / 180f * 3.1415927f;
			this.no_deactivate = this.Cp.getMeta().GetB("no_deactivate", false);
			this.no_write_sf_ = -1;
			if (LpArea != null)
			{
				if (LpArea.Meta.Get("no_write_sf") != null)
				{
					this.no_write_sf_ = LpArea.Meta.GetI("no_write_sf", 1, 0);
				}
				string[] array2 = LpArea.Meta.Get("ev_activate");
				this.no_auto_target = LpArea.Meta.GetB("no_auto_target", false);
				if (array2 != null)
				{
					this.ev_activate = array2[0];
					if (array2.Length != 0)
					{
						X.shift<string>(ref array2, 1);
						this.Aev_activate_var = array2;
					}
				}
				string key = LpArea.key;
				string text2 = "_";
				int num2 = LpArea.switch_index;
				LpArea.switch_index = num2 + 1;
				base.key = key + text2 + num2.ToString();
			}
			Material mtr = M2D.MIchip.getMtr(MTR.getShd("Hachan/ShaderGDTWhiteColoring"), -50 - this.puzzle_id);
			mtr.SetColor("_WhiteColor", C32.d2c(MTR.Apuzzle_switch_col[this.puzzle_id]));
			mtr.SetColor("_Color", C32.d2c(MTR.Apuzzle_switch_col_dark[this.puzzle_id]));
			this.MtrPuzChip = mtr;
			this.maxhp = 9999;
			this.Cp.Mp.assignMover(this, false);
			this.FD_drawOnBinder = new M2DrawBinder.FnEffectBind(this.drawOnBinder);
			this.EdC = this.Mp.setEDC("puzswitch_c", this.FD_drawOnBinder, 0f);
			this.repositionToChip();
			this.fineHittingLayer();
		}

		public override void repositionToChip()
		{
			Vector2 vector = this.Cp.PixelToMapPoint((float)(this.Cp.Img.iwidth / 2), (float)this.Cp.Img.iheight * 0.5f + this.pos_shifty);
			this.setTo(vector.x, vector.y);
		}

		public override void activate()
		{
			this.activate(false, true);
		}

		public void activate(bool immediate, bool call_event = true)
		{
			if (this.isActive())
			{
				return;
			}
			this.t = (float)(immediate ? 20 : 0);
			if (this.EdEf != null)
			{
				this.EdEf = this.Mp.remED(this.EdEf);
			}
			this.EdEf = this.Mp.setED("puzswitch", this.FD_drawOnBinder, 0f);
			if (!immediate && this.Mp.floort > 10f)
			{
				this.defineParticlePreVariable();
				this.Mp.PtcST("puzzle_switch_activate", null, PTCThread.StFollow.NO_FOLLOW);
			}
			PuzzLiner.activateConnector(this, immediate);
			this.Cp.light_alpha = 1f;
			if (call_event && TX.valid(this.ev_activate))
			{
				EV.stack(this.ev_activate, 0, -1, this.Aev_activate_var, null);
			}
			this.SaveActivateState();
		}

		public override void deactivate()
		{
			if (this.Mp == null || !this.isActive())
			{
				return;
			}
			this.t = -1f;
			if (this.Mp.floort > 10f)
			{
				this.defineParticlePreVariable();
				this.Mp.PtcST("puzzle_switch_deactivate", null, PTCThread.StFollow.NO_FOLLOW);
			}
			this.Cp.light_alpha = 0f;
			PuzzLiner.deactivateConnector(this, false);
			this.SaveActivateState();
		}

		public override void runPre()
		{
			bool need_finalize = this.need_finalize;
			base.runPre();
			if (need_finalize && !this.need_finalize)
			{
				if (this.no_write_sf_ == -1)
				{
					this.no_write_sf_ = ((this.BelongLp != null && this.BelongLp.isManageableSwitchId(this.puzzle_id)) ? 1 : 0);
				}
				if (!this.no_write_sf && COOK.getSF(this.sf_key) != 0)
				{
					this.activate(true, false);
					this.CpSwitch.fineActivation();
				}
			}
			if (this.t >= 0f)
			{
				this.t += Map2d.TS;
				return;
			}
			if (this.t > -27f)
			{
				this.t = X.Mx(-27f, this.t - Map2d.TS);
				if (this.t == -27f && this.EdEf != null)
				{
					this.EdEf = this.Mp.remED(this.EdEf);
				}
			}
		}

		public override int applyHpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			if (!force && this.applyHpDamageRatio(Atk) == 0f)
			{
				return 0;
			}
			if (val > 0)
			{
				if (!this.isActive())
				{
					if (this.t < -1f)
					{
						this.activate(false, true);
					}
				}
				else if (this.t > 0f)
				{
					if (this.no_deactivate)
					{
						return 0;
					}
					this.deactivate();
				}
			}
			return 0;
		}

		public void SaveActivateState()
		{
			if (!this.no_write_sf)
			{
				COOK.setSF(this.sf_key, this.isActive() ? 1 : 0);
			}
		}

		public override int applyMpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			return 0;
		}

		private bool drawOnBinder(EffectItem Ef, M2DrawBinder Ed)
		{
			bool flag = Ed == this.EdC;
			if (!flag && Ed != this.EdEf)
			{
				this.EdEf = null;
				return false;
			}
			if (flag || this.isActive() || this.t > -27f)
			{
				float num = X.COSIT_floating(0.5f);
				float num2 = 1.5707964f + this.Cp.draw_rotR + this.crystal_agR;
				float num3 = (flag ? ((this.t >= 0f) ? X.ZSIN2(this.t, 20f) : (1f - X.ZPOW(-this.t, 27f))) : 1f) + 0.15f * num * X.ZLINE(this.t, 20f);
				float num4 = this.crystal_float_y * num3 * this.Mp.rCLEN;
				Ef.x = (this.ef_x = base.x + num4 * X.Cos(num2));
				Ef.y = (this.ef_y = base.y - num4 * X.Sin(num2));
				Material material;
				if (flag && this.isActive())
				{
					material = this.MtrPuzChip;
				}
				else
				{
					material = (flag ? base.M2D.MIchip.getMtr(BLEND.NORMAL, -1) : MTRX.MIicon.getMtr(BLEND.ADD, -1));
				}
				MeshDrawer meshDrawer = this.initMdRot(Ef.GetMesh("", material, flag), flag, false);
				if (flag)
				{
					if (this.isActive())
					{
						meshDrawer.base_z -= 0.0002f;
					}
					if (this.t > -27f)
					{
						meshDrawer.Col = (this.isActive() ? C32.d2c(4282335039U) : MTRX.ColWhite);
						int num5 = 5;
						int num6 = ((this.t >= 0f) ? ((int)(X.ZLINE(this.t, 20f) * (float)num5 * 2f) + ((this.t < 20f) ? 0 : X.ANM((int)this.t, num5, 9f))) : ((int)((1f - X.ZLINE(-this.t, 27f)) * (float)num5))) % num5;
						PxlLayer pxlLayer = this.AnimSqCrystal.getFrame(num6).getLayer(0);
						if (this.Mp.M2D.IMGS.initAtlasMd(meshDrawer, pxlLayer.Img))
						{
							meshDrawer.RotaGraph(0f, 0f, 1f, this.crystal_agR, null, false);
						}
						if (this.t >= 0f)
						{
							meshDrawer = this.initMdRot(Ef.GetMeshImg("", base.M2D.MIchip, BLEND.ADD, flag), flag, false);
							pxlLayer = this.AnimSqCrystal.getFrame(num6 + num5).getLayer(0);
							meshDrawer.Col = meshDrawer.ColGrd.White().blend(MTR.Apuzzle_switch_col[this.puzzle_id], 0.375f + num * 0.375f).C;
							if (this.Mp.M2D.IMGS.initAtlasMd(meshDrawer, pxlLayer.Img))
							{
								meshDrawer.RotaGraph(0f, 0f, 1f, this.crystal_agR, null, false);
							}
						}
					}
					if (this.t < 0f)
					{
						PxlLayer pxlLayer2 = this.AnimSqCrystal.getFrame(this.AnimSqCrystal.countFrames() - 1).getLayer(0);
						float num7 = 0.85f + 0.125f * num;
						float num8 = X.ZPOW(-this.t, 27f);
						meshDrawer.Col = meshDrawer.ColGrd.Set(MTR.Apuzzle_switch_col[this.puzzle_id]).mulA(num8).multiply(num7, false)
							.C;
						if (this.Mp.M2D.IMGS.initAtlasMd(meshDrawer, pxlLayer2.Img))
						{
							meshDrawer.RotaGraph(0f, 0f, 1f, this.crystal_agR, null, false);
						}
						pxlLayer2 = this.AnimSqCrystal.getFrame(0).getLayer(0);
						if (this.Mp.M2D.IMGS.initAtlasMd(meshDrawer, pxlLayer2.Img))
						{
							float num9 = -0.4f + 0.6f * num;
							if (num9 > 0f)
							{
								meshDrawer.Col = meshDrawer.ColGrd.Set(MTR.Apuzzle_switch_col[this.puzzle_id]).mulA(num8 * num9).C;
								meshDrawer.RotaGraph(0f, 0f, 1f, this.crystal_agR, null, false);
							}
						}
					}
				}
				else
				{
					float num10 = ((this.t >= 0f) ? X.ZSIN2(this.t, 20f) : (1f - X.ZSIN2(-this.t, 27f)));
					float num11 = ((this.t >= 0f) ? (1f - X.ZLINE((float)((int)this.t % 20), 20f)) : num10);
					float num12 = ((this.t >= 0f) ? (0.4f + num11 * 0.5f) : num11);
					MeshDrawer meshDrawer2 = this.initMdRot(Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, true), flag, false);
					float num13 = ((this.t >= 0f) ? num10 : X.ZPOW(num10 - 0.3f, 0.6f));
					float num14 = 2.5f - 1.5f * num13;
					meshDrawer.Col = C32.MulA(MTR.Apuzzle_switch_col[this.puzzle_id], num11 * (0.9f + 0.1f * X.COSIT(3.92f)));
					meshDrawer.RotaPF(0f, 0f, num13, num14, this.crystal_agR, MTR.APuzzleMarker[this.puzzle_id], false, false, false, uint.MaxValue, false, 0);
					meshDrawer.Col = C32.MulA(MTR.Apuzzle_switch_col[this.puzzle_id], num12 * (0.6f + 0.05f * X.COSIT(11.37f) + 0.05f * X.COSIT(14.57f)));
					meshDrawer.RotaPF(0f, 0f, num13, num14, this.crystal_agR, MTR.APuzzleMarker[this.puzzle_id + 5], false, false, false, uint.MaxValue, false, 0);
					meshDrawer2.Col = C32.MulA(MTR.Apuzzle_switch_col_sub[this.puzzle_id], num12 * (0.85f + 0.15f * X.COSIT(7.12f)));
					meshDrawer2.RotaPF(0f, 0f, num13 * 1.3f, num14 * 1.3f, this.crystal_agR, MTR.APuzzleMarker[this.puzzle_id + 5], false, false, false, uint.MaxValue, false, 0);
					meshDrawer2.Identity();
				}
				meshDrawer.Identity();
			}
			if (this.AnimSq != null && (flag || this.isActive()))
			{
				Vector2 vector = this.Cp.PixelToMapPoint((this.Aanm_sq_pos != null && this.Aanm_sq_pos.Length >= 1) ? this.Aanm_sq_pos[0] : ((float)this.Cp.Img.iwidth * 0.5f), (this.Aanm_sq_pos != null && this.Aanm_sq_pos.Length >= 2) ? this.Aanm_sq_pos[1] : ((float)((int)((float)this.Cp.Img.iheight - (float)this.AnimSq.pPose.height * 0.5f))));
				Ef.x = vector.x;
				Ef.y = vector.y;
				Material material = (flag ? this.Mp.MyDrawerT.getMaterial() : this.MtrPuzChip);
				MeshDrawer meshDrawer = this.initMdRot(Ef.GetMesh("", material, false), flag, flag);
				int num15 = ((this.t < 0f) ? X.IntR(X.NI(4, 0, X.ZLINE(-this.t, 27f))) : (1 + X.ANML((int)this.t, this.AnimSq.countFrames() - 1, 6, this.AnimSq.loop_to - 1)));
				PxlFrame frame = this.AnimSq.getFrame(num15);
				int num16 = frame.countLayers();
				for (int i = 0; i < num16; i++)
				{
					PxlLayer layer = frame.getLayer(i);
					if (TX.isStart(layer.name, "switch", 0) == flag && this.Mp.M2D.IMGS.initAtlasMd(meshDrawer, layer.Img))
					{
						meshDrawer.DrawCen(layer.x, -layer.y, null);
					}
				}
				meshDrawer.Identity();
			}
			return true;
		}

		private MeshDrawer initMdRot(MeshDrawer Md, bool is_chip, bool color_gray = false)
		{
			Md.Identity();
			if (!is_chip)
			{
				Md.Scale(this.Mp.base_scale, this.Mp.base_scale, false);
			}
			if (this.Cp.flip)
			{
				Md.Scale(-1f, 1f, false);
			}
			Md.Rotate(this.Cp.draw_rotR, false);
			Md.Col = (color_gray ? C32.d2c(4286545791U) : MTRX.ColWhite);
			return Md;
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			this.remEf();
			base.destruct();
		}

		private void remEf()
		{
			if (this.Mp == null)
			{
				this.EdC = null;
				this.EdEf = null;
				return;
			}
			if (this.EdC != null)
			{
				this.EdC = this.Mp.remED(this.EdC);
			}
			if (this.EdEf != null)
			{
				this.EdEf = this.Mp.remED(this.EdEf);
			}
		}

		public override HITTYPE getHitType(M2Ray Ray)
		{
			return HITTYPE.OTHER;
		}

		public override RAYHIT can_hit(M2Ray Ray)
		{
			if ((this.no_deactivate && this.isActive()) || !this.CpSwitch.can_hit())
			{
				return RAYHIT.NONE;
			}
			return base.can_hit(Ray) | (this.no_auto_target ? RAYHIT.DO_NOT_AUTO_TARGET : RAYHIT.NONE);
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			float num = base.applyHpDamageRatio(Atk) * (float)(this.isDamagingOrKo() ? 0 : 1);
			if (num <= 0f)
			{
				return 0f;
			}
			if (Atk != null && Atk.AttackFrom is M2MoverPr && Atk.AttackFrom == this.Mp.Pr && !this.Mp.playerActionUseable())
			{
				return 0f;
			}
			return num;
		}

		public override void defineParticlePreVariable()
		{
			this.Mp.PtcSTsetVar("x", (double)this.ef_x).PtcSTsetVar("y", (double)this.ef_y).PtcSTsetVar("agR", (double)this.Cp.draw_rotR)
				.PtcSTsetVar("col", (double)this.puzzle_id);
		}

		public string sf_key
		{
			get
			{
				return "SWT_" + this.Mp.key + "__" + this.puzzle_id.ToString();
			}
		}

		public bool no_write_sf
		{
			get
			{
				return this.no_write_sf_ != 0;
			}
		}

		public bool isBelongPuzArea(bool consider_puzzle_id = true)
		{
			return this.BelongLp != null && (!consider_puzzle_id || this.BelongLp.isManageableSwitchId(this.puzzle_id));
		}

		public override bool isDamagingOrKo()
		{
			return -27f < this.t && this.t < 20f;
		}

		public bool isActive()
		{
			return this.t >= 0f;
		}

		public int puzzle_id;

		private M2DrawBinder EdC;

		private M2DrawBinder EdEf;

		private PxlSequence AnimSq;

		private PxlSequence AnimSqCrystal;

		private float[] Aanm_sq_pos;

		private float t = -27f;

		private const int FADE_T = 27;

		public const int FADEIN_T = 20;

		private const float shifty = 10f;

		private float pos_shifty = -10f;

		private float crystal_float_y;

		private float crystal_agR;

		private bool no_deactivate;

		private bool no_auto_target;

		private int no_write_sf_ = -1;

		public string ev_activate;

		private string[] Aev_activate_var;

		private float ef_x;

		private float ef_y;

		private Material MtrPuzChip;

		private NelChipPuzzleSwitch CpSwitch;

		private M2DrawBinder.FnEffectBind FD_drawOnBinder;
	}
}
