using System;
using System.Collections.Generic;
using Better;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2WaterWatcher
	{
		public M2WaterWatcher(MistManager _MIST, MistManager.MistKind _MistK, byte id)
		{
			this.MIST = _MIST;
			this.MistK = _MistK;
			this.AMerged = new List<MistManager.MistKind>(1);
			this.ANeedCheckLine = new List<M2WaterWatcher.WLine>(4);
			this.AMerged.Add(this.MistK);
			this.merged_id |= 1UL << (int)id;
			this.K_SurfaceColor = this.MistK.surface_color;
			this.K_color0 = this.MistK.color0;
			this.K_color1 = this.MistK.color1;
			this.K_water_surface_top = this.MistK.water_surface_top;
			this.Mp = this.MIST.Mp;
			int num = this.Mp.rows - this.MIST.crop * 2;
			this.AALineData = new M2WaterWatcher.WLine[num][];
			this.ReloadFlags = new Bool1024(num, null);
			this.AFallItem = new List<M2WaterWatcher.WLDraw>();
			this.OFallSnd = new BDic<uint, M2WaterWatcher.WLFallSound>();
			this.ConnectionReloadFlagsB = new Bool1024(num, null);
			this.ConnectionReloadFlagsT = new Bool1024(num, null);
			this.RcW = new DRect("rcw");
			if (M2WaterWatcher.DrawCache == null)
			{
				M2WaterWatcher.DrawCache = new List<M2WaterWatcher.WLDraw>(1);
			}
		}

		public void MistKindMerge(MistManager.MistKind MistK, byte id)
		{
			if ((this.merged_id & (1UL << (int)id)) != 0UL)
			{
				return;
			}
			this.merged_id |= 1UL << (int)id;
			this.AMerged.Add(MistK);
			float num = 1f / (float)this.AMerged.Count;
			if (!this.MIST.isWaterStaticMerged(this.merged_id) && MistK.water_surface_top >= 0)
			{
				if (this.K_water_surface_top < 0)
				{
					this.K_water_surface_top = MistK.water_surface_top;
					this.K_SurfaceColor = MistK.surface_color;
				}
				else
				{
					this.K_water_surface_top = X.Mn(MistK.water_surface_top, this.K_water_surface_top);
				}
			}
			C32 col = EffectItem.Col1;
			this.K_color0 = col.Set(this.K_color0).blend(MistK.color0, num).C;
			this.K_color1 = col.Set(this.K_color1).blend(MistK.color1, num).C;
		}

		public void clear()
		{
			this.fine_flag = true;
			this.next_fine_flag = false;
			if (this.BaseChip != null)
			{
				this.BaseChip = null;
			}
			if (this.EfCon != null)
			{
				this.EfCon.destruct();
				this.EfCon = null;
			}
			if (this.PEHandle != null)
			{
				this.releaseKeyPrSinkEffect();
			}
			if (this.SndLoop != null)
			{
				this.SndLoop = this.Mp.M2D.Snd.Environment.RemLoop("water_fall_loop", this.MistK.type.ToString());
			}
			this.posteffect_timeout = 0;
			int num = this.AALineData.Length;
			for (int i = 0; i < num; i++)
			{
				M2WaterWatcher.WLine[] array = this.AALineData[i];
				if (array != null)
				{
					int num2 = array.Length;
					for (int j = 0; j < num2; j++)
					{
						array[j].resetDraw(i + this.MIST.crop, true);
						array[j].deassignAllChips(this.Mp, this.MIST, false);
					}
					this.AALineData[i] = null;
				}
			}
		}

		public void reconsiderConfigAround(int mapy, int mapb)
		{
			if (this.AALineData == null)
			{
				return;
			}
			mapy = X.MMX(0, mapy - this.MIST.crop - 1, this.AALineData.Length - 1);
			mapb = X.MMX(0, mapb - this.MIST.crop + 1, this.AALineData.Length - 1);
			for (int i = mapy; i < mapb; i++)
			{
				this.ReloadFlags[i] = true;
			}
			this.fine_flag = true;
		}

		public void allocSize()
		{
			int num = this.Mp.rows - this.MIST.crop * 2;
			if (num != this.AALineData.Length)
			{
				Array.Resize<M2WaterWatcher.WLine[]>(ref this.AALineData, num);
				this.ReloadFlags.Alloc(num);
				this.ConnectionReloadFlagsB.Alloc(num);
				this.ConnectionReloadFlagsT.Alloc(num);
			}
		}

		public unsafe M2WaterWatcher.WLine[] getLine(int y, bool no_check_connection = false)
		{
			y -= this.MIST.crop;
			if (!X.BTW(0f, (float)y, (float)this.AALineData.Length))
			{
				return null;
			}
			M2WaterWatcher.WLine[] array = this.AALineData[y];
			if (array == null || this.ReloadFlags[y])
			{
				this.ReloadFlags[y] = false;
				List<M2WaterWatcher.WLine> alineBuf = M2WaterWatcher.ALineBuf;
				M2WaterWatcher.ALineBuf.Clear();
				int crop = this.MIST.crop;
				int num = this.Mp.clms - crop;
				this.ConnectionReloadFlagsB[y] = true;
				this.ConnectionReloadFlagsT[y] = true;
				this.ConnectionReloadFlagsB[X.Mx(y - 1, 0)] = true;
				this.ConnectionReloadFlagsT[X.Mn(y + 1, this.AALineData.Length - 1)] = true;
				fixed (MistManager.MistPD* ptr = &this.MIST.getPointDataArray()[0, 0])
				{
					MistManager.MistPD* ptr2 = ptr;
					MistManager.MistPD* ptr3 = this.MIST.getPtr(ptr2, crop, y + this.MIST.crop);
					M2WaterWatcher.WLine wline = null;
					for (int i = crop; i < num; i++)
					{
						if (ptr3->canStand(this.Mp, true, i, y + this.MIST.crop))
						{
							if (wline == null)
							{
								alineBuf.Add(wline = new M2WaterWatcher.WLine(i, y + this.MIST.crop));
							}
							else
							{
								wline.ex++;
							}
						}
						else
						{
							wline = null;
						}
						ptr3++;
					}
				}
				M2WaterWatcher.WLine[] array2 = alineBuf.ToArray();
				if (array != null)
				{
					int num2 = array.Length;
					int num3 = array2.Length;
					for (int j = 0; j < num2; j++)
					{
						M2WaterWatcher.WLine wline2 = array[j];
						bool flag = false;
						for (int k = 0; k < num3; k++)
						{
							M2WaterWatcher.WLine wline3 = array2[k];
							if (wline3.y == wline2.y && wline3.isCovering(wline2.sx, wline2.ex + 1))
							{
								wline3.initDraw(wline2, y + this.MIST.crop, flag);
								flag = true;
							}
						}
					}
					this.Mp.checkReindexAllLayer();
					for (int l = 0; l < num2; l++)
					{
						array[l].deassignAllChips(this.Mp, this.MIST, true);
					}
					this.Mp.checkReindexAllLayer();
				}
				array = (this.AALineData[y] = array2);
			}
			bool flag2 = !no_check_connection && this.ConnectionReloadFlagsB[y] && y < this.AALineData.Length - 1;
			bool flag3 = !no_check_connection && this.ConnectionReloadFlagsT[y] && y > 0;
			if (flag2 || flag3)
			{
				if (flag2)
				{
					this.ConnectionReloadFlagsB[y] = false;
				}
				if (flag3)
				{
					this.ConnectionReloadFlagsT[y] = false;
				}
				int num4 = array.Length;
				if (flag3)
				{
					M2WaterWatcher.WLine[] line = this.getLine(y + this.MIST.crop - 1, true);
					for (int m = 0; m < num4; m++)
					{
						array[m].ATop = array[m].makeConnection(line, M2WaterWatcher.ACncBuf, false);
					}
				}
				if (flag2)
				{
					M2WaterWatcher.WLine[] line2 = this.getLine(y + this.MIST.crop + 1, true);
					for (int n = 0; n < num4; n++)
					{
						array[n].ABottom = array[n].makeConnection(line2, M2WaterWatcher.ACncBuf, false);
					}
				}
			}
			return array;
		}

		public static M2WaterWatcher.WLine getLineAt(int x, M2WaterWatcher.WLine[] LD, bool get_near = false)
		{
			return M2WaterWatcher.WLine.getLineAt(x, LD, get_near);
		}

		public M2WaterWatcher.WLine getLineAt(int x, int y, bool get_near = false)
		{
			return M2WaterWatcher.WLine.getLineAt(x, this.getLine(y, false), get_near);
		}

		public bool hasPoint(int x, int y, bool get_near = false)
		{
			y -= this.MIST.crop;
			if (!X.BTW(0f, (float)y, (float)this.AALineData.Length))
			{
				return false;
			}
			M2WaterWatcher.WLine[] array = this.AALineData[y];
			return array != null && M2WaterWatcher.WLine.getLineAt(x, array, get_near) != null;
		}

		public M2WaterWatcher reloadSpecificLine(int y)
		{
			y -= this.MIST.crop;
			if (X.BTW(0f, (float)y, (float)this.AALineData.Length))
			{
				this.ReloadFlags[y] = true;
			}
			return this;
		}

		public void waveActEffect(float x, float y)
		{
			M2WaterWatcher.WLine lineAt = this.getLineAt((int)x, (int)y, false);
			if (lineAt != null)
			{
				lineAt.waveActEffect(this.Mp, x, y);
			}
		}

		private void checkInit()
		{
			this.need_checkinit = false;
			this.RcW.Set(0f, 0f, 0f, 0f);
			DRect waterExistRect = this.MIST.getWaterExistRect(M2WaterWatcher.static_area_searching);
			this.AFallItem.Clear();
			this.ANeedCheckLine.Clear();
			int num = X.Mn(this.AALineData.Length, (int)waterExistRect.bottom + 1 - this.MIST.crop);
			for (int i = X.Mx(0, (int)waterExistRect.y - 1 - this.MIST.crop); i < num; i++)
			{
				M2WaterWatcher.WLine[] array = this.AALineData[i];
				if (array != null)
				{
					int num2 = array.Length;
					for (int j = 0; j < num2; j++)
					{
						array[j].resetDraw(i + this.MIST.crop, false).checkInit();
					}
				}
			}
		}

		public unsafe void check(MistManager.MistPD* Ptr, int sx, int ex, int y)
		{
			this.check(Ptr, sx, ex, y, this.getLineAt(sx, y, true));
		}

		private unsafe void check(MistManager.MistPD* Ptr, int sx, int ex, int y, M2WaterWatcher.WLine L)
		{
			if (this.fine_flag && this.need_checkinit)
			{
				this.checkInit();
			}
			if (L == null)
			{
				X.dl("水嵩エラー", null, false, false);
				return;
			}
			this.RcW.Expand((float)sx, (float)y, (float)(ex - sx + 1), 1f, false);
			while (sx < L.sx)
			{
				sx++;
				Ptr++;
			}
			MistManager.MistPD* ptr = Ptr;
			ex = X.Mn(L.ex, ex);
			bool flag = this.fine_flag;
			int num = sx;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			M2WaterWatcher.WLDraw wldraw = null;
			bool flag2 = true;
			bool flag3 = false;
			bool flag4 = false;
			L.initDraw(null, 0, false);
			L.check_completed = true;
			int num6 = 0;
			int num7 = -1;
			int num8 = -1;
			int num9 = 0;
			bool flag5 = false;
			bool flag6 = sx == L.sx && ex == L.ex;
			for (int i = sx; i <= ex; i++)
			{
				MistManager.MistPD mistPD = *Ptr;
				bool flag7 = mistPD.aimGoesST();
				bool flag8 = mistPD.aimComeFromSt();
				bool flag9 = mistPD.aimComeFromBehind();
				int num10 = 0;
				flag3 = flag3 || mistPD.aimGoesBehind();
				L.APt[num6].come_from_top = flag9;
				if (!flag7 && flag9 && (mistPD.aim_check & (MistManager.AimCk)112) == MistManager.AimCk.NONE)
				{
					flag7 = true;
				}
				if (flag7)
				{
					M2WaterWatcher.WLine connectionB = L.getConnectionB(i);
					if (connectionB != null && ((sx == L.sx && ex == L.ex) || mistPD.canStand(this.Mp, false, i, y)) && !connectionB.water_fill_static && (connectionB.water_fill_influence || connectionB.water_fill))
					{
						flag7 = false;
						if (this.fine_flag)
						{
							num10 |= 1;
						}
					}
					if (num8 != 0)
					{
						num8 = (((mistPD.aim_check & MistManager.AimCk.BEHIND_HAS_ROOM) != MistManager.AimCk.NONE) ? 1 : 0);
					}
				}
				else if (num7 != 0)
				{
					num7 = (((mistPD.aim_check & MistManager.AimCk.BEHIND_HAS_ROOM) != MistManager.AimCk.NONE) ? 1 : 0);
				}
				if (flag7)
				{
					if (num < i && this.fine_flag)
					{
						this.createFillArea(L, num, i - 1, y);
					}
					num = i + 1;
					if (this.fine_flag)
					{
						M2WaterWatcher.WLine connectionT = L.getConnectionT(i);
						M2WaterWatcher.WLDraw wldraw2 = ((connectionT != null) ? connectionT.getDrawnFlow2Bottom(i) : null);
						if (wldraw2 != null)
						{
							if (wldraw2 != wldraw)
							{
								wldraw2.level = X.Mn(wldraw2.level, wldraw2.sy - (float)y - 1f);
								L.createDrawItem(wldraw2);
							}
							wldraw = wldraw2.expandX(i);
							if (flag8)
							{
								wldraw = null;
							}
						}
						if (wldraw2 == null)
						{
							if (wldraw != null && wldraw.sy == (float)y)
							{
								wldraw.expandX(i);
							}
							else
							{
								bool flag10 = wldraw != null && wldraw.sy != (float)y;
								wldraw = L.createDrawItem(false).Set(i, i, y, -1f);
								this.AFallItem.Add(wldraw);
								if (flag10)
								{
									wldraw.aim_bit |= 2;
								}
								else if (i > sx)
								{
									wldraw.aim_bit |= 1;
								}
							}
						}
					}
				}
				else
				{
					if (wldraw != null)
					{
						if (wldraw.sy == (float)y && (wldraw.aim_bit & 2) == 0)
						{
							wldraw.aim_bit |= 4;
						}
						wldraw = null;
					}
					flag3 = flag3 || mistPD.aimGoesBehind();
					if (!flag9)
					{
						if (flag8 || !mistPD.aimAlreadyCheckedS())
						{
							flag2 = false;
						}
						num3 += 255;
						num2 += mistPD.level255;
					}
					if (this.fine_flag)
					{
						num10 |= 2;
					}
				}
				if (num3 == 0)
				{
					num5 += 255;
					num4 += mistPD.level255;
				}
				if (num6 == 0)
				{
					num9 = (int)mistPD.step_behind;
				}
				else if (num9 != (int)mistPD.step_behind)
				{
					flag5 = true;
					num9 = X.Mx(num9, (int)mistPD.step_behind);
				}
				if (flag6 && mistPD.step != 0)
				{
					flag6 = false;
				}
				if (L.water_fill && !L.reset_waterfill_flag && (mistPD.aim_check & (MistManager.AimCk)14680064) == MistManager.AimCk.NONE)
				{
					L.reset_waterfill_flag = true;
				}
				if (L.water_fill_static && !mistPD.isWaterStatic())
				{
					L.water_fill_static = false;
				}
				if (num10 > 0)
				{
					M2WaterWatcher.WLine connectionT2 = L.getConnectionT(i);
					M2WaterWatcher.WLDraw wldraw3 = ((connectionT2 != null) ? connectionT2.getDrawnFlow2Bottom(i) : null);
					if (wldraw3 != null)
					{
						if ((wldraw3.aim_bit & 8) == 0 && mistPD.aimGoesLR())
						{
							wldraw3.level = wldraw3.sy - (float)y - 0.25f;
							wldraw3.aim_bit |= 8;
							if ((num10 & 2) != 0)
							{
								if (this.fine_flag)
								{
									this.SetWaterEffectW(WaterEffectItem.TYPE.FALL_INJECT, (float)i + 0.5f, wldraw3.width, (float)y + 0.5f, 0f, 0);
								}
								else
								{
									this.GetWaterEffectW(WaterEffectItem.TYPE.FALL_INJECT, (float)i + 0.5f, wldraw3.width, (float)y + 0.5f, -0.5f * this.Mp.CLEN, 0);
								}
							}
						}
						else
						{
							wldraw3.level = X.Mn(wldraw3.level, wldraw3.sy - (float)y - 1f);
							L.createDrawItem(wldraw3);
						}
					}
				}
				flag4 = flag4 || (mistPD.aim_check & MistManager.AimCk.ALLOW_GO_INFLUENCE) > MistManager.AimCk.NONE;
				num6++;
				Ptr++;
			}
			if (flag5)
			{
				Ptr = ptr;
				for (int j = sx; j <= ex; j++)
				{
					Ptr->setStepBehind(num9, this.MIST);
					Ptr++;
				}
			}
			if (num3 == 0)
			{
				num2 = num4;
				num3 = num5;
			}
			float num11 = X.ZLINE((float)num2, (float)num3 * 0.75f);
			if (num <= ex)
			{
				bool flag11 = num == L.sx && ex == L.ex && (num11 >= this.MistK.raise_alloc_lv || L.water_fill) && ((num7 == -1) ? (num8 == 1) : (num7 == 1));
				flag2 = flag2 && flag11;
				flag4 = flag4 && flag11;
				if (this.fine_flag)
				{
					this.createFillArea(L, num, ex, y);
				}
			}
			else
			{
				flag4 = (flag2 = false);
			}
			int num12 = L.countDrawItem(true);
			if ((flag3 || flag2) && (num11 >= 0.8f || L.water_fill))
			{
				num11 = 1f;
			}
			else
			{
				num11 *= 0.875f;
			}
			for (int k = 0; k < num12; k++)
			{
				M2WaterWatcher.WLDraw wldraw4 = L.ADrawF[k];
				if (X.BTWW(0f, wldraw4.level, 1f))
				{
					wldraw4.level = num11;
					if (flag6)
					{
						wldraw4.drawlevel = wldraw4.level;
					}
				}
				else if (X.BTWW(1f, wldraw4.level, 2f))
				{
					wldraw4.level = 1f + num11;
					if (flag6)
					{
						wldraw4.drawlevel = num11;
					}
				}
			}
			if (L.reset_waterfill_flag || L.reset_influence_flag || flag2 != L.water_fill || flag4 != L.water_fill_influence)
			{
				L.water_fill = flag2;
				L.water_fill_influence = flag4;
				L.reset_waterfill_flag = false;
				Ptr = ptr;
				for (int l = sx; l <= ex; l++)
				{
					L.reset_waterfill_flag = Ptr->setWaterBehindFlag(L.water_fill_static ? 0 : (L.water_fill ? 2 : (flag4 ? 1 : 0)), flag4 ? 1 : (L.reset_influence_flag ? 0 : 0)) || L.reset_waterfill_flag;
					Ptr++;
				}
				L.reset_influence_flag = false;
				this.next_fine_flag = !L.water_fill_static;
			}
		}

		private void createFillArea(M2WaterWatcher.WLine L, int sx, int ex, int y)
		{
			if (L.ATop == null)
			{
				return;
			}
			int num = sx;
			int num2 = 0;
			int num3 = L.ATop.Length;
			L.countDrawItem(true);
			while (num <= ex && num2 < num3)
			{
				M2WaterWatcher.WLine l = L.ATop[num2].L;
				if (num <= l.ex)
				{
					if (ex < l.sx)
					{
						break;
					}
					int num4 = l.countDrawItem(true);
					for (int i = 0; i < num4; i++)
					{
						M2WaterWatcher.WLDraw wldraw = l.ADrawF[i];
						if (wldraw.level >= 0f && wldraw.isCovering(num, ex + 1) && wldraw.level <= 0.5f)
						{
							if (num < wldraw.sx)
							{
								L.createDrawItem(true).Set(num, wldraw.sx - 1, y, -0.25f);
							}
							M2WaterWatcher.WLDraw wldraw2 = L.createDrawItem(true).Set(X.Mx(sx, wldraw.sx), X.Mn(ex, wldraw.ex), y, -0.5f);
							if ((l.splitFillDraw(i, wldraw, wldraw2.sx, wldraw2.ex) & 1) != 0)
							{
								i++;
							}
							num4 = l.countDrawItem(true);
							wldraw.level += 1f;
							num = wldraw2.ex + 1;
							if (num > ex)
							{
								break;
							}
						}
					}
				}
				num2++;
			}
			if (num <= ex)
			{
				L.createDrawItem(true).Set(num, ex, y, 1f);
			}
		}

		public void assignOrGenerateChip(int mapx, int mapy, M2Chip Cp)
		{
			M2WaterWatcher.WLine lineAt = this.getLineAt(mapx, mapy, false);
			if (lineAt == null)
			{
				X.dl("水嵩エラー", null, false, false);
				return;
			}
			bool flag = false;
			string text = "GENERATE_MIST";
			if (Cp != null)
			{
				if (this.BaseChip == null)
				{
					this.BaseChip = Cp;
				}
				text = "GENERATE_MIST_BASE";
			}
			else
			{
				if (this.BaseChip == null)
				{
					return;
				}
				Cp = this.BaseChip;
				Cp = Cp.Lay.MakeChip(Cp.Img, (int)((float)mapx * Cp.CLEN), (int)((float)mapy * Cp.CLEN), Cp.opacity, Cp.rotation, Cp.flip) as M2Chip;
				Cp.mapx = mapx;
				Cp.mapy = mapy;
				Cp.arrangeable = true;
				Cp.inputRots(true);
				flag = true;
			}
			flag = lineAt.initDraw(null, 0, false).assignChip(Cp) && flag;
			lineAt.setWaveHigh(this.Mp);
			if (Cp is NelChipWater)
			{
				NelChipWater nelChipWater = Cp as NelChipWater;
				nelChipWater.addActiveRemoveKey(text, false);
				nelChipWater.no_draw = flag;
			}
			if (flag)
			{
				Cp.Lay.assignNewMapChip(Cp, -2, true, false);
			}
			this.fine_flag = true;
		}

		public void fineDrawLevel()
		{
			int num = this.AALineData.Length;
			for (int i = 0; i < num; i++)
			{
				M2WaterWatcher.WLine[] array = this.AALineData[i];
				if (array != null)
				{
					int num2 = array.Length;
					for (int j = 0; j < num2; j++)
					{
						M2WaterWatcher.WLine wline = array[j];
						if (wline.ADrawF != null)
						{
							for (int k = wline.current_draw_max_b - 1; k >= 0; k--)
							{
								M2WaterWatcher.WLDraw wldraw = wline.ADrawB[k];
								if (wldraw.level <= -1f && wldraw.level != wldraw.drawlevel)
								{
									wldraw.drawlevel = wldraw.level;
									this.fine_flag = true;
								}
							}
						}
					}
				}
			}
		}

		public unsafe bool finalizeCheck(MistManager.MistPD* Ptr0)
		{
			bool flag = false;
			if (this.ANeedCheckLine.Count != 0)
			{
				for (int i = this.ANeedCheckLine.Count - 1; i >= 0; i--)
				{
					M2WaterWatcher.WLine wline = this.ANeedCheckLine[i];
					if (!wline.check_completed)
					{
						this.check(this.MIST.getPtr(Ptr0, wline.sx, wline.y), wline.sx, wline.ex, wline.y, wline);
					}
				}
				this.ANeedCheckLine.Clear();
			}
			this.need_checkinit = true;
			if (this.PEHandle != null && this.posteffect_timeout > 0)
			{
				int num = this.posteffect_timeout - 1;
				this.posteffect_timeout = num;
				if (num <= 0)
				{
					this.releaseKeyPrSinkEffect();
				}
				else
				{
					this.PEHandle.fine(20);
				}
			}
			if (this.fine_flag)
			{
				flag = true;
				foreach (KeyValuePair<uint, M2WaterWatcher.WLFallSound> keyValuePair in this.OFallSnd)
				{
					keyValuePair.Value.is_active = false;
				}
				if (this.AFallItem.Count != 0)
				{
					if (this.SndLoop == null || this.SndLoop.destructed)
					{
						this.SndLoop = this.Mp.M2D.Snd.Environment.AddLoop("water_fall_loop", this.MistK.type.ToString(), 0f, 0f, 7f, 4f, 2f, 2f, null);
						this.SndLoop.clearRect();
						this.OFallSnd.Clear();
					}
					int count = this.AFallItem.Count;
					for (int j = 0; j < count; j++)
					{
						M2WaterWatcher.WLDraw wldraw = this.AFallItem[j];
						float num2 = -wldraw.level;
						float num3 = (float)(wldraw.sx + wldraw.ex + 1) * 0.5f;
						float num4 = wldraw.sy + num2 * 0.5f;
						float num5 = (float)(wldraw.ex - wldraw.sx + 1) * 0.6f;
						float num6 = num2 * 0.5f;
						uint num7 = Map2d.xy2b(wldraw.sx, (int)wldraw.sy);
						M2WaterWatcher.WLFallSound wlfallSound;
						if (this.OFallSnd.TryGetValue(num7, out wlfallSound))
						{
							wlfallSound.is_active = true;
							wlfallSound.S.cx = num3;
							wlfallSound.S.cy = num4;
							wlfallSound.S.areax = num5 + 4f;
							wlfallSound.S.areay = num6 + 4f;
							wlfallSound.S.volume = X.Mx(0f, wlfallSound.S.volume);
						}
						else
						{
							Dictionary<uint, M2WaterWatcher.WLFallSound> ofallSnd = this.OFallSnd;
							uint num8 = num7;
							M2WaterWatcher.WLFallSound wlfallSound2 = new M2WaterWatcher.WLFallSound();
							wlfallSound2.S = this.SndLoop.AddArea(this.MistK.type.ToString(), num3, num4, 7f, 6f, num5 + 4f, num6 + 4f, null);
							wlfallSound2.is_active = true;
							M2WaterWatcher.WLFallSound wlfallSound3 = wlfallSound2;
							ofallSnd[num8] = wlfallSound2;
							wlfallSound = wlfallSound3;
							wlfallSound.S.volume = 0f;
							this.Mp.M2D.Snd.posFine();
						}
					}
				}
				if (this.K_water_surface_top >= 0)
				{
					if (!this.MIST.isWaterStaticMerged(this.merged_id))
					{
						int num9 = this.K_water_surface_top - this.MIST.crop;
						int num10 = -1;
						int num11 = -1;
						this.water_surface_top = -1f;
						M2WaterWatcher.WLine[] array = null;
						for (int k = (int)(this.RcW.bottom - 1f - (float)this.MIST.crop); k >= num9; k--)
						{
							array = this.AALineData[k];
							if (array == null || array.Length == 0)
							{
								array = null;
								if (num10 >= 0)
								{
									num11 = k + this.MIST.crop;
									break;
								}
							}
							else
							{
								int num12 = array.Length;
								for (int l = 0; l < num12; l++)
								{
									M2WaterWatcher.WLine wline2 = array[l];
									if (this.RcW.isCoveringXy((float)wline2.sx, (float)(k + this.MIST.crop), (float)(wline2.ex + 1), (float)(k + this.MIST.crop + 1), 0f, -1000f) && ((wline2.current_draw_max_b == 0 && wline2.current_draw_max_f == 0) || !wline2.effect_filled))
									{
										num11 = k + this.MIST.crop;
										break;
									}
								}
								if (num11 >= 0)
								{
									break;
								}
								if (num10 < 0)
								{
									num10 = k + this.MIST.crop;
								}
							}
						}
						if (num11 < 0 && num10 >= 0)
						{
							num11 = (int)this.RcW.top;
						}
						if (num11 >= 0)
						{
							this.water_surface_top = (float)X.Mx(this.K_water_surface_top, num11);
							if (array != null)
							{
								float num13 = 0f;
								float num14 = 0f;
								int num15 = array.Length;
								for (int m = 0; m < num15; m++)
								{
									M2WaterWatcher.WLine wline3 = array[m];
									if (this.RcW.isCoveringXy((float)wline3.sx, this.water_surface_top, (float)(wline3.ex + 1), this.water_surface_top + 1f, 0f, -1000f))
									{
										num13 += wline3.getFillLevel(ref num14);
									}
								}
								this.water_surface_top += 1.33f - X.ZSIN2(num13, num14);
							}
							else
							{
								this.water_surface_top += 1f;
							}
							if (this.water_surface_appeared_f == -1f)
							{
								this.water_surface_appeared_f = this.Mp.floort;
							}
						}
						else
						{
							this.water_surface_appeared_f = -1f;
						}
					}
					else
					{
						this.water_surface_top = (float)this.MIST.getStaticWaterAreaTop(this.MistK.type);
						if (this.water_surface_top >= 0f)
						{
							this.water_surface_top += 0.33f;
							if (this.water_surface_appeared_f == -1f)
							{
								this.water_surface_appeared_f = this.Mp.floort;
							}
						}
						else
						{
							this.water_surface_appeared_f = -1f;
						}
					}
				}
			}
			if (this.OFallSnd.Count > 0)
			{
				this.Mp.M2D.Snd.posFine();
				List<uint> list = null;
				foreach (KeyValuePair<uint, M2WaterWatcher.WLFallSound> keyValuePair2 in this.OFallSnd)
				{
					M2WaterWatcher.WLFallSound value = keyValuePair2.Value;
					if (!value.is_active)
					{
						value.S.volume -= 0.08f;
						if (value.S.volume < 0f)
						{
							if (list == null)
							{
								list = new List<uint>(2);
							}
							list.Add(keyValuePair2.Key);
						}
					}
					else
					{
						value.S.volume = X.VALWALK(value.S.volume, 1f, 0.08f);
					}
				}
				if (list != null)
				{
					int count2 = list.Count;
					for (int n = 0; n < count2; n++)
					{
						this.OFallSnd.Remove(list[n]);
					}
				}
			}
			this.fine_flag = this.next_fine_flag;
			this.next_fine_flag = false;
			return flag;
		}

		public WaterEffectItem SetWaterEffect(WaterEffectItem.TYPE type, float x, float y, float z = 0f, int time = 0)
		{
			if (!this.MIST.set_water_particle || M2WaterWatcher.static_area_searching)
			{
				return null;
			}
			if (this.EfCon == null)
			{
				this.EfCon = new M2WaterEffect(this);
			}
			return this.EfCon.Make(type, x, y, z, time);
		}

		public WaterEffectItem GetWaterEffect(WaterEffectItem.TYPE type, float x, float y, float z, int time)
		{
			if (this.EfCon == null || !this.MIST.set_water_particle || M2WaterWatcher.static_area_searching)
			{
				return null;
			}
			WaterEffectItem waterEffectItem = this.EfCon.Get(type, x, y);
			if (waterEffectItem != null)
			{
				waterEffectItem.makeEffect(x, y, z, time);
			}
			return waterEffectItem;
		}

		public void SetWaterEffectW(WaterEffectItem.TYPE type, float x, int width, float y, float z = 0f, int time = 0)
		{
			for (int i = 0; i < width; i++)
			{
				this.SetWaterEffect(type, x, y, z, time);
				x += 1f;
			}
		}

		public void GetWaterEffectW(WaterEffectItem.TYPE type, float x, int width, float y, float z, int time)
		{
			for (int i = 0; i < width; i++)
			{
				this.GetWaterEffect(type, x, y, z, time);
				x += 1f;
			}
		}

		public WaterEffectItem GetWaterEffect(WaterEffectItem.TYPE type, float x, float y)
		{
			if (this.EfCon == null || !this.MIST.set_water_particle || M2WaterWatcher.static_area_searching)
			{
				return null;
			}
			return this.EfCon.Get(type, x, y);
		}

		public M2Chip removeChipAt(int mapx, int mapy)
		{
			M2WaterWatcher.WLine lineAt = this.getLineAt(mapx, mapy, false);
			if (lineAt == null || !lineAt.isin(mapx))
			{
				return null;
			}
			this.ANeedCheckLine.Add(lineAt);
			M2Chip m2Chip = lineAt.removeChip(mapx);
			if (m2Chip != null)
			{
				if (m2Chip.hasActiveRemoveKey("GENERATE_MIST"))
				{
					this.Mp.removeChip(m2Chip, true, false);
				}
				else
				{
					m2Chip = null;
				}
			}
			this.fine_flag = true;
			return m2Chip;
		}

		public void drawWater(MeshDrawer Md, int fcnt)
		{
			float w = this.Mp.M2D.Cam.get_w();
			float h = this.Mp.M2D.Cam.get_h();
			DRect waterExistRect = this.MIST.getWaterExistRect(true);
			int num = X.Mx(this.MIST.crop, (int)X.Mx(this.Mp.M2D.Cam.lt_mapx - 2f, waterExistRect.x));
			int num2 = X.Mx(this.MIST.crop, (int)X.Mx(this.Mp.M2D.Cam.lt_mapy - 2f, waterExistRect.y));
			int num3 = X.Mn(this.Mp.clms - this.MIST.crop, (int)X.Mn((float)(X.IntC(this.Mp.M2D.Cam.lt_mapx + w * this.Mp.rCLEN) + 1 + 2), waterExistRect.right));
			int num4 = X.Mn(this.Mp.rows - this.MIST.crop, (int)X.Mn((float)(X.IntC(this.Mp.M2D.Cam.lt_mapy + h * this.Mp.rCLEN) + 1 + 2), waterExistRect.bottom));
			int num5 = num2 - this.MIST.crop;
			int num6 = num4 - this.MIST.crop;
			List<M2WaterWatcher.WLDraw> list = null;
			float num7 = ((this.MIST.SurfaceDraw != null) ? this.MIST.SurfaceDraw.alpha : 0f);
			if (this.water_surface_top >= 0f && this.water_surface_appeared_f != -1f)
			{
				num7 = X.VALWALK(num7, (this.MIST.water_applied_pr == 2) ? 0f : ((this.MIST.water_applied_pr == 1) ? 0.25f : 1f), 0.04f * (float)fcnt);
				if (num7 > 0f && this.water_surface_appeared_f >= 0f)
				{
					num7 *= X.ZLINE(this.Mp.floort - this.water_surface_appeared_f, 30f);
				}
			}
			for (int i = num5; i < num6; i++)
			{
				M2WaterWatcher.WLine[] array = this.AALineData[i];
				if (array != null)
				{
					int num8 = array.Length;
					int num9 = i + this.MIST.crop;
					float num10 = this.Mp.map2meshy((float)num9);
					for (int j = 0; j < num8; j++)
					{
						M2WaterWatcher.WLine wline = array[j];
						if (wline.isCovering(num, num3))
						{
							this.drawWaterLine(Md, fcnt, wline, num, num3, num9, num10, ref list);
						}
					}
				}
			}
			if (num7 >= 0f)
			{
				float num11 = this.Mp.map2meshy(this.water_surface_top);
				this.MIST.activateSurfaceReflectDrawer(num11, this.MistK.type, this.K_SurfaceColor, num7, (float)fcnt * this.MistK.surface_raise_pixel);
			}
		}

		public static float vib_xshift(Map2d Mp, float x, float y, bool another)
		{
			int num = X.IntR(x / 0.5f);
			int num2 = X.IntR(y / 0.5f);
			uint ran = X.GETRAN2((int)(Mp.floort / 8f) + num * 43 + num2 * 51 + (another ? 7 : 0), 2 + num2 % 3 + num % 5);
			return X.NI(0f, 3.5f, X.RAN(ran, 2954)) + (float)(another ? 0 : 4);
		}

		private static void Uv23(MeshDrawer Md, int count, bool first_reset_ver = true, float scalex = 1f, float scaley = 1f, float spdx = 1f, float spdy = 1f)
		{
			scalex *= 15f;
			scaley *= 15f;
			spdx *= 0.25f;
			spdy *= 0.25f;
			Md.Uv2(scalex, scaley, first_reset_ver);
			Md.Uv3(spdx, spdy, first_reset_ver);
			for (int i = 1; i < count; i++)
			{
				Md.Uv2(scalex, scaley, false);
				Md.Uv3(spdx, spdy, false);
			}
		}

		private void drawWaterLine(MeshDrawer Md, int fcnt, M2WaterWatcher.WLine L, int mapleft, int mapr, int mapy, float drawy, ref List<M2WaterWatcher.WLDraw> ADrawnAnotherLine)
		{
			float clen = this.Mp.CLEN;
			drawy -= clen;
			bool flag = false;
			Md.Col = this.K_color1;
			for (int i = 0; i < 2; i++)
			{
				List<M2WaterWatcher.WLDraw> list = ((i == 0) ? L.ADrawF : L.ADrawB);
				int num = ((i == 0) ? L.current_draw_max_f : L.current_draw_max_b);
				int j = 0;
				while (j < num)
				{
					M2WaterWatcher.WLDraw wldraw = list[j];
					if (wldraw.sy != (float)mapy)
					{
						if (ADrawnAnotherLine == null)
						{
							ADrawnAnotherLine = M2WaterWatcher.DrawCache;
							M2WaterWatcher.DrawCache.Clear();
						}
						else if (ADrawnAnotherLine.IndexOf(wldraw) >= 0)
						{
							goto IL_06F2;
						}
						ADrawnAnotherLine.Add(wldraw);
						goto IL_009D;
					}
					goto IL_009D;
					IL_06F2:
					j++;
					continue;
					IL_009D:
					if (wldraw.level == 0f)
					{
						goto IL_06F2;
					}
					float num2 = this.Mp.map2meshx((float)wldraw.sx);
					if (wldraw.level == 1f || wldraw.level == -0.25f)
					{
						Md.chooseSubMesh(2, false, false);
						M2WaterWatcher.Uv23(Md, 4, true, 1f, 1f, 1f, 1f);
						wldraw.drawlevel = 1f;
						Md.RectBL(num2 - 14f, drawy, clen * (float)wldraw.width + 28f, clen, false);
						goto IL_06F2;
					}
					if (wldraw.level == -0.5f)
					{
						Md.chooseSubMesh(2, false, false);
						M2WaterWatcher.Uv23(Md, 4, true, 1f, 1f, 1f, 1f);
						wldraw.drawlevel = 1f;
						Md.RectBL(num2 - 14f, drawy, clen * (float)wldraw.width + 28f, clen * 0.5f, false);
						goto IL_06F2;
					}
					if (wldraw.level == 2f)
					{
						Md.chooseSubMesh(2, false, false);
						M2WaterWatcher.Uv23(Md, 4, true, 1f, 1f, 1f, 1f);
						wldraw.drawlevel = 1f;
						Md.RectBL(num2 - 14f, drawy * 1.5f, clen * (float)wldraw.width + 28f, clen * 1.5f, false);
						goto IL_06F2;
					}
					if (wldraw.level <= -1f)
					{
						int num3 = (int)wldraw.sy + 1;
						float num4 = this.Mp.map2meshy((float)num3);
						int num5 = wldraw.width;
						float num6 = num2 + (float)wldraw.width * clen;
						if (wldraw.drawlevel > 0f)
						{
							wldraw.drawlevel = -X.ZLINE((wldraw.drawlevel == (float)((int)wldraw.drawlevel)) ? 1f : X.frac(wldraw.drawlevel), 0.875f);
						}
						wldraw.drawlevel = ((wldraw.drawlevel < wldraw.level) ? wldraw.level : X.VALWALK(wldraw.drawlevel, wldraw.level, 0.18f * (float)fcnt));
						float num7 = 0f;
						bool flag2 = (wldraw.aim_bit & 5) != 0;
						if (flag2)
						{
							float num8 = X.ZLINE(-wldraw.drawlevel);
							flag = true;
							int vertexMax = Md.getVertexMax();
							M2WaterWatcher.Uv23(Md, 1, true, 1f, 1f, 1f, 1f);
							for (int k = 0; k < 2; k++)
							{
								Md.chooseSubMesh(k + 1, false, false);
								Md.Col = ((k == 0) ? this.K_color0 : this.K_color1);
								WaterShakeDrawerInteractive waveDrw = L.getWaveDrw(num3, this.Mp, k == 1);
								if ((wldraw.aim_bit & 1) != 0)
								{
									if (k == 0)
									{
										num5--;
									}
									float num9 = num6 - clen;
									Md.Arc2(num9, num4, clen - M2WaterWatcher.vib_xshift(this.Mp, (float)wldraw.ex + 0.5f, (float)(num3 + 1), k == 0), clen * 0.875f + waveDrw.getShiftY(num9 + waveDrw.calc_include_draw_x) - (float)(k * 4), 1.5707964f * (1f - num8), 1.5707964f, 0f, 0f, 0f);
								}
								if ((wldraw.aim_bit & 4) != 0)
								{
									if (k == 0)
									{
										num5--;
									}
									float num9 = num2 + clen;
									Md.Arc2(num9, num4, clen - M2WaterWatcher.vib_xshift(this.Mp, (float)wldraw.sx, (float)(num3 + 1), k == 0), clen * 0.875f + waveDrw.getShiftY(num9 + waveDrw.calc_include_draw_x) - (float)(k * 4), 1.5707964f, 1.5707964f * (1f + num8), 0f, 0f, 0f);
								}
							}
							M2WaterWatcher.Uv23(Md, Md.getVertexMax() - (vertexMax + 1), false, 1f, 1f, 1f, 1f);
						}
						bool flag3 = false;
						if (num5 > 0)
						{
							Md.Col = this.K_color1;
							Md.chooseSubMesh(2, false, false);
							if (flag2)
							{
								M2WaterWatcher.Uv23(Md, 4, true, 1f, 1f, 1f, 1f);
								Md.RectBL(num2 + (((wldraw.aim_bit & 4) != 0) ? clen : 0f), num4, (float)num5 * clen, clen, false);
							}
							else
							{
								flag3 = true;
								num3--;
								num7 += 1f;
							}
						}
						if (-wldraw.drawlevel + num7 > 1f)
						{
							M2WaterWatcher.drawWaterFall(Md, this.Mp, wldraw.sx, wldraw.width, num3, 1f, -wldraw.drawlevel - 1f + num7, this.K_color0, this.K_color1, flag3, 3U, true);
						}
						Md.Col = this.K_color1;
						goto IL_06F2;
					}
					if (wldraw.drawlevel <= 0f)
					{
						wldraw.drawlevel = 0f;
					}
					flag = true;
					float num10 = X.frac(wldraw.level);
					wldraw.drawlevel = X.ZSIN(X.VALWALK(wldraw.drawlevel, num10, ((wldraw.drawlevel < num10) ? 0.09f : 0.4f) * (float)fcnt), 0.875f) * 0.875f;
					float num11 = ((wldraw.level > 1f) ? (0.5f * clen) : 0f);
					float num12 = wldraw.drawlevel * clen;
					for (int l = 0; l < 2; l++)
					{
						WaterShakeDrawerT<WaveItemInteractive> waveDrw2 = L.getWaveDrw(mapy, this.Mp, l == 1);
						Md.Col = ((l == 0) ? this.K_color0 : this.K_color1);
						int num13 = Md.getVertexMax() + 2;
						Md.chooseSubMesh(l + 1, false, false);
						M2WaterWatcher.Uv23(Md, 2, true, 1f, 1f, 1f, 1f);
						waveDrw2.drawBL(Md, num2, drawy - num11, (float)wldraw.width * clen, num11 + (num12 - (float)(l * 4)));
						M2WaterWatcher.Uv23(Md, Md.getVertexMax() - num13, false, 0f, 0f, 1f, 1f);
					}
					Md.Col = this.K_color1;
					goto IL_06F2;
				}
			}
			if (flag)
			{
				L.progressWaveDrw(this.Mp);
			}
		}

		public static void drawWaterFall(MeshDrawer Md, Map2d Mp, int s_mapx, int width, int starty, float width_ratio, float height, Color32 K_color0, Color32 K_color1, bool fall_head = true, uint draw_layer = 3U, bool use_submesh = false)
		{
			float num = height * 2f + (float)(fall_head ? 2 : 0);
			int num2 = X.IntC(num);
			float num3 = Mp.map2meshx((float)s_mapx);
			float num4 = num3 + (float)width * Mp.CLEN;
			float num5 = X.NI(num3, num4, 0.5f);
			float num6 = Mp.map2meshy((float)starty);
			for (int i = 0; i < 2; i++)
			{
				M2WaterWatcher.Uv23(Md, 2, true, 1f, 1f, 1f, 1f);
				if (((ulong)draw_layer & (ulong)(1L << (i & 31))) != 0UL)
				{
					Md.Col = ((i == 0) ? K_color0 : K_color1);
					if (use_submesh)
					{
						Md.chooseSubMesh(i + 1, false, false);
					}
					for (int j = 1; j <= num2; j++)
					{
						int num7 = j * 2;
						Md.Tri(num7 - 2, num7 - 1, num7, false).Tri(num7, num7 - 1, num7 + 1, false);
						M2WaterWatcher.Uv23(Md, 2, false, 0.7f, 2f, 14.5f, 72f);
					}
					float num8 = 0f;
					float num9 = 0f;
					float num10 = 0f;
					float num11 = 0f;
					for (int k = 0; k <= num2; k++)
					{
						float num12 = X.NI(num5, num3 + M2WaterWatcher.vib_xshift(Mp, (float)s_mapx, (float)(starty + 1) + (float)k * 0.5f, i == 0), width_ratio) * 0.015625f;
						float num13 = (num6 - Mp.CLEN * (float)k * 0.5f) * 0.015625f;
						float num14 = X.NI(num5, num4 - M2WaterWatcher.vib_xshift(Mp, (float)(s_mapx + width - 1) + 0.5f, (float)(starty + 1) + (float)k * 0.5f, i == 0), width_ratio) * 0.015625f;
						float num15 = (num6 - Mp.CLEN * (float)k * 0.5f) * 0.015625f;
						if (k == num2 && (float)k > num)
						{
							float num16 = num - (float)(k - 1);
							num12 = X.NI(num8, num12, num16);
							num13 = X.NI(num9, num13, num16);
							num14 = X.NI(num10, num14, num16);
							num15 = X.NI(num11, num15, num16);
						}
						Md.Pos(num12, num13, null);
						Md.Pos(num14, num15, null);
						num8 = num12;
						num9 = num13;
						num10 = num14;
						num11 = num15;
					}
				}
			}
		}

		public void drawDebug(MeshDrawer Md)
		{
			Md.Col = this.K_color0;
			Md.ColGrd.Set(this.K_color1);
			float w = this.Mp.M2D.Cam.get_w();
			float h = this.Mp.M2D.Cam.get_h();
			DRect waterExistRect = this.MIST.getWaterExistRect(true);
			int num = X.Mx(this.MIST.crop, (int)X.Mx(this.Mp.M2D.Cam.lt_mapx - 2f, waterExistRect.x - 1f));
			int num2 = X.Mx(this.MIST.crop, (int)X.Mx(this.Mp.M2D.Cam.lt_mapy - 2f, waterExistRect.y - 1f));
			int num3 = X.Mn(this.Mp.clms - this.MIST.crop, (int)X.Mx((float)(X.IntC(this.Mp.M2D.Cam.lt_mapx + w / this.Mp.CLEN) + 1 + 2), waterExistRect.right + 1f));
			int num4 = X.Mn(this.Mp.rows - this.MIST.crop, (int)X.Mx((float)(X.IntC(this.Mp.M2D.Cam.lt_mapy + h / this.Mp.CLEN) + 1 + 2), waterExistRect.bottom + 1f));
			int num5 = num2 - this.MIST.crop;
			int num6 = num4 - this.MIST.crop;
			Color32 color = C32.d2c(4294954061U);
			Color32 color2 = C32.d2c(4294921626U);
			float clen = this.Mp.CLEN;
			for (int i = num5; i < num6; i++)
			{
				M2WaterWatcher.WLine[] array = this.AALineData[i];
				if (array != null)
				{
					int num7 = array.Length;
					int num8 = i + this.MIST.crop;
					float num9 = this.Mp.map2meshy((float)num8) - clen;
					for (int j = 0; j < num7; j++)
					{
						M2WaterWatcher.WLine wline = array[j];
						if (wline.isCovering(num, num3))
						{
							for (int k = 0; k < 2; k++)
							{
								List<M2WaterWatcher.WLDraw> list = ((k == 0) ? wline.ADrawF : wline.ADrawB);
								int num10 = ((k == 0) ? wline.current_draw_max_f : wline.current_draw_max_b);
								for (int l = 0; l < num10; l++)
								{
									M2WaterWatcher.WLDraw wldraw = list[l];
									if (wldraw.sy == (float)num8)
									{
										float num11 = this.Mp.map2meshx((float)wldraw.sx);
										float num12 = (float)wldraw.width * clen;
										if (wldraw.level <= -1f)
										{
											Md.Col = color;
											Md.Circle(num11 + num12 * 0.5f, num9 + 0.5f * clen, 4f, 0f, false, 0f, 0f);
											Md.Line(num11 + 2f, num9 + 0.5f * clen, num11 + num12 - 2f, num9 + 0.5f * clen, 1f, false, 0f, 0f);
											Md.Line(num11 + 4f, num9 + 0.5f * clen - 4.5f, num11 + 4f, num9 + 0.5f * clen + 4.5f, 2f, false, 0f, 0f);
											Md.Line(num11 + num12 - 4f, num9 + 0.5f * clen - 4.5f, num11 + num12 - 4f, num9 + 0.5f * clen + 4.5f, 2f, false, 0f, 0f);
											Md.Line(num11 + num12 * 0.5f, num9 + 0.5f * clen, num11 + num12 * 0.5f, num9 + (1f + wldraw.level) * clen, 2f, false, 0f, 0f);
											Md.Rotate(1.5707964f, false).TranslateP(num11 + num12 * 0.5f, num9 + (1f + wldraw.level) * clen - 1f, false);
											Md.GT(-8f, 0f, 8f, 16f, 2f, false, 0f, 0f);
											Md.Identity();
										}
										else if (wldraw.level <= 1f)
										{
											Md.Col = color2;
											Md.BoxBL(num11 + 1f, num9 + 1f, num12 - 2f, (clen - 2f) * X.Abs(wldraw.level), 1f, false);
										}
										else
										{
											Md.Col = color2;
											Md.BoxBL(num11 + 1f, num9 + clen * 0.5f + 1f, num12 - 2f, clen * 0.5f + (clen - 2f) * X.Abs(wldraw.level - 1f), 1f, false);
										}
									}
								}
							}
							if (wline.water_fill)
							{
								float num13 = this.Mp.map2meshx((float)wline.sx);
								float num14 = this.Mp.map2meshx((float)(wline.ex + 1));
								Md.Col = C32.d2c(4294919750U);
								Md.Line(num13, num9 - 1f, num14, num9 - 1f, 1f, false, 0f, 0f);
							}
							if (wline.water_fill_influence)
							{
								float num15 = this.Mp.map2meshx((float)wline.sx);
								float num16 = this.Mp.map2meshx((float)(wline.ex + 1));
								Md.Col = C32.d2c(4294483975U);
								Md.Line(num15, num9 + clen - 1f, num16, num9 + clen - 1f, 1f, false, 0f, 0f);
								Md.Col = MTRX.ColWhite;
								Md.LineDashed(num15, num9 + clen + 1f, num16, num9 + clen + 1f, 0f, 7, 1f, false, 0.5f);
							}
						}
					}
				}
			}
		}

		public bool isFallingWater(int x, int y)
		{
			y -= this.MIST.crop;
			if (this.AALineData == null || !X.BTW(0f, (float)y, (float)this.AALineData.Length) || this.AALineData[y] == null)
			{
				return false;
			}
			M2WaterWatcher.WLine lineAt = this.getLineAt(x, y + this.MIST.crop, true);
			return lineAt != null && lineAt.getDrawnFlow2Bottom(x) != null;
		}

		public string checkWaterFootSound(float x, float y, int ix, int iy, ref string snd_type, bool top_empty)
		{
			iy -= this.MIST.crop;
			if (this.AALineData == null || !X.BTW(0f, y, (float)this.AALineData.Length) || this.AALineData[iy] == null)
			{
				return snd_type;
			}
			M2WaterWatcher.WLine lineAt = this.getLineAt(ix, iy + this.MIST.crop, true);
			if (lineAt == null)
			{
				return snd_type;
			}
			snd_type = null;
			if (top_empty && !this.MIST.isWholeWater())
			{
				lineAt.waveActEffect(this.Mp, x, y);
				return "water";
			}
			return "";
		}

		public bool applySinkEffect(MistManager.MistDamageApply Da)
		{
			NelM2Attacker atk = Da.Atk;
			M2Mover m2Mover = atk as M2Mover;
			if (m2Mover == null)
			{
				return true;
			}
			if (m2Mover.destructed)
			{
				return false;
			}
			M2Phys physic = m2Mover.getPhysic();
			NelM2DBase nelM2DBase = this.Mp.M2D as NelM2DBase;
			float num;
			float num2;
			atk.getMouthPosition(out num, out num2);
			if (!this.MistK.isWaterSwamp())
			{
				physic.setWaterDunk(Da.gen_id, (int)this.MistK.type);
			}
			if (Da.crt == CREATION.DEACTIVATE || Da.crt >= CREATION.ACTIVATE)
			{
				if (!(m2Mover is PR))
				{
					string text = null;
					this.checkWaterFootSound(num, num2, (int)num, (int)num2, ref text, true);
				}
				if (this.Mp.floort > 10f && !this.MIST.isWholeWater())
				{
					m2Mover.playSndPos((Da.crt >= CREATION.ACTIVATE) ? "dive_to_water" : "leave_water", 1);
				}
			}
			else
			{
				bool flag = false;
				if (Da.fall_flag)
				{
					M2WaterWatcher.WLine wline = null;
					try
					{
						wline = ((X.BTW(0f, (float)((int)num2), (float)(this.Mp.rows - this.MIST.crop)) && this.AALineData[(int)num2 - this.MIST.crop] != null) ? this.getLineAt((int)num, (int)num2, false) : null);
					}
					catch
					{
					}
					if (((wline != null) ? wline.getDrawnFlow2Bottom((int)num) : null) != null)
					{
						flag = true;
					}
				}
				if (flag)
				{
					this.SetWaterEffect(WaterEffectItem.TYPE.FALL_HIT, m2Mover.x, m2Mover.y - m2Mover.sizey * 0.75f, 0f, 0);
					if (physic != null && !m2Mover.hasFoot() && m2Mover.canApplyCarryVelocity())
					{
						physic.addFoc(FOCTYPE.KNOCKBACK | FOCTYPE._NO_CONSIDER_WATER, 0f, 0.06f, -1f, 0, 5, 0, -1, 0);
					}
				}
			}
			if (m2Mover == this.Mp.getKeyPr() && Da.crt >= CREATION.RUNNING && !X.DEBUGNOSND)
			{
				if (this.posteffect_timeout <= 0)
				{
					if (this.PEHandle == null)
					{
						this.PEHandle = new EffectHandlerPE(2);
						this.SndGogogo = nelM2DBase.Snd.createAbs("WATER_gogogo");
						this.SndBubble = nelM2DBase.Snd.createAbs("WATER_bubble");
					}
					this.PEHandle.Set(nelM2DBase.PE.setPE(POSTM.BGM_WATER, 30f, 1f, -20));
					this.SndGogogo.prepare("loop_in_water", false);
					this.SndGogogo.getPlayerInstance().SetEnvelopeReleaseTime(800f);
					this.SndGogogo.Start();
					this.SndBubble.prepare("loop_in_water_bubble", false);
					this.SndBubble.getPlayerInstance().SetEnvelopeReleaseTime(800f);
					this.SndBubble.Start();
				}
				this.posteffect_timeout = 4;
			}
			else if (this.PEHandle != null)
			{
				this.releaseKeyPrSinkEffect();
			}
			return true;
		}

		public float current_water_surface_top
		{
			get
			{
				return this.water_surface_top;
			}
		}

		public void releaseKeyPrSinkEffect()
		{
			this.posteffect_timeout = 0;
			this.PEHandle.deactivate(false);
			this.SndGogogo.Stop();
			this.SndBubble.Stop();
		}

		public readonly MistManager MIST;

		public readonly MistManager.MistKind MistK;

		private readonly List<MistManager.MistKind> AMerged;

		private ulong merged_id;

		public Color32 K_SurfaceColor;

		public Color32 K_color0;

		public Color32 K_color1;

		public int K_water_surface_top;

		public readonly Map2d Mp;

		private M2Chip BaseChip;

		public Bool1024 ReloadFlags;

		public Bool1024 ConnectionReloadFlagsB;

		public Bool1024 ConnectionReloadFlagsT;

		public M2WaterWatcher.WLine[][] AALineData;

		private bool need_checkinit = true;

		public bool fine_flag;

		public bool has_static;

		public bool next_fine_flag;

		private const float fill_thresh = 0.75f;

		private const int wave_shift_behind = 4;

		private const float max_wave_height = 0.875f;

		private static List<M2WaterWatcher.WLDraw> DrawCache;

		private M2WaterEffect EfCon;

		private int posteffect_timeout;

		private EffectHandlerPE PEHandle;

		private SndPlayer SndGogogo;

		private SndPlayer SndBubble;

		private M2SndLoopItem SndLoop;

		private readonly List<M2WaterWatcher.WLDraw> AFallItem;

		private readonly List<M2WaterWatcher.WLine> ANeedCheckLine;

		private readonly BDic<uint, M2WaterWatcher.WLFallSound> OFallSnd;

		private float water_surface_top = -1f;

		private float water_surface_appeared_f = -2f;

		private readonly DRect RcW;

		private const string snd_key = "water_fall_loop";

		public static bool static_area_searching = false;

		private static readonly List<M2WaterWatcher.WLine.WLineConnection> ACncBuf = new List<M2WaterWatcher.WLine.WLineConnection>(4);

		private static readonly List<M2WaterWatcher.WLine> ALineBuf = new List<M2WaterWatcher.WLine>(8);

		public class WLDraw
		{
			public int width
			{
				get
				{
					return this.ex - this.sx + 1;
				}
			}

			public WLDraw(M2WaterWatcher.WLDraw Src = null)
			{
				if (Src != null)
				{
					this.Set(Src);
				}
			}

			public bool isin(int _x)
			{
				return X.BTWW((float)this.sx, (float)_x, (float)this.ex);
			}

			public bool isCovering(int _x, int _r)
			{
				return X.isCovering((float)this.sx, (float)(this.ex + 1), (float)_x, (float)_r, 0f);
			}

			public M2WaterWatcher.WLDraw expandX(int _x)
			{
				this.sx = X.Mn(_x, this.sx);
				this.ex = X.Mx(_x, this.ex);
				return this;
			}

			public static List<M2WaterWatcher.WLDraw> DupeList(List<M2WaterWatcher.WLDraw> ASrc, int src_max, List<M2WaterWatcher.WLDraw> ADest = null, int si = 0)
			{
				if (ADest == null)
				{
					ADest = new List<M2WaterWatcher.WLDraw>(src_max);
				}
				for (int i = 0; i < src_max; i++)
				{
					ADest.Insert(si++, new M2WaterWatcher.WLDraw(ASrc[i]));
				}
				return ADest;
			}

			public M2WaterWatcher.WLDraw Set(M2WaterWatcher.WLDraw Dr)
			{
				this.is_active = Dr.is_active;
				this.level = Dr.level;
				this.drawlevel = Dr.drawlevel;
				this.sy = Dr.sy;
				this.sx = Dr.sx;
				this.ex = Dr.ex;
				this.aim_bit = Dr.aim_bit;
				return this;
			}

			public M2WaterWatcher.WLDraw Set(int _sx, int _ex, int _sy, float _level)
			{
				this.sx = _sx;
				this.ex = _ex;
				this.sy = (float)_sy;
				this.level = _level;
				this.aim_bit = 0;
				return this;
			}

			public M2WaterWatcher.WLDraw SetFill(M2WaterWatcher.WLine L, int _sx, int _ex, int _sy, float _level)
			{
				return this.Set((_sx <= L.sx + 1) ? L.sx : _sx, (_ex >= L.ex - 1) ? L.ex : _ex, _sy, this.level);
			}

			public bool is_active = true;

			public float level;

			public float drawlevel;

			public float sy;

			public int sx;

			public int ex;

			public int aim_bit;
		}

		public class WLFallSound
		{
			public M2SndLoopItem.M2SL_Area S;

			public bool is_active = true;
		}

		public struct WLPoint
		{
			public M2Chip Cp;

			public bool come_from_top;
		}

		public class WLine
		{
			public int width
			{
				get
				{
					return this.ex - this.sx + 1;
				}
			}

			public WLine(int _sx, int _y)
			{
				this.ex = _sx;
				this.sx = _sx;
				this.y = _y;
			}

			public bool isin(int _x)
			{
				return X.BTWW((float)this.sx, (float)_x, (float)this.ex);
			}

			public bool isCovering(int _x, int _r)
			{
				return X.isCovering((float)this.sx, (float)(this.ex + 1), (float)_x, (float)_r, 0f);
			}

			public int getLen(int _x)
			{
				if (this.isin(_x))
				{
					return 0;
				}
				if (_x <= this.ex)
				{
					return this.sx - _x;
				}
				return _x - this.ex;
			}

			public M2WaterWatcher.WLine initDraw(M2WaterWatcher.WLine SrcLine = null, int _this_y = 0, bool duplicate = false)
			{
				if (SrcLine != null && SrcLine.ADrawB != null)
				{
					if (duplicate)
					{
						this.ADrawB = new List<M2WaterWatcher.WLDraw>();
						this.ADrawF = new List<M2WaterWatcher.WLDraw>();
						this.APt = new M2WaterWatcher.WLPoint[this.width];
					}
					else if (this.ADrawB == null)
					{
						this.ADrawB = SrcLine.ADrawB;
						this.ADrawF = SrcLine.ADrawF;
						this.APt = new M2WaterWatcher.WLPoint[this.width];
						this.current_draw_max_f = SrcLine.current_draw_max_f;
						this.current_draw_max_b = SrcLine.current_draw_max_b;
					}
					else
					{
						for (int i = 0; i < SrcLine.current_draw_max_b; i++)
						{
							this.ADrawB.Insert(this.current_draw_max_b, SrcLine.ADrawB[i]);
						}
						for (int j = 0; j < SrcLine.current_draw_max_f; j++)
						{
							this.ADrawF.Insert(this.current_draw_max_f, SrcLine.ADrawF[j]);
						}
					}
					this.reset_waterfill_flag = this.reset_waterfill_flag || SrcLine.reset_waterfill_flag;
					int num = SrcLine.sx - this.sx;
					int width = SrcLine.width;
					int width2 = this.width;
					for (int k = X.Mx(0, -num); k < width; k++)
					{
						int num2 = k + num;
						if (num2 >= width2)
						{
							break;
						}
						this.APt[num2] = SrcLine.APt[k];
						SrcLine.APt[k].Cp = null;
					}
					for (int l = 0; l < 2; l++)
					{
						List<M2WaterWatcher.WLDraw> list = ((l == 0) ? this.ADrawF : this.ADrawB);
						for (int m = ((l == 0) ? this.current_draw_max_f : this.current_draw_max_b) - 1; m >= 0; m--)
						{
							M2WaterWatcher.WLDraw wldraw = list[m];
							if (wldraw.sy == (float)_this_y)
							{
								wldraw.sx = X.Mx(this.sx, wldraw.sx);
								wldraw.ex = X.Mx(this.ex, wldraw.ex);
							}
						}
					}
				}
				else if (this.ADrawB == null)
				{
					this.ADrawB = new List<M2WaterWatcher.WLDraw>();
					this.ADrawF = new List<M2WaterWatcher.WLDraw>();
					this.APt = new M2WaterWatcher.WLPoint[this.width];
				}
				return this;
			}

			public bool assignChip(M2Chip Cp)
			{
				this.initDraw(null, 0, false);
				int num = Cp.mapx - this.sx;
				if (!X.BTWW(0f, (float)num, (float)(this.ex - this.sx)))
				{
					return false;
				}
				if (this.APt[num].Cp != null)
				{
					return false;
				}
				this.APt[num].Cp = Cp;
				return true;
			}

			public M2WaterWatcher.WLine removeChip(M2Chip Cp)
			{
				if (this.ADrawF == null)
				{
					return this;
				}
				int num = Cp.mapx - this.sx;
				if (X.BTWW(0f, (float)num, (float)(this.ex - this.sx)) && this.APt[num].Cp == Cp)
				{
					this.APt[num].Cp = null;
				}
				return this;
			}

			public M2Chip removeChip(int mapx)
			{
				if (this.ADrawF == null)
				{
					return null;
				}
				int num = mapx - this.sx;
				if (X.BTWW(0f, (float)num, (float)(this.ex - this.sx)))
				{
					M2Chip cp = this.APt[num].Cp;
					this.APt[num].Cp = null;
					return cp;
				}
				return null;
			}

			public void deassignAllChips(Map2d Mp, MistManager MIST, bool no_sort = false)
			{
				if (this.ADrawF == null)
				{
					return;
				}
				int width = this.width;
				if (!no_sort)
				{
					Mp.checkReindexAllLayer();
				}
				for (int i = 0; i < width; i++)
				{
					M2Chip cp = this.APt[i].Cp;
					if (cp != null)
					{
						MIST.removeAt(cp.mapx, cp.mapy);
						if (cp.hasActiveRemoveKey("GENERATE_MIST"))
						{
							Mp.removeChip(cp, true, true);
						}
						this.APt[i].Cp = null;
					}
				}
				if (!no_sort)
				{
					Mp.checkReindexAllLayer();
				}
			}

			public M2WaterWatcher.WLine resetDraw(int _this_y, bool clear_draw_level = false)
			{
				if (this.ADrawF == null)
				{
					return this;
				}
				for (int i = 0; i < 2; i++)
				{
					List<M2WaterWatcher.WLDraw> list = ((i == 0) ? this.ADrawF : this.ADrawB);
					for (int j = ((i == 0) ? this.current_draw_max_f : this.current_draw_max_b) - 1; j >= 0; j--)
					{
						M2WaterWatcher.WLDraw wldraw = list[j];
						if (wldraw.sy != (float)_this_y)
						{
							M2WaterWatcher.WLDraw wldraw2 = (list[j] = new M2WaterWatcher.WLDraw(null));
							if (!clear_draw_level)
							{
								if (wldraw.drawlevel >= -0.5f)
								{
									wldraw2.drawlevel = wldraw.drawlevel;
								}
								else
								{
									wldraw2.drawlevel = X.Mn(0f, wldraw.drawlevel + ((float)_this_y - wldraw.sy));
								}
							}
						}
						else
						{
							wldraw.is_active = false;
							if (clear_draw_level)
							{
								wldraw.drawlevel = 0f;
							}
						}
					}
				}
				if (clear_draw_level)
				{
					this.AWav = null;
				}
				this.current_draw_max_f = (this.current_draw_max_b = 0);
				return this;
			}

			public M2WaterWatcher.WLine checkInit()
			{
				this.check_completed = false;
				return this;
			}

			public int splitFillDraw(int fi, M2WaterWatcher.WLDraw Dr, int left, int right)
			{
				if (this.ADrawF == null)
				{
					return 0;
				}
				if (left <= Dr.sx && Dr.ex <= right)
				{
					return 0;
				}
				int num = 0;
				if (Dr.sx < left && Dr.ex <= left)
				{
					M2WaterWatcher.WLDraw wldraw = new M2WaterWatcher.WLDraw(null).Set(Dr);
					this.ADrawF.Insert(fi++, wldraw);
					num |= 1;
					this.current_draw_max_f++;
					wldraw.ex = left - 1;
					Dr.sx = left;
				}
				if (Dr.sx <= right && right < Dr.ex)
				{
					M2WaterWatcher.WLDraw wldraw2 = new M2WaterWatcher.WLDraw(null).Set(Dr);
					this.ADrawF.Insert(fi + 1, wldraw2);
					num |= 2;
					this.current_draw_max_f++;
					wldraw2.sx = right + 1;
					Dr.ex = right;
				}
				return num;
			}

			public static M2WaterWatcher.WLine getLineAt(int x, M2WaterWatcher.WLine[] LD, bool get_near = false)
			{
				if (LD == null)
				{
					return null;
				}
				int num = LD.Length;
				M2WaterWatcher.WLine wline = null;
				int i = 0;
				while (i < num)
				{
					M2WaterWatcher.WLine wline2 = LD[i];
					if (x < wline2.sx)
					{
						if (!get_near)
						{
							return null;
						}
						if (wline != null && wline2.getLen(x) >= wline.getLen(x))
						{
							return wline;
						}
						return wline2;
					}
					else
					{
						if (x <= wline2.ex)
						{
							return wline2;
						}
						wline = wline2;
						i++;
					}
				}
				if (!get_near)
				{
					return null;
				}
				return wline;
			}

			public M2WaterWatcher.WLine.WLineConnection[] makeConnection(M2WaterWatcher.WLine[] LD_Another, List<M2WaterWatcher.WLine.WLineConnection> ACache, bool dont_make_array = false)
			{
				ACache.Clear();
				if (LD_Another.Length != 0)
				{
					int num = this.sx;
					int num2 = 0;
					while (num <= this.ex && num2 < LD_Another.Length)
					{
						M2WaterWatcher.WLine wline = LD_Another[num2];
						if (num <= wline.ex)
						{
							if (this.ex < wline.sx)
							{
								break;
							}
							int num3 = X.Mx(num, wline.sx);
							int num4 = X.Mn(this.ex, wline.ex);
							M2WaterWatcher.WLine.WLineConnection wlineConnection = new M2WaterWatcher.WLine.WLineConnection(num3, num4, wline);
							ACache.Add(wlineConnection);
							num = num4 + 2;
						}
						num2++;
					}
				}
				if (!dont_make_array)
				{
					return ACache.ToArray();
				}
				return null;
			}

			public M2WaterWatcher.WLine getConnectionT(int x)
			{
				if (this.ATop == null)
				{
					return null;
				}
				int num = this.ATop.Length;
				for (int i = 0; i < num; i++)
				{
					M2WaterWatcher.WLine.WLineConnection wlineConnection = this.ATop[i];
					if (x < wlineConnection.sx)
					{
						return null;
					}
					if (x <= wlineConnection.ex)
					{
						return wlineConnection.L;
					}
				}
				return null;
			}

			public M2WaterWatcher.WLine getConnectionB(int x)
			{
				if (this.ABottom == null)
				{
					return null;
				}
				int num = this.ABottom.Length;
				for (int i = 0; i < num; i++)
				{
					M2WaterWatcher.WLine.WLineConnection wlineConnection = this.ABottom[i];
					if (x < wlineConnection.sx)
					{
						return null;
					}
					if (x <= wlineConnection.ex)
					{
						return wlineConnection.L;
					}
				}
				return null;
			}

			public M2WaterWatcher.WLDraw createDrawItem(bool is_fill)
			{
				List<M2WaterWatcher.WLDraw> list = (is_fill ? this.ADrawF : this.ADrawB);
				float num = 0f;
				List<M2WaterWatcher.WLDraw> adrawF = this.ADrawF;
				int num2 = this.current_draw_max_f;
				for (int i = 0; i < num2; i++)
				{
					M2WaterWatcher.WLDraw wldraw = adrawF[i];
					if (wldraw == null)
					{
						break;
					}
					num = X.Mx(num, wldraw.drawlevel);
				}
				int num3 = (is_fill ? this.current_draw_max_f : this.current_draw_max_b);
				M2WaterWatcher.WLDraw wldraw2;
				if (num3 < list.Count)
				{
					wldraw2 = list[num3];
					wldraw2.is_active = true;
				}
				else
				{
					wldraw2 = new M2WaterWatcher.WLDraw(null);
					list.Add(wldraw2);
				}
				if (is_fill || wldraw2.drawlevel >= 0f)
				{
					wldraw2.drawlevel = X.Mx(num, wldraw2.drawlevel);
				}
				if (is_fill)
				{
					this.current_draw_max_f++;
				}
				else
				{
					this.current_draw_max_b++;
				}
				return wldraw2;
			}

			public M2WaterWatcher.WLine createDrawItem(M2WaterWatcher.WLDraw AssignDr)
			{
				bool flag = AssignDr.level >= -0.5f;
				List<M2WaterWatcher.WLDraw> list = (flag ? this.ADrawF : this.ADrawB);
				int num = (flag ? this.current_draw_max_f : this.current_draw_max_b);
				for (int i = 0; i < num; i++)
				{
					if (list[i] == AssignDr)
					{
						return this;
					}
				}
				if (num < list.Count)
				{
					list[num] = AssignDr;
					AssignDr.is_active = true;
				}
				else
				{
					list.Add(AssignDr);
				}
				if (flag)
				{
					this.current_draw_max_f++;
				}
				else
				{
					this.current_draw_max_b++;
				}
				return this;
			}

			public int countDrawItem(bool is_fill)
			{
				if (!is_fill)
				{
					return this.current_draw_max_b;
				}
				return this.current_draw_max_f;
			}

			public void progressWaveDrw(Map2d Mp)
			{
				if (this.AWav == null)
				{
					return;
				}
				for (int i = 0; i < 2; i++)
				{
					this.AWav[i].waveProgress((float)Mp.width, (int)Mp.floort);
				}
			}

			public void addWaveInteractive(Map2d Mp, float x)
			{
				if (this.AWav == null)
				{
					return;
				}
				float num = X.NIXP(0.7f, 1.3f);
				for (int i = 0; i < 2; i++)
				{
					WaterShakeDrawerInteractive waterShakeDrawerInteractive = this.AWav[i];
					waterShakeDrawerInteractive.addInteractive(Mp.map2meshx(x) + waterShakeDrawerInteractive.calc_include_draw_x, num, (int)Mp.floort);
				}
			}

			public void setWaveHigh(Map2d Mp)
			{
				if (this.AWav == null)
				{
					return;
				}
				for (int i = 0; i < 2; i++)
				{
					this.AWav[i].setWaveHigh(0.6f);
				}
			}

			public WaterShakeDrawerInteractive getWaveDrw(int this_y, Map2d Mp, bool another)
			{
				if (this.AWav == null)
				{
					this.AWav = new WaterShakeDrawerInteractive[2];
					for (int i = 0; i < 2; i++)
					{
						WaterShakeDrawerInteractive[] awav = this.AWav;
						int num = i;
						WaterShakeDrawerInteractive waterShakeDrawerInteractive = new WaterShakeDrawerInteractive(5);
						waterShakeDrawerInteractive.resolution = 14f;
						waterShakeDrawerInteractive.auto_wave_progress = false;
						waterShakeDrawerInteractive.grd_level_b = (float)(1 - i);
						waterShakeDrawerInteractive.grd_level_t = 0f;
						waterShakeDrawerInteractive.min_h = 3f;
						waterShakeDrawerInteractive.max_h = 6f;
						waterShakeDrawerInteractive.min_w = 130f;
						waterShakeDrawerInteractive.max_w = 340f;
						waterShakeDrawerInteractive.calc_include_draw_x = (float)(Mp.width / 2);
						waterShakeDrawerInteractive.level_change_speed = 0.0012f;
						waterShakeDrawerInteractive.dep_level = 0.35f;
						WaterShakeDrawerInteractive waterShakeDrawerInteractive2 = waterShakeDrawerInteractive;
						awav[num] = waterShakeDrawerInteractive;
						WaterShakeDrawerInteractive waterShakeDrawerInteractive3 = waterShakeDrawerInteractive2;
						waterShakeDrawerInteractive3.setSeedXy(3, this_y, (i == 1) ? 299 : 13);
						waterShakeDrawerInteractive3.waveProgress((float)Mp.width, (int)Mp.floort);
					}
				}
				return this.AWav[another ? 1 : 0];
			}

			public void waveActEffect(Map2d Mp, float x, float y)
			{
				if (this.ADrawF == null)
				{
					return;
				}
				int num = this.current_draw_max_f;
				int num2 = (int)x;
				for (int i = 0; i < num; i++)
				{
					M2WaterWatcher.WLDraw wldraw = this.ADrawF[i];
					if (wldraw.isin(num2) && wldraw.level != 1f && wldraw.level != 2f && wldraw.level >= 0f)
					{
						Mp.PtcN("foot_water", x, y, 0f, (int)(4U + X.xors() % 2U), 0);
						Mp.PtcN("foot_water_splash_dot", x, y, 0f, 0, 0);
						this.addWaveInteractive(Mp, x);
						return;
					}
				}
			}

			public M2WaterWatcher.WLDraw getDrawnFlow2Bottom(int x)
			{
				if (this.ADrawB == null)
				{
					return null;
				}
				int num = this.current_draw_max_b;
				for (int i = 0; i < num; i++)
				{
					M2WaterWatcher.WLDraw wldraw = this.ADrawB[i];
					if (wldraw.level <= -1f && wldraw.isin(x))
					{
						return wldraw;
					}
				}
				return null;
			}

			public M2WaterWatcher.WLDraw getFilledDrawn(int x)
			{
				if (this.ADrawF == null)
				{
					return null;
				}
				int num = this.current_draw_max_f;
				for (int i = 0; i < num; i++)
				{
					M2WaterWatcher.WLDraw wldraw = this.ADrawF[i];
					if (wldraw.level == 1f && wldraw.isin(x))
					{
						return wldraw;
					}
				}
				return null;
			}

			public float getFillLevel(ref float width_total)
			{
				float num = 0f;
				float num2 = (float)(this.ex - this.sx + 1);
				width_total += num2;
				if (this.effect_filled)
				{
					return num + num2;
				}
				for (int i = 0; i < this.current_draw_max_f; i++)
				{
					M2WaterWatcher.WLDraw wldraw = this.ADrawF[i];
					float num3 = (float)(wldraw.ex - wldraw.sx + 1);
					float num4 = wldraw.level;
					if (num4 >= 1f)
					{
						num4 -= 1f;
					}
					if (num4 <= -0.5f)
					{
						num4 = 0.5f;
					}
					else if (num4 < 0f)
					{
						num4 = 1f;
					}
					num += num4 * num3;
				}
				return num;
			}

			public M2WaterWatcher.WLDraw getFDrawn(int x)
			{
				if (this.ADrawF == null)
				{
					return null;
				}
				int num = this.current_draw_max_f;
				for (int i = 0; i < num; i++)
				{
					M2WaterWatcher.WLDraw wldraw = this.ADrawF[i];
					if (wldraw.isin(x))
					{
						return wldraw;
					}
				}
				return null;
			}

			public bool effect_filled
			{
				get
				{
					return this.water_fill_static || this.water_fill || this.water_fill_influence;
				}
			}

			public int sx;

			public int ex;

			public int y;

			public int current_draw_max_f;

			public int current_draw_max_b;

			public List<M2WaterWatcher.WLDraw> ADrawF;

			public List<M2WaterWatcher.WLDraw> ADrawB;

			public bool check_completed = true;

			public bool water_fill;

			public bool water_fill_influence;

			public bool water_fill_static = true;

			public bool reset_waterfill_flag;

			public bool reset_influence_flag = true;

			public M2WaterWatcher.WLine.WLineConnection[] ATop;

			public M2WaterWatcher.WLine.WLineConnection[] ABottom;

			public M2WaterWatcher.WLPoint[] APt;

			public WaterShakeDrawerInteractive[] AWav;

			public class WLineConnection
			{
				public WLineConnection(int _sx, M2WaterWatcher.WLine _L)
				{
					this.ex = _sx;
					this.sx = _sx;
					this.L = _L;
				}

				public WLineConnection(int _sx, int _ex, M2WaterWatcher.WLine _L)
				{
					this.sx = _sx;
					this.ex = _ex;
					this.L = _L;
				}

				public int sx;

				public int ex;

				public M2WaterWatcher.WLine L;
			}
		}
	}
}
