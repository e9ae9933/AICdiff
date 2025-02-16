using System;
using System.Collections.Generic;
using PixelLiner.PixelLinerLib;
using XX;

namespace m2d
{
	public sealed class M2MapLayer : IIdvName
	{
		public M2MapLayer(Map2d Map, string _name = null, string _comment = null, uint rgba = 0U)
		{
			this.name = "Layer";
			this.index = Map.count_layers;
			this.LayerColor = new C32(4286545791U);
			this.Bounds.key = this.name;
			if (_name != null)
			{
				this.name = _name;
				this.comment_ = _comment;
				this.LayerColor.rgba = rgba;
			}
			this.Meta = new METAMapLayer(this.comment_);
			this.LP = new M2LabelPointContainer(this, Map.M2D.FnCreateLp);
			this.GRD = new M2GradationContainer(this);
			this.copyValuesFromMap(Map);
			this.Achips = new M2Puts[64];
		}

		public M2MapLayer(Map2d Map, CsvReader CR)
			: this(Map, TX.decodeURIComponent(CR._1), TX.decodeURIComponent(CR._2), global::XX.X.NmUI(CR._4, 4286545791U, true, true))
		{
		}

		public void initMeshDrawer(MeshDrawer _MdLPMap)
		{
		}

		public void fineCheckFlag()
		{
			this.unloaded_ = false;
			this.Meta.clearCache();
			if (!Map2d.editor_decline_lighting)
			{
				if (this.Meta.Get("load_if") != null)
				{
					this.unloaded_ = TX.eval(TX.join<string>(" ", this.Meta.Get("load_if"), 0, -1), "") == 0.0;
				}
				if (this.unloaded_)
				{
					this.Meta.is_chip_arrangeable = true;
				}
				if (this.summon_show > 0)
				{
					this.addActiveRemoveKeyToAll("SUMMON_SHOW", true, true);
				}
			}
		}

		public void reopen(bool remake_lp = true)
		{
			this.Bounds.key = this.name;
			this.remakeLabelPoint(remake_lp);
			this.fineCheckFlag();
		}

		public float CLEN
		{
			get
			{
				return this.Mp.CLEN;
			}
		}

		public M2DBase M2D
		{
			get
			{
				return this.Mp.M2D;
			}
		}

		public int clms
		{
			get
			{
				return this.Mp.clms;
			}
		}

		public int rows
		{
			get
			{
				return this.Mp.rows;
			}
		}

		public float center_x
		{
			get
			{
				if (!this.Bounds.isEmpty())
				{
					return this.Bounds.cx;
				}
				return (float)this.Mp.clms * 0.5f;
			}
		}

		public float center_y
		{
			get
			{
				if (!this.Bounds.isEmpty())
				{
					return this.Bounds.cy;
				}
				return (float)this.Mp.rows * 0.5f;
			}
		}

		public string comment
		{
			get
			{
				return this.comment_;
			}
			set
			{
				if (this.comment_ == value)
				{
					return;
				}
				this.comment_ = value;
				bool do_not_consider_config = this.do_not_consider_config;
				this.Meta = new METAMapLayer(this.comment_);
				if (do_not_consider_config != this.do_not_consider_config)
				{
					this.Mp.considerConfig4(0, 0, this.clms, this.rows);
					this.Mp.need_update_collider = true;
				}
			}
		}

		public void initDebugLayerMode()
		{
		}

		internal void copyValuesFromMap(Map2d Map = null)
		{
			if (Map != null)
			{
				this.Mp = Map;
			}
			if (this.Achips == null)
			{
				return;
			}
			int num = this.Achips.Length;
			for (int i = 0; i < num; i++)
			{
				M2Puts m2Puts = this.Achips[i];
				if (m2Puts == null)
				{
					break;
				}
				this.connectImgLink(m2Puts, Map2d.CONNECTIMG.ASSIGN, true);
			}
		}

		public void recalcBounds()
		{
			this.Bounds.Set(0f, 0f, 0f, 0f);
			int num = this.Achips.Length;
			for (int i = 0; i < num; i++)
			{
				M2Puts m2Puts = this.Achips[i];
				if (m2Puts != null)
				{
					int num2;
					int num3;
					int num4;
					int num5;
					m2Puts.getPutsBounds(out num2, out num3, out num4, out num5);
					this.Mp.extendBounds(this.Bounds, num2, num3, num4 - num2, num5 - num3);
				}
			}
		}

