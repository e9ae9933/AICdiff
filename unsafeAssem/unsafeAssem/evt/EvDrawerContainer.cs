using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using XX;

namespace evt
{
	public class EvDrawerContainer : IEventListener, IHkdsFollowableContainer
	{
		public EvDrawerContainer(MultiMeshRenderer _MMRD)
		{
			this.MMRD = _MMRD;
			this.mmrd_first_count = this.MMRD.Length;
			this.GobMdManpu = IN.CreateGob(this.MMRD.gameObject, "-manpu");
			this.MdManpu = MeshDrawer.prepareMeshRenderer(this.GobMdManpu, MTRX.MIicon.getMtr(BLEND.NORMAL, -1), 0f, -1, null, true, false);
			this.ValotManpu = this.GobMdManpu.GetComponent<ValotileRenderer>();
			this.ADrawer = new EvDrawer[16];
			this.ATalker = new TalkDrawer[6];
			this.SWin = new EvSmallWin(this, "%W0", EvDrawerContainer.LAYER.WINDOW);
			this.SortDrawer = new SORT<EvDrawer>(new Comparison<EvDrawer>(EvDrawerContainer.fnSortDrawer));
			this.PoolSpw = new ObjPool<SpineViewerBehaviour>("ManpuSpine", this.MMRD.transform, 8, 1);
			this.MtrManpu = MTRX.newMtr("Hachan/ShaderGDTC");
			this.DrRad = new RadiationDrawer();
			EV.addListener(this);
			EvDrawerContainer.createListenerEval(this);
		}

		public bool use_valotile
		{
			get
			{
				return this.MMRD.valotile_enabled;
			}
			set
			{
				if (value)
				{
					this.MMRD.use_valotile = true;
					this.MMRD.valotile_enabled = true;
					this.ValotManpu.enabled = true;
				}
				else if (this.MMRD.use_valotile)
				{
					this.MMRD.valotile_enabled = false;
					this.ValotManpu.enabled = false;
				}
				this.SWin.use_valotile = value;
				int count_act = this.PoolSpw.count_act;
				for (int i = 0; i < count_act; i++)
				{
					this.PoolSpw.GetOn(i).valotile_enabled = value;
				}
			}
		}

		public EvDrawerContainer destruct()
		{
			int num = this.ADrawer.Length;
			if (this.GobMdManpu != null)
			{
				IN.DestroyOne(this.GobMdManpu);
				IN.DestroyOne(this.MtrManpu);
				this.MtrManpu = null;
				this.MdManpu.destruct();
			}
			this.SWin.destruct();
			this.deactivateEvent();
			EV.remListener(this);
			TX.removeListenerEval(this);
			ManpuDrawer.releaseMti();
			return null;
		}

		public void initEvent()
		{
			this.deactivateEvent();
			int num = this.ADrawer.Length;
			for (int i = 0; i < num; i++)
			{
				EvDrawer evDrawer = this.ADrawer[i];
				if (evDrawer != null)
				{
					evDrawer.release();
				}
			}
			X.ALLN<EvDrawer>(this.ADrawer);
			X.ALLN<TalkDrawer>(this.ATalker);
			this.talker_active_cnt = (this.drawer_active_cnt = 0);
			this.need_sort = false;
			this.SWin.release();
			if (this.GobMdManpu != null)
			{
				this.GobMdManpu.SetActive(false);
				this.MdManpu.clear(false, false);
			}
		}

		public void clear()
		{
			this.initEvent();
		}

		public EvDrawerContainer deactivateDrawer(bool immediate = false)
		{
			int num = this.ADrawer.Length;
			for (int i = 0; i < num; i++)
			{
				EvDrawer evDrawer = this.ADrawer[i];
				if (evDrawer != null)
				{
					evDrawer.deactivate();
				}
			}
			this.SWin.deactivate();
			if (immediate)
			{
				this.fine();
			}
			return this;
		}

		public EvDrawerContainer deactivateEvent()
		{
			this.deactivateDrawer(false);
			return this;
		}

