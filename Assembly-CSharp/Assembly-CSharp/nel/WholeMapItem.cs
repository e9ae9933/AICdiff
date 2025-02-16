using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class WholeMapItem : IBgmManageArea
	{
		public static string getTransferRectKey(global::XX.AIM aim, string jump_key)
		{
			return "Exit" + global::XX.CAim.parseInt(aim) + "_" + jump_key;
		}

		public static string getTransferOppositRectKey(global::XX.AIM aim, string jump_key)
		{
			return "Exit" + global::XX.CAim.parseInt(global::XX.CAim.get_opposite(aim)) + "_" + jump_key;
		}

		public static string getWorpOppositRectKey(global::XX.AIM aim, string jump_key)
		{
			return "ExitW" + global::XX.CAim.parseInt(global::XX.CAim.get_opposite(aim)) + "_" + jump_key;
		}

		public WholeMapItem(WholeMapManager _Con, Map2d _Mp)
		{
			this.Mp = _Mp;
			this.Con = _Con;
			this.M2D = this.Con.M2D;
			this.Bounds = new DRect(_Mp.key);
			this.text_key = this.Mp.key;
			this.Avisitted = new List<Map2d>();
			this.AReg = new List<WholeMapItem.WMRegex>();
			this.OMarker = new BDic<uint, byte>();
			this.OANoticedIcon = new BDic<Map2d, List<WMIcon>>();
			this.OASpecialIcon = new BDic<Map2d, List<WholeMapItem.WMSpecialIcon>>();
			this.OIconHiddenDeperture = new BDic<string, WMIconHiddenDeperture>();
			this.ATreasure = new List<WholeMapItem.WMTreasureBoxCache>();
			if (WholeMapItem.Tri == null)
			{
				WholeMapItem.Tri = new Triangulator();
				WholeMapItem.LplWM = new LabelPointListener<WholeMapItem.WMTransferPoint>();
				WholeMapItem.WpConOld = new LabelPointContainer<WholeMapItem.WMTransferPoint>();
			}
			WholeMapItem.need_remake_flag = true;
		}

		public WholeMapItem PrepareMapData()
		{
			if (this.M2D.GobBase == null || this.AWmi != null)
			{
				return this;
			}
			this.AWmi = new List<WholeMapItem.WMItem>();
			if (!this.Mp.loaded || !this.Mp.prepared)
			{
				this.Mp.open(this.M2D.GobBase, MAPMODE.TEMP_WHOLE, null);
				this.Mp.visible = false;
			}
			this.remake(true);
			this.M2D.initMapMaterialASync(this.Mp, 0, false);
			return this;
		}

		private static BorderDrawer<bool> createBD()
		{
			return new BorderDrawer<bool>(new Func<int, int, bool[,], bool>(global::XX.X.isExistBoolean), null);
		}

		public void remake(bool force = false)
		{
			if (!force && !WholeMapItem.need_remake_flag)
			{
				return;
			}
			WholeMapItem.need_remake_flag = false;
			this.AWmi.Clear();
			int count_layers = this.Mp.count_layers;
			bool flag = false;
			if (WholeMapItem.RECALC_ITEM_BORDER >= 0)
			{
				WholeMapItem.RECALC_ITEM_BORDER = 0;
			}
			BorderDrawer<bool> borderDrawer = null;
			for (int i = 0; i < count_layers; i++)
			{
				M2MapLayer layer = this.Mp.getLayer(i);
				Map2d map2d = this.M2D.Get(layer.name, true);
				if (map2d != null)
				{
					WholeMapItem.WMItem wmitem = new WholeMapItem.WMItem(this, layer, map2d, ref borderDrawer, flag);
					if (wmitem.isActive())
					{
						this.AWmi.Add(wmitem);
						this.FineTransferCache(wmitem);
						this.checkSpecialIconPos(wmitem, false, false);
					}
				}
			}
			int recalc_ITEM_BORDER = WholeMapItem.RECALC_ITEM_BORDER;
			this.fineSpecialIconPosWhole();
		}

		public void fineSpecialIconPosWhole()
		{
			this.completion_calc_count = 0f;
			this.completion_ratio_ = -1f;
			for (int i = this.AWmi.Count - 1; i >= 0; i--)
			{
				WholeMapItem.WMItem wmitem = this.AWmi[i];
				if (REG.match(wmitem.Lay.comment, WholeMapItem.RegWholeMapPos))
				{
					wmitem.SpecialPos = this.getSpecialPosition(REG.R1);
				}
				else
				{
					wmitem.SpecialPos = default(WholeMapItem.WMSpecialIcon);
				}
				if (!wmitem.SpecialPos.valid)
				{
					this.completion_calc_count += 1f;
				}
				if (wmitem.SrcMap == this.CurMap)
				{
					this.CurPosition = wmitem.SpecialPos;
				}
			}
		}

		public void initS(Map2d Mp, WholeMapItem.WMRegex MatchWr)
		{
			this.CurMap = Mp;
			this.CurPosition = default(WholeMapItem.WMSpecialIcon);
			WholeMapItem.WMItem wmi = this.GetWmi(Mp, null);
			if (wmi != null)
			{
				if (this.Avisitted.IndexOf(Mp) == -1)
				{
					this.Avisitted.Add(Mp);
				}
				if (!wmi.visitted)
				{
					wmi.visitted = true;
					this.completion_ratio_ = -1f;
				}
				this.CurPosition = wmi.SpecialPos;
				this.CurDgnBuffer = ((MatchWr != null && MatchWr.Dgn != null) ? MatchWr.Dgn : this.DefDungeon);
				return;
			}
			string s = Mp.Meta.GetS("wholemap_pos");
			if (TX.noe(s))
			{
				global::XX.X.de("WM レイヤーを指定しない場合、 Map側Metaに wholemap_pos で WMSpecialIcon を指定しなければなりません", null);
				return;
			}
			this.CurPosition = this.getSpecialPosition(s);
			if (this.CurPosition.valid && !this.CurPosition.Wmi.visitted)
			{
				this.CurPosition.Wmi.visitted = true;
				this.completion_ratio_ = -1f;
			}
		}

		public WholeMapItem.WMSpecialIcon getSpecialPosition(string wmkey)
		{
			if (TX.noe(wmkey))
			{
				return default(WholeMapItem.WMSpecialIcon);
			}
			int num = wmkey.IndexOf("..");
			if (num == -1)
			{
				global::XX.X.de("wholemap_pos " + wmkey + " の書式が不正 (Map..LP_name) ", null);
				return default(WholeMapItem.WMSpecialIcon);
			}
			STB stb = TX.PopBld(wmkey, 0);
			stb.Splice(0, num + 2);
			WholeMapItem.WMSpecialIcon specialPosition = this.getSpecialPosition(TX.slice(wmkey, 0, num), stb, false);
			TX.ReleaseBld(stb);
			return specialPosition;
		}

		public WholeMapItem.WMSpecialIcon getSpecialPosition(string map_key, STB lpkey, bool no_error = false)
		{
			Map2d map2d = this.M2D.Get(map_key, false);
			List<WholeMapItem.WMSpecialIcon> list;
			if (map2d == null || !this.OASpecialIcon.TryGetValue(map2d, out list))
			{
				if (!no_error)
				{
					global::XX.X.de("wholemap_pos " + map_key + " に対応するマップが見つかりません", null);
				}
				return default(WholeMapItem.WMSpecialIcon);
			}
			WholeMapItem.WMSpecialIcon wmspecialIcon = default(WholeMapItem.WMSpecialIcon);
			for (int i = list.Count - 1; i >= 0; i--)
			{
				WholeMapItem.WMSpecialIcon wmspecialIcon2 = list[i];
				if (lpkey.Equals(wmspecialIcon2.key))
				{
					wmspecialIcon = wmspecialIcon2;
					break;
				}
			}
			if (!wmspecialIcon.valid && !no_error)
			{
				lpkey.Insert(0, "wholemap_pos ");
				lpkey += " が見つかりません";
				global::XX.X.de(lpkey.ToString(), null);
				return default(WholeMapItem.WMSpecialIcon);
			}
			return wmspecialIcon;
		}

		public List<WholeMapItem.WMSpecialIcon> getSpecialPositionList(string map_key)
		{
			Map2d map2d = this.M2D.Get(map_key, false);
			List<WholeMapItem.WMSpecialIcon> list;
			if (map2d == null || !this.OASpecialIcon.TryGetValue(map2d, out list))
			{
				return null;
			}
			return list;
		}

		public void drawTo(MeshDrawer MdFill, MeshDrawer MdLine, MeshDrawer MdMask, MeshDrawer MdIco, float center_mapx, float center_mapy, float width, float height, float cell_size, float alpha = 1f, float icon_alpha = 1f, DRect DrawBounds = null)
		{
			float num = (float)global::XX.X.IntC(width / cell_size);
			float num2 = (float)global::XX.X.IntC(height / cell_size);
			float num3 = center_mapx - num / 2f;
			float num4 = center_mapx + num / 2f;
			float num5 = center_mapy - num2 / 2f;
			float num6 = center_mapy + num2 / 2f;
			float num7 = cell_size / this.Mp.CLEN;
			int count = this.AWmi.Count;
			for (int i = 0; i < count; i++)
			{
				WholeMapItem.WMItem wmitem = this.AWmi[i];
				if ((wmitem.AAmap_outline_pos != null || wmitem.Rc.w != 0f) && !wmitem.SpecialPos.valid)
				{
					uint num8;
					if (wmitem.type == WholeMapItem.WMItem.WMITYPE.SECRET)
					{
						if (!wmitem.visitted)
						{
							if (this.item_revealed == WholeMapItem.REVEALED.NONE)
							{
								goto IL_0832;
							}
							num8 = (((this.item_revealed & WholeMapItem.REVEALED.MAP) == WholeMapItem.REVEALED.NONE) ? 0U : 4286159221U);
						}
						else
						{
							num8 = 4284737186U;
						}
					}
					else
					{
						num8 = (wmitem.visitted ? 4288068337U : 4287078326U);
					}
					if (DrawBounds != null)
					{
						DrawBounds.ExpandRc(wmitem.Rc, false);
					}
					if (wmitem.Rc.isCoveringXy(num3, num5, num4, num6, 1f, -1000f))
					{
						bool flag = wmitem.visitted || wmitem.show_icon_always;
						List<WMIcon> list = (flag ? global::XX.X.Get<Map2d, List<WMIcon>>(this.OANoticedIcon, wmitem.SrcMap) : null);
						List<WholeMapItem.WMSpecialIcon> list2 = (flag ? global::XX.X.Get<Map2d, List<WholeMapItem.WMSpecialIcon>>(this.OASpecialIcon, wmitem.SrcMap) : null);
						MdFill.Col = MdFill.ColGrd.Set(num8).mulA(alpha).C;
						MdLine.Col = MdLine.ColGrd.White().mulA(alpha).C;
						MdIco.Col = C32.WMulA(alpha);
						if (wmitem.AAmap_outline_pos == null)
						{
							float num9 = this.map2meshx((float)((int)wmitem.Rc.x), center_mapx, cell_size);
							float num10 = this.map2meshy((float)((int)wmitem.Rc.bottom), center_mapy, cell_size);
							float num11 = this.map2meshx((float)((int)wmitem.Rc.right), center_mapx, cell_size);
							float num12 = this.map2meshy((float)((int)wmitem.Rc.y), center_mapy, cell_size);
							MdFill.RectBL(num9, num10, num11 - num9, num12 - num10, false);
							if (wmitem.visitted)
							{
								MdLine.Line(num9, num12, num11, num12, 2f, false, 0f, 0f);
								MdLine.Line(num11, num12, num11, num10, 2f, false, 0f, 0f);
								MdLine.Line(num11, num10, num9, num10, 2f, false, 0f, 0f);
								MdLine.Line(num9, num10, num9, num12, 2f, false, 0f, 0f);
							}
						}
						else
						{
							Vector2 vector = new Vector2(wmitem.Rc.x, wmitem.Rc.y);
							int num13 = wmitem.AAmap_outline_pos.Length;
							for (int j = 0; j < num13; j++)
							{
								Vector2[] array = wmitem.AAmap_outline_pos[j];
								int[] array2 = wmitem.AAtri[j];
								for (int k = array2.Length - 1; k >= 0; k--)
								{
									MdFill.Tri(array2[k]);
								}
								int num14 = array.Length;
								Vector2 vector2 = array[num14 - 1] + vector;
								vector2.x = this.map2meshx(vector2.x, center_mapx, cell_size);
								vector2.y = this.map2meshy(vector2.y, center_mapy, cell_size);
								for (int l = 0; l < num14; l++)
								{
									Vector2 vector3 = array[l] + vector;
									vector3.x = this.map2meshx(vector3.x, center_mapx, cell_size);
									vector3.y = this.map2meshy(vector3.y, center_mapy, cell_size);
									MdFill.PosD(vector3.x, vector3.y, null);
									if (wmitem.visitted)
									{
										MdLine.Line(vector3.x, vector3.y, vector2.x, vector2.y, 2f, false, 0f, 0f);
									}
									vector2 = vector3;
								}
							}
						}
						if (list2 != null)
						{
							int count2 = list2.Count;
							for (int m = 0; m < count2; m++)
							{
								WholeMapItem.WMSpecialIcon wmspecialIcon = list2[m];
								if (wmspecialIcon.PF != null)
								{
									float num15 = this.map2meshx(wmspecialIcon.SrcLP.mapfocx, center_mapx, cell_size);
									float num16 = this.map2meshy(wmspecialIcon.SrcLP.mapfocy, center_mapy, cell_size);
									MdIco.RotaPF(num15, num16, 1f, 1f, 0f, wmspecialIcon.PF, false, false, false, uint.MaxValue, false, 0);
								}
								else if (wmspecialIcon.arrow >= 0)
								{
									float num17 = this.map2meshx(wmspecialIcon.SrcLP.mapfocx, center_mapx, cell_size);
									float num18 = this.map2meshy(wmspecialIcon.SrcLP.mapfocy, center_mapy, cell_size);
									float num19 = (cell_size - 2f) / 2f;
									MdLine.Arrow(num17, num18, num19, global::XX.CAim.get_agR((global::XX.AIM)wmspecialIcon.arrow, 0f), 2f, false);
								}
							}
						}
						if (list != null)
						{
							int count3 = list.Count;
							float num20 = this.map2meshx(wmitem.Rc.x, center_mapx, cell_size);
							float num21 = this.map2meshy(wmitem.Rc.y, center_mapy, cell_size);
							for (int n = 0; n < count3; n++)
							{
								list[n].drawTo(MdIco, wmitem, num20, num21, cell_size, icon_alpha);
							}
						}
						if (wmitem.visitted)
						{
							LabelPointListener<WholeMapItem.WMTransferPoint> lplWM = WholeMapItem.LplWM;
							wmitem.WpCon.beginAll(lplWM);
							while (lplWM.next())
							{
								int rectLength = lplWM.cur.getRectLength();
								for (int num22 = 0; num22 < rectLength; num22++)
								{
									WholeMapItem.WMTransferPoint.WMRectItem rect = lplWM.cur.getRect(num22);
									if (((this.item_revealed & WholeMapItem.REVEALED.MAP) != WholeMapItem.REVEALED.NONE || !(rect.sf_key != "") || COOK.getSF(rect.sf_key) != 0) && !rect.no_exit_wm)
									{
										if (rect.index >= 0)
										{
											float num23 = wmitem.Rc.x + rect.x;
											float num24 = wmitem.Rc.y + rect.y;
											float num25 = wmitem.Rc.x + rect.w + rect.x;
											float num26 = wmitem.Rc.y + rect.h + rect.y;
											int index = rect.index;
											if (index == 0)
											{
												num23 = (float)((int)num23) - 0.3f;
												num25 -= 0.3f;
											}
											if (index == 1)
											{
												num24 = (float)((int)num24) - 0.3f;
												num26 -= 0.3f;
											}
											if (index == 2)
											{
												num25 = (float)((int)(num25 + 0.9f)) + 0.3f;
												num23 += 0.3f;
											}
											if (index == 3)
											{
												num26 = (float)((int)(num26 + 0.9f)) + 0.3f;
												num24 += 0.3f;
											}
											float num27 = this.map2meshx(num23, center_mapx, cell_size);
											float num28 = this.map2meshy(num26, center_mapy, cell_size);
											MdMask.RectBL(num27, num28, this.map2meshx(num25, center_mapx, cell_size) - num27 - 1f, this.map2meshy(num24, center_mapy, cell_size) - num28 - 1f, false);
										}
										else
										{
											global::XX.AIM aim = rect.getAim();
											float num29 = wmitem.Rc.x + rect.cx + (float)global::XX.CAim._XD(aim, 1);
											float num30 = wmitem.Rc.y + rect.cy - (float)global::XX.CAim._YD(aim, 1);
											float num31 = (cell_size - 2f) / 2f;
											float num32 = this.map2meshx(num29, center_mapx, cell_size);
											float num33 = this.map2meshy(num30, center_mapy, cell_size);
											MdLine.Arrow(num32, num33, num31, global::XX.CAim.get_agR(aim, 0f), 2f, false);
										}
									}
								}
							}
						}
					}
				}
				IL_0832:;
			}
		}

		public bool drawMarker(MeshDrawer MdIco, MeshDrawer MdFill, float center_mapx, float center_mapy, float width, float height, float cell_size, float alpha = 1f)
		{
			float num = (float)global::XX.X.IntC(width / cell_size);
			float num2 = (float)global::XX.X.IntC(height / cell_size);
			float num3 = center_mapx - num / 2f;
			float num4 = center_mapx + num / 2f;
			float num5 = center_mapy - num2 / 2f;
			float num6 = center_mapy + num2 / 2f;
			MdIco.Col = C32.MulA(uint.MaxValue, alpha);
			bool flag = false;
			foreach (KeyValuePair<uint, byte> keyValuePair in this.OMarker)
			{
				uint num7 = this.key2posx(keyValuePair.Key);
				uint num8 = this.key2posy(keyValuePair.Key);
				if (global::XX.X.BTWW(num3, num7, num4) && global::XX.X.BTWW(num5, num8, num6))
				{
					float num9 = this.map2meshx(num7 + 0.5f, center_mapx, cell_size);
					float num10 = this.map2meshy(num8 + 0.5f, center_mapy, cell_size);
					bool flag2 = this.ef_totalframe > 0 && num7 == (uint)this.MarkerEf.x && num8 == (uint)this.MarkerEf.y;
					if (flag2)
					{
						int num11 = IN.totalframe - this.ef_totalframe;
						float num12 = global::XX.X.ZLINE((float)num11, 24f);
						if (num12 < 1f)
						{
							flag = true;
						}
						MdIco.Col = MdIco.ColGrd.White().mulA(num12 * alpha).C;
						if (WholeMapItem.EfCircle == null)
						{
							WholeMapItem.EfCircle = new EfParticleOnce("general_white_circle", EFCON_TYPE.UI);
						}
						if (!WholeMapItem.EfCircle.drawTo(MdFill, num9, num10, (float)num11, 0f) && !flag)
						{
							this.ef_totalframe = 0;
						}
						else
						{
							flag = true;
						}
					}
					UiGmMapMarker.drawTo(MdIco, num9, num10, (int)keyValuePair.Value);
					if (flag2)
					{
						MdIco.Col = C32.MulA(uint.MaxValue, alpha);
					}
				}
			}
			return flag;
		}

		public bool prepareAllVisitted(bool progress = true)
		{
			if (this.prepared_all < 0)
			{
				return true;
			}
			if (!progress)
			{
				return false;
			}
			int num = 2;
			while (this.prepared_all < this.Avisitted.Count)
			{
				List<Map2d> avisitted = this.Avisitted;
				int num2 = this.prepared_all;
				this.prepared_all = num2 + 1;
				Map2d map2d = avisitted[num2];
				if (!map2d.prepared)
				{
					map2d.prepared = true;
					if (--num <= 0)
					{
						return false;
					}
				}
			}
			this.prepared_all = -1;
			return true;
		}

		public float map2meshx(float mappos_x, float center_mapx, float size)
		{
			return (mappos_x - center_mapx) * size;
		}

		public float map2meshy(float mappos_y, float center_mapy, float size)
		{
			return -(mappos_y - center_mapy) * size;
		}

		public bool fixPlayerPosOnWM(PR Pr, ref float x, ref float y)
		{
			if (Pr == null)
			{
				return false;
			}
			WholeMapItem.WMItem wmitem = this.GetWmi(Pr.Mp, null);
			WholeMapItem.WMSpecialIcon wmspecialIcon = default(WholeMapItem.WMSpecialIcon);
			if (wmitem == null)
			{
				wmspecialIcon = this.CurPosition;
			}
			else
			{
				wmspecialIcon = wmitem.SpecialPos;
			}
			if (wmspecialIcon.valid)
			{
				wmitem = wmspecialIcon.Wmi;
				wmitem.SrcMap.prepared = true;
				x = wmspecialIcon.SrcLP.mapfocx;
				y = wmspecialIcon.SrcLP.mapfocy;
				return true;
			}
			Vector2 positionOnWm = wmitem.getPositionOnWm(Pr.x, Pr.y, true, true);
			x = positionOnWm.x;
			y = positionOnWm.y;
			return true;
		}

		public WholeMapItem.WMItem GetWmi(Map2d Map, M2MapLayer CreationLayerInWM = null)
		{
			if (Map == null)
			{
				return null;
			}
			for (int i = this.AWmi.Count - 1; i >= 0; i--)
			{
				WholeMapItem.WMItem wmitem = this.AWmi[i];
				if (wmitem.Lay.name == Map.key)
				{
					return wmitem;
				}
			}
			if (CreationLayerInWM == null)
			{
				return null;
			}
			BorderDrawer<bool> borderDrawer = null;
			WholeMapItem.WMItem wmitem2 = new WholeMapItem.WMItem(this, CreationLayerInWM, Map, ref borderDrawer, false);
			if (!wmitem2.isActive())
			{
				return null;
			}
			this.AWmi.Add(wmitem2);
			this.FineTransferCache(wmitem2);
			this.checkSpecialIconPos(wmitem2, false, false);
			return wmitem2;
		}

		public int getWmiIndex(string name)
		{
			int count = this.AWmi.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.AWmi[i].Lay.name == name)
				{
					return i;
				}
			}
			return -1;
		}

		public WMIcon GetIconFor(Map2d _Mp, int x, int y, WMIcon.TYPE type, string sf_key = null, int margin = 0)
		{
			List<WMIcon> list;
			if (!this.OANoticedIcon.TryGetValue(_Mp, out list))
			{
				return null;
			}
			if (sf_key != null)
			{
				for (int i = list.Count - 1; i >= 0; i--)
				{
					WMIcon wmicon = list[i];
					if (wmicon.sf_key == sf_key)
					{
						return wmicon;
					}
				}
			}
			for (int j = list.Count - 1; j >= 0; j--)
			{
				WMIcon wmicon2 = list[j];
				if (global::XX.X.LENGTHXYN((float)x, (float)y, (float)wmicon2.x, (float)wmicon2.y) <= (float)margin && wmicon2.type == type)
				{
					return wmicon2;
				}
			}
			return null;
		}

		public List<WMIcon> GetIconVectorFor(Map2d _Mp)
		{
			return global::XX.X.Get<Map2d, List<WMIcon>>(this.OANoticedIcon, _Mp);
		}

		public WMIcon assignIcon(Map2d _Mp, WMIcon Ico)
		{
			List<WMIcon> list = global::XX.X.Get<Map2d, List<WMIcon>>(this.OANoticedIcon, _Mp);
			if (list == null)
			{
				list = new List<WMIcon>(1) { Ico };
				this.OANoticedIcon[_Mp] = list;
			}
			else
			{
				list.Add(Ico);
				list.Sort((WMIcon a, WMIcon b) => WholeMapItem.fnSortIconList(a, b));
			}
			return null;
		}

		public void treasureBoxOpened(Map2d curMap)
		{
			this.treasure_box_opened_ratio = -1f;
			List<WMIcon> list = global::XX.X.Get<Map2d, List<WMIcon>>(this.OANoticedIcon, curMap);
			if (list != null)
			{
				list.Sort((WMIcon a, WMIcon b) => WholeMapItem.fnSortIconList(a, b));
			}
		}

		public BDic<Map2d, List<WMIcon>> getNoticedIconObject()
		{
			return this.OANoticedIcon;
		}

		public static int fnSortIconList(WMIcon Ia, WMIcon Ib)
		{
			if (Ia.type == Ib.type)
			{
				if (Ia.y < Ib.y)
				{
					return -1;
				}
				if (Ia.y != Ib.y)
				{
					return 1;
				}
				return 0;
			}
			else
			{
				int sort_variable = Ia.sort_variable;
				int sort_variable2 = Ib.sort_variable;
				if (sort_variable < sort_variable2)
				{
					return -1;
				}
				if (sort_variable != sort_variable2)
				{
					return 1;
				}
				return 0;
			}
		}

		public WMIconHiddenDeperture assignHiddenIconDeperture(string sppos_key, WMIcon.TYPE type, Map2d _Mp, int x, int y, string sf_key = null)
		{
			if (_Mp == null)
			{
				return null;
			}
			string text = sppos_key + "__" + type.ToString();
			WMIconHiddenDeperture wmiconHiddenDeperture;
			if (this.OIconHiddenDeperture.TryGetValue(text, out wmiconHiddenDeperture))
			{
				wmiconHiddenDeperture.x = (ushort)x;
				wmiconHiddenDeperture.y = (ushort)y;
			}
			else
			{
				wmiconHiddenDeperture = (this.OIconHiddenDeperture[text] = new WMIconHiddenDeperture(_Mp, x, y));
			}
			return wmiconHiddenDeperture;
		}

		public WMIconHiddenDeperture getHiddenIconDeperture(string sppos_key, WMIcon.TYPE type)
		{
			if (sppos_key == null)
			{
				return null;
			}
			string text = sppos_key + "__" + type.ToString();
			return global::XX.X.Get<string, WMIconHiddenDeperture>(this.OIconHiddenDeperture, text);
		}

		public List<WMIconPosition> getNoticedIconList(WMIcon.TYPE type = WMIcon.TYPE.BENCH)
		{
			List<WMIconPosition> list = new List<WMIconPosition>(4);
			foreach (KeyValuePair<Map2d, List<WMIcon>> keyValuePair in this.OANoticedIcon)
			{
				List<WMIcon> value = keyValuePair.Value;
				int count = value.Count;
				WholeMapItem.WMItem wmitem = null;
				for (int i = 0; i < count; i++)
				{
					WMIcon wmicon = value[i];
					if (wmicon.type == type && wmicon.noticed)
					{
						if (wmitem == null)
						{
							wmitem = this.GetWmi(keyValuePair.Key, null);
						}
						if (wmitem != null)
						{
							list.Add(new WMIconPosition(wmicon, wmitem, this.getHiddenIconDeperture(wmicon.sppos_key, wmicon.type)));
						}
					}
				}
			}
			return list;
		}

		public void fineIconsForLabelPoint(Map2d SrcMp)
		{
			this.GetWmi(SrcMp, this.Mp.getLayer(SrcMp.key));
		}

		private void FineTransferCache(WholeMapItem.WMItem Wmi)
		{
			LabelPointContainer<M2LabelPoint> lp = Wmi.Lay.LP;
			Map2d map2d = this.M2D.Get(Wmi.Lay.name, false);
			LabelPointListener<M2LabelPoint> labelPointListener = new LabelPointListener<M2LabelPoint>();
			lp.beginAll(labelPointListener);
			Wmi.WpCon.Clear();
			while (labelPointListener.next())
			{
				if (TX.isStart(labelPointListener.cur.key, "TransferCache_", 0))
				{
					WholeMapItem.WMTransferPoint wmtransferPoint = Wmi.WpCon.Get(labelPointListener.cur.key, true, true);
					if (wmtransferPoint == null)
					{
						Wmi.WpCon.Add(wmtransferPoint = new WholeMapItem.WMTransferPoint(map2d, labelPointListener.cur));
					}
					wmtransferPoint.readFromComment(labelPointListener.cur, this.M2D);
				}
				else if (TX.isStart(labelPointListener.cur.key, "TreasureBoxCache_", 0))
				{
					string text = TX.slice(labelPointListener.cur.key, "TreasureBoxCache_".Length);
					if (TX.valid(text))
					{
						this.ATreasure.Add(new WholeMapItem.WMTreasureBoxCache(Wmi.Lay.name, text));
					}
				}
			}
			Wmi.visitted = this.Avisitted.IndexOf(map2d) >= 0;
			this.Bounds.ExpandRc(Wmi.Rc, false);
		}

		private void checkSpecialIconPos(WholeMapItem.WMItem Wmi, bool remove_already_in_this_layer = false, bool check_other_pos_refine = false)
		{
			List<WholeMapItem.WMSpecialIcon> list = global::XX.X.Get<Map2d, List<WholeMapItem.WMSpecialIcon>>(this.OASpecialIcon, Wmi.SrcMap);
			int num = ((list != null) ? list.Count : 0);
			List<WholeMapItem.WMSpecialIcon> list2 = new List<WholeMapItem.WMSpecialIcon>();
			LabelPointListener<M2LabelPoint> labelPointListener = new LabelPointListener<M2LabelPoint>();
			Wmi.Lay.LP.beginAll(labelPointListener);
			while (labelPointListener.next())
			{
				if (!TX.isStart(labelPointListener.cur.key, "TransferCache_", 0))
				{
					WholeMapItem.WMSpecialIcon wmspecialIcon = new WholeMapItem.WMSpecialIcon(labelPointListener.cur, Wmi);
					list2.Add(wmspecialIcon);
				}
			}
			if (list2.Count > 0)
			{
				this.OASpecialIcon[Wmi.SrcMap] = list2;
			}
			else
			{
				this.OASpecialIcon.Remove(Wmi.SrcMap);
			}
			if (check_other_pos_refine && (num > 0 || list2.Count > 0 || Wmi.Lay.comment.IndexOf("wholemap_pos") >= 0))
			{
				this.fineSpecialIconPosWhole();
			}
		}

		public WholeMapItem.WMTransferPoint.WMRectItem getDepertureRect(Map2d SrcMp, int aim, string jump_key)
		{
			WholeMapItem.WMItem wmi = this.GetWmi(SrcMp, this.Mp.getLayer(SrcMp.key));
			if (wmi == null)
			{
				return null;
			}
			LabelPointListener<WholeMapItem.WMTransferPoint> lplWM = WholeMapItem.LplWM;
			wmi.WpCon.beginAll(lplWM);
			while (lplWM.next())
			{
				WholeMapItem.WMTransferPoint.WMRectItem byAimAndJumpKey = lplWM.cur.getByAimAndJumpKey(aim, jump_key);
				if (byAimAndJumpKey != null)
				{
					return byAimAndJumpKey;
				}
			}
			return null;
		}

		public WholeMapItem.WMTransferPoint.WMRectItem getDepertureRectForHomeSwitch(WholeMapItem From0, WholeMapItem From1, out WholeMapItem RcTargetWmi)
		{
			RcTargetWmi = null;
			WAManager.WARecord wa = WAManager.GetWa(this.text_key, false);
			WAManager.WARecord.DepertTarget depertTarget;
			string text;
			if (wa == null || !wa.getFrontDepert(this.Con.M2D, From0, From1, out depertTarget, out text))
			{
				return null;
			}
			WholeMapItem.WMItem wmi = this.GetWmi(this.M2D.Get(text, false), null);
			if (wmi == null)
			{
				return null;
			}
			for (int i = 0; i < 2; i++)
			{
				LabelPointListener<WholeMapItem.WMTransferPoint> labelPointListener = new LabelPointListener<WholeMapItem.WMTransferPoint>();
				wmi.WpCon.beginAll(labelPointListener);
				while (labelPointListener.next())
				{
					int rectLength = labelPointListener.cur.getRectLength();
					for (int j = 0; j < rectLength; j++)
					{
						WholeMapItem.WMTransferPoint.WMRectItem rect = labelPointListener.cur.getRect(j);
						if (rect.is_worp)
						{
							WholeMapItem wholeFor = this.Con.GetWholeFor(rect.Mp, false);
							if (wholeFor != null && (i != 0 || !(wholeFor.text_key != depertTarget.wm_key)))
							{
								RcTargetWmi = wholeFor;
								return rect;
							}
						}
					}
				}
			}
			return null;
		}

		public void pushAnotherConnection(M2MapLayer WMLay, ref M2MapLayer[] ALay, ref int ARMX)
		{
			WholeMapItem.WMItem wmi = this.GetWmi(this.M2D.Get(WMLay.name, false), WMLay);
			if (wmi == null)
			{
				return;
			}
			LabelPointListener<WholeMapItem.WMTransferPoint> labelPointListener = new LabelPointListener<WholeMapItem.WMTransferPoint>();
			wmi.WpCon.beginAll(labelPointListener);
			while (labelPointListener.next())
			{
				int rectLength = labelPointListener.cur.getRectLength();
				for (int i = 0; i < rectLength; i++)
				{
					WholeMapItem.WMTransferPoint.WMRectItem rect = labelPointListener.cur.getRect(i);
					M2MapLayer layer = this.Mp.getLayer(rect.Mp.key);
					if (layer != null && global::XX.X.isinC<M2MapLayer>(ALay, layer, ARMX) == -1)
					{
						global::XX.X.pushToEmptyS<M2MapLayer>(ref ALay, layer, ref ARMX, 16);
					}
				}
			}
		}

		public WholeMapItem.WMItem getAppeardWmi(int mapx, int mapy)
		{
			if (!global::XX.X.BTW(0f, (float)mapx, (float)this.Mp.clms) || !global::XX.X.BTW(0f, (float)mapy, (float)this.Mp.rows))
			{
				return null;
			}
			int count = this.AWmi.Count;
			for (int i = 0; i < count; i++)
			{
				WholeMapItem.WMItem wmitem = this.AWmi[i];
				if (wmitem.appeared && !wmitem.SpecialPos.valid && wmitem.isin(mapx, mapy))
				{
					return wmitem;
				}
			}
			return null;
		}

		public float getCompletionRatio()
		{
			if (this.completion_ratio_ < 0f)
			{
				if (this.completion_calc_count == 0f)
				{
					this.completion_ratio_ = 0f;
				}
				else
				{
					float num = 0f;
					for (int i = this.AWmi.Count - 1; i >= 0; i--)
					{
						WholeMapItem.WMItem wmitem = this.AWmi[i];
						if (wmitem.visitted && !wmitem.SpecialPos.valid)
						{
							num += 1f;
						}
					}
					this.completion_ratio_ = num / this.completion_calc_count;
				}
			}
			return this.completion_ratio_;
		}

		public float treasure_box_opened_ratio
		{
			get
			{
				if (this.treasure_box_opened_ratio_ == -1f)
				{
					this.treasure_box_opened_ratio_ = 0f;
					if (this.ATreasure.Count == 0)
					{
						this.treasure_box_opened_ratio_ = -2f;
					}
					else
					{
						for (int i = this.ATreasure.Count - 1; i >= 0; i--)
						{
							int sf = COOK.getSF(this.ATreasure[i].getSfKey());
							this.treasure_box_opened_ratio_ += (float)((sf != 0) ? 1 : 0);
						}
						this.treasure_box_opened_ratio_ /= (float)this.ATreasure.Count;
					}
				}
				return this.treasure_box_opened_ratio_;
			}
			set
			{
				this.treasure_box_opened_ratio_ = -1f;
			}
		}

		public float getCompletionTreasureRatio()
		{
			return this.treasure_box_opened_ratio;
		}

		public int getMarkerAt(int mapx, int mapy)
		{
			if (!global::XX.X.BTW(0f, (float)mapx, (float)this.Mp.clms) || !global::XX.X.BTW(0f, (float)mapy, (float)this.Mp.rows))
			{
				return -2;
			}
			uint num = this.pos2key(mapx, mapy);
			byte b;
			if (this.OMarker.TryGetValue(num, out b))
			{
				return (int)b;
			}
			if (this.getAppeardWmi(mapx, mapy) == null)
			{
				return -2;
			}
			return -1;
		}

		public int getMarkerAt(int mapx, int mapy, WholeMapItem.WMItem WmiAppeared)
		{
			if (!global::XX.X.BTW(0f, (float)mapx, (float)this.Mp.clms) || !global::XX.X.BTW(0f, (float)mapy, (float)this.Mp.rows))
			{
				return -2;
			}
			uint num = this.pos2key(mapx, mapy);
			byte b;
			if (this.OMarker.TryGetValue(num, out b))
			{
				return (int)b;
			}
			if (WmiAppeared == null)
			{
				return -2;
			}
			return -1;
		}

		public bool setMarkerAt(Vector2 Pos, int c, bool set_effect = false)
		{
			if (!global::XX.X.BTW(0f, Pos.x, (float)this.Mp.clms) || !global::XX.X.BTW(0f, Pos.y, (float)this.Mp.rows))
			{
				return false;
			}
			uint num = this.pos2key((int)Pos.x, (int)Pos.y);
			bool flag;
			if (c < 0)
			{
				flag = this.OMarker.ContainsKey(num);
				this.OMarker.Remove(num);
			}
			else
			{
				flag = !this.OMarker.ContainsKey(num) || (int)this.OMarker[num] != c;
				this.OMarker[num] = (byte)c;
			}
			if (set_effect)
			{
				this.MarkerEf.Set(Pos.x, Pos.y);
				this.ef_totalframe = IN.totalframe;
			}
			return flag;
		}

		public void countMapMarker(BDic<UiGmMapMarker.MK, int> O)
		{
			foreach (KeyValuePair<uint, byte> keyValuePair in this.OMarker)
			{
				UiGmMapMarker.MK value = (UiGmMapMarker.MK)keyValuePair.Value;
				int num;
				if (O.TryGetValue(value, out num))
				{
					O[value] = num + 1;
				}
				else
				{
					O[value] = 1;
				}
			}
		}

		public float CLEN
		{
			get
			{
				return this.Mp.CLEN;
			}
		}

		public string localized_name
		{
			get
			{
				if (!(this.text_key == ""))
				{
					return TX.Get("Area_" + this.text_key, this.text_key);
				}
				return "";
			}
		}

		public string localized_name_areatitle
		{
			get
			{
				string text_key_areatitle = this.text_key_areatitle;
				if (!(text_key_areatitle == ""))
				{
					return TX.Get("Area_" + text_key_areatitle, text_key_areatitle);
				}
				return "";
			}
		}

		public uint pos2key(int mx, int my)
		{
			return (uint)((mx << 10) | my);
		}

		public uint key2posx(uint k)
		{
			return (k >> 10) & 1023U;
		}

		public uint key2posy(uint k)
		{
			return k & 1023U;
		}

		public WholeMapItem.WMRegex isMatch(Map2d Mp)
		{
			string key = Mp.key;
			for (int i = this.AReg.Count - 1; i >= 0; i--)
			{
				WholeMapItem.WMRegex wmregex = this.AReg[i];
				if (wmregex.Reg.Match(key).Success)
				{
					return wmregex;
				}
			}
			return null;
		}

		public Dungeon getDgn(Map2d Mp, bool alloc_empty = false)
		{
			if (Mp == this.CurMap && this.CurDgnBuffer != null)
			{
				return this.CurDgnBuffer;
			}
			WholeMapItem.WMRegex wmregex = this.isMatch(Mp);
			Dungeon dungeon = ((wmregex != null) ? wmregex.Dgn : null);
			if (dungeon != null || alloc_empty)
			{
				return dungeon;
			}
			return this.DefDungeon;
		}

		public void blurDgnBufer()
		{
			this.CurDgnBuffer = null;
		}

		public Map2d getFirstMap()
		{
			int count_layers = this.Mp.count_layers;
			for (int i = 0; i < count_layers; i++)
			{
				Map2d map2d = this.M2D.Get(this.Mp.getLayer(i).name, false);
				if (map2d != null)
				{
					return map2d;
				}
			}
			return null;
		}

		public void assignLoadMaterial(string[] Astr)
		{
			if (this.Aload_material == null)
			{
				this.Aload_material = global::XX.X.concat<string>(Astr, null, -1, -1);
				return;
			}
			this.Aload_material = global::XX.X.concat<string>(this.Aload_material, Astr, -1, -1);
		}

		public void initWmLoadMaterial()
		{
			if (this.Aload_material == null)
			{
				return;
			}
			int num = this.Aload_material.Length;
			for (int i = 0; i < num; i++)
			{
				string text = this.Aload_material[i];
				if (text.IndexOf("pxl.") == 0)
				{
					this.M2D.loadMaterialPxl(TX.slice(text, 4), "", true, true);
				}
				else if (text.IndexOf("snd.") == 0)
				{
					this.M2D.loadMaterialSnd(TX.slice(text, 4));
				}
			}
		}

		public bool getCueAndSheet(ref string cue, ref string sheet)
		{
			if (this.bgm_cue_key == null)
			{
				return false;
			}
			cue = this.bgm_cue_key;
			sheet = this.bgm_sheet_timing;
			return true;
		}

		public List<WholeMapItem.WMItem> getWMItemList()
		{
			return this.AWmi;
		}

		public bool isVisitted(string map_key)
		{
			Map2d map2d = this.M2D.Get(map_key, true);
			return map2d != null && this.Avisitted.Contains(map2d);
		}

		public bool isVisitted(Map2d Mp)
		{
			return Mp != null && this.Avisitted.Contains(Mp);
		}

		public string text_key_areatitle
		{
			get
			{
				if (this.text_key_areatitle_ == null)
				{
					return this.text_key;
				}
				return this.text_key_areatitle_;
			}
			set
			{
				this.text_key_areatitle_ = value;
			}
		}

		public void GetWMItem(string wmi_key, ref WholeMapItem.WMItem Wmi, ref WholeMapItem.WMSpecialIcon WmSpIco)
		{
			if (TX.isStart(wmi_key, "@", 0))
			{
				WmSpIco = this.getSpecialPosition(TX.slice(wmi_key, 1));
				Wmi = WmSpIco.Wmi;
				return;
			}
			Wmi = this.GetWmi(this.M2D.Get(wmi_key, false), null);
		}

		public void clearAccess()
		{
			this.Avisitted.Clear();
			this.OMarker.Clear();
			this.OANoticedIcon.Clear();
			this.OIconHiddenDeperture.Clear();
			this.treasure_box_opened_ratio_ = -1f;
			this.completion_ratio_ = -1f;
			this.item_revealed = WholeMapItem.REVEALED.NONE;
			this.reached_night_level = 0;
			this.prepared_all = 0;
		}

		public void readBinaryFrom(ByteArray Ba, int sf_version)
		{
			int num = Ba.readByte();
			int num2 = (int)Ba.readUShort();
			for (int i = 0; i < num2; i++)
			{
				string text = Ba.readString("utf-8", false);
				Map2d map2d = this.M2D.Get(text, true);
				if (map2d != null)
				{
					this.Avisitted.Add(map2d);
				}
			}
			num2 = (int)Ba.readUShort();
			for (int j = 0; j < num2; j++)
			{
				uint num3 = Ba.readUInt();
				this.OMarker[num3] = Ba.readUByte();
			}
			if (num >= 1)
			{
				num2 = (int)Ba.readUShort();
				for (int k = 0; k < num2; k++)
				{
					string text2 = Ba.readString("utf-8", false);
					this.OIconHiddenDeperture[text2] = new WMIconHiddenDeperture(Ba, this.M2D);
				}
			}
			ByteArray byteArray = Ba.readExtractBytes(4);
			num2 = (int)byteArray.readUShort();
			for (int l = 0; l < num2; l++)
			{
				string text3 = byteArray.readString("utf-8", false);
				int num4 = byteArray.readByte();
				List<WMIcon> list = new List<WMIcon>(num4);
				int num5 = 0;
				for (int m = 0; m < num4; m++)
				{
					WMIcon wmicon = new WMIcon(byteArray, num);
					if (num > 1 || wmicon.type != WMIcon.TYPE.ENEMY)
					{
						if (text3 != null)
						{
							uint num6 = <PrivateImplementationDetails>.ComputeStringHash(text3);
							if (num6 <= 2857614877U)
							{
								if (num6 <= 2082146567U)
								{
									if (num6 != 933167440U)
									{
										if (num6 == 2082146567U)
										{
											if (text3 == "forest_puzzle_burnivy")
											{
												if (wmicon.type == WMIcon.TYPE.TREASURE)
												{
													goto IL_03CD;
												}
											}
										}
									}
									else if (text3 == "forest_secret_glacier_0")
									{
										if (wmicon.type == WMIcon.TYPE.TREASURE && sf_version < 33)
										{
											goto IL_03CD;
										}
									}
								}
								else if (num6 != 2845325890U)
								{
									if (num6 == 2857614877U)
									{
										if (text3 == "forest_senzyo")
										{
											if (num <= 5 && wmicon.type == WMIcon.TYPE.TREASURE)
											{
												goto IL_03CD;
											}
										}
									}
								}
								else if (text3 == "forest_wood_nightlake")
								{
									if (wmicon.type == WMIcon.TYPE.BENCH && num <= 5 && wmicon.type == WMIcon.TYPE.BENCH)
									{
										if (num5++ >= 1)
										{
											goto IL_03CD;
										}
										wmicon.sppos_key = "SPI_Layer_bench";
										this.assignHiddenIconDeperture(wmicon.sppos_key, wmicon.type, this.M2D.Get(text3, true), (int)wmicon.x, (int)wmicon.y, null);
										wmicon.x = 16;
										wmicon.y = 15;
									}
								}
							}
							else if (num6 <= 3662074009U)
							{
								if (num6 != 3458569092U)
								{
									if (num6 == 3662074009U)
									{
										if (text3 == "forest_puzzle_ct")
										{
											if (wmicon.type == WMIcon.TYPE.TREASURE && wmicon.x == 26 && wmicon.y == 25)
											{
												goto IL_03CD;
											}
										}
									}
								}
								else if (text3 == "forest_entrance_grazia")
								{
									if (wmicon.type == WMIcon.TYPE.TREASURE && wmicon.x == 57 && wmicon.y == 14)
									{
										goto IL_03CD;
									}
								}
							}
							else if (num6 != 3850538442U)
							{
								if (num6 == 4083262388U)
								{
									if (text3 == "forest_hiroba")
									{
										if (wmicon.type == WMIcon.TYPE.TREASURE && wmicon.x == 20 && wmicon.y == 29)
										{
											goto IL_03CD;
										}
									}
								}
							}
							else if (text3 == "forest_c")
							{
								if (wmicon.type == WMIcon.TYPE.BENCH && wmicon.x == 32 && wmicon.y == 23)
								{
									goto IL_03CD;
								}
							}
						}
						list.Add(wmicon);
					}
					IL_03CD:;
				}
				Map2d map2d2 = this.M2D.Get(text3, true);
				if (map2d2 != null)
				{
					this.OANoticedIcon[map2d2] = list;
					if (num <= 6)
					{
						list.Sort((WMIcon a, WMIcon b) => WholeMapItem.fnSortIconList(a, b));
					}
				}
			}
			this.remake(false);
			num2 = this.AWmi.Count;
			for (int n = 0; n < num2; n++)
			{
				WholeMapItem.WMItem wmitem = this.AWmi[n];
				wmitem.visitted = false;
				for (int num7 = this.Avisitted.Count - 1; num7 >= 0; num7--)
				{
					if (this.Avisitted[num7] == wmitem.SrcMap)
					{
						wmitem.visitted = true;
						break;
					}
				}
			}
			this.completion_ratio_ = -1f;
			if (num >= 4)
			{
				this.item_revealed = (WholeMapItem.REVEALED)Ba.readByte();
				if (num >= 5)
				{
					this.reached_night_level = Ba.readUShort();
				}
			}
		}

		public ByteArray createBinary()
		{
			if (this.Avisitted.Count == 0 && this.OMarker.Count == 0)
			{
				return null;
			}
			ByteArray byteArray = new ByteArray(0U);
			byteArray.writeByte(7);
			int num = this.Avisitted.Count;
			byteArray.writeUShort((ushort)num);
			for (int i = 0; i < num; i++)
			{
				byteArray.writeString(this.Avisitted[i].key, "utf-8");
			}
			byteArray.writeUShort((ushort)this.OMarker.Count);
			foreach (KeyValuePair<uint, byte> keyValuePair in this.OMarker)
			{
				byteArray.writeUInt(keyValuePair.Key);
				byteArray.writeByte((int)keyValuePair.Value);
			}
			byteArray.writeUShort((ushort)this.OIconHiddenDeperture.Count);
			foreach (KeyValuePair<string, WMIconHiddenDeperture> keyValuePair2 in this.OIconHiddenDeperture)
			{
				byteArray.writeString(keyValuePair2.Key, "utf-8");
				keyValuePair2.Value.writeBinaryTo(byteArray);
			}
			ByteArray byteArray2 = new ByteArray(0U);
			num = this.OANoticedIcon.Count;
			byteArray2.writeUShort((ushort)num);
			foreach (KeyValuePair<Map2d, List<WMIcon>> keyValuePair3 in this.OANoticedIcon)
			{
				byteArray2.writeString(keyValuePair3.Key.key, "utf-8");
				List<WMIcon> value = keyValuePair3.Value;
				int count = value.Count;
				byteArray2.writeByte(count);
				for (int j = 0; j < count; j++)
				{
					value[j].writeBinaryTo(byteArray2);
				}
			}
			byteArray.writeExtractBytes(byteArray2, 4, -1);
			byteArray.writeByte((int)this.item_revealed);
			byteArray.writeUShort(this.reached_night_level);
			return byteArray;
		}

		private static int RECALC_ITEM_BORDER = -1;

		public const int WMRI_AIM_WORP = -2;

		public const string WMRI_AIM_WORP_str = "-2";

		private readonly List<Map2d> Avisitted;

		private readonly BDic<Map2d, List<WMIcon>> OANoticedIcon;

		private readonly BDic<Map2d, List<WholeMapItem.WMSpecialIcon>> OASpecialIcon;

		private readonly BDic<string, WMIconHiddenDeperture> OIconHiddenDeperture;

		private List<WholeMapItem.WMItem> AWmi;

		private float completion_calc_count;

		private float completion_ratio_ = -1f;

		private static bool need_remake_flag = true;

		private static Triangulator Tri;

		private static LabelPointListener<WholeMapItem.WMTransferPoint> LplWM;

		private static LabelPointContainer<WholeMapItem.WMTransferPoint> WpConOld;

		private readonly List<WholeMapItem.WMTreasureBoxCache> ATreasure;

		private float treasure_box_opened_ratio_;

		private int prepared_all;

		public readonly Map2d Mp;

		public readonly WholeMapManager Con;

		public readonly NelM2DBase M2D;

		public readonly DRect Bounds;

		public string text_key = "";

		private string text_key_areatitle_;

		public string bgm_sheet_timing;

		public string bgm_cue_key;

		public ushort reached_night_level;

		public string store_flush_onbattle_key;

		public StoreManager.MODE store_flush_onbattle_type;

		public string store_flush_onenter_key;

		public StoreManager.MODE store_flush_onenter_type;

		public bool safe_area;

		public bool dark_area;

		public bool default_show_icon_always;

		public string zx_evt;

		public MagicItem.FnMagicRun FD_initS;

		public MagicItem.FnMagicRun FD_initS_draw;

		public Action FD_endS;

		public string ev_omorashi;

		public string ev_mobtype = "";

		public WholeMapItem.REVEALED item_revealed;

		public Dungeon DefDungeon;

		private Dungeon CurDgnBuffer;

		public Map2d CurMap;

		public WholeMapItem.WMSpecialIcon CurPosition;

		public List<WholeMapItem.WMRegex> AReg;

		public const string cache_la_key = "TransferCache_";

		public const string cache_la_treasure_key = "TreasureBoxCache_";

		public const uint fillcol_visited = 4288068337U;

		public const uint fillcol_hidden = 4287078326U;

		public const uint fillcol_secret = 4284737186U;

		public const float GATE_WH = 0.16f;

		public BDic<uint, byte> OMarker;

		private Vector2 MarkerEf = new Vector2(0f, 0f);

		private int ef_totalframe;

		private static EfParticleOnce EfCircle;

		private string[] Aload_material;

		private static readonly Regex RegWholeMapPos = new Regex("wholemap_pos[ \\s\\t,]+([\\w\\.]+)");

		public static readonly Regex RegWholeSpiSelect = new Regex("^@([^\\.]+)\\.\\.");

		public class WMTransferPoint : DRect
		{
			public WMTransferPoint(Map2d _SrcMp, M2LabelPoint _Lp)
				: base(_Lp.key)
			{
				this.SrcMp = _SrcMp;
				this.AAimRect = new List<WholeMapItem.WMTransferPoint.WMRectItem>();
				this.PosFine(_Lp);
			}

			public void replaceLabelPoint(M2LabelPoint _Lp)
			{
				this.Lp = _Lp;
			}

			public WholeMapItem.WMTransferPoint PosFine(M2LabelPoint _Lp)
			{
				this.Lp = _Lp;
				base.Set(this.Lp.x / this.SrcMp.CLEN, this.Lp.y / this.SrcMp.CLEN, (float)global::XX.X.IntC(this.Lp.w / this.SrcMp.CLEN), (float)global::XX.X.IntC(this.Lp.h / this.SrcMp.CLEN));
				return this;
			}

			public WholeMapItem.WMTransferPoint readFromComment(M2LabelPoint Lp, NelM2DBase M2D)
			{
				this.Lp = Lp;
				META meta = new META(Lp.comment);
				this.AAimRect.Clear();
				int i = meta.GetI("whole_map_transfer_cache_count", 0, 0);
				for (int j = 0; j < i; j++)
				{
					string[] array = meta.Get("whole_map_transfer_cache" + j.ToString());
					if (array != null && array.Length >= 7)
					{
						Map2d map2d = M2D.Get(array[1], false);
						if (map2d != null)
						{
							int num = global::XX.CAim.parseString(array[2], global::XX.X.NmI(array[2], 0, false, false));
							WholeMapItem.WMTransferPoint.WMRectItem wmrectItem = new WholeMapItem.WMTransferPoint.WMRectItem(array[0], num, map2d);
							wmrectItem.Set(global::XX.X.Nm(array[3], 0f, false), global::XX.X.Nm(array[4], 0f, false), global::XX.X.Nm(array[5], 0f, false), global::XX.X.Nm(array[6], 0f, false));
							if (array.Length > 7)
							{
								wmrectItem.another_key_for_worp = array[7];
							}
							if (array.Length > 8)
							{
								wmrectItem.flags = (byte)global::XX.X.NmI(array[8], 0, false, false);
							}
							this.AAimRect.Add(wmrectItem);
							string s = meta.GetS("whole_map_transfer_need_sf_key" + j.ToString());
							if (TX.valid(s))
							{
								wmrectItem.sf_key = s;
							}
						}
					}
				}
				return this;
			}

			public int getRectLength()
			{
				return this.AAimRect.Count;
			}

			public WholeMapItem.WMTransferPoint.WMRectItem getRect(int i)
			{
				return this.AAimRect[i];
			}

			public WholeMapItem.WMTransferPoint.WMRectItem getByAimAndJumpKey(int _aim, string jump_key)
			{
				for (int i = this.AAimRect.Count - 1; i >= 0; i--)
				{
					WholeMapItem.WMTransferPoint.WMRectItem wmrectItem = this.AAimRect[i];
					if (wmrectItem.key == jump_key && wmrectItem.index == _aim)
					{
						return wmrectItem;
					}
				}
				return null;
			}

			public readonly Map2d SrcMp;

			private M2LabelPoint Lp;

			private readonly List<WholeMapItem.WMTransferPoint.WMRectItem> AAimRect;

			public class WMRectItem : M2Rect
			{
				public bool walk_in
				{
					get
					{
						return (this.flags & 1) > 0;
					}
					set
					{
						if (value)
						{
							this.flags |= 1;
							return;
						}
						this.flags = (byte)((int)this.flags & -2);
					}
				}

				public bool sync_pos
				{
					get
					{
						return (this.flags & 2) > 0;
					}
					set
					{
						if (value)
						{
							this.flags |= 2;
							return;
						}
						this.flags = (byte)((int)this.flags & -3);
					}
				}

				public bool no_exit_wm
				{
					get
					{
						return (this.flags & 4) > 0;
					}
					set
					{
						if (value)
						{
							this.flags |= 4;
							return;
						}
						this.flags = (byte)((int)this.flags & -5);
					}
				}

				public WMRectItem(string _key, int _aim, Map2d _Mp)
					: base(_key, _aim, _Mp)
				{
				}

				public bool is_worp
				{
					get
					{
						return this.index == -2;
					}
				}

				public global::XX.AIM getAim()
				{
					if (this.index >= 0)
					{
						return (global::XX.AIM)this.index;
					}
					if (this.cache_aim != 255)
					{
						return (global::XX.AIM)this.cache_aim;
					}
					if (REG.match(this.key, M2LpMapTransfer.RegForAimCalc))
					{
						this.cache_aim = (byte)global::XX.CAim.parseString(REG.R1, 0);
						return (global::XX.AIM)this.cache_aim;
					}
					this.cache_aim = 3;
					return (global::XX.AIM)this.cache_aim;
				}

				public string getAnotherLabelPointKey()
				{
					if (this.index >= 0)
					{
						return WholeMapItem.getTransferOppositRectKey((global::XX.AIM)this.index, this.key);
					}
					return this.another_key_for_worp;
				}

				public static WholeMapItem.WMTransferPoint.WMRectItem createTemporary(Map2d SrcMp, string hash)
				{
					int num = -1;
					int num2;
					if ((num2 = hash.IndexOf("@")) >= 0)
					{
						num = global::XX.CAim.parseString(TX.slice(hash, num2 + 1), -1);
						hash = TX.slice(hash, 0, num2);
					}
					return new WholeMapItem.WMTransferPoint.WMRectItem(hash, num, SrcMp);
				}

				public string sf_key = "";

				public string another_key_for_worp = "_";

				public byte flags;

				private byte cache_aim = byte.MaxValue;
			}
		}

		public sealed class WMItem : BorderVectorListener.IPositionListener
		{
			public WMItem(WholeMapItem Con, M2MapLayer _Lay, Map2d _SrcMap, ref BorderDrawer<bool> Bd, bool force_recreate_border = false)
			{
				this.Lay = _Lay;
				this.SrcMap = _SrcMap;
				META meta = new META(this.Lay.comment);
				if (meta.GetB("secret", false))
				{
					this.type = WholeMapItem.WMItem.WMITYPE.SECRET;
				}
				this.show_icon_always = meta.GetNm("show_icon_always", (float)(Con.default_show_icon_always ? 1 : 0), 0) == 1f;
				this.Rc = this.Lay.Bounds;
				this.Rc.key = _Lay.name;
				this.WpCon = new LabelPointContainer<WholeMapItem.WMTransferPoint>();
				if (this.Rc.isEmpty())
				{
					return;
				}
				string[] array;
				if (force_recreate_border)
				{
					if (Bd == null)
					{
						Bd = WholeMapItem.createBD();
					}
					this.prepareDrawData(Bd, null, meta);
					if (WholeMapItem.RECALC_ITEM_BORDER >= 0)
					{
						WholeMapItem.RECALC_ITEM_BORDER++;
						return;
					}
				}
				else if ((array = meta.Get("pos_manual") ?? meta.Get("pos")) != null)
				{
					this.readDrawData(array, meta.Get("tri"));
				}
			}

			public void prepareDrawData(BorderDrawer<bool> Bd, bool[,] Ab = null, META Meta = null)
			{
				if (!this.Lay.Mp.point_prepared)
				{
					this.Lay.Mp.reconnectWholeChips();
				}
				if (Meta.Get("pos_manual") == null)
				{
					if (Ab == null)
					{
						using (BList<bool> blist = ListBuffer<bool>.Pop(0))
						{
							this.getBooleanArray(blist);
							int num = (int)this.Rc.w;
							Ab = new bool[num, (int)this.Rc.h];
							int count = blist.Count;
							for (int i = 0; i < count; i++)
							{
								Ab[i % num, i / num] = blist[i];
							}
						}
					}
					Bd.checkOutPath(Ab, false);
					Bd.makePathes(this, WholeMapItem.WMItem.fnCalcX, WholeMapItem.WMItem.fnCalcY, new BorderDrawer<bool>.FnCalcSlopePos(this.fnCalcSlopePos));
					if (this.AAmap_outline_pos.Length == 1 && this.AAmap_outline_pos[0][0].Equals(new Vector2(0f, (float)((int)this.Rc.h))) && this.AAmap_outline_pos[0][1].Equals(new Vector2(0f, 0f)) && this.AAmap_outline_pos[0][2].Equals(new Vector2((float)((int)this.Rc.w), 0f)) && this.AAmap_outline_pos[0][3].Equals(new Vector2((float)((int)this.Rc.w), (float)((int)this.Rc.h))))
					{
						this.AAmap_outline_pos = null;
						this.AAtri = null;
					}
					Meta = Meta ?? new META(this.Lay.comment);
					if (this.AAmap_outline_pos != null)
					{
						Meta.Add("pos", this.getPositionSaveString());
					}
					else
					{
						Meta.Del("pos");
					}
					if (this.AAtri != null)
					{
						Meta.Add("tri", this.getTriSaveString());
					}
					else
					{
						Meta.Del("tri");
					}
					this.Lay.comment = Meta.getValueString();
				}
			}

			public void getBooleanArray(List<bool> Ab)
			{
				int count_chips = this.Lay.count_chips;
				int num = (int)this.Rc.w;
				int num2 = num * (int)this.Rc.h;
				if (Ab.Capacity < num2)
				{
					Ab.Capacity = num2;
				}
				while (Ab.Count < num2)
				{
					Ab.Add(false);
				}
				for (int i = 0; i < count_chips; i++)
				{
					M2Chip m2Chip = this.Lay.getChipByIndex(i) as M2Chip;
					if (m2Chip != null && m2Chip.getMeta().GetI("whole_map_fill", 0, 0) != 0)
					{
						for (int j = 0; j < m2Chip.clms; j++)
						{
							for (int k = 0; k < m2Chip.rows; k++)
							{
								if (m2Chip.getConfig(j, k) != 0)
								{
									Ab[(k + m2Chip.mapy - (int)this.Rc.y) * num + (j + m2Chip.mapx - (int)this.Rc.x)] = true;
								}
							}
						}
					}
				}
			}

			private List<BDVector> fnCalcSlopePos(List<BDVector> ABuf, int bx, int by, BDCorner C, BDCorner NextC)
			{
				int z = C.z;
				int z2 = C.z;
				bool flag = (NextC.z & 2) > 0;
				bool flag2 = (NextC.z & 4) > 0;
				float num = (float)(global::XX.X.IntR(NextC.x) - (flag ? 1 : 0));
				int num2 = global::XX.X.IntR(NextC.y) - (flag2 ? 1 : 0);
				global::XX.AIM aim = (global::XX.AIM)NextC.aim;
				int num3 = bx + (int)this.Rc.x;
				int num4 = by + (int)this.Rc.y;
				int num5 = global::XX.CAim._XD(aim, 1);
				int num6 = -global::XX.CAim._YD(aim, 1);
				int num7 = (int)global::XX.X.LENGTHXYS(num, (float)num2, (float)bx, (float)by) + (((NextC.z & 8) == 0) ? 1 : 0);
				int i = 0;
				if ((C.z & 8) != 0)
				{
					i++;
					num3 += num5;
					num4 += num6;
				}
				while (i < num7)
				{
					M2Pt pointPuts = this.Lay.Mp.getPointPuts(num3, num4, false, false);
					M2Chip m2Chip = null;
					if (pointPuts != null)
					{
						for (int j = pointPuts.count - 1; j >= 0; j--)
						{
							M2Chip c = pointPuts.GetC(j);
							if (c != null && c.Lay == this.Lay)
							{
								int i2 = c.getMeta().GetI("whole_map_fill", 0, 0);
								if (c != null && i2 > 0)
								{
									if (i2 >= 2)
									{
										m2Chip = null;
										int num8 = num3 - (int)this.Rc.x;
										int num9 = num4 - (int)this.Rc.y;
										switch (aim)
										{
										case global::XX.AIM.L:
											ABuf.Add(new BDVector((float)(num8 + 1), (float)(num9 + 1)));
											ABuf.Add(new BDVector((float)num8, (float)(num9 + 1)));
											goto IL_024E;
										case global::XX.AIM.T:
											ABuf.Add(new BDVector((float)num8, (float)(num9 + 1)));
											ABuf.Add(new BDVector((float)num8, (float)num9));
											goto IL_024E;
										case global::XX.AIM.R:
											ABuf.Add(new BDVector((float)num8, (float)num9));
											ABuf.Add(new BDVector((float)(num8 + 1), (float)num9));
											goto IL_024E;
										case global::XX.AIM.B:
											ABuf.Add(new BDVector((float)(num8 + 1), (float)num9));
											ABuf.Add(new BDVector((float)(num8 + 1), (float)(num9 + 1)));
											goto IL_024E;
										default:
											goto IL_024E;
										}
									}
									else
									{
										m2Chip = c;
									}
								}
							}
						}
						IL_024E:
						if (m2Chip != null)
						{
							Vector2[] array = METACImg.pointsRotate(m2Chip, "whole_map_points_", (global::XX.AIM)C.aim, (float)m2Chip.clms * this.Lay.Mp.CLEN * 0.5f, (float)m2Chip.rows * this.Lay.Mp.CLEN * 0.5f);
							if (array == null)
							{
								global::XX.X.dl(string.Concat(new string[]
								{
									"whole_map_fill == 1 のチップは whole_map_points_<AIM> を設定すること (pos: ",
									num3.ToString(),
									",",
									num4.ToString(),
									")"
								}), null, false, false);
							}
							else
							{
								int num10 = array.Length;
								for (int k = 0; k < num10; k++)
								{
									Vector2 vector = array[k];
									ABuf.Add(new BDVector((float)m2Chip.mapx + vector.x * this.Lay.Mp.rCLEN - this.Rc.x, (float)(m2Chip.mapy + m2Chip.rows) - vector.y * this.Lay.Mp.rCLEN - this.Rc.y));
								}
							}
						}
					}
					num3 += num5;
					num4 += num6;
					i++;
				}
				return ABuf;
			}

			public void setPathCount(int i)
			{
				this.AAmap_outline_pos = new Vector2[i][];
				this.AAtri = new int[i][];
			}

			public void SetPath(int i, List<BDVector> Apos, FnZoom fnCalcX, FnZoom fnCalcY, int len)
			{
				VPts vpts = new VPts();
				vpts.resolution = (int)this.Lay.Mp.CLEN;
				vpts.Add(Apos, len, fnCalcX, fnCalcY).removeDupe(true);
				this.AAmap_outline_pos[i] = vpts.ToArrayV2();
				this.AAtri[i] = WholeMapItem.Tri.Triangulate(this.AAmap_outline_pos[i], 0, -1, null, 0).ToArray();
			}

			private void readDrawData(string[] Apos, string[] Atri)
			{
				int num = Apos.Length;
				this.AAmap_outline_pos = new Vector2[num][];
				if (Atri != null && Atri.Length != Apos.Length)
				{
					Atri = null;
				}
				else
				{
					this.AAtri = new int[num][];
				}
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.ToString();
					using (BList<int> blist = ListBuffer<int>.Pop(0))
					{
						for (int i = 0; i < num; i++)
						{
							stb.Set(Apos[i]);
							blist.Clear();
							stb.splitIndex("_", blist);
							int num2 = blist.Count;
							Vector2[] array = (this.AAmap_outline_pos[i] = new Vector2[num2]);
							for (int j = 0; j < num2; j++)
							{
								int num3 = blist[j];
								int num4 = ((j < num2 - 1) ? (blist[j + 1] - 1) : stb.Length);
								int num5 = stb.IndexOf(":", num3, num4);
								if (num5 >= 0)
								{
									int num7;
									float num6 = (float)STB.NmRes(stb.Nm(num3, out num7, num5, false), 0.0);
									float num8 = (float)STB.NmRes(stb.Nm(num5 + 1, out num7, num4, false), 0.0);
									array[j] = new Vector2(num6, num8);
								}
							}
							if (Atri != null)
							{
								blist.Clear();
								stb.Set(Atri[i]);
								stb.splitIndex("_", blist);
								num2 = blist.Count;
								int[] array2 = (this.AAtri[i] = new int[num2]);
								for (int k = 0; k < num2; k++)
								{
									int num9;
									stb.splitByIndex(k, blist, "_", out num9, -1);
									array2[k] = num9;
								}
							}
						}
					}
				}
			}

			public string getPositionSaveString()
			{
				if (this.AAmap_outline_pos == null)
				{
					return null;
				}
				string text2;
				using (STB stb = TX.PopBld(null, 0))
				{
					int num = this.AAmap_outline_pos.Length;
					for (int i = 0; i < num; i++)
					{
						Vector2[] array = this.AAmap_outline_pos[i];
						int num2 = array.Length;
						string text = "";
						for (int j = 0; j < num2; j++)
						{
							Vector2 vector = array[j];
							stb.Add((j > 0) ? "_" : "").Add("", vector.x, ":").Add(vector.y);
						}
						stb.Add((i > 0) ? " " : "").Add(text);
					}
					text2 = stb.ToString();
				}
				return text2;
			}

			public string getTriSaveString()
			{
				if (this.AAtri == null)
				{
					return null;
				}
				string text;
				using (STB stb = TX.PopBld(null, 0))
				{
					int num = this.AAtri.Length;
					for (int i = 0; i < num; i++)
					{
						stb.Add((i > 0) ? " " : "");
						int[] array = this.AAtri[i];
						int num2 = array.Length;
						for (int j = 0; j < num2; j++)
						{
							stb.Add((j > 0) ? "_" : "").Add(array[j]);
						}
					}
					text = stb.ToString();
				}
				return text;
			}

			public bool isin(int x, int y)
			{
				if (!this.Rc.isin((float)x, (float)y, 0f))
				{
					return false;
				}
				if (this.AAmap_outline_pos == null)
				{
					return true;
				}
				float num = (float)x + 0.5f - this.Rc.x + 0.0625f;
				float num2 = (float)y + 0.5f - this.Rc.y;
				for (int i = this.AAmap_outline_pos.Length - 1; i >= 0; i--)
				{
					Vector2[] array = this.AAmap_outline_pos[i];
					int num3 = array.Length;
					if (num3 != 1)
					{
						Vector2 vector = array[num3 - 1];
						int num4 = 0;
						for (int j = 0; j < num3; j++)
						{
							Vector2 vector2 = array[j];
							if (vector.y != vector2.y)
							{
								if (vector.x == vector2.x)
								{
									if (vector.x > num && global::XX.X.BTWRV(vector.y, num2, vector2.y))
									{
										num4++;
									}
								}
								else
								{
									Vector3 vector3 = global::XX.X.crosspoint(vector.x, vector.y, vector2.x, vector2.y, num, num2, num + 1f, num2);
									if (vector3.z != 0f && vector3.x > num && global::XX.X.BTWRV(vector.x, vector3.x, vector2.x) && global::XX.X.BTWRV(vector.y, vector3.y, vector2.y))
									{
										num4++;
									}
								}
							}
							vector = vector2;
						}
						if (num4 % 2 == 1)
						{
							return true;
						}
					}
				}
				return false;
			}

			public bool appeared
			{
				get
				{
					return this.visitted || this.type != WholeMapItem.WMItem.WMITYPE.SECRET;
				}
			}

			public bool isActive()
			{
				return !this.Rc.isEmpty();
			}

			public WholeMapItem.WMTransferPoint getPointByAimAndJumpKey(int a, string jump_key)
			{
				int length = this.WpCon.Length;
				for (int i = 0; i < length; i++)
				{
					WholeMapItem.WMTransferPoint wmtransferPoint = this.WpCon.Get(i);
					if (wmtransferPoint.getByAimAndJumpKey(a, jump_key) != null)
					{
						return wmtransferPoint;
					}
				}
				return null;
			}

			public override string ToString()
			{
				return "<WMItem>" + this.Lay.name;
			}

			public Vector2 getPositionOnWm(float mapx, float mapy, bool check_empty_cell = true, bool integerize = false)
			{
				int num = (int)this.Rc.w;
				int num2 = (int)this.Rc.h;
				Vector2 mapWmLevel = this.SrcMap.getMapWmLevel(mapx, mapy, (float)num, (float)num2, true, integerize);
				Vector2 vector = new Vector2(this.Rc.x + mapWmLevel.x, this.Rc.y + mapWmLevel.y);
				if (check_empty_cell && this.AAmap_outline_pos != null)
				{
					using (BList<bool> blist = ListBuffer<bool>.Pop(0))
					{
						this.getBooleanArray(blist);
						if (!this.Rc.isContainingXy(this.Rc.x + mapWmLevel.x, this.Rc.y + mapWmLevel.y, this.Rc.x + mapWmLevel.x, this.Rc.y + mapWmLevel.y, -0.5f) || !blist[(int)mapWmLevel.y * num + (int)mapWmLevel.x])
						{
							float num3 = -1f;
							for (int i = 0; i < num; i++)
							{
								for (int j = 0; j < num2; j++)
								{
									if (blist[j * num + i])
									{
										float num4 = (float)i + 0.5f;
										float num5 = (float)j + 0.5f;
										float num6 = global::XX.X.LENGTHXYS(num4, num5, mapWmLevel.x, mapWmLevel.y);
										if (num3 < 0f || num3 > num6)
										{
											num3 = num6;
											vector.Set(num4 + this.Rc.x, num5 + this.Rc.y);
										}
									}
								}
							}
						}
					}
				}
				return vector;
			}

			public readonly M2MapLayer Lay;

			public readonly Map2d SrcMap;

			public DRect Rc;

			public Vector2[][] AAmap_outline_pos;

			public int[][] AAtri;

			public bool visitted;

			public WholeMapItem.WMItem.WMITYPE type;

			public LabelPointContainer<WholeMapItem.WMTransferPoint> WpCon;

			public WholeMapItem.WMSpecialIcon SpecialPos;

			public bool show_icon_always;

			public static FnZoom fnCalcX = (float v) => v;

			public static FnZoom fnCalcY = WholeMapItem.WMItem.fnCalcX;

			public enum WMITYPE
			{
				NORMAL,
				SECRET
			}
		}

		public enum REVEALED : byte
		{
			NONE,
			MAP
		}

		public sealed class WMRegex
		{
			public WMRegex(string s)
			{
				this.Reg = new Regex(s);
			}

			public Regex Reg;

			public Dungeon Dgn;
		}

		public struct WMSpecialIcon
		{
			public readonly string key
			{
				get
				{
					return this.SrcLP.key;
				}
			}

			public WMSpecialIcon(M2LabelPoint LP, WholeMapItem.WMItem _Wmi)
			{
				this.Wmi = _Wmi;
				this.SrcLP = LP;
				this.arrow = -1;
				this.PF = null;
				this.go_other_wm = null;
				if (TX.valid(LP.comment))
				{
					META meta = new META(LP.comment);
					this.arrow = meta.getDirsI("arrow", 0, false, 0, -1);
					this.go_other_wm = meta.GetSi(0, "go_other_wm");
					if (this.arrow < 0)
					{
						string s = meta.GetS("icon");
						this.PF = (TX.valid(s) ? MTRX.getPF(s) : null);
					}
				}
			}

			public bool valid
			{
				get
				{
					return this.Wmi != null;
				}
			}

			public bool Equals(WholeMapItem.WMSpecialIcon Ico)
			{
				return this.SrcLP == Ico.SrcLP;
			}

			public readonly PxlFrame PF;

			public readonly WholeMapItem.WMItem Wmi;

			public readonly M2LabelPoint SrcLP;

			public readonly string go_other_wm;

			public readonly int arrow;
		}

		public sealed class WMTreasureBoxCache
		{
			public WMTreasureBoxCache(string map_key, string box_sf_key)
			{
				this.key = map_key + "..." + box_sf_key;
			}

			public bool mapIs(string map_key)
			{
				return TX.isStart(this.key, map_key + "...", 0);
			}

			public string getSfKey()
			{
				return TX.slice(this.key, this.key.IndexOf("...") + 3);
			}

			public override string ToString()
			{
				return "WMTreasureBoxCache:: " + this.key;
			}

			public string key;
		}
	}
}
