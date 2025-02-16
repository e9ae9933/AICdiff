using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class WAManager
	{
		public static void initWAScript(PxlSequence Sq)
		{
			WAManager.ORec = new BDic<string, WAManager.WARecord>(4);
			int num = Sq.countFrames();
			for (int i = 0; i < num; i++)
			{
				PxlFrame frame = Sq.getFrame(i);
				WAManager.WARecord warecord = null;
				int num2 = frame.countLayers();
				for (int j = 0; j < num2; j++)
				{
					PxlLayer layer = frame.getLayer(j);
					if (!layer.isGroup())
					{
						if (TX.isStart(layer.name, "_whole_", 0))
						{
							if (warecord != null)
							{
								warecord.finalize();
							}
							string text = TX.slice(layer.name, 7);
							warecord = global::XX.X.Get<string, WAManager.WARecord>(WAManager.ORec, text);
							if (warecord == null)
							{
								warecord = (WAManager.ORec[text] = new WAManager.WARecord(text));
							}
						}
						if (warecord != null)
						{
							warecord.addSourceLayer(layer);
						}
					}
				}
				if (warecord != null)
				{
					warecord.finalize();
				}
			}
		}

		public static void linkWA(string whole_key, StringHolder Sth)
		{
			WAManager.WARecord warecord = global::XX.X.Get<string, WAManager.WARecord>(WAManager.ORec, whole_key);
			if (warecord != null)
			{
				warecord.linkWA(Sth);
			}
		}

		public void depertAssign(string whole_a, string map_a, string whole_b, string map_b)
		{
			WAManager.WARecord warecord = global::XX.X.Get<string, WAManager.WARecord>(WAManager.ORec, whole_a);
			WAManager.WARecord warecord2 = global::XX.X.Get<string, WAManager.WARecord>(WAManager.ORec, whole_b);
			if (warecord == null || warecord2 == null)
			{
				return;
			}
			warecord.depertAssign(map_a, map_b);
			warecord2.depertAssign(map_b, map_a);
		}

		public bool copyDepert(NelM2DBase M2D, WholeMapItem WmFrom, List<string> Adest, string connected_to_wm)
		{
			WAManager.WARecord warecord = global::XX.X.Get<string, WAManager.WARecord>(WAManager.ORec, WmFrom.text_key);
			if (warecord == null)
			{
				return false;
			}
			warecord.copyDepert(M2D, Adest, connected_to_wm);
			return true;
		}

		public void newGame()
		{
			foreach (KeyValuePair<string, WAManager.WARecord> keyValuePair in WAManager.ORec)
			{
				keyValuePair.Value.newGame();
			}
		}

		public void NewGameFirstAssign()
		{
			WAManager.WARecord warecord = WAManager.ORec["forest"];
			warecord.Touch("forest_secret_lake", true, false);
			warecord.Touch("_whole_forest", true, false);
			WAManager.ORec["house"].Touch("_whole_house", true, false);
			this.depertAssign("forest", "forest_ahletic_home_thorn", "house", "house_road");
			this.FirstAssign26();
		}

		public void FirstAssign26()
		{
			this.depertAssign("city", "city_scl_center", "school", "school_entrance");
			this.depertAssign("forest", "forest_entrance_grazia", "city", "city_entrance_left");
		}

		public void initializeWmaTouchedData(WholeMapManager WMA)
		{
			foreach (KeyValuePair<string, WAManager.WARecord> keyValuePair in WAManager.ORec)
			{
				keyValuePair.Value.initializeWmaTouchedData(WMA);
			}
		}

		public static WAManager.WARecord GetWa(string whole_key, bool no_error = false)
		{
			WAManager.WARecord warecord = global::XX.X.Get<string, WAManager.WARecord>(WAManager.ORec, whole_key);
			if (warecord == null && !no_error)
			{
				global::XX.X.de("不明なWARecord: " + whole_key, null);
			}
			return warecord;
		}

		public void Touch(string whole_key, StringHolder Hld, int index_from = 2)
		{
			WAManager.WARecord wa = WAManager.GetWa(whole_key, false);
			if (wa == null)
			{
				return;
			}
			int clength = Hld.clength;
			for (int i = index_from; i < clength; i++)
			{
				wa.Touch(Hld.getIndex(i), false, false);
			}
		}

		public void Touch(string whole_key, string _key, bool strong = false, bool special = false)
		{
			WAManager.WARecord wa = WAManager.GetWa(whole_key, false);
			if (wa == null)
			{
				return;
			}
			wa.Touch(_key, strong, special);
		}

		public bool isActivated(string whole_key)
		{
			WAManager.WARecord wa = WAManager.GetWa(whole_key, false);
			return wa != null && wa.isActivated();
		}

		public void readBinaryFrom(ByteArray Ba)
		{
			int num = Ba.readByte();
			int num2 = Ba.readByte();
			for (int i = 0; i < num2; i++)
			{
				string text = Ba.readPascalString("utf-8", false);
				WAManager.WARecord.readBinaryFrom(Ba, num, global::XX.X.Get<string, WAManager.WARecord>(WAManager.ORec, text));
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(2);
			Ba.writeByte(WAManager.ORec.Count);
			foreach (KeyValuePair<string, WAManager.WARecord> keyValuePair in WAManager.ORec)
			{
				Ba.writePascalString(keyValuePair.Key, "utf-8");
				keyValuePair.Value.writeBinaryTo(Ba);
			}
		}

		public void validate()
		{
			foreach (KeyValuePair<string, WAManager.WARecord> keyValuePair in WAManager.ORec)
			{
				keyValuePair.Value.validate();
			}
		}

		public static void touchFinalizeAll()
		{
			foreach (KeyValuePair<string, WAManager.WARecord> keyValuePair in WAManager.ORec)
			{
				keyValuePair.Value.touchFinalizeAll();
			}
		}

		public static bool initializePositionAtSkin(out float x, out float y, string wm_from, DRect RcBounds = null, List<DRect> Aappeared = null)
		{
			if (RcBounds != null)
			{
				RcBounds.Set(0f, 0f, 0f, 0f);
				Aappeared.Clear();
			}
			x = (y = 0f);
			bool flag = false;
			foreach (KeyValuePair<string, WAManager.WARecord> keyValuePair in WAManager.ORec)
			{
				WAManager.BufRc.Set(0f, 0f, 0f, 0f);
				if (keyValuePair.Value.expandDrawingBounds(WAManager.BufRc))
				{
					if (wm_from == keyValuePair.Key)
					{
						flag = true;
						x = WAManager.BufRc.cx;
						y = WAManager.BufRc.cy;
					}
					if (RcBounds != null)
					{
						Aappeared.Add(new DRect(keyValuePair.Key, WAManager.BufRc));
						RcBounds.ExpandRc(WAManager.BufRc, false);
					}
				}
			}
			if (!flag)
			{
				WAManager.WARecord warecord = global::XX.X.Get<string, WAManager.WARecord>(WAManager.ORec, wm_from);
				if (warecord == null)
				{
					return false;
				}
				x = warecord.cx;
				y = warecord.cy;
			}
			return true;
		}

		public static int drawWA(MeshDrawer Md, MeshDrawer MdF, MeshDrawer MdIco, float cx, float cy, float outw, float outh, float cell_size, float alpha_, float fade_level, List<TextRenderer> ATx, int text_start_i, ref WholeMapItem FocusWM, out bool redraw_wa)
		{
			redraw_wa = false;
			FocusWM = null;
			PxlFrame pf = MTRX.getPF("IconNoel0");
			bool flag = false;
			int num = 0;
			bool flag2 = false;
			int num2 = -1;
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			string text = null;
			foreach (KeyValuePair<string, WAManager.WARecord> keyValuePair in WAManager.ORec)
			{
				WAManager.WARecord value = keyValuePair.Value;
				WAManager.MaTemp.clear(Md).Set(true);
				WAManager.MaTempF.clear(MdF).Set(true);
				WAManager.BufRc.Set(0f, 0f, 0f, 0f);
				bool flag3;
				bool flag4;
				if (value.draw(Md, MdF, cx, cy, cell_size, alpha_, fade_level, WAManager.BufRc, out flag3, out flag4))
				{
					if (nelM2DBase.WM.CurWM.text_key == keyValuePair.Key)
					{
						if (flag4)
						{
							num = 2;
						}
						MdIco.Col = C32.MulA(uint.MaxValue, 0f);
						MdIco.RotaPF((WAManager.BufRc.cx - cx) * cell_size, (-(WAManager.BufRc.cy - cy) - WAManager.BufRc.height * 0.3f) * cell_size, 1f, 1f, 0f, pf, false, false, false, uint.MaxValue, false, 0);
						flag = true;
					}
					else if (flag4 && num < 1)
					{
						num = 1;
					}
					int num3 = -1;
					if (text_start_i < ATx.Count)
					{
						TextRenderer textRenderer = ATx[text_start_i];
						textRenderer.alpha = alpha_;
						IN.PosP2(textRenderer.transform, (WAManager.BufRc.cx - cx) * cell_size, (-(WAManager.BufRc.cy - cy) + WAManager.BufRc.height * 0.5f - 3f) * cell_size);
						num3 = text_start_i++;
					}
					if (flag3 && FocusWM == null)
					{
						text = value.key;
						flag2 = flag4;
						num2 = num3;
					}
				}
			}
			if (FocusWM == null && text != null)
			{
				FocusWM = nelM2DBase.WM.GetWholeDescriptionByName(text, true);
				if (FocusWM != null)
				{
					if (!flag2)
					{
						float num4 = (0.6f + global::XX.X.Abs(0.4f * global::XX.X.COSIT(50f))) * alpha_;
						WAManager.MaTemp.Set(false).setAlpha1(num4, false);
						WAManager.MaTempF.Set(false).setAlpha1(num4, false);
						if (num2 >= 0)
						{
							ATx[num2].alpha = num4;
						}
					}
					redraw_wa = true;
				}
			}
			if (!flag)
			{
				MdIco.Col = C32.MulA(uint.MaxValue, 0f);
				MdIco.RotaPF(0f, 0f, 1f, 1f, 0f, pf, false, false, false, uint.MaxValue, false, 0);
			}
			MdIco.Col = C32.d2c(4283780170U);
			float num5 = 90f * cell_size;
			MdIco.initForImg(MTRX.IconWhite, 0).RectBL(-0.5f, num5, 1f, outh * 0.5f - num5, false).RectBL(num5, -0.5f, outw * 0.5f - num5, 1f, false)
				.RectBL(-0.5f, -outh * 0.5f, 1f, outh * 0.5f - num5, false)
				.RectBL(-outw * 0.5f, -0.5f, outw * 0.5f - num5, 1f, false);
			return num;
		}

		public static void fineMaIconAlpha(MeshDrawer Md, MdArranger Ma, float alpha_)
		{
			int startVerIndex = Ma.getStartVerIndex();
			if (Md.getVertexMax() < startVerIndex + 20)
			{
				return;
			}
			float num = (0.2f + global::XX.X.Abs(0.8f * global::XX.X.COSIT(140f))) * alpha_;
			WAManager.MaTemp.clear(Md).SetWhole(true).SetLastVerAndTriIndex(startVerIndex + 4, 6);
			Ma.setAlpha1(num, false);
			WAManager.MaTemp.SwapLastIndex().SetLastVerAndTriIndex(startVerIndex + 20, 30);
			Ma.setAlpha1(num * 0.4f, false);
		}

		private static BDic<string, WAManager.WARecord> ORec;

		private static readonly DRect BufRc = new DRect("_");

		private static readonly MdArranger MaTemp = new MdArranger(null);

		private static readonly MdArranger MaTempF = new MdArranger(null);

		private class WARLayer
		{
			public void newGame()
			{
				this.touched = 0;
			}

			public WARLayer(PxlLayer _Source)
			{
				this.Source = _Source;
			}

			public bool already_touched
			{
				get
				{
					return (this.touched & 3) > 0;
				}
			}

			public bool touch_finalized
			{
				get
				{
					return (this.touched & 3) >= 2;
				}
				set
				{
					if (value && !this.touch_finalized)
					{
						this.touched |= 3;
					}
				}
			}

			public bool temp_drawn
			{
				get
				{
					return (this.touched & 4) > 0;
				}
				set
				{
					if (value)
					{
						this.touched |= 4;
						return;
					}
					this.touched = (byte)((int)this.touched & -5);
				}
			}

			public string name
			{
				get
				{
					return this.Source.name;
				}
			}

			public PxlImage Img
			{
				get
				{
					return this.Source.Img;
				}
			}

			public bool Touch(bool strong = false)
			{
				if (strong)
				{
					if ((this.touched & 3) < 2)
					{
						this.touched |= 2;
						return true;
					}
				}
				else if ((this.touched & 3) == 0)
				{
					this.touched |= 1;
					return true;
				}
				return false;
			}

			public float left
			{
				get
				{
					return this.Source.x - this.width * 0.5f;
				}
			}

			public float width
			{
				get
				{
					return (float)this.Source.Img.width;
				}
			}

			public float top
			{
				get
				{
					return this.Source.y - this.height * 0.5f;
				}
			}

			public float height
			{
				get
				{
					return (float)this.Source.Img.height;
				}
			}

			public float x
			{
				get
				{
					return this.Source.rot_center_x;
				}
			}

			public float y
			{
				get
				{
					return this.Source.rot_center_y;
				}
			}

			public void flushDrawFlag()
			{
				this.touched = (byte)((int)this.touched & -5);
			}

			public void touchFinalize()
			{
				if ((this.touched & 3) == 1)
				{
					this.touched |= 3;
				}
			}

			public readonly PxlLayer Source;

			private byte touched;
		}

		public class WARecord : DRect
		{
			public void newGame()
			{
				this.OADepert.Clear();
				this.Aspecial_record.Clear();
				foreach (KeyValuePair<string, WAManager.WARLayer> keyValuePair in this.OImgLay)
				{
					keyValuePair.Value.newGame();
				}
			}

			public WARecord(string _key)
				: base(_key, 0f, 0f, 0f, 0f, 0f)
			{
				this.OImgLay = new BDic<string, WAManager.WARLayer>(1);
				this.OADepert = new BDic<string, List<WAManager.WARecord.DepertTarget>>();
				this.Aspecial_record = new List<string>();
			}

			public void addSourceLayer(PxlLayer Lay)
			{
				WAManager.WARLayer warlayer = (this.OImgLay[Lay.name] = new WAManager.WARLayer(Lay));
				if (this.FirstLayer == null)
				{
					this.FirstLayer = warlayer;
				}
			}

			public void linkWA(StringHolder Sth)
			{
				WAManager.WARLayer warlayer = global::XX.X.Get<string, WAManager.WARLayer>(this.OImgLay, Sth._1);
				if (warlayer == null)
				{
					Sth.tError("WARecord " + this.key + ".linkWA :: 不明なソースレイヤー " + Sth._1);
					return;
				}
				for (int i = 2; i < Sth.clength; i++)
				{
					this.OImgLay[Sth.getIndex(i)] = warlayer;
				}
			}

			public void finalize()
			{
				foreach (KeyValuePair<string, WAManager.WARLayer> keyValuePair in this.OImgLay)
				{
					base.Expand(keyValuePair.Value.left, keyValuePair.Value.top, keyValuePair.Value.width, keyValuePair.Value.height, false);
				}
			}

			public void initializeWmaTouchedData(WholeMapManager WMA)
			{
				WholeMapItem wholeDescriptionByName = WMA.GetWholeDescriptionByName(this.key, true);
				if (wholeDescriptionByName == null)
				{
					return;
				}
				bool flag = true;
				foreach (KeyValuePair<string, WAManager.WARLayer> keyValuePair in this.OImgLay)
				{
					if (!keyValuePair.Value.already_touched && wholeDescriptionByName.isVisitted(keyValuePair.Value.name))
					{
						keyValuePair.Value.Touch(true);
						if (flag)
						{
							flag = false;
							this.Touch("_whole_" + this.key, true, false);
						}
					}
				}
			}

			public void Touch(string l_key, bool strong = false, bool special = false)
			{
				WAManager.WARLayer warlayer = global::XX.X.Get<string, WAManager.WARLayer>(this.OImgLay, l_key);
				if (warlayer != null && (warlayer.Touch(strong) && special) && special && this.Aspecial_record.IndexOf(l_key) == -1)
				{
					this.Aspecial_record.Add(l_key);
				}
			}

			public void depertAssign(string from_map, string dep_map)
			{
				List<WAManager.WARecord.DepertTarget> list;
				if (!this.OADepert.TryGetValue(from_map, out list))
				{
					list = (this.OADepert[from_map] = new List<WAManager.WARecord.DepertTarget>(1));
				}
				for (int i = list.Count - 1; i >= 0; i--)
				{
					if (list[i].map_key == dep_map)
					{
						return;
					}
				}
				list.Add(new WAManager.WARecord.DepertTarget(dep_map));
			}

			public bool getFrontDepert(NelM2DBase M2D, WholeMapItem From0, WholeMapItem From1, out WAManager.WARecord.DepertTarget Target, out string from_map)
			{
				for (int i = 0; i < 3; i++)
				{
					if ((i != 0 || From0 != null) && (i != 1 || From1 != null))
					{
						foreach (KeyValuePair<string, List<WAManager.WARecord.DepertTarget>> keyValuePair in this.OADepert)
						{
							int count = keyValuePair.Value.Count;
							for (int j = 0; j < count; j++)
							{
								WAManager.WARecord.DepertTarget depertTarget = this.FineWmKey(keyValuePair.Value, j, M2D);
								bool flag;
								if (i == 0)
								{
									flag = From0 != null && depertTarget.wm_key == From0.text_key;
								}
								else
								{
									flag = i != 1 || (From1 != null && depertTarget.wm_key == From1.text_key);
								}
								if (flag)
								{
									Target = depertTarget;
									from_map = keyValuePair.Key;
									return true;
								}
							}
						}
					}
				}
				from_map = null;
				Target = default(WAManager.WARecord.DepertTarget);
				return false;
			}

			public void copyDepert(NelM2DBase M2D, List<string> Adest, string connected_to_wm)
			{
				foreach (KeyValuePair<string, List<WAManager.WARecord.DepertTarget>> keyValuePair in this.OADepert)
				{
					List<WAManager.WARecord.DepertTarget> value = keyValuePair.Value;
					for (int i = value.Count - 1; i >= 0; i--)
					{
						if (this.FineWmKey(value, i, M2D).wm_key == connected_to_wm)
						{
							Adest.Add(keyValuePair.Key);
							break;
						}
					}
				}
			}

			public WAManager.WARecord.DepertTarget FineWmKey(List<WAManager.WARecord.DepertTarget> A, int i, NelM2DBase M2D)
			{
				WAManager.WARecord.DepertTarget depertTarget = A[i];
				if (depertTarget.wm_key == null)
				{
					WholeMapItem wholeFor = M2D.WM.GetWholeFor(M2D.Get(depertTarget.map_key, false), false);
					depertTarget.wm_key = wholeFor.text_key;
					A[i] = depertTarget;
				}
				return depertTarget;
			}

			public static void readBinaryFrom(ByteArray Ba, int vers, WAManager.WARecord Target)
			{
				int num = Ba.readByte();
				for (int i = 0; i < num; i++)
				{
					int num2 = Ba.readByte();
					if (num2 != 0)
					{
						string text = Ba.readPascalString("utf-8", false);
						List<WAManager.WARecord.DepertTarget> list = null;
						if (Target != null && !Target.OADepert.TryGetValue(text, out list))
						{
							list = (Target.OADepert[text] = new List<WAManager.WARecord.DepertTarget>(num2));
						}
						for (int j = 0; j < num2; j++)
						{
							string text2 = Ba.readPascalString("utf-8", false);
							if (list != null)
							{
								if (vers == 0)
								{
									for (int k = list.Count - 1; k >= 0; k--)
									{
										if (list[k].map_key == text2)
										{
											text2 = null;
											break;
										}
									}
								}
								if (TX.valid(text2))
								{
									list.Add(new WAManager.WARecord.DepertTarget(text2));
								}
							}
						}
					}
				}
				num = Ba.readByte();
				for (int l = 0; l < num; l++)
				{
					string text3 = Ba.readPascalString("utf-8", false);
					if (Target != null)
					{
						Target.Touch(text3, true, true);
					}
				}
				if (vers >= 2)
				{
					num = Ba.readByte();
					if (num != 0 && Target != null)
					{
						Target.FirstLayer.Touch(true);
					}
				}
			}

			public void validate()
			{
				NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
				foreach (KeyValuePair<string, List<WAManager.WARecord.DepertTarget>> keyValuePair in this.OADepert)
				{
					List<WAManager.WARecord.DepertTarget> value = keyValuePair.Value;
					for (int i = value.Count - 1; i >= 0; i--)
					{
						if (nelM2DBase.Get(value[i].map_key, true) == null)
						{
							value.RemoveAt(i);
						}
					}
				}
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writeByte(this.OADepert.Count);
				foreach (KeyValuePair<string, List<WAManager.WARecord.DepertTarget>> keyValuePair in this.OADepert)
				{
					List<WAManager.WARecord.DepertTarget> value = keyValuePair.Value;
					int count = value.Count;
					Ba.writeByte(count);
					if (count != 0)
					{
						Ba.writePascalString(keyValuePair.Key, "utf-8");
						for (int i = 0; i < count; i++)
						{
							Ba.writePascalString(value[i].map_key, "utf-8");
						}
					}
				}
				int count2 = this.Aspecial_record.Count;
				Ba.writeByte(count2);
				for (int j = 0; j < count2; j++)
				{
					Ba.writePascalString(this.Aspecial_record[j], "utf-8");
				}
				Ba.writeByte(this.isActivated() ? 1 : 0);
			}

			public bool expandDrawingBounds(DRect RcBounds)
			{
				bool flag = false;
				if (this.rect_already_known)
				{
					RcBounds.ExpandRc(this, false);
				}
				foreach (KeyValuePair<string, WAManager.WARLayer> keyValuePair in this.OImgLay)
				{
					if (keyValuePair.Value.already_touched)
					{
						flag = true;
						if (this.rect_already_known)
						{
							break;
						}
						RcBounds.Expand(keyValuePair.Value.left, keyValuePair.Value.top, keyValuePair.Value.width, keyValuePair.Value.height, false);
					}
				}
				return flag;
			}

			public Vector2 getDrawCenter()
			{
				DRect bufRc = WAManager.BufRc;
				bufRc.Set(0f, 0f, 0f, 0f);
				this.expandDrawingBounds(bufRc);
				return new Vector2(bufRc.cx, bufRc.cy);
			}

			public void touchFinalizeAll()
			{
				foreach (KeyValuePair<string, WAManager.WARLayer> keyValuePair in this.OImgLay)
				{
					keyValuePair.Value.touchFinalize();
				}
			}

			public bool draw(MeshDrawer Md, MeshDrawer MdF, float cx, float cy, float cell_size, float alpha_, float fade_level, DRect DrawBounds, out bool focused, out bool fade_use)
			{
				focused = false;
				fade_use = false;
				bool flag = false;
				foreach (KeyValuePair<string, WAManager.WARLayer> keyValuePair in this.OImgLay)
				{
					WAManager.WARLayer value = keyValuePair.Value;
					if (value.already_touched)
					{
						flag = true;
					}
					value.flushDrawFlag();
				}
				if (!flag)
				{
					return false;
				}
				if (this.rect_already_known)
				{
					DrawBounds.ExpandRc(this, false);
				}
				foreach (KeyValuePair<string, WAManager.WARLayer> keyValuePair2 in this.OImgLay)
				{
					WAManager.WARLayer value2 = keyValuePair2.Value;
					if (value2.already_touched && !value2.temp_drawn)
					{
						value2.temp_drawn = true;
						if (!this.rect_already_known)
						{
							DrawBounds.Expand(value2.left, value2.top, value2.width, value2.height, false);
						}
						if (!focused && global::XX.X.isContaining(value2.left, value2.left + value2.width, cx, cx, 20f) && global::XX.X.isContaining(value2.top, value2.top + value2.height, cy, cy, 20f))
						{
							focused = true;
						}
						if (!value2.touch_finalized && fade_level >= 1f)
						{
							value2.touch_finalized = true;
						}
						MeshDrawer meshDrawer;
						if (!value2.touch_finalized)
						{
							meshDrawer = MdF;
							meshDrawer.Col = C32.MulA(4283780170U, alpha_);
							fade_use = true;
							meshDrawer.allocUv23(4, false);
						}
						else
						{
							meshDrawer = Md;
							meshDrawer.Col = C32.MulA(4283780170U, alpha_);
						}
						meshDrawer.initForImg(value2.Img, 0).RotaGraph((value2.x - cx) * cell_size, -(value2.y - cy) * cell_size, cell_size, 0f, null, false);
						if (!value2.touch_finalized)
						{
							meshDrawer.Uv2(0f, 1f, false).Uv2(0f, 0f, false).Uv2(1f, 0f, false)
								.Uv2(1f, 1f, false);
							meshDrawer.Uv3(fade_level, 0f, false).allocUv3(0, true);
						}
					}
				}
				return true;
			}

			public bool isActivated()
			{
				return this.FirstLayer.already_touched;
			}

			private readonly BDic<string, WAManager.WARLayer> OImgLay;

			private WAManager.WARLayer FirstLayer;

			private BDic<string, List<WAManager.WARecord.DepertTarget>> OADepert;

			private List<string> Aspecial_record;

			public bool rect_already_known;

			public struct DepertTarget
			{
				public DepertTarget(string _map_key)
				{
					this.map_key = _map_key;
					this.wm_key = null;
				}

				public string map_key;

				public string wm_key;
			}
		}
	}
}
