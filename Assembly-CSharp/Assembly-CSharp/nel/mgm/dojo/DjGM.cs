using System;
using evt;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel.mgm.dojo
{
	public class DjGM
	{
		internal DjGM(MgmDojo _DJ)
		{
			this.DJ = _DJ;
			this.FD_DrawHk = new FnEffectRun(this.DrawHk);
			this.HK = new DjHkds(this);
			this.ARpc = new DjRPC[]
			{
				new DjRPC(this, false),
				new DjRPC(this, true)
			};
			this.VarP = new VariableP(8);
			this.Cutin = new DjCutin(this);
			this.SndVo = new SndPlayer("dojo_vo", SndPlayer.SNDTYPE.VOICE);
		}

		public void destruct()
		{
			if (this.EF != null)
			{
				this.EF.destruct();
				CameraBidingsBehaviour.UiBind.deassignPostRenderFunc(this.EfBindB);
				CameraBidingsBehaviour.UiBind.deassignPostRenderFunc(this.EfBindT);
			}
			this.SndVo.Dispose();
			this.Cutin.destruct();
		}

		internal void initEffect()
		{
			if (this.EF == null)
			{
				this.Cutin.initMaterial();
				this.SqRpc = this.DJ.Pxc.getPoseByName("rps").getSequence(0);
				this.SqTexts = this.DJ.Pxc.getPoseByName("subtitle").getSequence(0);
				this.SqBWinCutin = this.DJ.Pxc.getPoseByName("cutin_bwin").getSequence(0);
				this.SqGWinCutin = this.DJ.Pxc.getPoseByName("cutin_gwin").getSequence(0);
				this.SqBLoseCutin = this.DJ.Pxc.getPoseByName("cutin_blose").getSequence(0);
				this.SqPat = this.DJ.Pxc.getPoseByName("pattern").getSequence(0);
				this.ImgWhite = this.SqPat.getImage(1, 0);
				this.SqLife = MTRX.PxlIcon.getPoseByName("ui_heart").getSequence(0);
				this.SqGLoseFlash = this.DJ.Pxc.getPoseByName("glose_flash").getSequence(0);
				this.EF = new Effect<EffectItemNel>(this.DJ.MMRD.gameObject, 100);
				this.EF.initEffect("DJ_EF", IN.getGUICamera(), new Effect<EffectItemNel>.FnCreateEffectItem(EffectItemNel.fnCreateOneNel), EFCON_TYPE.NORMAL);
				this.EF.setLayer(IN.gui_layer, IN.gui_layer);
				this.EF.topBaseZ = -3.6499999f;
				this.EF.bottomBaseZ = -3.45f;
				this.EF.no_graphic_render = (this.EF.draw_gl_only = true);
				this.EfBindT = new CameraRenderBinderFunc("UIEF-top", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					this.EF.RenderOneSide(false, JCon.CameraProjectionTransformed, Cam, false);
					return true;
				}, this.EF.topBaseZ);
				this.EfBindB = new CameraRenderBinderFunc("UIEF-bottom", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					this.EF.RenderOneSide(true, JCon.CameraProjectionTransformed, Cam, false);
					return true;
				}, this.EF.bottomBaseZ);
				IN.getGUICamera();
				CameraBidingsBehaviour.UiBind.assignPostRenderFunc(this.EfBindT);
				CameraBidingsBehaviour.UiBind.assignPostRenderFunc(this.EfBindB);
			}
		}

		internal void initG(string _skill_key)
		{
			this.skill_key = _skill_key;
			this.lose_bits = 0U;
			this.initEffect();
			this.ARpc[0].activate();
			this.HK.clearGenerated();
			this.LifePr.reset(3, 3);
			this.changeState(DjGM.STATE.INTRO);
			if (!this.is_tuto)
			{
				this.ARpc[1].activate();
				this.DJ.initWait(MgmDojo.EWAIT.GAME_PLAYING);
				this.HkGen = this.HK.GetGen(this.skill_key) ?? this.HK.GetGen("_random");
				this.LifeEn.reset(3, this.HkGen.count);
			}
			else
			{
				this.HkGen = null;
				this.DJ.initWait(MgmDojo.EWAIT.GAME_PLAYING);
				this.LifeEn.reset(3, 0);
			}
			this.finePrFigureImage();
			IValotileSetable valotileSetable = EV.getMessageContainer() as IValotileSetable;
			if (valotileSetable != null)
			{
				this.pre_valotile = valotileSetable.use_valotile;
				valotileSetable.use_valotile = true;
			}
		}

		public void deactivate()
		{
			bool flag = this.isLoseState();
			this.changeState(DjGM.STATE.OFFLINE);
			this.DJ.initBgm(0, true, false);
			this.DJ.initBgmBlock(false);
			if (this.EF != null)
			{
				this.Cutin.deactivate();
			}
			for (int i = 0; i < 2; i++)
			{
				this.ARpc[i].deactivate();
				this.DJ.APr[i].deactivate(true);
			}
			if (flag)
			{
				this.DJ.APr[0].changeFigIdTo(1);
			}
			IValotileSetable valotileSetable = EV.getMessageContainer() as IValotileSetable;
			if (valotileSetable != null)
			{
				valotileSetable.use_valotile = this.pre_valotile;
			}
		}

		private void changeState(DjGM.STATE _stt)
		{
			DjGM.STATE state = this.stt;
			uint num = this.hand_type_bits;
			int num2 = this.clear_count;
			this.hand_type_bits = 0U;
			this.t_input_alloc = -1000f;
			float num3 = 0f;
			if (state != DjGM.STATE.INTRO)
			{
				if (state - DjGM.STATE.LOSE0 <= 2)
				{
					if (_stt != DjGM.STATE.OFFLINE)
					{
						for (int i = 0; i < 2; i++)
						{
							this.ARpc[i].Life.finishVibAnim(true);
						}
					}
					this.Cutin.deactivate(DjCutin.CUTIN_TYPE.LOSEB);
					num3 = 140f;
				}
			}
			else
			{
				this.Cutin.deactivate(DjCutin.CUTIN_TYPE.INITG);
			}
			this.stt = _stt;
			this.t_state = 0f;
			this.HkEffectDeactivate();
			DjGM.STATE state2 = this.stt;
			if (state2 <= DjGM.STATE.TUTO_PLAY3)
			{
				switch (state2)
				{
				case DjGM.STATE.INTRO:
					this.Cutin.setEffect(DjCutin.CUTIN_TYPE.INITG);
					break;
				case DjGM.STATE.PLAY_W0:
					break;
				case DjGM.STATE.PLAY0:
					this.DJ.initBgm(0, true, false);
					this.DJ.initBgmBlock(true);
					this.t_state -= num3;
					break;
				case DjGM.STATE.PLAY_W1:
					this.DJ.initBgm(-1, true, true);
					this.LifePr.cureCrack(true);
					break;
				case DjGM.STATE.PLAY1:
					this.DJ.initBgm(1, true, false);
					this.DJ.initBgmBlock(true);
					this.LifePr.cureCrack(true);
					this.t_state -= X.Mx(30f, num3);
					break;
				case DjGM.STATE.PLAY_W2:
					this.DJ.initBgm(-1, true, true);
					this.LifePr.cureCrack(true);
					break;
				case DjGM.STATE.PLAY2:
					this.DJ.initBgm(2, true, false);
					this.DJ.initBgmBlock(true);
					this.LifePr.cureCrack(true);
					this.t_state -= X.Mx(50f, num3);
					break;
				default:
					if (state2 == DjGM.STATE.TUTO_PLAY3)
					{
						this.DJ.initBgmBlock(true);
						this.HkGen = this.HK.GetGen("_tuto3");
					}
					break;
				}
			}
			else if (state2 != DjGM.STATE.TUTO_PLAY4)
			{
				if (state2 == DjGM.STATE.CLEARED)
				{
					this.DJ.initBgm(-1, true, true);
				}
			}
			else
			{
				this.HkGen = this.HK.GetGen("_tuto4");
				this.LifeEn.reset(3, this.HkGen.count);
			}
			this.clear_count = 0;
			if (this.is_tuto)
			{
				if (this.isHitableState())
				{
					this.fineTutoBoxMsg();
					EV.TutoBox.remActiveFlag("EVENT");
				}
				else
				{
					EV.TutoBox.RemText(true, false);
				}
			}
			if (this.isMainPlayState())
			{
				for (int j = 0; j < 2; j++)
				{
					this.ARpc[j].closeHitHand();
					this.DJ.APr[j].deactivate(false);
				}
				this.finePrFigureImage();
				this.HK.clearGenerated();
				if (num3 > 0f)
				{
					this.clear_count = num2;
				}
			}
			if (this.isTutoPlayState())
			{
				this.t_state = X.Mn(this.t_state, -30f);
			}
			if (this.isLoseState())
			{
				this.clear_count = num2;
				this.HkEffectDeactivate();
				this.hand_type_bits = num;
				if (this.LifePr.is_alive)
				{
					this.LifePr.addCrack(true);
				}
				for (int k = 0; k < 2; k++)
				{
					this.DJ.APr[k].initHiding();
					this.ARpc[k].deactivate();
				}
			}
			if (this.isClearedState())
			{
				this.ARpc[0].fine_tx_alpha = true;
			}
		}

		internal void finePrFigureImage()
		{
			DjFigure djFigure = this.DJ.APr[0];
			if (!this.isActive())
			{
				djFigure.deactivate(true);
				return;
			}
			if (!this.LifePr.is_alive && this.isLoseState())
			{
				if (this.loseb_pantsu_off)
				{
					djFigure.changeFigIdTo(7);
					return;
				}
				if (this.loseb_bra_off)
				{
					djFigure.changeFigIdTo(6);
					return;
				}
				djFigure.changeFigIdTo(5);
				return;
			}
			else
			{
				if (this.LifePr.get_life(true) <= 1 || this.isLoseState())
				{
					djFigure.changeFigIdTo(1);
					return;
				}
				djFigure.changeFigIdTo(0);
				return;
			}
		}

		internal void reshowRpc()
		{
			if (this.isActive() && this.isLoseState())
			{
				for (int i = 0; i < 2; i++)
				{
					this.ARpc[i].activate();
				}
			}
		}

		internal void digestPrCrack()
		{
			if (this.isActive())
			{
				if (this.LifePr.isActive())
				{
					this.LifePr.finishVibAnim(true);
				}
				if (this.LifeEn.isActive())
				{
					this.LifeEn.finishVibAnim(true);
				}
			}
		}

		private void progressBgm()
		{
			DjGM.STATE state = this.stt;
			if (state == DjGM.STATE.PLAY_W1)
			{
				this.DJ.initBgm(1, true, true);
				return;
			}
			if (state != DjGM.STATE.PLAY_W2)
			{
				return;
			}
			this.DJ.initBgm(2, true, true);
		}

		public void run(float fcnt)
		{
			if (this.SndVo.need_update_flag)
			{
				this.SndVo.UpdateAll();
			}
			DjGM.STATE state = this.stt;
			if (state != DjGM.STATE.PLAY_W1)
			{
				if (state == DjGM.STATE.PLAY_W2)
				{
					if (this.t_state >= 0f)
					{
						this.DJ.initBgm(2, true, false);
					}
				}
			}
			else if (this.t_state >= 0f)
			{
				this.DJ.initBgm(1, true, false);
			}
			if (this.stt > DjGM.STATE.INTRO)
			{
				if (this.t_input_alloc < -1000f)
				{
					this.t_input_alloc = X.Mn(this.t_input_alloc + fcnt, -1000f);
				}
				if (!this.HK.hk_generated && this.HkGen != null)
				{
					this.HK.Generate(this.HkGen, 20);
				}
				if (this.t_input_alloc > -1000f)
				{
					if (this.t_input_alloc >= 2000f)
					{
						this.t_input_alloc += X.Mn(5f, fcnt * (((this.hand_type_bits & 2048U) != 0U) ? 1.25f : 1f));
						float num = X.MMX(18f, 3600f * BGM.cur_bpm_r, 40f);
						if (this.t_input_alloc % 100f >= num)
						{
							this.t_input_alloc = (float)((int)(this.t_input_alloc / 100f) * 100 + 100);
							this.BeatAttacked(true);
						}
					}
					else if (this.t_input_alloc >= 1100f)
					{
						this.t_input_alloc -= fcnt;
					}
					if (this.t_input_alloc < 1000f)
					{
						if ((this.hand_type_bits & 2048U) == 0U && this.t_input_alloc >= 0f)
						{
							this.t_input_alloc = X.VALWALK(this.t_input_alloc, 0f, fcnt);
						}
						else
						{
							this.t_input_alloc -= fcnt;
						}
					}
					if (this.hand_not_inputted)
					{
						if (IN.isLP(1))
						{
							this.hitHand(0);
						}
						else if (IN.isTP(1))
						{
							this.hitHand(2);
						}
						if (IN.isRP(1))
						{
							this.hitHand(1);
						}
					}
				}
			}
			if (this.EfHk != null && this.EfHk.z >= 0f)
			{
				this.EfHk.z += fcnt;
			}
			if (this.EF != null)
			{
				this.EF.runDrawOrRedrawMesh(X.D_EF, (float)X.AF_EF, UILog.uilog_frame_base_speed);
			}
			for (int i = 0; i < 2; i++)
			{
				this.ARpc[i].run(fcnt);
			}
		}

		public void BeatAttacked(bool on_nosound = false)
		{
			if (!this.isActive())
			{
				return;
			}
			if (on_nosound != this.t_input_alloc >= 2000f)
			{
				return;
			}
			int num = (on_nosound ? ((int)this.t_input_alloc / 100 % 4) : this.DJ.beat4);
			if (this.stt == DjGM.STATE.INTRO && this.t_state >= 80f && this.ARpc[0].full_appeared && num == 2)
			{
				this.changeState(this.is_tuto ? DjGM.STATE.TUTO_W0 : DjGM.STATE.PLAY_W0);
			}
			if ((this.hand_type_bits & 2048U) == 0U)
			{
				NEL.PadVib("mg_dojo_beat", 1f);
				if (num == 0 && this.t_input_alloc >= -1000f && this.t_input_alloc < -500f)
				{
					for (int i = 0; i < 2; i++)
					{
						this.DJ.APr[i].deactivate(false);
					}
					if (!this.isHitableState() || this.t_state < 0f)
					{
						this.ARpc[1].closeHitHand();
						return;
					}
					if (!this.HK.hk_generated && this.HkGen != null)
					{
						this.HK.Generate(this.HkGen, -1);
					}
					if (this.stt == DjGM.STATE.TUTO_PLAY3)
					{
						this.HK.cur_hand = 1;
					}
					this.ARpc[1].initHitHand(true, 255);
					this.hand_type_bits = 0U;
					this.t_input_alloc = 1100f + BGM.nextbeattiming * 0.5f;
					if (this.isTutoFirstState())
					{
						this.Cutin.setEffect(DjCutin.CUTIN_TYPE.CD2);
						SND.Ui.play("dojo_beat_appear_tuto", false);
						return;
					}
					if (!this.isFailCutinState())
					{
						this.Cutin.setEffect(DjCutin.CUTIN_TYPE.CD2);
					}
					this.EfHk = this.EF.setEffectWithSpecificFn("Hk", 0f, 0f, -1f, X.IntC(X.Mx(20f, BGM.nextbeattiming * 2f)), 0, this.FD_DrawHk);
					SND.Ui.play("dojo_beat_appear", false);
					return;
				}
				else if (this.t_input_alloc > -1000f)
				{
					if ((this.hand_type_bits & 4096U) == 0U)
					{
						if (this.t_input_alloc > 1100f && this.t_input_alloc < 2000f)
						{
							return;
						}
						this.hand_type_bits |= 4096U;
						if (this.t_input_alloc < 2000f)
						{
							this.t_input_alloc = BGM.nextbeattiming;
						}
						if (!this.isFailCutinState())
						{
							this.Cutin.setEffect(DjCutin.CUTIN_TYPE.CD1);
							return;
						}
					}
					else
					{
						this.hand_type_bits |= 2048U;
						if (this.t_input_alloc < 2000f)
						{
							this.t_input_alloc = X.Mn(this.t_input_alloc, 0f);
						}
						if (this.isTutoFirstState())
						{
							SND.Ui.play("dojo_beat_hit_tuto", false);
							this.Cutin.setEffect(DjCutin.CUTIN_TYPE.GO);
						}
						this.HkEffectDeactivate();
						if (!this.isFailCutinState())
						{
							NelMSG byId = (EV.getMessageContainer() as NelMSGContainer).GetById("djt");
							if (byId != null)
							{
								byId.hideMsg(false);
								return;
							}
						}
						else
						{
							if ((this.hand_type_bits & 288U) == 0U)
							{
								this.ARpc[1].initHitHand(false, this.HK.cur_hand);
							}
							if ((this.hand_type_bits & 256U) != 0U)
							{
								this.playVo("dojo_voice_0");
								return;
							}
						}
					}
				}
			}
			else
			{
				this.hand_type_bits &= 4294961151U;
				this.t_input_alloc = -1000f;
				this.Cutin.deactivate();
				if (!this.isFailCutinState())
				{
					this.HK.clearGenerated();
					this.HkEffectDeactivate();
					for (int j = 0; j < 2; j++)
					{
						this.ARpc[j].closeHitHand();
					}
					if (this.clear_count >= ((this.HkGen != null) ? this.HkGen.count : 4))
					{
						this.NextPhase();
						return;
					}
					if (this.hand_type_bits != 0U)
					{
						if ((this.hand_type_bits & 64U) != 0U)
						{
							this.TutorialMsg("djt_tuto_bad_wrong");
							return;
						}
						if ((this.hand_type_bits & 32U) != 0U)
						{
							this.TutorialMsg("djt_tuto_bad_fast");
							return;
						}
						if ((this.hand_type_bits & 16U) != 0U)
						{
							this.TutorialMsg("djt_tuto_bad_slow");
							return;
						}
						this.TutorialMsg("djt_tuto_good");
						return;
					}
				}
				else
				{
					for (int k = 0; k < 2; k++)
					{
						this.ARpc[k].closeHitHand();
					}
					if ((this.hand_type_bits & 256U) != 0U)
					{
						this.HK.clearGenerated();
						if (this.clear_count >= this.HkGen.count)
						{
							this.LifeEn.addCrack(true);
							EV.getVariableContainer().define("_result", "win", true);
							this.NextPhase();
							return;
						}
					}
					else
					{
						if (this.hand_not_inputted)
						{
							this.DJ.initBgm(-1, true, true);
							this.hand_type_bits |= 1040U;
						}
						if ((this.hand_type_bits & 32U) != 0U)
						{
							uint num2 = this.hand_type_bits & 15U;
							this.HK.cur_hand = ((num2 == 1U) ? 2 : ((num2 == 2U) ? 0 : 1));
						}
						DjCutin.CUTIN_TYPE cutin_TYPE;
						if ((this.hand_type_bits & 624U) != 0U && this.changeCutinLoseB(out cutin_TYPE))
						{
							if (this.is_tuto)
							{
								EV.TutoBox.RemText(true, false);
							}
							this.changeState((this.stt == DjGM.STATE.TUTO_PLAY4 || this.stt == DjGM.STATE.PLAY0) ? DjGM.STATE.LOSE0 : ((this.stt == DjGM.STATE.PLAY1) ? DjGM.STATE.LOSE1 : DjGM.STATE.LOSE2));
							bool flag = this.LifePr.get_life(true) <= 0;
							if (flag && (this.lose_bits & 512U) != 0U && (cutin_TYPE == DjCutin.CUTIN_TYPE.LOSEB_FAST || cutin_TYPE == DjCutin.CUTIN_TYPE.LOSEB_SLOW))
							{
								this.lose_bits |= 16384U;
							}
							EV.getVariableContainer().define("_lose_count", flag ? "-1" : (((this.lose_bits & (1U << (int)cutin_TYPE)) == 0U) ? "0" : "1"), true);
							EV.getVariableContainer().define("_result", cutin_TYPE.ToString(), true);
							this.Cutin.setEffect(DjCutin.CUTIN_TYPE.LOSEB);
						}
					}
				}
			}
		}

		private void NextPhase()
		{
			this.LifeEn.finishVibAnim(true);
			DjGM.STATE state = this.stt;
			switch (state)
			{
			case DjGM.STATE.PLAY0:
				this.changeState(DjGM.STATE.PLAY_W1);
				this.t_state = -85f;
				return;
			case DjGM.STATE.PLAY_W1:
			case DjGM.STATE.PLAY_W2:
				break;
			case DjGM.STATE.PLAY1:
				this.changeState(DjGM.STATE.PLAY_W2);
				this.t_state = -85f;
				return;
			case DjGM.STATE.PLAY2:
				this.changeState(DjGM.STATE.CLEARED);
				return;
			default:
				if (state == DjGM.STATE.TUTO_PLAY4)
				{
					this.changeState(DjGM.STATE.TUTO_CLEARED);
					return;
				}
				break;
			}
			if (!this.is_tuto)
			{
				this.DJ.initBgm(-1, true, true);
			}
			this.changeState(this.stt + 1);
		}

		internal bool waitHold(MgmDojo.EWAIT waitstt)
		{
			if (waitstt != MgmDojo.EWAIT.GAME_PLAYING)
			{
				return waitstt == MgmDojo.EWAIT.GAME_BGM_INIT && this.t_state < 0f;
			}
			return this.stt == DjGM.STATE.INTRO || this.isHitableState();
		}

		internal void gameProgressFromEvent()
		{
			DjGM.STATE state = this.stt;
			switch (state)
			{
			case DjGM.STATE.PLAY_W0:
			case DjGM.STATE.PLAY_W1:
			case DjGM.STATE.PLAY_W2:
			case DjGM.STATE.TUTO_W0:
			case DjGM.STATE.TUTO_W1:
			case DjGM.STATE.TUTO_W2:
			case DjGM.STATE.TUTO_W3:
			case DjGM.STATE.TUTO_W4:
				this.changeState(this.stt + 1);
				this.DJ.initWait(MgmDojo.EWAIT.GAME_PLAYING);
				return;
			case DjGM.STATE.PLAY0:
			case DjGM.STATE.PLAY1:
			case DjGM.STATE.PLAY2:
			case DjGM.STATE.TUTO_PLAY0:
			case DjGM.STATE.TUTO_PLAY1:
			case DjGM.STATE.TUTO_PLAY2:
			case DjGM.STATE.TUTO_PLAY3:
				break;
			default:
				if (state == DjGM.STATE.CLEARED)
				{
					this.Cutin.setEffect(DjCutin.CUTIN_TYPE.WING);
					return;
				}
				break;
			}
			if (this.isLoseState())
			{
				DjCutin.CUTIN_TYPE cutin_TYPE;
				if (this.changeCutinLoseB(out cutin_TYPE))
				{
					if (cutin_TYPE == DjCutin.CUTIN_TYPE.LOSEB_FAST || cutin_TYPE == DjCutin.CUTIN_TYPE.LOSEB_SLOW)
					{
						this.lose_bits |= 1536U;
					}
					else
					{
						this.lose_bits |= 1U << (int)cutin_TYPE;
					}
				}
				if (this.LifePr.get_life(true) <= 0)
				{
					this.Cutin.setEffect(DjCutin.CUTIN_TYPE.LOSEG);
					return;
				}
				if (!this.is_tuto)
				{
					this.changeState((this.stt == DjGM.STATE.LOSE0) ? DjGM.STATE.PLAY0 : ((this.stt == DjGM.STATE.LOSE1) ? DjGM.STATE.PLAY1 : DjGM.STATE.PLAY2));
					this.DJ.initWait(MgmDojo.EWAIT.GAME_PLAYING);
				}
			}
		}

		internal bool changeCutinLoseB(out DjCutin.CUTIN_TYPE type)
		{
			type = DjCutin.CUTIN_TYPE.OFFLINE;
			if (!this.isActive())
			{
				return false;
			}
			if ((this.hand_type_bits & 32U) != 0U)
			{
				type = DjCutin.CUTIN_TYPE.LOSEB_FAST;
			}
			else if ((this.hand_type_bits & 16U) != 0U && (this.hand_type_bits & 1024U) == 0U)
			{
				type = DjCutin.CUTIN_TYPE.LOSEB_SLOW;
			}
			else
			{
				type = DjCutin.CUTIN_TYPE.LOSEB_RK + this.HK.cur_hand;
			}
			return true;
		}

		internal void backFromLoseState()
		{
			if (this.isLoseState())
			{
				this.changeState((this.stt == DjGM.STATE.LOSE0) ? DjGM.STATE.PLAY0 : ((this.stt == DjGM.STATE.LOSE1) ? DjGM.STATE.PLAY1 : DjGM.STATE.PLAY2));
			}
		}

		private void hitHand(int rpc)
		{
			this.hand_type_bits = (1U << rpc) | (this.hand_type_bits & 6144U);
			if (this.t_input_alloc >= 14f)
			{
				this.hand_type_bits |= 32U;
			}
			if (this.t_input_alloc < -14f)
			{
				this.hand_type_bits |= 16U;
			}
			switch (this.stt)
			{
			case DjGM.STATE.TUTO_PLAY0:
			case DjGM.STATE.TUTO_PLAY3:
				if (rpc != 0)
				{
					this.hand_type_bits |= 64U;
					goto IL_01BC;
				}
				this.hand_type_bits |= 256U;
				goto IL_01BC;
			case DjGM.STATE.TUTO_PLAY1:
				if (rpc != 1)
				{
					this.hand_type_bits |= 64U;
					goto IL_01BC;
				}
				goto IL_01BC;
			case DjGM.STATE.TUTO_PLAY2:
				if (rpc != 2)
				{
					this.hand_type_bits |= 64U;
					goto IL_01BC;
				}
				goto IL_01BC;
			}
			if ((this.hand_type_bits & 112U) == 0U)
			{
				int num = DjRPC.checkWin(rpc, this.HK.cur_hand);
				this.hand_type_bits |= ((num == 1) ? 256U : ((num == 0) ? 128U : 512U));
				NEL.PadVib("mg_dojo_hithand_same", 1f);
				if (num == 1)
				{
					NEL.PadVib("mg_dojo_hithand_win", 1f);
				}
				if (num == 0 && this.LifePr.addCrack(false))
				{
					this.hand_type_bits |= 512U;
				}
			}
			if ((this.hand_type_bits & 624U) != 0U)
			{
				this.DJ.initBgm(-1, true, true);
				this.t_input_alloc = (float)(2000 + (((this.hand_type_bits & 2048U) == 0U) ? 60 : 0));
				if (this.is_tuto)
				{
					EV.TutoBox.RemText(true, false);
				}
			}
			IL_01BC:
			bool flag = (this.hand_type_bits & 112U) > 0U;
			this.DJ.APr[0].initHitHand(flag, rpc);
			this.ARpc[0].initHitHand(flag, rpc);
			if (flag)
			{
				SND.Ui.play("dojo_hit_failure", false);
				return;
			}
			this.HkEffectDeactivate();
			SND.Ui.play("dojo_send_pr", false);
			if (!this.isFailCutinState() || (this.hand_type_bits & 256U) != 0U)
			{
				this.clear_count++;
				if (this.is_tuto)
				{
					this.fineTutoBoxMsg();
				}
				if (!this.isTutoFirstState())
				{
					SND.Ui.play("dojo_hit", false);
					this.Cutin.setEffect(DjCutin.CUTIN_TYPE.WINB_RK + rpc);
					this.ARpc[1].closeHitHand();
				}
				if (this.isFailCutinState())
				{
					if ((this.hand_type_bits & 2048U) != 0U)
					{
						this.playVo("dojo_voice_0");
					}
					this.LifeEn.addCrack(false);
				}
			}
		}

		private bool DrawHk(EffectItem Ef)
		{
			bool flag = true;
			if (!this.HK.hk_generated)
			{
				flag = false;
			}
			else
			{
				float num = 1f;
				if (Ef.z >= 0f)
				{
					if (Ef != this.EfHk || Ef.z >= 8f)
					{
						flag = false;
					}
					else
					{
						num *= 1f - X.ZPOW(Ef.z, 8f);
					}
				}
				else if (Ef != this.EfHk)
				{
					return false;
				}
				if (flag)
				{
					flag = this.HK.Draw(Ef, num);
				}
			}
			if (!flag)
			{
				if (Ef == this.EfHk)
				{
					this.EfHk = null;
					this.HK.quitEffect();
				}
				return false;
			}
			return true;
		}

		private void HkEffectDeactivate()
		{
			if (this.EfHk != null)
			{
				this.EfHk.z = X.Mx(this.EfHk.z, 0f);
			}
		}

		private void TutorialMsg(string msg_label)
		{
			NelMSG nelMSG = (EV.getMessageContainer() as NelMSGContainer).createDrawer("___dojo/mg_tuto", msg_label, "", true, NelMSG.HKDS_BOUNDS.TT, NelMSG.HKDSTYPE._MAX, null, null);
			if (nelMSG != null)
			{
				nelMSG.auto_hide_time = 240f;
				nelMSG.use_valotile = true;
			}
		}

		private void fineTutoBoxMsg()
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					switch (this.stt)
					{
					case DjGM.STATE.TUTO_PLAY0:
					case DjGM.STATE.TUTO_PLAY3:
						stb2.AddTxA("Mgm_dojo_tuto_play0", false);
						goto IL_0087;
					case DjGM.STATE.TUTO_PLAY1:
						stb2.AddTxA("Mgm_dojo_tuto_play1", false);
						goto IL_0087;
					case DjGM.STATE.TUTO_PLAY2:
						stb2.AddTxA("Mgm_dojo_tuto_play2", false);
						goto IL_0087;
					case DjGM.STATE.TUTO_PLAY4:
						stb2.AddTxA("Mgm_dojo_tuto_play4", false);
						goto IL_0087;
					}
					return;
					IL_0087:
					using (STB stb3 = TX.PopBld(null, 0))
					{
						stb3.AddTxA("Mgm_dojo_tuto_play_rest", false);
						stb3.TxRpl(((this.HkGen != null) ? this.HkGen.count : 4) - this.clear_count);
						string text = stb.AddTxA("__adding", false).TxRpl(stb2).TxRpl(stb3)
							.ToString();
						EV.TutoBox.setPosition("", "B");
						EV.TutoBox.AddText(text, -1, "");
					}
				}
			}
		}

		internal void tutoShowEnemyRpc()
		{
			this.ARpc[1].activate();
			this.LifeEn.reset(3, 0);
		}

		public DjGM PtcVar(string key, float v)
		{
			this.VarP.Add(key, (double)v);
			return this;
		}

		public DjGM PtcVarS(string key, string v)
		{
			this.VarP.Add(key, v);
			return this;
		}

		public DjGM PtcVarS(string key, MGATTR v)
		{
			return this.PtcVarS(key, FEnum<MGATTR>.ToStr(v));
		}

		public PTCThread PtcST(string ptcst_name, IEfPInteractale Listener = null)
		{
			if (this.EF == null)
			{
				return null;
			}
			return this.EF.PtcST(ptcst_name, Listener, PTCThread.StFollow.NO_FOLLOW, this.VarP);
		}

		public Effect<EffectItemNel> get_EF()
		{
			return this.EF;
		}

		public void playVo(string que_key)
		{
			this.SndVo.play(que_key, false);
		}

		public string get_skill_key()
		{
			return this.skill_key;
		}

		public bool loseb_bra_off
		{
			get
			{
				return X.sensitive_level == 0 && (this.lose_bits & 512U) > 0U;
			}
		}

		public bool loseb_pantsu_off
		{
			get
			{
				return X.sensitive_level == 0 && (this.lose_bits & 16384U) > 0U;
			}
		}

		private bool hand_not_inputted
		{
			get
			{
				return (this.hand_type_bits & 4294961151U) == 0U;
			}
		}

		public DjLife LifePr
		{
			get
			{
				return this.ARpc[0].Life;
			}
		}

		public DjLife LifeEn
		{
			get
			{
				return this.ARpc[1].Life;
			}
		}

		public bool isActive()
		{
			return this.stt > DjGM.STATE.OFFLINE;
		}

		public bool is_tuto
		{
			get
			{
				return this.skill_key == "_tuto";
			}
		}

		public bool isMainPlayState()
		{
			return this.stt == DjGM.STATE.PLAY0 || this.stt == DjGM.STATE.PLAY1 || this.stt == DjGM.STATE.PLAY2;
		}

		public bool isTutoPlayState()
		{
			return this.stt == DjGM.STATE.TUTO_PLAY0 || this.stt == DjGM.STATE.TUTO_PLAY1 || this.stt == DjGM.STATE.TUTO_PLAY2 || this.stt == DjGM.STATE.TUTO_PLAY3 || this.stt == DjGM.STATE.TUTO_PLAY4;
		}

		public bool isTutoFirstState()
		{
			return this.stt == DjGM.STATE.TUTO_PLAY0 || this.stt == DjGM.STATE.TUTO_PLAY1 || this.stt == DjGM.STATE.TUTO_PLAY2;
		}

		public bool isFailCutinState()
		{
			return this.isMainPlayState() || this.stt == DjGM.STATE.TUTO_PLAY4;
		}

		public bool isLoseState()
		{
			return this.stt == DjGM.STATE.LOSE0 || this.stt == DjGM.STATE.LOSE1 || this.stt == DjGM.STATE.LOSE2;
		}

		public bool isClearedState()
		{
			return this.stt == DjGM.STATE.CLEARED || this.stt == DjGM.STATE.TUTO_CLEARED;
		}

		public bool isHitableState()
		{
			return this.isTutoPlayState() || this.isMainPlayState();
		}

		public float t_state
		{
			get
			{
				return this.DJ.t_state;
			}
			set
			{
				this.DJ.t_state = value;
			}
		}

		public MImage MI
		{
			get
			{
				return this.DJ.MI;
			}
		}

		public float tz_beat
		{
			get
			{
				return this.DJ.tz_beat;
			}
		}

		public readonly MgmDojo DJ;

		public readonly DjHkds HK;

		private const string tuto_key = "_tuto";

		private float t_input_alloc = -1000f;

		private EffectItem EfHk;

		private DjGM.STATE stt;

		private string skill_key;

		public readonly DjCutin Cutin;

		private Effect<EffectItemNel> EF;

		private CameraRenderBinderFunc EfBindT;

		private CameraRenderBinderFunc EfBindB;

		private VariableP VarP;

		private SndPlayer SndVo;

		public readonly DjRPC[] ARpc;

		private DjHkdsGenerator HkGen;

		internal PxlSequence SqRpc;

		internal PxlSequence SqTexts;

		internal PxlSequence SqBWinCutin;

		internal PxlSequence SqGWinCutin;

		internal PxlSequence SqBLoseCutin;

		internal PxlSequence SqPat;

		internal PxlImage ImgWhite;

		internal PxlSequence SqLife;

		internal PxlSequence SqGLoseFlash;

		public const int BEAT4_APPEAR = 0;

		public const int hand_fast_time = 14;

		public const int hand_slow_time = 14;

		public const float T_BGM_INIT = 85f;

		public const uint HAND_B_RK = 1U;

		public const uint HAND_B_SC = 2U;

		public const uint HAND_B_PA = 4U;

		public const uint HAND_B__HTYPE = 15U;

		public const uint HAND__SLOW = 16U;

		public const uint HAND__FAST = 32U;

		public const uint HAND__WRONG = 64U;

		public const uint HAND__SAME = 128U;

		public const uint HAND__WIN = 256U;

		public const uint HAND__LOSE = 512U;

		public const uint HAND__IGNORED = 1024U;

		public const uint HAND__NEXT_CALC = 2048U;

		public const uint HAND__NEXT_HAND = 4096U;

		public const uint HAND__PUNYO_BITS = 112U;

		public const uint HAND__LOSE_BITS = 624U;

		public const uint HAND__BEAT_BITS = 6144U;

		public uint lose_bits;

		public bool pre_valotile;

		private uint hand_type_bits;

		private const int tuto_clear_count = 4;

		private int clear_count;

		public readonly FnEffectRun FD_DrawHk;

		private const int MAX_PR_LIFE = 3;

		private const int MAX_PR_CRACK = 3;

		private const int MAX_EN_LIFE = 3;

		private enum STATE
		{
			OFFLINE,
			INTRO,
			PLAY_W0,
			PLAY0,
			PLAY_W1,
			PLAY1,
			PLAY_W2,
			PLAY2,
			TUTO_W0,
			TUTO_PLAY0,
			TUTO_W1,
			TUTO_PLAY1,
			TUTO_W2,
			TUTO_PLAY2,
			TUTO_W3,
			TUTO_PLAY3,
			TUTO_W4,
			TUTO_PLAY4,
			LOSE0,
			LOSE1,
			LOSE2,
			CLEARED,
			TUTO_CLEARED
		}
	}
}
