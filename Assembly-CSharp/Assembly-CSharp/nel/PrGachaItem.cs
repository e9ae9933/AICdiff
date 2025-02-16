using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class PrGachaItem : IRunAndDestroy
	{
		public bool ev_assign
		{
			get
			{
				return this.Con.Con.ev_assign;
			}
		}

		public PrGachaItem(AbsorbManager _Con)
		{
			this.Con = _Con;
		}

		public void clearTextInstance(M2Mover BaseMover)
		{
			try
			{
				if (this.Tx != null)
				{
					IN.DestroyOne(this.Tx.gameObject);
				}
			}
			catch
			{
			}
			this.Tx = null;
		}

		public PrGachaItem activate(PrGachaItem.TYPE _type, int need_count, uint _key_alloc_bit = 63U)
		{
			return this.activate(_type, need_count, false, _key_alloc_bit);
		}

		public PrGachaItem activateNotDiffFix(PrGachaItem.TYPE _type, int need_count, uint _key_alloc_bit = 63U)
		{
			return this.activate(_type, need_count, true, _key_alloc_bit);
		}

		private PrGachaItem activate(PrGachaItem.TYPE _type, int need_count, bool do_not_fix_by_difficulty, uint _key_alloc_bit = 63U)
		{
			if (this.Pr == null)
			{
				return this;
			}
			if (!this.Pr.is_alive)
			{
				_type = PrGachaItem.TYPE.CANNOT_RELEASE;
			}
			PrGachaItem.TYPE type = this.type;
			this.type = _type;
			this.t = (this.post = 0f);
			this.t_input = 0f;
			this.t_lock_coruppt = 0f;
			this.corrupt_val = 0f;
			this.count = 0f;
			this.SoloPositionPixel = new Vector3(0f, 0f, -1f);
			if (!do_not_fix_by_difficulty && this.type != PrGachaItem.TYPE.CANNOT_RELEASE)
			{
				DIFF.fixGachaInput(this.Con.Con, this, ref this.type, ref need_count, ref _key_alloc_bit);
			}
			this.count0 = need_count * 100;
			this.key_alloc_bit = _key_alloc_bit;
			this.gacha_hit_abs = false;
			if (this.type == PrGachaItem.TYPE.SEQUENCE && (this.key_alloc_bit & 15U) != 0U)
			{
				this.key_alloc_bit &= 15U;
			}
			if (this.key_alloc_bit == 0U)
			{
				this.key_alloc_bit = ((DIFF.I >= 1) ? 48U : 16U);
			}
			if (this.type != PrGachaItem.TYPE.CANNOT_RELEASE)
			{
				this.HkdsColor = C32.d2c(4292730071U);
				this.PMeshHkds = MTR.PMeshGachaUni;
				this.Con.Con.need_fine_gacha_effect = true;
				this.prepareTx(ref this.Tx);
				this.Akeys.Clear();
				switch (this.type)
				{
				case PrGachaItem.TYPE.REP_AFTER_ORGASM:
					this.Akeys.Add(KEY.getRandomKeyBit(this.key_alloc_bit));
					this.HkdsColor = C32.d2c(4286807939U);
					this.PMeshHkds = MTR.PMeshGachaCircle;
					goto IL_02E0;
				case PrGachaItem.TYPE.SEQUENCE:
					this.prepareTx(ref this.TxNext);
					this.t_input = 10f;
					goto IL_02E0;
				case PrGachaItem.TYPE.PENDULUM:
				case PrGachaItem.TYPE.PENDULUM_ONNIE:
					this.Akeys.Add(KEY.getRandomKeyBit(this.key_alloc_bit));
					if (this.Pdr == null)
					{
						this.Pdr = new PendulumDrawer();
						this.Pdr.fnDrawStone = new PendulumDrawer.FnDrawStone(this.fnDrawPendulumStone);
					}
					this.Pdr.ColAccpOff = C32.d2c(4291480266U);
					this.Pdr.ColStoneOff = C32.d2c(4281874488U);
					this.Pdr.ColStoneOn = C32.d2c(4285098345U);
					this.Pdr.ColStoneBorderOn = C32.d2c(4294049624U);
					if (this.type == PrGachaItem.TYPE.PENDULUM_ONNIE)
					{
						this.HkdsColor = C32.d2c(4286807939U);
						this.Pdr.ColAccpOff = C32.d2c(4292532954U);
						this.Pdr.ColStoneBorderOff = C32.d2c(4292532954U);
						this.PMeshHkds = MTR.PMeshGachaCircle;
					}
					this.Pdr.need_recalc = true;
					this.Pdr.resetTime();
					goto IL_02E0;
				}
				this.Akeys.Add(KEY.getRandomKeyBit(this.key_alloc_bit));
				IL_02E0:
				this.fineTextContent();
				this.runPosition(true);
				if ((this.key_alloc_bit & 15U) != 0U)
				{
					this.Con.Con.has_gacha_lrtb = true;
				}
			}
			else
			{
				this.Tx != null;
			}
			return this;
		}

		public bool isConfisionGacha
		{
			get
			{
				return this.Con.Con.isConfisionGacha;
			}
		}

		private void prepareTx(ref TextRenderer Tx)
		{
			if (Tx != null)
			{
				return;
			}
			Tx = IN.CreateGob(IN._stage.gameObject, "-Gacha-Tx" + this.Con.id.ToString()).AddComponent<TextRenderer>();
			Tx.gameObject.layer = IN.LAY(IN.gui_layer_name);
			Tx.Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE);
			Tx.html_mode = true;
			Tx.Col(C32.d2c(uint.MaxValue)).BorderCol(C32.d2c(4278190080U)).Size(Tx.getStorage().defaultRendererSize);
			Tx.gameObject.SetActive(false);
		}

		private void fineTextContent()
		{
			if (this.Tx == null || (this.Akeys.Count == 0 && this.type != PrGachaItem.TYPE.SEQUENCE))
			{
				return;
			}
			this.Tx.alpha = 1f;
			using (STB stb = TX.PopBld(null, 0))
			{
				PrGachaItem.TYPE type = this.type;
				if (type - PrGachaItem.TYPE.REP > 1)
				{
					if (type == PrGachaItem.TYPE.SEQUENCE)
					{
						if (this.Akeys.Count == 0)
						{
							this.Akeys.Add("");
						}
						while (this.Akeys.Count < 3)
						{
							this.Akeys.Add(KEY.getRandomKeyBit(this.key_alloc_bit));
						}
						stb.AddLw(" <key ", this.Akeys[1], " />");
						using (STB stb2 = TX.PopBld(null, 0))
						{
							this.TxNext.Txt(stb2.AddLw(" <key ", this.Akeys[2], " />"));
							goto IL_0150;
						}
					}
					stb.AddLw(" <key ", this.Akeys[0], " />");
					if (this.type == PrGachaItem.TYPE.PENDULUM || this.type == PrGachaItem.TYPE.PENDULUM_ONNIE)
					{
						this.Tx.alpha = 0.5f;
					}
				}
				else
				{
					stb.AddLw(" <key ", this.Akeys[0], " />");
				}
				IL_0150:
				this.Tx.Txt(stb);
			}
			this.fineTextAlphaPos();
			this.Tx.MustRedraw();
		}

		public void fineTextAlphaPos()
		{
			if (this.type == PrGachaItem.TYPE.SEQUENCE)
			{
				float num = ((this.t_input < 0f) ? 1f : X.ZSIN(this.t_input, 10f));
				this.Tx.alpha = X.NIL(0.5f, 1f, num, 1f);
				this.Tx.transform.localPosition = new Vector3(20f * (1f - num), 4f, 0f) * 0.015625f;
				this.TxNext.alpha = X.NIL(0f, 0.5f, num, 1f);
				this.TxNext.transform.localPosition = new Vector3(20f + 20f * (1f - num), 4f, 0f) * 0.015625f;
				return;
			}
			this.Tx.transform.localPosition = new Vector3(0f, 4f, 0f) * 0.015625f;
		}

		public PrGachaItem clear(PR _Pr)
		{
			this.type = PrGachaItem.TYPE.CANNOT_RELEASE;
			this.Pr = _Pr;
			this.count = (float)this.count0;
			this.t_lock_coruppt = 0f;
			this.corrupt_val = 0f;
			this.Tx != null;
			if (this.t >= 0f)
			{
				this.t = -1f;
			}
			return this;
		}

		public PrGachaItem setDep(float _dx, float _dy, bool solo = false)
		{
			this.depx = _dx;
			this.depy = _dy;
			if (this.ev_assign)
			{
				this.depx *= 0.5f;
				this.depy *= 0.5f;
			}
			this.post = (float)((this.t <= 2f) ? (-1) : 0);
			if (this.SoloPositionPixel.z >= 0f && solo)
			{
				this.depx += this.SoloPositionPixel.x;
				this.depy += this.SoloPositionPixel.y;
			}
			this.ran = X.xors();
			return this;
		}

		public void destruct()
		{
			this.clear(null);
		}

		public bool can_input_player()
		{
			return this.Pr.Ser.xSpeedRate() > 0f;
		}

		public void addCountOne(float lvl = 1f, bool padvib = true)
		{
			float num;
			if (this.ev_assign)
			{
				num = 1f;
			}
			else if (this.gacha_hit_abs)
			{
				num = ((DIFF.I == 0) ? 1.5f : 1f);
			}
			else
			{
				num = X.NI(this.Pr.GachaReleaseRate(), 1f, DIFF.gacha_punch_ser_screen_value);
				float num2 = this.Pr.getRE(RCP.RPI_EFFECT.ARREST_ESCAPE);
				if (num2 > 0f)
				{
					num2 = X.Scr2(num2, DIFF.gacha_punch_screen_value) * 0.5f;
				}
				num = X.Mx(0.1f, num * X.NI(1f, 3f, num2));
			}
			this.count += 100f * num * lvl;
			if (this.count >= (float)this.count0 && this.corrupt_val > 0f)
			{
				this.t_lock_coruppt = ((this.t_lock_coruppt == 0f) ? 45f : X.Mx(this.t_lock_coruppt, 22f));
			}
			if (padvib)
			{
				this.Pr.PadVib("gacha_countone", 1f);
			}
		}

		public void addCountAbs(float pow, float max_level = 0.8f)
		{
			this.count = X.Mn(this.count + pow, X.Mx(this.count, (float)this.count0 * max_level));
		}

		public void runPosition(bool force_dep_set = false)
		{
			float num2;
			float num3;
			float num4;
			if (!this.ev_assign)
			{
				float num = X.Mn(0.5f, 1f / this.Mp.M2D.Cam.getScale(true));
				Vector3 damageCounterShiftMapPos = this.Pr.getDamageCounterShiftMapPos();
				num2 = this.Mp.ux2effectScreenx(this.Mp.pixel2ux(this.Pr.drawx + damageCounterShiftMapPos.x * this.Mp.CLEN + this.depx * num));
				num3 = this.Mp.uy2effectScreeny(this.Mp.pixel2uy(this.Pr.drawy + damageCounterShiftMapPos.y * this.Mp.CLEN - this.depy * num));
				num4 = 1f;
			}
			else
			{
				num2 = this.depx * 0.5f * 0.015625f;
				num3 = this.depy * 0.5f * 0.015625f;
				num4 = 2f;
			}
			if (force_dep_set)
			{
				this.x = num2;
				this.y = num3;
				return;
			}
			if (this.post < 0f)
			{
				this.post = 20f;
				this.x = num2;
				this.y = num3;
				return;
			}
			if (this.post < 20f)
			{
				float num5 = 1f / (20f - this.post);
				this.x = X.VALWALK(this.x, num2, X.Abs(num2 - this.x) * num5);
				this.y = X.VALWALK(this.y, num3, X.Abs(num3 - this.y) * num5);
				return;
			}
			if (num2 != this.x || num3 != this.y)
			{
				num4 *= 0.0546875f;
				this.x = X.MULWALKMN(this.x, num2, 0.04f, num4);
				this.y = X.MULWALKMN(this.y, num3, 0.04f, num4);
			}
		}

		public bool run(float fcnt)
		{
			if (this.t < 0f || this.type == PrGachaItem.TYPE.CANNOT_RELEASE || this.Pr == null)
			{
				return false;
			}
			if (!this.Pr.is_alive)
			{
				return true;
			}
			this.runPosition(false);
			this.post += fcnt;
			bool flag = true;
			if (this.count >= 0f && this.can_input_player())
			{
				bool flag2 = this.isFinished();
				if (this.t_lock_coruppt > 0f)
				{
					this.t_lock_coruppt = X.Mx(this.t_lock_coruppt - fcnt, 0f);
				}
				if (this.corrupt_val > 0f && (this.t_lock_coruppt == 0f || !flag2))
				{
					float num = fcnt * this.CORRUPT_REDUCE * DIFF.gacha_corrupt_level;
					this.corrupt_val = X.Mx(this.corrupt_val - num, 0f);
					this.count = X.Mx(this.count - num, 0f);
					if (flag2)
					{
						this.Con.gacha_releaseable = false;
						this.Con.Con.need_fine_gacha_release = true;
					}
				}
				bool flag3 = this.count > 0f && this.t_input > 0f;
				switch (this.type)
				{
				case PrGachaItem.TYPE.REP:
				case PrGachaItem.TYPE.REP_AFTER_ORGASM:
					this.Con.KeyIsSafe(this.Akeys[0]);
					if (this.Con.isKeyPD(this.Akeys[0], this.input_breaking, true))
					{
						this.addCountOne(1f, true);
						this.t_input = 1f;
						this.Punch();
					}
					flag = this.count < (float)this.count0;
					break;
				case PrGachaItem.TYPE.HOLD:
					this.Con.KeyIsSafe(this.Akeys[0]);
					if ((this.count == 0f) ? this.Con.isKeyPD(this.Akeys[0], this.input_breaking, true) : this.Con.isKeyPO(this.Akeys[0], this.input_breaking, true))
					{
						if (this.t_anim_punch <= 7.3333335f)
						{
							this.Punch();
						}
						this.addCountOne(fcnt * 0.1f, true);
						this.t_input = 1f;
					}
					flag = this.count < (float)this.count0;
					break;
				case PrGachaItem.TYPE.SEQUENCE:
				{
					string text = this.Akeys[0];
					if (TX.valid(text))
					{
						if (this.Con.isKeyPO(text, true, true))
						{
							if (!this.Con.isKeyPD(text, true, true))
							{
								this.Con.KeyIsSafe(text);
							}
						}
						else
						{
							this.Akeys[0] = "";
						}
					}
					if (this.count >= (float)this.count0)
					{
						flag = false;
					}
					else
					{
						flag3 = true;
						string text2 = this.Akeys[1];
						this.Con.KeyIsSafe(text2);
						if (this.Con.isKeyPD(text2, this.input_breaking, true))
						{
							float num2 = this.count;
							this.addCountOne(1f, true);
							this.t_input = 1f;
							this.Punch();
							this.Akeys.RemoveAt(1);
							this.fineTextContent();
							this.Akeys[0] = text2;
							if (this.corrupt_val <= 0f && this.count + (this.count - num2) + 0.0001f >= (float)this.count0)
							{
								this.TxNext.text_content = "";
							}
						}
						flag = this.count < (float)this.count0;
					}
					break;
				}
				case PrGachaItem.TYPE.PENDULUM:
				case PrGachaItem.TYPE.PENDULUM_ONNIE:
					this.Pdr.run(fcnt);
					if (this.Pdr.isAccepting(false))
					{
						if (this.Con.isKeyPD(this.Akeys[0], false, true) && this.Pdr.Tap())
						{
							this.Con.isKeyPD(this.Akeys[0], true, true);
							this.addCountOne(fcnt, this.type != PrGachaItem.TYPE.PENDULUM_ONNIE);
							this.t_input = 1f;
							if (this.type == PrGachaItem.TYPE.PENDULUM)
							{
								this.Punch();
							}
						}
					}
					else if (this.count > 0f && this.type == PrGachaItem.TYPE.PENDULUM && this.Con.isKeyPD(this.Akeys[0], true, true))
					{
						this.ErrorOccured(false, true);
					}
					flag = this.count < (float)this.count0;
					if (this.count > 0f && this.type == PrGachaItem.TYPE.PENDULUM && this.Pdr.getIgnoredTap(true))
					{
						this.ErrorOccured(false, true);
					}
					break;
				}
				if (flag3)
				{
					this.t_input += fcnt;
				}
			}
			else
			{
				this.t_input = 0f;
				this.count = X.Mn(this.count + fcnt, 0f);
				PrGachaItem.TYPE type = this.type;
				if (type - PrGachaItem.TYPE.PENDULUM <= 1)
				{
					this.Pdr.resetTime();
				}
			}
			if (this.t_anim_punch > 0f)
			{
				this.t_anim_punch = X.Mx(this.t_anim_punch - fcnt, 0f);
			}
			if (this.t_input < 0f)
			{
				this.t_input = X.Mn(this.t_input + fcnt, 0f);
			}
			this.t += fcnt;
			return flag;
		}

		public void ErrorOccured(bool timeout = false, bool reset_count = true)
		{
			if (!timeout)
			{
				this.count = -40f;
				this.t_input = X.Mn(this.t_input, 0f);
			}
			else
			{
				this.count = (reset_count ? X.Mn(this.count, 0f) : this.count);
				this.t_input = -40f;
			}
			this.corrupt_val = 0f;
			this.t_lock_coruppt = 0f;
			if (this.type == PrGachaItem.TYPE.SEQUENCE)
			{
				this.fineTextContent();
			}
		}

		private void Punch()
		{
			this.t_anim_punch = 22f;
			M2DBase.playSnd("hit_gacha_escape");
		}

		public bool isActive()
		{
			return this.t >= 0f;
		}

		public bool isInputtable()
		{
			return this.isUseable() && this.count >= 0f && this.t_input >= 0f;
		}

		public bool isUseable()
		{
			return this.Pr != null && this.t >= 0f && this.type > PrGachaItem.TYPE.CANNOT_RELEASE;
		}

		public bool isErrorInput()
		{
			return this.count < 0f;
		}

		public Map2d Mp
		{
			get
			{
				return this.Con.Mp;
			}
		}

		public void releaseEffect()
		{
			if (this.Pr != null)
			{
				this.Pr.Mp.PtcN("gacha_release", this.Pr.x + this.depx / this.Pr.CLENM, this.Pr.y - this.depy / this.Pr.CLENM, 0f, 0, 0);
			}
		}

		public bool input_breaking
		{
			get
			{
				return this.count < (float)this.count0;
			}
		}

		public bool CorruptGacha(float level, bool use_limit = true)
		{
			if (!this.isUseable() || this.count <= 0f || this.type == PrGachaItem.TYPE.PENDULUM_ONNIE)
			{
				return false;
			}
			bool flag = this.corrupt_val == 0f;
			if (this.isFinished() && this.t_lock_coruppt == 0f)
			{
				this.t_lock_coruppt = 45f;
			}
			level *= 100f;
			if (use_limit)
			{
				this.corrupt_val = X.Mn(X.Mx(this.corrupt_val, level * 4f), this.corrupt_val + level);
				return flag;
			}
			this.corrupt_val += level;
			return flag;
		}

		public bool drawGachaB(MeshDrawer Md, bool event_bind)
		{
			if (!this.isUseable() || !this.Pr.is_alive || !this.Pr.Mp.M2D.Cam.isBaseMover(this.Pr))
			{
				return true;
			}
			Vector3 vector = this.Mp.M2D.Cam.getQuakeXyPixel() * 0.015625f * 2f;
			float num = 1f;
			float num2;
			if (!event_bind)
			{
				num2 = this.Mp.M2D.Cam.getScaleRev() * 2f;
			}
			else
			{
				num2 = 2f;
				num = 2f;
			}
			Md.base_x = this.x * num - vector.x;
			Md.base_y = this.y * num - vector.y;
			float num3;
			if (this.type == PrGachaItem.TYPE.PENDULUM || this.type == PrGachaItem.TYPE.PENDULUM_ONNIE)
			{
				num3 = X.ZLINE(this.Pdr.getUnderBeatFrame(), 44f);
			}
			else if (this.t < 0f)
			{
				num3 = 1f;
			}
			else
			{
				num3 = this.t + (float)(this.index * 10);
				num3 = (float)((int)num3 % 44) / 44f;
			}
			float num4 = 1f - X.ZLINE(X.ZSIN(num3, 0.3f));
			bool flag = false;
			Md.ColGrd.Set(this.HkdsColor).blend(4290493108U, num4);
			if ((this.key_alloc_bit & 15U) != 0U && this.isConfisionGacha)
			{
				flag = true;
				Md.Col = C32.MulA(4294760311U, 0.5f + 0.5f * num4);
				Md.ColGrd.blend(4284762172U, 0.7f);
			}
			Md.ColGrd.mulA((this.t < 0f) ? 0.5f : 1f);
			float num5 = X.NI(1f, 1.15f, num4);
			if (this.count >= (float)this.count0)
			{
				Md.ColGrd.mulA(0.65f + X.COSIT(70f) * 0.2f);
				num5 = 1f;
			}
			if (this.t_anim_punch > 0f)
			{
				num5 = X.NI(0.75f, num5, X.ZSIN(22f - this.t_anim_punch, 11f));
				Md.ColGrd.blend(4294962266U, X.ZLINE(this.t_anim_punch, 22f)).mulA((this.count >= (float)this.count0) ? 0.8f : 1f);
			}
			num5 *= num2;
			Md.Col = Md.ColGrd.C;
			Md.RotaMesh(0f, 0f, num5, num5 * 80f / 160f, X.NI(-0.5f, 0.5f, X.RAN(this.ran, 1403)) * 3.1415927f * 0.12f, flag ? MTR.PMeshGachaCircle : this.PMeshHkds, false, false);
			Md.Identity();
			if (flag)
			{
				Md.Col = C32.MulA(4294760311U, 0.5f + 0.5f * num4);
				int num6 = 7;
				float num7 = 1.5707964f;
				float num8 = 6.2831855f / (float)num6;
				float num9 = X.ANMP((int)this.Mp.floort, 120, 6.2831855f);
				for (int i = 0; i < num6; i++)
				{
					Md.Circle(70f * X.Cos(num7) + 10f * X.Cos(num7 + num9), 25f * X.Sin(num7) + 10f * X.Sin(num7 + num9), 20f, 1f, false, 0f, 0f);
					num7 += num8;
				}
			}
			return true;
		}

		public void InitMesh(MeshDrawer Md, TextRenderer.MESH_TYPE type, bool no_identity = false)
		{
			if (Md.getSubMeshMaterial((int)type) == null)
			{
				Md.chooseSubMesh((int)type, false, false);
				Md.setMaterial(this.Tx.SubMeshMtr(type), false);
			}
			else
			{
				Md.chooseSubMesh((int)type, false, false);
				if (Md.getTriMax() == 0)
				{
					Md.setMaterial(this.Tx.SubMeshMtr(type), false);
				}
			}
			if (!no_identity)
			{
				Md.Identity();
			}
		}

		public bool drawGachaT(MeshDrawer Md, bool event_bind)
		{
			M2Camera cam = this.Mp.M2D.Cam;
			if (!this.isUseable() || !this.Pr.is_alive || !cam.isBaseMover(this.Pr) || this.Tx == null)
			{
				return true;
			}
			this.Tx.MustRedraw();
			if (!event_bind)
			{
				float num = this.Mp.ux2effectScreenx(this.Mp.pixel2ux(cam.x));
				float num2 = this.Mp.uy2effectScreeny(this.Mp.pixel2uy(cam.y));
				float num3 = 0.5f * cam.getScale(true);
				Md.base_x = (this.x - num) * num3;
				Md.base_y = (this.y - num2) * num3;
			}
			else
			{
				Md.base_x = this.x;
				Md.base_y = this.y;
			}
			float base_x = Md.base_x;
			float base_y = Md.base_y;
			float num4;
			if (this.type == PrGachaItem.TYPE.PENDULUM || this.type == PrGachaItem.TYPE.PENDULUM_ONNIE)
			{
				num4 = X.ZLINE(this.Pdr.getUnderBeatFrame(), 44f);
			}
			else if (this.t < 0f)
			{
				num4 = 1f;
			}
			else
			{
				num4 = this.t + (float)(this.index * 10);
				num4 = (float)((int)num4 % 44) / 44f;
			}
			float num5 = 1f - X.ZLINE(X.ZSIN(num4, 0.3f));
			if (this.isConfisionGacha && this.index == 0)
			{
				Md.base_x = 0f;
				Md.base_y = -IN.hh * 0.6f * 0.54f * 0.015625f;
				this.InitMesh(Md, TextRenderer.MESH_TYPE.ICO_T0, false);
				Md.Col = C32.MulA(uint.MaxValue, 0.75f + 0.25f * num5);
				Md.RotaPF(-40f, 0f, 1f, 1f, 0f, MTRX.AUiSerIcon[10], false, false, false, uint.MaxValue, false, 0);
				STB stb = TX.PopBld("REVERSE", 0);
				Md.Col = C32.d2c(2852126720U);
				MTRX.ChrM.DrawScaleStringTo(Md, stb, -8f, -1f, 2f, 2f, ALIGN.LEFT, ALIGNY.MIDDLE, false, 0f, 0f, null);
				Md.Col = C32.MulA(4294960028U, 0.75f + 0.25f * num5);
				MTRX.ChrM.DrawScaleStringTo(Md, stb, -8f, 1f, 2f, 2f, ALIGN.LEFT, ALIGNY.MIDDLE, false, 0f, 0f, null);
				TX.ReleaseBld(stb);
				Md.Identity();
				Md.base_x = base_x;
				Md.base_y = base_y;
			}
			Vector3 vector;
			if (this.type == PrGachaItem.TYPE.SEQUENCE && this.TxNext != null)
			{
				this.TxNext.MustRedraw();
				vector = this.TxNext.transform.localPosition;
				Md.MergeSubMeshAll(this.TxNext.getMeshDrawer(), Md.base_x + vector.x, Md.base_y + vector.y);
			}
			vector = this.Tx.transform.localPosition;
			Md.MergeSubMeshAll(this.Tx.getMeshDrawer(), Md.base_x + vector.x, Md.base_y + vector.y);
			switch (this.type)
			{
			case PrGachaItem.TYPE.REP:
			case PrGachaItem.TYPE.REP_AFTER_ORGASM:
			{
				this.InitMesh(Md, TextRenderer.MESH_TYPE.ICO_T0, false);
				PxlFrame pxlFrame = MTRX.getPF("rep_hand");
				float num6 = (float)X.ANMT(2, 6f);
				Md.Col = MTRX.ColWhite;
				Md.RotaPF(-18f + 6f * num6, 4f, 1f, 1f, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
				if (num6 == 1f)
				{
					this.InitMesh(Md, TextRenderer.MESH_TYPE.MESH_T1, false);
					Md.Col = C32.d2c(4294918766U);
					Md.ColGrd.Set(4292673588U);
					Md.Star(-11f, 4f, 12f, 1.5707964f, 7, 0.6f, 0f, false, 1f, 0f);
				}
				break;
			}
			case PrGachaItem.TYPE.HOLD:
			{
				this.InitMesh(Md, TextRenderer.MESH_TYPE.ICO_T0, false);
				PxlFrame pxlFrame = MTRX.getPF("arrow_b");
				float num6 = 1f - X.ZPOW(1f - num4);
				Md.Col = Md.ColGrd.Set(4288256409U).blend(uint.MaxValue, X.ZLINE(num4 - 0.2f, 0.4f)).C;
				Md.RotaPF(0f, 34f - 23f * num6, 1f, 0.65f + 0.5f * X.ZSIN(num4, 0.25f) - 0.15f * X.ZCOS(num4 - 0.25f, 0.3f), 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
				break;
			}
			case PrGachaItem.TYPE.SEQUENCE:
			{
				if (this.t_input >= 0f && this.t_input <= 16f)
				{
					this.fineTextAlphaPos();
				}
				this.InitMesh(Md, TextRenderer.MESH_TYPE.MESH_T1, false);
				float num7 = X.ANMPT(90, 1f);
				Md.Col = C32.MulA(4283780170U, 0.8f + 0.2f * X.COSIT(45f));
				for (int i = 0; i < 4; i++)
				{
					Md.Identity().Scale(0.8f, 1f, false).Rotate((num7 + (float)i * 0.25f) * 6.2831855f, false)
						.TranslateP(0f, 4f, false);
					Md.Poly(60f, 0f, 9f, 3.1415927f, 3, 0f, false, 0f, 0f);
				}
				break;
			}
			case PrGachaItem.TYPE.PENDULUM:
			case PrGachaItem.TYPE.PENDULUM_ONNIE:
				this.InitMesh(Md, TextRenderer.MESH_TYPE.ICO_T0, false);
				this.Tx.alpha = (this.Pdr.isAccepting(true) ? 1f : 0.5f);
				this.InitMesh(Md, TextRenderer.MESH_TYPE.MESH_B1, false);
				Md.Scale(0.5f, 0.5f, false).TranslateP(0f, 28f, false);
				this.Pdr.drawTo(Md, 0f, 0f);
				if (this.t_input > 0f)
				{
					this.InitMesh(Md, TextRenderer.MESH_TYPE.ICO_T1, false);
					MTR.EplPendulumTapCircle.drawTo(Md, Md.base_x * 64f, Md.base_y * 64f + 18f, 0f, 0, this.t_input, 0f);
				}
				break;
			}
			float num8 = 70f;
			float num9 = 7f;
			this.InitMesh(Md, TextRenderer.MESH_TYPE.MESH_T0, false);
			Md.TranslateP(0f, -12f, false);
			uint num10 = 4281677109U;
			if (this.count < 0f || !this.can_input_player())
			{
				float num11 = X.ZLINE(40f + this.count, 40f);
				Md.Col = Md.ColGrd.Set(4289009207U).blend(num10, X.ZSIN(num11, 0.6f)).C;
				Md.Rotate(0.25132743f * X.COSIT(12.2f) * (1f - X.ZLINE(num11, 0.7f)), false);
				Md.Box(0f, 0f, num8, num9, 0f, false);
				float num12 = X.NI(1.3f, 1f, X.ZSIN(num11, 0.3f));
				this.InitMesh(Md, TextRenderer.MESH_TYPE.MESH_T1, false);
				Md.Scale(num12, num12, false).TranslateP(0f, 4f, false);
				Md.Col = C32.d2c(4282058760U);
				Md.Line(-30f, 30f, 30f, -30f, 10f, false, 0f, 0f).Line(30f, 30f, -30f, -30f, 10f, false, 0f, 0f);
			}
			else if (this.t_input < 0f)
			{
				float num13 = X.ZLINE(40f + this.t_input, 40f);
				Md.Col = Md.ColGrd.Set(4287335307U).blend(num10, X.ZPOW(num13, 0f)).C;
				Md.TranslateP(8f * X.COSIT(27.2f) * (1f - X.ZLINE(num13, 0.95f)), 0f, false);
				Md.Box(0f, 0f, num8, num9, 0f, false);
				this.InitMesh(Md, TextRenderer.MESH_TYPE.ICO_T0, true);
				Md.Col = Md.ColGrd.Set(10526880).blend(uint.MaxValue, (0.8f + X.COSIT(26f) * 0.2f) * (1f - X.ZLINE(num13 - 0.7f, 0.3f))).C;
				Md.RotaPF(-num8 * 0.5f, 0f, 2f, 2f, 0f, MTRX.getPF("question"), false, false, false, uint.MaxValue, false, 0);
			}
			else
			{
				float num14 = (float)X.IntR(X.Mn(this.count, (float)this.count0) / (float)this.count0 * (num8 - 2f));
				float num15 = 0f;
				if (this.count > 0f && this.t_input > 0f)
				{
					num15 = 1f - X.ZSIN(this.t_input, 18f);
					num14 = X.Mn(num14 + X.Sin(num15 * 3.1415927f * 0.7f) * 8f, num8 - 2f);
				}
				if (this.count >= (float)this.count0)
				{
					if (this.corrupt_val > 0f)
					{
						Md.ColGrd.Set(4294921549U).blend(4291242339U, 0.5f + X.COSIT(40f) * 0.5f);
					}
					else
					{
						Md.ColGrd.White();
					}
					Md.Col = Md.ColGrd.blend(4294952560U, num15).C;
					Md.Box(0f, 0f, num8, num9, 0f, false);
					Md.Col = MTRX.ColBlack;
					Md.CheckMark(0f, 4f, num8 * 0.45f + 4f, 8f, false);
					Md.Col = Md.ColGrd.Set(num10).blend(4294965302U, 0.25f + X.COSIT(70f) * 0.2f).C;
					Md.CheckMark(0f, 4f, num8 * 0.45f, 4f, false);
				}
				else
				{
					Md.Col = C32.d2c(num10);
					Md.Box(0f, 0f, num8, num9, 0f, false);
					if (this.corrupt_val > 0f)
					{
						Md.ColGrd.Set(4294921549U).blend(4291242339U, 0.5f + X.COSIT(40f) * 0.5f);
					}
					else
					{
						Md.ColGrd.White();
					}
					Md.Col = Md.ColGrd.blend(4294952560U, num15).C;
					Md.Box(0f, 0f, num14, num9 - 2f, 0f, false);
				}
			}
			Md.Identity();
			return true;
		}

		private void fnDrawPendulumStone(PendulumDrawer Pdr, MeshDrawer Md, float x, float y, float agR, float pd_agR, bool is_bottom)
		{
			if (!is_bottom)
			{
				this.InitMesh(Md, TextRenderer.MESH_TYPE.ICO_B1, true);
				Md.RotaPF(x, y, 0.5f, 0.5f, agR, MTRX.getPF("shape_heart"), false, false, false, uint.MaxValue, false, 0);
				this.InitMesh(Md, TextRenderer.MESH_TYPE.MESH_B1, true);
			}
		}

		public static bool checkDrawPoints(M2Mover BaseMover, AbsorbManager[] AItems, int LEN)
		{
			int num = 0;
			int num2 = 0;
			float num3 = BaseMover.sizey * BaseMover.CLENM;
			bool flag = true;
			for (int i = 0; i < LEN; i++)
			{
				AbsorbManager absorbManager = AItems[i];
				if (absorbManager.get_Gacha().isUseable())
				{
					num |= 1 << i;
					M2Attackable publishMover = AItems[i].getPublishMover();
					if (!absorbManager.isTortureUsing())
					{
						num3 = X.Mx(num3, X.Mn(1.5f, publishMover.sizey) * publishMover.CLENM);
					}
					num2++;
					if (flag)
					{
						Vector3 soloPositionPixel = absorbManager.get_Gacha().SoloPositionPixel;
						if (soloPositionPixel.z < 0f || (soloPositionPixel.z == 0f && X.bit_count((uint)num) >= 2))
						{
							flag = false;
						}
					}
				}
			}
			if (num == 0)
			{
				return false;
			}
			int num4 = (int)X.Mx(25f, num3 * 0.6f);
			int num5 = 0;
			for (int j = 0; j < LEN; j++)
			{
				if ((num & (1 << j)) != 0)
				{
					PrGachaItem gacha = AItems[j].get_Gacha();
					gacha.index = num5;
					if (num2 == 1)
					{
						if (num5 == 0)
						{
							gacha.setDep(0f, (float)(X.Mn(60, num4) + 51), flag);
						}
					}
					else if (num2 == 2)
					{
						if (num5 != 0)
						{
							if (num5 == 1)
							{
								gacha.setDep(72f, (float)(num4 + 28), flag);
							}
						}
						else
						{
							gacha.setDep(-80f, (float)(num4 + 3), flag);
						}
					}
					else if (num2 == 3)
					{
						switch (num5)
						{
						case 0:
							gacha.setDep(-90f, (float)(num4 + 43), flag);
							break;
						case 1:
							gacha.setDep(92f, (float)(num4 + 8), flag);
							break;
						case 2:
							gacha.setDep(-93f, (float)(num4 - 19), flag);
							break;
						}
					}
					else if (num2 == 4)
					{
						switch (num5)
						{
						case 0:
							gacha.setDep(-85f, (float)(num4 + 43), flag);
							break;
						case 1:
							gacha.setDep(82f, (float)(num4 + 58), flag);
							break;
						case 2:
							gacha.setDep(-80f, (float)(num4 - 24), flag);
							break;
						case 3:
							gacha.setDep(86f, (float)(num4 - 11), flag);
							break;
						}
					}
					else
					{
						switch (num5)
						{
						case 0:
							gacha.setDep(-71f, (float)(num4 - 40), flag);
							break;
						case 1:
							gacha.setDep(84f, (float)(num4 + 36), flag);
							break;
						case 2:
							gacha.setDep(-83f, (float)(num4 + 20), flag);
							break;
						case 3:
							gacha.setDep(82f, (float)(num4 - 28), flag);
							break;
						case 4:
							gacha.setDep(-20f, (float)(num4 + 53), flag);
							break;
						}
					}
					num5++;
				}
			}
			return true;
		}

		public float getCount(bool is_real = false)
		{
			if (!is_real)
			{
				return this.count / 100f;
			}
			return this.count;
		}

		public float getTotalCount(bool is_real = false)
		{
			return (float)(is_real ? this.count0 : (this.count0 / 100));
		}

		public float getCountLevel()
		{
			return X.ZLINE(this.count, (float)this.count0);
		}

		public uint get_key_alloc_bit()
		{
			return this.key_alloc_bit;
		}

		public bool isFinished()
		{
			return this.count >= (float)this.count0;
		}

		public PendulumDrawer getPendulumDrawer()
		{
			if (this.type != PrGachaItem.TYPE.PENDULUM && this.type != PrGachaItem.TYPE.PENDULUM_ONNIE)
			{
				return null;
			}
			return this.Pdr;
		}

		private float x;

		private float y;

		private float depx;

		private float depy;

		public Vector3 SoloPositionPixel;

		public const float dw = 160f;

		public const float dh = 80f;

		private float count = 100f;

		private int count0 = 100;

		private float t_input;

		private float t_anim_punch;

		private float post;

		private uint key_alloc_bit;

		public const float t_timoout_maxt = 80f;

		public const float T_MISS_FREEZE = 40f;

		public const float T_SEQUENCE_SCROLL_T = 10f;

		private const float T_ANIM_PUNCH_MAXT = 22f;

		private const float POS_MAXT = 20f;

		private const int count_one_hit = 100;

		private List<string> Akeys = new List<string>();

		private const float tx_shift_px_y = 4f;

		public PrGachaItem.TYPE type;

		private float t;

		private uint ran;

		private PR Pr;

		private PendulumDrawer Pdr;

		private TextRenderer Tx;

		private TextRenderer TxNext;

		public int index;

		private Color32 HkdsColor;

		private PxlMeshDrawer PMeshHkds;

		private float t_lock_coruppt;

		private float corrupt_val;

		private const int CORRUPT_LOCK_TIME_ON_FINISHED = 45;

		private float CORRUPT_REDUCE = 1.6666666f;

		public bool gacha_hit_abs;

		public readonly AbsorbManager Con;

		public enum TYPE
		{
			CANNOT_RELEASE,
			REP,
			REP_AFTER_ORGASM,
			HOLD,
			SEQUENCE,
			PENDULUM,
			PENDULUM_ONNIE
		}
	}
}
