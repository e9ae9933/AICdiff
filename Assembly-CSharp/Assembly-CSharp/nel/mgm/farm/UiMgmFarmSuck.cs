using System;
using evt;
using m2d;
using XX;

namespace nel.mgm.farm
{
	public class UiMgmFarmSuck : IEventWaitListener, IRunAndDestroy
	{
		public UiMgmFarmSuck(M2LpMgmFarm _Con)
		{
			this.Con = _Con;
			this.Aamount = new float[3];
			this.Amilk = new int[3];
		}

		public void initAction()
		{
			if (this.Tx == null)
			{
				this.Tx = IN.CreateGobGUI(null, "_Tx_Farm_KD").AddComponent<TextRenderer>();
				this.Tx.size = 16f;
				this.Tx.html_mode = true;
				this.Tx.alignx = ALIGN.CENTER;
				this.Tx.aligny = ALIGNY.MIDDLE;
				this.Tx.text_content = TX.Get("KD_farm_suck", "");
				this.Tx.use_valotile = true;
				IN.setZ(this.Tx.transform, -4.5f);
			}
			this.Tx.gameObject.SetActive(false);
		}

		public void closeAction()
		{
			this.quitSuck();
			if (this.Tx != null)
			{
				IN.DestroyOne(this.Tx.gameObject);
				this.Tx = null;
			}
		}

		public void resetScore()
		{
			this.score_added = false;
			X.ALL0(this.Aamount);
			X.ALL0(this.Amilk);
		}

		public void initSuck(NelNMgmFarmCow Cow)
		{
			if (!this.Con.isMainGame())
			{
				return;
			}
			if (this.Ef == null)
			{
				this.resetScore();
				if (this.FD_efDraw == null)
				{
					this.FD_efDraw = delegate(EffectItem Ef)
					{
						if (!this.efDraw(Ef))
						{
							if (Ef == this.Ef)
							{
								this.Ef = null;
							}
							return false;
						}
						return true;
					};
				}
				this.Ef = UIBase.Instance.getEffect().setEffectWithSpecificFn("farm_suck", 0f, 0f, 0f, 0, 0, this.FD_efDraw);
				this.Mp.addRunnerObject(this);
				this.drop_level = (float)Cow.hp_drop_level / (float)Cow.hp_drop_max;
			}
			else
			{
				this.Ef.af = 0f;
				this.Ef.time++;
				this.Ef.z = 0f;
				this.fineUiPicAnim(0);
			}
			this.pushdown_maxt = X.NI(85f, 23f, X.ZSIN(this.drop_level)) * X.NIL(1f, 0.5f, (float)this.Ef.time, 2f);
			float num = X.NIXP(1f, 1.25f);
			this.madded_time = this.pushdown_maxt * (num - 1f);
			this.pushdown_maxt *= num;
			this.Tx.gameObject.SetActive(true);
			this.fineTxPos();
			IN.clearArrowPD(15U, true, 20);
			EV.initWaitFn(this, 0);
		}

		public void fineUiPicAnim(int i)
		{
			UIPicture.Instance.setFade("cuts_cow_0", (i == 2) ? UIPictureBase.EMSTATE.SMASH : ((i == 1) ? UIPictureBase.EMSTATE.BATTLE : UIPictureBase.EMSTATE.NORMAL), i >= 1, false, false);
		}

		public void quitPtcEffect()
		{
			if (this.ThrMilkShot != null)
			{
				this.ThrMilkShot.Def("_quit", 1f);
				this.ThrMilkShot = null;
			}
		}

		public int quitSuck()
		{
			if (this.Ef != null)
			{
				this.Ef.af = 0f;
				this.Ef = null;
			}
			int num = 0;
			if (!this.score_added)
			{
				for (int i = 0; i < 3; i++)
				{
					int num2 = this.Amilk[i];
					if (num2 > 0)
					{
						this.Con.addTotalScore(num2, this.drop_level);
						num += num2;
					}
				}
			}
			this.score_added = true;
			this.quitPtcEffect();
			this.Mp.remRunnerObject(this);
			if (this.Tx != null)
			{
				this.Tx.gameObject.SetActive(false);
			}
			return num;
		}

