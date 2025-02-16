using System;
using Better;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class PrimulaPVV11 : M2AttackableEventManipulatable
	{
		public override void appear(Map2d Mp)
		{
			base.appear(Mp);
			if (this.Shield == null)
			{
				this.Shield = new M2Shield(this, MGHIT.EN);
				this.Shield.appearable_time = 6000f;
			}
			if (this.Anm != null && this.OCanePos == null)
			{
				this.OCanePos = new BDic<PxlFrame, Vector2>();
			}
			Mp.M2D.loadMaterialSnd("ev_primula");
			Mp.M2D.loadMaterialSnd("enemy_mush");
			this.FD_MgDrawMistShot = new MagicItem.FnMagicRun(this.MgDrawMistShot);
			this.FD_MgRunMistShot = new MagicItem.FnMagicRun(this.MgRunMistShot);
			this.MkSleep = new MistManager.MistKind(NelNMush.MkSleep);
			this.MkSleep.adding_damage_count = 4;
		}

		public override void EvtRead(StringHolder rER)
		{
			int num = rER.Int(2, -1);
			this.changeState((PrimulaPVV11.STATE)num);
		}

		private void changeState(PrimulaPVV11.STATE statei)
		{
			this.Shield.deactivate(false, false);
			base.killPtc(PtcHolder.PTC_HOLD.ACT);
			this.phase = 0;
			this.t = 0f;
			this.fail = PrimulaPVV11.PRFAIL.NONE;
			this.failt = 0f;
			this.Par.breakPoseFixOnWalk(1);
			base.aimToPr();
			this.Par.SpSetPose("stand_cane", -1, null, false);
			if (statei < PrimulaPVV11.STATE.NONE)
			{
				this.Mp.removeMover(this);
				return;
			}
			this.state = statei;
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			this.Shield.run(this.TS, 1f, -1f);
			switch (this.state)
			{
			case PrimulaPVV11.STATE.MIST:
			{
				if (this.Pr.Ser.has(SER.SLEEP))
				{
					this.fail |= PrimulaPVV11.PRFAIL.SLEEP;
				}
				bool flag = true;
				if (this.Pr.isBreatheStop(false, false))
				{
					flag = false;
				}
				if (this.phase >= 51 && this.Pr.Skill.canShildGuard() && this.t < 330f)
				{
					this.failt += 1f;
				}
				if (base.PhaseP(0, 60))
				{
					this.Shield.activate(false);
				}
				else if (base.PhaseP(1, 25))
				{
					this.Par.SpSetPose("chant", -1, null, false);
				}
				else if (base.PhaseP(2, 12))
				{
					base.PtcST("primula_mist_charge", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
				}
				else if (base.PhaseP(3, 115))
				{
					base.killPtc(PtcHolder.PTC_HOLD.ACT);
					this.phase = 10;
					this.t = 100f;
				}
				else if (10 <= this.phase && this.phase < 16 && this.t >= 6f)
				{
					this.initMistShot(this.phase - 10);
					this.t = 0f;
					this.phase++;
					if (this.phase >= 16)
					{
						this.phase = 50;
					}
				}
				if (base.PhaseP(50, 80))
				{
					this.Par.SpSetPose("stand_cane", -1, null, false);
				}
				if (this.phase == 51)
				{
					if ((this.fail & PrimulaPVV11.PRFAIL.SLEEP) != PrimulaPVV11.PRFAIL.NONE)
					{
						this.t = X.Mx(this.t, 360f);
					}
					else if (!flag)
					{
						this.t = X.Mn(this.t, 260f);
					}
					if (base.PhaseP(51, 480))
					{
						if ((this.fail & PrimulaPVV11.PRFAIL.SLEEP) != PrimulaPVV11.PRFAIL.NONE)
						{
							EV.stack("s04_3b", 0, -1, null, null);
						}
						else if (this.failt < 120f)
						{
							EV.stack("s04_3c", 0, -1, null, null);
						}
						else
						{
							EV.stack("s04_3f", 0, -1, null, null);
						}
						this.changeState(PrimulaPVV11.STATE.NONE);
						return;
					}
				}
				break;
			}
			case PrimulaPVV11.STATE.EVADE_TEST:
			{
				bool flag2 = false;
				if (this.Pr.isEvadeState() && this.Pr.get_state_time() <= 5f)
				{
					if (this.failt >= 0f)
					{
						this.failt = -(this.failt + 1f);
						flag2 = true;
					}
				}
				else if (this.failt < 0f)
				{
					this.failt = -this.failt;
				}
				if (base.PhaseP(0, 20))
				{
					flag2 = true;
				}
				if (this.phase == 1 && flag2)
				{
					if (X.Abs(this.failt) >= 3f)
					{
						this.phase = 2;
						this.t = 0f;
						EV.TutoBox.RemText(true, false);
					}
					else
					{
						EV.TutoBox.AddText("&&Tuto_evade_" + X.Abs(this.failt).ToString(), -1, null);
						EV.TutoBox.setPosition("C", "T");
					}
				}
				if (base.PhaseP(2, 60))
				{
					this.changeState(PrimulaPVV11.STATE.NONE);
					EV.stack("s04_4a_2", 0, -1, null, null);
					return;
				}
				break;
			}
			case PrimulaPVV11.STATE.EVADE_WAVE:
				if (this.phase < 50 && (this.fail & PrimulaPVV11.PRFAIL.HITTED) != PrimulaPVV11.PRFAIL.NONE && ((this.fail & PrimulaPVV11.PRFAIL.GUARD_SUCCESS) == PrimulaPVV11.PRFAIL.NONE || this.phase >= 20))
				{
					if (this.phase < 20)
					{
						this.phase = 50;
						this.t = 0f;
					}
					else
					{
						this.phase = 51;
						this.t = 0f;
						this.Par.SpSetPose("stand_cane", -1, null, false);
					}
				}
				if (this.phase == 0 && (this.Pr.isDamagingOrKo() || this.Pr.isGaraakiState()))
				{
					this.t = X.Mn(this.t, 20f);
				}
				if (base.PhaseP(0, 60))
				{
					this.Par.SpSetPose("chant", -1, null, false);
					base.PtcST("primula_wave_charge", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
				}
				if (base.PhaseP(1, 120))
				{
					base.killPtc(PtcHolder.PTC_HOLD.ACT);
					base.PtcST("primula_wave_charge_w1", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
					this.Par.SpSetPose("shot", -1, null, false);
				}
				if (base.PhaseP(2, 30))
				{
					this.phase = 10;
					this.t = 100f;
					if (GF.getC("PRM2") >= 2U)
					{
						this.fail |= PrimulaPVV11.PRFAIL.SLEEP;
					}
				}
				if (this.phase >= 10 && this.phase < 13 && this.t >= (float)(((this.fail & PrimulaPVV11.PRFAIL.SLEEP) != PrimulaPVV11.PRFAIL.NONE) ? 100 : 75))
				{
					this.t = 0f;
					this.phase++;
					this.initShockWave();
					if (this.phase == 13)
					{
						this.phase = 20;
					}
				}
				if (base.PhaseP(20, 50))
				{
					this.Par.SpSetPose("stand_cane", -1, null, false);
				}
				if (base.PhaseP(21, 150))
				{
					EV.stack("s04_4g", 0, -1, null, null);
					this.changeState(PrimulaPVV11.STATE.NONE);
					return;
				}
				if (this.phase >= 50)
				{
					if (base.PhaseP(50, 30))
					{
						base.PtcST("primula_magic_abort", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.Par.SpSetPose("stand_cane", -1, null, false);
					}
					if (base.PhaseP(51, 170))
					{
						if ((this.fail & PrimulaPVV11.PRFAIL.GUARD_SUCCESS) != PrimulaPVV11.PRFAIL.NONE)
						{
							EV.stack("s04_4h", 0, -1, null, null);
						}
						else
						{
							EV.stack("s04_4b", 0, -1, null, null);
						}
						this.changeState(PrimulaPVV11.STATE.NONE);
						return;
					}
				}
				break;
			case PrimulaPVV11.STATE.FINAL_TEST:
				if (this.phase == 0 && (this.Pr.isDamagingOrKo() || this.Pr.isGaraakiState()))
				{
					this.t = X.Mn(this.t, 20f);
				}
				if ((this.fail & PrimulaPVV11.PRFAIL.SLEEP) == PrimulaPVV11.PRFAIL.NONE && this.Pr.Ser.has(SER.SLEEP))
				{
					this.fail |= PrimulaPVV11.PRFAIL.SLEEP;
					this.phase = X.Mn(45, this.phase);
				}
				if (this.phase < 50 && (this.fail & (PrimulaPVV11.PRFAIL)3) != PrimulaPVV11.PRFAIL.NONE)
				{
					if (this.phase < 45)
					{
						this.phase = 50;
						this.t = 0f;
					}
					else
					{
						this.phase = 51;
						this.t = 0f;
						this.Par.SpSetPose("stand_cane", -1, null, false);
						this.Shield.deactivate(false, false);
					}
				}
				if (base.PhaseP(0, 60))
				{
					this.Par.SpSetPose("chant", -1, null, false);
					this.Shield.activate(false);
					base.PtcST("primula_mist_charge", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
				}
				if (base.PhaseP(1, 70))
				{
					base.killPtc(PtcHolder.PTC_HOLD.ACT);
					this.Par.SpSetPose("shot", -1, null, false);
					this.phase = 10;
					this.t = 100f;
				}
				if (this.phase >= 10 && this.phase < 20 && this.t >= 6f)
				{
					this.t = 0f;
					this.initMistShot(this.phase % 10 + 10);
					int num = this.phase + 1;
					this.phase = num;
					if (num >= 14)
					{
						this.phase = 20;
					}
				}
				if (base.PhaseP(20, 20))
				{
					this.Par.SpSetPose("stand_cane", -1, null, false);
					this.t = 200f - DIFF.V(200f, 120f, 60f);
				}
				if (base.PhaseP(21, 200))
				{
					this.Par.SpSetPose("chant", -1, null, false);
					base.PtcST("primula_wave_charge", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
					this.t = 200f - DIFF.V(70f, 63f, 45f);
				}
				if (base.PhaseP(22, 200))
				{
					base.killPtc(PtcHolder.PTC_HOLD.ACT);
					base.PtcST("primula_wave_charge_w1", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
					this.Par.SpSetPose("shot", -1, null, false);
				}
				if (base.PhaseP(23, 30))
				{
					this.initShockWave();
					this.t = 200f - DIFF.V(200f, 120f, 40f);
				}
				if (base.PhaseP(24, 200))
				{
					this.phase = 30;
					this.t = 100f;
				}
				if (this.phase >= 30 && this.phase < 40 && this.t >= 6f)
				{
					this.t = 0f;
					this.initMistShot(this.phase % 10 + 20);
					int num = this.phase + 1;
					this.phase = num;
					if (num >= 34)
					{
						this.phase = 40;
						this.t = 200f - DIFF.V(225f, 140f, 80f);
					}
				}
				if (base.PhaseP(40, 200))
				{
					base.PtcST("primula_wave_charge_w1", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
					this.Par.SpSetPose("shot", -1, null, false);
					if (DIFF.I == 0)
					{
						this.phase = 42;
						this.t = 150f;
					}
					else
					{
						this.t = 200f - DIFF.V(40f, 45f, 30f);
					}
				}
				if (base.PhaseP(41, 200))
				{
					this.initShockWave();
					this.t = 200f - DIFF.V(120f, 140f, 80f);
				}
				if (base.PhaseP(42, 200))
				{
					this.initShockWave();
				}
				if (base.PhaseP(43, 90))
				{
					this.phase = 45;
					this.Par.SpSetPose("stand_cane", -1, null, false);
				}
				if (this.phase == 45)
				{
					if (this.Pr.isBreatheStop(false, false))
					{
						this.t = X.Mn(this.t, 80f);
					}
					if (this.t >= 180f)
					{
						EV.stack("s04_5f", 0, -1, null, null);
						this.changeState(PrimulaPVV11.STATE.NONE);
						return;
					}
				}
				else if (this.phase >= 50)
				{
					if (base.PhaseP(50, 30))
					{
						base.PtcST("primula_magic_abort", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.Par.SpSetPose("stand_cane", -1, null, false);
						this.Shield.deactivate(false, false);
					}
					if (base.PhaseP(51, 170))
					{
						if ((this.fail & PrimulaPVV11.PRFAIL.SLEEP) != PrimulaPVV11.PRFAIL.NONE)
						{
							EV.stack("s04_5b", 0, -1, null, null);
						}
						else
						{
							EV.stack("s04_5b_2", 0, -1, null, null);
						}
						this.changeState(PrimulaPVV11.STATE.NONE);
					}
				}
				break;
			default:
				return;
			}
		}

		public MagicItem initMistShot(int id)
		{
			MagicItem magicItem = base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, (MGHIT)1025).initFunc(this.FD_MgRunMistShot, this.FD_MgDrawMistShot);
			Vector2 targetPos = this.getTargetPos();
			magicItem.sx = targetPos.x;
			magicItem.sy = targetPos.y;
			float num = -3f;
			float num2 = 1.4f;
			float num3 = 0f;
			switch (id)
			{
			case 0:
				num = -3f;
				goto IL_01D0;
			case 1:
				num = -8f;
				num2 = 2.1f;
				goto IL_01D0;
			case 2:
				num = 2.6f;
				num2 = 1.1f;
				goto IL_01D0;
			case 3:
				num = -11f;
				num2 = 2.1f;
				goto IL_01D0;
			case 4:
				num = 6f;
				goto IL_01D0;
			case 5:
				num = -16.1f;
				num2 = 3.1f;
				goto IL_01D0;
			case 10:
				num = -9.6f;
				num2 = 1.1f;
				goto IL_01D0;
			case 11:
				num = -4f;
				num2 = 8.1f;
				num3 = -5.2f;
				goto IL_01D0;
			case 12:
				num = 2.6f;
				num2 = 1.1f;
				goto IL_01D0;
			case 13:
				num = 14f;
				num2 = 8.1f;
				num3 = -5.2f;
				goto IL_01D0;
			}
			if (20 <= id && id < 30)
			{
				float num4;
				if (false == (id == 23))
				{
					num2 = 8.1f;
					num3 = -5.2f;
					num4 = 2f;
				}
				else
				{
					num2 = 1.1f;
					magicItem.phase = 3;
					num4 = 1.4f;
				}
				if (id == 23)
				{
					num = (this.Pr.x - this.Par.x) * num4;
				}
				else
				{
					num = (float)((id - 21) * CAim._XD(this.Par.aim, 1) * 8) * num4;
				}
			}
			IL_01D0:
			Vector4 jumpVelocity = M2Mover.getJumpVelocity(this.Mp, num, num3, num2, 0.32000002f, 0f);
			magicItem.createDropper(jumpVelocity.x, jumpVelocity.y, 0.125f, -1f, -1f).gravity_scale = 0.2f;
			return magicItem;
		}

		private bool MgRunMistShot(MagicItem Mg, float fcnt)
		{
			if (this.Mp == null)
			{
				return false;
			}
			if (Mg.t <= 0f)
			{
				Mg.Ray.projectile_power = 500;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.NONE;
			}
			return fcnt == 0f || NelNMush.MgRunMushShotS(Mg, (this.state == PrimulaPVV11.STATE.MIST) ? this.MkSleep : NelNMush.MkSleep, fcnt, "primula_mist_shot_init", 150, 0.2f, 0.8f, 0f, 8f, -2, 0.007f);
		}

		private bool MgDrawMistShot(MagicItem Mg, float fcnt)
		{
			MeshDrawer mesh = Mg.Ef.GetMesh("", uint.MaxValue, BLEND.NORMAL, false);
			float num = (Mg.Dro.on_ground ? 0f : Mg.sa);
			mesh.Rotate(num, false);
			mesh.Col = mesh.ColGrd.Set(4281862409U).C;
			mesh.Rect(0f, 0f, 34f, 6f, false);
			mesh.Col = mesh.ColGrd.Set(uint.MaxValue).blend(4293598750U, 0.5f + X.COSI(Mg.t, (float)((Mg.da < 50f) ? 40 : 117)) * 0.5f).C;
			mesh.Rect(0f, 0f, 34f, 2f, false);
			mesh.Identity();
			return true;
		}

		public void initShockWave()
		{
			int num = 2;
			for (int i = 0; i < num; i++)
			{
				base.nM2D.MGC.setMagic(this, MGKIND.BASIC_SHOT, (MGHIT)1025).initFunc(new MagicItem.FnMagicRun(this.MgRunShockwave), new MagicItem.FnMagicRun(this.MgDrawShockwave)).sa = ((i == 0) ? 0f : 3.1415927f);
			}
		}

		private bool MgRunShockwave(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				if (fcnt == 0f)
				{
					return true;
				}
				Mg.phase = 1;
				Mg.efpos_s = true;
				Vector2 targetPos = this.getTargetPos();
				Mg.sx = targetPos.x;
				Mg.sy = targetPos.y;
				Mg.PtcST("primula_wave_shot", PTCThread.StFollow.NO_FOLLOW, false);
			}
			if (Mg.phase == 1)
			{
				float num = (float)X.MPF(Mg.sa == 0f) * X.NI(0.018f, 0.233f, X.ZCOS(Mg.t - 12f, 55f));
				int num2 = (int)Mg.sx;
				Mg.sx += num * fcnt;
				float num3 = 1.4f * X.ZSIN2(Mg.t, 20f) * (float)X.MPF(Mg.sa == 0f);
				if (Mg.sz == 0f && !this.Pr.isNoDamageActive() && this.Pr.isCovering(Mg.sx + num3 - 0.125f, Mg.sx + num3 + 0.125f, Mg.sy - 9.8f, Mg.sy + 9.8f, -0.3f))
				{
					bool flag = this.Pr.Skill.canShildGuard();
					this.Pr.applyDamage(this.AtkShockWave.BurstDir((float)X.MPF(Mg.sa == 0f)), true);
					this.fail |= PrimulaPVV11.PRFAIL.HITTED;
					if (flag && !this.Pr.Skill.canShildGuard())
					{
						this.fail |= PrimulaPVV11.PRFAIL.GUARD_SUCCESS;
					}
					else
					{
						this.fail &= (PrimulaPVV11.PRFAIL)(-5);
					}
					if (this.Pr.isDamagingOrKo())
					{
						Mg.sz = 1f;
					}
				}
				if (num2 != (int)Mg.sx && !this.Mp.canStand((int)Mg.sx, (int)Mg.sy))
				{
					Mg.phase = 2;
					Mg.t = 0f;
				}
			}
			else if (Mg.phase == 2)
			{
				float num4 = (float)X.MPF(Mg.sa == 0f) * 0.18f;
				Mg.sx += num4 * fcnt;
				return Mg.t < 20f;
			}
			return true;
		}

		private unsafe bool MgDrawShockwave(MagicItem Mg, float fcnt)
		{
			EffectItem ef = Mg.Ef;
			MeshDrawer mesh = ef.GetMesh("", MTR.MtrFireWallAdd, false);
			if (mesh.getTriMax() == 0)
			{
				mesh.base_z += 0.1f;
			}
			float num = X.ZSIN(Mg.t, 22f);
			mesh.ColGrd.Set(4288586239U);
			float num2 = Mg.t;
			if (Mg.phase == 2)
			{
				mesh.ColGrd.mulA(1f - num);
				num = 1f;
				num2 = 200f;
			}
			mesh.Col = mesh.ColGrd.C;
			mesh.ColGrd.mulA((1f - num) * 0.875f);
			int vertexMax = mesh.getVertexMax();
			mesh.Scale(0.33f * (float)X.MPF(Mg.sa == 0f), 1.25f * X.NI(1f, 7f, num), false);
			float num3 = 1.4f * this.Mp.CLENB * (1f + X.ZSIN(num2, 54f) * 2.25f);
			mesh.allocUv2(20, false);
			mesh.Arc2(0f, 0f, num3, num3, -1.1780972f, 1.1780972f, num3 * 0.9f, 0f, 1f);
			mesh.Uv2(70f, 5f, false);
			mesh.allocUv2(0, true);
			int num4 = mesh.getVertexMax() - vertexMax;
			float num5 = (float)(-(float)X.MPF(Mg.sa == 0f)) * (X.ZSIN(num2, 50f) * 95f) * 0.015625f;
			Vector2 vector = Vector2.zero;
			Vector2 vector2 = Vector2.zero;
			Color32[] colorArray = mesh.getColorArray();
			Vector3[] vertexArray = mesh.getVertexArray();
			fixed (Vector3* ptr = &vertexArray[vertexMax])
			{
				Vector3* ptr2 = ptr;
				vector = *ptr2;
				vector2 = vertexArray[vertexMax + (int)((float)num4 * 0.15f) * 2];
				fixed (Color32* ptr3 = &colorArray[vertexMax])
				{
					Color32* ptr4 = ptr3;
					Vector3* ptr5 = ptr2 + 1;
					for (int i = 0; i < num4; i += 2)
					{
						float num6 = (float)i / (float)(num4 - 2);
						float num7 = X.ZSIN(0.5f - X.Abs(num6 - 0.5f), 0.5f);
						ptr4->a = (byte)((float)ptr4->a * num7);
						ptr4++;
						ptr4->a = (byte)((float)ptr4->a * num7);
						ptr4++;
						ptr5->x += num5 * num7;
						ptr5 += 2;
					}
				}
			}
			if (Mg.phase == 1)
			{
				MeshDrawer mesh2 = ef.GetMesh("", MTRX.MtrMeshAdd, false);
				mesh2.base_x = 0f;
				mesh2.base_y = 0f;
				float num8 = 0.75f + 0.125f * X.COSIT(17.3f) + 0.125f * X.COSIT(8.23f);
				ElecDrawer elec = MTRX.Elec;
				uint ran = X.GETRAN2((int)(this.Mp.floort * 0.25f) + 33, (Mg.id & 7) + 3);
				elec.Ran(ran).BallRadius(0f, 0f).Thick(4f, -1000f)
					.JumpRatio(0.7f)
					.JumpHeight(6f, 10f)
					.DivideWidth(60f);
				elec.finePos(vector.x * 64f, vector.y * 64f, vector2.x * 64f, vector2.y * 64f).drawTz(mesh2, num8, true);
				elec.finePos(vector.x * 64f, -vector.y * 64f, vector2.x * 64f, -vector2.y * 64f).drawTz(mesh2, num8, true);
				elec.DivideWidth(63f);
				elec.finePos(vector2.x * 64f, vector2.y * 64f, vector2.x * 64f, -vector2.y * 64f).drawTz(mesh2, num8, true);
			}
			return true;
		}

		protected override Vector2 calcCanePos(PxlFrame F)
		{
			float num = 0f;
			float num2 = 0f;
			M2PxlAnimator.getRodPosS(this.Mp.rCLENB, F, ref num, ref num2, "cane", "PLT", 47f, 13f, ALIGN.LEFT, ALIGNY.TOP, 0);
			return new Vector2(num, num2);
		}

		public override string get_parent_name()
		{
			return "primula";
		}

		private PrimulaPVV11.STATE state;

		private PrimulaPVV11.PRFAIL fail;

		private float failt;

		private M2Shield Shield;

		private NelAttackInfo AtkShockWave = new NelAttackInfo
		{
			hpdmg0 = 0,
			split_mpdmg = 35,
			huttobi_ratio = 100f,
			shield_break_ratio = -9999f,
			burst_vx = 0.19f,
			burst_vy = -0.1f
		};

		private MagicItem.FnMagicRun FD_MgRunMistShot;

		private MagicItem.FnMagicRun FD_MgDrawMistShot;

		private MistManager.MistKind MkSleep;

		private const float mist_gravity_scale = 0.2f;

		private const float shockwave_cir_r = 1.4f;

		private const float shockwave_high_ratio = 7f;

		private enum STATE
		{
			_OFFLINE = -1,
			NONE,
			MIST,
			EVADE_TEST,
			EVADE_WAVE,
			FINAL_TEST
		}

		private enum PRFAIL
		{
			NONE,
			SLEEP,
			HITTED,
			GUARD_SUCCESS = 4,
			EVADE_SUCCESS = 8
		}
	}
}
