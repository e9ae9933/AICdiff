using System;
using System.Collections.Generic;
using Better;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class MatoateReader
	{
		public static void initReader()
		{
			if (MatoateReader.OReader != null)
			{
				return;
			}
			if (MatoateReader.OReader == null)
			{
				MatoateReader.OReader = new BDic<string, MatoateReader>(1);
			}
			CsvReaderA csvReaderA = new CsvReaderA(TX.getResource("Data/_puzz_matoate", ".csv", false), true);
			csvReaderA.tilde_replace = true;
			MatoateReader matoateReader = null;
			MatoateReader.MVTYPE mvtype = MatoateReader.MVTYPE.STOP;
			float num = 0f;
			int num2 = 0;
			Comparison<MatoateReader.MatoItem> comparison = new Comparison<MatoateReader.MatoItem>(MatoateReader.sortMatoItem);
			while (csvReaderA.read())
			{
				if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
				{
					if (matoateReader != null)
					{
						matoateReader.AMato.Sort(comparison);
					}
					string index = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
					if (!MatoateReader.OReader.TryGetValue(index, out matoateReader))
					{
						matoateReader = (MatoateReader.OReader[index] = new MatoateReader());
					}
					else
					{
						matoateReader.clear();
					}
					mvtype = MatoateReader.MVTYPE.STOP;
					num = 0f;
					num2 = 0;
				}
				else if (matoateReader != null)
				{
					string cmd = csvReaderA.cmd;
					if (cmd != null)
					{
						uint num3 = <PrivateImplementationDetails>.ComputeStringHash(cmd);
						if (num3 <= 3265483080U)
						{
							if (num3 <= 1727275117U)
							{
								if (num3 != 274698077U)
								{
									if (num3 == 1727275117U)
									{
										if (cmd == "%WHOLE_TIME")
										{
											matoateReader.whole_time = csvReaderA.Int(1, 0);
											continue;
										}
									}
								}
								else if (cmd == "%killable_not_pr")
								{
									matoateReader.killable_not_pr = csvReaderA.Nm(1, 1f) != 0f;
									continue;
								}
							}
							else if (num3 != 1750609613U)
							{
								if (num3 != 2076634374U)
								{
									if (num3 == 3265483080U)
									{
										if (cmd == "%TYPE")
										{
											if (!FEnum<MatoateReader.MVTYPE>.TryParse(csvReaderA._1U, out mvtype, true))
											{
												csvReaderA.tError("不明な MVTYPE: " + csvReaderA._1);
												continue;
											}
											continue;
										}
									}
								}
								else if (cmd == "%LEVEL")
								{
									num = csvReaderA.Nm(1, 0f);
									continue;
								}
							}
							else if (cmd == "%HOLD")
							{
								matoateReader.default_hold = csvReaderA.Int(1, 0);
								continue;
							}
						}
						else if (num3 <= 3984483097U)
						{
							if (num3 != 3716843161U)
							{
								if (num3 == 3984483097U)
								{
									if (cmd == "%WHOLE_NORMA")
									{
										matoateReader.whole_nolma = csvReaderA.Nm(1, 0f);
										continue;
									}
								}
							}
							else if (cmd == "%APPEAR")
							{
								matoateReader.default_appear = csvReaderA.Int(1, 0);
								continue;
							}
						}
						else if (num3 != 4074695482U)
						{
							if (num3 != 4075149483U)
							{
								if (num3 == 4133573802U)
								{
									if (cmd == "%ROT_DURATION")
									{
										matoateReader.default_rotate_duration = csvReaderA.Nm(1, 0f);
										continue;
									}
								}
							}
							else if (cmd == "%SPLITTER")
							{
								int num4 = ((matoateReader.AMato.Count > 0) ? (matoateReader.AMato[matoateReader.AMato.Count - 1].appear + 1) : (-1));
								matoateReader.AMato.Add(new MatoateReader.MatoItem("splitter_" + matoateReader.AMato.Count.ToString(), MatoateReader.MVTYPE.__SPLITTER, ++num2, 0f, 0f, num4, 0, 0f));
								num2++;
								continue;
							}
						}
						else if (cmd == "%POS")
						{
							matoateReader.OAppearPos[csvReaderA._1] = new Vector2(csvReaderA.Nm(2, 0f), csvReaderA.Nm(3, 0f));
							continue;
						}
					}
					if (TX.isStart(csvReaderA.cmd, "@", 0))
					{
						string text = TX.slice(csvReaderA.cmd, 1);
						float num5 = csvReaderA.Nm(1, -1000f);
						float num6 = csvReaderA.Nm(2, 0f);
						int num7 = csvReaderA.Int(3, -2);
						int num8 = csvReaderA.Int(4, -2);
						matoateReader.AMato.Add(new MatoateReader.MatoItem(text, mvtype, num2, (num5 == -1000f) ? num : num5, num6, (num7 <= -2) ? matoateReader.default_appear : num7, (num8 <= -2) ? matoateReader.default_hold : num8, matoateReader.default_rotate_duration));
					}
					else
					{
						csvReaderA.tError("MatoateReader::initReader - 不明なコマンド: " + csvReaderA.cmd);
					}
				}
			}
			if (matoateReader != null)
			{
				matoateReader.AMato.Sort(comparison);
			}
		}

		public static MatoateReader Get(string key)
		{
			MatoateReader.initReader();
			return X.Get<string, MatoateReader>(MatoateReader.OReader, key);
		}

		private static int sortMatoItem(MatoateReader.MatoItem Ma, MatoateReader.MatoItem Mb)
		{
			if (Ma.splitter_id != Mb.splitter_id)
			{
				return Ma.splitter_id - Mb.splitter_id;
			}
			return Ma.appear - Mb.appear;
		}

		public MatoateReader()
		{
			this.OAppearPos = new BDic<string, Vector2>(4);
			this.AMato = new List<MatoateReader.MatoItem>(2);
		}

		public MatoateReader clear()
		{
			this.OAppearPos.Clear();
			this.AMato.Clear();
			this.default_appear = 60;
			this.default_rotate_duration = 150f;
			this.default_hold = -1;
			this.whole_time = -1;
			this.whole_nolma = -1f;
			this.mato_hit_cnt_ = -1;
			this.killable_not_pr = false;
			return this;
		}

		public MatoateReader.MatoItem GetI(int i)
		{
			if (this.AMato.Count <= i || i < 0)
			{
				return null;
			}
			return this.AMato[i];
		}

		public int mato_hit_cnt
		{
			get
			{
				if (this.mato_hit_cnt_ < 0)
				{
					this.mato_hit_cnt_ = 0;
					for (int i = this.AMato.Count - 1; i >= 0; i--)
					{
						if (this.AMato[i].mvtype != MatoateReader.MVTYPE.__SPLITTER)
						{
							this.mato_hit_cnt_++;
						}
					}
				}
				return this.mato_hit_cnt_;
			}
		}

		public int norma_count
		{
			get
			{
				return (int)((this.whole_nolma < 0f) ? ((float)this.mato_hit_cnt * -(int)this.whole_nolma) : this.whole_nolma);
			}
		}

		private static BDic<string, MatoateReader> OReader;

		private const string data_path = "Data/_puzz_matoate";

		private const string data_ext = ".csv";

		private int default_appear = 60;

		private int default_hold = -1;

		private float default_rotate_duration = 150f;

		private BDic<string, Vector2> OAppearPos;

		private List<MatoateReader.MatoItem> AMato;

		public int whole_time = -1;

		public float whole_nolma = -1f;

		public int mato_hit_cnt_ = -1;

		public bool killable_not_pr;

		public enum MVTYPE
		{
			STOP,
			SIN_H,
			SIN_V,
			CIRCLE,
			CIRCLE_H,
			CIRCLE_V,
			__SPLITTER
		}

		public class MatoItem
		{
			public MatoItem(string _pos_key, MatoateReader.MVTYPE _mvtype, int _splitter_id, float _start_level, float _agR, int _appear, int _hold, float _rot_duration)
			{
				this.pos_key = _pos_key;
				this.mvtype = _mvtype;
				this.splitter_id = _splitter_id;
				this.appear = _appear;
				this.hold = _hold;
				this.start_level = _start_level;
				this.agR = _agR;
				this.rot_duration = _rot_duration;
			}

			public string pos_key;

			public MatoateReader.MVTYPE mvtype;

			public int splitter_id;

			public int appear;

			public int hold;

			public float start_level;

			public float agR;

			public float rot_duration;
		}

		public class MatoatePlayer<T> where T : M2MatoateTarget
		{
			public MatoatePlayer(Map2d _Mp, MatoateReader _Reader, ObjPool<T> _Pool, float _base_mapx, float _base_mapy, ref float t, ref int mato_appeared)
			{
				t = 0f;
				this.Reader = _Reader;
				mato_appeared = 0;
				this.Mp = _Mp;
				this.Pool = _Pool;
				this.base_mapx = _base_mapx;
				this.base_mapy = _base_mapy;
				this.next_timer_timing = 60f;
				this.Ed = this.Mp.setED("matoate_player", new M2DrawBinder.FnEffectBind(this.drawMatoateTarget), 0f);
			}

			public MatoateReader.MatoatePlayer<T> closeGame()
			{
				this.Pool.ReleaseAll();
				return null;
			}

			public bool run(float fcnt, ref float t, ref int mato_appeared)
			{
				bool flag = true;
				while (this.Reader.AMato.Count > mato_appeared)
				{
					MatoateReader.MatoItem matoItem = this.Reader.AMato[mato_appeared];
					if (matoItem.mvtype == MatoateReader.MVTYPE.__SPLITTER)
					{
						this.splitter_t = 0f;
						if (this.Pool.count_act != 0)
						{
							flag = false;
							break;
						}
						mato_appeared++;
					}
					else
					{
						if ((float)matoItem.appear > this.splitter_t)
						{
							break;
						}
						T t2 = this.Pool.Next();
						Vector2 zero;
						if (!this.Reader.OAppearPos.TryGetValue(matoItem.pos_key, out zero))
						{
							zero = Vector2.zero;
						}
						t2.appear(this.Mp, this.Reader, matoItem, new Vector2(this.base_mapx + zero.x, this.base_mapy + zero.y), mato_appeared);
						mato_appeared++;
						this.pre_appeared = true;
					}
				}
				for (int i = this.Pool.count_act - 1; i >= 0; i--)
				{
					T on = this.Pool.GetOn(i);
					int num = on.run();
					if (num > 0)
					{
						this.pre_appeared = true;
						this.Pool.Release(on);
						if (num >= 2)
						{
							this.defeated++;
						}
					}
				}
				if (flag)
				{
					this.splitter_t += fcnt;
				}
				t += fcnt;
				if (this.Reader.whole_time >= 0 && t >= this.next_timer_timing)
				{
					SND.Ui.play("tiktok" + (this.timer_sound_id % 2).ToString(), false);
					if (this.timer_sound_id < 2 && t >= (float)(this.Reader.whole_time - 240))
					{
						this.timer_sound_id = 3;
					}
					else
					{
						this.timer_sound_id = 1 - this.timer_sound_id % 2 + this.timer_sound_id / 2 * 2;
					}
					this.next_timer_timing += (float)((this.timer_sound_id >= 2) ? 15 : 30);
				}
				return (this.Reader.whole_time < 0 || t < (float)this.Reader.whole_time) && (this.Pool.count_act > 0 || this.Reader.AMato.Count > mato_appeared);
			}

			public void releaseAll()
			{
				for (int i = this.Pool.count_act - 1; i >= 0; i--)
				{
					this.Pool.GetOn(i).isActive();
				}
				this.Pool.ReleaseAll();
				this.Ed = this.Mp.remED(this.Ed);
			}

			public void setFocusPoint(M2LpCamFocus Foc)
			{
				if (this.pre_appeared)
				{
					this.pre_appeared = false;
					float num = 0f;
					float num2 = 0f;
					for (int i = this.Pool.count_act - 1; i >= 0; i--)
					{
						T on = this.Pool.GetOn(i);
						num += on.x;
						num2 += on.y;
					}
					float num3 = 1f / (float)this.Pool.count_act;
					num *= num3;
					num2 *= num3;
					Foc.x = num - 0.5f - (float)Foc.mapx;
					Foc.y = num2 - 0.5f - (float)Foc.mapy;
				}
			}

			public bool isSuccess()
			{
				return this.defeated >= this.Reader.norma_count;
			}

			public T GetTarget(int i)
			{
				if (i < 0 || i >= this.Pool.count_act)
				{
					return default(T);
				}
				return this.Pool.GetOn(i);
			}

			private bool drawMatoateTarget(EffectItem Ef, M2DrawBinder Ed)
			{
				MeshDrawer meshDrawer = null;
				MeshDrawer meshDrawer2 = null;
				for (int i = this.Pool.count_act - 1; i >= 0; i--)
				{
					this.Pool.GetOn(i).drawMatoateTarget(Ef, Ed, ref meshDrawer, ref meshDrawer2);
				}
				return true;
			}

			public readonly Map2d Mp;

			public readonly MatoateReader Reader;

			public readonly ObjPool<T> Pool;

			public readonly float base_mapx;

			public readonly float base_mapy;

			private int defeated;

			private bool pre_appeared = true;

			private int timer_sound_id;

			private float next_timer_timing;

			private float splitter_t;

			private M2DrawBinder Ed;
		}
	}
}
