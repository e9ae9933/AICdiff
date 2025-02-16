using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public static class PuzzLiner
	{
		public static void initG(NelM2DBase M2D)
		{
			PuzzLiner.MtrRail = M2D.MIchip.getMtr(MTRX.getShd("Hachan/ShaderGDTPAdd2"), -1);
		}

		public static void initS(Map2d _Mp)
		{
			if (_Mp == null)
			{
				PuzzLiner.to_offline = true;
				return;
			}
			PuzzLiner.Ed = null;
			PuzzLiner.Mp = _Mp;
			PuzzLiner.to_offline = false;
			PuzzLiner.draw_depert = false;
			PuzzLiner.activation_lock = false;
			if (PuzzLiner.AConnector == null)
			{
				PuzzLiner.ARailChipActive = new List<NelChipPuzzleRail>();
				PuzzLiner.ADepert = new List<PuzzLiner.LinerDepart>();
				PuzzLiner.AFootSwitch = new List<NelChipFootSwitch>[5];
				for (int i = 4; i >= 0; i--)
				{
					PuzzLiner.AFootSwitch[i] = new List<NelChipFootSwitch>();
				}
				PuzzLiner.AConnector = new List<NelChipPuzzleConnector>();
				PuzzLiner.ASwitch = new List<NelChipPuzzleSwitch>();
				PuzzLiner.ALpBuf = new List<M2LabelPoint>();
				PuzzLiner.ACpBuf = new List<M2Puts>();
				PuzzLiner.Halo = new HaloDrawer(-1f, -1f, -1f, -1f, -1f, -1f).Set(11f, 50f, -1f, -1f, -1f, -1f);
			}
			else
			{
				PuzzLiner.ARailChipActive.Clear();
				PuzzLiner.ADepert.Clear();
			}
			PuzzLiner.initializePuzzleCaches();
		}

		public static void initializePuzzleCaches()
		{
			CarrierRailBlock.initS();
			for (int i = 4; i >= 0; i--)
			{
				PuzzLiner.AFootSwitch[i].Clear();
			}
			PuzzLiner.AConnector.Clear();
			PuzzLiner.ASwitch.Clear();
		}

		public static void Assign(M2Chip Cp)
		{
			if (Cp is NelChipPuzzleSwitch)
			{
				PuzzLiner.ASwitch.Add(Cp as NelChipPuzzleSwitch);
			}
			if (Cp is NelChipPuzzleConnector)
			{
				PuzzLiner.AConnector.Add(Cp as NelChipPuzzleConnector);
			}
		}

		public static void activateConnector(M2PuzzleSwitch Swt, bool immediate)
		{
			PuzzLiner.activateConnector(Swt.puzzle_id, immediate);
		}

		public static void activateConnector(int puzzle_id, bool immediate)
		{
			if (PuzzLiner.activation_lock)
			{
				return;
			}
			PuzzLiner.activation_lock = true;
			int num = PuzzLiner.ASwitch.Count;
			for (int i = 0; i < num; i++)
			{
				M2PuzzleSwitch mover = PuzzLiner.ASwitch[i].getMover();
				if (!(mover == null) && !mover.isActive() && mover.puzzle_id == puzzle_id)
				{
					mover.activate();
				}
			}
			num = PuzzLiner.AConnector.Count;
			for (int j = 0; j < num; j++)
			{
				PuzzLiner.AConnector[j].activateConnector(puzzle_id, immediate);
			}
			PUZ.IT.connectorActivated(puzzle_id, true);
			PuzzLiner.activation_lock = false;
		}

		public static void deactivateConnector(M2PuzzleSwitch Swt, bool immediate)
		{
			PuzzLiner.deactivateConnector(Swt.puzzle_id, immediate, false);
		}

		public static bool deactivateConnector(int puzzle_id, bool immediate, bool no_recheck_liner = false)
		{
			if (PuzzLiner.ASwitch == null || PuzzLiner.activation_lock)
			{
				return false;
			}
			PuzzLiner.activation_lock = true;
			int num = PuzzLiner.ASwitch.Count;
			for (int i = 0; i < num; i++)
			{
				M2PuzzleSwitch mover = PuzzLiner.ASwitch[i].getMover();
				if (!(mover == null) && mover.isActive() && mover.puzzle_id == puzzle_id)
				{
					mover.deactivate();
				}
			}
			PuzzLiner.activation_lock = false;
			bool flag = false;
			num = PuzzLiner.AConnector.Count;
			for (int j = 0; j < num; j++)
			{
				flag = PuzzLiner.AConnector[j].deactivateConnector(puzzle_id, immediate, true) || flag;
			}
			if (flag && !no_recheck_liner)
			{
				PuzzLiner.recheckLiner(puzzle_id, immediate);
			}
			PUZ.IT.connectorActivated(puzzle_id, false);
			return flag;
		}

		public static void activateFootSwitch(NelChipFootSwitch Cp, int puzzle_id, bool deactivating)
		{
			if (puzzle_id < 0)
			{
				return;
			}
			List<NelChipFootSwitch> list = PuzzLiner.AFootSwitch[puzzle_id];
			if (!deactivating)
			{
				if (list.IndexOf(Cp) >= 0)
				{
					return;
				}
				list.Add(Cp);
				if (list.Count == 1)
				{
					PuzzLiner.activateConnector(puzzle_id, false);
					PuzzLiner.finalizeAnimation(true, puzzle_id, true);
					return;
				}
			}
			else
			{
				if (list.Count == 0)
				{
					return;
				}
				list.Remove(Cp);
				if (list.Count == 0)
				{
					PuzzLiner.deactivateConnector(puzzle_id, false, false);
					PuzzLiner.finalizeAnimation(true, puzzle_id, true);
				}
			}
		}

		public static void initMapEditor()
		{
			PuzzLiner.recheckLiner(-1, true, false, true);
			PuzzLiner.initializePuzzleCaches();
		}

		public static void quitMapEditorAfterInitAction()
		{
			PuzzLiner.recheckLiner(-1, false, true, true);
		}

		public static void recheckLiner(int deactivating_id, bool immediate)
		{
			PuzzLiner.recheckLiner(deactivating_id, true, true, immediate);
		}

		private static void recheckLiner(int deactivating_id, bool pre_deactivate = true, bool post_recheck_liner = true, bool immediate = false)
		{
			if (pre_deactivate)
			{
				for (int i = PuzzLiner.ADepert.Count - 1; i >= 0; i--)
				{
					PuzzLiner.LinerDepart linerDepart = PuzzLiner.ADepert[i];
					if (deactivating_id < 0 || linerDepart.hasSwitchId(deactivating_id) || PuzzLiner.to_offline)
					{
						if (immediate || PuzzLiner.to_offline)
						{
							linerDepart.deactivate(true, false);
							PuzzLiner.ADepert.RemoveAt(i);
						}
						else
						{
							linerDepart.deactivate(false, false);
						}
					}
				}
				PuzzLiner.ARailChipActive.Clear();
			}
			if (post_recheck_liner)
			{
				int count = PuzzLiner.AConnector.Count;
				for (int j = 0; j < count; j++)
				{
					PuzzLiner.AConnector[j].recheckLiner(immediate);
				}
			}
		}

		public static void initLiner(Map2d Mp, float sx, float sy, int switch_id, int a, int walk_count = 0, bool immediate = false)
		{
			bool flag = walk_count == 0;
			int num = 1;
			bool flag2 = true;
			while (--num >= 0)
			{
				int num2 = (int)(sx + ((CAim._XD(a, 1) < 0) ? (-0.5f) : 0f));
				int num3 = (int)(sy + ((-CAim._YD(a, 1) < 0) ? (-0.5f) : 0f));
				int num4 = (int)((a == -1) ? ((AIM)4294967295U) : CAim.get_opposite((AIM)a));
				PuzzLiner.ACpBuf.Clear();
				Mp.getPointMetaPutsTo(num2, num3, PuzzLiner.ACpBuf, "puzzle_rail");
				int num5 = PuzzLiner.ACpBuf.Count;
				for (int i = 0; i < num5; i++)
				{
					NelChipPuzzleRail nelChipPuzzleRail = PuzzLiner.ACpBuf[i] as NelChipPuzzleRail;
					if (nelChipPuzzleRail != null && PuzzLiner.ARailChipActive.IndexOf(nelChipPuzzleRail) < 0)
					{
						PuzzLiner.ARailChipActive.Add(nelChipPuzzleRail);
						nelChipPuzzleRail.walk_count = walk_count;
						int[] dirs = nelChipPuzzleRail.getDirs();
						int num6 = dirs.Length;
						int j = 0;
						while (j < num6)
						{
							int num7;
							if (dirs[j] == num4)
							{
								num7 = dirs[j + 1];
							}
							else
							{
								if (dirs[j + 1] != num4)
								{
									j += 2;
									continue;
								}
								num7 = dirs[j];
							}
							if (num7 != -1)
							{
								num++;
								walk_count++;
								sx = (float)nelChipPuzzleRail.mapx + 0.5f + 0.5f * (float)CAim._XD(num7, 1);
								sy = (float)nelChipPuzzleRail.mapy + 0.5f - 0.5f * (float)CAim._YD(num7, 1);
								a = num7;
								num5 = 0;
								break;
							}
							if (num6 == 2)
							{
								sx = (float)num2 + 0.5f;
								sy = (float)num3 + 0.5f;
								break;
							}
							for (int k = 0; k < num6; k += 2)
							{
								num7 = -1;
								if (dirs[k] == -1)
								{
									num7 = dirs[k + 1];
								}
								else if (dirs[k + 1] == -1)
								{
									num7 = dirs[k];
								}
								if (num7 >= 0 && num7 != num4)
								{
									PuzzLiner.initLiner(Mp, (float)nelChipPuzzleRail.mapx + 0.5f + 0.5f * (float)CAim._XD(num7, 1), (float)nelChipPuzzleRail.mapy + 0.5f - 0.5f * (float)CAim._YD(num7, 1), switch_id, num7, walk_count + 1, immediate);
								}
							}
							num5 = 0;
							flag2 = false;
							break;
						}
					}
				}
			}
			if (flag2)
			{
				PuzzLiner.checkReceiverAt(Mp, sx, sy, switch_id, a, immediate);
			}
			if (flag && PuzzLiner.Ed == null)
			{
				PuzzLiner.Ed = Mp.setED("liner", (EffectItem Ef, M2DrawBinder Ed) => PuzzLiner.fnDrawOnEffect(Ef, Ed), 0f);
			}
		}

		public static void reloadM2DTexture(Texture Tx)
		{
			PuzzLiner.MtrRail.SetTexture("_MainTex", Tx);
		}

		private static void checkReceiverAt(Map2d Mp, float sx, float sy, int switch_id, int a, bool immediate = false)
		{
			int cpx = (int)(sx + ((CAim._XD(a, 1) < 0) ? (-0.5f) : 0f));
			int cpy = (int)(sy + ((-CAim._YD(a, 1) < 0) ? (-0.5f) : 0f));
			PuzzLiner.ALpBuf.Clear();
			List<M2LabelPoint> labelPointAll = Mp.getLabelPointAll((M2LabelPoint V) => PuzzLiner.canReceiveLp(V, cpx, cpy, a), PuzzLiner.ALpBuf);
			if (labelPointAll.Count > 0)
			{
				int count = labelPointAll.Count;
				for (int i = 0; i < count; i++)
				{
					PuzzLiner.depertAddCheck(sx, sy, switch_id, a, labelPointAll[i] as ILinerReceiver, immediate);
				}
				return;
			}
			List<M2Puts> list = Mp.findPuts(cpx, cpy, (M2Puts V, List<M2Puts> _APuts) => PuzzLiner.canReceivePuts(V, cpx, cpy, a), null);
			if (list != null && list.Count > 0)
			{
				int count2 = list.Count;
				for (int j = 0; j < count2; j++)
				{
					PuzzLiner.depertAddCheck(sx, sy, switch_id, a, list[j] as ILinerReceiver, immediate);
				}
				return;
			}
			PuzzLiner.ADepert.Add(new PuzzLiner.LinerDepart(sx, sy, switch_id, a, null));
		}

		private static void depertAddCheck(float sx, float sy, int switch_id, int a, ILinerReceiver Rcv, bool immediate)
		{
			if (Rcv == null)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			for (int i = PuzzLiner.ADepert.Count - 1; i >= 0; i--)
			{
				PuzzLiner.LinerDepart linerDepart = PuzzLiner.ADepert[i];
				if (linerDepart.Target == Rcv && linerDepart.hasSwitchId(switch_id))
				{
					if (linerDepart.t < 0f)
					{
						linerDepart.deactivate(true, false);
						PuzzLiner.ADepert.RemoveAt(i);
					}
					else
					{
						if (immediate)
						{
							linerDepart.activate(true, false);
						}
						if (linerDepart.isSame(sx, sy, a, Rcv))
						{
							flag = true;
						}
						else
						{
							flag2 = true;
						}
					}
				}
			}
			if (!flag)
			{
				PuzzLiner.LinerDepart linerDepart2 = new PuzzLiner.LinerDepart(sx, sy, switch_id, a, Rcv);
				PuzzLiner.ADepert.Add(linerDepart2);
				immediate = Rcv.initEffect(true, ref linerDepart2.RcEffect) || immediate || flag2;
				if (immediate)
				{
					if (flag2)
					{
						linerDepart2.activateDecliningSendingToTarget();
						return;
					}
					linerDepart2.activate(true, false);
				}
			}
		}

		private static bool canReceiveLp(M2LabelPoint Lp, int cpx, int cpy, int a)
		{
			return a != -1 && Lp is ILinerReceiver && X.BTW(0f, (float)(cpx - Lp.mapx), (float)Lp.mapw) && X.BTW(0f, (float)(cpy - Lp.mapy), (float)Lp.maph);
		}

		private static bool canReceivePuts(M2Puts _Cp, int cpx, int cpy, int a)
		{
			return _Cp is ILinerReceiver;
		}

		private static bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			int num = PuzzLiner.ARailChipActive.Count;
			Map2d mp = Ed.Mp;
			if (num >= 0)
			{
				MeshDrawer mesh = Ef.GetMesh("_Rail", PuzzLiner.MtrRail, false);
				mesh.base_z -= 1f;
				for (int i = 0; i < num; i++)
				{
					NelChipPuzzleRail nelChipPuzzleRail = PuzzLiner.ARailChipActive[i];
					if (nelChipPuzzleRail.Img.initAtlasMd(mesh, 0U))
					{
						mesh.Col = C32.MulA(uint.MaxValue, 0.75f + 0.25f * X.SINI(Ed.t - (float)nelChipPuzzleRail.walk_count * 1.3f, 40f));
						mesh.base_x = mp.ux2effectScreenx(mp.map2ux(nelChipPuzzleRail.mapcx));
						mesh.base_y = mp.uy2effectScreeny(mp.map2uy(nelChipPuzzleRail.mapcy));
						mesh.RotaGraph(0f, 0f, mp.base_scale, nelChipPuzzleRail.draw_rotR, null, nelChipPuzzleRail.flip);
					}
				}
			}
			if (PuzzLiner.draw_depert)
			{
				MeshDrawer mesh2 = Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
				num = PuzzLiner.ADepert.Count;
				for (int j = 0; j < num; j++)
				{
					PuzzLiner.LinerDepart linerDepart = PuzzLiner.ADepert[j];
					if (linerDepart.Target != null && linerDepart.RcEffect != null)
					{
						DRect rcEffect = linerDepart.RcEffect;
						float num2;
						if (linerDepart.t >= 0f)
						{
							num2 = X.ZLINE(linerDepart.t, 20f);
							if (linerDepart.t < 20f)
							{
								float num3 = X.ZLINE(num2, 0.85f);
								if (num3 < 1f)
								{
									float num4 = X.ZSIN(num3);
									float num5 = X.ZPOW(num3 - 0.6f, 0.39999998f);
									float num6 = 0.7853982f + 1.5707964f * num3;
									mesh2.Col = C32.d2c(2859367294U);
									mesh2.base_x = mp.ux2effectScreenx(mp.map2ux(rcEffect.x + rcEffect.width * 0.5f));
									mesh2.base_y = mp.uy2effectScreeny(mp.map2uy(rcEffect.y + rcEffect.height * 0.5f));
									float num7 = rcEffect.width * 0.5f * mp.CLENB;
									float num8 = rcEffect.height * 0.5f * mp.CLENB;
									PuzzLiner.Halo.Set(3f + 30f * num5, 30f + 40f * num5, -1f, -1f, -1f, -1f);
									for (int k = 0; k < 2; k++)
									{
										float num9 = (float)X.MPF(k == 0) * num7;
										mesh2.Line(num9, -num8 * num4, num9, num8 * num4, 1.5f, false, 0f, 0f);
										PuzzLiner.Halo.drawTo(mesh2, num9, -num8 * num4, num6, false, 3);
										PuzzLiner.Halo.drawTo(mesh2, num9, num8 * num4, num6, false, 3);
									}
									for (int l = 0; l < 2; l++)
									{
										float num10 = (float)X.MPF(l == 0) * num8;
										mesh2.Line(-num7 * num4, num10, num7 * num4, num10, 1.5f, false, 0f, 0f);
										PuzzLiner.Halo.drawTo(mesh2, -num7 * num4, num10, num6, false, 3);
										PuzzLiner.Halo.drawTo(mesh2, num7 * num4, num10, num6, false, 3);
									}
								}
							}
							mesh2.base_x = mp.ux2effectScreenx(mp.map2ux(rcEffect.x));
							mesh2.base_y = mp.uy2effectScreeny(mp.map2uy(rcEffect.y + rcEffect.height));
						}
						else
						{
							num2 = X.ZLINE(-linerDepart.t - 1f, 20f);
							mesh2.base_x = mp.ux2effectScreenx(mp.map2ux(rcEffect.x));
							mesh2.base_y = mp.uy2effectScreeny(mp.map2uy(rcEffect.y + rcEffect.height));
							float num3 = X.ZLINE(num2, 0.85f);
							if (num3 < 1f)
							{
								mesh2.Col = mesh2.ColGrd.Set(1718516606).blend(2861934764U, X.ZPOW(num3)).C;
								mesh2.StripedRectBL(0f, 0f, rcEffect.width * mp.CLENB, rcEffect.height * mp.CLENB, X.ANMPT(28, 1f), X.NI(0.25f, 0.6f, num3), 10f, false);
							}
						}
						if (num2 >= 1f)
						{
							num2 = X.ZLINE(((linerDepart.t >= 0f) ? linerDepart.t : (-linerDepart.t - 1f)) - 20f, 40f);
							if (num2 < 1f)
							{
								mesh2.Col = mesh2.ColGrd.Set(13626867).blend(2865753587U, 1f - X.ZSIN2(num2)).C;
								mesh2.RectBL(0f, 0f, rcEffect.width * mp.CLENB, rcEffect.height * mp.CLENB, false);
								mesh2.BoxBL(0f, 0f, rcEffect.width * mp.CLENB, rcEffect.height * mp.CLENB, 1f, false);
								mesh2.Col = mesh2.ColGrd.Set(4291816947U).blend(13626867U, X.ZLINE(num2 - 0.4f, 0.6f)).C;
								int num11 = 6 * (int)rcEffect.width * (int)rcEffect.height;
								for (int m = 0; m < num11; m++)
								{
									uint ran = X.GETRAN2(m * 7 + j % 6, 3 + m % 13);
									mesh2.base_x = mp.ux2effectScreenx(mp.map2ux(rcEffect.x + rcEffect.width * X.RAN(ran, 2299)));
									mesh2.base_y = mp.uy2effectScreeny(mp.map2uy(rcEffect.y + rcEffect.height * X.RAN(ran, 1205)));
									float num12 = X.RAN(ran, 2116) * 6.2831855f;
									float num13 = X.NI(22, 45, X.RAN(ran, 2434)) * X.ZSIN2(num2);
									float num14 = X.NI(2.5f, 6f, X.RAN(ran, 1569));
									mesh2.Daia(X.Cos(num12) * num13, X.Sin(num12) * num13, num14, num14, false);
								}
							}
						}
						else if (num2 >= 0.85f)
						{
							float num3 = (num2 - 0.85f) / 0.14999998f;
							mesh2.Col = mesh2.ColGrd.Set(2859367294U).blend(4287998124U, num3).C;
							mesh2.BoxBL(0f, 0f, rcEffect.width * mp.CLENB, rcEffect.height * mp.CLENB, 1f, false);
							mesh2.Col = mesh2.ColGrd.Set(7240574).blend(4287998124U, num3).C;
							mesh2.RectBL(0f, 0f, rcEffect.width * mp.CLENB, rcEffect.height * mp.CLENB, false);
						}
					}
				}
			}
			return true;
		}

		public static void run(Map2d Mp)
		{
			PuzzLiner.draw_depert = false;
			for (int i = PuzzLiner.ADepert.Count - 1; i >= 0; i--)
			{
				PuzzLiner.LinerDepart linerDepart = PuzzLiner.ADepert[i];
				if (linerDepart.Target == null)
				{
					if (linerDepart.t < 0f || linerDepart.a == -1)
					{
						PuzzLiner.ADepert.RemoveAt(i);
					}
					else if (linerDepart.t == 0f)
					{
						int num = CAim._XD(linerDepart.a, 1);
						for (int j = 0; j < 2; j++)
						{
							M2DropObject m2DropObject = Mp.DropCon.Add((M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed) => PuzzLiner.fnDrawEmptyMana(Dro, Ef, Ed), linerDepart.x + 0.12f * (float)CAim._XD(linerDepart.a, 1), linerDepart.y - 0.12f * (float)CAim._XD(linerDepart.a, 1), (float)num * X.NIXP(0.02f, 0.07f) + X.NIXP(0.008f, 0.014f) * (float)((num == 0) ? X.MPFXP() : num), (float)(-(float)CAim._YD(linerDepart.a, 1)) * X.NIXP(0.05f, 0.16f) + X.NIXP(0.012f, 0.028f), X.NIXP(2f, 4f), 60f);
							m2DropObject.type = DROP_TYPE.KILL_IF_STOP;
							m2DropObject.bounce_y_reduce = 0f;
						}
						linerDepart.t = X.NIXP(40f, 90f);
					}
					else
					{
						linerDepart.t = X.Mx(0f, linerDepart.t - Map2d.TS);
					}
				}
				else if (linerDepart.a != -1)
				{
					if (linerDepart.t >= 0f)
					{
						bool opened = linerDepart.opened;
						if (linerDepart.t < 60f)
						{
							if (Mp.floort < 10f)
							{
								linerDepart.t = 60f;
							}
							else
							{
								linerDepart.t += Map2d.TS;
							}
						}
						PuzzLiner.draw_depert = true;
						if (!opened && linerDepart.opened)
						{
							linerDepart.Target.activateLiner(false);
						}
					}
					else
					{
						bool closed = linerDepart.closed;
						if (Mp.floort < 10f)
						{
							linerDepart.t = -60f;
						}
						else
						{
							linerDepart.t -= Map2d.TS;
						}
						if (!closed && linerDepart.closed)
						{
							linerDepart.Target.deactivateLiner(false);
						}
						if (linerDepart.t <= -60f)
						{
							PuzzLiner.ADepert.RemoveAt(i);
						}
						else
						{
							PuzzLiner.draw_depert = true;
						}
					}
				}
				else
				{
					linerDepart.t += Map2d.TS;
					if (linerDepart.t < 60f)
					{
						PuzzLiner.draw_depert = true;
					}
				}
			}
			if (PuzzLiner.Ed != null && !PuzzLiner.draw_depert && PuzzLiner.ARailChipActive.Count == 0)
			{
				PuzzLiner.Ed.destruct();
				PuzzLiner.Ed = null;
			}
		}

		public static void finalizeAnimation(bool immediate = true, int switch_target = -1, bool excluding_target = false)
		{
			for (int i = PuzzLiner.ADepert.Count - 1; i >= 0; i--)
			{
				PuzzLiner.LinerDepart linerDepart = PuzzLiner.ADepert[i];
				if (linerDepart.Target != null && (switch_target < 0 || linerDepart.hasSwitchId(switch_target)) && linerDepart.a != -1)
				{
					if (linerDepart.t >= 0f)
					{
						if (!linerDepart.opened)
						{
							linerDepart.activate(immediate, excluding_target);
						}
					}
					else if (!linerDepart.closed)
					{
						linerDepart.deactivate(immediate, excluding_target);
					}
				}
			}
		}

		private static bool fnDrawEmptyMana(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			if (Dro.af_ground >= 3f)
			{
				return false;
			}
			MeshDrawer mesh = Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
			mesh.Col = C32.d2c((X.ANMT(2, 5f) == 1) ? 4286048511U : 4289785852U);
			mesh.Daia2(0f, 0f, Dro.z, 0f, false);
			return true;
		}

		private static List<M2Puts> ACpBuf;

		private static List<M2LabelPoint> ALpBuf;

		private static List<NelChipPuzzleSwitch> ASwitch;

		private static List<NelChipPuzzleConnector> AConnector;

		private static List<NelChipFootSwitch>[] AFootSwitch;

		private static List<NelChipPuzzleRail> ARailChipActive;

		private static List<PuzzLiner.LinerDepart> ADepert;

		private static bool activation_lock;

		private static bool draw_depert;

		private static Material MtrRail;

		private static HaloDrawer Halo;

		private static Map2d Mp;

		public const int SWITCH_MAX = 5;

		private static bool to_offline;

		private static M2DrawBinder Ed;

		public sealed class LinerDepart
		{
			public LinerDepart(float _x, float _y, int _switch_id, int _a, ILinerReceiver _T)
			{
				this.x = _x;
				this.y = _y;
				this.a = _a;
				this.switch_id = _switch_id;
				this.Target = _T;
			}

			public bool isSame(float _x, float _y, int _a, ILinerReceiver _T)
			{
				return this.x == _x && this.y == _y && this.a == _a && this.Target == _T;
			}

			public void deactivate(bool immediate, bool excluding_target = false)
			{
				if (this.t >= 0f)
				{
					this.t = X.Mn(-1f, -20f + this.t + 1f);
				}
				if (immediate && !this.closed)
				{
					float num = (float)(excluding_target ? (-19) : (-20));
					this.t = X.Mn(num, this.t);
					if (this.closed && this.Target != null)
					{
						this.Target.deactivateLiner(true);
					}
				}
			}

			public void activate(bool immediate, bool excluding_target = false)
			{
				bool opened = this.opened;
				float num = (float)(immediate ? (excluding_target ? 19 : 20) : 0);
				this.t = X.Mx(num, this.t);
				if (!opened && this.opened && this.Target != null)
				{
					this.Target.activateLiner(true);
				}
			}

			public bool hasSwitchId(int _switch_id)
			{
				return this.switch_id >= 0 && this.switch_id == _switch_id;
			}

			public void activateDecliningSendingToTarget()
			{
				this.t = 80f;
			}

			public bool opened
			{
				get
				{
					return this.t >= 20f;
				}
			}

			public bool closed
			{
				get
				{
					return this.t <= -20f;
				}
			}

			public ILinerReceiver Target;

			public float t;

			private int switch_id;

			public readonly int a;

			public readonly float x;

			public readonly float y;

			public bool reserved_deactivate;

			public const int FADE_T = 20;

			public const int FLASHOUT_T = 40;

			public DRect RcEffect;
		}
	}
}