		public TalkDrawer activateTalker(string key, bool fine_order = false, string person = "_", string person_msg = "", string show_type = "")
		{
			if (key == "")
			{
				if (EvPerson.getPerson(person, null) == null)
				{
					X.de("不明な発話者: " + key, null);
				}
				else
				{
					EvDrawer evDrawer = this.Get(person, false, true);
					if (evDrawer == null)
					{
						X.de("不明な発話者: " + person, null);
					}
					else
					{
						evDrawer.release();
					}
				}
				return null;
			}
			TalkDrawer talkDrawer = TalkDrawer.getTk(key, true);
			if (talkDrawer == null)
			{
				X.de("不明な発話位置: " + key, null);
				return null;
			}
			TalkDrawer talkDrawer2 = this.getTalker(person, false);
			if (talkDrawer2 != null)
			{
				if (talkDrawer2.isCompletelyHidden())
				{
					talkDrawer2.release();
					talkDrawer2 = null;
				}
				else
				{
					talkDrawer2.animateDepertPosTo(talkDrawer);
					talkDrawer = talkDrawer2;
				}
			}
			if (talkDrawer2 == null)
			{
				if (talkDrawer.activatePerson(this, person, show_type) == null)
				{
					X.de("不明な発話者: " + person, null);
					return null;
				}
				talkDrawer.prepare();
			}
			if (person_msg != "")
			{
				talkDrawer.person_msg = person_msg;
			}
			int num = X.isinC<TalkDrawer>(this.ATalker, talkDrawer);
			if (num >= 0)
			{
				if (fine_order)
				{
					X.shiftEmpty<TalkDrawer>(this.ATalker, 1, num, -1);
					X.pushToEmpty<TalkDrawer>(this.ATalker, talkDrawer, 1);
					this.reorderTalker();
				}
			}
			else
			{
				talkDrawer.id_in_layer = this.talker_active_cnt;
				X.pushToEmptyS<TalkDrawer>(ref this.ATalker, talkDrawer, ref this.talker_active_cnt, 16);
				X.pushToEmptyS<EvDrawer>(ref this.ADrawer, talkDrawer, ref this.drawer_active_cnt, 16);
			}
			this.need_sort = true;
			return talkDrawer;
		}

		public void fixObjectPositionTo(Transform Tr, string key)
		{
			if (TX.noe(key))
			{
				return;
			}
			EvDrawer evDrawer = TalkDrawer.getTk(key, true);
			if (evDrawer == null)
			{
				evDrawer = this.Get(key, false, true);
				if (evDrawer == null)
				{
					X.de("不明な発話位置: " + key, null);
					return;
				}
			}
			IN.Pos2(Tr, evDrawer.dx_real * 0.015625f, evDrawer.dy_real * 0.015625f);
		}

		public TalkDrawer getTalker(string person = "", bool fine_order = false)
		{
			for (int i = 0; i < this.talker_active_cnt; i++)
			{
				TalkDrawer talkDrawer = this.ATalker[i];
				EvPerson person2 = talkDrawer.get_Person();
				if (person2 != null && person2.key == person)
				{
					if (fine_order)
					{
						X.shiftEmpty<TalkDrawer>(this.ATalker, 1, i, -1);
						X.pushToEmpty<TalkDrawer>(this.ATalker, talkDrawer, 1);
						this.reorderTalker();
					}
					return talkDrawer;
				}
			}
			return null;
		}

		public EvDrawer Get(string key, bool fine_order = false, bool no_make = true)
		{
			EvDrawerContainer.LAYER layer = EvDrawerContainer.keyToLayer(key);
			if (layer == EvDrawerContainer.LAYER.OTHER)
			{
				return null;
			}
			if (layer == EvDrawerContainer.LAYER.TALKER)
			{
				return this.getTalker(key, fine_order);
			}
			if (layer == EvDrawerContainer.LAYER.WINDOW)
			{
				if (key.IndexOf("%W") != 0)
				{
					return null;
				}
				if (!no_make)
				{
					if (X.pushToEmptyRI<EvDrawer>(ref this.ADrawer, this.SWin, this.drawer_active_cnt))
					{
						this.need_sort = true;
						this.drawer_active_cnt++;
					}
					return this.SWin;
				}
				if (X.isinC<EvDrawer>(this.ADrawer, this.SWin, this.drawer_active_cnt) < 0)
				{
					return null;
				}
				return this.SWin;
			}
			else
			{
				if (X.NmI(TX.slice(key, 1), -1, true, false) < 0)
				{
					return null;
				}
				for (int i = 0; i < this.drawer_active_cnt; i++)
				{
					EvDrawer evDrawer = this.ADrawer[i];
					if (evDrawer.get_key() == key)
					{
						return evDrawer;
					}
				}
				if (!no_make)
				{
					EvDrawer evDrawer = new EvDrawer(this, key, layer);
					X.pushToEmptyS<EvDrawer>(ref this.ADrawer, evDrawer, ref this.drawer_active_cnt, 4);
					this.need_sort = true;
					return evDrawer;
				}
				return null;
			}
		}

		public bool EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		public bool EvtClose(bool is_first_or_end)
		{
			return true;
		}

		public bool EvtMoveCheck()
		{
			return true;
		}