		public void checkReindex(bool force_sort = false)
		{
			if (!force_sort && !this.need_reindex)
			{
				return;
			}
			this.need_reindex = false;
			int i = 0;
			int num = 0;
			while (i < this.chip_count)
			{
				M2Puts m2Puts = ((num == 0) ? this.Achips[i] : (this.Achips[i - num] = this.Achips[i]));
				if (m2Puts == null)
				{
					num++;
				}
				else
				{
					m2Puts.index = i - num;
				}
				i++;
			}
			if (num > 0)
			{
				i = this.chip_count - num;
				while (i < this.chip_count)
				{
					this.Achips[i++] = null;
				}
				this.chip_count -= num;
			}
		}

		public static M2MapLayer readBytesContentLay(ByteArray BaLoad, Map2d Mp, out bool is_key_layer)
		{
			string text = BaLoad.readPascalString("utf-8", Mp == null);
			string text2 = BaLoad.readString("utf-8", Mp == null);
			is_key_layer = BaLoad.readByte() != 0;
			uint num = BaLoad.readUInt();
			if (Mp != null)
			{
				return new M2MapLayer(Mp, text, text2, num);
			}
			return null;
		}

		public bool load(ByteArray BaLoad, ref uint pattern, int shift_drawx = 0, int shift_drawy = 0)
		{
			int num = 0;
			return this.load(BaLoad, ref pattern, ref num, shift_drawx, shift_drawy);
		}

		internal bool load(ByteArray BaLoad, ref uint pattern, ref int unload_chip, int shift_drawx = 0, int shift_drawy = 0)
		{
			this.checkReindex(false);
			switch (BaLoad.readByte())
			{
			case 81:
				pattern = BaLoad.readUInt();
				break;
			case 82:
			{
				M2ChipImage m2ChipImage = this.Mp.IMGS.GetById(BaLoad.readUInt()) as M2ChipImage;
				M2Chip m2Chip = M2MapLayer.readBytesContentCp(BaLoad, this, m2ChipImage, shift_drawx, shift_drawy) as M2Chip;
				if (m2Chip == null)
				{
					unload_chip++;
					return true;
				}
				if (m2Chip != null)
				{
					m2Chip.pattern = pattern;
					this.assignNewMapChip(m2Chip, this.chip_count, true, false);
				}
				break;
			}
			case 83:
			{
				M2ChipImage m2ChipImage = this.Mp.IMGS.GetById(BaLoad.readUInt()) as M2ChipImage;
				M2Picture m2Picture = M2MapLayer.readBytesContentPic(BaLoad, this, m2ChipImage, shift_drawx, shift_drawy);
				if (m2Picture != null)
				{
					m2Picture.pattern = pattern;
					this.assignNewMapChip(m2Picture, this.chip_count, true, false);
				}
				else
				{
					unload_chip++;
				}
				break;
			}
			case 84:
				if (this.labelpoint_ba_position == 0U)
				{
					this.labelpoint_ba_position = (uint)BaLoad.position - 1U;
				}
				M2LabelPoint.readBytesContentLp(BaLoad, this, false, shift_drawx, shift_drawy);
				break;
			case 85:
				M2GradationRect.readBytesContentGrd(BaLoad, this, false, shift_drawx, shift_drawy);
				break;
			default:
				BaLoad.position -= 1UL;
				return false;
			}
			return true;
		}

		public static M2DrawItem readBytesContentCp(ByteArray BaLoad, M2MapLayer Lay, M2ChipImage I, int shift_drawx = 0, int shift_drawy = 0)
		{
			int num = (int)BaLoad.readShort() + shift_drawx;
			int num2 = (int)BaLoad.readShort() + shift_drawy;
			int num3 = BaLoad.readByte();
			int num4 = (int)BaLoad.readShort();
			bool flag = false;
			if (num3 > 100)
			{
				num3 -= 100;
				flag = true;
			}
			if (Lay != null && I != null)
			{
				return Lay.MakeChip(I, num, num2, num3, num4, flag);
			}
			return null;
		}

		public static M2Picture readBytesContentPic(ByteArray BaLoad, M2MapLayer Lay, M2ChipImage I, int shift_drawx = 0, int shift_drawy = 0)
		{
			int num = (int)BaLoad.readShort() + shift_drawx;
			int num2 = (int)BaLoad.readShort() + shift_drawy;
			int num3 = BaLoad.readByte();
			int num4 = (int)BaLoad.readShort();
			bool flag = false;
			if (num3 > 100)
			{
				num3 -= 100;
				flag = true;
			}
			if (Lay != null && I != null)
			{
				return Lay.MakePicture(I, num, num2, num3, num4, flag, Lay.chip_count);
			}
			return null;
		}

