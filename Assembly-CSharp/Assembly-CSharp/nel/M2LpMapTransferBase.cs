using System;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class M2LpMapTransferBase : NelLpRunner
	{
		public M2LpMapTransferBase(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
			this.M2D = this.Mp.M2D as NelM2DBase;
		}

		public string getNeedSFKey()
		{
			if (this.sf_key_ == null)
			{
				this.sf_key_ = "";
				M2LabelPoint labelPoint = this.Lay.getLabelPoint((M2LabelPoint Lp) => Lp.isCovering(this, 0f) && Lp is M2LpBreakable);
				if (labelPoint != null)
				{
					this.sf_key_ = (labelPoint as M2LpBreakable).sf_key;
				}
				else if (this.Lay.is_fake)
				{
					this.sf_key_ = this.Lay.fakewall_sf_key;
				}
			}
			return this.sf_key_;
		}

		public abstract void getDepertureRect();

		public override void considerConfig4(int _l, int _t, int _r, int _b, M2Pt[,] AAPt)
		{
			if (this.use_out_collider != 1 || !this.Mp.apply_chip_effect)
			{
				return;
			}
			_l = X.Mx(0, X.Mx(_l, this.mapx));
			_r = X.Mn(this.Mp.clms, X.Mn(_r, this.mapx + this.mapw));
			_t = X.Mx(0, X.Mx(_t, this.mapy));
			_b = X.Mn(this.Mp.rows, X.Mn(_b, this.mapy + this.maph));
			int crop_value = this.Mp.get_crop_value();
			int num = this.Mp.clms - crop_value;
			int num2 = this.Mp.rows - crop_value;
			for (int i = _l; i < _r; i++)
			{
				bool flag = X.BTW((float)crop_value, (float)i, (float)num);
				for (int j = _t; j < _b; j++)
				{
					if (!flag || !X.BTW((float)crop_value, (float)j, (float)num2))
					{
						CCON.calcConfigManual(ref AAPt[i, j], 128);
					}
				}
			}
		}

		public STB event_script_transfer_head(STB Stb, string aim, bool move_simulate = true, bool pre_unload_cmd = true, Map2d DepMp = null, int deperture_aim = -1, bool sync_position = false)
		{
			Stb.AR("IF summoner_active SEEK_END");
			Stb.AR("MAPTITLE_HIDE");
			string[] array = this.Meta.Get("ev_start");
			if (array != null)
			{
				Stb.AR("STOP_LETTERBOX");
				Stb.AR("_result=1");
				Stb.Add("CHANGE_EVENT2 ").addSliceJoin(array, 0, " ").Ret("\n");
				Stb.AR("IF $_result'==0' SEEK_END\n");
			}
			string text = this.Meta.GetS("sf_set");
			if (TX.valid(text))
			{
				Stb.Add("SF_SET ", text, " ").Add(this.Meta.GetI("sf_set", 1, 1)).Ret("\n");
			}
			Stb.Ret("\n");
			text = this.Meta.GetS("through_break");
			if (TX.valid(text))
			{
				Stb.AR("SF_SET ", "M2D_LP_BRK_", text, " 1");
			}
			Stb.Ret("\n");
			Stb.AR("INIT_MAP_MATERIAL ", (DepMp ?? this.DepertureRect.Mp).key, " ", this.flushing ? "2" : "1");
			int num = ((deperture_aim >= 0) ? deperture_aim : ((int)((this.DepertureRect != null) ? this.DepertureRect.getAim() : ((AIM)4294967295U))));
			if (pre_unload_cmd)
			{
				Stb.Add("SEND_EVENT_CORRUPTION ").AR("PRE_UNLOAD");
			}
			Stb.AR("#< % >");
			if (move_simulate && (num == 0 || num == 2 || num == 3))
			{
				Stb.Add("PR_KEY_SIMULATE ").Add((AIM)num).Ret("\n");
			}
			Stb.AR("STOP_LETTERBOX");
			Stb.AR("VALOTIZE");
			Stb.AR("DENY_SKIP");
			Stb.AR("LP_ACTIVATE ", this.Lay.name, " ", this.key);
			if (sync_position)
			{
				if (num == 0 || num == 2)
				{
					Stb.Add("__sync_pos=~(m2d_current_y-", this.mapy, ")/").Add(this.maph).Ret("\n");
				}
				else
				{
					Stb.Add("__sync_pos=~(m2d_current_x-", this.mapx, ")/").Add(this.mapw).Ret("\n");
				}
				Stb.AR("__sync_pos=~saturate[$__sync_pos]");
			}
			this.event_script_transfer_fadein(Stb);
			if (move_simulate && (num == 0 || num == 2))
			{
				Stb.AR("IF 'm2d_has_foot' {");
				Stb.Add("PR_KEY_SIMULATE ", (num == 0) ? "L" : "R").Ret("\n");
				Stb.AR("} ELSE {");
				Stb.AR("_crouch=~crouch[%]");
				Stb.Add("IF $_crouch'==1 || ", this.small_height ? 1 : 0, "' { \n").Ret("\n");
				Stb.AR("  PR_KEY_SIMULATE B ");
				Stb.AR("}");
				Stb.AR("}");
			}
			return Stb.Ret("\n");
		}

		public string screen_layer
		{
			get
			{
				if (!this.screen_top_layer)
				{
					return "#9";
				}
				return "&9";
			}
		}

		public virtual void event_script_transfer_fadein(STB Stb)
		{
			Stb.Add("PIC_FILL ", this.screen_layer, " ").AddColor(NEL.FillingBgCol.rgba).Ret("\n");
			Stb.AR("PIC_FADEIN ", this.screen_layer, " 10");
		}

		public void event_script_transfer_body(STB Stb, string aim_after, string jump_key, bool walk_in = false)
		{
			bool flag = aim_after == "T" || aim_after == "B";
			if (flag && walk_in)
			{
				Stb.AR("IF 'm2d_has_foot' {");
				Stb.AR("PR_KEY_SIMULATE @");
				Stb.AR("PR_KEY_SIMULATE B 0");
				Stb.Add("WAIT ", 13, "\n");
				Stb.AR("GOTO _EXECUTE_MAP_TRANSFER");
				Stb.AR("} ");
			}
			if (aim_after == "T")
			{
				Stb.AR("#MS_ % '>+[0,-30:15] >+[0,-50:15]'");
			}
			Stb.Add("WAIT ", 13, "\n");
			if (flag)
			{
				Stb.AR("#MS_ % 'q'");
			}
			Stb.AR("LABEL _EXECUTE_MAP_TRANSFER");
			if (walk_in)
			{
				Stb.AR("__walk_in_success!");
			}
			Stb.AR("WAIT_FN MAP_TRANSFER");
			Stb.Add("NEL_MAP_TRANSFER ", this.Mp.key, " ", aim_after, " ").AR(jump_key);
			this.event_script_transfer_fadeout(Stb);
		}

		public virtual void event_script_transfer_fadeout(STB Stb)
		{
			string[] array = this.Meta.Get("ev_fadeout");
			if (array != null)
			{
				Stb.Add("CHANGE_EVENT2 ").addSliceJoin(array, 0, " ").Ret("\n");
			}
			Stb.AR("IFNDEF _fadet _fadet=27");
			Stb.AR("WAIT 2");
			Stb.AR("PIC_FADEOUT " + this.screen_layer + " $_fadet");
			Stb.Add("SEND_EVENT_CORRUPTION ").AR("PRE_LOAD");
		}

		public void event_script_transfer_foot(STB Stb, STB StbInjectBeforeWaitMove = null, bool sync_pos = false)
		{
			AIM aim = this.DepertureRect.getAim();
			this.event_script_transfer_foot(Stb, (int)aim, StbInjectBeforeWaitMove, 55, sync_pos);
		}

		public void event_script_transfer_foot(STB Stb, int deperture_aim, STB StbInjectBeforeWaitMove = null, int lr_walk_w = 55, bool sync_pos = false)
		{
			Stb.Ret("\n");
			if (deperture_aim == 0 || deperture_aim == 2)
			{
				if (this.small_height)
				{
					Stb.AR("#MS_ % 'q'");
					Stb.AR(StbInjectBeforeWaitMove);
					Stb.AR("WAIT 30");
				}
				else
				{
					Stb.AR("_xspd=~walk_xspeed[%]");
					Stb.AR("_xspd=~abs[$_xspd]");
					Stb.AR("#MS_ % 'q'");
					if (sync_pos)
					{
						Stb.AR("#MS_ % '-G'");
					}
					Stb.AR("IF $_xspd' < 0.02' {");
					Stb.Add("  #MS_ % '>+[", CAim._XD(deperture_aim, 1) * lr_walk_w, ",0 <2.3 ]' ").Ret("\n");
					Stb.AR(StbInjectBeforeWaitMove);
					Stb.AR("WAIT_MOVE");
					Stb.AR("} ELSE {");
					Stb.AR("  _=~2.021/$_xspd ");
					Stb.AR(StbInjectBeforeWaitMove);
					Stb.AR("  WAIT $_ ");
					Stb.AR("}");
					if (sync_pos)
					{
						Stb.AR("#MS_ % '+G'");
					}
				}
			}
			else
			{
				Stb.AR("PR_KEY_SIMULATE @ 0");
				this.event_script_transfer_foot_TB(Stb, deperture_aim);
				Stb.AR(StbInjectBeforeWaitMove);
				Stb.AR("WAIT_MOVE");
			}
			Stb.Ret("\n");
			this.event_script_transfer_foot_evt(Stb);
		}

		protected void event_script_transfer_foot_evt(STB Stb)
		{
			string[] array = this.Meta.Get("ev_end");
			if (array != null)
			{
				Stb.Add("CHANGE_EVENT2 ").addSliceJoin(array, 0, " ").Ret("\n");
			}
			Stb.Ret("\n");
		}

		protected virtual void event_script_transfer_foot_TB(STB Stb, int deperture_aim)
		{
			if (deperture_aim == 1)
			{
				Stb.AR("#MS_ % '+G ^ P[jump~~] >+[0, -35 :6 ]>+[0, -25 :6 ]>+[0, -15:6 ]>+[0, -12 :6 ] .' ");
				return;
			}
			if (deperture_aim == 3)
			{
				Stb.AR("#MS_ % '+G  >+[0,40 :25 ] .' ");
			}
		}

		public static bool executeTransfer(Map2d SrcMp, string aim, string jump_key)
		{
			EV.stopGMain(false);
			CsvVariableContainer variableContainer = EV.getVariableContainer();
			M2LabelPoint m2LabelPoint = null;
			int num = -1;
			string text = jump_key;
			M2MoverPr m2MoverPr = null;
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			bool flag = true;
			bool flag2 = false;
			if (TX.isStart(jump_key, "!", 0))
			{
				if (nelM2DBase.curMap == null)
				{
					return false;
				}
				m2MoverPr = nelM2DBase.curMap.getKeyPr();
				if (jump_key == "!!")
				{
					flag = false;
					flag2 = true;
				}
				else
				{
					string text2 = TX.slice(jump_key, 1);
					m2LabelPoint = nelM2DBase.curMap.getLabelPoint(text2);
					if (m2LabelPoint != null)
					{
						flag = false;
						if (m2LabelPoint is M2LpMapTransferWarp)
						{
							num = (int)CAim.get_opposite((m2LabelPoint as M2LpMapTransferWarp).getDestAim());
						}
						else
						{
							num = CAim.parseString(aim, 0);
							if (m2LabelPoint is M2LpWalkDecline)
							{
							}
						}
					}
				}
			}
			else if (REG.match(jump_key, M2LpMapTransfer.RegTransferDoorLp))
			{
				if (nelM2DBase.curMap == null || nelM2DBase.curMap == SrcMp)
				{
					return false;
				}
				m2MoverPr = nelM2DBase.curMap.getKeyPr();
				m2LabelPoint = nelM2DBase.curMap.getLabelPoint(jump_key);
				if (m2LabelPoint is M2LpMapTransferDoor)
				{
					M2LpMapTransferDoor m2LpMapTransferDoor = m2LabelPoint as M2LpMapTransferDoor;
					flag = false;
					if (m2LpMapTransferDoor.walk_aim != -1)
					{
						num = (int)CAim.get_opposite((AIM)m2LpMapTransferDoor.walk_aim);
					}
				}
			}
			if (flag)
			{
				int num2 = CAim.parseString(aim, X.NmI(aim, -1, false, false));
				WholeMapItem wholeFor = nelM2DBase.WM.GetWholeFor(SrcMp, false);
				if (wholeFor == null)
				{
					return false;
				}
				WholeMapItem.WMTransferPoint.WMRectItem depertureRect = wholeFor.getDepertureRect(SrcMp, num2, jump_key);
				if (depertureRect == null)
				{
					return false;
				}
				num = (int)depertureRect.getAim();
				if (nelM2DBase.curMap != depertureRect.Mp)
				{
					nelM2DBase.changeMap(depertureRect.Mp);
				}
				text = depertureRect.getAnotherLabelPointKey();
				m2LabelPoint = depertureRect.Mp.getLabelPoint(text);
				m2MoverPr = depertureRect.Mp.getKeyPr();
				variableContainer.Get("__walk_in") == "1";
			}
			if (m2LabelPoint == null)
			{
				if (!flag2)
				{
					X.de("矩形 " + text + "が見つかりませんでした。", null);
				}
				if (m2MoverPr != null && m2MoverPr.Mp != null)
				{
					m2MoverPr.setToDefaultPosition(false, m2MoverPr.Mp);
				}
			}
			if (m2LabelPoint != null)
			{
				m2LabelPoint.activate();
				if (m2MoverPr != null)
				{
					float sizey = m2MoverPr.sizey;
					Map2d mp = m2MoverPr.Mp;
					if (num == -1)
					{
						m2MoverPr.setTo(m2LabelPoint.mapfocx, m2LabelPoint.bottom * mp.rCLEN - m2MoverPr.sizey);
					}
					else
					{
						M2LpMapTransferBase m2LpMapTransferBase = m2LabelPoint as M2LpMapTransferBase;
						float num3 = (float)CAim._XD(num, 1);
						float num4 = (float)CAim._YD(num, 1);
						bool flag3 = variableContainer.Get("__walk_in") == "1";
						float num5 = ((num3 == 0f) ? m2LabelPoint.mapfocx : ((m2LabelPoint.cx + num3 * (m2LabelPoint.w / 2f - 30f)) * mp.rCLEN));
						float num6 = ((num4 == 0f) ? (m2LabelPoint.bottom * mp.rCLEN - sizey - 0.125f) : (flag3 ? ((m2LabelPoint.cy - num4 * (m2LabelPoint.h / 2f - 22f)) * mp.rCLEN) : ((m2LabelPoint.cy - num4 * (m2LabelPoint.h / 2f - 50f)) * mp.rCLEN - 1f)));
						string text3 = variableContainer.Get("__sync_pos");
						if (text3 != null)
						{
							float num7 = X.Nm(text3, 0.5f, false);
							if (num3 != 0f)
							{
								num6 = (m2LabelPoint.y + m2LabelPoint.h * num7) * mp.rCLEN;
							}
							else
							{
								num5 = (m2LabelPoint.x + m2LabelPoint.w * num7) * mp.rCLEN;
							}
						}
						m2MoverPr.setTo(num5, num6);
						if (num3 != 0f)
						{
							int num8 = (int)(m2MoverPr.mbottom + 0.52f);
							float num9 = m2LabelPoint.Mp.getFootableY(m2MoverPr.x, num8, flag3 ? (-X.IntC(X.Mx(4f, (float)(m2LabelPoint.maph + m2LabelPoint.mapy) - m2MoverPr.mbottom))) : (-4), true, -1f, false, true, true, 0f);
							if (num9 >= 0f)
							{
								m2MoverPr.moveBy(0f, num9 - m2MoverPr.mbottom, true);
							}
							else
							{
								num9 = m2LabelPoint.Mp.getFootableY(m2MoverPr.x, m2LabelPoint.mapy, flag3 ? (-m2LabelPoint.maph) : (-4), true, -1f, false, true, true, 0f);
								if (num9 >= 0f)
								{
									m2MoverPr.moveBy(0f, num9 - m2MoverPr.mbottom, true);
								}
							}
						}
						else if ((!flag3 || !M2LpMapTransferBase.executeTransferTBWalkIn(m2MoverPr, m2LabelPoint, (int)num4)) && num4 > 0f)
						{
							float footableY = m2LabelPoint.Mp.getFootableY(m2LabelPoint.mapfocx, m2LabelPoint.mapy - 3, 3, true, -1f, false, false, true, 0f);
							if (footableY >= 0f && footableY - 1f < m2MoverPr.mtop && footableY > (float)(m2LabelPoint.mapy - 4))
							{
								m2MoverPr.moveBy(0f, footableY - 1f - m2MoverPr.mtop, false);
							}
						}
						if (m2LpMapTransferBase != null && m2LpMapTransferBase.Meta.GetB("stop_run", false))
						{
							m2MoverPr.stopRunning(false, false);
						}
					}
				}
			}
			nelM2DBase.Cam.fineImmediately();
			return true;
		}

		public static bool executeTransferTBWalkIn(M2MoverPr Pr, M2LabelPoint DepRect, int _yd)
		{
			Map2d mp = Pr.Mp;
			int num = (int)Pr.mbottom;
			int num2 = (int)Pr.x;
			int mapx = DepRect.mapx;
			int num3 = DepRect.mapx + DepRect.mapw;
			int num4 = 0;
			int num6;
			int config;
			for (;;)
			{
				bool flag = false;
				int num5 = ((num4 == 0) ? 1 : 2);
				for (int i = 0; i < num5; i++)
				{
					num6 = num2 + X.MPF(i == 1) * num4;
					if (X.BTW((float)mapx, (float)num6, (float)num3))
					{
						flag = true;
						config = mp.getConfig(num6, num);
						if (CCON.isSlope(config))
						{
							goto Block_3;
						}
					}
				}
				if (!flag && num4 >= 3)
				{
					return false;
				}
				num4++;
			}
			Block_3:
			float tiltLevel = CCON.getTiltLevel(config);
			float num7 = X.NI(CCON.getSlopeLevel(config, false), CCON.getSlopeLevel(config, true), 0.5f);
			Pr.setTo((float)num6 + 0.5f + Pr.sizex * (float)X.MPF(tiltLevel > 0f), (float)num + num7 - Pr.sizey);
			Pr.setAim((tiltLevel > 0f == _yd < 0) ? AIM.R : AIM.L, false);
			Pr.simulateKeyDown("@", false);
			Pr.simulateKeyDown("B", false);
			Pr.simulateKeyDown("T", false);
			EV.getVariableContainer().define("__walk_in_success", (Pr.aim == AIM.R) ? "R" : "L", true);
			return true;
		}

		public static void executeTransferFastTravel(Map2d DepMp, int mapx, int mapy, int delay = 40)
		{
			EV.stopGMain(false);
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			if (nelM2DBase.curMap != DepMp)
			{
				nelM2DBase.changeMap(DepMp);
			}
			M2MoverPr keyPr = DepMp.getKeyPr();
			if (keyPr != null)
			{
				keyPr.quitMoveScript(false);
				keyPr.getPhysic().killSpeedForce(true, true, true, false, false);
				float num = DepMp.getFootableY((float)mapx + 0.5f, mapy, 10, false, -1f, false, true, true, 0f);
				Vector3 vector = M2Mover.checkDirectionWalkable(keyPr, (float)mapx + 0.5f, num, 3, true);
				num = DepMp.getFootableY(vector.x, (int)vector.y, 10, true, -1f, true, true, true, keyPr.sizex);
				keyPr.setTo(vector.x, num - keyPr.sizey - 0.001f);
				keyPr.assignMoveScript(string.Concat(new string[]
				{
					"LOAD >+[ ",
					((int)(((float)mapx + 0.5f - vector.x) * keyPr.CLEN * 0.88f)).ToString(),
					",0 :",
					delay.ToString(),
					" ]`"
				}), false);
			}
			nelM2DBase.Cam.fineImmediately();
		}

		public override bool run(float fcnt)
		{
			if (!EV.isActive(false))
			{
				this.deactivate();
			}
			return true;
		}

		public override void initAction(bool normal_map)
		{
			if (!normal_map)
			{
				return;
			}
			this.getNeedSFKey();
			base.initAction(normal_map);
			this.getDepertureRect();
			if (this.DepertureRect != null)
			{
				AIM aim = this.DepertureRect.getAim();
				if (aim == AIM.R || aim == AIM.L)
				{
					int crop_value = this.Mp.get_crop_value();
					X.MMX(crop_value, (aim == AIM.R) ? (this.mapx + this.mapw - 1) : this.mapx, this.Mp.clms - crop_value - 1);
					this.small_height = this.maph <= 2;
					return;
				}
			}
			else if (this.need_wm_rect)
			{
				X.dl("Lp " + this.key + " に Tranfer が正しくセットされていません。 WholeMap の該当レイヤーを保存して下さい。 ", null, false, false);
			}
		}

		public override void closeAction(bool when_map_close)
		{
			if (when_map_close)
			{
				this.deactivate();
			}
			if (this.Ev != null)
			{
				this.Lay.Mp.removeMover(this.Ev);
				this.Ev = null;
			}
		}

		public override void activate()
		{
			if (this.use_out_collider == 1)
			{
				this.use_out_collider = 0;
				this.Mp.addRunnerObject(this);
				this.Mp.considerConfig4(this.mapx, this.mapy, this.mapx + this.mapw, this.mapy + this.maph);
				this.Mp.need_update_collider = true;
			}
			this.fineSF();
		}

		public override void deactivate()
		{
			this.Mp.remRunnerObject(this);
			if (this.use_out_collider == 0)
			{
				this.use_out_collider = 1;
				this.Mp.considerConfig4(this.mapx, this.mapy, this.mapx + this.mapw, this.mapy + this.maph);
				this.Mp.need_update_collider = true;
			}
			this.fineSF();
		}

		public void fineSF()
		{
			if (this.Ev != null && this.sf_key_ != "")
			{
				bool flag = COOK.getSF(this.sf_key_) == 1;
				this.Ev.setExecutable(M2EventItem.CMD.STAND, flag);
				this.Ev.setExecutable(M2EventItem.CMD.CHECK, flag);
			}
		}

		protected string sf_key_;

		protected NelM2DBase M2D;

		protected M2EventItem Ev;

		protected int use_out_collider = -1;

		private bool small_height;

		protected bool flushing;

		protected bool need_wm_rect = true;

		public bool stop_run;

		protected bool screen_top_layer;

		protected WholeMapItem.WMTransferPoint.WMRectItem DepertureRect;

		public const int wait_before_time = 13;
	}
}