		public int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			int num = -1;
			int num2 = -1;
			string text = "";
			if (cmd == "PIC" || cmd == "PIC_SILHOUETTE")
			{
				num = 1;
				num2 = 2;
			}
			else if (cmd == "PIC_B")
			{
				num = -1;
				num2 = 2;
			}
			else if (cmd == "PIC_SWIN" || cmd == "PIC_SWIN_G" || cmd == "PIC_SWIN2")
			{
				num2 = 1;
			}
			else if (cmd == "TUTO_CAP")
			{
				num2 = 1;
			}
			else if (cmd == "PIC_MP")
			{
				ManpuDrawer.reloadTexture();
			}
			if (num2 < 0)
			{
				return 0;
			}
			if (rER.clength <= num2)
			{
				return 0;
			}
			EvPerson evPerson = ((text != "") ? EvPerson.getPerson(text, null) : ((num >= 1) ? EvPerson.getPerson(rER.getIndex(num), null) : null));
			if (evPerson == null)
			{
				return EV.Pics.cacheReadFor(rER.getIndex(num2));
			}
			if (evPerson.cacheGraphics(false))
			{
				return 2;
			}
			return 1;
		}

		public bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			string text = rER.cmd.ToUpper();
			if (text == "TALKER")
			{
				this.activateTalker(rER._2, true, rER._1, rER._3, rER._4);
				return true;
			}
			if (!TX.isStart(text, "PIC", 0))
			{
				return false;
			}
			string text2 = ((skipping != 0) ? "I" : "");
			float num = ((skipping >= 2) ? 0f : ((skipping != 0) ? 0.1f : 1f));
			EvDrawer evDrawer;
			if (text != null)
			{
				uint num2 = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num2 <= 1267446024U)
				{
					if (num2 <= 563861782U)
					{
						if (num2 <= 527604789U)
						{
							if (num2 != 272617774U)
							{
								if (num2 != 527604789U)
								{
									goto IL_0807;
								}
								if (!(text == "PIC_CLEAR_TERM_CACHE"))
								{
									goto IL_0807;
								}
								EvPerson person = EvPerson.getPerson(rER._1, null);
								if (person == null)
								{
									rER.tError("不明な EvPerson :" + rER._1);
								}
								else
								{
									person.clearTermCache();
								}
								return true;
							}
							else if (!(text == "PIC_RIDE"))
							{
								goto IL_0807;
							}
						}
						else if (num2 != 546097861U)
						{
							if (num2 != 563861782U)
							{
								goto IL_0807;
							}
							if (!(text == "PIC_SILHOUETTE"))
							{
								goto IL_0807;
							}
							if ((evDrawer = this.Get(rER._1, false, false)) == null)
							{
								return this.RnoDrwr(rER, "");
							}
							if (evDrawer.setGrp(rER._2, rER._4U + text2 + "H_"))
							{
								evDrawer.initSilhouettePic(TalkDrawer.getTk(rER._3, true), true, false, (text2 + rER._4U).IndexOf("I") >= 0);
							}
							return true;
						}
						else
						{
							if (!(text == "PIC_SWIN_PASTE"))
							{
								goto IL_0807;
							}
							return true;
						}
					}
					else if (num2 <= 791802728U)
					{
						if (num2 != 780439178U)
						{
							if (num2 != 791802728U)
							{
								goto IL_0807;
							}
							if (!(text == "PIC_REM"))
							{
								goto IL_0807;
							}
							this.removeDrawer(this.Get(rER._1, false, true), -1);
							return true;
						}
						else
						{
							if (!(text == "PIC_MVA_WHOLE"))
							{
								goto IL_0807;
							}
							this.applyMvaWhole(rER._1, rER._N2 * num, rER._3);
							return true;
						}
					}
					else if (num2 != 1000141474U)
					{
						if (num2 != 1240892106U)
						{
							if (num2 != 1267446024U)
							{
								goto IL_0807;
							}
							if (!(text == "PIC_HIDE_ALL"))
							{
								goto IL_0807;
							}
							this.picHideWhole(rER._1, rER._B2 || rER._2 == "I");
							return true;
						}
						else
						{
							if (!(text == "PIC_SWIN_SHADOW"))
							{
								goto IL_0807;
							}
							return true;
						}
					}
					else
					{
						if (!(text == "PIC_RECT"))
						{
							goto IL_0807;
						}
						if ((evDrawer = this.Get(rER._1, false, false)) == null)
						{
							return this.RnoDrwr(rER, "");
						}
						if (evDrawer.setGrp((rER._2 != "") ? rER._2 : "WHITE", "rI"))
						{
							evDrawer.initPosition(rER._3, rER._4, rER._5, rER._6, 0f);
							evDrawer.set_z((float)rER.Int(7, 1));
						}
						return true;
					}
				}
				else if (num2 <= 2124021762U)
				{
					if (num2 <= 1593742175U)
					{
						if (num2 != 1463561995U)
						{
							if (num2 != 1593742175U)
							{
								goto IL_0807;
							}
							if (!(text == "PIC_SWAP"))
							{
								goto IL_0807;
							}
						}
						else
						{
							if (!(text == "PIC_RADIATION"))
							{
								goto IL_0807;
							}
							if ((evDrawer = this.Get(rER._1, false, false)) == null)
							{
								return this.RnoDrwr(rER, "");
							}
							if (evDrawer.setGrp((rER._2 != "") ? rER._2 : "WHITE", rER._3U + "D" + text2))
							{
								evDrawer.fadein(0, 0, true);
							}
							return true;
						}
					}
					else if (num2 != 1967579565U)
					{
						if (num2 != 1973277231U)
						{
							if (num2 != 2124021762U)
							{
								goto IL_0807;
							}
							if (!(text == "PIC_B"))
							{
								goto IL_0807;
							}
							if ((evDrawer = this.Get(rER._1, false, false)) == null)
							{
								return this.RnoDrwr(rER, "");
							}
							evDrawer.setGrp(rER._2, rER._3U + "B" + text2);
							if (rER._4 != "")
							{
								evDrawer.setGrp(rER._4, "Af");
							}
							return true;
						}
						else
						{
							if (!(text == "PIC_SWIN_G"))
							{
								goto IL_0807;
							}
							if (this.Get("%W", false, false) == null)
							{
								return this.RnoDrwr(rER, "");
							}
							this.SWin.setGrpAndText(rER._1, null, rER._2U + text2, null, -1f, -1000f, -1000f);
							return true;
						}
					}
					else
					{
						if (!(text == "PIC_FILL"))
						{
							goto IL_0807;
						}
						if ((evDrawer = this.Get(rER._1, false, false)) == null)
						{
							return this.RnoDrwr(rER, "");
						}
						if (evDrawer.setGrp((rER._2 != "") ? rER._2 : "WHITE", rER._3U + "f" + text2))
						{
							evDrawer.fadein(0, 0, true);
						}
						return true;
					}
				}
				else if (num2 <= 2508601190U)
				{
					if (num2 != 2501013529U)
					{
						if (num2 != 2508601190U)
						{
							goto IL_0807;
						}
						if (!(text == "PIC_FLASH"))
						{
							goto IL_0807;
						}
						if ((evDrawer = this.Get(rER._1, false, false)) == null)
						{
							return this.RnoDrwr(rER, "");
						}
						if (evDrawer.setGrp((rER._5 != "") ? rER._5 : "WHITE", "f" + text2))
						{
							evDrawer.setFlash((int)(rER._N2 * num), (int)X.Mx(1f, rER._N3 * num), (int)(rER._N4 * X.Scr(0.3f, num)));
						}
						return true;
					}
					else
					{
						if (!(text == "PIC_SWIN"))
						{
							goto IL_0807;
						}
						if (this.Get("%W", false, false) == null)
						{
							return this.RnoDrwr(rER, "");
						}
						this.SWin.setGrpAndText(rER._1, rER._2, rER._3U + text2, null, -1f, rER.Nm(4, -1000f), rER.Nm(5, -1000f));
						return true;
					}
				}
				else if (num2 != 2633243496U)
				{
					if (num2 != 3607532465U)
					{
						if (num2 != 3908569443U)
						{
							goto IL_0807;
						}
						if (!(text == "PIC"))
						{
							goto IL_0807;
						}
						if ((evDrawer = this.Get(rER._1, true, false)) == null)
						{
							return this.RnoDrwr(rER, "");
						}
						evDrawer.setGrp(rER._2, rER._3 + text2);
						return true;
					}
					else
					{
						if (!(text == "PIC_SWIN2"))
						{
							goto IL_0807;
						}
						if (this.Get("%W", false, false) == null)
						{
							return this.RnoDrwr(rER, "");
						}
						this.SWin.setGrpAndText(rER._1, rER._2, rER._4U + text2, null, rER.Nm(3, -1f), rER.Nm(5, -1000f), rER.Nm(6, -1000f));
						return true;
					}
				}
				else
				{
					if (!(text == "PIC_FINE_ALL"))
					{
						goto IL_0807;
					}
					this.fine();
					return true;
				}
				this.swapDrawer(rER._1, rER._2, text == "PIC_RIDE");
				return true;
			}
			IL_0807:
			if (text == "PIC_HIDE" && rER._1 == "")
			{
				this.deactivateDrawer(false);
			}
			if ((evDrawer = this.Get(rER._1, true, true)) == null)
			{
				return this.RnoDrwr(rER, "");
			}
			if (text != null)
			{
				uint num2 = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num2 <= 3056609248U)
				{
					if (num2 <= 944542164U)
					{
						if (num2 <= 164678870U)
						{
							if (num2 != 90135138U)
							{
								if (num2 != 164678870U)
								{
									return false;
								}
								if (!(text == "PIC_TFADE"))
								{
									return false;
								}
								evDrawer.prepareFader(rER._2, -1, TFKEY.SD2);
								return true;
							}
							else if (!(text == "PIC_MVA"))
							{
								return false;
							}
						}
						else if (num2 != 209441661U)
						{
							if (num2 != 944542164U)
							{
								return false;
							}
							if (!(text == "PIC_FINE"))
							{
								return false;
							}
							evDrawer.fine(true, true, true);
							return true;
						}
						else
						{
							if (!(text == "PIC_FADEIN"))
							{
								return false;
							}
							goto IL_0BAC;
						}
					}
					else if (num2 <= 1280948729U)
					{
						if (num2 != 1274588276U)
						{
							if (num2 != 1280948729U)
							{
								return false;
							}
							if (!(text == "PIC_MP2"))
							{
								return false;
							}
							evDrawer.setManpu(rER._2, rER._3, rER._4 + text2);
							return true;
						}
						else
						{
							if (!(text == "PIC_HIDE"))
							{
								return false;
							}
							evDrawer.deactivate();
							if (rER._B2)
							{
								evDrawer.fine(true, true, true);
							}
							return true;
						}
					}
					else if (num2 != 1286509408U)
					{
						if (num2 != 1559322342U)
						{
							if (num2 != 3056609248U)
							{
								return false;
							}
							if (!(text == "PIC_MOVEA"))
							{
								return false;
							}
						}
						else
						{
							if (!(text == "PIC_FADEOUT"))
							{
								return false;
							}
							goto IL_0BCB;
						}
					}
					else
					{
						if (!(text == "PIC_TEMP_REPLACE_TERM"))
						{
							return false;
						}
						evDrawer.addTemporaryTerm(rER._2, rER._3, rER._4);
						return true;
					}
					float num3 = X.Nm(rER._2, 0f, false);
					evDrawer.moveA(rER._3, (num3 >= 0f) ? ((int)(num3 * num)) : ((int)num3));
					return true;
				}
				if (num2 <= 4016337725U)
				{
					if (num2 <= 3562999103U)
					{
						if (num2 != 3326059745U)
						{
							if (num2 != 3562999103U)
							{
								return false;
							}
							if (!(text == "PIC_MV2"))
							{
								return false;
							}
						}
						else
						{
							if (!(text == "PIC_MOVE"))
							{
								return false;
							}
							goto IL_0C20;
						}
					}
					else if (num2 != 3912267817U)
					{
						if (num2 != 3999560106U)
						{
							if (num2 != 4016337725U)
							{
								return false;
							}
							if (!(text == "PIC_FY"))
							{
								return false;
							}
							evDrawer.flip(false, true, "N" + rER._2U + text2);
							return true;
						}
						else
						{
							if (!(text == "PIC_FX"))
							{
								return false;
							}
							evDrawer.flip(true, false, "N" + rER._2U + text2);
							return true;
						}
					}
					else if (!(text == "PIC_MOVE2"))
					{
						return false;
					}
					evDrawer.moveTo(rER._2, rER._3, rER._4, rER._5, (int)(X.Nm(rER._6, 0f, false) * num), rER._7);
					return true;
				}
				if (num2 <= 4067803415U)
				{
					if (num2 != 4034248177U)
					{
						if (num2 != 4067803415U)
						{
							return false;
						}
						if (!(text == "PIC_MV"))
						{
							return false;
						}
					}
					else
					{
						if (!(text == "PIC_MP"))
						{
							return false;
						}
						if (rER._2 == "")
						{
							evDrawer.deactivateManpu(rER._3.IndexOf('I') >= 0 || text2.IndexOf('I') >= 0, false);
						}
						else
						{
							evDrawer.setManpu("_", rER._2, rER._3 + text2);
						}
						return true;
					}
				}
				else if (num2 != 4184113915U)
				{
					if (num2 != 4262395919U)
					{
						if (num2 != 4284779629U)
						{
							return false;
						}
						if (!(text == "PIC_FI"))
						{
							return false;
						}
						goto IL_0BAC;
					}
					else
					{
						if (!(text == "PIC_FLIP"))
						{
							return false;
						}
						string text3 = rER._2.ToLower();
						evDrawer.flip(text3.IndexOf("x") >= 0, text3.IndexOf("y") >= 0, "N" + rER._3U + text2);
						return true;
					}
				}
				else
				{
					if (!(text == "PIC_FO"))
					{
						return false;
					}
					goto IL_0BCB;
				}
				IL_0C20:
				evDrawer.moveTo(rER._2, rER._3, (int)(X.Nm(rER._4, 0f, false) * num), rER._5);
				return true;
				IL_0BAC:
				evDrawer.fadein((int)(X.Nm(rER._2, 0f, false) * num), 0, true);
				return true;
				IL_0BCB:
				evDrawer.fadeout((int)(X.Nm(rER._2, 0f, false) * num));
				return true;
			}
			return false;
		}

		private bool RnoDrwr(StringHolder rER, string t = "")
		{
			return true;
		}

		public static void createListenerEval(EvDrawerContainer DC)
		{
			TxEvalListenerContainer txEvalListenerContainer = TX.createListenerEval(DC, 3, true);
			txEvalListenerContainer.Add("EVDRAWER_X", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				X.Get<string>(Aargs, 0, "");
				EvDrawer evDrawer;
				if ((evDrawer = EV.DC.Get(REG.R1, false, true)) == null)
				{
					TX.InputE(0f);
					return;
				}
				TX.InputE(evDrawer.get_x());
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("EVDRAWER_Y", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				X.Get<string>(Aargs, 0, "");
				EvDrawer evDrawer2;
				if ((evDrawer2 = EV.DC.Get(REG.R1, false, true)) == null)
				{
					TX.InputE(0f);
					return;
				}
				TX.InputE(evDrawer2.get_y());
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("pic_exists", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				X.Get<string>(Aargs, 0, "");
				TX.InputE((float)((EV.DC.Get(REG.R1, false, true) != null) ? 1 : 0));
			}, Array.Empty<string>());
		}

		public EvDrawerContainer run(float fcnt, bool deleting = false)
		{
			if (this.need_sort)
			{
				this.need_sort = false;
				this.SortDrawer.qSort(this.ADrawer, null, -1);
				this.manpu_draw_z = 0f;
				float num = 0f;
				for (int i = this.drawer_active_cnt - 1; i >= 0; i--)
				{
					EvDrawer evDrawer = this.ADrawer[i];
					num = ((evDrawer.layer > EvDrawerContainer.LAYER.BACK) ? (-1.8000002f) : 0f) + 0.3f - (float)i * 0.0001f;
					evDrawer.repositMesh(num);
					if (this.manpu_draw_z == 0f && evDrawer.layer <= EvDrawerContainer.LAYER.TALKER)
					{
						this.manpu_draw_z = num;
					}
				}
				if (this.manpu_draw_z == 0f)
				{
					this.manpu_draw_z = num - 0.0001f;
				}
			}
			if (this.drawer_active_cnt > 0)
			{
				for (int j = this.drawer_active_cnt - 1; j >= 0; j--)
				{
					EvDrawer evDrawer = this.ADrawer[j];
					if (!evDrawer.runDraw(fcnt, deleting))
					{
						this.removeDrawer(evDrawer, j);
					}
				}
				if (this.drawer_active_cnt == 0 && deleting)
				{
					ManpuDrawer.releaseTexture();
				}
			}
			if (X.D && this.fine_manpu_md)
			{
				this.fine_manpu_md = false;
				if (this.MdManpu != null)
				{
					this.MdManpu.updateForMeshRenderer(false);
				}
			}
			return this;
		}

		public EvDrawerContainer fine()
		{
			for (int i = this.drawer_active_cnt - 1; i >= 0; i--)
			{
				this.ADrawer[i].fine(true, true, true);
			}
			return this;
		}

		public int getActiveCount()
		{
			return this.drawer_active_cnt;
		}

		public SpineViewerBehaviour getPooledSpineViewer(bool fliped)
		{
			bool flag;
			SpineViewerBehaviour spineViewerBehaviour = this.PoolSpw.Next(out flag);
			spineViewerBehaviour.prepareMaterial(this.MtrManpu, false);
			spineViewerBehaviour.gameObject.SetActive(true);
			return spineViewerBehaviour;
		}

		public SpineViewerBehaviour releasePooledSpineViewer(SpineViewerBehaviour Spb)
		{
			if (Spb != null)
			{
				this.PoolSpw.Release(Spb);
			}
			return null;
		}

		public MeshDrawer getManpuMeshDrawer()
		{
			this.GobMdManpu.SetActive(true);
			return this.MdManpu;
		}

		public int isFilledWholeScreen()
		{
			int num = 0;
			for (int i = this.drawer_active_cnt - 1; i >= 0; i--)
			{
				num = X.Mx(this.ADrawer[i].isFilledWholeScreen(), num);
			}
			return num;
		}

		public IHkdsFollowable FindHkdsFollowableObject(string key)
		{
			return this.Get(key, false, true);
		}

		public string swapDrawer(string key_s, string key_d, bool riding = true)
		{
			if (key_s == null)
			{
				return "swapDrawer:: Src が空文字";
			}
			if (key_d == null)
			{
				return "swapDrawer:: Dest が空文字";
			}
			if (key_d == key_s)
			{
				return "swapDrawer:: Src と Dest が同じ";
			}
			EvDrawer evDrawer = this.Get(key_s, false, true);
			if (evDrawer == null)
			{
				return "swapDrawer:: Src 側が 存在しない";
			}
			EvDrawer evDrawer2 = this.Get(key_d, false, true);
			EvDrawerContainer.LAYER layer = EvDrawerContainer.keyToLayer(key_d);
			if (evDrawer.layer != EvDrawerContainer.LAYER.BACK && evDrawer.layer != EvDrawerContainer.LAYER.FRONT)
			{
				return "swapDrawer:: Src 側が #/& で始まっていない";
			}
			if (layer != EvDrawerContainer.LAYER.BACK && layer != EvDrawerContainer.LAYER.FRONT)
			{
				return "swapDrawer:: Dest 側が #/& で始まっていない";
			}
			if (evDrawer2 == null || riding)
			{
				if (evDrawer2 != null)
				{
					this.removeDrawer(evDrawer2, -1);
				}
				evDrawer.rewriteKeyAndLayer(key_d);
			}
			else
			{
				evDrawer.rewriteKeyAndLayer(key_d);
				evDrawer2.rewriteKeyAndLayer(key_s);
			}
			this.need_sort = true;
			return "";
		}

		public void applyMvaWhole(string layerset, float time, string type)
		{
			bool flag = true;
			bool flag2 = true;
			bool flag3 = true;
			bool flag4 = true;
			bool flag5 = true;
			if (TX.valid(layerset))
			{
				flag4 = layerset.IndexOf("W") >= 0;
				flag = layerset.IndexOf("T") >= 0;
				flag2 = layerset.IndexOf("#") >= 0;
				flag3 = layerset.IndexOf("&") >= 0;
				flag5 = layerset.IndexOf("%") >= 0;
			}
			int i = this.drawer_active_cnt - 1;
			while (i >= 0)
			{
				EvDrawer evDrawer = this.ADrawer[i];
				bool flag6;
				switch (evDrawer.layer)
				{
				case EvDrawerContainer.LAYER.BACK:
					flag6 = flag2;
					break;
				case EvDrawerContainer.LAYER.TALKERB:
					flag6 = flag5;
					break;
				case EvDrawerContainer.LAYER.TALKER:
					flag6 = flag;
					break;
				case EvDrawerContainer.LAYER.WINDOW:
					goto IL_00C0;
				case EvDrawerContainer.LAYER.FRONT:
					flag6 = flag3;
					break;
				default:
					goto IL_00C0;
				}
				IL_00C3:
				if (flag6)
				{
					evDrawer.moveA(type, (int)time);
				}
				i--;
				continue;
				IL_00C0:
				flag6 = false;
				goto IL_00C3;
			}
			if (flag4)
			{
				this.SWin.moveA(type, (int)time);
			}
		}

		public void picHideWhole(string layerset, bool immediate)
		{
			bool flag = true;
			bool flag2 = true;
			bool flag3 = true;
			bool flag4 = true;
			bool flag5 = true;
			if (TX.valid(layerset))
			{
				flag4 = layerset.IndexOf("W") >= 0;
				flag = layerset.IndexOf("T") >= 0;
				flag2 = layerset.IndexOf("#") >= 0;
				flag3 = layerset.IndexOf("&") >= 0;
				flag5 = layerset.IndexOf("%") >= 0;
			}
			int i = this.drawer_active_cnt - 1;
			while (i >= 0)
			{
				EvDrawer evDrawer = this.ADrawer[i];
				bool flag6;
				switch (evDrawer.layer)
				{
				case EvDrawerContainer.LAYER.BACK:
					flag6 = flag2;
					break;
				case EvDrawerContainer.LAYER.TALKERB:
					flag6 = flag5;
					break;
				case EvDrawerContainer.LAYER.TALKER:
					flag6 = flag;
					break;
				case EvDrawerContainer.LAYER.WINDOW:
					goto IL_00C0;
				case EvDrawerContainer.LAYER.FRONT:
					flag6 = flag3;
					break;
				default:
					goto IL_00C0;
				}
				IL_00C3:
				if (flag6)
				{
					evDrawer.deactivate();
				}
				i--;
				continue;
				IL_00C0:
				flag6 = true;
				goto IL_00C3;
			}
			if (flag4)
			{
				this.SWin.deactivate();
			}
			if (immediate)
			{
				this.fine();
			}
		}

		public void removeDrawer(EvDrawer Dr, int all_index = -1)
		{
			if (Dr == null)
			{
				return;
			}
			Dr.release();
			if (all_index == -1)
			{
				all_index = X.isinC<EvDrawer>(this.ADrawer, Dr);
			}
			if (all_index >= 0)
			{
				X.shiftEmpty<EvDrawer>(this.ADrawer, 1, all_index, -1);
				this.drawer_active_cnt--;
			}
			EvDrawer[] atalker = this.ATalker;
			int num;
			if ((num = X.isinC<EvDrawer>(atalker, Dr)) >= 0)
			{
				X.shiftEmpty<TalkDrawer>(this.ATalker, 1, num, -1);
				this.talker_active_cnt--;
			}
		}

		private EvDrawerContainer reorderTalker()
		{
			for (int i = 0; i < this.talker_active_cnt; i++)
			{
				this.ATalker[i].id_in_layer = i;
			}
			this.need_sort = true;
			return this;
		}

		public TalkDrawer[] getTalkerDrawerList(out int max)
		{
			max = this.talker_active_cnt;
			return this.ATalker;
		}

		private static int fnSortDrawer(EvDrawer Da, EvDrawer Db)
		{
			if (Da == null && Db == null)
			{
				return 0;
			}
			if (Da == null || Db == null)
			{
				if (Da != null)
				{
					return -1;
				}
				return 1;
			}
			else
			{
				int layer = (int)Da.layer;
				int layer2 = (int)Db.layer;
				if (layer != layer2)
				{
					if (layer >= layer2)
					{
						return 1;
					}
					return -1;
				}
				else
				{
					if (Da.id_in_layer == Db.id_in_layer)
					{
						return 0;
					}
					if (Da.id_in_layer >= Db.id_in_layer)
					{
						return 1;
					}
					return -1;
				}
			}
		}

		public static EvDrawerContainer.LAYER keyToLayer(string s)
		{
			if (TX.noe(s))
			{
				return EvDrawerContainer.LAYER.OTHER;
			}
			char c = s.ToCharArray()[0];
			if (c == '#')
			{
				return EvDrawerContainer.LAYER.BACK;
			}
			if (c == '&')
			{
				return EvDrawerContainer.LAYER.FRONT;
			}
			if (c != '%')
			{
				return EvDrawerContainer.LAYER.TALKER;
			}
			if (s.IndexOf("%W") == 0)
			{
				return EvDrawerContainer.LAYER.WINDOW;
			}
			return EvDrawerContainer.LAYER.TALKERB;
		}

		public MultiMeshRenderer get_MMRD()
		{
			return this.MMRD;
		}

		private readonly EvSmallWin SWin;

		private EvDrawer[] ADrawer;

		private TalkDrawer[] ATalker;

		private int drawer_active_cnt;

		private int talker_active_cnt;

		private readonly SORT<EvDrawer> SortDrawer;

		private readonly MultiMeshRenderer MMRD;

		public bool need_sort;

		public bool fine_manpu_md;

		private readonly ObjPool<SpineViewerBehaviour> PoolSpw;

		public RadiationDrawer DrRad;

		internal MTI MtiManpuImage;

		private readonly GameObject GobMdManpu;

		private readonly ValotileRenderer ValotManpu;

		private readonly MeshDrawer MdManpu;

		private Material MtrManpu;

		public float manpu_draw_z;

		public readonly int mmrd_first_count;

		public MProperty MpbManpuFlip;

		private static readonly Regex RegPosX = new Regex("^EVDRAWER_X\\[ *([\\&\\#\\%]?\\w+) *\\]");

		private static readonly Regex RegPosY = new Regex("^EVDRAWER_Y\\[ *([\\&\\#\\%]?\\w+) *\\]");

		private static readonly Regex RegPicExists = new Regex("^pic_exists\\[ *([\\#\\%\\&]\\d+|\\w+) *\\]/", RegexOptions.IgnoreCase);

		public enum LAYER
		{
			OTHER = -1,
			BACK,
			TALKERB,
			TALKER,
			WINDOW,
			FRONT
		}
	}
}