		public M2DrawItem MakeChip(M2ChipImage Img, int drawx, int drawy, int opacity, int rotation, bool flip)
		{
			if (Img == null)
			{
				return null;
			}
			return Img.CreateOneChip(this, drawx, drawy, opacity, rotation, flip);
		}

		public M2Picture MakePicture(M2ChipImage Img, int drawx, int drawy, int opacity, int rotation, bool flip, int len = -1)
		{
			if (Img == null)
			{
				return null;
			}
			return new M2Picture(this, drawx, drawy, opacity, rotation, flip, len, Img);
		}

		public M2MapLayer resortChips(bool check_all_pcon = false, bool auto_expand_sort_area = false)
		{
			if (M2MapLayer.Sorter == null)
			{
				M2MapLayer.Sorter = new SORT<M2Puts>(new Comparison<M2Puts>(METACImg.fnSort));
			}
			M2MapLayer.Sorter.qSort(this.Achips, null, this.count_chips);
			this.need_reindex = true;
			return this;
		}

		public bool assignNewMapChip(List<M2Puts> MC, bool no_consider_config = false, bool sorting = false)
		{
			if (MC == null)
			{
				return false;
			}
			int count = MC.Count;
			int count_chips = this.count_chips;
			bool flag = false;
			DRect drect = ((!no_consider_config) ? new DRect("_") : null);
			for (int i = 0; i < count; i++)
			{
				M2Puts m2Puts = MC[i];
				if (m2Puts == null)
				{
					break;
				}
				flag = this.assignNewMapChip(m2Puts, count_chips++, true, sorting) || flag;
				if (!no_consider_config && m2Puts is M2Chip)
				{
					M2Chip m2Chip = m2Puts as M2Chip;
					if (drect.isEmpty())
					{
						drect.Set((float)m2Chip.mapx, (float)m2Chip.mapy, (float)m2Chip.clms, (float)m2Chip.rows);
					}
					else
					{
						global::XX.X.rectExpand(drect, (float)m2Chip.mapx, (float)m2Chip.mapy, (float)m2Chip.clms, (float)m2Chip.rows);
					}
				}
			}
			if (!no_consider_config && !this.do_not_consider_config)
			{
				this.Mp.considerConfig4((int)drect.x, (int)drect.y, (int)drect.right, (int)drect.bottom);
			}
			return flag;
		}

		public bool assignNewMapChip(M2Puts MC, int index = -1, bool no_consider_config = false, bool sorting = false)
		{
			bool flag = true;
			bool flag2 = false;
			int num = -1;
			if (index < 0)
			{
				if (index == -2 && !Map2d.editor_decline_lighting)
				{
					flag2 = true;
				}
				if (!sorting)
				{
					index = this.count_chips;
				}
				else
				{
					sorting = false;
					index = 0;
					MC.index = this.count_chips;
					for (int i = this.count_chips - 1; i >= 0; i--)
					{
						if (METACImg.fnSort(MC, this.Achips[i]) >= 0)
						{
							index = i + 1;
							break;
						}
					}
					if (this.Achips.Length <= this.count_chips)
					{
						Array.Resize<M2Puts>(ref this.Achips, this.count_chips + 256);
						flag = true;
					}
					this.chip_count++;
					global::XX.X.unshiftEmpty<M2Puts>(this.Achips, null, index, 1, -1);
					num = index + 1;
				}
			}
			else
			{
				flag = false;
				if (index < this.Achips.Length && this.Achips[index] != null && this.Achips[index] != MC)
				{
					global::XX.X.dl(string.Concat(new string[]
					{
						"チップ ",
						MC.src,
						" (",
						MC.mapx.ToString(),
						", ",
						MC.mapy.ToString(),
						") のインプット先には既にチップが存在します..."
					}), null, false, false);
					while (index < this.Achips.Length && this.Achips[index] != null)
					{
						index++;
					}
				}
			}
			this.connectImgLink(MC, Map2d.CONNECTIMG.ASSIGN, no_consider_config);
			if (this.Achips.Length <= index)
			{
				Array.Resize<M2Puts>(ref this.Achips, index + 256);
				flag = true;
			}
			MC.index = index;
			this.Achips[index++] = MC;
			this.chip_count = global::XX.X.Mx(index, this.chip_count);
			if (sorting)
			{
				this.resortChips(false, false);
				num = -1;
			}
			if (flag2)
			{
				this.Mp.entryChipPlaying(MC);
			}
			if (num >= 0)
			{
				for (int j = num; j < this.chip_count; j++)
				{
					M2Puts m2Puts = this.Achips[j];
					if (m2Puts != null)
					{
						m2Puts.index = j;
					}
				}
			}
			this.checkReindex(false);
			return flag;
		}