		public bool run(float fcnt)
		{
			if (this.Ef == null)
			{
				return false;
			}
			if (X.D)
			{
				this.fineTxPos();
			}
			if (this.Ef.z <= 0f)
			{
				if (this.Ef.af >= 8f && IN.isBP(1))
				{
					this.Ef.z += fcnt;
					this.fineUiPicAnim(1);
					NEL.PadVib("mg_farm_suck_new", 1f);
				}
			}
			else if (this.Aamount[this.Ef.time] == 0f)
			{
				bool flag = this.Ef.z > 10f;
				bool flag2 = this.Ef.z >= this.pushdown_maxt + 10f + this.alloc_over_t;
				if (!IN.isBO(0))
				{
					if (!flag)
					{
						this.Ef.z = -1f;
						this.fineUiPicAnim(0);
					}
					else
					{
						float num = this.drop_level;
						NelNMgmFarmCow suckTarget = this.Con.getSuckTarget();
						bool flag3 = false;
						float num2 = ((suckTarget.get_mp() >= suckTarget.get_maxmp()) ? 1f : (0.66f * suckTarget.mp_ratio));
						int num3;
						float num4;
						if (suckTarget.get_mp() == 0f || flag2)
						{
							this.Aamount[this.Ef.time] = 2f;
							this.fineUiPicAnim(2);
							num3 = ((this.pushdown_level >= 0.66f) ? 1 : 0);
							num4 = X.NI(120, 180, X.ZPOWV(num, 0.6f));
							flag3 = X.XORSP() < this.getSuckOverAfterGrawlRatio(suckTarget);
						}
						else
						{
							float num5 = this.pushdown_level;
							this.Aamount[this.Ef.time] = num5;
							if (num5 < 1f)
							{
								num5 *= 0.875f;
								num5 *= num5;
							}
							num5 *= num2;
							num3 = ((num5 >= 0.9f) ? 2 : ((num5 >= 0.4f) ? 1 : 0));
							num4 = X.NI(120, 250, X.ZPOWV(num, 0.6f));
						}
						int num6 = X.IntR(this.pushdown_level * (num4 + this.madded_time * 0.8f));
						UIBase.Instance.PtcVar("cx", this.get_x(this.Ef.time)).PtcVar("cy", this.get_y() - 60f - 20f).PtcVar("grade", (float)num3)
							.PtcST("mg_mini_fanfare", null, PTCThread.StFollow.NO_FOLLOW);
						num6 = (this.Amilk[this.Ef.time] = X.Mx(1, num6));
						this.Con.score += num6;
						UiMgmFarmSuck.VarCon.define("_rvalue", flag3 ? "-1" : this.Aamount[this.Ef.time].ToString(), true);
						UiMgmFarmSuck.VarCon.define("_drop_level", num.ToString(), true);
						this.Tx.gameObject.SetActive(false);
						this.Ef.af = 0f;
					}
					return true;
				}
				NEL.PadVib((this.Ef.z >= this.pushdown_maxt) ? "mg_farm_sucking_over" : "mg_farm_sucking", 1f);
				this.Ef.z += fcnt;
				if (this.Ef.z > 10f)
				{
					NelNMgmFarmCow suckTarget2 = this.Con.getSuckTarget();
					if (!flag)
					{
						UIBase.Instance.PtcVar("mp_ratio", suckTarget2.mp_ratio).PtcVar("_quit", 0f).PtcST("mg_farm_milk_shot", null, PTCThread.StFollow.NO_FOLLOW);
					}
					if (suckTarget2.get_mp() == 0f && this.Ef.z >= 30f)
					{
						this.fineUiPicAnim(2);
						this.Aamount[this.Ef.time] = -1f;
						UiMgmFarmSuck.VarCon.define("_rvalue", "-1", true);
						return true;
					}
					if (!flag2 && this.Ef.z >= this.pushdown_maxt + 10f + this.alloc_over_t)
					{
						SND.Ui.play("cow_water_empty", false);
					}
				}
			}
			return true;
		}

		public void destruct()
		{
			this.closeAction();
		}

