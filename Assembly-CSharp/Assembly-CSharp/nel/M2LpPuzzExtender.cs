using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public class M2LpPuzzExtender : NelLpRunnerEf, ILinerReceiver, IPuzzSwitchListener, IBCCEventListener, IPuzzRevertable
	{
		public M2LpPuzzExtender(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
			if (M2LpPuzzExtender.ABufCp == null)
			{
				M2LpPuzzExtender.ABufCp = new List<M2Puts>(16);
			}
			this.start_af_min = (this.start_af_max = 0);
			this.effect_2_ed = true;
			this.set_effect_on_initaction = false;
		}

		public override void initActionPre()
		{
			base.initActionPre();
			M2LpPuzzExtender.ABufCp.Clear();
			this.Mp.getAllPointMetaPutsTo(this.mapx, this.mapy, this.mapw, this.maph, M2LpPuzzExtender.ABufCp, 1UL << this.Lay.index, "extender");
			int count = M2LpPuzzExtender.ABufCp.Count;
			this.AEcp = new List<M2LpPuzzExtender.ExChip>(count);
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < count; i++)
			{
				M2Chip m2Chip = M2LpPuzzExtender.ABufCp[i] as M2Chip;
				if (m2Chip != null)
				{
					m2Chip.arrangeable = true;
					METACImg meta = m2Chip.getMeta();
					int[] dirs = meta.getDirs("extender", m2Chip.rotation, m2Chip.flip, 0);
					int num = dirs.Length;
					for (int j = 0; j < num; j++)
					{
						if (dirs[j] != -1)
						{
							AIM aim = (AIM)dirs[j];
							int num2 = 0;
							switch (aim)
							{
							case AIM.L:
								num2 = m2Chip.mapx - this.mapx;
								break;
							case AIM.T:
								num2 = m2Chip.mapy - this.mapy;
								break;
							case AIM.R:
								num2 = this.mapx + this.mapw - (m2Chip.mapx + m2Chip.clms);
								break;
							case AIM.B:
								num2 = this.mapy + this.maph - (m2Chip.mapy + m2Chip.rows);
								break;
							}
							if (num2 > 0)
							{
								this.AEcp.Add(new M2LpPuzzExtender.ExChip(this, aim, num2, m2Chip));
								flag = true;
								if (!flag2)
								{
									flag2 = true;
									string text;
									if (TX.valid(text = meta.GetS("slide_snd_key")))
									{
										this.slide_snd_key = text;
									}
									if (TX.valid(text = meta.GetS("finish_open_ptc_key")))
									{
										this.finish_open_ptc_key = text;
									}
									if (TX.valid(text = meta.GetS("finish_close_ptc_key")))
									{
										this.finish_close_ptc_key = text;
									}
								}
							}
						}
					}
				}
			}
			if (!flag)
			{
				X.de("エクステンド対象が不明 " + this.ToString(), null);
				return;
			}
			this.Mp.addBCCEventListener(this);
			this.Mp.setDangerousFlag(this.mapx, this.mapy, this.mapw, this.maph);
		}

		public override void initAction(bool normal_map)
		{
			if (this.AEcp == null)
			{
				this.initActionPre();
			}
			base.initAction(normal_map);
			if (normal_map)
			{
				string open_summoner_key = this.Meta.GetS("open_summoner");
				this.OpenerSummon = null;
				this.pre_on = this.Meta.GetB("pre_on", false);
				this.activate_switch_id = this.Meta.GetI("activate_switch_id", -1, 0);
				this.Aentry_if_not = this.Meta.Get("entry_if_not");
				bool flag = false;
				this.t_thorn = -1f;
				if (TX.valid(open_summoner_key))
				{
					this.OpenerSummon = this.Mp.getLabelPoint((M2LabelPoint V) => V is M2LpSummon && (V as M2LpSummon).reader_key == open_summoner_key) as M2LpSummon;
					this.pre_on = false;
					if (this.OpenerSummon == null)
					{
						if (open_summoner_key.IndexOf("..") >= 0)
						{
							NightController.SummonerData lpInfo = base.nM2D.NightCon.GetLpInfo(open_summoner_key, null, false);
							if (lpInfo != null)
							{
								if (lpInfo.defeat_count == 0)
								{
									flag = (this.pre_on = true);
								}
							}
							else
							{
								X.de("Summoner " + open_summoner_key + " が存在しません。他のマップのSummoner指定をしたい場合は(Map)..(summoner_key) と指定する", null);
							}
						}
						else
						{
							X.de("Summoner " + open_summoner_key + " が存在しません。他のマップのSummoner指定をしたい場合は(Map)..(summoner_key) と指定する", null);
						}
					}
					else if (this.OpenerSummon.get_defeated_count() > 0)
					{
						this.OpenerSummon = null;
						this.Aentry_if_not = null;
					}
					else
					{
						flag = (this.pre_on = true);
					}
				}
				this.immediate_t = -1f;
				bool flag2 = this.pre_on;
				for (int i = this.AEcp.Count - 1; i >= 0; i--)
				{
					this.AEcp[i].initExtender(this.pre_on);
				}
				if (flag || this.Aentry_if_not != null)
				{
					this.runner_assigned = true;
				}
			}
		}

		public override void closeAction(bool normal_map)
		{
			base.closeAction(normal_map);
			if (this.slide_snd_cnt > 0)
			{
				this.slide_snd_cnt = 0;
				this.Mp.M2D.Snd.kill(base.unique_key);
			}
			this.runner_assigned = false;
			if (this.AEcp != null)
			{
				int count = this.AEcp.Count;
				this.Lay.checkReindex(false);
				for (int i = 0; i < count; i++)
				{
					this.AEcp[i].destruct(true);
				}
				this.Lay.checkReindex(false);
				this.Mp.considerConfig4(this.mapx, this.mapy, this.mapx + this.mapw, this.mapy + this.maph);
				this.Mp.need_update_collider = true;
				this.Mp.remBCCEventListener(this);
				this.OpenerSummon = null;
			}
		}

		private void SlideSndAssign(bool adding)
		{
			if (this.slide_snd_key == "")
			{
				return;
			}
			if (adding)
			{
				int num = this.slide_snd_cnt;
				this.slide_snd_cnt = num + 1;
				if (num == 0)
				{
					this.Mp.playSnd(this.slide_snd_key, base.unique_key, base.mapcx, base.mapcy, 1);
					return;
				}
			}
			else if (this.slide_snd_cnt > 0)
			{
				int num = this.slide_snd_cnt - 1;
				this.slide_snd_cnt = num;
				if (num == 0)
				{
					this.Mp.M2D.Snd.kill(base.unique_key);
				}
			}
		}

		public void makeSnapShot(PuzzSnapShot.RevertItem Rvi)
		{
			if (this.AEcp == null)
			{
				return;
			}
			if (this.isActive())
			{
				Rvi.time = 1;
			}
			for (int i = this.AEcp.Count - 1; i >= 0; i--)
			{
				if (this.isActive())
				{
					Rvi.time |= 1 << i;
				}
			}
		}

		public void puzzleRevert(PuzzSnapShot.RevertItem Rvi)
		{
			if (this.AEcp == null || Rvi.time != 1)
			{
				return;
			}
			this.immediate_t = -1f;
			for (int i = this.AEcp.Count - 1; i >= 0; i--)
			{
				this.AEcp[i].recheck(true);
			}
		}

		public void activateLiner(bool immediate)
		{
			if (!this.liner_enabled)
			{
				this.liner_enabled = true;
				this.immediate_t = (float)(immediate ? (-1) : 1);
				this.fineExtendState();
			}
		}

		public void deactivateLiner(bool immediate)
		{
			if (this.liner_enabled)
			{
				this.liner_enabled = false;
				this.immediate_t = (float)(immediate ? (-1) : 1);
				this.fineExtendState();
			}
		}

		private void fineExtendState()
		{
			bool flag = this.isActive();
			for (int i = this.AEcp.Count - 1; i >= 0; i--)
			{
				this.AEcp[i].SetEnable(flag, false);
			}
		}

		public void changePuzzleSwitchActivation(int id, bool activated)
		{
			if (id == this.activate_switch_id)
			{
				if (activated)
				{
					this.activateLiner(false);
					return;
				}
				this.deactivateLiner(false);
			}
		}

		public override void activate()
		{
			this.activateLiner(false);
		}

		public override void deactivate()
		{
			if (this.OpenerSummon != null && this.OpenerSummon.get_defeated_count() > 0 && this.pre_on)
			{
				this.pre_on = false;
				this.immediate_t = 0f;
				if (!this.liner_enabled)
				{
					this.fineExtendState();
				}
			}
			this.deactivateLiner(false);
		}

		public bool runner_assigned
		{
			get
			{
				return this.runner_assigned_;
			}
			set
			{
				if (this.runner_assigned == value)
				{
					return;
				}
				this.runner_assigned_ = value;
				if (value)
				{
					this.Mp.addRunnerObject(this);
					return;
				}
				this.Mp.remRunnerObject(this);
				this.Ed = null;
			}
		}

		public override bool run(float fcnt)
		{
			if (this.AEcp == null)
			{
				this.initActionPre();
			}
			if (this.Aentry_if_not != null)
			{
				for (int i = this.Aentry_if_not.Length - 1; i >= 0; i--)
				{
					M2LabelPoint point = this.Mp.getPoint(this.Aentry_if_not[i], false);
					if (point != null && point.isCoveringMover(this.Mp.Pr, base.CLEN * 3f))
					{
						this.pre_on = !this.pre_on;
						this.OpenerSummon = null;
						this.immediate_t = -1f;
						this.fineExtendState();
						break;
					}
				}
				this.Aentry_if_not = null;
			}
			if (this.OpenerSummon != null && this.t_thorn < 0f && this.isActive())
			{
				this.t_thorn = 0f;
				this.setEffect();
				if (this.Thorn != null)
				{
					this.Thorn.randomize();
				}
			}
			int count = this.AEcp.Count;
			bool flag = true;
			for (int j = count - 1; j >= 0; j--)
			{
				flag = this.AEcp[j].fineDrawPos(Map2d.TS, this.immediate) && flag;
			}
			if (this.t_thorn > 0f && this.OpenerSummon != null && this.isActive())
			{
				flag = false;
				if (this.t_thorn < this.Mp.floort)
				{
					this.t_thorn = this.Mp.floort + 20f;
					M2MoverPr keyPr = this.Mp.getKeyPr();
					if (keyPr != null && base.isCoveringMover(keyPr, base.CLEN * 2f))
					{
						this.deactivate();
					}
				}
			}
			if (flag)
			{
				this.runner_assigned_ = false;
				if (this.immediate_t < 0f)
				{
					this.immediate_t = this.Mp.floort + 1f;
				}
				return false;
			}
			return true;
		}

		public bool isActive()
		{
			return this.pre_on != this.liner_enabled;
		}

		public void BCCInitializing(M2BlockColliderContainer BCC)
		{
		}

		public bool isBCCListenerActive(M2BlockColliderContainer.BCCLine BCC)
		{
			if (!BCC.isCoveringXy((float)this.mapx, (float)this.mapy, (float)(this.mapx + this.mapw), (float)(this.mapy + this.maph), 1f, -1000f))
			{
				return false;
			}
			BCC.has_danger_another = true;
			return false;
		}

		public void BCCtouched(M2BlockColliderContainer.BCCLine BCC, M2FootManager FootD)
		{
		}

		public bool runBCCEvent(M2BlockColliderContainer.BCCLine BCC, M2FootManager FootD)
		{
			return false;
		}

		public bool initEffect(bool activating, ref DRect RcEffect)
		{
			if (RcEffect == null)
			{
				RcEffect = new DRect(base.unique_key);
			}
			RcEffect.Set((float)this.mapx, (float)this.mapy, (float)this.mapw, (float)this.maph);
			return false;
		}

		protected override bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			base.fnDrawOnEffect(Ef, Ed);
			if (!Ed.isinCamera(Ef, (float)(this.mapw / 2 + 1), (float)(this.maph / 2 + 1)))
			{
				return true;
			}
			bool flag = false;
			if (this.OpenerSummon != null)
			{
				if (this.t_thorn == 0f && this.OpenerSummon.get_defeated_count() > 0)
				{
					this.t_thorn = this.Mp.floort + 10f;
					this.runner_assigned = true;
				}
				float num = 1f;
				int count = this.AEcp.Count;
				uint num2 = 4278190080U;
				if (this.t_thorn > 0f)
				{
					if (this.isActive())
					{
						if (X.ANMT(2, 7f) == 0)
						{
							num2 = 4284178784U;
						}
					}
					else
					{
						num = 1f - X.ZLINE(this.Mp.floort - this.t_thorn, 28f);
					}
				}
				if (num > 0f)
				{
					flag = true;
				}
				MeshDrawer meshDrawer = null;
				for (int i = 0; i < count; i++)
				{
					M2LpPuzzExtender.ExChip exChip = this.AEcp[i];
					if (exChip.capacity > 0)
					{
						int capacity = exChip.capacity;
						int num3 = exChip.basex + CAim._XD(exChip.aim, 1) * capacity;
						int num4 = exChip.basey - CAim._YD(exChip.aim, 1) * capacity;
						if (this.Thorn == null)
						{
							this.Thorn = new ThornDrawer(18f);
							this.Thorn.Set(2, -70f, -50f, 500f, 800f, 16f, 25f, 16f, 20f, 4.8f, 12.8f);
							this.Thorn.gradation_behind = 0.5f;
							this.Thorn.FCol = MTRX.ColWhite;
							this.Thorn.BCol = MTRX.colb.Set(this.Thorn.FCol).mulA(0.6f).C;
						}
						float num5 = ((float)capacity + 1.5f) * base.CLENB * 0.015625f;
						bool flag2 = false;
						if (this.Thorn.totalheight_max != num5)
						{
							this.Thorn.totalheight_max = num5;
							flag2 = true;
							this.Thorn.totalheight_min = X.Mx(this.Thorn.totalheight_max - 0.46875f, this.Thorn.totalheight_max * 0.8f);
						}
						Ef.x = (float)num3 + 0.5f + 0.5f * (float)CAim._XD(exChip.aim, 1);
						Ef.y = (float)num4 + 0.5f - 0.5f * (float)CAim._YD(exChip.aim, 1);
						if (meshDrawer == null)
						{
							meshDrawer = Ef.GetMesh("", uint.MaxValue, BLEND.NORMAL, false);
							meshDrawer.Col = C32.d2c(num2);
						}
						meshDrawer.RotaTempMeshDrawer(0f, 0f, 1f, 1f, CAim.get_agR(CAim.get_opposite(exChip.aim), 0f), this.Thorn.RemakeMesh(num, flag2), false, true, 0, -1, 0, -1);
					}
				}
			}
			if (!flag)
			{
				this.Ed = null;
				this.OpenerSummon = null;
			}
			return flag;
		}

		public bool immediate
		{
			get
			{
				return this.immediate_t < 0f || (this.immediate_t > 0f && this.Mp.floort - this.immediate_t < 50f);
			}
		}

		private static List<M2Puts> ABufCp;

		private List<M2LpPuzzExtender.ExChip> AEcp;

		private bool pre_on;

		private bool liner_enabled;

		private float immediate_t;

		public const string active_rem_key = "__LNEX";

		private string slide_snd_key = "";

		private string finish_open_ptc_key = "";

		private string finish_close_ptc_key = "";

		public const float anim_move_spd = 3f;

		public const int T_QUAKE = 18;

		private int activate_switch_id = -1;

		private const float T_FADE = 28f;

		private int slide_snd_cnt;

		private bool runner_assigned_;

		private float t_thorn = -1f;

		private M2LpSummon OpenerSummon;

		private ThornDrawer Thorn;

		private string[] Aentry_if_not;

		public interface IExtenderListener
		{
			void ExtendChanged(M2LpPuzzExtender Lp, M2Chip CenterChip, AIM aim, int pre_length, int cur_length, bool animation_finished);

			void SlideDuplicated(M2LpPuzzExtender Lp, M2Chip CenterChip);
		}

		private sealed class ExChip
		{
			public ExChip(M2LpPuzzExtender _Con, AIM _aim, int _capacity, M2Chip _Cp)
			{
				this.Con = _Con;
				this.aim = _aim;
				this.capacity = _capacity;
				this.Cp = _Cp;
				this.Cp.arrangeable = true;
				this.basex = this.Cp.mapx;
				this.basey = this.Cp.mapy;
				this.AExtended = new List<M2Chip>(this.capacity);
				M2MapLayer lay = this.Cp.Lay;
				M2ChipImage m2ChipImage = _Cp.Img;
				string s = _Cp.getMeta().GetS("extender_body");
				if (TX.valid(s))
				{
					M2ChipImage m2ChipImage2;
					if ((m2ChipImage2 = this.Cp.IMGS.Get(_Cp.Img.dirname, s)) == null)
					{
						m2ChipImage2 = this.Cp.IMGS.Get(s) ?? m2ChipImage;
					}
					m2ChipImage = m2ChipImage2;
				}
				for (int i = 1; i <= this.capacity; i++)
				{
					M2Chip m2Chip = lay.MakeChip(m2ChipImage, (int)((float)this.basex * this.Cp.CLEN), (int)((float)this.basey * this.Cp.CLEN), this.Cp.opacity, this.Cp.rotation, this.Cp.flip) as M2Chip;
					m2Chip.mapx = this.basex;
					m2Chip.mapy = this.basey;
					m2Chip.arrangeable = true;
					m2Chip.inputRots(true);
					this.AExtended.Add(m2Chip);
					lay.assignNewMapChip(m2Chip, -2, true, false);
					if (m2Chip is M2LpPuzzExtender.IExtenderListener)
					{
						(m2Chip as M2LpPuzzExtender.IExtenderListener).SlideDuplicated(_Con, _Cp);
					}
					M2CImgDrawerPuzzleExtender m2CImgDrawerPuzzleExtender = m2Chip.CastDrawer<M2CImgDrawerPuzzleExtender>();
					if (m2CImgDrawerPuzzleExtender != null)
					{
						m2CImgDrawerPuzzleExtender.slideTo(this.Cp.draw_meshx, this.Cp.draw_meshy);
					}
				}
			}

			public void initExtender(bool initialize_extend)
			{
				if (this.Ext != null)
				{
					this.Ext.destruct();
				}
				this.Ext = new M2ExtenderChecker(this.Cp.ToString(), this.Cp.Mp, this.aim, 0f, 0f, 0.25f);
				this.fineShotPos();
				this.Ext.auto_descend = false;
				this.Ext.ChipCheckingFn = new Func<int, int, M2Pt, bool>(this.FnChipChecking);
				this.Ext.reachable_max = (float)this.capacity;
				this.Ext.bcc_init_auto_fine_intv = 22f;
				this.Ext.fine_interval = 22f;
				this.Ext.fnReachFined = new M2ExtenderChecker.FnReachFined(this.fnExtReachFined);
				this.SetEnable(initialize_extend, true);
			}

			private void fineShotPos()
			{
				float num;
				float num2;
				this.Ext.calcShotStartPos(this.Cp, out num, out num2, 0);
				this.Ext.shot_sx = num;
				this.Ext.shot_sy = num2;
			}

			public void SetEnable(bool v, bool force = false)
			{
				if (this.Ext == null || (!force && this.Ext.auto_ascend == v))
				{
					return;
				}
				this.Ext.auto_ascend = v;
				this.Ext.auto_descend = false;
				if (v)
				{
					this.fineShotPos();
				}
				else
				{
					this.Ext.reachable_len = 0f;
				}
				this.Ext.Fine(0f, false);
				bool immediate = this.Con.immediate;
			}

			private void fnExtReachFined(bool initted, bool changed)
			{
				if (this.Ext == null)
				{
					return;
				}
				this.Ext.auto_descend = false;
				int num = (int)X.Mn(this.Ext.reachable_len, (float)this.capacity);
				if (this.dest_reachable_len != num || initted)
				{
					int num2 = CAim._XD(this.aim, 1);
					int num3 = CAim._YD(this.aim, 1);
					this.pre_length = this.dest_reachable_len;
					if (this.dest_reachable_len < num)
					{
						for (int i = X.Mx(1, this.dest_reachable_len); i <= num; i++)
						{
							this.AExtended[i - 1].remActiveRemoveKey("__LNEX", false);
						}
						for (int j = num - 1; j >= -1; j--)
						{
							M2Chip m2Chip = ((j == -1) ? this.Cp : this.AExtended[j]);
							int num4 = 0;
							int num5 = 0;
							if (m2Chip.rows > 1 && this.aim == AIM.T)
							{
								num5 = -(m2Chip.rows - 1);
							}
							if (m2Chip.clms > 1 && this.aim == AIM.L)
							{
								num4 = -(m2Chip.clms - 1);
							}
							m2Chip.Lay.translateChip(m2Chip, this.basex + (num - 1 - j) * num2 + num4, this.basey - (num - 1 - j) * num3 + num5, true);
							if (j >= 0)
							{
								M2CImgDrawerPuzzleExtender m2CImgDrawerPuzzleExtender = m2Chip.CastDrawer<M2CImgDrawerPuzzleExtender>();
								if (m2CImgDrawerPuzzleExtender != null)
								{
									m2CImgDrawerPuzzleExtender.initExtending(this.aim, this.basex, this.basey, j + 1, this.Cp);
								}
							}
						}
					}
					else
					{
						for (int k = -1; k < this.capacity; k++)
						{
							M2Chip m2Chip2 = ((k == -1) ? this.Cp : this.AExtended[k]);
							if (m2Chip2 != null)
							{
								int num6 = ((k == -1) ? 0 : 1);
								m2Chip2.Lay.translateChip(m2Chip2, this.basex - num2 * num6, this.basey + num3 * num6, true);
								if (k >= 0)
								{
									m2Chip2.visible_when_removed = true;
									m2Chip2.addActiveRemoveKey("__LNEX", true);
								}
							}
						}
					}
					this.quake_t = 0f;
					this.Ext.fineArea((float)X.Mx(num, this.dest_reachable_len));
					this.Con.Mp.considerConfig4(this.Con);
					this.dest_reachable_len = num;
					this.Con.Mp.need_update_collider = true;
					if (this.Cp is M2LpPuzzExtender.IExtenderListener)
					{
						(this.Cp as M2LpPuzzExtender.IExtenderListener).ExtendChanged(this.Con, this.Cp, this.aim, this.pre_length, num, false);
					}
				}
				if (!this.fineDrawPos(0f, this.Con.immediate) || this.Con.immediate)
				{
					this.Con.runner_assigned = true;
				}
			}

			public bool FnChipChecking(int mapx, int mapy, M2Pt Pt)
			{
				switch (this.aim)
				{
				case AIM.L:
					if (mapy == this.Cp.mapy && X.BTWW((float)(this.basex - this.dest_reachable_len), (float)mapx, (float)this.basex))
					{
						return true;
					}
					break;
				case AIM.T:
					if (mapx == this.Cp.mapx && X.BTWW((float)(this.basey - this.dest_reachable_len), (float)mapy, (float)this.basey))
					{
						return true;
					}
					break;
				case AIM.R:
					if (mapy == this.Cp.mapy && X.BTW((float)this.basex, (float)mapx, (float)(this.basex + this.Cp.clms + this.dest_reachable_len)))
					{
						return true;
					}
					break;
				case AIM.B:
					if (mapx == this.Cp.mapx && X.BTW((float)this.basey, (float)mapy, (float)(this.basey + this.Cp.rows + this.dest_reachable_len)))
					{
						return true;
					}
					break;
				}
				return CCON.canStand(Pt.cfg);
			}

			public bool fineDrawPos(float fcnt, bool immediate = false)
			{
				if (this.Ext == null)
				{
					return false;
				}
				float num = (immediate ? ((float)(this.capacity + 64) * this.Con.Mp.CLEN) : (this.Con.Mp.CLEN / 3f));
				int num2 = (this.Ext.auto_ascend ? this.dest_reachable_len : this.capacity);
				bool flag = true;
				for (int i = -1; i < num2; i++)
				{
					M2Chip m2Chip = ((i == -1) ? this.Cp : this.AExtended[i]);
					if (m2Chip != null && (!m2Chip.active_closed || m2Chip.visible_when_removed))
					{
						M2CImgDrawerSlide m2CImgDrawerSlide = m2Chip.CastDrawer<M2CImgDrawerSlide>();
						if (m2CImgDrawerSlide != null)
						{
							if (m2CImgDrawerSlide.slideToByAnimating(m2Chip.draw_meshx, m2Chip.draw_meshy, num))
							{
								flag = false;
							}
							else if (i >= 0 && !this.Ext.auto_ascend)
							{
								m2Chip.addActiveRemoveKey("__LNEX", false);
							}
						}
					}
				}
				if (flag)
				{
					if (this.quake_t >= 18f)
					{
						return true;
					}
					int num3 = CAim._XD(this.aim, 1);
					int num4 = CAim._YD(this.aim, 1);
					float num5 = ((float)this.Cp.drawx + (float)this.Cp.iwidth * 0.5f + (float)(num3 * this.Cp.iwidth / 2)) * this.Con.Mp.rCLEN;
					float num6 = ((float)this.Cp.drawy + (float)this.Cp.iheight * 0.5f - (float)(num4 * this.Cp.iheight / 2)) * this.Con.Mp.rCLEN;
					if (this.quake_t == 0f)
					{
						this.quake_t = 0.0625f;
						this.slide_snd_assigned = false;
						if (!immediate && this.Con.Mp.floort >= 20f)
						{
							if (this.Ext.auto_ascend)
							{
								if (this.Con.finish_open_ptc_key != "")
								{
									this.Con.Mp.PtcSTsetVar("x", (double)num5).PtcSTsetVar("y", (double)num6).PtcSTsetVar("agR", (double)CAim.get_agR(this.aim, 0f))
										.PtcST(this.Con.finish_open_ptc_key, null, PTCThread.StFollow.NO_FOLLOW);
								}
							}
							else if (this.Con.finish_close_ptc_key != "")
							{
								this.Con.Mp.PtcSTsetVar("x", (double)num5).PtcSTsetVar("y", (double)num6).PtcSTsetVar("agR", (double)CAim.get_agR(this.aim, 0f))
									.PtcST(this.Con.finish_close_ptc_key, null, PTCThread.StFollow.NO_FOLLOW);
							}
						}
						if (this.Cp is M2LpPuzzExtender.IExtenderListener)
						{
							(this.Cp as M2LpPuzzExtender.IExtenderListener).ExtendChanged(this.Con, this.Cp, this.aim, this.pre_length, this.dest_reachable_len, true);
						}
					}
					if (!immediate)
					{
						this.quake_t += fcnt;
					}
					else
					{
						this.quake_t = 18f;
					}
					float num7 = X.ZPOW(18f - this.quake_t, 18f);
					for (int j = -1; j < num2; j++)
					{
						M2Chip m2Chip2 = ((j == -1) ? this.Cp : this.AExtended[j]);
						if (m2Chip2 != null && !m2Chip2.active_closed && (this.Ext.auto_ascend || j < 0))
						{
							M2CImgDrawerSlide m2CImgDrawerSlide2 = m2Chip2.CastDrawer<M2CImgDrawerSlide>();
							if (m2CImgDrawerSlide2 != null)
							{
								if (this.quake_t >= 18f)
								{
									m2CImgDrawerSlide2.quake(0f, 0f);
								}
								else
								{
									m2CImgDrawerSlide2.quake((float)(X.Abs(num3) + 3 * X.Abs(num4)) * num7, (float)(3 * X.Abs(num3) + X.Abs(num4)) * num7);
								}
							}
						}
					}
					if (this.quake_t >= 18f)
					{
						return true;
					}
				}
				else
				{
					this.quake_t = 0f;
					if (!immediate && this.Con.Mp.floort >= 20f)
					{
						this.slide_snd_assigned = true;
					}
				}
				return false;
			}

			public bool slide_snd_assigned
			{
				get
				{
					return this.slide_snd_assigned_;
				}
				set
				{
					if (this.slide_snd_assigned == value)
					{
						return;
					}
					this.slide_snd_assigned_ = value;
					this.Con.SlideSndAssign(value);
				}
			}

			public void destruct(bool no_sort)
			{
				if (this.Cp == null)
				{
					return;
				}
				int count = this.AExtended.Count;
				M2MapLayer lay = this.Cp.Lay;
				lay.translateChip(this.Cp, this.basex, this.basey, true);
				if (!no_sort)
				{
					lay.checkReindex(false);
				}
				for (int i = 0; i < count; i++)
				{
					lay.removeChip(this.AExtended[i], true, true, false);
					this.AExtended[i] = null;
				}
				if (!no_sort)
				{
					lay.checkReindex(false);
				}
				if (this.Ext != null)
				{
					this.Ext.destruct();
					this.Ext = null;
				}
				this.slide_snd_assigned_ = false;
			}

			public void recheck(bool auto_descend = true)
			{
				if (this.Ext == null)
				{
					return;
				}
				bool auto_ascend = this.Ext.auto_ascend;
				this.SetEnable(false, false);
				this.Ext.need_fine_map_bcc = true;
				this.Ext.Fine(0f, true);
				this.Ext.runner_assigned = false;
				this.Ext.run(0f);
				if (auto_ascend)
				{
					this.SetEnable(true, false);
				}
			}

			public readonly M2LpPuzzExtender Con;

			public readonly AIM aim;

			public readonly int capacity;

			public int dest_reachable_len;

			private int pre_length;

			public int basex;

			public int basey;

			public float quake_t;

			public M2Chip Cp;

			public List<M2Chip> AExtended;

			private M2ExtenderChecker Ext;

			private bool slide_snd_assigned_;
		}
	}
}