		public void mapPutsReindex()
		{
			int num = this.Achips.Length;
			for (int i = 0; i < num; i++)
			{
				M2Puts m2Puts = this.Achips[i];
				if (m2Puts == null)
				{
					break;
				}
				m2Puts.index = i;
			}
		}

		public bool connectImgLink(M2Puts Mcp, Map2d.CONNECTIMG connect = Map2d.CONNECTIMG.ASSIGN, bool no_consider_config = false)
		{
			int num = this.Mp.connectImgLink(Mcp, connect, this.Bounds, no_consider_config || this.do_not_consider_config);
			if (connect == Map2d.CONNECTIMG.DELETE && Mcp.index >= 0 && this.Achips[Mcp.index] == Mcp)
			{
				this.Achips[Mcp.index] = null;
				this.need_reindex = true;
				Mcp.index = -1;
				Mcp.clearDrawer(false);
			}
			return num != 0;
		}

		public bool removeChip(List<M2Puts> C, bool no_consider_config = false)
		{
			if (C == null)
			{
				return false;
			}
			this.checkReindex(false);
			bool flag = false;
			int count = C.Count;
			DRect drect = ((!no_consider_config) ? new DRect("_") : null);
			for (int i = 0; i < count; i++)
			{
				M2Puts m2Puts = C[i];
				flag = this.removeChip(m2Puts, true, true) || flag;
				if (!no_consider_config && m2Puts is M2Chip)
				{
					M2Chip m2Chip = m2Puts as M2Chip;
					if (drect.isEmpty())
					{
						drect.Set((float)m2Chip.mapx, (float)m2Chip.mapy, (float)m2Chip.clms, (float)m2Chip.rows);
					}
					else
					{
						global::XX.X.rectExpand(drect, (float)m2Chip.mapx, (float)m2Chip.mapy, (float)m2Chip.clms, (float)m2Chip.rows);
					}
				}
			}
			if (flag)
			{
				this.checkReindex(false);
			}
			if (!no_consider_config)
			{
				this.Mp.considerConfig4((int)drect.x, (int)drect.y, (int)drect.width, (int)drect.height);
			}
			return flag;
		}

		public bool removeChip(M2Puts C, bool no_consider_config = false, bool no_sort = false)
		{
			if (C == null)
			{
				return false;
			}
			C.closeAction(true, false);
			if (!no_sort)
			{
				this.checkReindex(false);
			}
			return this.connectImgLink(C, Map2d.CONNECTIMG.DELETE, no_consider_config);
		}

		public void translateChip(M2Chip C, int dep_mapx, int dep_mapy, bool no_consider_config = false)
		{
			if (this.unloaded || C == null || (C.mapx == dep_mapx && C.mapy == dep_mapy))
			{
				return;
			}
			bool flag = this.connectImgLink(C, Map2d.CONNECTIMG.DELETE_TEMP, true);
			C.mapx = dep_mapx;
			C.mapy = dep_mapy;
			C.inputRots(true);
			if (flag)
			{
				this.connectImgLink(C, Map2d.CONNECTIMG.ASSIGN, no_consider_config);
			}
		}

		public List<T> getAreaCastDrawer<T>(int _mapx, int _mapy, int _mapw, int _maph, List<T> ARet) where T : M2CImgDrawer
		{
			this.Mp.getAllPointMetaPutsTo(_mapx, _mapy, _mapw, _maph, null, delegate(M2Puts Cp, List<M2Puts> A)
			{
				if (Cp.Lay != this)
				{
					return false;
				}
				T t = Cp.CastDrawer<T>();
				if (t != null && ARet.IndexOf(t) == -1)
				{
					ARet.Add(t);
				}
				return false;
			});
			return ARet;
		}

		public M2Chip findChip(int mapx, int mapy, string key = null)
		{
			return this.Mp.findChip(mapx, mapy, 1UL << this.index, key);
		}

		public List<M2Puts> copyPutsTo(List<M2Puts> APuts, int len = -1)
		{
			this.checkReindex(false);
			if (this.unloaded)
			{
				return APuts;
			}
			if (len < 0)
			{
				len = this.chip_count;
			}
			APuts.Capacity = global::XX.X.Mx(APuts.Capacity, APuts.Count + len);
			for (int i = 0; i < len; i++)
			{
				APuts.Add(this.Achips[i]);
			}
			return APuts;
		}