		private bool efDraw(EffectItem E)
		{
			MeshDrawer mesh = E.GetMesh("_farm", uint.MaxValue, BLEND.NORMAL, false);
			MeshDrawer meshImg = E.GetMeshImg("_farm", MTRX.MIicon, BLEND.NORMAL, false);
			mesh.base_z = (meshImg.base_z = -2f);
			mesh.base_px_y = this.get_y() - 60f;
			meshImg.base_px_y = mesh.base_px_y - 60f - 18f;
			float num = 1f;
			float num2 = E.af;
			if (E != this.Ef)
			{
				num2 = 1000f;
				num = 1f - X.ZLINE(E.af, 25f);
				if (num <= 0f)
				{
					return false;
				}
			}
			int num3 = 0;
			while (num3 < 3 && num3 <= E.time)
			{
				meshImg.base_x = (mesh.base_x = this.get_x(num3) * 0.015625f);
				float num4 = 0f;
				float num5 = 0f;
				int num6 = this.Amilk[num3];
				float num7;
				float num8;
				float num9;
				if (num3 == E.time)
				{
					num7 = this.pushdown_level;
					num8 = num2;
					num9 = ((E.z != 0f) ? 1f : X.ZSIN2(num2, 15f));
					if (this.Aamount[num3] >= 2f || num7 >= 1f)
					{
						num4 = 0.66f + 0.33f * X.COSIT((this.Aamount[num3] > 0f) ? 11f : 20.5f);
						if (num7 >= 1f && this.Aamount[num3] == 0f)
						{
							X.IntR(X.COSIT(4.37f) * 3.5f);
							num5 = (float)X.IntR(X.COSIT(5.7f) * 3.5f);
						}
					}
				}
				else
				{
					num9 = 1f;
					num7 = this.Aamount[num3];
					if (num7 >= 2f)
					{
						num4 = 0.25f + 0.25f * X.COSIT(70f);
					}
					if (num3 == E.time - 1 && this.Aamount[E.time] == 0f)
					{
						num8 = num2;
					}
					else
					{
						num8 = 1000f;
					}
				}
				float num10 = 114f * X.ZLINE(1f - num7);
				num5 += 280f * (1f - num9) + 60f;
				mesh.Col = C32.MulA(4278190080U, 0.8f * num);
				mesh.Rect(0f, num5, 32f, 122f, false);
				mesh.ColGrd.Set(4293321927U);
				if (num3 != E.time)
				{
					mesh.ColGrd.blend(4290295723U, X.ZLINE(num8, 25f));
				}
				mesh.Col = mesh.ColGrd.mulA(num).C;
				mesh.RectBL(-15f, num5 - 60f, 30f, 1f + num10, false);
				mesh.ColGrd.White();
				if (num4 > 0f)
				{
					mesh.ColGrd.blend(4294912811U, num4);
				}
				mesh.Col = mesh.ColGrd.mulA(num).C;
				mesh.Box(0f, num5, 34f, 124f, 1f, false);
				num9 = 0f;
				if (num3 == E.time)
				{
					if (this.Aamount[num3] > 0f)
					{
						num9 = 5f - 3f * X.ZLINE(E.af, 20f);
						meshImg.ColGrd.Set((X.ANMT(2, 8f) == 0) ? ((this.Aamount[num3] >= 2f) ? 4293294457U : 4294047175U) : 4294180722U);
					}
				}
				else
				{
					num9 = 2f;
					meshImg.ColGrd.Set(4294047175U).blend((this.Aamount[num3] >= 2f) ? 4288833640U : 4292072643U, X.ZLINE(num8, 25f));
				}
				if (num9 > 0f)
				{
					meshImg.ColGrd.mulA(num);
					using (STB stb = TX.PopBld(null, 0))
					{
						using (STB stb2 = TX.PopBld(null, 0))
						{
							stb.Add("+", this.Amilk[num3], "");
							stb2.Add("ml");
							float num11 = MTRX.ChrL.DrawScaleStringTo(null, stb, 0f, 0f, num9, num9, ALIGN.LEFT, ALIGNY.TOP, false, 0f, 0f, null);
							float num12 = MTRX.ChrM.DrawScaleStringTo(null, stb2, 0f, 0f, num9, num9, ALIGN.LEFT, ALIGNY.TOP, false, 0f, 0f, null);
							float num13 = 6f * num9;
							float num14 = num11 + num13 + num12;
							meshImg.Col = meshImg.ColGrd.C;
							MTRX.ChrL.DrawScaleStringTo(meshImg, stb, -num14 * 0.5f, 0f, num9, num9, ALIGN.LEFT, ALIGNY.BOTTOM, false, 0f, 0f, null);
							float num15 = -num14 * 0.5f + num11 + num13;
							meshImg.ColGrd.Black().mulA(num);
							MTRX.ChrM.DrawBorderedScaleStringTo(meshImg, stb2, num15, 0f, num9, num9, ALIGN.LEFT, ALIGNY.BOTTOM, false, 0f, 0f, null);
						}
					}
				}
				num3++;
			}
			return true;
		}

