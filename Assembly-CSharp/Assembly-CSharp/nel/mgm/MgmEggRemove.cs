using System;
using System.Collections.Generic;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel.mgm
{
	public sealed class MgmEggRemove : MonoBehaviourAutoRun, IEventWaitListener, IEventListener, IValotileSetable
	{
		public static int EvtCacheReadS(EvReader ER, string cmd, CsvReader rER)
		{
			if (rER._1 == "INIT")
			{
				if (NEL.Instance.transform.Find("MGM_EGGRMV") == null)
				{
					MgmEggRemove mgmEggRemove = IN.CreateGobGUI(null, "MGM_EGGRMV").AddComponent<MgmEggRemove>();
					mgmEggRemove.transform.SetParent(NEL.Instance.transform, false);
					EV.addListener(mgmEggRemove);
					IN.setZAbs(mgmEggRemove.transform, -4.26f);
					mgmEggRemove.gameObject.SetActive(true);
				}
				return EV.Pics.cacheReadFor(MgmEggRemove.Aevimg_key[0]);
			}
			return 0;
		}

		private void Init(EvReader ER, string pos_key)
		{
			if (this.stt == MgmEggRemove.STATE.OFFLINE)
			{
				EV.addListener(this);
				this.total_egg = -1;
				this.item_buffer = 0;
				this.AEggCountAndGrade = new List<PrEggManager.EggInfo>();
				this.FD_fnDrawFaceCutin = new AnimateCutin.FnDrawAC(this.fnDrawFaceCutin);
				this.Pdr = new PendulumDrawer
				{
					intv_t = 60,
					accept_t = 7,
					fnDrawStone = new PendulumDrawer.FnDrawStone(this.fnDrawPendulumStone),
					draw_center_line = true
				};
				this.createUi();
				if (M2DBase.Instance != null)
				{
					this.TargetPr = M2EventCommand.EvMV as PR;
					if (this.TargetPr == null)
					{
						ER.tError("対象となるプレイヤーオブジェクトがありません。");
					}
					else
					{
						this.TargetPr.EpCon.SituCon.flushLastExSituationTemp();
						this.total_egg = this.TargetPr.EggCon.listupCountAndGrade(this.AEggCountAndGrade, true);
						this.situcon_locked = true;
						this.TargetPr.EpCon.SituCon.FlgLockWrite.Add("EGGREMOVE");
					}
					M2DBase.Instance.addValotAddition(this);
					M2DBase.Instance.loadMaterialSnd("ev_cuts_eggremove");
				}
				else
				{
					SND.loadSheets("ev_cuts_eggremove", "EGGREMOVE");
				}
				this.prepareTxKD(true);
				if (this.total_egg < 0)
				{
					this.total_egg = 5;
					this.AEggCountAndGrade.Add(new PrEggManager.EggInfo(5, 0, PrEggManager.CATEG.SLIME));
				}
				Vector4 vector;
				if (TalkDrawer.getDefinedPosition(pos_key, out vector))
				{
					IN.PosP2(this.DsMain.transform, vector.x, vector.y);
				}
			}
			this.Pdr.resetTime(0, false);
			base.gameObject.SetActive(true);
			EV.initWaitFn(this, 0);
			this.changeState(MgmEggRemove.STATE.DOING);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			EV.remListener(this);
			EV.remWaitListener(this);
			SND.unloadSheets("ev_cuts_eggremove", "EGGREMOVE");
			this.prepareTxKD(false);
			if (this.TargetPr != null)
			{
				this.TargetPr.NM2D.remValotAddition(this);
			}
			if (this.situcon_locked && this.TargetPr != null)
			{
				this.situcon_locked = false;
				this.TargetPr.EpCon.SituCon.FlgLockWrite.Rem("EGGREMOVE");
				if (this.insert_count > 0)
				{
					EpSituation situCon = this.TargetPr.EpCon.SituCon;
					situCon.clearManual(false);
					if (this.for_enjoy)
					{
						situCon.addManual("&&GM_ep_situation_eggremove_forenjoy");
					}
					else if (this.eggremoved_count == 0 && this.liq_removed_count == 0)
					{
						situCon.addManual("&&GM_ep_situation_eggremove_failure");
					}
					else
					{
						if (this.eggremoved_count > 0)
						{
							situCon.addManual("&&GM_ep_situation_eggremove[" + this.eggremoved_count.ToString() + "]");
						}
						if (this.liq_removed_count > 0)
						{
							situCon.addManual("&&GM_ep_situation_eggremove_liquid[" + this.liq_removed_count.ToString() + "]");
						}
					}
					if (this.orgasmed_count > 0)
					{
						situCon.addManual("&&GM_ep_situation_eggremove_orgasm[" + this.orgasmed_count.ToString() + "]");
					}
				}
			}
			this.DsMain = null;
		}

		private void createUi()
		{
			this.MdB = new MeshDrawer(null, 60, 200);
			this.MdB.draw_gl_only = true;
			this.MdB.activate("mdB", MTRX.MtrMeshNormal, false, MTRX.ColWhite, null);
			this.DsMain = IN.CreateGobGUI(base.gameObject, "-DsMain").AddComponent<Designer>();
			this.DsMain.WH(240f, 376f);
			this.DsMain.margin_in_lr = 26f;
			this.DsMain.margin_in_tb = 30f;
			this.DsMain.item_margin_y_px = 30f;
			this.DsMain.bgcol = C32.MulA(4278190080U, 0.7f);
			this.DsMain.use_valotile = true;
			this.DsMain.auto_destruct_when_deactivate = false;
			this.DsMain.init();
			this.FbMain = this.DsMain.addImg(new DsnDataImg
			{
				name = "main_gauge",
				swidth = this.DsMain.use_w,
				sheight = this.DsMain.use_h - 100f,
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawMainGauge)
			});
			this.DsMain.Br();
			this.FbPendulum = this.DsMain.addImg(new DsnDataImg
			{
				name = "main_pendulum",
				swidth = this.DsMain.use_w,
				sheight = 100f,
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawMainPendulum),
				html = true,
				text = TX.Get("Mgm_eggremove_KD", ""),
				aligny = ALIGNY.BOTTOM,
				text_margin_y = 12f
			});
			GameObject gameObject = IN.CreateGobGUI(base.gameObject, "-darken");
			this.MdDarken = MeshDrawer.prepareMeshRenderer(gameObject, MTRX.MtrMeshNormal, 0f, -1, null, true, true);
			IN.setZAbs(gameObject.transform, -4.15f);
			this.ValotDarken = gameObject.GetComponent<ValotileRenderer>();
		}

		public override string ToString()
		{
			return "MgmEggRemove";
		}

		private void activateDarken()
		{
			if (this.t_darken < 0f)
			{
				this.t_darken = 50f * X.ZLINE(40f + this.t_darken, 40f);
			}
		}

		private void deactivateDarken()
		{
			if (this.t_darken >= 0f)
			{
				this.t_darken = X.Mn(-1f, -40f + X.ZLINE(this.t_darken, 50f) * 40f);
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.DsMain.use_valotile;
			}
			set
			{
				if (this.DsMain == null)
				{
					return;
				}
				this.DsMain.use_valotile = value;
				this.ValotDarken.enabled = value;
			}
		}

		private void changeState(MgmEggRemove.STATE _state)
		{
			MgmEggRemove.STATE state = this.stt;
			this.stt = _state;
			if (state != MgmEggRemove.STATE.OFFLINE)
			{
				if (state == MgmEggRemove.STATE.ORGASM_WAITING)
				{
					if (this.EfOrgasm != null && this.EfOrgasm.z >= 0f)
					{
						this.EfOrgasm.z = -1f;
						this.EfOrgasm.af = 0f;
					}
					this.EfOrgasm = null;
				}
			}
			else
			{
				this.FbMain.redraw_flag = true;
			}
			this.t_state = 0f;
			if (this.DsMain != null)
			{
				this.FbMain.redraw_flag = true;
			}
			if (this.TargetPr != null)
			{
				this.TargetPr.VO.breath_key = null;
			}
			IN.clearPushDown(true);
			switch (this.stt)
			{
			case MgmEggRemove.STATE.DEACTIVATING:
				EV.getVariableContainer().define("_result", "success", true);
				EV.remListener(this);
				base.transform.SetParent(null, false);
				if (this.DsMain != null)
				{
					this.DsMain.deactivate();
					this.prepareTxKD(false);
					this.deactivateDarken();
				}
				break;
			case MgmEggRemove.STATE.DOING:
				this.DsMain.activate();
				this.activateDarken();
				this.prepareTxKD(true);
				this.egg_anim_applied = 0;
				this.gauge_timing_grade = 1f;
				this.need_redraw_gauge = true;
				this.eggpow = 0f;
				this.Pdr.snd_tick = "pendulum_tick";
				break;
			case MgmEggRemove.STATE.ORGASM:
				this.orgasmed_count++;
				break;
			case MgmEggRemove.STATE.ORGASM_WAITING:
				EV.getVariableContainer().define("_result", "orgasm", true);
				this.DsMain.deactivate();
				this.deactivateDarken();
				if (this.EfOrgasm == null && UIBase.Instance != null)
				{
					this.ImgOrgasm = EV.Pics.getPic(MgmEggRemove.Aevimg_key[X.xors(MgmEggRemove.Aevimg_key.Length)], true, true);
					this.EfOrgasm = UIBase.Instance.getEffect().setEffectWithSpecificFn("eggremove_orgasm", 0f, 0f, 0f, 0, 0, new FnEffectRun(this.fnDrawOrgasmCutin));
					UIPicture.Instance.setFade("down_b", UIPictureBase.EMSTATE.SHAMED | UIPictureBase.EMSTATE.SMASH | UIPictureBase.EMSTATE.ORGASM, false, true, false);
				}
				break;
			}
			if (this.TkKD != null)
			{
				if (this.stt != MgmEggRemove.STATE.DOING)
				{
					this.TkKD.hold_blink = false;
				}
				this.TkKD.need_fine = true;
			}
		}

		protected override bool runIRD(float fcnt)
		{
			bool flag = false;
			switch (this.stt)
			{
			case MgmEggRemove.STATE.OFFLINE:
				return true;
			case MgmEggRemove.STATE.DEACTIVATING:
				if (this.t_state >= 40f)
				{
					return true;
				}
				break;
			case MgmEggRemove.STATE.DOING:
				this.TargetPr.EpCon.run(fcnt);
				if (this.TkKD != null)
				{
					this.TkKD.hold_blink = IN.isCancelOn(0) || IN.isMenuO(0);
				}
				if (this.t_state >= 20f && this.gauge_level <= 0f && (IN.isCancelOn(20) || IN.isMenuO(20)))
				{
					this.changeState(MgmEggRemove.STATE.DEACTIVATING);
					EV.getVariableContainer().define("_result", "cancel", true);
					SND.Ui.play("cancel", false);
					return true;
				}
				if ((this.gauge_level <= 0f) ? (IN.isSubmitPD(1) && this.Pdr.Tap(out this.gauge_timing_grade, 5f, true)) : (this.t_gauge_input < 10f || IN.isSubmitOn(0)))
				{
					flag = true;
					if (this.gauge_level <= 0f)
					{
						this.insert_count++;
						this.gauge_level = 0f;
						this.t_gauge_input = 0f;
						NEL.PadVib("mg_farm_suck_new", 1f);
						this.PtcVar("timing", this.gauge_timing_grade);
						this.PtcST("ui_cuts_eggremove_mgm_insert", null);
						this.applyEpDmg(EPCATEG.VAGINA, 1f, false);
						if (this.gauge_timing_grade == 1f && UIBase.Instance != null)
						{
							UIPicture.Instance.TeCon.setBounceZoomIn(1.125f, 20f, 0);
						}
						this.t_gauge_epdmg = X.NIXP(40f, 80f);
						if (this.TargetPr != null)
						{
							this.TargetPr.playVo("mustl", false, false);
						}
						this.Pdr.snd_tick = null;
					}
					this.FbPendulum.redraw_flag = true;
					float gauge_level = this.gauge_level;
					this.gauge_level += 0.011111111f;
					this.t_gauge_input += 1f;
					if (this.gauge_level >= 1f)
					{
						NEL.PadVib("mg_farm_sucking_over", 1f);
						if (gauge_level < 1f)
						{
							SND.Ui.play("eggremove_reached", false);
						}
					}
					else
					{
						NEL.PadVib("mg_farm_sucking", 1f);
						if (X.XORSP() < 0.02f)
						{
							SND.Ui.play("absorb_guchu", false);
						}
					}
				}
				if (!flag)
				{
					this.Pdr.run(fcnt);
					this.FbPendulum.redraw_flag = true;
				}
				else
				{
					this.t_gauge_epdmg = X.VALWALK(this.t_gauge_epdmg, 0f, fcnt);
					if (this.t_gauge_epdmg <= 0f)
					{
						this.t_gauge_epdmg = X.NIXP(40f, 80f);
						if (X.XORSP() < 0.2f)
						{
							this.t_gauge_epdmg *= 0.2f;
						}
						this.applyEpDmg(EPCATEG.CANAL, X.NIL(1f, 0.1f, this.t_gauge_input, 270f), true);
						SND.Ui.play("absorb_guchu", false);
						if (this.TargetPr != null)
						{
							this.TargetPr.VO.playMgmEggRemoveVoIn();
						}
					}
				}
				break;
			case MgmEggRemove.STATE.ORGASM:
			case MgmEggRemove.STATE.FINISH:
				this.TargetPr.EpCon.run(fcnt);
				if (this.stt == MgmEggRemove.STATE.ORGASM && this.TargetPr != null && this.TargetPr.EpCon.isOrgasmInitTime())
				{
					flag = this.t_state <= 60f;
				}
				else if (this.t_state >= 120f)
				{
					this.changeState((this.stt == MgmEggRemove.STATE.ORGASM && (this.total_egg > 0 || this.liq_removed_count + this.eggremoved_count == 0)) ? MgmEggRemove.STATE.ORGASM_WAITING : MgmEggRemove.STATE.DEACTIVATING);
				}
				break;
			case MgmEggRemove.STATE.ORGASM_WAITING:
				this.TargetPr.EpCon.run(fcnt);
				if (this.t_state >= 70f)
				{
					this.t_state = 70f - X.NIXP(90f, 120f);
					if (this.EfOrgasm != null && this.EfOrgasm.z == 0f && this.TargetPr != null)
					{
						this.TargetPr.playVo("breath_aft", false, false);
					}
				}
				break;
			}
			if (this.gauge_level > 0f && !flag)
			{
				float num = X.ZPOW(this.gauge_level) * ((this.t_gauge_input > 104f) ? X.NIL(0.9f, 0.33f, this.t_gauge_input - 90f - 14f, 30f) : 1f) * this.gauge_timing_grade;
				float num2 = num;
				if (num2 >= 1f)
				{
					num2 *= 1.5f;
				}
				this.t_gauge_input = -1f;
				this.killPtc("ui_cuts_eggremove_mgm_insert");
				this.Pdr.fixCenter();
				if (this.TargetPr != null)
				{
					this.TargetPr.VO.playMgmEggRemoveVoOut();
					if (this.gauge_level >= 0.5f && this.AEggCountAndGrade.Count > 0 && !this.AEggCountAndGrade[0].is_liquid && X.XORSP() < 0.5f)
					{
						this.applyEpDmg(EPCATEG.UTERUS, 2f, true);
					}
					else
					{
						this.applyEpDmg((this.gauge_level < 0.25f) ? EPCATEG.VAGINA : ((this.gauge_level < 0.75f) ? EPCATEG.CANAL : EPCATEG.GSPOT), 1f, true);
					}
				}
				if (UIBase.Instance != null)
				{
					UIPictureBodyData.Qu.HandShake(X.NIXP(20f, 40f), X.NIXP(30f, 50f), X.NIXP(10f, 18f), 0);
				}
				this.gauge_level = X.Mn(-1f, -40f * X.saturate(this.gauge_level));
				this.eggpow += num2 + X.Mx(num, 0.25f) * X.XORSP() * 0.18f * ((this.AEggCountAndGrade.Count > 0 && this.AEggCountAndGrade[0].is_liquid) ? 1.88f : 1f);
				bool flag2 = false;
				float num3 = 0.2f + num * 0.5f;
				if (this.total_egg > 0 && this.eggpow >= 2.75f)
				{
					num3 *= 0.2f;
					flag2 = true;
					if (this.AEggCountAndGrade.Count > 0 && this.AEggCountAndGrade[0].is_liquid)
					{
						this.eggpow -= 2.75f;
					}
					else
					{
						this.eggpow = 0f;
					}
					this.total_egg--;
					while (this.AEggCountAndGrade.Count > 0)
					{
						PrEggManager.EggInfo eggInfo = this.AEggCountAndGrade[0];
						if (eggInfo.count == 0)
						{
							this.AEggCountAndGrade.RemoveAt(0);
							this.eggpow = 0f;
						}
						else
						{
							int count = (int)eggInfo.count;
							this.AddAlert(TX.GetA(eggInfo.is_liquid ? "EP_eggremove_liquid" : "EP_eggremove_one", eggInfo.getEggLocalizedTitle()));
							this.item_buffer++;
							byte b = eggInfo.count - 1;
							eggInfo.count = b;
							if (b == 0)
							{
								if (this.TargetPr != null)
								{
									this.TargetPr.EggCon.dropEggItem(eggInfo.categ, this.item_buffer, (int)eggInfo.grade, X.XORSPS() * 0.02f, -X.XORSP() * 0.025f);
								}
								this.item_buffer = 0;
								this.AEggCountAndGrade.RemoveAt(0);
								this.eggpow = 0f;
							}
							else
							{
								this.AEggCountAndGrade[0] = eggInfo;
							}
							int num4 = 0;
							if (this.TargetPr != null)
							{
								if (eggInfo.count == 0)
								{
									num4 = this.TargetPr.EggCon.Remove(eggInfo.categ);
								}
								else
								{
									num4 = this.TargetPr.EggCon.Reduce(eggInfo.categ, 1f / (float)count);
								}
								if (!eggInfo.is_liquid)
								{
									this.TargetPr.EpCon.addEggLayCount(eggInfo.categ, 1);
								}
							}
							if (eggInfo.is_liquid)
							{
								this.liq_removed_count += num4;
							}
							else
							{
								this.eggremoved_count++;
							}
							if (!(UIBase.Instance != null))
							{
								break;
							}
							UIPicture.Instance.CutinMng.applyLayingEggCutin(eggInfo.is_liquid, -1f, false, true);
							if (this.egg_anim_applied >= 2)
							{
								break;
							}
							UIPictureBodySpine uipictureBodySpine = UIPicture.Instance.getBodyData() as UIPictureBodySpine;
							if (uipictureBodySpine != null)
							{
								string text = "egg_out_" + this.egg_anim_applied.ToString();
								uipictureBodySpine.getViewer().addAnim(2 + this.egg_anim_applied, text, -1000, 0f, 1f);
								this.egg_anim_applied++;
								break;
							}
							break;
						}
					}
					if (this.total_egg <= 0 && this.stt == MgmEggRemove.STATE.DOING)
					{
						this.changeState(MgmEggRemove.STATE.FINISH);
					}
				}
				this.PtcVar("pow", num2).PtcVar("egg_removed", (float)(flag2 ? 1 : 0)).PtcST("eggremove_released", null);
				if (UIBase.Instance != null && this.TargetPr != null && !this.TargetPr.EpCon.isOrgasmInitTime() && X.XORSP() < num3)
				{
					string text2 = "cuts_eggremove";
					AnimateCutin animateCutin = UIBase.Instance.getAnimateCutin(text2);
					if (animateCutin != null)
					{
						animateCutin.restart(8f);
					}
					else
					{
						animateCutin = UIBase.Instance.PopPoolCutin(text2, this.FD_fnDrawFaceCutin, true);
						animateCutin.TS_spv = X.NIXP(0.6f, 0.875f);
						animateCutin.stencil_ref = 251;
						UIBase.Instance.CutinAssignFader(animateCutin, this.TargetPr, text2, UIPictureBase.EMSTATE.PROG2 | (((float)this.TargetPr.ep >= 700f) ? UIPictureBase.EMSTATE.PROG0 : UIPictureBase.EMSTATE.NORMAL));
					}
					IN.PosP2Abs(animateCutin.transform, -IN.wh * 0.4f, IN.hh * 0.64f);
					animateCutin.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
				}
			}
			if (this.gauge_level < -1f)
			{
				this.gauge_level = X.VALWALK(this.gauge_level, -1f, fcnt);
			}
			this.t_state += fcnt;
			if (X.D)
			{
				if (this.t_darken >= 0f)
				{
					if (this.t_darken < 50f)
					{
						this.t_darken += (float)X.AF * fcnt;
						this.redrawDarken();
					}
				}
				else if (this.t_darken > -40f)
				{
					this.t_darken -= (float)X.AF * fcnt;
					this.redrawDarken();
				}
			}
			return true;
		}

		private void applyEpDmg(EPCATEG pos, float level = 1f, bool can_execute_orgasme = true)
		{
			if (this.TargetPr == null)
			{
				return;
			}
			this.EpDmg.Clear();
			this.EpDmg.Set((int)pos, 15);
			this.EpDmg.val = X.IntC((float)((pos == EPCATEG.GSPOT) ? 45 : 20) * level);
			this.TargetPr.EpCon.applyEpDamage(this.EpDmg, null, EPCATEG_BITS._ALL, 20f, can_execute_orgasme);
			if (this.TargetPr.EpCon.isOrgasmInitTime() && this.stt != MgmEggRemove.STATE.ORGASM)
			{
				this.changeState(MgmEggRemove.STATE.ORGASM);
			}
		}

		private bool fnDrawMainGauge(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			if (this.stt == MgmEggRemove.STATE.OFFLINE)
			{
				return true;
			}
			update_meshdrawer = true;
			float sheight_px = FI.get_sheight_px();
			float swidth_px = FI.get_swidth_px();
			float num = sheight_px;
			float num2 = swidth_px * 0.5f;
			float num3 = num2 - 1.5f;
			Md.Col = Md.ColGrd.Set(4288893281U).mulA(alpha).C;
			Md.ColGrd.mulA(0f);
			Md.clear(false, false);
			Md.RectDoughnut(0f, sheight_px * 0.5f - num * 0.5f, swidth_px, num, 0f, sheight_px * 0.5f - num * 0.5f - 13.6f, swidth_px - 60f, num - 60f, false, 1f, 0f, true);
			float num4 = ((this.gauge_level < -1f) ? X.ZPOW(-this.gauge_level - 1f, 39f) : X.ZSIN(this.gauge_level));
			if (this.need_redraw_gauge)
			{
				this.need_redraw_gauge = false;
				int num5 = X.Mx(2, X.IntR(num / 30f));
				float num6 = num / (float)num5;
				this.MdB.clearSimple();
				this.MdB.Col = this.MdB.ColGrd.Set(4293366450U).C;
				this.MdB.ColGrd.mulA(0f);
				for (int i = 0; i < num5; i++)
				{
					this.MdB.PosD(0f, -num6 * (float)i, this.MdB.ColGrd);
				}
				this.MdB.PosD(0f, -num, this.MdB.ColGrd);
				float num7 = -num + (num - 34f) * X.Mx(0f, num4);
				float num8 = num3 - 50f;
				for (int j = 0; j < 20; j++)
				{
					float num9 = X.BEZIER_I(num8, num8, num3, num3, (float)j * 0.05f);
					float num10 = X.BEZIER_I(0f, -13.6f, -20.4f, -34f, (float)j * 0.05f);
					this.MdB.PosD(num9, num10, null);
				}
				float num11 = num3 - 25f - 1f;
				float num12 = (num - 34f) * (1f - num4);
				int num13 = X.Mx(1, X.IntC(num12 / 30f));
				float num14 = num12 / (float)num13;
				for (int k = 0; k <= num13; k++)
				{
					this.MdB.PosD(num3, -34f - num14 * (float)k, null);
				}
				if (num4 > 0f)
				{
					float num15 = num7 - 8f;
					float num16 = num7 - 20f + 8f;
					for (int l = 1; l < 14; l++)
					{
						float num17 = X.BEZIER_I(num3, num3, num11, num11, (float)l * 0.071428575f);
						float num18 = X.BEZIER_I(num7, num15, num16, num7 - 20f, (float)l * 0.071428575f);
						if (num18 <= -num)
						{
							break;
						}
						this.MdB.PosD(num17, num18, null);
					}
					float num19 = -34f - num12 - 20f;
					if (num19 > -num)
					{
						num12 = num + num19;
						int num20 = X.Mx(1, X.IntC(num12 / 30f));
						float num21 = num12 / (float)num20;
						for (int m = 0; m <= num20; m++)
						{
							this.MdB.PosD(num11, num19 - num21 * (float)m, null);
						}
					}
				}
				int vertexMax = this.MdB.getVertexMax();
				Vector3[] vertexArray = this.MdB.getVertexArray();
				Color32[] colorArray = this.MdB.getColorArray();
				int num22 = -1;
				for (int n = num5 + 1; n < vertexMax; n++)
				{
					Vector3 vector = vertexArray[n] * 64f;
					int num23 = X.Mn(num5 - 1, (int)(vector.y / -num6));
					if (num22 >= 0)
					{
						this.MdB.Tri(-vertexMax + num22 + 1, -vertexMax + n - 1, -vertexMax + n, false);
					}
					if (num22 != num23)
					{
						this.MdB.Tri(-vertexMax + num23 + 1, -vertexMax + num23, -vertexMax + n, false);
						num22 = num23;
					}
					float num24 = X.Mn(X.Abs(vector.y - 0f), X.Abs(vector.y + num)) * 0.033333335f;
					if (num24 < 1f)
					{
						colorArray[n] = C32.MulA(colorArray[n], num24);
					}
				}
			}
			Md.Col = C32.WMulA(alpha);
			for (int num25 = 0; num25 < 2; num25++)
			{
				Md.Identity();
				if (num25 == 0)
				{
					Md.TranslateP(-num2, sheight_px * 0.5f, false);
				}
				else
				{
					Md.Scale(-1f, 1f, false).TranslateP(num2, sheight_px * 0.5f, false);
				}
				Md.RotaTempMeshDrawer(0f, 0f, 1f, 1f, 0f, this.MdB, false, true, 0, -1, 0, -1);
			}
			float num26 = (num - 34f) * num4 - 10f;
			if (num26 > 0f)
			{
				int num27 = ((this.gauge_level_ >= 0f && X.ANMPT((int)this.gauge_level_blink_intv, 1f) < 0.5f) ? 2 : 1);
				for (int num28 = 0; num28 < num27; num28++)
				{
					Md.Identity();
					if (num28 == 0)
					{
						float num29 = 0f;
						if (this.t_gauge_input >= 90f)
						{
							num29 = X.COSI(this.t_gauge_input - 90f, this.gauge_level_blink_intv * 2f);
						}
						Md.Col = Md.ColGrd.Set(4279970776U).blend(uint.MaxValue, num29).mulA(alpha)
							.C;
						Md.ColGrd.Set(4280878976U).blend(uint.MaxValue, num29).mulA(alpha);
					}
					else
					{
						Md.Col = Md.ColGrd.Set(uint.MaxValue).mulA(alpha).C;
					}
					Md.TranslateP(X.COSIT(4.3f) * 1.5f, sheight_px * 0.5f - num + X.COSIT(6.93f) * 1.5f, false);
					if (num26 < 25f)
					{
						Md.Scale(1f, num26 / 25f, true);
						Md.Arc(0f, 0f, 25f, 0f, 3.1415927f, (float)num28);
					}
					else
					{
						if (num28 == 0)
						{
							Md.RectBLGradation(-25f, 0f, 50f, num26 - 25f, GRD.TOP2BOTTOM, false);
						}
						else
						{
							Md.BoxBL(-25f, 0f, 50f, num26 - 25f, 1f, false);
						}
						Md.Arc(0f, num26 - 25f, 25f, 0f, 3.1415927f, (float)num28);
					}
				}
			}
			return num4 == 0f || num4 == -1f;
		}

		private bool fnDrawMainPendulum(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			update_meshdrawer = true;
			if (!this.isPlayingState())
			{
				return true;
			}
			this.Pdr.drawTo(Md, 0f, 38f);
			if (this.t_gauge_input > 0f && this.gauge_timing_grade == 1f && this.t_gauge_input < 35f)
			{
				float num = X.ZSIN(this.t_gauge_input, 35f);
				Md.Col = Md.ColGrd.White().mulA(1f - num).C;
				Md.ColGrd.mulA(0f);
				this.Pdr.drawAcceptArea(Md, 0f, 38f, 10f + 34f * num, 24f + 10f * num, 0f, 1f);
				if (this.t_gauge_input < 8f)
				{
					Md.Col = Md.ColGrd.White().mulA(0.6f).C;
					this.Pdr.drawAcceptArea(Md, 0f, 38f, this.Pdr.accept_area_r + this.Pdr.accept_area_thick, 0f, 0f, 0f);
				}
			}
			return false;
		}

		private void fnDrawPendulumStone(PendulumDrawer Pdr, MeshDrawer Md, float x, float y, float agR, float pd_agR, bool is_bottom)
		{
			if (this.stt != MgmEggRemove.STATE.DOING)
			{
				return;
			}
			if (!is_bottom)
			{
				int num = (int)this.gauge_level_blink_intv;
				Md.Col = Md.ColGrd.Set(4281278680U).blend(4291947007U, (this.gauge_level <= 0f) ? 0f : (0.6f + 0.4f * X.SINIT((float)num))).C;
				Md.Rotate(agR + 1.5707964f, false).TranslateP(x, y, false);
				Md.KadomaruRect(0f, 0f, 24f, 20f, 3f, 0f, false, 0f, 0f, false);
				Md.Col = MTRX.ColWhite;
				float num2 = X.ANMPT(num, 1f);
				for (int i = 0; i < 3; i++)
				{
					Md.GT(-10f + 6f * ((float)i + num2), 0f, 5f, 10f, 2f, false, 0f, 0f);
				}
				Md.Identity();
			}
		}

		public float gauge_level_blink_intv
		{
			get
			{
				if (this.gauge_timing_grade != 1f)
				{
					return X.NI(24, 8, this.gauge_timing_grade);
				}
				return 4f;
			}
		}

		public float gauge_level
		{
			get
			{
				return this.gauge_level_;
			}
			set
			{
				if (this.gauge_level == value)
				{
					return;
				}
				float num = X.Mn(this.gauge_level_, 1f);
				this.gauge_level_ = value;
				if (X.Mn(this.gauge_level_, 1f) != num)
				{
					this.need_redraw_gauge = true;
					this.FbMain.redraw_flag = true;
				}
			}
		}

		private bool fnDrawOrgasmCutin(EffectItem Ef)
		{
			if (this.ImgOrgasm == null)
			{
				return false;
			}
			float wh = IN.wh;
			float num;
			float num2;
			if (Ef.z == 0f)
			{
				num = X.ZLINE(Ef.af, 50f);
				num2 = X.NIL(1.25f, 1f, Ef.af, 70f);
			}
			else
			{
				num = 1f - X.ZLINE(Ef.af, 30f);
				if (num <= 0f)
				{
					return false;
				}
				num2 = 1f;
			}
			num2 *= 0.5f;
			MeshDrawer mesh = Ef.GetMesh("eggremove_cutin", MTRX.getMI(this.ImgOrgasm.PF.pChar, false).getMtr(250), false);
			MeshDrawer mesh2 = Ef.GetMesh("eggremove_cutin", MTRX.getMtr(BLEND.MASK, 250), false);
			mesh.base_z -= 0.1f;
			mesh.TranslateP(-IN.wh * 0.4f, 0f, false);
			mesh2.setCurrentMatrix(mesh.getCurrentMatrix(), false);
			mesh2.Col = C32.d2c(4291085508U);
			mesh2.Rect(0f, 0f, IN.w * 0.54f * num, IN.h * 0.9f * num, false);
			num2 += X.COSIT(455f) * 0.008f;
			mesh.Col = MTRX.ColWhite;
			mesh.RotaPF(X.COSIT(320f) * 7.5f, X.COSIT(293f) * 7.5f, num2, num2, 0f, this.ImgOrgasm.PF, false, false, false, uint.MaxValue, false, 0);
			return true;
		}

		private bool fnDrawFaceCutin(AnimateCutin Cti, Map2d Mp, MeshDrawer Md, MeshDrawer MdT, float t, float anim_t, ref bool update_meshdrawer)
		{
			update_meshdrawer = true;
			if (t == 0f)
			{
				Md.setMaterial(MTRX.getMtr(BLEND.MASK, Cti.stencil_ref), false);
				Cti.getMeshRenderer(Md).sharedMaterial = Md.getMaterial();
				if (this.PP_FaceCutin == null)
				{
					StringHolder stringHolder = new StringHolder("4|-181|-50|-144|137|218|36|178|-123,0|0|0|0,8.00003051757|-0.79998397827,34,20,0,1", CsvReader.RegComma);
					this.PP_FaceCutin = new PopPolyDrawer(4);
					this.PP_FaceCutin.loadFromSH(stringHolder, 0);
				}
			}
			float num = X.ZSIN(anim_t, 15f);
			float num2 = X.ZLINE(80f - t, 35f);
			if (num2 <= 0f)
			{
				return false;
			}
			float num3 = (1f - X.ZLINE(anim_t, 40f)) * X.COSIT(48f) * 40f;
			Cti.setBase(40f + X.COSIT(420f) * 3.5f, -170f + num3 * 0.3f + X.COSIT(392f) * 3.5f, 1.25f);
			Md.clear(false, false);
			Md.Col = Md.ColGrd.Set(4294920124U).blend(4290690750U, X.ZSIN(anim_t, 25f)).C;
			this.PP_FaceCutin.t = X.NI(Cti.restarted ? 0.75f : 0.25f, 1f, num) * num2;
			this.PP_FaceCutin.drawTo(Md, 0f, num3, 0f, 1f);
			Cti.setMulColor(4294920124U, 1f - X.ZLINE(t, 35f));
			return true;
		}

		private void redrawDarken()
		{
			float num;
			if (this.t_darken >= 0f)
			{
				num = X.ZLINE(this.t_darken, 50f);
			}
			else
			{
				num = X.ZLINE(40f + this.t_darken, 40f);
				if (num <= 0f)
				{
					return;
				}
			}
			this.MdDarken.clear(false, false);
			this.MdDarken.Col = this.MdDarken.ColGrd.Set(2566914048U).mulA(num).C;
			this.MdDarken.ColGrd.mulA(0f);
			this.MdDarken.RectBLGradation(-IN.wh - 10f, IN.hh - 520f + 2f, IN.w + 20f, 520f, GRD.TOP2BOTTOM, false);
			this.MdDarken.updateForMeshRenderer(false);
		}

		public MgmEggRemove PtcVar(string key, float v)
		{
			if (UIBase.Instance != null)
			{
				UIBase.Instance.PtcVar(key, v);
			}
			return this;
		}

		public MgmEggRemove PtcVarS(string key, string v)
		{
			if (UIBase.Instance != null)
			{
				UIBase.Instance.PtcVarS(key, v);
			}
			return this;
		}

		public PTCThread PtcST(string ptcst_name, IEfPInteractale Listener = null)
		{
			if (UIBase.Instance != null)
			{
				return UIBase.Instance.PtcST(ptcst_name, Listener, PTCThread.StFollow.NO_FOLLOW);
			}
			return null;
		}

		public void killPtc(string ptcst_name)
		{
			if (UIBase.Instance != null)
			{
				UIBase.Instance.killPtc(ptcst_name, null);
			}
		}

		public UILogRow AddAlertTX(string t)
		{
			if (UILog.Instance == null)
			{
				return null;
			}
			return UILog.Instance.AddAlertTX(t, UILogRow.TYPE.ALERT_EP);
		}

		public UILogRow AddAlert(string t)
		{
			if (UILog.Instance == null)
			{
				return null;
			}
			return UILog.Instance.AddAlert(t, UILogRow.TYPE.ALERT_EP);
		}

		public bool for_enjoy
		{
			get
			{
				return this.total_egg + this.liq_removed_count + this.eggremoved_count == 0;
			}
		}

		public void prepareTxKD(bool flag)
		{
			if (flag)
			{
				if (this.TkKD == null && this.TargetPr != null)
				{
					if (this.FD_KeyDesc == null)
					{
						this.FD_KeyDesc = new TxKeyDesc.FnGetKD(this.getKD);
					}
					this.TkKD = this.TargetPr.NM2D.TxKD.AddTicket(170, this.FD_KeyDesc, this);
					this.TkKD.showable_front_ui = true;
					return;
				}
			}
			else if (this.TkKD != null)
			{
				this.TkKD = this.TkKD.destruct();
			}
		}

		public void getKD(STB Stb, object Target)
		{
			if (this.stt == MgmEggRemove.STATE.DOING || this.stt == MgmEggRemove.STATE.ORGASM)
			{
				Stb.AddTxA("Mstb_KeyHelp", false);
			}
		}

		public bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			if (rER.cmd != "MGM_EGGRMV")
			{
				return false;
			}
			string _ = rER._1;
			if (_ != null)
			{
				if (!(_ == "INIT"))
				{
					if (!(_ == "DEACTIVATE"))
					{
						if (!(_ == "DEACTIVATE_EFFECT"))
						{
							return false;
						}
						if (this.EfOrgasm != null && this.EfOrgasm.z >= 0f)
						{
							this.EfOrgasm.z = -1f;
							this.EfOrgasm.af = 0f;
						}
					}
					else
					{
						if (this.EfOrgasm != null)
						{
							this.EfOrgasm.destruct();
							this.EfOrgasm = null;
						}
						if (this.TargetPr != null)
						{
							this.prepareTxKD(false);
							this.TargetPr.NM2D.remValotAddition(this);
							this.TargetPr.VO.breath_key = null;
							if (this.TargetPr.isBenchState() && this.TargetPr.EpCon.isOrgasm())
							{
								this.TargetPr.UP.setFade("masturbate", UIPictureBase.EMSTATE.ORGASM, true, true, false);
								this.TargetPr.getAnimator().setPose("bench_must_orgasm_2", -1, false);
								this.TargetPr.getAnimator().animReset(9);
								this.TargetPr.VO.breath_key = "breath_aft";
							}
							else
							{
								UIBase.FlgEmotDefaultLock.Rem("__EVENT");
								this.TargetPr.UP.changeEmotDefault(true, true);
							}
						}
						EV.getVariableContainer().define("_liqcount", this.liq_removed_count.ToString(), true);
						EV.getVariableContainer().define("_eggcount", this.eggremoved_count.ToString(), true);
						IN.DestroyE(base.gameObject);
					}
				}
				else
				{
					this.Init(ER, rER._2);
				}
				return true;
			}
			return false;
		}

		public bool EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		public bool EvtClose(bool is_first_or_end)
		{
			if (is_first_or_end)
			{
				try
				{
					EV.remListener(this);
					IN.remRunner(this);
					IN.DestroyE(base.gameObject);
				}
				catch
				{
				}
			}
			return true;
		}

		public int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		public bool EvtMoveCheck()
		{
			return true;
		}

		public bool EvtWait(bool is_first = false)
		{
			return is_first || (this.stt != MgmEggRemove.STATE.OFFLINE && this.stt != MgmEggRemove.STATE.DEACTIVATING && this.stt != MgmEggRemove.STATE.ORGASM_WAITING);
		}

		public bool isPlayingState()
		{
			return this.stt == MgmEggRemove.STATE.FINISH || this.stt == MgmEggRemove.STATE.DOING || this.stt == MgmEggRemove.STATE.ORGASM;
		}

		public const string ev_cmd = "MGM_EGGRMV";

		public const string gameobject_name = "MGM_EGGRMV";

		public static string[] Aevimg_key = new string[] { "eggremove/1", "eggremove/2" };

		public const uint bg_evimg_fill = 4291085508U;

		private const string var_result_name = "_result";

		private const string var_liqcount_name = "_liqcount";

		private const string var_eggcount_name = "_eggcount";

		private const string result_orgasm = "orgasm";

		private const string result_success = "success";

		private const string result_cancel = "cancel";

		private PR TargetPr;

		private const int epdmg_base = 20;

		private const int epdmg_gspot = 45;

		private EpAtk EpDmg = new EpAtk(20, "eggremove");

		private MeshDrawer MdDarken;

		private ValotileRenderer ValotDarken;

		private const float MAXT_DARKEN = 50f;

		private const float MAXT_DARKEN_FADEOUT = 40f;

		private float t_darken = -40f;

		private int total_egg;

		private float t_state = -1f;

		private MgmEggRemove.STATE stt;

		private bool need_redraw_gauge;

		private List<PrEggManager.EggInfo> AEggCountAndGrade;

		private float gauge_level_ = -1f;

		private float t_gauge_input;

		private float t_gauge_epdmg;

		private const float EPDMG_APPLY_MAXT = 80f;

		private const float EPDMG_APPLY_MINT = 40f;

		private const float GAGE_FADEOUT_T = 40f;

		private const float liquid_multiple = 1.88f;

		private float gauge_timing_grade;

		private const float eggpow_perfect_multiple = 1.5f;

		private const float eggpow_liquid_multiple = 2f;

		private const float eggpow_randomize = 0.18f;

		private const float EGGPOW_MAX = 2.75f;

		private const float pendulum_h = 100f;

		private const float pendulum_y = 38f;

		public const int PENDULUM_ACCEPT_T = 14;

		public const int PENDULUM_ACCEPT_MIN_T = 5;

		public const int INSERTHOLD_ACCEPT_T = 14;

		private float eggpow;

		private int egg_anim_applied;

		private const int EGG_ANIM_INDEX = 2;

		public const float GAUGE_HOLD_MAXT = 90f;

		private const float main_w = 240f;

		private const float main_margin_h = 20f;

		private const float d_margin = 28f;

		private const float main_h = 376f;

		private Designer DsMain;

		private MeshDrawer MdB;

		private FillImageBlock FbMain;

		private FillImageBlock FbPendulum;

		private AnimateCutin.FnDrawAC FD_fnDrawFaceCutin;

		private PendulumDrawer Pdr;

		private int insert_count;

		private bool situcon_locked;

		private int orgasmed_count;

		private int eggremoved_count;

		private int liq_removed_count;

		private int item_buffer;

		private EffectItem EfOrgasm;

		private EvImg ImgOrgasm;

		private TxKeyDesc.KDTicket TkKD;

		private PopPolyDrawer PP_FaceCutin;

		private TxKeyDesc.FnGetKD FD_KeyDesc;

		private enum STATE
		{
			OFFLINE,
			DEACTIVATING,
			DOING,
			ORGASM,
			ORGASM_WAITING,
			FINISH
		}
	}
}