		public List<M2Puts> getAllMetaPutsTo(List<M2Puts> APuts, string key)
		{
			this.checkReindex(false);
			if (this.unloaded)
			{
				return APuts;
			}
			for (int i = 0; i < this.chip_count; i++)
			{
				M2Puts m2Puts = this.Achips[i];
				if (m2Puts != null)
				{
					METACImg meta = m2Puts.getMeta();
					if (key == null || meta.GetS(key) != null)
					{
						if (APuts == null)
						{
							APuts = new List<M2Puts>(8);
						}
						APuts.Add(m2Puts);
					}
				}
			}
			return APuts;
		}

		public void fineSaveFlag()
		{
			if (this.LP == null || this.unloaded)
			{
				return;
			}
			int length = this.LP.Length;
			for (int i = 0; i < length; i++)
			{
				M2LabelPoint m2LabelPoint = this.LP.Get(i);
				if (m2LabelPoint is ISfListener)
				{
					(m2LabelPoint as ISfListener).fineSF(true);
				}
			}
		}

		public int reentryAllChips(MdMap MdB, MdMap MdG, MdMap MdT, MdMap MdL, MdMap MdTT, ref M2Puts[] ARedrawing, ref int redrawing_cnt)
		{
			if (!this.visible || this.unloaded)
			{
				return 0;
			}
			int num = 0;
			int num2 = this.chip_count;
			int num3 = 134217727;
			int num4 = 0;
			this.Meta.useWindowRemover(this.Mp);
			if (this.is_background)
			{
				MdT = (MdG = (MdTT = MdB));
				num3 = -8199;
				num4 = 1;
			}
			else if (this.is_ground)
			{
				MdTT = (MdT = (MdB = MdG));
				num3 = -8198;
				num4 = 2;
			}
			else if (this.Meta.overtop == METAMapLayer.OVERTOP.ALLTT)
			{
				MdT = (MdG = (MdB = MdTT));
				num3 = -8;
				num4 = 8192;
			}
			else if (this.Meta.overtop == METAMapLayer.OVERTOP.T2TT)
			{
				MdT = MdTT;
				num3 = -5;
				num4 = 8192;
			}
			else if (this.Meta.overtop == METAMapLayer.OVERTOP.ALLT)
			{
				MdB = (MdG = (MdTT = MdT));
				num3 = -8196;
			}
			else if (!Map2d.editor_decline_lighting && this.Meta.on_light)
			{
				MdB = MdL;
				MdT = MdL;
				MdG = MdL;
				num3 = -8;
				num4 = 512;
			}
			MdB.Col = (MdG.Col = (MdT.Col = (MdL.Col = (MdTT.Col = this.LayerColor.C))));
			if (this.do_not_consider_config && (Map2d.editor_decline_lighting || this.Meta.is_decrared))
			{
				MdB.Col.a = (MdG.Col.a = (MdT.Col.a = (MdL.Col.a = (MdTT.Col.a = (Map2d.editor_decline_lighting ? (this.LayerColor.C.a / 2) : 0)))));
			}
			bool flag = this.is_chip_arrangeable || this.Meta.do_not_consider_draw_bounds;
			for (int i = 0; i < num2; i++)
			{
				M2Puts m2Puts = this.Achips[i];
				if (m2Puts != null && m2Puts.Img != null && m2Puts.Img.chip_id != 0U)
				{
					int num5 = m2Puts.entryChipMesh(MdB, MdG, MdT, MdL, MdTT, 0f, 0f, 1f, 0f);
					if ((num5 & 256) > 0)
					{
						if (ARedrawing == null)
						{
							ARedrawing = new M2Puts[64];
						}
						global::XX.X.pushToEmptyS<M2Puts>(ref ARedrawing, m2Puts, ref redrawing_cnt, 16);
					}
					if ((num5 & 1) != 0 && MdB.canBakeSimplifyOnMiddle(true))
					{
						MdB.reentryLayerAfter(flag, true);
					}
					if ((num5 & 2) != 0 && MdG.canBakeSimplifyOnMiddle(true))
					{
						MdG.reentryLayerAfter(flag, true);
					}
					if ((num5 & 4) != 0 && MdT.canBakeSimplifyOnMiddle(true))
					{
						MdT.reentryLayerAfter(flag, true);
					}
					if ((num5 & 512) != 0 && MdL.canBakeSimplifyOnMiddle(true))
					{
						MdL.reentryLayerAfter(flag, true);
					}
					if ((num5 & 8192) != 0 && MdTT.canBakeSimplifyOnMiddle(true))
					{
						MdTT.reentryLayerAfter(flag, true);
					}
					num |= num5;
				}
			}
			if (this.GRD.use_chip_layer_order)
			{
				num |= this.GRD.drawGradationChipLayer(MdB, MdG, MdT, false);
			}
			MdB.reentryLayerAfter(flag, false);
			MdG.reentryLayerAfter(flag, false);
			MdT.reentryLayerAfter(flag, false);
			MdL.reentryLayerAfter(flag, false);
			MdTT.reentryLayerAfter(flag, false);
			int num6 = num & num3;
			if (num6 != num)
			{
				num6 |= num4;
			}
			return num6;
		}