		private void fineTxPos()
		{
			int time = this.Ef.time;
			float num = this.get_x(time);
			float num2 = this.get_y() + 60f + 6f;
			if (this.Ef.z > 0f)
			{
				num += 12f * X.COSIT(22f);
				num2 -= 120f * this.pushdown_level;
				this.Tx.alpha = X.COSIT(28f) * 0.25f + 0.75f;
			}
			else
			{
				this.Tx.alpha = 1f;
				num2 += 6f + 8f * X.Abs(X.COSIT(40f));
			}
			IN.PosP2(this.Tx.transform, num, num2);
		}

		public bool EvtWait(bool is_first = false)
		{
			if (!this.Con.isMainGame() || this.Ef == null || this.Aamount[this.Ef.time] != 0f)
			{
				this.Con.defineIsInGame();
				return false;
			}
			PR pr = this.Mp.Pr as PR;
			if (pr == null || !pr.isNormalState())
			{
				UiMgmFarmSuck.VarCon.define("_in_game", "-1", true);
				return false;
			}
			pr.lockSink(2f, true);
			return true;
		}

		public float pushdown_maxt
		{
			get
			{
				if (this.Ef == null)
				{
					return 85f;
				}
				return this.Ef.y;
			}
			set
			{
				if (this.Ef != null)
				{
					this.Ef.y = value;
				}
			}
		}

		public float pushdown_level
		{
			get
			{
				if (this.Ef == null)
				{
					return 0f;
				}
				float pushdown_maxt = this.pushdown_maxt;
				float num = this.drop_level;
				float num2 = X.ZLINE(this.Ef.z - 10f, pushdown_maxt);
				if (num >= 0.125f)
				{
					float num3 = X.ZPOW(this.Ef.z - 10f, pushdown_maxt);
					num2 = X.NI(num2, num3, X.ZSIN(num - 0.125f, 0.75f));
				}
				return num2;
			}
		}

		public float alloc_over_t
		{
			get
			{
				if (this.Ef == null)
				{
					return 0f;
				}
				float num = this.drop_level;
				return X.NI(14, 2, num);
			}
		}

		public float getSuckOverAfterGrawlRatio(NelNMgmFarmCow Cow)
		{
			return X.ZSIN(X.Scr(1f - Cow.mp_ratio, (float)Cow.hp_drop_level / (float)Cow.hp_drop_max) - 0.125f, 0.875f);
		}

		public bool isActive()
		{
			return this.Ef != null;
		}

		public bool decided_this_phase
		{
			get
			{
				return this.Ef != null && this.Aamount[this.Ef.time] > 0f;
			}
		}

		public float get_x(int i)
		{
			return IN.wh * 0.45f + (float)((-1 + i) * 80);
		}

		public float get_y()
		{
			return IN.hh * 0.52f;
		}

		public static CsvVariableContainer VarCon
		{
			get
			{
				return EV.getVariableContainer();
			}
		}

		public NelM2DBase M2D
		{
			get
			{
				return this.Con.nM2D;
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.Con.Mp;
			}
		}

		public override string ToString()
		{
			return "UiMgmFarmSuck";
		}

		private M2LpMgmFarm Con;

		private EffectItem Ef;

		private FnEffectRun FD_efDraw;

		private TextRenderer Tx;

		public bool score_added;

		private const int SUCKABLE_MAX = 3;

		private const float GAUGE_H = 120f;

		private const float GAUGE_HH = 60f;

		private const float PUSHDOWN_HOLD_MINT = 85f;

		private const float PUSHDOWN_HOLD_MAXT = 23f;

		private const float PUSHDOWN_PRECUT_T = 10f;

		private const float milk_drop_max = 0.6f;

		private float[] Aamount;

		private int[] Amilk;

		private PTCThread ThrMilkShot;

		private float mp_ratio;

		private float madded_time;

		public float drop_level;
	}
}
