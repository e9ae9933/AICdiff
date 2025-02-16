using System;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NelTreasureBoxDrawer : TreasureBoxDrawer, IRunAndDestroy, IEventWaitListener
	{
		public static void initAnimT()
		{
		}

		public NelTreasureBoxDrawer(M2LpPuzzTreasure _Lp, float _w = -1f)
			: base(_w)
		{
			this.M2D = M2DBase.Instance as NelM2DBase;
			this.Mp = _Lp.Mp;
			this.Lp = _Lp;
			if (_w < 0f)
			{
				base.Set(40f, 18f);
			}
		}

		public NelTreasureBoxDrawer Set(NelTreasureBoxDrawer.BOXTYPE _boxtype, bool force = false)
		{
			if (this.isDisappear() || (!force && this.boxtype_ == _boxtype))
			{
				return this;
			}
			this.boxtype_ = _boxtype;
			this.opent = 70f;
			this.open_walkt = 20f;
			this.open_fadet = 60f;
			if (!this.is_image)
			{
				switch (this.boxtype_)
				{
				case NelTreasureBoxDrawer.BOXTYPE.DEACTIVATE:
					this.ColT.Set(uint.MaxValue);
					break;
				case NelTreasureBoxDrawer.BOXTYPE.M2D_NORMAL:
					this.ColT.Set(4288277503U);
					this.ColB.Set(4281975759U);
					this.ColLine.Set(4278202345U);
					this.ColIcon.Set(4278736291U);
					this.CenterIcon = MTR.ANelChars[9];
					break;
				case NelTreasureBoxDrawer.BOXTYPE.M2D_FLUSH:
					this.ColT.Set(4293651435U);
					this.ColB.Set(4287406501U);
					this.ColLine.Set(4286742396U);
					this.ColIcon.Set(4283256402U);
					this.CenterIcon = MTR.ANelChars[4];
					break;
				case NelTreasureBoxDrawer.BOXTYPE.M2D_MONEY:
					this.ColT.Set(4293918581U);
					this.ColB.Set(4291605825U);
					this.ColLine.Set(4278202345U);
					this.ColIcon.Set(4278736291U);
					this.CenterIcon = MTR.ANelChars[15];
					break;
				case NelTreasureBoxDrawer.BOXTYPE.M2D_MONEY_FLUSH:
					this.ColT.Set(4294178780U);
					this.ColB.Set(4291153017U);
					this.ColLine.Set(4278202345U);
					this.ColIcon.Set(4278736291U);
					this.CenterIcon = MTR.ANelChars[15];
					break;
				case NelTreasureBoxDrawer.BOXTYPE.M2D_SKILL:
					this.ColT.Set(4283299675U);
					this.ColB.Set(4278207387U);
					this.ColLine.Set(4287484415U);
					this.ColIcon.Set(4285483605U);
					this.CenterIcon = MTR.ANelChars[11];
					break;
				case NelTreasureBoxDrawer.BOXTYPE.M2D_ENHANCER:
					this.ColT.Set(4288192204U);
					this.ColB.Set(4278195107U);
					this.ColLine.Set(4294962266U);
					this.ColIcon.Set(4278916696U);
					this.CenterIcon = MTR.ANelChars[10];
					break;
				case NelTreasureBoxDrawer.BOXTYPE.M2D_SKILL_HP:
					this.ColT.Set(4293170102U);
					this.ColB.Set(4293388263U);
					this.ColLine.Set(4278202345U);
					this.ColIcon.Set(4278736291U);
					this.CenterIcon = MTR.ANelChars[12];
					break;
				case NelTreasureBoxDrawer.BOXTYPE.M2D_SKILL_MP:
					this.ColT.Set(4291556076U);
					this.ColB.Set(4288992204U);
					this.ColLine.Set(4278202345U);
					this.ColIcon.Set(4278736291U);
					this.CenterIcon = MTR.ANelChars[13];
					break;
				case NelTreasureBoxDrawer.BOXTYPE.FORBIDDEN:
					this.ColT.Set(4282992969U);
					this.ColB.Set(4288256409U);
					this.ColLine.Set(4292031283U);
					this.ColIcon.Set(uint.MaxValue);
					this.CenterIcon = MTR.ANelChars[14];
					break;
				}
			}
			else
			{
				this.ColIcon.White();
				NelTreasureBoxDrawer.BOXTYPE boxtype = this.boxtype_;
				if (boxtype == NelTreasureBoxDrawer.BOXTYPE.DEACTIVATE_MAGIC || boxtype == NelTreasureBoxDrawer.BOXTYPE.M2D_MAGIC)
				{
					this.ColT.Set(uint.MaxValue);
					this.ColB.Set(uint.MaxValue);
					this.opent = 903f;
					this.open_walkt = 80f;
					this.open_fadet = 95f;
					this.CenterIcon = MTRX.getPF(this.is_active_type ? "magic_device" : "magic_device_offline");
				}
			}
			if (this.getted)
			{
				this.ColLine.mulA(0.2f);
				this.ColT.blend(3148326052U, 0.7f);
				this.ColB.blend(3143523934U, 0.7f);
				this.ColIcon.blend(3148326052U, 0.7f);
			}
			this.ColT0.Set(this.ColT);
			this.ColB0.Set(this.ColB);
			this.ColIcon0.Set(this.ColIcon);
			return this;
		}

		public void initBright()
		{
			this.bright_f0 = this.Mp.floort;
		}

		public float getOpeningLevel()
		{
			if (!this.getted && this.state > NelTreasureBoxDrawer.STATE.CLOSE)
			{
				float num = (float)(DateTime.Now - this.opening_time0).TotalSeconds * 60f;
				if (this.PE != null)
				{
					if (this.state >= NelTreasureBoxDrawer.STATE.DISAPPEARING)
					{
						NelTreasureBoxDrawer.box_level = 1.0001f;
					}
					else
					{
						if (num >= this.open_walkt)
						{
							num -= this.open_walkt;
							NelTreasureBoxDrawer.box_level = X.ZLINE(num, this.open_fadet);
						}
						else
						{
							NelTreasureBoxDrawer.box_level = 0f;
						}
						if (this.LevelUp(NelTreasureBoxDrawer.box_level) && NelTreasureBoxDrawer.box_level != 1f)
						{
							this.levelup_time0 = DateTime.Now;
							if (NelTreasureBoxDrawer.box_level < 1f && this.isReleased() && !this.is_magic)
							{
								this.M2D.Cam.TeCon.setBounceZoomIn2(1.0625f, 11f, 30f, 0);
							}
						}
					}
				}
				else
				{
					NelTreasureBoxDrawer.box_level = 1f + X.ZLINE(num + 1f, 61f);
					if (this.LevelUp(NelTreasureBoxDrawer.box_level))
					{
						this.levelup_time0 = DateTime.Now;
					}
					if (NelTreasureBoxDrawer.box_level >= 2f)
					{
						return 2f;
					}
				}
			}
			else
			{
				NelTreasureBoxDrawer.box_level = 0f;
			}
			return NelTreasureBoxDrawer.box_level;
		}

		private bool LevelUp(float _level)
		{
			bool flag = false;
			if (this.level < _level)
			{
				this.level = _level;
				NelTreasureBoxDrawer.STATE state = this.state;
				if (this.level >= 2f)
				{
					state = NelTreasureBoxDrawer.STATE.DISAPPEAR;
				}
				else if (this.level > 1f)
				{
					state = NelTreasureBoxDrawer.STATE.DISAPPEARING;
				}
				else if (this.level == 1f)
				{
					state = NelTreasureBoxDrawer.STATE.FULLOPEN;
				}
				else if (this.level > 0.55f)
				{
					state = NelTreasureBoxDrawer.STATE.RELEASE;
				}
				else if (this.level > 0f)
				{
					state = NelTreasureBoxDrawer.STATE.OPENING;
				}
				if (this.state < state)
				{
					flag = true;
					this.state = state;
					if (this.is_magic)
					{
						PRNoel prNoel = this.M2D.getPrNoel();
						switch (this.state)
						{
						case NelTreasureBoxDrawer.STATE.OPENING:
						{
							float num = 713f - (float)(DateTime.Now - this.opening_time0).TotalSeconds * 60f;
							this.levelup_time0 = DateTime.Now;
							this.Spz = new EfSpellRegisteration(this.Lp, this.Lp.mgkind, 20);
							this.efy = this.Lp.mapfocy;
							this.M2D.PE.setPEabsorbed(POSTM.MAGIC_DEVICE_ACTIVATE, 4f, 90f, 1f, 0);
							this.PE.Set(this.M2D.PE.setPEbounce(POSTM.POST_BLOOM, num - 90f, 0.25f, 90));
							this.PE.Set(this.M2D.PE.setPEbounce(POSTM.POST_BLOOM, 40f, 0.75f, (int)num - 60));
							this.PE.Set(this.M2D.PE.setPEabsorbed(POSTM.MAGIC_DEVICE_ACTIVATE, num - 40f, 60f, 0.125f, 40));
							this.PE.Set(this.M2D.PE.setPEbounce(POSTM.MAGIC_DEVICE_ACTIVATE, 60f, 0.87f, (int)num - 60));
							this.PE.Set(this.M2D.PE.setPEfadeinout(POSTM.ZOOM2, num - 220f, -220f, 0.33f, 0));
							this.EfSt = this.M2D.getPrNoel().PtcVar("lx", (double)this.Lp.mapfocx).PtcVar("ly", (double)this.Lp.mapfocy)
								.PtcVar("maxt", (double)num)
								.PtcST("magic_device_opening", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							SND.Ui.play("magicdevice_1", false);
							break;
						}
						case NelTreasureBoxDrawer.STATE.FULLOPEN:
							SND.Ui.play("magicdevice_2_1", false);
							break;
						case NelTreasureBoxDrawer.STATE.DISAPPEARING:
							if (this.EfSt != null)
							{
								this.EfSt.kill(false);
							}
							SND.Ui.Stop();
							this.M2D.Cam.assignBaseMover(this.M2D.getPrNoel(), -1);
							this.levelup_time0 = DateTime.Now;
							this.PE.deactivate(true);
							if (this.Spz != null)
							{
								this.Spz = this.Spz.destruct();
							}
							this.PE.Set(this.M2D.PE.setPEabsorbed(POSTM.ZOOM2, 55f, 0.3f, 0));
							this.PE.Set(this.M2D.PE.setPEabsorbed(POSTM.MAGIC_DEVICE_ACTIVATE, 14f, 90f, 0.5f, -14));
							this.PE.Set(this.M2D.PE.setPEbounce(POSTM.POST_BLOOM, 188f, 1f, -173));
							this.EfSt = prNoel.PtcVar("lx", (double)this.Lp.mapfocx).PtcVar("ly", (double)this.Lp.mapfocy).PtcST("magic_device_closing", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_T);
							SND.Ui.play("magicdevice_3_0", false);
							NEL.PadVib("getmagic_fullopen", 1f);
							break;
						}
					}
					else
					{
						NelTreasureBoxDrawer.STATE state2 = this.state;
						if (state2 != NelTreasureBoxDrawer.STATE.OPENING)
						{
							if (state2 == NelTreasureBoxDrawer.STATE.RELEASE)
							{
								M2DBase.playSnd(this.sound_release);
							}
						}
						else
						{
							M2DBase.playSnd(this.sound_opening);
						}
					}
				}
			}
			return flag;
		}

		public TreasureBoxDrawer drawOnEffect(EffectItem Ef, float scale = 1f)
		{
			MeshDrawer meshDrawer = null;
			MeshDrawer meshDrawer2 = null;
			MeshDrawer meshDrawer3 = null;
			MeshDrawer meshDrawer4 = null;
			float num = 0f;
			float num2 = 7.84f * X.COSIT(160f);
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			if (this.bright_f0 >= 0f)
			{
				num5 = 1f - X.ZLINE(this.Mp.floort - this.bright_f0, 30f);
				if (num5 <= 0f)
				{
					this.ColT.Set(this.ColT0);
					this.ColB.Set(this.ColB0);
					this.ColIcon.Set(this.ColIcon0);
					this.bright_f0 = -1f;
				}
				else
				{
					uint num6 = (this.getted ? 3154116607U : uint.MaxValue);
					this.ColT.Set(this.ColT0).blend(num6, num5);
					this.ColB.Set(this.ColB0).blend(num6, num5);
					this.ColIcon.Set(this.ColIcon0).blend(num6, num5);
				}
			}
			if (!this.is_magic)
			{
				scale *= 2f;
				this.getMd(ref meshDrawer2, Ef, BLEND.SUB, 0.012f, uint.MaxValue, false);
				this.getMd(ref meshDrawer, Ef, BLEND.NORMAL, 0.011f, uint.MaxValue, false);
				if (this.boxtype_ == NelTreasureBoxDrawer.BOXTYPE.DEACTIVATE)
				{
					float num7 = X.ANMPT(77, 1f);
					meshDrawer.Col = this.ColT.C;
					meshDrawer2.Col = this.ColLine.C;
					float num8 = this.w * scale;
					float num9 = this.center_w * scale;
					float num10 = num * 0.015625f;
					float num11 = num2 * 0.015625f;
					meshDrawer.RectDashed(num10, num11, num8, num8, num7, 20, TreasureBoxDrawer.px, true, true);
					meshDrawer2.RectDashed(num10, num11, num8, num8, num7, 20, TreasureBoxDrawer.px + TreasureBoxDrawer.px * 2f, true, true);
					meshDrawer.RectDashed(num10, num11, num9, num9, 1f - num7, 6, TreasureBoxDrawer.px, true, true);
					meshDrawer2.RectDashed(num10, num11, num9, num9, 1f - num7, 6, TreasureBoxDrawer.px + TreasureBoxDrawer.px * 2f, true, true);
				}
				else
				{
					this.getMd(ref meshDrawer4, Ef, BLEND.NORMAL, MTRX.MIicon, 0.01f, uint.MaxValue, false);
					meshDrawer.TranslateP(num, num2, false);
					Bench.P("draw-treasurebox");
					base.drawTo(meshDrawer, meshDrawer2, meshDrawer4, 0f, 0f, scale);
					Bench.Pend("draw-treasurebox");
				}
				if (NelTreasureBoxDrawer.box_level > 0f)
				{
					MeshDrawer mesh = Ef.GetMesh("", uint.MaxValue, BLEND.ADD, true);
					float num12 = (float)(DateTime.Now - this.levelup_time0).TotalSeconds * 60f;
					float num13 = 1f;
					float num14 = 1f;
					float num15;
					if (this.isDisappear())
					{
						num15 = X.NI(2.5f, 0.6f, X.ZSIN(num12, 20f));
						num14 = 1f - X.ZLINE(NelTreasureBoxDrawer.box_level - 1f);
					}
					else if (this.isReleased())
					{
						num15 = 1f + 0.5f * X.ZSINV(num12, 13f) + X.ZSIN(num12 - 13f, 20f) * 3.5f - 2.5f * X.ZCOS(num12 - 13f - 18f, 30f);
						num13 = 1.5f + X.ZSIN2(num12, 14f) * 0.5f - 1f * X.ZCOS(num12 - 18f, 60f);
					}
					else
					{
						num13 = 0.4f + 0.4f * X.ZPOW3(num12, 30f);
						num15 = 0.3f + X.ZSINV(num12, 22f) * 0.7f;
					}
					UniBrightDrawer uniBrightDrawer = MTRX.UniBright.Count(13).RotTime(180f, 340f).Col(C32.MulA(4293263288U, num14), C32.MulA(7180082U, num14));
					uniBrightDrawer.Radius(70f * num15, 100f * num15).CenterCicle(30f * num15, 40f * num15, 160f).Thick(12f * num13 * num15, 30f * num13 * num15);
					uniBrightDrawer.rot_anti_ratio = 0f;
					uniBrightDrawer.drawTo(mesh, 0f, 0f, this.Mp.floort, true);
					uniBrightDrawer.Count(6).Radius(30f * num15, 35f * num15).CenterCicle(0f, -1f, 160f)
						.Thick(10f * num13 * num15, 24f * num13 * num15);
					uniBrightDrawer.rot_anti_ratio = 1f;
					uniBrightDrawer.drawTo(mesh, 0f, 0f, this.Mp.floort, true);
					num3 = X.ZLINE(NelTreasureBoxDrawer.box_level - 0.33f, 0.66999996f) - X.ZLINE(NelTreasureBoxDrawer.box_level - 1.12f, 0.5f);
				}
			}
			else
			{
				num2 -= 15f;
				if (this.state < NelTreasureBoxDrawer.STATE.DISAPPEARING)
				{
					float num16 = 0f;
					if (this.state == NelTreasureBoxDrawer.STATE.FULLOPEN || this.state == NelTreasureBoxDrawer.STATE.OPENING)
					{
						num16 = (float)(DateTime.Now - this.levelup_time0).TotalSeconds * 60f + ((this.state == NelTreasureBoxDrawer.STATE.FULLOPEN) ? this.open_fadet : 0f);
						float num17 = X.ZSIN(num16, 260f);
						num2 = X.NI(num2, 20f, num17);
						Ef.y -= num2 / this.Mp.CLENB;
						this.efy = Ef.y;
						if (this.Spz != null)
						{
							this.Spz.efy = Ef.y;
						}
						num2 = 0f;
						if (this.state == NelTreasureBoxDrawer.STATE.FULLOPEN && num16 >= 150f)
						{
							NEL.PadVib("getmagic_charge", X.NI(0.125f, 1f, X.ZSIN(num16 - 150f, 300f)));
						}
					}
					this.getMd(ref meshDrawer2, Ef, BLEND.SUB, MTRX.MIicon, 0.042f, this.ColB.rgba, true);
					this.getMd(ref meshDrawer3, Ef, BLEND.ADD, MTRX.MIicon, 0.0415f, this.ColT.rgba, true);
					this.getMd(ref meshDrawer, Ef, BLEND.NORMAL, 0.04f, uint.MaxValue, false);
					if (this.state == NelTreasureBoxDrawer.STATE.FULLOPEN || this.state == NelTreasureBoxDrawer.STATE.OPENING)
					{
						num3 = 0.5f + 0.5f * X.ZLINE(num16, 100f);
						num4 = 0.75f * (0.125f + 0.875f * X.ZLINE(num16, 100f));
						float num18 = (float)(DateTime.Now - this.opening_time0).TotalSeconds * 60f;
						num5 = 1f - X.ZLINE(num16, 30f) + X.ZLINE(num18 - 100f, 180f);
						if (num18 >= 384f && this.Spz != null)
						{
							if (this.circulation_time0 == DateTime.MinValue)
							{
								this.circulation_time0 = DateTime.Now;
								SND.Ui.play("magicdevice_2_2", false);
							}
							else if ((float)(DateTime.Now - this.circulation_time0).TotalSeconds * 60f >= 15f)
							{
								this.circulation_time0 = DateTime.Now;
								this.Spz.circulation_level = X.ZLINE(num18 - 384f, 329f);
								this.Spz.remakeCirculation();
								if (num16 >= 200f)
								{
									NEL.PadVib("getmagic_charge_hit", X.NI(0.125f, 1f, X.ZPOW(num16 - 200f, 290f)));
								}
							}
						}
						if (this.Cane == null)
						{
							PRNoel prNoel = this.M2D.getPrNoel();
							this.Cane = prNoel.getFloatCane();
							if (this.Cane != null)
							{
								float num19 = 713f - num18;
								this.Phy = this.Cane.getPhysic();
								this.Cane.transform.localEulerAngles = new Vector3(0f, 0f, prNoel.is_right ? 0f : 3.1415927f);
								this.M2D.Cam.assignBaseMover(this.Cane, -1);
								this.Phy.addLockWallHitting(this, num19).addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, num19).addLockGravity(this, 0f, num19);
							}
						}
						else if (num16 >= 480f)
						{
							this.M2D.getPrNoel().SpSetPose("get_magic4", -1, "get_magic4", false);
						}
						if (num18 >= 713f)
						{
							this.LevelUp(1.0001f);
						}
					}
					if (this.CenterIcon != null)
					{
						this.getMd(ref meshDrawer4, Ef, BLEND.NORMALP3, MTRX.MIicon, 0.011f, uint.MaxValue, false);
						meshDrawer4.allocUv23(12, false);
						meshDrawer4.Uv23(meshDrawer4.ColGrd.Transparent().blend(16777215U, num5).C, false);
						meshDrawer4.RotaPF(num, num2, 1f, 1f, 0f, this.CenterIcon, false, false, false, uint.MaxValue, false, 0);
						float num20 = (float)X.ANMT(4, 4f) * 1.5707964f;
						float num21 = 3f + 0.24f * X.COSIT(230f);
						meshDrawer2.RotaPF(num, num2, num21, num21, -num20, MTRX.AEff[4], false, false, false, uint.MaxValue, false, 0);
						meshDrawer3.RotaPF(num, num2, num21, num21, num20, MTRX.AEff[7], false, false, false, uint.MaxValue, false, 0);
						meshDrawer4.allocUv23(0, true);
					}
				}
				else
				{
					float num22 = (float)(DateTime.Now - this.levelup_time0).TotalSeconds * 60f;
					num3 = 1f - X.ZLINE(num22, 50f);
					num5 = 1f - X.ZSIN(num22, 130f);
					num4 = 0.75f * (1f - X.ZLINE(num22, 50f));
				}
			}
			if (this.getted)
			{
				float px = TreasureBoxDrawer.px;
				for (int i = 0; i < 2; i++)
				{
					meshDrawer.Col = ((i == 0) ? C32.d2c(3426104886U) : C32.d2c(2868903935U));
					meshDrawer.CheckMark(0f, 0f - ((i == 0) ? (TreasureBoxDrawer.px * 2f * scale) : 0f), this.w * 1.2f * scale, TreasureBoxDrawer.px * 4f * scale, true);
				}
			}
			if (num3 > 0f)
			{
				MeshDrawer mesh2 = Ef.GetMesh("box", uint.MaxValue, BLEND.MUL, true);
				mesh2.Col = mesh2.ColGrd.Set(4289967027U).mulA(num3).C;
				mesh2.ColGrd.setA(num4);
				this.M2D.drawCamCenterDoughnut(mesh2, this.Lp.mapfocx, this.Lp.mapfocy, X.NI(0.6f, 0.33f, num3), X.NI(0.6f, 0.45f, num3));
			}
			if (this.PE != null)
			{
				this.PE.fine(100);
			}
			return this;
		}

		public float kira_open_effect_af
		{
			get
			{
				if (this.PE == null || this.state == NelTreasureBoxDrawer.STATE.CLOSE)
				{
					return -1f;
				}
				if (this.is_magic)
				{
					return (float)(DateTime.Now - this.opening_time0).TotalSeconds * 60f - this.opent;
				}
				return (float)(DateTime.Now - this.opening_time0).TotalSeconds * 60f - 40f;
			}
		}

		private MeshDrawer getMd(ref MeshDrawer _Md, EffectItem Ef, BLEND blnd, float z, uint col = 4294967295U, bool bottom_flag = false)
		{
			if (_Md == null)
			{
				_Md = Ef.GetMesh("box", col, blnd, bottom_flag);
				_Md.base_z += z;
			}
			return _Md;
		}

		private MeshDrawer getMd(ref MeshDrawer _Md, EffectItem Ef, BLEND blnd, MImage MI, float z, uint col = 4294967295U, bool bottom_flag = false)
		{
			if (_Md == null)
			{
				_Md = Ef.GetMeshImg("box", MI, blnd, bottom_flag);
				_Md.base_z += z;
			}
			_Md.Col = C32.d2c(col);
			return _Md;
		}

		public bool getted
		{
			get
			{
				return this.getted_;
			}
			set
			{
				this.getted_ = value;
				this.Set(this.boxtype_, true);
			}
		}

		public bool is_magic
		{
			get
			{
				return this.boxtype_ == NelTreasureBoxDrawer.BOXTYPE.M2D_MAGIC || this.boxtype_ == NelTreasureBoxDrawer.BOXTYPE.DEACTIVATE_MAGIC;
			}
		}

		public bool is_image
		{
			get
			{
				return this.boxtype_ == NelTreasureBoxDrawer.BOXTYPE.M2D_MAGIC || this.boxtype_ == NelTreasureBoxDrawer.BOXTYPE.DEACTIVATE_MAGIC;
			}
		}

		public bool is_opening
		{
			get
			{
				return this.PE != null;
			}
		}

		public bool is_active_type
		{
			get
			{
				return this.boxtype_ != NelTreasureBoxDrawer.BOXTYPE.DEACTIVATE_MAGIC && this.boxtype_ > NelTreasureBoxDrawer.BOXTYPE.DEACTIVATE;
			}
		}

		public bool isDisappear()
		{
			return this.state >= NelTreasureBoxDrawer.STATE.DISAPPEARING;
		}

		public bool isReleased()
		{
			return this.state >= NelTreasureBoxDrawer.STATE.RELEASE;
		}

		public static string getEventMoveScript(float drawx, bool is_magic, bool wait_cmd)
		{
			int num = (int)(is_magic ? 903f : 70f);
			int num2 = (int)(is_magic ? 55f : 50f);
			string text = "STOP_LETTERBOX\n";
			string text2;
			string text3;
			if (is_magic)
			{
				text = "DENY_SKIP\n";
				text2 = "P[get_magic~] ";
				text3 = "WAIT_FN TREASURE_BOX 1700\n";
				text3 += "#MS_ % '= P[get_magic5~]' \n";
				text3 = text3 + "WAIT " + 190f.ToString() + " \n";
			}
			else
			{
				text += "DENY_SKIP\n";
				text2 = "P[get_item~]";
				text3 = "WAIT " + num.ToString();
			}
			text = string.Concat(new string[]
			{
				text,
				"#MS_ % '>[ ",
				((int)drawx).ToString(),
				",+=0 :",
				num2.ToString(),
				" ] ",
				text2,
				"' \n"
			});
			if (wait_cmd)
			{
				text = text + text3 + " \n";
			}
			return text;
		}

		public void initOpening()
		{
			if (this.is_opening)
			{
				this.quitOpening();
			}
			EV.addWaitListener("TREASURE_BOX", this);
			this.opening_time0 = DateTime.Now;
			this.state = NelTreasureBoxDrawer.STATE.OPENING_PREPARE;
			this.efy = this.Lp.mapfocy;
			this.PE = new EffectHandlerPE(2);
			if (!this.is_magic)
			{
				this.PE.Set(this.M2D.PE.setPEfadeinoutZSINV(POSTM.ZOOM2, 40f, -1f, 0.25f, 0));
				return;
			}
			this.Mp.addRunnerObject(this);
		}

		public bool EvtWait(bool is_first = false)
		{
			return this.PE != null && this.state < NelTreasureBoxDrawer.STATE.DISAPPEARING;
		}

		public void quitOpening()
		{
			EV.remWaitListener(this);
			this.Mp.remRunnerObject(this);
			if (this.PE == null)
			{
				return;
			}
			if (this.state > NelTreasureBoxDrawer.STATE.CLOSE && this.state < NelTreasureBoxDrawer.STATE.FULLOPEN)
			{
				this.state = NelTreasureBoxDrawer.STATE.DISAPPEARING;
				this.levelup_time0 = DateTime.Now;
			}
			if (this.Spz != null)
			{
				this.Spz = this.Spz.destruct();
			}
			this.opening_time0 = DateTime.Now;
			this.circulation_time0 = DateTime.MinValue;
			M2DBase.playSnd("itembox_disappear");
			this.releaseCane();
			this.PE.deactivate(true);
			this.PE = null;
			if (this.EfSt != null)
			{
				this.EfSt.kill(false);
			}
			this.EfSt = null;
		}

		private void releaseCane()
		{
			if (this.Cane == null)
			{
				return;
			}
			this.Phy.clearLock();
			this.circulation_time0 = DateTime.MinValue;
			this.Phy.addLockWallHitting(this, 5f).addLockMoverHitting(HITLOCK.SPECIAL_ATTACK, 120f);
			this.Cane = null;
			this.Phy = null;
			this.M2D.Cam.assignBaseMover(this.M2D.getPrNoel(), -1);
		}

		public bool run(float fcnt)
		{
			bool flag = true;
			if (this.PE == null)
			{
				flag = false;
			}
			else if (this.Cane != null)
			{
				PRNoel prNoel = this.M2D.getPrNoel();
				if (prNoel.getFloatCane() == null)
				{
					this.releaseCane();
				}
				else if (this.state < NelTreasureBoxDrawer.STATE.DISAPPEARING)
				{
					float appear_t = this.Cane.appear_t;
					float num = prNoel.mpf_is_right * -1f;
					if (appear_t < 80f)
					{
						this.Phy.angleSpeedR = -(1f - X.ZLINE(appear_t, 80f)) * num * 0.08f * 3.1415927f;
					}
					else
					{
						this.Phy.angleSpeedR = X.angledif(this.Cane.transform.localEulerAngles.z / 360f, -0.25f) * 6.2831855f * (0.5f + 0.5f * X.ZLINE(appear_t - 80f, 50f)) * 0.022f;
					}
					float num2 = X.NI3(1.5f, 1f, 0.55f, X.ZSINV(appear_t, 70f)) * 0.034f;
					this.Phy.addFric(5f).addFocXy(FOCTYPE.SPECIAL_ATTACK | FOCTYPE._INDIVIDUAL, (this.Lp.mapfocx - this.Cane.x) * num2, (this.efy + 0.3f - this.Cane.y) * num2, -1f, -1, 1, 0, -1, 0);
				}
				else
				{
					float num3 = (float)(DateTime.Now - this.levelup_time0).TotalSeconds * 60f;
					this.Phy.angleSpeedR = X.angledif(this.Cane.transform.localEulerAngles.z / 360f, prNoel.is_right ? 0.61f : (-0.11f)) * 6.2831855f * 0.03f;
					float num4 = (1f - X.ZSINV(num3, 50f) * 0.6f) * 0.034f;
					this.Phy.addFocXy(FOCTYPE.SPECIAL_ATTACK, (prNoel.x - this.Cane.x) * num4, (prNoel.y - 1.4f - this.Cane.y) * num4, -1f, -1, 1, 0, -1, 0);
				}
			}
			return flag;
		}

		public void destruct()
		{
			this.quitOpening();
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				stb += "<NelTreasureBox> ";
				stb += this.Lp.ToString();
				this._tostring = stb.ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		private readonly NelM2DBase M2D;

		private readonly Map2d Mp;

		private readonly M2LpPuzzTreasure Lp;

		private C32 ColT0 = new C32();

		private C32 ColB0 = new C32();

		private C32 ColIcon0 = new C32();

		private NelTreasureBoxDrawer.BOXTYPE boxtype_;

		private NelTreasureBoxDrawer.STATE state;

		private PTCThread EfSt;

		private bool getted_;

		private float bright_f0 = -1f;

		private DateTime levelup_time0;

		private DateTime opening_time0;

		public string sound_opening = "itembox_open_0";

		public string sound_release = "itembox_open_1";

		private const uint colline_once = 4278202345U;

		private const uint colicon_once = 4278736291U;

		private EfSpellRegisteration Spz;

		private M2NoelCane Cane;

		private M2Phys Phy;

		private const float open_walkt_normal = 20f;

		private const float open_walkt_magic = 80f;

		private const float open_fadet_normal = 60f;

		private const float open_fadet_magic = 95f;

		private const float magic_anim_rod_release = 25f;

		private const float magic_anim_charge_start_t = 390f;

		private const float magic_anim_explode_t = 713f;

		private const float opent_normal = 70f;

		private const float magic_anim_explode_after = 190f;

		private const float opent_magic = 903f;

		public float opent = 70f;

		public float open_walkt = 20f;

		public float open_fadet = 60f;

		private DateTime circulation_time0 = DateTime.MinValue;

		private float efy;

		private EffectHandlerPE PE;

		private static float box_level;

		private string _tostring;

		public enum BOXTYPE
		{
			DEACTIVATE,
			DEACTIVATE_MAGIC,
			M2D_NORMAL,
			M2D_FLUSH,
			M2D_MONEY,
			M2D_MONEY_FLUSH,
			M2D_SKILL,
			M2D_SLOT,
			M2D_MAGIC,
			M2D_ENHANCER,
			M2D_SKILL_HP,
			M2D_SKILL_MP,
			FORBIDDEN
		}

		private enum STATE
		{
			CLOSE,
			OPENING_PREPARE,
			OPENING,
			RELEASE,
			FULLOPEN,
			DISAPPEARING,
			DISAPPEAR
		}
	}
}