		public int reentryGradation(MdMap MdU, MdMap MdB, MdMap MdG, MdMap MdT)
		{
			if (!this.visible || this.unloaded)
			{
				return 0;
			}
			int num = 0;
			if (this.GRD.Length > 0)
			{
				num |= this.GRD.drawGradation(MdU, MdB, MdG, MdT, false);
				if (num != 0)
				{
					bool flag = this.is_chip_arrangeable || this.Meta.do_not_consider_draw_bounds;
					MdU.reentryLayerAfter(flag, false);
					MdB.reentryLayerAfter(flag, false);
					MdG.reentryLayerAfter(flag, false);
					MdT.reentryLayerAfter(flag, false);
				}
			}
			return num;
		}

		public void clearChipsDrawer(bool close_action)
		{
			this.checkReindex(false);
			for (int i = this.chip_count - 1; i >= 0; i--)
			{
				this.Achips[i].clearDrawer(close_action);
			}
		}

		public void unshiftSpecificMeshDrawer(MeshDrawer Md, int ver, int tri)
		{
			for (int i = this.chip_count - 1; i >= 0; i--)
			{
				this.Achips[i].unshiftSpecificMeshDrawer(Md, ver, tri);
			}
		}

		public void loadLayerFromEvent()
		{
			if (this.unloaded_)
			{
				this.unloaded_ = false;
				bool flag = true;
				this.initActionPre(flag);
				Map2d.reentryAllChipsForOneLayer(this.Mp, this);
				this.Mp.addUpdateMesh(this.reentryGradation(this.Mp.MyDrawerUGrd, this.Mp.MyDrawerBGrd, this.Mp.MyDrawerGGrd, this.Mp.MyDrawerTGrd), false);
				this.initActionLP(flag);
				this.initAction(flag);
			}
		}

		public M2LabelPointContainer remakeLabelPoint(bool force = true)
		{
			ByteArray byteArray = null;
			CsvReader csvReader = null;
			if (this.LP == null)
			{
				this.LP = new M2LabelPointContainer(this, this.Mp.M2D.FnCreateLp);
			}
			else if (!force)
			{
				return this.LP;
			}
			if (this.labelpoint_ba_position == 0U)
			{
				return this.LP;
			}
			this.LP.Clear();
			this.Mp.getMapBodyContentReader(ref csvReader, ref byteArray);
			if (byteArray != null)
			{
				ulong position = byteArray.position;
				byteArray.position = (ulong)this.labelpoint_ba_position;
				while (byteArray.bytesAvailable > 0UL && byteArray.readByte() == 84)
				{
					M2LabelPoint.readBytesContentLp(byteArray, this, false, 0, 0);
				}
				byteArray.position = position;
			}
			this.LP.reindex();
			return this.LP;
		}

		public M2LabelPoint getLabelPoint(string key)
		{
			if (this.unloaded)
			{
				return null;
			}
			if (this.remakeLabelPoint(false) == null)
			{
				return null;
			}
			int length = this.LP.Length;
			for (int i = 0; i < length; i++)
			{
				M2LabelPoint m2LabelPoint = this.LP.Get(i);
				if (m2LabelPoint.key == key)
				{
					return m2LabelPoint;
				}
			}
			return null;
		}

		public M2LabelPoint getLabelPoint(M2LabelPoint.fnCheckLP Func)
		{
			if (this.unloaded)
			{
				return null;
			}
			if (this.remakeLabelPoint(false) == null)
			{
				return null;
			}
			int length = this.LP.Length;
			for (int i = 0; i < length; i++)
			{
				M2LabelPoint m2LabelPoint = this.LP.Get(i);
				if (Func(m2LabelPoint))
				{
					return m2LabelPoint;
				}
			}
			return null;
		}

		public List<M2LabelPoint> getLabelPointAll(M2LabelPoint.fnCheckLP Func, List<M2LabelPoint> ALp = null)
		{
			if (this.unloaded)
			{
				return ALp;
			}
			if (this.remakeLabelPoint(false) == null)
			{
				return null;
			}
			int length = this.LP.Length;
			for (int i = 0; i < length; i++)
			{
				M2LabelPoint m2LabelPoint = this.LP.Get(i);
				if (Func(m2LabelPoint))
				{
					if (ALp == null)
					{
						ALp = new List<M2LabelPoint>();
					}
					ALp.Add(m2LabelPoint);
				}
			}
			return ALp;
		}

