using System;
using System.Collections.Generic;
using evt;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2DEventListener : IEventListener
	{
		public M2DEventListener(M2DBase _M2D)
		{
			this.M2D = _M2D;
			this.createListenerEval(0);
			EV.addListener(this);
		}

		public virtual void newGame()
		{
		}

		public virtual void destruct()
		{
			TX.removeListenerEval(this);
			EV.remListener(this);
		}

		public virtual int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		public virtual TxEvalListenerContainer createListenerEval(int cap_fn = 0)
		{
			TxEvalListenerContainer txEvalListenerContainer = TX.createListenerEval(this, cap_fn + 10, true);
			txEvalListenerContainer.Add("Exist_mover", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (this.curMap != null)
				{
					TX.InputE((float)((this.curMap.getMoverByName(X.Get<string>(Aargs, 0), false) != null) ? 1 : 0));
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("map_x", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (this.curMap != null)
				{
					Vector2 pos = this.curMap.getPos(X.Get<string>(Aargs, 0), 0f, 0f, null);
					TX.InputE((pos.x != -1000f) ? pos.x : 0f);
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("map_y", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (this.curMap != null)
				{
					Vector2 pos2 = this.curMap.getPos(X.Get<string>(Aargs, 0), 0f, 0f, null);
					TX.InputE((pos2.x != -1000f) ? pos2.y : 0f);
				}
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("lp_foc_x", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (this.curMap == null || Aargs.Count == 0)
				{
					return;
				}
				M2LabelPoint m2LabelPoint = null;
				if (Aargs.Count >= 2)
				{
					M2MapLayer layer = this.curMap.getLayer(Aargs[0]);
					if (layer == null)
					{
						X.de("レイヤーが不明:" + Aargs[0], null);
					}
					else
					{
						m2LabelPoint = layer.LP.Get(Aargs[1], true, false);
					}
				}
				else
				{
					m2LabelPoint = this.curMap.getLabelPoint(Aargs[0]);
				}
				TX.InputE((m2LabelPoint != null) ? m2LabelPoint.mapfocx : (M2DBase.Instance.Cam.x * this.curMap.rCLEN));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("lp_foc_y", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (this.curMap == null || Aargs.Count == 0)
				{
					return;
				}
				M2LabelPoint m2LabelPoint2 = null;
				if (Aargs.Count >= 2)
				{
					M2MapLayer layer2 = this.curMap.getLayer(Aargs[0]);
					if (layer2 == null)
					{
						X.de("レイヤーが不明:" + Aargs[0], null);
					}
					else
					{
						m2LabelPoint2 = layer2.LP.Get(Aargs[1], true, false);
					}
				}
				else
				{
					m2LabelPoint2 = this.curMap.getLabelPoint(Aargs[0]);
				}
				TX.InputE((m2LabelPoint2 != null) ? m2LabelPoint2.mapfocy : (M2DBase.Instance.Cam.y * this.curMap.rCLEN));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("cam_x", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(M2DBase.Instance.Cam.x * this.curMap.rCLEN);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("cam_y", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(M2DBase.Instance.Cam.y * this.curMap.rCLEN);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("m2d_current_x", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (M2EventCommand.EvMV == null)
				{
					X.de("不明なM2D操作対象", null);
					return;
				}
				TX.InputE(M2EventCommand.EvMV.x);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("m2d_current_y", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (M2EventCommand.EvMV == null)
				{
					X.de("不明なM2D操作対象", null);
					return;
				}
				TX.InputE(M2EventCommand.EvMV.y);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("m2d_current_sizex", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (M2EventCommand.EvMV == null)
				{
					X.de("不明なM2D操作対象", null);
					return;
				}
				TX.InputE(M2EventCommand.EvMV.sizex);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("m2d_current_sizey", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (M2EventCommand.EvMV == null)
				{
					X.de("不明なM2D操作対象", null);
					return;
				}
				TX.InputE(M2EventCommand.EvMV.sizey);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("m2d_has_foot", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (M2EventCommand.EvMV == null)
				{
					X.de("不明なM2D操作対象", null);
					return;
				}
				TX.InputE((float)(M2EventCommand.EvMV.hasFoot() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("m2d_walkable_mpf", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				if (this.curMap == null)
				{
					return;
				}
				if (M2EventCommand.EvMV == null)
				{
					X.de("不明なM2D操作対象", null);
					return;
				}
				M2Phys physic = M2EventCommand.EvMV.getPhysic();
				if (physic == null || !physic.hasFoot())
				{
					TX.InputE(0f);
					return;
				}
				M2Mover evMV = M2EventCommand.EvMV;
				M2BlockColliderContainer.BCCLine footBCC = physic.getFootManager().get_FootBCC();
				if (footBCC == null)
				{
					TX.InputE(0f);
					return;
				}
				float num = 1f;
				if (Aargs.Count > 0)
				{
					num = X.Nm(Aargs[0], num, false);
				}
				float num2;
				float num3;
				M2BlockColliderContainer.BCCLine bccline;
				M2BlockColliderContainer.BCCLine bccline2;
				footBCC.getLinearWalkableArea(0f, out num2, out num3, out bccline, out bccline2, num);
				float num4 = X.Abs(num2 - evMV.x);
				float num5 = X.Abs(num3 - evMV.x);
				if (num4 < num && num5 < num)
				{
					TX.InputE(0f);
					return;
				}
				if (num4 < num)
				{
					TX.InputE(1f);
					return;
				}
				if (num5 < num)
				{
					TX.InputE(-1f);
					return;
				}
				TX.InputE(evMV.mpf_is_right);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("CLEN", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(this.CLEN);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("warp_not_injectable", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE((float)(this.M2D.FlgWarpEventNotInjectable.isActive() ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("blood_restrict", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(1f - (float)M2DBase.blood_weaken / 100f);
			}, Array.Empty<string>());
			return txEvalListenerContainer;
		}

		public virtual bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 1116662464U)
				{
					if (num <= 735139189U)
					{
						if (num <= 188643197U)
						{
							if (num != 177837449U)
							{
								if (num != 188643197U)
								{
									goto IL_060B;
								}
								if (!(cmd == "QU_HANDSHAKE"))
								{
									goto IL_060B;
								}
								this.Cam.Qu.HandShake(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, 1f), rER.Int(4, 0));
								return true;
							}
							else if (!(cmd == "DEFEAT_EVENT2"))
							{
								goto IL_060B;
							}
						}
						else if (num != 226500968U)
						{
							if (num != 735139189U)
							{
								goto IL_060B;
							}
							if (!(cmd == "DEF_CURMAP"))
							{
								goto IL_060B;
							}
							if (TX.valid(rER._1) && this.curMap != null)
							{
								ER.VarCon.define(rER._1, this.curMap.key, true);
							}
							return true;
						}
						else
						{
							if (!(cmd == "STOP_DGN_HALF_BGM"))
							{
								goto IL_060B;
							}
							this.M2D.fineDgnHalfBgm(false);
							return true;
						}
					}
					else if (num <= 997752879U)
					{
						if (num != 740914389U)
						{
							if (num != 997752879U)
							{
								goto IL_060B;
							}
							if (!(cmd == "VALOTIZE"))
							{
								goto IL_060B;
							}
							if (rER.Nm(1, 1f) != 0f)
							{
								this.M2D.FlagValotStabilize.Rem("EV");
							}
							else
							{
								this.M2D.FlagValotStabilize.Add("EV");
							}
							return true;
						}
						else
						{
							if (!(cmd == "QU_VIB"))
							{
								goto IL_060B;
							}
							this.Cam.Qu.Vib(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0));
							return true;
						}
					}
					else if (num != 1002657295U)
					{
						if (num != 1025204630U)
						{
							if (num != 1116662464U)
							{
								goto IL_060B;
							}
							if (!(cmd == "QU_SINV"))
							{
								goto IL_060B;
							}
							this.Cam.Qu.SinV(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0));
							return true;
						}
						else
						{
							if (!(cmd == "INIT_MAP_MATERIAL"))
							{
								goto IL_060B;
							}
							Map2d map2d = this.M2D.Get(rER._1, false);
							if (map2d == null)
							{
								return rER.tError("Map2d が不明: " + rER._1);
							}
							this.M2D.initMapMaterialASync(map2d, rER.Int(2, 1), true);
							return true;
						}
					}
					else
					{
						if (!(cmd == "#AUTO_BGM_REPLACE"))
						{
							goto IL_060B;
						}
						this.M2D.autoBgmReplace(rER.Nm(1, 0f) != 0f);
						return true;
					}
				}
				else if (num <= 1656570214U)
				{
					if (num <= 1460610478U)
					{
						if (num != 1286108617U)
						{
							if (num != 1460610478U)
							{
								goto IL_060B;
							}
							if (!(cmd == "HIDDEN_WHOLE_SCREEN"))
							{
								goto IL_060B;
							}
							if (rER.Nm(1, 1f) != 0f)
							{
								this.M2D.FlgHideWholeScreen.Add("EV");
							}
							else
							{
								this.M2D.FlgHideWholeScreen.Rem("EV");
							}
							return true;
						}
						else
						{
							if (!(cmd == "ITEM_DEACTIVATE"))
							{
								goto IL_060B;
							}
							this.curMap.evItemActivate(rER._1, -1);
							return true;
						}
					}
					else if (num != 1619991034U)
					{
						if (num != 1656570214U)
						{
							goto IL_060B;
						}
						if (!(cmd == "ITEM_ACTIVATE"))
						{
							goto IL_060B;
						}
						this.curMap.evItemActivate(rER._1, 1);
						return true;
					}
					else
					{
						if (!(cmd == "QU_SINH"))
						{
							goto IL_060B;
						}
						this.Cam.Qu.SinH(rER.Nm(1, 0f), (float)rER.Int(2, 1), rER.Nm(3, -1f), rER.Int(4, 0));
						return true;
					}
				}
				else if (num <= 2443818774U)
				{
					if (num != 2123024112U)
					{
						if (num != 2443818774U)
						{
							goto IL_060B;
						}
						if (!(cmd == "START_DGN_HALF_BGM"))
						{
							goto IL_060B;
						}
						this.M2D.fineDgnHalfBgm(true);
						return true;
					}
					else
					{
						if (!(cmd == "ADD_MAPFLUSH_FLAG"))
						{
							goto IL_060B;
						}
						this.M2D.setFlushOtherMatFlag();
						this.M2D.setFlushMapFlag();
						this.M2D.addMapFlushFlagToLoader();
						return true;
					}
				}
				else if (num != 2793803244U)
				{
					if (num != 3006393793U)
					{
						if (num != 3714866345U)
						{
							goto IL_060B;
						}
						if (!(cmd == "DEF_MOBTYPE"))
						{
							goto IL_060B;
						}
						if (TX.valid(rER._1))
						{
							ER.VarCon.define(rER._1, this.M2D.ev_mobtype ?? "", true);
						}
						return true;
					}
					else if (!(cmd == "DEFEAT_EVENT"))
					{
						goto IL_060B;
					}
				}
				else
				{
					if (!(cmd == "SEND_EVENT_CORRUPTION"))
					{
						goto IL_060B;
					}
					M2EventContainer eventContainer = this.curMap.getEventContainer();
					if (eventContainer == null)
					{
						return rER.tError("EVC is empty");
					}
					if (eventContainer.stackSpecialCommand(rER._1, null, true))
					{
						EV.moduleInitAfter(ER, null, false);
					}
					return true;
				}
				if (this.M2D.isGameOverActive())
				{
					if (rER.cmd == "DEFEAT_EVENT")
					{
						ER.seek_set(ER.getLength());
					}
					return true;
				}
				if (rER.cmd == "DEFEAT_EVENT2")
				{
					EV.moduleInit(ER, rER._1, rER.slice(2, -1000), false);
				}
				else
				{
					EV.changeEvent(rER._1, 0, rER.slice(2, -1000));
				}
				return true;
			}
			IL_060B:
			return this.curMap != null && this.curMap.evReadMap(ER, rER, skipping);
		}

		public bool EvtMoveCheck()
		{
			return this.curMap == null || EV.isStoppingGame() || this.curMap.evMoveCheck();
		}

		public bool EvtOpen(bool is_start)
		{
			return this.M2D.EvtOpen(is_start);
		}

		public virtual bool EvtClose(bool is_start)
		{
			return this.M2D.EvtClose(is_start);
		}

		public Map2d curMap
		{
			get
			{
				return this.M2D.curMap;
			}
		}

		public override string ToString()
		{
			return "M2DEventListener";
		}

		public float CLEN
		{
			get
			{
				return this.M2D.CLEN;
			}
		}

		public float rCLEN
		{
			get
			{
				return 1f / this.M2D.CLEN;
			}
		}

		public M2Camera Cam
		{
			get
			{
				return this.M2D.Cam;
			}
		}

		public readonly M2DBase M2D;

		public const byte BLOOD_WEAKEN_MAX = 100;
	}
}
