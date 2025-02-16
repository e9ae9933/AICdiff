using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel.gm
{
	public class UiWmSkinController
	{
		public bool use_target_show
		{
			get
			{
				return this.AFocusTarget != null;
			}
		}

		public UiWmSkinController(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.FD_fnSwitchWA = new ButtonSkinWholeMapArea.FnSwitchWA(this.fnSwitchWA);
		}

		public void initAppear(ButtonSkinWholeMapArea _WmSkin, float _first_map_center_x = -1000f, float _first_map_center_y = -1000f)
		{
			this.WmSkin = _WmSkin;
			this.WmSkin.FD_FnSwitch += this.FD_fnSwitchWA;
			this.WmSkin.getBtn().do_not_tip_on_navi_loop = true;
			if (this.always_unselectable)
			{
				this.WmSkin.getBtn().unselectable(true);
			}
			Vector2 cursorMapPos = this.WmSkin.getCursorMapPos();
			this.first_map_center_x = ((_first_map_center_x == -1000f) ? cursorMapPos.x : _first_map_center_x);
			this.first_map_center_y = ((_first_map_center_y == -1000f) ? cursorMapPos.y : _first_map_center_x);
			if (this.auto_hide_wa_switch_tx)
			{
				this.WmSkin.setWaSwitchKDVisible(false);
			}
			if (this.use_mapname_box)
			{
				this.BxMapName = IN.CreateGob(this.WmSkin.getBtn().gameObject, "-BxMapName").AddComponent<MsgBox>();
				this.BxMapName.col(3137339392U);
				this.BxMapName.Align(ALIGN.CENTER, ALIGNY.MIDDLE);
				this.BxMapName.margin(new float[] { 10f, 4f, 10f, 4f });
				this.BxMapName.TxSize(12f);
				this.BxMapName.bkg_scale(true, false, false);
				this.BxMapName.TxCol(uint.MaxValue);
				this.BxMapName.T_SHOW = 0;
				IN.setZ(this.BxMapName.transform, -0.01f);
			}
		}

		public void initAppearAgain()
		{
			if (this.WmSkin != null)
			{
				this.WmSkin.fineCurrentQuestDepert();
				IN.addRunner(this.WmSkin);
			}
		}

		public void initEdit()
		{
			this.WmSkin.getBtn().SetChecked(true, true);
			this.WmSkin.getBtn().Select(true);
			if (!this.always_unselectable)
			{
				this.WmSkin.getBtn().unselectable(true);
			}
			if (this.auto_hide_wa_switch_tx)
			{
				this.WmSkin.setWaSwitchKDVisible(true);
			}
		}

		public void quitEdit(bool quit_dragging = true)
		{
			this.WmSkin.getBtn().SetChecked(false, true);
			if (quit_dragging)
			{
				this.WmSkin.quitDragging();
			}
			if (!this.always_unselectable)
			{
				this.WmSkin.getBtn().unselectable(false);
			}
			if (this.auto_hide_wa_switch_tx)
			{
				this.WmSkin.setWaSwitchKDVisible(false);
			}
			if (this.BxMapName != null)
			{
				this.BxMapName.deactivate();
			}
		}

		public void quitAppear()
		{
			if (this.WmSkin != null)
			{
				this.WmSkin.quitDragging();
				this.WmSkin.fineWaFading();
				this.WmSkin.FD_FnSwitch -= this.FD_fnSwitchWA;
				if (this.WmSkin.is_wa)
				{
					this.WmSkin.switchWA(false, false, true);
					this.WmSkin.fineWaFading();
				}
				if (this.WmSkin.getWholeMapTarget() != this.M2D.WM.CurWM)
				{
					this.WmSkin.setWholeMapTarget(this.M2D.WM.CurWM, this.first_map_center_x, this.first_map_center_y);
				}
				if (this.can_use_fasttravel && !this.WmSkin.fast_travel_active)
				{
					this.WmSkin.fast_travel_active = !this.WmSkin.fast_travel_active;
				}
				IN.remRunner(this.WmSkin);
			}
		}

		public void destruct()
		{
			if (this.WmSkin != null)
			{
				IN.remRunner(this.WmSkin);
			}
		}

		public void hideMapNameBox()
		{
			if (this.BxMapName != null)
			{
				this.BxMapName.deactivate();
			}
		}

		public virtual void fineMapKD(STB Stb)
		{
			if (this.WmSkin.is_detail)
			{
				if (this.use_target_show)
				{
					if (this.AFocusTarget != null && this.AFocusTarget.Count > 0)
					{
						Stb.Add("<key sort/>").AddTxA("Map_show_target", false).Add(" ");
					}
					Stb.Add("<key shift/>+");
				}
				Stb.AddTxA("GM_KD_map_current_pos", false);
				Stb.Add(" ");
				Stb.AddTxA((!this.WmSkin.is_zoomin) ? "KD_map_zoomin" : "KD_map_zoomout", false);
				return;
			}
			Stb.AddTxA("KD_map_wa_def", false).Add(" ");
			Stb.AddTxA((!this.WmSkin.is_zoomin) ? "KD_map_zoomin" : "KD_map_zoomout", false);
			if (this.WmSkin.hasWAFocus())
			{
				Stb.Add(" ").AddTxA("KD_map_show_detail", false);
			}
		}

		public virtual bool runAppearing()
		{
			if (this.WmSkin != null)
			{
				this.WmSkin.getWholeMapTarget().prepareAllVisitted(true);
			}
			return false;
		}

		public bool runEdit(float fcnt, bool handle, ref byte need_tuto_msg)
		{
			bool flag = true;
			WholeMapItem wholeMapTarget = this.WmSkin.getWholeMapTarget();
			if (this.WmSkin.is_cancelable && this.WmSkin.is_detail && IN.isUiRemPD() && this.can_use_fasttravel && wholeMapTarget == this.M2D.WM.CurWM)
			{
				SND.Ui.play("tool_drag_init", false);
				this.WmSkin.fast_travel_active = !this.WmSkin.fast_travel_active;
				need_tuto_msg = 2;
			}
			int num = IN.cursXD(this.WmSkin.smooth_walk);
			int num2 = -IN.cursYD(this.WmSkin.smooth_walk);
			if (num != 0 || num2 != 0)
			{
				this.WmSkin.getBtn().Select(true);
			}
			if (this.WmSkin.can_handle)
			{
				if (this.WmSkin.is_wa)
				{
					if (this.WmSkin.walkCurs((float)num, (float)num2, false))
					{
						need_tuto_msg = 2;
					}
					if (IN.isUiSortPD() && (!this.use_target_show || IN.isUiShiftO()))
					{
						SND.Ui.play("cursor_gear_reset", false);
						this.WmSkin.SetCenterDefault(true, true);
						this.WmSkin.quitDragging();
					}
					if (IN.isUiAddPD())
					{
						if (!this.WmSkin.is_detail_swithing && !this.WmSkin.is_wa_fade_animation)
						{
							this.WmSkin.changeZoomInOut();
						}
					}
					else if (!IN.isUiShiftO() && !IN.isUiAddO() && IN.isSubmit())
					{
						if (this.WmSkin.switchWA(true, false, true))
						{
							need_tuto_msg = 2;
							IN.clearPushDown(true);
							SND.Ui.play("tool_drag_init", false);
							if (this.can_use_fasttravel && this.WmSkin.getWholeMapTarget() == this.M2D.WM.CurWM)
							{
								this.WmSkin.fast_travel_active = true;
							}
						}
					}
					else if (IN.isLTabPD() && this.WmSkin.switchWA(false, false, true))
					{
						need_tuto_msg = 2;
						SND.Ui.play("tool_drag_init", false);
						if (this.can_use_fasttravel && this.WmSkin.getWholeMapTarget() == this.M2D.WM.CurWM)
						{
							this.WmSkin.fast_travel_active = true;
						}
					}
				}
				else
				{
					if (this.WmSkin.walkCurs((float)num, (float)num2, false))
					{
						need_tuto_msg = 2;
					}
					if (IN.isUiSortPD() && (!this.use_target_show || IN.isUiShiftO()))
					{
						if (this.WmSkin.getWholeMapTarget() != this.M2D.WM.CurWM)
						{
							this.WmSkin.reveal(new WmDeperture(this.M2D.WM.CurWM.text_key, null), true, false);
							return true;
						}
						SND.Ui.play("cursor_gear_reset", false);
						this.WmSkin.SetCenterDefault(true, true);
						this.WmSkin.quitDragging();
						need_tuto_msg = 2;
					}
					if (IN.isUiAddPD())
					{
						if (!this.WmSkin.is_detail_swithing)
						{
							this.WmSkin.changeZoomInOut();
						}
					}
					else if (IN.isRTabPD())
					{
						this.hideMapNameBox();
						this.WmSkin.switchWA(false, false, true);
						need_tuto_msg = 2;
						SND.Ui.play("tool_drag_init", false);
					}
				}
				if (this.use_target_show && this.AFocusTarget.Count > 0 && IN.isUiSortPD() && !IN.isUiShiftO())
				{
					this.nextForcusTarget();
				}
			}
			if (this.WmSkin.scale_changed)
			{
				this.WmSkin.scale_changed = false;
				need_tuto_msg = 2;
			}
			flag = this.WmSkin.is_cancelable && flag;
			if (flag && IN.isCancel())
			{
				this.hideMapNameBox();
				return false;
			}
			if (need_tuto_msg > 0 && this.WmSkin.is_detail && this.use_mapname_box)
			{
				WholeMapItem.WMItem wmiCurPos = this.WmSkin.WmiCurPos;
				bool flag2 = false;
				if (wmiCurPos != null && wmiCurPos.visitted)
				{
					Map2d map2d = null;
					DRect drect = null;
					if (this.WmSkin.fast_travel_active)
					{
						WMIconPosition fastTravelFocused = this.WmSkin.FastTravelFocused;
						if (fastTravelFocused.valid)
						{
							map2d = fastTravelFocused.Whd.DestMap;
							drect = new DRect("", fastTravelFocused.wmx, fastTravelFocused.wmy, 0.01f, 0f, 0f);
						}
					}
					string text;
					if (map2d != null)
					{
						text = "MAP_" + map2d.key;
					}
					else
					{
						text = "MAP_" + wmiCurPos.SrcMap.key;
					}
					drect = drect ?? wmiCurPos.Rc;
					string text2 = TX.Get(text, "");
					Vector2 cursorMapPos = this.WmSkin.getCursorMapPos();
					if (TX.valid(text2))
					{
						flag2 = true;
						if (!this.BxMapName.textIs(text2))
						{
							this.BxMapName.make(text2);
						}
						this.BxMapName.activate();
						this.BxMapName.wh_anim(this.BxMapName.get_text_swidth_px(), 16f, true, false);
						float num3 = (this.WmSkin.is_zoomin ? 40f : 20f);
						Vector2 vector = this.WmSkin.Map2Upos(new Vector2(drect.cx, drect.cy), true) * 64f;
						float num4 = this.WmSkin.swidth - this.WmSkin.inner_margin * 2f - this.BxMapName.get_swidth_px() * 0.7f;
						float num5 = this.WmSkin.sheight - this.WmSkin.inner_margin * 2f;
						vector.x = X.MMX(-num4 * 0.5f, vector.x, num4 * 0.5f);
						vector.y += (float)X.MPF(this.WmSkin.Map2Upos(cursorMapPos, true).y < -num5 * 0.14f * 0.015625f) * (num3 * 0.5f * drect.height + 8f + this.BxMapName.get_sheight_px() * 0.5f);
						if (!this.BxMapName.isActive() || !vector.Equals(this.BxPos))
						{
							this.BxPos = vector;
							this.BxMapName.wh_animZero(true, false, 0.7f);
							this.BxMapName.posSetA(vector.x, vector.y, vector.x, vector.y, false);
						}
					}
				}
				if (!flag2)
				{
					this.hideMapNameBox();
				}
			}
			return true;
		}

		public void fineDragAfter(bool aborted, bool shortclick, bool doubleclick)
		{
			if (this.WmSkin.is_wa)
			{
				if (doubleclick)
				{
					this.WmSkin.Fine();
					if (this.WmSkin.hasWAFocus())
					{
						this.WmSkin.switchWA(true, false, true);
						return;
					}
				}
				else if (shortclick)
				{
					this.WmSkin.walkCurs(0f, 0f, true);
					return;
				}
			}
			else if (this.WmSkin.fast_travel_active && shortclick)
			{
				this.WmSkin.walkCurs(0f, 0f, true);
			}
		}

		public void reveal(WmDeperture Depert, bool no_first_delay = false)
		{
			if (this.WmSkin != null && (Depert.map_key != null || Depert.wm_key != null))
			{
				this.WmSkin.reveal(Depert, no_first_delay, false);
			}
		}

		public void reveal(WmPosition Depert, bool no_first_delay = false)
		{
			if (Depert.Wm != null && Depert.Wmi != null)
			{
				this.reveal(new WmDeperture(Depert.Wm.text_key, Depert.Wmi.SrcMap.key), no_first_delay);
			}
		}

		public List<WmPosition> ClearFocusTarget()
		{
			if (this.AFocusTarget == null)
			{
				this.AFocusTarget = new List<WmPosition>(1);
			}
			this.AFocusTarget.Clear();
			this.focustarget_index = 0;
			return this.AFocusTarget;
		}

		public void nextForcusTarget()
		{
			if (this.AFocusTarget == null || this.AFocusTarget.Count == 0)
			{
				return;
			}
			bool is_wa = this.WmSkin.is_wa;
			WmPosition wmPosition = this.AFocusTarget[this.focustarget_index];
			if (!is_wa)
			{
				WholeMapItem.WMItem wmiCurPos = this.WmSkin.WmiCurPos;
				int i = this.AFocusTarget.Count;
				while (i >= 0)
				{
					wmPosition = this.AFocusTarget[this.focustarget_index];
					if (wmiCurPos != wmPosition.Wmi)
					{
						break;
					}
					i--;
					this.focustarget_index = (this.focustarget_index + 1) % this.AFocusTarget.Count;
				}
			}
			this.reveal(wmPosition, true);
		}

		public void fnSwitchWA(bool to_wa)
		{
			if (to_wa)
			{
				this.hideMapNameBox();
			}
		}

		public bool isFocusingToMapArea()
		{
			return aBtn.PreSelected == this.WmSkin.getBtn();
		}

		public EnemySummoner getCurrentFocusEnemySummoner(out NightController.SummonerData NInfo, out WMIcon TargetIco)
		{
			NInfo = null;
			TargetIco = null;
			if (this.WmSkin.is_detail)
			{
				WholeMapItem.WMItem wmiCurPos = this.WmSkin.WmiCurPos;
				if (wmiCurPos != null)
				{
					WholeMapItem wholeMapTarget = this.WmSkin.getWholeMapTarget();
					wmiCurPos.SrcMap.prepared = true;
					Vector2 cursorMapPos = this.WmSkin.getCursorMapPos();
					int num = wmiCurPos.SrcMap.clms - wmiCurPos.SrcMap.crop * 2;
					int num2 = wmiCurPos.SrcMap.rows - wmiCurPos.SrcMap.crop * 2;
					WMIcon iconFor = wholeMapTarget.GetIconFor(wmiCurPos.SrcMap, wmiCurPos.SrcMap.crop + X.IntR(((float)((int)cursorMapPos.x) + 0.5f - wmiCurPos.Rc.x) / wmiCurPos.Rc.w * (float)num), X.IntR((float)wmiCurPos.SrcMap.crop + ((float)((int)cursorMapPos.y) + 0.5f - wmiCurPos.Rc.y) / wmiCurPos.Rc.h * (float)num2), WMIcon.TYPE.ENEMY, null, X.IntR(1.25f * X.Mx((float)num / wmiCurPos.Rc.w, (float)num2 / wmiCurPos.Rc.h)));
					if (iconFor != null && TX.valid(iconFor.sf_key))
					{
						NInfo = this.M2D.NightCon.GetLpInfo(iconFor.sf_key, null, false);
						if (NInfo != null && NInfo.defeat_count > 0 && !this.M2D.NightCon.isSuddenOnMap(NInfo, wmiCurPos.SrcMap))
						{
							TargetIco = iconFor;
							return NInfo.getSummoner(iconFor.sf_key);
						}
					}
				}
			}
			return null;
		}

		public readonly NelM2DBase M2D;

		private ButtonSkinWholeMapArea WmSkin;

		public bool can_use_fasttravel;

		public float first_map_center_x;

		public float first_map_center_y;

		public UiGmMapMarker Marker;

		public bool auto_hide_wa_switch_tx;

		public bool always_unselectable;

		public bool use_mapname_box;

		private MsgBox BxMapName;

		private Vector2 BxPos;

		private ButtonSkinWholeMapArea.FnSwitchWA FD_fnSwitchWA;

		private List<WmPosition> AFocusTarget;

		private int focustarget_index;
	}
}