		public void EachLP(M2LabelPoint.fnEachLP Func)
		{
			if (this.unloaded)
			{
				return;
			}
			if (this.remakeLabelPoint(false) == null)
			{
				return;
			}
			int length = this.LP.Length;
			for (int i = 0; i < length; i++)
			{
				M2LabelPoint m2LabelPoint = this.LP.Get(i);
				Func(m2LabelPoint);
			}
		}

		public void open()
		{
		}

		public void initActionPre(bool normal_map)
		{
			if (this.unloaded)
			{
				return;
			}
			if (this.Meta.is_fake && !Map2d.editor_decline_lighting && !this.Meta.auto_declare && this.Mp.M2D.getSF(this.fakewall_sf_key) != 0)
			{
				this.Meta.is_decrared = true;
				this.addActiveRemoveKeyToAll("FAKEWALL", true, true);
			}
			if (normal_map)
			{
				this.LpFakeReveal = null;
				for (int i = this.LP.Length - 1; i >= 0; i--)
				{
					this.LP.Get(i).initActionPre();
				}
				if (this.Meta.is_fake && !this.Meta.is_decrared && this.LpFakeReveal == null)
				{
					M2LpFakeReveal m2LpFakeReveal = new M2LpFakeReveal("FakeDeclare_After_Created", -1, this, true);
					m2LpFakeReveal.Set((this.Bounds.x + 1f) * this.CLEN, (this.Bounds.y + 1f) * this.CLEN, (this.Bounds.w - 2f) * this.CLEN, (this.Bounds.h - 2f) * this.CLEN);
					m2LpFakeReveal.initActionPre();
				}
			}
		}

		public void initActionLP(bool normal_map)
		{
			if (this.unloaded)
			{
				return;
			}
			if (this.LpFakeReveal != null && this.LpFakeReveal.created_by_after)
			{
				this.LpFakeReveal.initAction(normal_map);
			}
			for (int i = this.LP.Length - 1; i >= 0; i--)
			{
				this.LP.Get(i).initAction(normal_map);
			}
		}

		public void initAction(bool normal_map)
		{
			if (this.unloaded)
			{
				return;
			}
			if (!this.Meta.is_decrared)
			{
				int num = this.Achips.Length;
				for (int i = 0; i < num; i++)
				{
					M2Puts m2Puts = this.Achips[i];
					if (m2Puts != null && !m2Puts.active_closed)
					{
						m2Puts.initAction(normal_map);
					}
				}
				return;
			}
			this.LP.deactivateAll();
		}

		public void closeAction(bool when_map_close)
		{
			int num = this.Achips.Length;
			for (int i = 0; i < num; i++)
			{
				M2Puts m2Puts = this.Achips[i];
				if (m2Puts != null)
				{
					m2Puts.closeAction(when_map_close, false);
				}
			}
			for (int j = this.LP.Length - 1; j >= 0; j--)
			{
				this.LP.Get(j).closeAction(when_map_close);
			}
			if (this.LpFakeReveal != null && this.LpFakeReveal.created_by_after)
			{
				this.LpFakeReveal.closeAction(when_map_close);
				if (when_map_close)
				{
					this.LpFakeReveal = null;
				}
			}
			if (this.need_reindex)
			{
				this.checkReindex(false);
			}
			this.is_chip_arrangeable_ = false;
		}

		public void closeActionWithoutRemoveDrawer(bool when_map_close)
		{
			int num = this.Achips.Length;
			for (int i = 0; i < num; i++)
			{
				M2Puts m2Puts = this.Achips[i];
				if (m2Puts != null)
				{
					m2Puts.closeAction(when_map_close, true);
				}
			}
		}

		public void close()
		{
			int num = this.chip_count;
			for (int i = 0; i < num; i++)
			{
				M2Puts m2Puts = this.Achips[i];
				if (m2Puts != null)
				{
					m2Puts.releaseArranger();
				}
			}
			if (this.LP != null && this.labelpoint_ba_position > 0U)
			{
				this.LP = null;
			}
		}

		public void fakeWallDissolveFinalize(bool shown)
		{
			if (shown)
			{
				this.initAction(true);
				this.addActiveRemoveKeyToAll("FAKEWALL", false, false);
				this.LP.activateAll();
				return;
			}
			this.closeActionWithoutRemoveDrawer(true);
			this.addActiveRemoveKeyToAll("FAKEWALL", false, true);
			if (!this.Meta.auto_declare)
			{
				M2DBase.playSnd("fanfare_fake_wall");
				this.Mp.M2D.setSF(this.fakewall_sf_key, 1);
			}
			this.LP.deactivateAll();
		}

		public void addActiveRemoveKeyToAll(string key, bool to_grd, bool adding = true)
		{
			int count_chips = this.count_chips;
			for (int i = 0; i < count_chips; i++)
			{
				M2Puts m2Puts = this.Achips[i];
				if (m2Puts != null)
				{
					if (adding)
					{
						m2Puts.addActiveRemoveKey(key, false);
					}
					else
					{
						m2Puts.remActiveRemoveKey(key, false);
					}
				}
			}
			if (to_grd)
			{
				this.GRD.setAlpha(0f);
			}
		}

		public void reassignChipAtlas()
		{
			int count_chips = this.count_chips;
			M2ImageAtlas atlas = this.M2D.IMGS.Atlas;
			for (int i = 0; i < count_chips; i++)
			{
				M2Puts m2Puts = this.Achips[i];
				atlas.prepareChipImageDirectory(m2Puts.Img, false);
				if (m2Puts.Img.SourceLayer != null)
				{
					atlas.initCspAtlas(m2Puts.Img.SourceLayer.pChar.title);
				}
			}
		}

		public int count_chips
		{
			get
			{
				this.checkReindex(false);
				return this.chip_count;
			}
		}

		public M2Puts[] getChipsVector()
		{
			this.checkReindex(false);
			return this.Achips;
		}

		public M2Puts getChipByIndex(int c)
		{
			if (!global::XX.X.BTW(0f, (float)c, (float)this.count_chips))
			{
				return null;
			}
			return this.Achips[c];
		}

		public bool isFrontLayer()
		{
			return true;
		}

		public bool isBehindLayer()
		{
			return true;
		}

		public bool isKeyLayer()
		{
			return this.Mp.KeyLayer == this;
		}

		public bool do_not_consider_config
		{
			get
			{
				return this.Mp.is_whole || this.Meta.do_not_consider_config || this.unloaded;
			}
		}

		public bool overtop
		{
			get
			{
				return this.Meta.overtop > METAMapLayer.OVERTOP.NONE;
			}
		}

		public bool all_overtop
		{
			get
			{
				return this.Meta.overtop == METAMapLayer.OVERTOP.ALLTT;
			}
		}

		public bool is_fake
		{
			get
			{
				return this.Meta.is_fake;
			}
		}

		public bool is_background
		{
			get
			{
				return this.Meta.GetB("background", false);
			}
		}

		public bool is_ground
		{
			get
			{
				return this.Meta.GetB("ground", false);
			}
		}

		public bool closed
		{
			get
			{
				return this.LP == null;
			}
		}

		public bool is_chip_arrangeable
		{
			get
			{
				return (this.Meta != null && this.Meta.is_chip_arrangeable) || this.is_chip_arrangeable_;
			}
			set
			{
				this.is_chip_arrangeable_ = value;
			}
		}

		public METAMapLayer getLayerMeta()
		{
			return this.Meta;
		}

		public bool unloaded
		{
			get
			{
				return this.unloaded_ && !Map2d.editor_decline_lighting;
			}
		}

		public int summon_hide
		{
			get
			{
				return this.Meta.GetI("summon_hide", 0, 0);
			}
		}

		public int summon_show
		{
			get
			{
				return this.Meta.GetI("summon_show", 0, 0);
			}
		}

		public bool no_chiplight
		{
			get
			{
				return this.Meta.no_chiplight;
			}
		}

		public override string ToString()
		{
			return ":" + this.name + "#" + this.Mp.key;
		}

		public string fakewall_sf_key
		{
			get
			{
				return "FW_" + this.Mp.key + "_" + this.name;
			}
		}

		public string get_individual_key()
		{
			return this.name;
		}

		public string name;

		public int index;

		public Map2d Mp;

		private METAMapLayer Meta;

		public DRect Bounds = new DRect("");

		private string comment_ = "";

		public C32 LayerColor;

		public M2LabelPointContainer LP;

		public M2GradationContainer GRD;

		private bool is_chip_arrangeable_;

		private M2Puts[] Achips;

		public bool visible = true;

		public bool resort_drawchips_flag;

		private bool need_reindex;

		private uint labelpoint_ba_position;

		private static SORT<M2Puts> Sorter;

		private bool unloaded_;

		public M2LpFakeReveal LpFakeReveal;

		private int chip_count;
	}
}
