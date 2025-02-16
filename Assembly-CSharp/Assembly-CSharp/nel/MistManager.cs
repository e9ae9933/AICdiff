using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class MistManager : IIrisOutListener, IPuzzRevertable
	{
		public bool isActive()
		{
			return this.gen_id_max > 0;
		}

		public bool isInitialized()
		{
			return this.AAPd != null;
		}

		public MistManager(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.Rvfirst_progress.Max(30f, false);
			this.FlgHideSurface = new Flagger(null, null);
		}

		public bool isAvailablePos(int x, int y)
		{
			if (this.AAPd == null)
			{
				this.fineWH();
			}
			return X.BTW((float)this.crop, (float)x, (float)(this.Mp.clms - this.crop)) && X.BTW((float)this.crop, (float)y, (float)(this.Mp.rows - this.crop));
		}

		public void fineWH()
		{
			int num = X.Mx(0, this.Mp.crop - 1);
			int num2 = this.Mp.clms - num * 2;
			int num3 = this.Mp.rows - num * 2;
			bool flag = false;
			if (this.AAPd == null || this.crop != num || this.AAPd.GetLength(0) != num3 || this.AAPd.GetLength(1) != num2)
			{
				this.AAPd = new MistManager.MistPD[num3, num2];
				this.crop = num;
				flag = true;
			}
			if (this.AGen == null)
			{
				this.AGen = new List<MistManager.MistGenerator>(4);
				this.ADmgAtk = new List<MistManager.MistDamageApply>(4);
				this.dmg_atk_max = 0;
				this.OWMng = new BDic<MistManager.MISTTYPE, M2WaterWatcher>();
			}
			if (flag && this.OWMng.Count != 0)
			{
				foreach (KeyValuePair<MistManager.MISTTYPE, M2WaterWatcher> keyValuePair in this.OWMng)
				{
					keyValuePair.Value.allocSize();
				}
				this.releaseWaterMesh();
			}
		}

		public void releaseWaterMesh()
		{
			if (this.water_mesh_v >= 0)
			{
				this.MdWater.revertVerAndTriIndex(this.water_mesh_v, this.water_mesh_t, false);
				this.MdWater = null;
				this.water_mesh_v = -1;
				this.water_mesh_t = -1;
			}
		}

		public void closeAction()
		{
			this.AMemory = null;
			if (this.AGen == null)
			{
				return;
			}
			this.whole_fill_config = 0;
			this.static_id_max = -1;
			this.water_static_bits = 0UL;
			this.mistmanager_whole = 0U;
			foreach (KeyValuePair<MistManager.MISTTYPE, M2WaterWatcher> keyValuePair in this.OWMng)
			{
				keyValuePair.Value.has_static = false;
				keyValuePair.Value.clear();
			}
			this.draw_intv = 1;
			this.AWaterSlicer = null;
			this.releaseWaterMesh();
			this.water_mesh_v = -2;
			this.Rvfirst_progress.Clear();
			this.clear(false);
		}

		public unsafe void clear(bool clear_all_pixel = false)
		{
			if (this.AGen == null)
			{
				return;
			}
			this.FlgHideSurface.Clear();
			int count = this.AGen.Count;
			for (int i = this.static_id_max + 1; i < count; i++)
			{
				MistManager.MistKind k = this.AGen[i].K;
				if (k.Msd != null)
				{
					k.Msd.releaseEffect();
					k.Msd = null;
				}
			}
			this.gen_id_max = (byte)(this.static_id_max + 1);
			this.AGen.RemoveRange((int)this.gen_id_max, this.AGen.Count - (int)this.gen_id_max);
			if (this.OWMng.Count != 0)
			{
				BDic<MistManager.MISTTYPE, M2WaterWatcher> bdic = null;
				foreach (KeyValuePair<MistManager.MISTTYPE, M2WaterWatcher> keyValuePair in this.OWMng)
				{
					if (keyValuePair.Value.has_static)
					{
						if (bdic == null)
						{
							bdic = new BDic<MistManager.MISTTYPE, M2WaterWatcher>();
						}
						bdic[keyValuePair.Key] = keyValuePair.Value;
					}
					else
					{
						keyValuePair.Value.clear();
					}
				}
				this.OWMng.Clear();
				if (bdic != null)
				{
					this.OWMng = bdic;
				}
			}
			if (clear_all_pixel)
			{
				if (this.static_id_max == -1)
				{
					int num = this.Mp.clms - this.crop * 2;
					int num2 = this.Mp.rows - this.crop * 2;
					this.AAPd = new MistManager.MistPD[num2, num];
				}
				else
				{
					int num3 = (int)this.Rc.x;
					int num4 = (int)this.Rc.y;
					int num5 = (int)this.Rc.right;
					int num6 = (int)this.Rc.bottom;
					fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
					{
						MistManager.MistPD* ptr2 = ptr;
						for (int j = num4; j < num6; j++)
						{
							MistManager.MistPD* ptr3 = this.getPtr(ptr2, num3, num4);
							for (int l = num3; l < num5; l++)
							{
								if ((int)ptr3->id > this.static_id_max)
								{
									ptr3->level255 = 0;
								}
								ptr3++;
							}
						}
					}
				}
			}
			this.Rc.Set(0f, 0f, 0f, 0f);
			this.RcBuf.Set(0f, 0f, 0f, 0f);
			this.water_mng_bits = this.water_static_bits;
			this.use_config_bits &= (1UL << (int)this.gen_id_max) - 1UL;
		}

		public unsafe void clearWithoutGenerator()
		{
			if (this.AGen == null)
			{
				return;
			}
			foreach (KeyValuePair<MistManager.MISTTYPE, M2WaterWatcher> keyValuePair in this.OWMng)
			{
				keyValuePair.Value.clear();
			}
			this.Mp.considerConfig4((int)this.Rc.x - 1, (int)this.Rc.y - 1, (int)this.Rc.right + 1, (int)this.Rc.bottom + 1);
			this.RcBuf.Set(0f, 0f, 0f, 0f);
			int num = this.Mp.clms - this.crop;
			int num2 = this.Mp.rows - this.crop;
			int num3 = this.crop;
			int num4 = this.crop;
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				MistManager.MistPD* ptr3 = null;
				for (int i = num4; i < num2; i++)
				{
					MistManager.MistPD* ptr4 = this.getPtr(ptr2, num3, i);
					int j = num3;
					while (j < num)
					{
						MistManager.MistPD mistPD = *ptr4;
						if (mistPD.level255 <= 0)
						{
							this.clearAt(ptr4, null, j, i);
							goto IL_0218;
						}
						byte id = mistPD.id;
						if ((this.water_static_bits & (1UL << (int)id)) != 0UL)
						{
							ptr4++;
							this.RcBuf.Expand((float)j, (float)i, 1f, 1f, false);
						}
						else
						{
							if (mistPD.step != 0)
							{
								this.clearAt(ptr4, this.AGen[(int)id], j, i);
								goto IL_0218;
							}
							ptr4->aim_check = MistManager.AimCk.BEHIND_HAS_ROOM;
							M2WaterWatcher m2WaterWatcher = X.Get<MistManager.MISTTYPE, M2WaterWatcher>(this.OWMng, this.AGen[(int)id].K.type);
							if (m2WaterWatcher != null)
							{
								m2WaterWatcher.assignOrGenerateChip(j, i, this.AGen[(int)id].BaseChip);
							}
							ptr4++;
							this.RcBuf.Expand((float)j, (float)i, 1f, 1f, false);
						}
						IL_0223:
						j++;
						continue;
						IL_0218:
						ptr4++;
						goto IL_0223;
					}
				}
			}
			this.Rc.Set(this.RcBuf);
		}

		public int addMistGenerator(MistManager.MistKind K, int amount, int mapx, int mapy, bool no_expand_rc = false)
		{
			if (this.AAPd == null)
			{
				this.fineWH();
			}
			mapx = X.MMX(this.crop, mapx, this.Mp.clms - this.crop);
			mapy = X.MMX(this.crop, mapy, this.Mp.rows - this.crop);
			byte b = 0;
			MistManager.MistGenerator mistGenerator = null;
			if (mistGenerator == null)
			{
				for (int i = 0; i < (int)this.gen_id_max; i++)
				{
					if ((this.appear_gen_bits & (1UL << i)) == 0UL)
					{
						mistGenerator = this.AGen[i];
						b = (byte)i;
						mistGenerator.amount = (mistGenerator.useable_count = 0);
						break;
					}
				}
			}
			if (mistGenerator == null)
			{
				if (this.gen_id_max >= 64)
				{
					X.dl("霧オブジェクトが多い", null, false, true);
					return -1;
				}
				b = this.gen_id_max;
				if ((int)this.gen_id_max < this.AGen.Count)
				{
					mistGenerator = this.AGen[(int)this.gen_id_max];
					mistGenerator.amount = (mistGenerator.useable_count = 0);
				}
				else
				{
					this.AGen.Add(mistGenerator = new MistManager.MistGenerator());
				}
				this.gen_id_max += 1;
			}
			this.use_config_bits &= ~(1UL << (int)b);
			this.assignMistGenerator(mistGenerator, K, b, amount, mapx, mapy, no_expand_rc);
			return (int)b;
		}

		public MistManager.MistGenerator addMistGeneratorChip(MistManager.MistKind K, int amount, int mapx, int mapy, M2Chip Cp, int static_clip_top = -1)
		{
			mapx = X.MMX(this.crop, mapx, this.Mp.clms - this.crop - 1);
			mapy = X.MMX(this.crop, mapy, this.Mp.rows - this.crop - 1);
			MistManager.MistGenerator mistGenerator = null;
			int num = 0;
			for (int i = 0; i < (int)this.gen_id_max; i++)
			{
				MistManager.MistGenerator mistGenerator2 = this.AGen[i];
				if (mistGenerator2.K == K)
				{
					num = i;
					mistGenerator = this.assignMistGenerator(mistGenerator2, K, (byte)i, amount, mapx, mapy, static_clip_top >= 0);
					break;
				}
			}
			if (mistGenerator == null)
			{
				num = this.addMistGenerator(K, amount, mapx, mapy, static_clip_top >= 0);
				if (num < 0)
				{
					return null;
				}
				mistGenerator = this.AGen[num];
			}
			if (K.isWater())
			{
				if (Cp != null && mistGenerator.BaseChip == null)
				{
					mistGenerator.BaseChip = Cp;
					if (Cp.Img.Aconfig[0] > 4)
					{
						this.use_config_bits |= 1UL << num;
					}
				}
				M2WaterWatcher m2WaterWatcher = this.OWMng[K.type];
				m2WaterWatcher.assignOrGenerateChip(mapx, mapy, Cp);
				m2WaterWatcher.fine_flag = true;
				if (static_clip_top >= 0)
				{
					m2WaterWatcher.has_static = true;
				}
			}
			if (static_clip_top >= 0)
			{
				this.setStaticWater(K, mapx, mapy, mapy - static_clip_top);
				this.water_static_bits |= 1UL << (num & 31);
				this.static_id_max = X.Mx(num, this.static_id_max);
			}
			return mistGenerator;
		}

		private MistManager.MistGenerator assignMistGenerator(MistManager.MistGenerator Gen, MistManager.MistKind K, byte _id, int amount, int mapx, int mapy, bool no_expand_rc = false)
		{
			bool flag = this.appear_gen_bits == 0UL;
			this.appear_gen_bits |= 1UL << (int)_id;
			Gen.K = K;
			if (K.infinity_amount || amount < 0)
			{
				Gen.infinity = true;
			}
			int num = ((amount < 0) ? 255 : X.Mn(amount, 255));
			Gen.amount = ((!Gen.infinity) ? (Gen.amount + amount - num) : X.Mn(65535, Gen.amount + ((amount < 0) ? 255 : amount)));
			Gen.useable_count += K.calcApplyCell(0.6f) - 1;
			if (mapx >= 0)
			{
				if (!no_expand_rc)
				{
					this.Rc.Expand((float)(mapx - 1), (float)(mapy - 1), 3f, 3f, false);
				}
				this.AAPd[mapy - this.crop, mapx - this.crop] = new MistManager.MistPD(_id, num);
			}
			if (Gen.K.isWater())
			{
				M2WaterWatcher m2WaterWatcher = X.Get<MistManager.MISTTYPE, M2WaterWatcher>(this.OWMng, K.type);
				if (m2WaterWatcher == null)
				{
					this.OWMng[K.type] = new M2WaterWatcher(this, K, _id);
				}
				else
				{
					m2WaterWatcher.MistKindMerge(K, _id);
				}
				this.water_mng_bits |= 1UL << (int)_id;
				if (this.Rvfirst_progress > 0f)
				{
					this.Rvfirst_progress.Max(30f, false);
				}
				if (mapx >= 0 && K.water_bottom_raise && K.mist_aim == MistManager.MISTAIM.B && mapy == this.Mp.rows - this.crop - 1)
				{
					this.AAPd[mapy - this.crop, mapx - this.crop].aimTouch(0, 0);
				}
			}
			else
			{
				if (Gen.K.Msd == null)
				{
					Gen.K.Msd = new MistDrawer(Gen.K);
				}
				if (mapx >= 0)
				{
					Gen.K.Msd.addPoint(this.Mp, mapx, mapy, num, 32639);
				}
				Gen.BaseChip = null;
			}
			if (flag && this.gen_id_max == 1)
			{
				if (this.dmg_atk_max == 0)
				{
					this.running_delay = 0f;
				}
				this.reconsider_flag = 0;
				this.RcReconsider.Set(0f, 0f, 0f, 0f);
			}
			return Gen;
		}

		public void assignWholeFillMist(MistManager.MistKind K, int config)
		{
			if (this.AAPd == null)
			{
				this.fineWH();
			}
			if (this.use_config_bits != 0UL)
			{
				X.de("重複して assignWholeFillMist が呼ばれている", null);
			}
			byte b = this.gen_id_max;
			this.gen_id_max = b + 1;
			byte b2 = b;
			MistManager.MistGenerator mistGenerator;
			if ((int)b2 < this.AGen.Count)
			{
				mistGenerator = this.AGen[(int)b2];
			}
			else
			{
				this.AGen.Add(mistGenerator = new MistManager.MistGenerator());
			}
			this.static_id_max = X.Mx(this.static_id_max, (int)b2);
			mistGenerator.infinity = true;
			this.assignMistGenerator(mistGenerator, K, b2, -1, -1, -1, true);
			this.mistmanager_whole |= 1U << (int)b2;
			this.OWMng[K.type].has_static = true;
			this.whole_fill_config = (byte)config;
			this.reconsider_flag = 1;
			this.RcReconsider.Set(0f, 0f, (float)this.Mp.clms, (float)this.Mp.rows);
		}

		public void addStaticWaterArea(string name, MistManager.MistKind K, int config, int mapx, int mapy, int mapw, int maph)
		{
			if (this.AWaterStatic == null)
			{
				this.AWaterStatic = new List<MistManager.MistStaticArea>(1);
			}
			this.AWaterStatic.Add(new MistManager.MistStaticArea(name, K, mapx, mapy, mapw, maph, config));
		}

		public int getStaticWaterAreaTop(MistManager.MISTTYPE t)
		{
			if (this.AWaterStatic == null)
			{
				return -1;
			}
			int num = -1;
			for (int i = this.AWaterStatic.Count - 1; i >= 0; i--)
			{
				MistManager.MistStaticArea mistStaticArea = this.AWaterStatic[i];
				if (mistStaticArea.K.type == t)
				{
					num = ((num < 0) ? mistStaticArea.reachable_y : X.Mn(num, mistStaticArea.reachable_y));
				}
			}
			return num;
		}

		public void assignWaterSlicer(M2LpWaterSlicer Slc)
		{
			if (this.AWaterSlicer == null)
			{
				this.AWaterSlicer = new List<M2LpWaterSlicer>(1);
			}
			this.AWaterSlicer.Add(Slc);
			this.fine_water_slicer = true;
		}

		public void deassignWaterSlicer(M2LpWaterSlicer Slc)
		{
			if (this.AWaterSlicer == null)
			{
				return;
			}
			this.AWaterSlicer.Remove(Slc);
			this.fine_water_slicer = true;
		}

		private unsafe void fineWaterSlicer()
		{
			if (this.AWaterSlicer == null || this.AWaterStatic == null)
			{
				return;
			}
			this.fine_water_slicer = false;
			int count = this.AWaterSlicer.Count;
			for (int i = this.AWaterStatic.Count - 1; i >= 0; i--)
			{
				MistManager.MistStaticArea mistStaticArea = this.AWaterStatic[i];
				int num = -1;
				for (int j = (int)(this.gen_id_max - 1); j >= 0; j--)
				{
					if (this.AGen[j].K == mistStaticArea.K)
					{
						num = j;
						break;
					}
				}
				if (num != -1)
				{
					M2WaterWatcher m2WaterWatcher = null;
					if (this.OWMng.TryGetValue(mistStaticArea.K.type, out m2WaterWatcher))
					{
						m2WaterWatcher.fine_flag = true;
						int reachable_y = mistStaticArea.reachable_y;
						mistStaticArea.reachable_y = mistStaticArea.y;
						for (int k = 0; k < count; k++)
						{
							M2LpWaterSlicer m2LpWaterSlicer = this.AWaterSlicer[k];
							if (m2LpWaterSlicer.activated)
							{
								mistStaticArea.reachable_y = X.Mx(mistStaticArea.reachable_y, m2LpWaterSlicer.mapy);
							}
						}
						if (mistStaticArea.reachable_y != reachable_y)
						{
							mistStaticArea.K.max_behind_influence -= mistStaticArea.reachable_y - reachable_y;
							int num2 = X.MMX(this.crop, X.Mn(mistStaticArea.reachable_y, reachable_y), this.Mp.rows - this.crop);
							int num3 = X.MMX(this.crop, X.Mx(mistStaticArea.reachable_y, reachable_y), this.Mp.rows - this.crop);
							this.reconsider_flag = 1;
							this.RcReconsider.Expand((float)mistStaticArea.x, (float)num2, (float)mistStaticArea.w, (float)(num3 - num2), false);
							bool flag = mistStaticArea.reachable_y < reachable_y;
							this.Rc.Expand((float)(mistStaticArea.x - 1), (float)(num2 - 1), (float)(mistStaticArea.w + 2), (float)(num3 - num2 + 2), false);
							this.appear_gen_bits |= 1UL << num;
							int reachable_y2 = mistStaticArea.reachable_y;
							fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
							{
								MistManager.MistPD* ptr2 = ptr;
								int right = mistStaticArea.right;
								int num4 = X.Mn(num3 + 1, this.Mp.rows - this.crop);
								for (int l = num2; l < num4; l++)
								{
									MistManager.MistPD* ptr3 = this.getPtr(ptr2, mistStaticArea.x, l);
									for (int m = mistStaticArea.x; m < right; m++)
									{
										bool flag2 = false;
										if (flag)
										{
											flag2 = m2WaterWatcher.getLineAt(m, l, false) != null;
										}
										if (ptr3->level255 > 0)
										{
											flag2 = (int)ptr3->id == num;
										}
										if (flag2)
										{
											if (flag && ptr3->level255 == 0)
											{
												*ptr3 = new MistManager.MistPD((byte)num, 255);
											}
											ptr3->level255 = X.Mn(ptr3->level255, 255);
											if (l == mistStaticArea.reachable_y)
											{
												ptr3->fineRoomFlagStatic();
												if (flag)
												{
													ptr3->setStaticWater(mistStaticArea.K, l - mistStaticArea.reachable_y, flag);
												}
												else
												{
													ptr3->setStepBehind(mistStaticArea.K.max_behind_influence, mistStaticArea.K, true);
												}
											}
											else
											{
												ptr3->setStaticWater(mistStaticArea.K, l - mistStaticArea.reachable_y, flag);
												if (flag && mistStaticArea.K.Msd != null)
												{
													mistStaticArea.K.Msd.addPoint(this.Mp, m, l, ptr3->level255, 32639);
												}
											}
										}
										ptr3++;
									}
								}
							}
							M2WaterWatcher m2WaterWatcher2;
							if (this.OWMng.TryGetValue(mistStaticArea.K.type, out m2WaterWatcher2))
							{
								m2WaterWatcher2.fine_flag = true;
							}
						}
					}
				}
			}
		}

		public bool isWaterStaticId(int id)
		{
			return (this.water_static_bits & (1UL << id)) > 0UL;
		}

		public bool isWaterStaticMerged(ulong merged_id)
		{
			return (this.water_static_bits & merged_id) > 0UL;
		}

		public void fineDrawLevelWaterWatcher()
		{
			if (this.OWMng == null)
			{
				return;
			}
			foreach (KeyValuePair<MistManager.MISTTYPE, M2WaterWatcher> keyValuePair in this.OWMng)
			{
				keyValuePair.Value.fineDrawLevel();
			}
		}

		public MistManager.MistPD[,] getPointDataArray()
		{
			return this.AAPd;
		}

		public unsafe MistManager.MistPD* getPtr(MistManager.MistPD* Ptr0, int _x, int _y)
		{
			return Ptr0 + (_y - this.crop) * this.AAPd.GetLength(1) + (_x - this.crop);
		}

		public void attachWindDirectional(int cenx, int ceny, int w, int h, AIM a, int time, int shift_x = 0, int shift_y = 0)
		{
			if (a == AIM.L)
			{
				cenx -= w - 1;
			}
			if (a == AIM.T)
			{
				ceny -= h - 1;
			}
			this.attachWind(cenx + CAim._XD(a, 1) * shift_x, ceny + CAim._YD(a, 1) * shift_y, w, h, a, time);
		}

		public unsafe void attachWind(int sx, int sy, int w, int h, AIM a, int time)
		{
			if (this.AAPd == null || (this.appear_gen_bits & ~((this.water_mng_bits | (ulong)this.mistmanager_whole) != 0UL)) == 0UL || !this.Rc.isCoveringXy((float)sx, (float)sy, (float)(sx + w), (float)(sy + h), -1f, -1000f))
			{
				return;
			}
			int num = X.Mn(sx + w, (int)this.Rc.right - 1);
			int num2 = X.Mn(sy + h, (int)this.Rc.bottom - 1);
			sx = X.Mx(sx, (int)this.Rc.x + 1);
			sy = X.Mx(sy, (int)this.Rc.y + 1);
			time /= 5;
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				for (int i = sy; i < num2; i++)
				{
					MistManager.MistPD* ptr3 = this.getPtr(ptr2, sx, i);
					MistManager.MistPD* ptr4;
					int num3;
					switch (a)
					{
					case AIM.L:
						ptr4 = ptr3 - 1;
						num3 = -1;
						break;
					case AIM.T:
						ptr4 = ptr3 - this.AAPd.GetLength(1);
						num3 = -1;
						break;
					case AIM.R:
						ptr4 = ptr3 + 1;
						num3 = 1;
						break;
					case AIM.B:
						ptr4 = ptr3 + this.AAPd.GetLength(1);
						num3 = 1;
						break;
					default:
						return;
					}
					for (int j = sx; j < num; j++)
					{
						if (ptr3->level255 > 0)
						{
							MistManager.MistGenerator mistGenerator = this.AGen[(int)ptr3->id];
							if (!mistGenerator.K.isWater())
							{
								float num4 = 1f;
								MistManager.MistPD* ptr5 = ptr3;
								MistManager.MistGenerator mistGenerator2 = mistGenerator;
								int level = ptr4->level255;
								if (level > 0)
								{
									MistManager.MistGenerator mistGenerator3 = this.AGen[(int)ptr4->id];
									if (mistGenerator3.K.isWater())
									{
										num4 = 0f;
									}
									else if (level >= mistGenerator3.K.influence_threshold255 || ((a == AIM.R || a == AIM.L) ? ptr3->wind_x : ptr3->wind_y) > 255 - time)
									{
										ptr5 = ptr4;
										mistGenerator2 = mistGenerator3;
										num4 = 0.6f;
									}
								}
								if (num4 != 0f)
								{
									bool flag = false;
									switch (a)
									{
									case AIM.L:
									case AIM.R:
									{
										int num5;
										flag = (num5 = ptr5->wind_x) == 0;
										ptr5->wind_x = num5 + (int)(num4 * (float)time) * num3;
										break;
									}
									case AIM.T:
									case AIM.B:
									{
										int num5;
										flag = (num5 = ptr5->wind_y) == 0;
										ptr5->wind_y = num5 + (int)(num4 * (float)time) * num3;
										break;
									}
									}
									if (flag && mistGenerator2.K.Msd != null)
									{
										mistGenerator2.K.Msd.fineWind(this.Mp, j, i, ptr5->wind);
									}
								}
							}
						}
						ptr3++;
						ptr4++;
					}
				}
			}
		}

		public unsafe void removeMistArea(int sx, int sy, int w, int h)
		{
			if (this.gen_id_max == 0)
			{
				return;
			}
			int num = X.Mn(sx + w, (int)this.Rc.right - 1);
			int num2 = X.Mn(sy + h, (int)this.Rc.bottom - 1);
			sx = X.Mx(sx, (int)this.Rc.x + 1);
			sy = X.Mx(sy, (int)this.Rc.y + 1);
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				for (int i = sy; i < num2; i++)
				{
					MistManager.MistPD* ptr3 = this.getPtr(ptr2, sx, i);
					for (int j = sx; j < num; j++)
					{
						if (ptr3->level255 > 0 && (this.water_mng_bits & (1UL << (int)(ptr3->id & 31))) == 0UL)
						{
							ptr3->level255 = 0;
							MistManager.MistGenerator mistGenerator = this.AGen[(int)ptr3->id];
							if (mistGenerator.K.Msd != null)
							{
								mistGenerator.K.Msd.remPoint(j, i);
							}
						}
						ptr3++;
					}
				}
			}
		}

		public void burstMistArea(int cx, int cy, int r)
		{
			if (this.gen_id_max == 0)
			{
				return;
			}
			int num = r / 2;
			this.removeMistArea(cx - num + 1, cy - num, r - 2, r);
			this.removeMistArea(cx - num, cy - num + 1, r, r - 2);
			this.attachWind(cx - num - 4, cy - num - 1, num + 4, r + 2, AIM.L, 40);
			this.attachWind(cx, cy - num - 1, num + 4, r + 2, AIM.R, 40);
		}

		public void run()
		{
			if (this.water_mesh_v >= 0)
			{
				byte b = this.draw_intv - 1;
				this.draw_intv = b;
				if (b == 0)
				{
					this.drawWater((int)(this.draw_intv = ((X.AF == 4) ? 4 : 3)));
				}
			}
			this.Rvfirst_progress.Update(1f);
			if (this.gen_id_max == 0 && this.dmg_atk_max == 0 && this.mistmanager_whole == 0U && this.AWaterStatic == null)
			{
				return;
			}
			this.running_delay -= Map2d.TS;
			if (this.running_delay <= 0f)
			{
				byte b2 = 0;
				if (this.MeshForDebug != null)
				{
					this.MeshForDebug.clear(false, false);
					this.MeshForDebugCharacter.clear(false, false);
				}
				this.running_delay += 5f;
				this.runMistWater(false);
				if (this.reconsider_flag > 0)
				{
					Bench.P("MIST reconsider config");
					this.reconsider_flag = 2;
					this.Mp.considerConfig4((int)this.RcReconsider.x, (int)this.RcReconsider.y, (int)this.RcReconsider.right, (int)this.RcReconsider.bottom);
					this.reconsider_flag = 0;
					this.RcReconsider.Set(0f, 0f, 0f, 0f);
					Bench.Pend("MIST reconsider config");
				}
				this.runMistWaterDamageApply();
				this.runDamageApply(ref b2);
				if (b2 == 0 && this.water_mng_bits != 0UL && this.Mp.Pr != null)
				{
					if (this.SurfaceDraw != null)
					{
						b2 = ((this.SurfaceDraw.drawt >= this.Mp.map2meshy((this.water_applied_pr > 0) ? (this.Mp.Pr.mbottom + 0.25f) : this.Mp.Pr.y)) ? 1 : 0);
					}
					else
					{
						foreach (KeyValuePair<MistManager.MISTTYPE, M2WaterWatcher> keyValuePair in this.OWMng)
						{
							if (keyValuePair.Value.K_water_surface_top >= 0 && keyValuePair.Value.current_water_surface_top <= this.Mp.Pr.y)
							{
								b2 = 1;
								break;
							}
						}
					}
				}
				this.water_applied_pr = b2;
				if (this.MeshForDebug != null)
				{
					this.redrawDebug(this.MeshForDebug, this.MeshForDebugCharacter);
					this.MeshForDebug.updateForMeshRenderer(false);
					this.MeshForDebugCharacter.updateForMeshRenderer(false);
				}
			}
			if (this.water_mesh_v == -1 && (this.water_mng_bits & (ulong)(~(ulong)this.mistmanager_whole)) != 0UL)
			{
				this.MdWater = this.Mp.getWaterDrawer();
				this.water_mesh_v = this.MdWater.getVertexMax();
				this.water_mesh_t = this.MdWater.getTriMax();
				if (X.D_EF)
				{
					this.drawWater(X.AF_EF);
				}
			}
		}

		public void runFirstProgress()
		{
			this.Rvfirst_progress.Update(1f);
		}

		public void initFirstProgress(int t = 30)
		{
			this.Rvfirst_progress.Max((float)t, false);
		}

		public void clearFirstProgress()
		{
			this.Rvfirst_progress.Set(0f, false);
		}

		public unsafe bool runMistWater(bool check_static_area = false)
		{
			if (this.fine_water_slicer)
			{
				this.fineWaterSlicer();
			}
			if (this.gen_id_max == 0 || (this.appear_gen_bits & (ulong)(~(ulong)this.mistmanager_whole)) == 0UL || this.Rc.isEmpty())
			{
				if (check_static_area)
				{
					this.runCheckWaterManagement(0, 0, 0, 0, true);
				}
				return false;
			}
			this.RcBuf.Set(0f, 0f, 0f, 0f);
			if (this.gen_id_max < 64)
			{
				byte b = this.gen_id_max;
			}
			this.appear_gen_bits = (ulong)this.mistmanager_whole;
			int num = (int)X.Mn(this.Rc.x + this.Rc.w, (float)(this.Mp.clms - this.crop));
			int num2 = (int)X.Mn(this.Rc.y + this.Rc.h, (float)(this.Mp.rows - this.crop));
			int num3 = (int)X.Mx(this.Rc.x, (float)this.crop);
			int num4 = (int)X.Mx(this.Rc.y, (float)this.crop);
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				MistManager.MistPD* ptr3 = null;
				for (int i = num4; i < num2; i++)
				{
					MistManager.MistPD* ptr4 = null;
					MistManager.MistPD* ptr5 = this.getPtr(ptr2, num3, i);
					MistManager.MistPD* ptr6 = ptr5;
					for (int j = num3; j < num; j++)
					{
						ptr5->level_translated = 0;
						if (ptr5->level255 > 0)
						{
							if (!ptr5->isWaterStatic())
							{
								this.RcBuf.Expand((float)j, (float)i, 1f, 1f, false);
							}
							byte id = ptr5->id;
							this.appear_gen_bits |= 1UL << (int)id;
							MistManager.MistGenerator mistGenerator = this.AGen[(int)id];
							bool flag = true;
							if ((ptr5->useable_step || ptr5->step <= mistGenerator.K.max_influence) && mistGenerator.amount > 0)
							{
								ptr5->assignReduceLock((int)this.Mp.floort, 20);
								this.Reduce(ptr5, mistGenerator, j, i);
								flag = false;
								int num5 = ((ptr5->step <= 1) ? 255 : ((255 + ptr5->level255) / 2));
								if (ptr5->level255 < num5)
								{
									int num6 = X.Mn(mistGenerator.amount, num5 - ptr5->level255);
									if (!mistGenerator.infinity)
									{
										mistGenerator.amount -= num6;
									}
									ptr5->level255 += num6;
								}
							}
							if (ptr4 != null)
							{
								int num7 = this.checkInfluence(ptr5, ptr4, mistGenerator, j, i, j - 1, i, true, true);
								if ((num7 & 1) != 0)
								{
									this.RcBuf.Expand((float)(j - 1), (float)i, 1f, 1f, false);
								}
								else
								{
									int num8 = num7 & 2;
								}
							}
							if (ptr3 != null && (this.checkInfluence(ptr5, ptr3, mistGenerator, j, i, j, i - 1, true, true) & 1) != 0)
							{
								this.RcBuf.Expand((float)j, (float)(i - 1), 1f, 1f, false);
							}
							if (flag)
							{
								this.Reduce(ptr5, mistGenerator, j, i);
							}
							if (mistGenerator.K.Msd != null)
							{
								mistGenerator.K.Msd.fineAlpha(j, i, ptr5->level255);
							}
						}
						else
						{
							if (ptr4 != null && ptr4->level255 > 0)
							{
								MistManager.MistGenerator mistGenerator = this.AGen[(int)ptr4->id];
								if ((this.checkInfluence(ptr4, ptr5, mistGenerator, j - 1, i, j, i, false, true) & 1) != 0)
								{
									this.RcBuf.Expand((float)(j - 1), (float)i, 2f, 1f, false);
								}
							}
							if (ptr3 != null && ptr3->level255 > 0)
							{
								MistManager.MistGenerator mistGenerator = this.AGen[(int)ptr3->id];
								if ((this.checkInfluence(ptr3, ptr5, mistGenerator, j, i - 1, j, i, false, true) & 1) != 0)
								{
									this.RcBuf.Expand((float)j, (float)(i - 1), 1f, 2f, false);
								}
							}
						}
						ptr4 = ptr5;
						ptr5++;
						if (ptr3 != null)
						{
							ptr3++;
						}
					}
					ptr3 = ptr6;
				}
				this.runCheckWaterManagement(num3, num4, num, num2, check_static_area);
			}
			this.use_config_bits &= this.appear_gen_bits;
			if (this.RcBuf.isEmpty())
			{
				this.clear(false);
				if (check_static_area)
				{
					this.runCheckWaterManagement(0, 0, 0, 0, true);
				}
				return false;
			}
			X.rectMultiply(this.Rc.Set(this.RcBuf.x - 1f, this.RcBuf.y - 1f, this.RcBuf.width + 2f, this.RcBuf.height + 2f), (float)this.crop, (float)this.crop, (float)(this.Mp.clms - this.crop * 2), (float)(this.Mp.rows - this.crop * 2));
			return true;
		}

		public void runCheckWaterManagementStatic()
		{
			if (this.AWaterStatic == null || this.water_static_bits == 0UL)
			{
				return;
			}
			this.runCheckWaterManagement(0, 0, 0, 0, true);
		}

		private unsafe void runCheckWaterManagement(int sx, int sy, int maxx, int maxy, bool check_static_area = false)
		{
			if (this.AAPd == null)
			{
				return;
			}
			M2WaterWatcher.static_area_searching = check_static_area;
			if (check_static_area)
			{
				this.RcBufS.Set((float)sx, (float)sy, (float)(maxx - sx), (float)(maxy - sy));
				this.ExpandWaterStaticArea(this.RcBufS);
				X.rectMultiply(this.RcBufS, (float)this.crop, (float)this.crop, (float)(this.Mp.clms - this.crop * 2), (float)(this.Mp.rows - this.crop * 2));
				sx = (int)this.RcBufS.x;
				sy = (int)this.RcBufS.y;
				maxx = (int)this.RcBufS.right;
				maxy = (int)this.RcBufS.bottom;
			}
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				MistManager.MistPD* ptr3 = null;
				ulong num = (this.water_mng_bits & this.appear_gen_bits) | this.water_static_bits;
				if (num != 0UL)
				{
					for (int i = sy; i < maxy; i++)
					{
						MistManager.MistPD* ptr4 = this.getPtr(ptr2, sx, i);
						int num2 = 0;
						MistManager.MISTTYPE misttype = MistManager.MISTTYPE.NONE;
						for (int j = sx; j < maxx; j++)
						{
							MistManager.MISTTYPE misttype2 = MistManager.MISTTYPE.NONE;
							ptr4->level_translated = 0;
							if (ptr4->level255 > 0)
							{
								MistManager.MistKind k = this.AGen[(int)ptr4->id].K;
								if (k.isWater())
								{
									misttype2 = k.type;
								}
							}
							if (misttype2 != misttype)
							{
								if (num2 > 0)
								{
									this.OWMng[misttype].check(ptr4 - num2, j - num2, j - 1, i);
									num2 = 0;
								}
								misttype = misttype2;
							}
							if (misttype != MistManager.MISTTYPE.NONE)
							{
								num2++;
							}
							ptr4++;
						}
						if (misttype != MistManager.MISTTYPE.NONE && num2 > 0)
						{
							this.OWMng[misttype].check(ptr4 - num2, maxx - num2, maxx - 1, i);
						}
					}
					bool flag = false;
					using (Dictionary<MistManager.MISTTYPE, M2WaterWatcher>.Enumerator enumerator = this.OWMng.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<MistManager.MISTTYPE, M2WaterWatcher> keyValuePair = enumerator.Current;
							flag = keyValuePair.Value.finalizeCheck(ptr2) || flag;
						}
						goto IL_0235;
					}
				}
				this.releaseWaterMesh();
				IL_0235:
				this.water_mng_bits = num;
			}
			M2WaterWatcher.static_area_searching = false;
		}

		private unsafe void runMistWaterDamageApply()
		{
			if ((this.gen_id_max == 0 || (this.appear_gen_bits & (ulong)(~(ulong)this.mistmanager_whole)) == 0UL) && this.AWaterStatic == null && this.mistmanager_whole == 0U)
			{
				return;
			}
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				int count_movers = this.Mp.count_movers;
				DRect waterExistRect = this.getWaterExistRect(true);
				int num = ((this.mistmanager_whole != 0U) ? X.beki_cnt(this.mistmanager_whole) : 0);
				for (int i = 0; i < count_movers; i++)
				{
					M2Mover mv = this.Mp.getMv(i);
					NelM2Attacker nelM2Attacker = mv as NelM2Attacker;
					if (nelM2Attacker != null)
					{
						MistManager.MistKind mistKind = null;
						float num2;
						float num3;
						nelM2Attacker.getMouthPosition(out num2, out num3);
						int num4 = 0;
						int num5 = 0;
						bool flag = false;
						if (nelM2Attacker.canApplyMistDamage() && X.BTW((float)this.crop, num2, (float)(this.Mp.clms - this.crop)) && X.BTW((float)this.crop, num3, (float)(this.Mp.rows - this.crop)) && waterExistRect.isin(num2, num3, 0f))
						{
							MistManager.MistPD mistPD = *this.getPtr(ptr2, (int)num2, (int)num3);
							if (mistPD.level255 > 0)
							{
								MistManager.MistGenerator mistGenerator = this.AGen[(int)mistPD.id];
								num4 = mistPD.level255;
								if (num4 >= mistGenerator.K.apply_level255)
								{
									mistKind = mistGenerator.K;
									num5 = (int)mistPD.id;
									flag = mistPD.aimGoesST();
								}
							}
						}
						if (mistKind == null && this.mistmanager_whole != 0U)
						{
							mistKind = this.AGen[num].K;
							num4 = 255;
							num5 = num;
						}
						if (mistKind != null)
						{
							this.applyDamageDaInit(nelM2Attacker, mistKind, num5, num4).fall_flag = flag;
						}
						M2Phys physic = mv.getPhysic();
						if (physic != null)
						{
							if (this.mistmanager_whole != 0U)
							{
								physic.setWaterDunk(num, (int)this.AGen[num].K.type);
							}
							else
							{
								int num6 = (int)(mv.mbottom - 0.125f);
								if (X.BTW((float)this.crop, (float)num6, (float)(this.Mp.rows - this.crop)) && waterExistRect.isin(num2, (float)num6, 0f))
								{
									MistManager.MistPD mistPD2 = *this.getPtr(ptr2, (int)num2, num6);
									if (mistPD2.level255 > 0)
									{
										MistManager.MistGenerator mistGenerator2 = this.AGen[(int)mistPD2.id];
										num4 = mistPD2.level255;
										if (mistGenerator2.K.isWaterSwamp() && num4 >= mistGenerator2.K.apply_level255)
										{
											physic.setWaterDunk((int)mistPD2.id, (int)mistGenerator2.K.type);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		private MistManager.MistDamageApply applyDamageDaInit(NelM2Attacker Mv, MistManager.MistKind AtkMist, int id, int level255)
		{
			MistManager.MistDamageApply da = this.getDA(AtkMist, Mv, id);
			da.timeout = X.Mx(da.timeout, 30 + ((da.timeout >= 30) ? 5 : 0));
			da.delay = X.VALWALK(X.Abs(da.delay), 0, 5);
			if (da.crt <= CREATION.HIDE_RUNNING)
			{
				da.crt = CREATION.ACTIVATE;
			}
			bool flag = AtkMist.applyDamageDefault(AtkMist, Mv, (float)(level255 * X.MPF(da.delay == 0)));
			da.delay = ((flag && AtkMist.damage_cooltime >= 0) ? AtkMist.damage_cooltime : ((da.delay <= 0) ? 1 : da.delay));
			if (flag)
			{
				da.dmg_count += X.MPF(da.dmg_count >= 0);
			}
			return da;
		}

		private void runDamageApply(ref byte water_applied_pr)
		{
			bool flag = false;
			M2MoverPr keyPr = this.Mp.getKeyPr();
			if (this.FlgHideSurface.isActive() && this.SurfaceDraw != null && keyPr != null && CCON.isWater(this.Mp.getConfig((int)(keyPr.drawx * this.Mp.rCLEN), (int)(keyPr.drawy * this.Mp.rCLEN + keyPr.sizey * 0.2f))))
			{
				water_applied_pr = 2;
			}
			for (int i = this.dmg_atk_max - 1; i >= 0; i--)
			{
				MistManager.MistDamageApply mistDamageApply = this.ADmgAtk[i];
				if (mistDamageApply.K == null)
				{
					if (this.dmg_atk_max == i - 1)
					{
						this.dmg_atk_max--;
					}
				}
				else
				{
					flag = true;
					if (mistDamageApply.timeout <= 0)
					{
						bool flag2 = true;
						if (mistDamageApply.K.damage_count_reduce > 0 && mistDamageApply.dmg_count > 0)
						{
							mistDamageApply.timeout -= 5;
							if (mistDamageApply.timeout <= -mistDamageApply.K.damage_count_reduce)
							{
								mistDamageApply.timeout += mistDamageApply.K.damage_count_reduce;
								mistDamageApply.dmg_count--;
							}
							flag2 = flag2 && mistDamageApply.dmg_count > 0;
						}
						mistDamageApply.crt = (flag2 ? CREATION.DESTRUCT : ((mistDamageApply.crt >= CREATION.RUNNING) ? CREATION.DEACTIVATE : CREATION.HIDE_RUNNING));
						if (mistDamageApply.K.isWater())
						{
							flag2 = this.OWMng[mistDamageApply.K.type].applySinkEffect(mistDamageApply) && flag2;
						}
						if (flag2)
						{
							if (i == this.dmg_atk_max - 1)
							{
								this.dmg_atk_max--;
							}
							this.ADmgAtk[i].K = null;
						}
						else if (mistDamageApply.delay < 0)
						{
							mistDamageApply.delay = 0;
						}
					}
					else
					{
						if (mistDamageApply.timeout < 30)
						{
							mistDamageApply.delay = X.VALWALK(X.Abs(mistDamageApply.delay), 0, 5);
							if (mistDamageApply.crt >= CREATION.RUNNING)
							{
								mistDamageApply.crt = CREATION.DEACTIVATE;
							}
							else
							{
								mistDamageApply.crt = CREATION.HIDE_RUNNING;
							}
						}
						if (mistDamageApply.K.isWater())
						{
							this.OWMng[mistDamageApply.K.type].applySinkEffect(mistDamageApply);
							if (mistDamageApply.Atk as M2Mover == this.Mp.getKeyPr())
							{
								water_applied_pr = 2;
							}
						}
						if (mistDamageApply.crt > CREATION.RUNNING)
						{
							mistDamageApply.crt = CREATION.RUNNING;
						}
						mistDamageApply.timeout -= 5;
					}
				}
			}
			if (!flag)
			{
				this.dmg_atk_max = 0;
			}
		}

		public void releaseWaterDunk(NelM2Attacker Atk)
		{
			for (int i = this.dmg_atk_max - 1; i >= 0; i--)
			{
				MistManager.MistDamageApply mistDamageApply = this.ADmgAtk[i];
				if (mistDamageApply.Atk == null && mistDamageApply.K.isWater())
				{
					mistDamageApply.timeout = 0;
					mistDamageApply.dmg_count = 0;
					mistDamageApply.delay = 0;
				}
			}
		}

		private unsafe int Reduce(MistManager.MistPD* P, MistManager.MistGenerator G, int _x, int _y)
		{
			return this.Reduce(P, G, _x, _y, G.K.reduce255, P->useable_step);
		}

		private unsafe int Reduce(MistManager.MistPD* P, MistManager.MistGenerator G, int _x, int _y, int red, bool alloc_reduce_from_amount = true)
		{
			int num = 0;
			if (!alloc_reduce_from_amount && (float)P->reduce_lock > this.Mp.floort)
			{
				alloc_reduce_from_amount = true;
			}
			if (alloc_reduce_from_amount)
			{
				int num2 = X.Mn(G.amount, red);
				if (!G.infinity)
				{
					G.amount -= num2;
				}
				num += num2;
				red -= num2;
			}
			red = X.Mn(P->level255, red);
			P->level255 = P->level255 - red;
			if (P->level255 <= 0)
			{
				this.clearAt(P, G, _x, _y);
				if (this.Rvfirst_progress > 0f)
				{
					this.Rvfirst_progress.Max(20f, false);
				}
			}
			return num + red;
		}

		private unsafe int checkInfluence(MistManager.MistPD* P, MistManager.MistPD* Pd, MistManager.MistGenerator G, int sx, int sy, int dx, int dy, bool reverse = true, bool mixing = true)
		{
			int num = 0;
			MistManager.MistGenerator mistGenerator = null;
			MistManager.MistPD* ptr = null;
			if (P->level255 > 0)
			{
				if (G == null)
				{
					G = this.AGen[(int)P->id];
				}
				if (Pd->level255 == 0)
				{
					MistManager.MistPD mistPD = *P;
					if (G.K.canInfluenceTo(P, Pd, G, this, sx, sy, dx, dy))
					{
						Pd->id = P->id;
						Pd->setStepBehind((int)P->step_behind, G.K, true);
						Pd->aimTouch(G.K, dx, dy, sx, sy, 0, this);
						Pd->reduce_lock = (int)this.Mp.floort + 10;
						if ((float)P->reduce_lock > this.Mp.floort)
						{
							Pd->reduce_lock = X.Mx(Pd->reduce_lock, P->reduce_lock + ((int)this.Mp.floort - P->reduce_lock_assigned));
						}
						if (G.useable_count > 0)
						{
							Pd->useable_step = true;
							G.useable_count--;
						}
						else
						{
							Pd->useable_step = false;
						}
						int num2 = this.Reduce(P, G, dx, dy, G.K.influence_threshold255, Pd->useable_step || (G.K.max_behind_influence >= 0 && (int)Pd->step_behind <= G.K.max_behind_influence));
						Pd->levelAdd(num2);
						Pd->step = P->step + 1;
						int num3 = (Pd->wind = P->weekenWind(0.4f));
						if (G.K.isWater())
						{
							if ((this.water_mng_bits & (1UL << (int)P->id)) != 0UL)
							{
								this.OWMng[G.K.type].assignOrGenerateChip(dx, dy, null);
								this.reconsider_flag = 1;
								this.RcReconsider.Expand((float)dx, (float)dy, 1f, 1f, false);
							}
							if (this.Rvfirst_progress > 0f)
							{
								this.Rvfirst_progress.Max(30f, false);
							}
						}
						else
						{
							G.K.Msd.addPoint(this.Mp, dx, dy, num2, num3);
						}
						reverse = (mixing = false);
						num |= 1;
						mistGenerator = G;
						ptr = Pd;
					}
				}
				if (Pd->level255 > 0)
				{
					if (mixing)
					{
						mistGenerator = this.AGen[(int)Pd->id];
					}
					if (mistGenerator != null && mistGenerator.K == G.K)
					{
						bool flag = P->isWaterStatic() == Pd->isWaterStatic();
						if (flag)
						{
							MistManager.MistPD* ptr2 = null;
							MistManager.MistPD* ptr3 = null;
							int num4 = 0;
							int num5 = 0;
							int level_transable = Pd->level_transable;
							int level_transable2 = P->level_transable;
							int num6 = ((sx != dx) ? mistGenerator.K.influence_speed255_lr : mistGenerator.K.influence_speed255_tb);
							if (level_transable >= P->level255 && Pd->step <= P->step)
							{
								ptr2 = P;
								ptr3 = Pd;
								num4 = P->level255;
								num5 = level_transable;
							}
							else if (Pd->level255 <= level_transable2 && Pd->step >= P->step)
							{
								ptr2 = Pd;
								ptr3 = P;
								num4 = Pd->level255;
								num5 = level_transable2;
							}
							else if (G.K.max_behind_influence >= 0)
							{
								if (level_transable >= P->level255 && Pd->step_behind < P->step_behind)
								{
									ptr2 = P;
									ptr3 = Pd;
									num4 = P->level255;
									num5 = level_transable;
								}
								else if (Pd->level255 <= level_transable2 && P->step_behind < Pd->step_behind)
								{
									ptr2 = Pd;
									ptr3 = P;
									num4 = Pd->level255;
									num5 = level_transable2;
								}
							}
							if (ptr2 != null && num5 >= num4)
							{
								int num7 = X.Mn(num6, num5 - num4);
								if (G.amount >= num7 && (ptr3->useable_step || ptr2->useable_step || ptr3->step < G.K.max_influence))
								{
									if (!G.infinity)
									{
										G.amount -= num7;
									}
									ptr2->assignReduceLock((int)this.Mp.floort, 35);
									ptr3->assignReduceLock(ptr2->reduce_lock_assigned, 35);
									ptr2->levelAdd(num7);
								}
								else if (num7 > 0)
								{
									if (num5 < num6 * 2)
									{
										ptr3->levelAdd(-num7);
										ptr2->levelAdd(num7);
									}
									else
									{
										int num8 = (Pd->level255 + P->level255) / 2;
										int num9 = num6 * ((P->useable_step && Pd->useable_step) ? 5 : 1);
										P->levelAdd(X.VALWALK(P->level255, num8, num9) - P->level255);
										Pd->levelAdd(X.VALWALK(Pd->level255, num8, num9) - Pd->level255);
									}
								}
							}
							if (P->step != Pd->step)
							{
								MistManager.MistPD* ptr4 = null;
								MistManager.MistPD* ptr5 = null;
								if (P->step > Pd->step)
								{
									ptr4 = Pd;
									ptr5 = P;
								}
								else
								{
									ptr4 = P;
									ptr5 = Pd;
								}
								int num10 = ptr4->step + 1;
								if (ptr5->step > num10)
								{
									ptr5->step = num10;
								}
								if (ptr4->reduce_lock_assigned > ptr5->reduce_lock_assigned)
								{
									ptr5->reduce_lock = ptr4->reduce_lock + ((int)this.Mp.floort - ptr4->reduce_lock_assigned);
									ptr5->reduce_lock_assigned = ptr4->reduce_lock_assigned;
								}
								else if (ptr4->reduce_lock_assigned < ptr5->reduce_lock_assigned)
								{
									ptr4->reduce_lock = ptr5->reduce_lock + ((int)this.Mp.floort - ptr5->reduce_lock_assigned);
									ptr4->reduce_lock_assigned = ptr5->reduce_lock_assigned;
								}
							}
						}
						if (G.K.tb_shift != 0 && sy != dy)
						{
							if (G.K.tb_shift < 0 == sy < dy)
							{
								if (ptr != P)
								{
									int num11 = X.Mn(X.Mn(X.Abs(G.K.tb_shift), Pd->level_transable), 255 - (P->isWaterStatic() ? 0 : P->level255));
									this.Reduce(Pd, G, dx, dy, num11, false);
									P->reduce_lock = X.Mx(Pd->reduce_lock, P->reduce_lock);
									P->levelAdd(num11);
								}
							}
							else if (ptr != Pd)
							{
								int num12 = X.Mn(X.Mn(X.Abs(G.K.tb_shift), P->level255), 255 - (Pd->isWaterStatic() ? 0 : Pd->level255));
								this.Reduce(P, G, sx, sy, num12, false);
								Pd->reduce_lock = X.Mx(Pd->reduce_lock, P->reduce_lock);
								Pd->levelAdd(num12);
							}
						}
						if (flag && mistGenerator.K.isWater())
						{
							if (sy != dy)
							{
								MistManager.MistPD* ptr6 = ((sy < dy) ? P : Pd);
								MistManager.MistPD* ptr7 = ((sy < dy) ? Pd : P);
								if ((ptr7->aim_check & MistManager.AimCk.ALLOW_GO_BEHIND) != MistManager.AimCk.NONE && ptr6->canStand(this.Mp, ptr7, sx, (sy < dy) ? dy : sy, sx, (sy < dy) ? sy : dy))
								{
									ptr7->aim_check &= (MistManager.AimCk)(-8388609);
									ptr6->setStepBehind(X.Mx((int)(ptr7->step_behind + 1), (int)ptr6->step_behind), G.K, false);
									if (G.K.max_behind_influence < 0 || (int)ptr6->step_behind <= G.K.max_behind_influence)
									{
										ptr6->aim_check |= MistManager.AimCk.ALLOW_GO_INFLUENCE;
									}
								}
							}
							if (sy == dy && P->aimGoesST() == Pd->aimGoesST())
							{
								int num13 = (int)X.Mn(P->step_behind, Pd->step_behind);
								P->setStepBehind(num13, G.K, false);
								Pd->setStepBehind(num13, G.K, false);
							}
						}
						P->aimTouch(G.K, sx, sy, dx, dy, -1, null);
						Pd->aimTouch(G.K, dx, dy, sx, sy, -1, null);
						if (G.K.isWater())
						{
						}
						num |= 2;
					}
					else if (sy != dy && G.K.isWater())
					{
						if (mistGenerator == null)
						{
							mistGenerator = this.AGen[(int)Pd->id];
						}
						if (mistGenerator.K.isWater())
						{
							((sy < dy) ? P : Pd)->aimTouch(0, 1);
						}
					}
					if (mistGenerator != null && (Pd->wind != 32639 || P->wind != 32639) && !mistGenerator.K.isWater() && !G.K.isWater())
					{
						if (sx == dx)
						{
							MistManager.MistPD* ptr8;
							MistManager.MistGenerator mistGenerator2;
							MistManager.MistPD* ptr9;
							MistManager.MistGenerator mistGenerator3;
							if (sy < dy)
							{
								ptr8 = P;
								mistGenerator2 = G;
								ptr9 = Pd;
								mistGenerator3 = mistGenerator;
							}
							else
							{
								ptr8 = Pd;
								mistGenerator2 = mistGenerator;
								ptr9 = P;
								mistGenerator3 = G;
							}
							int wind_y = ptr8->wind_y;
							int wind_y2 = ptr9->wind_y;
							int num14 = 0;
							if (wind_y > 0)
							{
								if ((ptr8->wind_y = wind_y - 1) == 0 && mistGenerator2.K.Msd != null)
								{
									mistGenerator2.K.Msd.fineWind(this.Mp, sx, X.Mn(sy, dy), ptr8->wind);
								}
								num14 += 30;
							}
							if (wind_y2 < 0)
							{
								if ((ptr9->wind_y = wind_y2 + 1) == 0 && mistGenerator3.K.Msd != null)
								{
									mistGenerator3.K.Msd.fineWind(this.Mp, sx, X.Mx(sy, dy), ptr9->wind);
								}
								num14 -= 30;
							}
							if (num14 != 0)
							{
								MistManager.MistPD.transfer(ptr8, ptr9, num14);
							}
						}
						if (sy == dy)
						{
							MistManager.MistPD* ptr10;
							MistManager.MistGenerator mistGenerator4;
							MistManager.MistPD* ptr11;
							MistManager.MistGenerator mistGenerator5;
							if (sx < dx)
							{
								ptr10 = P;
								mistGenerator4 = G;
								ptr11 = Pd;
								mistGenerator5 = mistGenerator;
							}
							else
							{
								ptr10 = Pd;
								mistGenerator4 = mistGenerator;
								ptr11 = P;
								mistGenerator5 = G;
							}
							int wind_x = ptr10->wind_x;
							int wind_x2 = ptr11->wind_x;
							int num15 = 0;
							if (wind_x > 0)
							{
								if ((ptr10->wind_x = wind_x - 1) == 0 && mistGenerator4.K.Msd != null)
								{
									mistGenerator4.K.Msd.fineWind(this.Mp, X.Mn(sx, dx), sy, ptr10->wind);
								}
								num15 += 30;
							}
							if (wind_x2 < 0)
							{
								if ((ptr11->wind_x = wind_x2 + 1) == 0 && mistGenerator5.K.Msd != null)
								{
									mistGenerator5.K.Msd.fineWind(this.Mp, X.Mx(sx, dx), sy, ptr11->wind);
								}
								num15 -= 30;
							}
							if (num15 != 0)
							{
								MistManager.MistPD.transfer(ptr10, ptr11, num15);
							}
						}
					}
					mixing = false;
				}
			}
			if (reverse)
			{
				return (this.checkInfluence(Pd, P, mistGenerator, dx, dy, sx, sy, false, mixing) << 8) | num;
			}
			return num;
		}

		private MistManager.MistDamageApply getDA(MistManager.MistKind K, NelM2Attacker Dmg, int gen_id)
		{
			MistManager.MistDamageApply mistDamageApply = null;
			bool flag = true;
			for (int i = 0; i < this.dmg_atk_max; i++)
			{
				MistManager.MistDamageApply mistDamageApply2 = this.ADmgAtk[i];
				if (mistDamageApply2.K == K && mistDamageApply2.Atk == Dmg)
				{
					return mistDamageApply2;
				}
				if (mistDamageApply2.K == null)
				{
					mistDamageApply = mistDamageApply2;
					flag = false;
					break;
				}
			}
			if (mistDamageApply == null)
			{
				if (this.dmg_atk_max < this.ADmgAtk.Count)
				{
					mistDamageApply = this.ADmgAtk[this.dmg_atk_max];
				}
				else
				{
					this.ADmgAtk.Add(mistDamageApply = new MistManager.MistDamageApply());
				}
			}
			mistDamageApply.K = K;
			mistDamageApply.gen_id = gen_id;
			mistDamageApply.Atk = Dmg;
			mistDamageApply.timeout = 30;
			mistDamageApply.delay = 0;
			mistDamageApply.crt = CREATION.CREATED;
			mistDamageApply.af = 0f;
			mistDamageApply.dmg_count = 0;
			if (flag)
			{
				this.dmg_atk_max++;
			}
			return mistDamageApply;
		}

		public unsafe void configReconsidered(M2Pt[,] AAPt, int _l0, int _t0, int _r0, int _b0)
		{
			ulong num = 0UL;
			if (this.whole_fill_config > 0)
			{
				for (int i = _t0; i < _b0; i++)
				{
					for (int j = _l0; j < _r0; j++)
					{
						CCON.calcConfigManual(ref AAPt[j, i], (int)this.whole_fill_config);
					}
				}
			}
			if (this.AWaterStatic != null)
			{
				for (int k = this.AWaterStatic.Count - 1; k >= 0; k--)
				{
					MistManager.MistStaticArea mistStaticArea = this.AWaterStatic[k];
					int num2 = X.Mx(mistStaticArea.x, _l0);
					int num3 = X.Mn(mistStaticArea.right, _r0);
					int num4 = X.Mx(mistStaticArea.reachable_y, _t0);
					int num5 = X.Mn(mistStaticArea.bottom, _b0);
					if (num2 < num3 && num4 < num5)
					{
						for (int l = num4; l < num5; l++)
						{
							for (int m = num2; m < num3; m++)
							{
								CCON.calcConfigManual(ref AAPt[m, l], mistStaticArea.config);
							}
						}
					}
				}
			}
			if (!this.Rc.isEmpty())
			{
				int num6 = X.Mx(this.crop, _t0);
				int num7 = X.Mx(this.crop, _l0);
				int num8 = X.Mn(this.Mp.clms - this.crop, _r0);
				int num9 = X.Mn(this.Mp.rows - this.crop, _b0);
				if (num6 < num9 && num7 < num8 && this.Rc.isCoveringXy((float)num7, (float)num6, (float)num8, (float)num9, 1f, -1000f))
				{
					fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
					{
						MistManager.MistPD* ptr2 = ptr;
						for (int n = num6; n < num9; n++)
						{
							MistManager.MistPD* ptr3 = this.getPtr(ptr2, num7, n);
							for (int num10 = num7; num10 < num8; num10++)
							{
								if (ptr3->level255 > 0 && (this.use_config_bits & (1UL << (int)ptr3->id)) != 0UL)
								{
									M2Chip baseChip = this.AGen[(int)ptr3->id].BaseChip;
									if (baseChip != null)
									{
										CCON.calcConfigManual(ref AAPt[num10, n], (int)baseChip.Img.Aconfig[0]);
									}
								}
								if (this.reconsider_flag < 2 && (ptr3->aim_check & MistManager.AimCk.CFG_CHECKED) != MistManager.AimCk.NONE)
								{
									MistManager.AimCk aimCk = ptr3->aim_check & (MistManager.AimCk)24576;
									MistManager.AimCk aimCk2 = ptr3->checkCanStand(this.Mp, num10, n);
									if (aimCk2 != aimCk)
									{
										ptr3->writeCanStandFlag(aimCk2);
										MistManager.MistKind mistKind = ((ptr3->level255 <= 0) ? null : this.AGen[(int)ptr3->id].K);
										if (aimCk2 == MistManager.AimCk.NONE && ptr3->level255 > 0)
										{
											this.clearAt(ptr3, this.AGen[(int)ptr3->id], num10, n);
											mistKind = null;
										}
										if (mistKind == null || mistKind.mist_aim != MistManager.MISTAIM.NO_AIM)
										{
											num |= this.reconsiderConfigAround(ptr2, mistKind, num10, n, aimCk > aimCk2);
										}
									}
								}
								ptr3++;
							}
						}
					}
				}
			}
			if (this.reconsider_flag < 2 && this.OWMng != null)
			{
				foreach (KeyValuePair<MistManager.MISTTYPE, M2WaterWatcher> keyValuePair in this.OWMng)
				{
					keyValuePair.Value.reconsiderConfigAround(_t0, _b0);
					if ((num & this.water_mng_bits) != 0UL)
					{
						keyValuePair.Value.fine_flag = true;
					}
				}
			}
		}

		public unsafe void clearAllWater()
		{
			this.removeSurfaceReflectDrawer();
			if (this.AAPd == null || this.OWMng == null || this.water_mng_bits == 0UL)
			{
				return;
			}
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				int num = (int)X.Mn(this.Rc.x + this.Rc.w, (float)(this.Mp.clms - this.crop));
				int num2 = (int)X.Mn(this.Rc.y + this.Rc.h, (float)(this.Mp.rows - this.crop));
				int num3 = (int)X.Mx(this.Rc.x, (float)this.crop);
				for (int i = (int)X.Mx(this.Rc.y, (float)this.crop); i < num2; i++)
				{
					MistManager.MistPD* ptr3 = this.getPtr(ptr2, num3, i);
					for (int j = num3; j < num; j++)
					{
						if (ptr3->level255 > 0)
						{
							MistManager.MistGenerator mistGenerator = this.AGen[(int)ptr3->id];
							if (mistGenerator.K.isWater())
							{
								this.clearAt(ptr3, mistGenerator, j, i);
							}
						}
						ptr3++;
					}
				}
			}
			this.water_mng_bits = 0UL;
			foreach (KeyValuePair<MistManager.MISTTYPE, M2WaterWatcher> keyValuePair in this.OWMng)
			{
				keyValuePair.Value.clear();
			}
			this.OWMng.Clear();
		}

		public unsafe void setStaticWater(MistManager.MistKind K, int x, int y, int behind_step_room)
		{
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				MistManager.MistPD* ptr3 = this.getPtr(ptr2, x, y);
				if (ptr3->level255 > 0)
				{
					MistManager.MistGenerator mistGenerator = this.AGen[(int)ptr3->id];
					if (mistGenerator.K == K)
					{
						ptr3->setStaticWater(K, behind_step_room, true);
						this.reconsider_flag = 1;
						this.RcReconsider.Expand((float)x, (float)y, 1f, 1f, false);
						if (mistGenerator.K.Msd != null)
						{
							mistGenerator.K.Msd.addPoint(this.Mp, x, y, ptr3->level255, 32639);
						}
					}
				}
			}
		}

		public unsafe void clearAt(MistManager.MistKind K, int x, int y)
		{
			if (this.AAPd == null)
			{
				return;
			}
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				MistManager.MistPD* ptr3 = this.getPtr(ptr2, x, y);
				if (ptr3->level255 > 0)
				{
					MistManager.MistGenerator mistGenerator = this.AGen[(int)ptr3->id];
					if (mistGenerator.K == K)
					{
						this.clearAt(ptr3, mistGenerator, x, y);
					}
				}
			}
		}

		public unsafe void clearAt(MistManager.MistPD* PtrC, MistManager.MistGenerator G, int x, int y)
		{
			PtrC->level255 = 0;
			PtrC->aim_check = MistManager.AimCk.BEHIND_HAS_ROOM;
			if (G == null)
			{
				return;
			}
			if (G.K.isWater())
			{
				if (G.BaseChip != null)
				{
					M2WaterWatcher m2WaterWatcher;
					if (this.OWMng != null && this.OWMng.TryGetValue(G.K.type, out m2WaterWatcher))
					{
						m2WaterWatcher.removeChipAt(x, y);
					}
					if ((this.water_mng_bits & (1UL << (int)PtrC->id)) != 0UL)
					{
						this.reconsider_flag = 1;
						this.RcReconsider.Expand((float)x, (float)y, 1f, 1f, false);
						return;
					}
				}
			}
			else if (G.K.Msd != null)
			{
				G.K.Msd.remPoint(x, y);
			}
		}

		public unsafe void removeAt(int x, int y)
		{
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				this.getPtr(ptr2, x, y)->level255 = 0;
			}
		}

		public void waveActEffect(float x, float y)
		{
			M2WaterWatcher waterInfo = this.getWaterInfo(x, y, false);
			if (waterInfo != null)
			{
				waterInfo.waveActEffect(x, y);
			}
		}

		public bool isFallingWater(int x, int y)
		{
			M2WaterWatcher waterInfo = this.getWaterInfo((float)x, (float)y, false);
			return waterInfo != null && waterInfo.isFallingWater(x, y);
		}

		public unsafe M2WaterWatcher getWaterInfo(float x, float y, bool strict = false)
		{
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				MistManager.MistPD* ptr3 = this.getPtr(ptr2, (int)x, (int)y);
				if (ptr3->level255 == 0)
				{
					return null;
				}
				MistManager.MistKind k = this.AGen[(int)ptr3->id].K;
				M2WaterWatcher m2WaterWatcher;
				if (this.OWMng == null || !this.OWMng.TryGetValue(k.type, out m2WaterWatcher))
				{
					ptr = null;
					return null;
				}
				if (!strict)
				{
					return m2WaterWatcher;
				}
				int num = (int)x;
				int num2 = (int)y;
				M2WaterWatcher.WLine lineAt = m2WaterWatcher.getLineAt(num, num2, false);
				if (lineAt == null)
				{
					return null;
				}
				M2WaterWatcher.WLDraw wldraw = lineAt.getDrawnFlow2Bottom(num);
				if (wldraw != null)
				{
					if (ptr3->level255 < k.apply_level255)
					{
						return null;
					}
					return m2WaterWatcher;
				}
				else
				{
					wldraw = lineAt.getFilledDrawn(num);
					if (wldraw == null)
					{
						return null;
					}
					if (1f - X.frac(y) > wldraw.drawlevel)
					{
						return null;
					}
					return m2WaterWatcher;
				}
			}
		}

		private unsafe ulong reconsiderConfigAround(MistManager.MistPD* Ptr0, MistManager.MistKind K, int _x, int _y, bool cut_fromdata)
		{
			ulong num = 0UL;
			for (int i = 0; i < 4; i++)
			{
				int num2 = _x + CAim._XD(i, 1);
				int num3 = _y - CAim._YD(i, 1);
				if (this.Rc.isin((float)num2, (float)num3, 0f))
				{
					MistManager.MistPD* ptr = this.getPtr(Ptr0, num2, num3);
					if (ptr->level255 != 0)
					{
						MistManager.MistKind k = this.AGen[(int)ptr->id].K;
						if ((K == null || (ptr->level255 > 0 && k != K)) && ptr->cutSpecificAim(k, CAim.get_opposite((AIM)i), cut_fromdata))
						{
							num |= 1UL << (int)ptr->id;
						}
					}
				}
			}
			return num;
		}

		public bool isWaterManagerActive()
		{
			return this.water_mng_bits > 0UL;
		}

		public unsafe string checkWaterFootSound(float x, float y, ref string snd_type)
		{
			if (this.water_mng_bits == 0UL || this.OWMng == null)
			{
				return snd_type;
			}
			string text;
			try
			{
				try
				{
					fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
					{
						MistManager.MistPD* ptr2 = ptr;
						int num = (int)x;
						int num2 = (int)(y - 0.125f);
						MistManager.MistPD mistPD = *this.getPtr(ptr2, num, num2);
						if (mistPD.level255 == 0 || (this.water_mng_bits & (1UL << (int)mistPD.id)) == 0UL)
						{
							text = snd_type;
						}
						else
						{
							bool flag;
							if (num2 > this.crop)
							{
								MistManager.MistPD mistPD2 = *this.getPtr(ptr2, num, num2 - 1);
								flag = mistPD.id != mistPD2.id || mistPD2.level255 == 0;
							}
							else
							{
								flag = true;
							}
							text = this.OWMng[this.AGen[(int)mistPD.id].K.type].checkWaterFootSound(x, y, num, num2, ref snd_type, flag);
						}
					}
				}
				finally
				{
					MistManager.MistPD* ptr = null;
				}
			}
			catch
			{
				text = snd_type;
			}
			return text;
		}

		public M2DropObject addWaterPuddleEffect(M2Mover Mv, int gen_id)
		{
			if (this.FD_drawWaterReleaseOne == null)
			{
				this.FD_drawWaterReleaseOne = new M2DropObject.FnDropObjectDraw(this.drawWaterReleaseOne);
			}
			M2DropObject m2DropObject = this.Mp.DropCon.Add(this.FD_drawWaterReleaseOne, Mv.x + (X.XORSP() - 0.5f) * 2f * 0.85f * Mv.sizex, Mv.y + X.XORSP() * 0.85f * Mv.sizey, 0f, X.XORSP() * 0.06f, 0f, (float)gen_id);
			m2DropObject.type = DROP_TYPE.WATER_BOUNCE;
			m2DropObject.gravity_scale = 0.125f;
			m2DropObject.bounce_y_reduce = 0f;
			return m2DropObject;
		}

		private bool drawWaterReleaseOne(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.AGen == null || !X.BTW(0f, (float)((int)Dro.time), (float)this.AGen.Count))
			{
				return false;
			}
			MistManager.MistGenerator mistGenerator = this.AGen[(int)Dro.time];
			MistManager.MISTTYPE type = mistGenerator.K.type;
			if (Dro.af_ground > 0f)
			{
				if (!CCON.isWater(this.Mp.getConfig((int)Dro.x, (int)(Dro.y + 0.25f))))
				{
					return false;
				}
				uint ran = X.GETRAN2(Dro.index * 27, Dro.index % 9);
				Ef.y += X.NI(0.125f, 0.25f, X.RAN(ran, 1634));
				return EffectItemNel.draw_rainfoot_puddle(Ef, MTRX.cola.White().blend(mistGenerator.K.color0, X.RAN(ran, 998)).C, BLEND.ADD, (float)((X.RAN(ran, 1716) < 0.3f) ? 1 : 2), Dro.af_ground, Ef.f0, Dro.index, true);
			}
			else
			{
				if (CCON.isWater(this.Mp.getConfig((int)Dro.x, (int)(Dro.y - 0.01f))))
				{
					return false;
				}
				uint ran2 = X.GETRAN2(Dro.index * 27, Dro.index % 9);
				MeshDrawer mesh = Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
				mesh.Col = mesh.ColGrd.Set(C32.c2d(mistGenerator.K.color0)).blend(MTRX.ColWhite, X.RAN(ran2, 2395)).C;
				float num = X.NI(1, 4, X.ZPOW(Dro.af, 24f));
				mesh.RectBL(-2f, -num, 2f, num, false);
				return true;
			}
		}

		public DRect getApplyRect()
		{
			return this.Rc;
		}

		public bool isWholeWater()
		{
			return this.mistmanager_whole > 0U && ((ulong)this.mistmanager_whole & this.water_mng_bits) > 0UL;
		}

		public unsafe bool isFire(int x, int y)
		{
			if (!this.isActive() || this.AGen == null || !X.BTW((float)this.crop, (float)x, (float)(this.Mp.clms - this.crop)) || !X.BTW((float)this.crop, (float)y, (float)(this.Mp.rows - this.crop)))
			{
				return false;
			}
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				MistManager.MistPD mistPD = *this.getPtr(ptr2, x, y);
				return mistPD.level255 != 0 && this.AGen[(int)mistPD.id].K.type == MistManager.MISTTYPE.LAVA;
			}
		}

		public DRect getWaterExistRect(bool check_static = true)
		{
			this.RcBufS.Set(this.Rc);
			if (!this.RcBufS.isEmpty())
			{
				this.RcBufS.x += 1f;
				DRect rcBufS = this.RcBufS;
				float y = rcBufS.y;
				rcBufS.y = y + 1f;
				this.RcBufS.width -= 2f;
				this.RcBufS.height -= 2f;
			}
			if (check_static)
			{
				this.ExpandWaterStaticArea(this.RcBufS);
			}
			X.rectMultiply(this.RcBufS, (float)this.crop, (float)this.crop, (float)(this.Mp.clms - this.crop * 2), (float)(this.Mp.rows - this.crop * 2));
			return this.RcBufS;
		}

		private DRect ExpandWaterStaticArea(DRect _Rc)
		{
			if (this.AWaterStatic == null)
			{
				return _Rc;
			}
			for (int i = this.AWaterStatic.Count - 1; i >= 0; i--)
			{
				MistManager.MistStaticArea mistStaticArea = this.AWaterStatic[i];
				_Rc.Expand((float)mistStaticArea.x, (float)mistStaticArea.reachable_y, (float)mistStaticArea.w, (float)(mistStaticArea.bottom - mistStaticArea.reachable_y), false);
			}
			return _Rc;
		}

		public M2WaterWatcher getWaterManager(MistManager.MISTTYPE p)
		{
			return X.Get<MistManager.MISTTYPE, M2WaterWatcher>(this.OWMng, p);
		}

		private void drawWater(int fcnt)
		{
			this.MdWater.chooseSubMesh(0, false, false);
			this.MdWater.revertVerAndTriIndex(this.water_mesh_v, this.water_mesh_t, false);
			this.MdWater.chooseSubMesh(1, false, true);
			this.MdWater.chooseSubMesh(2, false, true);
			this.MdWater.ColGrd.Set(MTRX.ColTrnsp);
			this.MdWater.Identity();
			foreach (KeyValuePair<MistManager.MISTTYPE, M2WaterWatcher> keyValuePair in this.OWMng)
			{
				keyValuePair.Value.drawWater(this.MdWater, fcnt);
			}
			this.Mp.addUpdateMesh(2048, false);
			this.MdWater.chooseSubMesh(0, false, false);
		}

		public void activateSurfaceReflectDrawer(float drawt, MistManager.MISTTYPE misttype, Color32 SurfaceColor, float surface_alpha, float surface_raise_pixel)
		{
			bool flag = false;
			if (this.SurfaceDraw == null)
			{
				float num = this.Mp.map2meshy(this.Mp.M2D.Cam.lt_mapy) - this.Mp.M2D.Cam.get_h();
				if (drawt > num)
				{
					M2UnstabilizeMapItem unstabilizeDrawerContainer = this.Mp.getUnstabilizeDrawerContainer();
					if (unstabilizeDrawerContainer != null)
					{
						flag = true;
						this.SurfaceDraw = new CameraRenderBinderWaterSurface(this.Mp, misttype, SurfaceColor, new MeshDrawer(null, 4, 6), delegate
						{
							if (this.Mp != M2DBase.Instance.curMap || this.water_mng_bits == 0UL)
							{
								this.removeSurfaceReflectDrawer();
								return false;
							}
							return true;
						});
						unstabilizeDrawerContainer.Add("water_surface", this.SurfaceDraw, this.SurfaceDraw.layer, null);
					}
				}
			}
			if (this.SurfaceDraw != null)
			{
				if (flag || this.Mp.floort < 30f)
				{
					this.SurfaceDraw.drawt = drawt;
				}
				else
				{
					this.SurfaceDraw.drawt = X.VALWALK(this.SurfaceDraw.drawt, drawt, surface_raise_pixel);
				}
				this.SurfaceDraw.alpha = surface_alpha;
			}
		}

		public void removeSurfaceReflectDrawer()
		{
			if (this.SurfaceDraw == null)
			{
				return;
			}
			M2UnstabilizeMapItem unstabilizeDrawerContainer = this.Mp.getUnstabilizeDrawerContainer();
			if (unstabilizeDrawerContainer != null)
			{
				unstabilizeDrawerContainer.Remove("water_surface");
			}
			this.SurfaceDraw = null;
		}

		private unsafe void redrawDebug(MeshDrawer Md, MeshDrawer MdC)
		{
			float w = this.Mp.M2D.Cam.get_w();
			float h = this.Mp.M2D.Cam.get_h();
			int num = X.Mx(this.crop, (int)this.Mp.M2D.Cam.lt_mapx);
			int num2 = X.Mx(this.crop, (int)this.Mp.M2D.Cam.lt_mapy);
			int num3 = X.Mn(this.Mp.clms - this.crop - 1, X.IntC(this.Mp.M2D.Cam.lt_mapx + w / this.Mp.CLEN) + 1);
			int num4 = X.Mn(this.Mp.rows - this.crop - 1, X.IntC(this.Mp.M2D.Cam.lt_mapy + h / this.Mp.CLEN) + 1);
			float num5 = this.Mp.map2meshx((float)num);
			float num6 = this.Mp.map2meshy((float)num2);
			Md.Col = MTRX.ColWhite;
			if (this.gen_id_max > 0)
			{
				MODIF modifier = KEY.getModifier(null);
				bool flag = (modifier & MODIF.SHIFT) > MODIF.NONE;
				BMListChars chrM = MTRX.ChrM;
				fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
				{
					MistManager.MistPD* ptr2 = ptr;
					num6 = this.Mp.map2meshy((float)num2);
					num5 = this.Mp.map2meshx((float)num);
					float clen = this.Mp.CLEN;
					float num7 = clen / 2f;
					for (int i = num2; i <= num4; i++)
					{
						MistManager.MistPD* ptr3 = this.getPtr(ptr2, num, i);
						float num8 = num5;
						for (int j = num; j <= num3; j++)
						{
							float num9 = (float)ptr3->level255;
							bool flag2 = false;
							if (num9 > 0f)
							{
								num9 /= 256f;
								int id = (int)ptr3->id;
								if (X.BTW(0f, (float)id, (float)this.gen_id_max))
								{
									MistManager.MistKind k = this.AGen[(int)ptr3->id].K;
									flag2 = k.max_behind_influence >= 0;
									Md.Col = Md.ColGrd.Set(ptr3->useable_step ? k.color0 : k.color1).setA1(X.NI(0.05f, 0.25f, num9) * ((ptr3->level255 >= k.apply_level255) ? 2.5f : 1f)).C;
									MdC.Col = Md.ColGrd.Set(4287943935U).blend(4290700799U, num9).C;
									Md.RectBL(num8 + 1f, num6 - clen, clen - 1f, clen - 1f, false);
									if (flag && k.mist_aim != MistManager.MISTAIM.NO_AIM)
									{
										MistManager.AimCk aim_check = ptr3->aim_check;
										if (aim_check != MistManager.AimCk.NONE)
										{
											for (int l = 0; l < 4; l++)
											{
												bool flag3 = (aim_check & (MistManager.AimCk)(16 << l)) > MistManager.AimCk.NONE;
												bool flag4 = (aim_check & (MistManager.AimCk)(1 << l)) > MistManager.AimCk.NONE;
												bool flag5 = (aim_check & (MistManager.AimCk)(65536 << l)) > MistManager.AimCk.NONE;
												if (flag3)
												{
													Md.Rotate(-CAim.get_agR((AIM)k.mist_aim, 0f), false);
													if (l == 1)
													{
														Md.Rotate(-1.5707964f, false);
													}
													else if (l == 2)
													{
														Md.Rotate(1.5707964f, false);
													}
													else if (l == 3)
													{
														Md.Rotate(3.1415927f, false);
													}
													Md.Scale(1f, -1f, false).TranslateP(num8 + clen * 0.5f, num6 - clen * 0.5f, false);
													if (flag5)
													{
														Md.Col = C32.d2c(4294162943U);
														Md.Arrow(num7 - 4f, 0f, 4f, 3.1415927f, 1f, false);
													}
													else if (flag4)
													{
														Md.Col = C32.d2c(4281681658U);
														Md.Arrow(num7 - 4f, 0f, 4f, 0f, 1f, false);
													}
													else
													{
														Md.Col = C32.d2c(4282269247U);
														Md.Line(num7 - 3f - 2f, 2f, num7 - 3f + 2f, -2f, 2f, false, 0f, 0f);
														Md.Line(num7 - 3f + 2f, 2f, num7 - 3f - 2f, -2f, 2f, false, 0f, 0f);
													}
												}
												Md.Identity();
											}
											if (k.mist_aim == MistManager.MISTAIM.B)
											{
												if ((aim_check & MistManager.AimCk.ALLOW_GO_BEHIND) != MistManager.AimCk.NONE)
												{
													Md.Col = C32.d2c(4294919750U);
													Md.Line(num8, num6 + 1f - clen, num8 + clen, num6 + 1f - clen, 1f, false, 0f, 0f);
												}
												if ((aim_check & MistManager.AimCk.ALLOW_GO_R) != MistManager.AimCk.NONE)
												{
													Md.Col = C32.d2c(4294919750U);
													Md.Line(num8 + 1f, num6, num8 + 1f, num6 - clen, 1f, false, 0f, 0f);
												}
												if ((aim_check & MistManager.AimCk.ALLOW_GO_L) != MistManager.AimCk.NONE)
												{
													Md.Col = C32.d2c(4294919750U);
													Md.Line(num8 + clen - 1f, num6, num8 + clen - 1f, num6 - clen, 1f, false, 0f, 0f);
												}
												if ((aim_check & MistManager.AimCk.ALLOW_GO_INFLUENCE) != MistManager.AimCk.NONE)
												{
													Md.Col = C32.d2c(4294483975U);
													Md.Line(num8, num6 - 1f, num8 + clen, num6 - 1f, 1f, false, 0f, 0f);
													Md.Col = MTRX.ColWhite;
													Md.LineDashed(num8, num6 - 1f, num8 + clen, num6 - 1f, 0f, 7, 1f, false, 0.5f);
												}
											}
										}
									}
								}
							}
							else
							{
								MdC.Col = C32.d2c(4287795858U);
							}
							num9 = (float)((int)((float)ptr3->level255 * 100f / 255f));
							STB stb = TX.PopBld(null, 0);
							stb.Add(flag2 ? ((int)ptr3->step_behind) : ptr3->step);
							stb.Add("l", num9.ToString());
							chrM.DrawScaleStringTo(MdC, stb, num8 + clen - 3f, num6 - clen + 9f, 0.5f, 0.5f, ALIGN.RIGHT, ALIGNY.BOTTOM, false, 0f, 0f, null);
							if (num9 > 0f)
							{
								int num10 = (int)(((float)ptr3->reduce_lock - this.Mp.floort) / 5f);
								if (num10 > 0)
								{
									MdC.Col = C32.d2c(4294966142U);
									chrM.DrawScaleStringTo(MdC, stb.Set(num10), num8 + clen - 3f, num6 - clen + 19f, 0.5f, 0.5f, ALIGN.RIGHT, ALIGNY.BOTTOM, false, 0f, 0f, null);
								}
							}
							TX.ReleaseBld(stb);
							num8 += this.Mp.CLEN;
							ptr3++;
						}
						num6 -= this.Mp.CLEN;
					}
				}
				if ((modifier & MODIF.CTRL) != MODIF.NONE)
				{
					foreach (KeyValuePair<MistManager.MISTTYPE, M2WaterWatcher> keyValuePair in this.OWMng)
					{
						keyValuePair.Value.drawDebug(Md);
					}
				}
			}
		}

		public bool isFirstProgressNotStabilized()
		{
			return this.Rvfirst_progress.isPreFrameInputted();
		}

		public bool isFirstProgressRunning()
		{
			return this.Rvfirst_progress > 0f;
		}

		public void makeSnapShot(PuzzSnapShot.RevertItem Rvi)
		{
			Rvi.time = -1;
			if (this.AGen == null)
			{
				return;
			}
			if (this.AMemory == null)
			{
				this.AMemory = new List<ByteArray>(1);
			}
			Rvi.time = this.AMemory.Count;
			ByteArray byteArray = new ByteArray(0U);
			this.writeBinaryTo(byteArray);
			this.AMemory.Add(byteArray);
		}

		public void puzzleRevert(PuzzSnapShot.RevertItem Rvi)
		{
			if (this.AMemory == null || Rvi.time < 0 || Rvi.time >= this.AMemory.Count)
			{
				return;
			}
			ByteArray byteArray = this.AMemory[Rvi.time];
			byteArray.position = 0UL;
			this.readBinaryFrom(byteArray);
		}

		public unsafe void readBinaryFrom(ByteReader Ba)
		{
			this.clearWithoutGenerator();
			this.Rc.Set((float)Ba.readUShort(), (float)Ba.readUShort(), (float)Ba.readUShort(), (float)Ba.readUShort());
			int num = (int)X.Mn(this.Rc.x + this.Rc.w, (float)(this.Mp.clms - this.crop));
			int num2 = (int)X.Mn(this.Rc.y + this.Rc.h, (float)(this.Mp.rows - this.crop));
			int num3 = (int)X.Mx(this.Rc.x, (float)this.crop);
			int num4 = (int)X.Mx(this.Rc.y, (float)this.crop);
			this.reconsider_flag = 1;
			this.RcReconsider.Expand(this.Rc.x, this.Rc.y, this.Rc.width, this.Rc.height, false);
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				MistManager.MistPD* ptr3 = null;
				for (int i = num4; i < num2; i++)
				{
					MistManager.MistPD* ptr4 = this.getPtr(ptr2, num3, i);
					for (int j = num3; j < num; j++)
					{
						ptr4->readBinaryFrom(Ba);
						ptr4++;
					}
				}
			}
		}

		public unsafe void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeUShort((ushort)this.Rc.x);
			Ba.writeUShort((ushort)this.Rc.y);
			Ba.writeUShort((ushort)this.Rc.width);
			Ba.writeUShort((ushort)this.Rc.height);
			int num = (int)X.Mn(this.Rc.x + this.Rc.w, (float)(this.Mp.clms - this.crop));
			int num2 = (int)X.Mn(this.Rc.y + this.Rc.h, (float)(this.Mp.rows - this.crop));
			int num3 = (int)X.Mx(this.Rc.x, (float)this.crop);
			int num4 = (int)X.Mx(this.Rc.y, (float)this.crop);
			fixed (MistManager.MistPD* ptr = &this.AAPd[0, 0])
			{
				MistManager.MistPD* ptr2 = ptr;
				MistManager.MistPD* ptr3 = null;
				for (int i = num4; i < num2; i++)
				{
					MistManager.MistPD* ptr4 = this.getPtr(ptr2, num3, i);
					for (int j = num3; j < num; j++)
					{
						ptr4->writeBinaryTo(Ba);
						ptr4++;
					}
				}
			}
		}

		public IrisOutManager.IRISOUT_TYPE getIrisOutKey()
		{
			return IrisOutManager.IRISOUT_TYPE.LAVA;
		}

		public bool initSubmitIrisOut(PR Pr, bool execute, ref bool no_iris_out)
		{
			return true;
		}

		public bool warpInitIrisOut(PR Pr, ref PR.STATE changestate, ref string change_pose)
		{
			Pr.Ser.Cure(SER.BURNED);
			changestate = PR.STATE.WATER_CHOKED_RELEASE;
			change_pose = "water_choked_release2";
			Pr.NM2D.Iris.deassignListener(this);
			return true;
		}

		public DRect Rc = new DRect("main", 0f, 0f, 0f, 0f, 0f);

		public DRect RcBuf = new DRect("buf", 0f, 0f, 0f, 0f, 0f);

		public DRect RcBufS = new DRect("bufS", 0f, 0f, 0f, 0f, 0f);

		public DRect RcReconsider = new DRect("reconsider", 0f, 0f, 0f, 0f, 0f);

		public const string generate_chip_key = "GENERATE_MIST";

		public const string generate_chip_base_key = "GENERATE_MIST_BASE";

		private byte gen_id_max;

		public int crop;

		public const int DAMAGE_TIMEOUT_MAXT = 30;

		public const int FINE_INTV = 5;

		public readonly Map2d Mp;

		private ulong appear_gen_bits;

		private int dmg_atk_max;

		private float running_delay;

		private byte reconsider_flag;

		private ulong water_mng_bits;

		private ulong use_config_bits;

		private ulong water_static_bits;

		public int water_mesh_v = -1;

		public int water_mesh_t = -1;

		public MeshDrawer MdWater;

		public bool set_water_particle = true;

		private BDic<MistManager.MISTTYPE, M2WaterWatcher> OWMng;

		private MistManager.MistPD[,] AAPd;

		private List<MistManager.MistGenerator> AGen;

		private List<MistManager.MistDamageApply> ADmgAtk;

		private List<MistManager.MistStaticArea> AWaterStatic;

		private List<M2LpWaterSlicer> AWaterSlicer;

		public float water_applied_changed_frame = -40f;

		public byte water_applied_pr;

		public MeshDrawer MeshForDebug;

		public MeshDrawer MeshForDebugCharacter;

		public CameraRenderBinderWaterSurface SurfaceDraw;

		public uint mistmanager_whole;

		public byte whole_fill_config;

		public int static_id_max = -1;

		public byte draw_intv = 1;

		public bool fine_water_slicer;

		private const int WIND_THRESH_ADD = -50;

		private const int WIND_INFL_ADD = 30;

		private RevCounter Rvfirst_progress = new RevCounter();

		public Flagger FlgHideSurface;

		public List<ByteArray> AMemory;

		private M2DropObject.FnDropObjectDraw FD_drawWaterReleaseOne;

		public enum MISTTYPE
		{
			NONE,
			POISON,
			MAGIC_JAMMING,
			WATER,
			SWAMP,
			LAVA,
			SEA
		}

		public enum MISTAIM : byte
		{
			NO_AIM = 8,
			L = 0,
			T,
			R,
			B
		}

		public enum AimCk
		{
			NONE,
			GO_ST,
			GO_L,
			GO_R = 4,
			GO_BH = 8,
			CHECK_ST = 16,
			CHECK_L = 32,
			CHECK_R = 64,
			CHECK_BH = 128,
			CONNECT_AL = 256,
			CONNECT_AT = 512,
			CONNECT_AR = 1024,
			CONNECT_AB = 2048,
			CFG_CHECKED = 4096,
			CFG_PASSABLE = 8192,
			CFG_PASSABLE_FALL = 16384,
			FROM_ST = 65536,
			FROM_L = 131072,
			FROM_R = 262144,
			FROM_BH = 524288,
			ALLOW_GO_INFLUENCE = 1048576,
			ALLOW_GO_L = 2097152,
			ALLOW_GO_R = 4194304,
			ALLOW_GO_BEHIND = 8388608,
			BEHIND_HAS_ROOM = 16777216,
			WATER_STATIC = 33554432
		}

		public delegate int FnMistApply(MistManager.MistKind K, NelM2Attacker Atk, float level01);

		public sealed class MistKind
		{
			public MistKind(MistManager.MISTTYPE _type)
			{
				this.type = _type;
				switch (this.type)
				{
				case MistManager.MISTTYPE.POISON:
					this.color0 = C32.d2c(4286437565U);
					this.color1 = C32.d2c(4286236571U);
					this.tb_shift = -40;
					return;
				case MistManager.MISTTYPE.MAGIC_JAMMING:
					this.color0 = C32.d2c(4286881955U);
					this.color1 = C32.d2c(4284514711U);
					return;
				case MistManager.MISTTYPE.WATER:
					this.color0 = C32.d2c(4287873766U);
					this.color1 = C32.d2c(4286221758U);
					this.use_water_watcher = 1;
					this.influence_speed255 = 200;
					this.max_influence = 0;
					this.tb_shift = 45;
					this.influence_threshold255 = 50;
					this.mist_aim = MistManager.MISTAIM.B;
					this.reduce255 = 30;
					return;
				case MistManager.MISTTYPE.SWAMP:
					this.color0 = C32.d2c(4279900698U);
					this.color1 = C32.d2c(4281479734U);
					this.use_water_watcher = 2;
					this.influence_speed255 = 200;
					this.max_influence = 0;
					this.tb_shift = 45;
					this.influence_threshold255 = 50;
					this.mist_aim = MistManager.MISTAIM.B;
					this.reduce255 = 30;
					return;
				case MistManager.MISTTYPE.LAVA:
					this.color0 = C32.d2c(4294830676U);
					this.color1 = C32.d2c(4294912512U);
					this.use_water_watcher = 2;
					this.apply_level255 = 200;
					this.influence_speed255_lr = 200;
					this.influence_speed255_tb = 11;
					this.max_influence = 0;
					this.tb_shift = 45;
					this.influence_threshold255 = 50;
					this.mist_aim = MistManager.MISTAIM.B;
					this.reduce255 = 80;
					return;
				case MistManager.MISTTYPE.SEA:
					this.color0 = C32.d2c(4286978072U);
					this.color1 = C32.d2c(4293267771U);
					this.use_water_watcher = 1;
					this.influence_speed255 = 200;
					this.max_influence = 0;
					this.tb_shift = 45;
					this.influence_threshold255 = 50;
					this.mist_aim = MistManager.MISTAIM.B;
					this.reduce255 = 30;
					return;
				default:
					return;
				}
			}

			public MistKind(MistManager.MistKind Src)
			{
				this.type = Src.type;
				this.use_water_watcher = Src.use_water_watcher;
				this.Set(Src);
			}

			public MistManager.MistKind Set(MistManager.MistKind Src)
			{
				this.color0 = Src.color0;
				this.color1 = Src.color1;
				this.damage_cooltime = Src.damage_cooltime;
				this.max_influence = Src.max_influence;
				this.max_behind_influence = Src.max_behind_influence;
				this.max_directional = Src.max_directional;
				this.influence_threshold255 = Src.influence_threshold255;
				this.influence_speed255_lr = Src.influence_speed255_lr;
				this.influence_speed255_tb = Src.influence_speed255_tb;
				this.tb_shift = Src.tb_shift;
				this.apply_level255 = Src.apply_level255;
				this.damage_count_reduce = Src.damage_count_reduce;
				this.raise_alloc_lv = Src.raise_alloc_lv;
				this.water_bottom_raise = Src.water_bottom_raise;
				this.water_surface_top = Src.water_surface_top;
				this.surface_raise_pixel = Src.surface_raise_pixel;
				this.infinity_amount = Src.infinity_amount;
				this.apply_o2 = Src.apply_o2;
				this.adding_damage_count = Src.adding_damage_count;
				this.reduce255 = Src.reduce255;
				this.mist_aim = Src.mist_aim;
				this.fnApply = Src.fnApply;
				this.AAtk = ((Src.AAtk != null) ? X.concat<MistAttackInfo>(Src.AAtk, null, -1, -1) : null);
				return this;
			}

			public int influence_speed255
			{
				set
				{
					this.influence_speed255_tb = value;
					this.influence_speed255_lr = value;
				}
			}

			public Color32 surface_color
			{
				get
				{
					switch (this.type)
					{
					case MistManager.MISTTYPE.SWAMP:
						return C32.d2c(3141154871U);
					case MistManager.MISTTYPE.LAVA:
						return C32.d2c(4293503823U);
					case MistManager.MISTTYPE.SEA:
						return C32.d2c(4290613144U);
					default:
						return C32.d2c(4286545791U);
					}
				}
			}

			public bool isWater()
			{
				return this.use_water_watcher > 0;
			}

			public bool isWaterSwamp()
			{
				return (this.use_water_watcher & 2) > 0;
			}

			public int calcApplyCell(float hosei = 1.4f)
			{
				int num = ((this.max_influence < 0) ? 10 : this.max_influence);
				return (int)((float)(2 * num * (num + 1)) * hosei) + 1;
			}

			public int calcAmount(int apply_time, float hosei = 1.4f)
			{
				int num = apply_time / 5;
				return this.calcApplyCell(hosei) * (this.apply_level255 + num * this.reduce255);
			}

			public bool applyDamageDefault(MistManager.MistKind K, NelM2Attacker Atk, float level01)
			{
				int num = 0;
				if (Atk is PR)
				{
					num = (Atk as PR).applyGasDamage(K, level01);
				}
				else if (this.fnApply != null)
				{
					num = this.fnApply(K, Atk, level01);
				}
				if ((num == -1 || num == 1) && this.AAtk != null)
				{
					Atk.applyGasDamage(K, this.AAtk[X.xors(this.AAtk.Length)]);
				}
				return num > 0;
			}

			public unsafe bool canInfluenceTo(MistManager.MistPD* P, MistManager.MistPD* Pd, MistManager.MistGenerator G, MistManager MIST, int sx, int sy, int dx, int dy)
			{
				Map2d mp = MIST.Mp;
				int num = this.influence_threshold255;
				if (this.tb_shift > 0 && sx == dx && sy < dy)
				{
					num /= 2;
				}
				if (P->wind != 32639)
				{
					int num2;
					if (sx == dx && (num2 = P->wind_y) != 0)
					{
						num += -50 * X.MPF(sy < dy == 0 < num2);
					}
					if (sy == dy && (num2 = P->wind_x) != 0)
					{
						num += -50 * X.MPF(sx < dx == 0 < num2);
					}
					num = X.MMX(1, num, 254);
				}
				if (P->level_transable <= num)
				{
					return false;
				}
				if (!Pd->canStand(mp, P, sx, sy, dx, dy))
				{
					if (this.mist_aim != MistManager.MISTAIM.NO_AIM)
					{
						P->aimTouch(this, sx, sy, dx, dy, 0, null);
					}
					return false;
				}
				if (this.mist_aim != MistManager.MISTAIM.NO_AIM)
				{
					if (P->aimAlreadyChecked())
					{
						return false;
					}
					int num3 = CAim.get_dif_tetra((AIM)this.mist_aim, CAim.get_aim_tetra(sx, -sy, dx, -dy));
					if (num3 != 0)
					{
						if (num3 == 4)
						{
							if ((P->aim_check & MistManager.AimCk.CHECK_BH) != MistManager.AimCk.NONE)
							{
								return false;
							}
							bool flag = (P->aim_check & MistManager.AimCk.ALLOW_GO_BEHIND) > MistManager.AimCk.NONE;
							if (this.max_behind_influence >= 0 && (int)P->step_behind >= this.max_behind_influence)
							{
								return false;
							}
							if (this.max_directional < 0 || P->step < this.max_directional)
							{
								if (G.K.isWater())
								{
									if (!P->aimAlreadyCheckedSLR() || !flag)
									{
										return false;
									}
								}
								else if (!P->aimCanGoBehind())
								{
									return false;
								}
							}
						}
						else
						{
							MistManager.AimCk aimCk = ((num3 == -2) ? MistManager.AimCk.ALLOW_GO_L : MistManager.AimCk.ALLOW_GO_R);
							if ((P->aim_check & ((num3 == -2) ? MistManager.AimCk.CHECK_L : MistManager.AimCk.CHECK_R)) != MistManager.AimCk.NONE)
							{
								return false;
							}
							bool flag2 = (P->aim_check & aimCk) > MistManager.AimCk.NONE;
							if (flag2)
							{
								P->aim_check = P->aim_check & ~aimCk;
							}
							else if ((P->aim_check & MistManager.AimCk.ALLOW_GO_INFLUENCE) != MistManager.AimCk.NONE)
							{
								flag2 = true;
							}
							if (this.isWater() && sy == dy)
							{
								if (!this.water_bottom_raise && sy >= mp.rows - MIST.crop - 1)
								{
									return false;
								}
								M2WaterWatcher waterManager = MIST.getWaterManager(this.type);
								if (waterManager == null)
								{
									return false;
								}
								M2WaterWatcher.WLine lineAt = waterManager.getLineAt(dx, dy + 1, false);
								if (lineAt != null && (lineAt.water_fill || lineAt.water_fill_influence))
								{
									return false;
								}
								MistManager.MistPD mistPD = P[mp.clms - MIST.crop * 2];
								if (mistPD.level255 > 0 && (mistPD.aim_check & (MistManager.AimCk)96) != (MistManager.AimCk)96)
								{
									return false;
								}
							}
							if (this.max_directional < 0 || P->step < this.max_directional)
							{
								if (G.K.isWater() && flag2)
								{
									if (!P->aimAlreadyCheckedS())
									{
										return false;
									}
								}
								else if (!P->aimCanGoLR())
								{
									return false;
								}
							}
						}
					}
					if (P->aimTouch(num3, 1) == -4)
					{
						return false;
					}
				}
				else if (G.K.max_influence >= 0 && P->step >= G.K.max_influence)
				{
					return false;
				}
				return true;
			}

			public readonly MistManager.MISTTYPE type;

			private readonly byte use_water_watcher;

			public Color32 color0;

			public Color32 color1;

			public int damage_cooltime = 70;

			public int max_influence = 3;

			public int max_behind_influence = -1;

			public int max_directional = -1;

			public int influence_threshold255 = 85;

			public int influence_speed255_lr = 22;

			public int influence_speed255_tb = 22;

			public int tb_shift;

			public int apply_level255 = 150;

			public int damage_count_reduce;

			public float raise_alloc_lv = 1f;

			public bool water_bottom_raise;

			public int water_surface_top = -1;

			public float surface_raise_pixel = 4.5f;

			public bool infinity_amount;

			public int apply_o2;

			public int adding_damage_count;

			public int reduce255 = 20;

			public MistManager.MISTAIM mist_aim = MistManager.MISTAIM.NO_AIM;

			public MistManager.FnMistApply fnApply;

			public MistAttackInfo[] AAtk;

			public const int mist_apply_atk_lock_intv = 22;

			public MistDrawer Msd;
		}

		public sealed class MistGenerator
		{
			public MistManager.MistKind K;

			public int amount;

			public int useable_count;

			public bool infinity;

			public M2Chip BaseChip;
		}

		public sealed class MistDamageApply
		{
			public MistManager.MistKind K;

			public NelM2Attacker Atk;

			public bool fall_flag;

			public int delay;

			public int gen_id;

			public int timeout;

			public CREATION crt = CREATION.CREATED;

			public int dmg_count;

			public float af;
		}

		public sealed class MistStaticArea
		{
			public MistStaticArea(string _name, MistManager.MistKind _K, int _x, int _y, int _w, int _h, int _config)
			{
				this.name = _name;
				this.K = _K;
				this.x = _x;
				this.reachable_y = _y;
				this.y = _y;
				this.w = _w;
				this.h = _h;
				this.config = _config;
			}

			public int right
			{
				get
				{
					return this.x + this.w;
				}
			}

			public int bottom
			{
				get
				{
					return this.y + this.h;
				}
			}

			public override string ToString()
			{
				return string.Concat(new string[]
				{
					"[",
					this.name,
					"] ",
					this.x.ToString(),
					", ",
					this.y.ToString(),
					", ",
					this.w.ToString(),
					", ",
					this.h.ToString()
				});
			}

			public string name;

			public MistManager.MistKind K;

			public int x;

			public int y;

			public int w;

			public int h;

			public int reachable_y;

			public int config;
		}

		public struct MistPD
		{
			public MistPD(byte _id, int _level)
			{
				this.id = _id;
				this.step = 0;
				this.step_behind_ = 0;
				this.reduce_lock = 0;
				this.reduce_lock_assigned = 0;
				this.angle_var = 0;
				this.aim_check = MistManager.AimCk.BEHIND_HAS_ROOM;
				this.level_translated = 0;
				this.level255 = _level;
				this.wind = 32639;
				this.useable_step = true;
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writeInt(this.level255);
				if (this.level255 > 0)
				{
					Ba.writeByte((int)this.id);
					Ba.writeInt(this.step);
					Ba.writeByte((int)this.step_behind_);
					Ba.writeByte((int)this.angle_var);
					Ba.writeInt(this.reduce_lock);
					Ba.writeInt(this.reduce_lock_assigned);
					Ba.writeInt(this.level_translated);
					Ba.writeInt(this.wind);
					Ba.writeInt((int)this.aim_check);
				}
			}

			public void readBinaryFrom(ByteReader Ba)
			{
				this.level255 = Ba.readInt();
				if (this.level255 > 0)
				{
					this.id = (byte)Ba.readByte();
					this.step = Ba.readInt();
					this.step_behind_ = (byte)Ba.readByte();
					this.angle_var = (byte)Ba.readByte();
					this.reduce_lock = Ba.readInt();
					this.reduce_lock_assigned = Ba.readInt();
					this.level_translated = Ba.readInt();
					this.wind = Ba.readInt();
					this.aim_check = (MistManager.AimCk)Ba.readInt();
				}
			}

			public byte step_behind
			{
				get
				{
					return this.step_behind_;
				}
			}

			public int wind_x
			{
				get
				{
					return ((this.wind & 65280) >> 8) - 127;
				}
				set
				{
					this.wind = (this.wind & 255) | (X.MMX(0, value + 127, 255) << 8);
				}
			}

			public int wind_y
			{
				get
				{
					return (this.wind & 255) - 127;
				}
				set
				{
					this.wind = (this.wind & 65280) | X.MMX(0, value + 127, 255);
				}
			}

			public int weekenWind(float val)
			{
				return ((int)((float)this.wind_x * val) + 127 << 8) | ((int)((float)this.wind_y * val) + 127);
			}

			public void setStepBehind(int val, MistManager.MistKind K, bool force = false)
			{
				if (!force && (val == (int)this.step_behind_ || this.level255 == 0))
				{
					return;
				}
				this.step_behind_ = (byte)((val > 255) ? 255 : val);
				this.fineRoomFlag(K);
			}

			public void setStepBehind(int val, MistManager MIST)
			{
				if (val == (int)this.step_behind_ || this.level255 == 0)
				{
					return;
				}
				this.step_behind_ = (byte)((val > 255) ? 255 : val);
				this.fineRoomFlag(MIST.AGen[(int)this.id].K);
			}

			public int level_transable
			{
				get
				{
					if (this.level_translated >= this.level255)
					{
						return 0;
					}
					if (this.level_translated <= 0)
					{
						return this.level255;
					}
					return this.level255 - this.level_translated;
				}
			}

			public void levelAdd(int l)
			{
				l = X.Mn(255 - this.level255, l);
				this.level255 += l;
				if (l > 0)
				{
					this.level_translated += l;
				}
			}

			public unsafe static void transfer(MistManager.MistPD* Src, MistManager.MistPD* Dest, int val)
			{
				if (val < 0)
				{
					MistManager.MistPD.transfer(Dest, Src, -val);
					return;
				}
				val = X.Mn(Src->level255, val);
				Src->levelAdd(-val);
				val = X.Mn(255 - Dest->level255, val);
				Dest->levelAdd(val);
			}

			public MistManager.MistPD fineAngle(int sx, int sy, int dx, int dy)
			{
				if ((this.angle_var & 16) == 0 && sx != dx)
				{
					this.angle_var |= (byte)(16 | ((sx > dx) ? 2 : 4));
				}
				if ((this.angle_var & 128) == 0 && sy != dy)
				{
					this.angle_var |= (byte)(128 | ((sy > dy) ? 32 : 64));
				}
				return this;
			}

			public bool useable_step
			{
				get
				{
					return (this.angle_var & 1) > 0;
				}
				set
				{
					if ((this.angle_var & 1) != 0 && !value)
					{
						this.angle_var -= 1;
					}
					if ((this.angle_var & 1) == 0 && value)
					{
						this.angle_var += 1;
					}
				}
			}

			public AIM aim
			{
				get
				{
					return CAim.get_aim2(0f, 0f, (float)(((this.angle_var & 18) == 18) ? (-1) : (((this.angle_var & 20) == 20) ? 1 : 0)), (float)(((this.angle_var & 160) == 160) ? 1 : (((this.angle_var & 192) == 192) ? (-1) : 0)), false);
				}
			}

			public void setStaticWater(MistManager.MistKind K, int behind_step_room, bool flag)
			{
				if (flag)
				{
					this.aim_check &= (MistManager.AimCk)28672;
					this.level255 = 255;
					this.useable_step = true;
					this.step_behind_ = (byte)X.MMX(0, K.max_behind_influence - behind_step_room, 255);
					this.fineRoomFlag(K);
					this.aimTouch(0, 0);
					this.aimTouch(2, 1);
					this.aimTouch(-2, 1);
					this.step = 0;
					this.aim_check |= MistManager.AimCk.WATER_STATIC;
					this.fineRoomFlagStatic();
					return;
				}
				this.aim_check &= (MistManager.AimCk)(-33554433);
				this.useable_step = false;
				this.step = K.max_influence + 1;
			}

			public void fineRoomFlagStatic()
			{
				if ((this.aim_check & MistManager.AimCk.BEHIND_HAS_ROOM) > MistManager.AimCk.NONE)
				{
					this.aimTouch(4, 1);
					this.aim_check |= MistManager.AimCk.FROM_BH;
					this.aim_check &= (MistManager.AimCk)(-65537);
					return;
				}
				this.aim_check |= MistManager.AimCk.FROM_ST;
				this.aim_check &= (MistManager.AimCk)(-524289);
			}

			public int aimTouch(MistManager.MistKind K, int sx, int sy, int dx, int dy, int go_ahead = 0, MistManager ResetMist = null)
			{
				if (ResetMist != null)
				{
					this.aim_check &= (MistManager.AimCk)28672;
				}
				if (K.mist_aim == MistManager.MISTAIM.NO_AIM)
				{
					return -3;
				}
				if (this.aimAlreadyChecked())
				{
					return -4;
				}
				int num = CAim.get_dif_tetra((AIM)K.mist_aim, CAim.get_aim_tetra(sx, -sy, dx, -dy));
				if (ResetMist != null)
				{
					switch (num)
					{
					case -2:
						this.aim_check |= MistManager.AimCk.FROM_L;
						break;
					case 0:
						this.aim_check |= MistManager.AimCk.FROM_ST;
						if (this.step_behind_ < 255)
						{
							this.step_behind_ += 1;
						}
						break;
					case 2:
						this.aim_check |= MistManager.AimCk.FROM_R;
						break;
					case 4:
						this.aim_check |= MistManager.AimCk.FROM_BH;
						if (this.step_behind_ > 0)
						{
							this.step_behind_ -= 1;
						}
						break;
					}
					this.fineRoomFlag(K);
					if (K.water_bottom_raise && K.mist_aim == MistManager.MISTAIM.B && sy == ResetMist.Mp.rows - ResetMist.crop - 1)
					{
						this.aimTouch(0, 0);
					}
				}
				return this.aimTouch(num, go_ahead);
			}

			public void fineRoomFlag(MistManager.MistKind K)
			{
				if (K.max_behind_influence < 0 || (int)this.step_behind < K.max_behind_influence)
				{
					this.aim_check |= MistManager.AimCk.BEHIND_HAS_ROOM;
					return;
				}
				this.aim_check &= (MistManager.AimCk)(-16777217);
			}

			public int aimTouch(int aim_dif, int go_ahead = 0)
			{
				switch (aim_dif)
				{
				case -2:
					if ((this.aim_check & MistManager.AimCk.CHECK_L) == MistManager.AimCk.NONE)
					{
						this.aim_check |= MistManager.AimCk.CHECK_L | ((go_ahead != 0) ? MistManager.AimCk.GO_L : MistManager.AimCk.NONE);
						return aim_dif;
					}
					return -4;
				case 0:
					if ((this.aim_check & MistManager.AimCk.CHECK_ST) == MistManager.AimCk.NONE)
					{
						this.aim_check |= MistManager.AimCk.CHECK_ST | ((go_ahead != 0) ? MistManager.AimCk.GO_ST : MistManager.AimCk.NONE);
						return aim_dif;
					}
					return -4;
				case 2:
					if ((this.aim_check & MistManager.AimCk.CHECK_R) == MistManager.AimCk.NONE)
					{
						this.aim_check |= MistManager.AimCk.CHECK_R | ((go_ahead != 0) ? MistManager.AimCk.GO_R : MistManager.AimCk.NONE);
						return aim_dif;
					}
					return -4;
				}
				if ((this.aim_check & MistManager.AimCk.CHECK_BH) != MistManager.AimCk.NONE)
				{
					return -4;
				}
				this.aim_check |= MistManager.AimCk.CHECK_BH | ((go_ahead > 0) ? MistManager.AimCk.GO_BH : MistManager.AimCk.NONE);
				return aim_dif;
			}

			public bool cutSpecificAim(MistManager.MistKind K, AIM a, bool cut_fromdata)
			{
				if (K.mist_aim == MistManager.MISTAIM.NO_AIM)
				{
					return false;
				}
				int num = CAim.get_dif_tetra((AIM)K.mist_aim, a);
				MistManager.AimCk aimCk = this.aim_check;
				switch (num)
				{
				case -2:
					if (!cut_fromdata || (this.aim_check & MistManager.AimCk.FROM_L) == MistManager.AimCk.NONE)
					{
						this.aim_check &= (MistManager.AimCk)(-131107);
					}
					break;
				case 0:
					if (!cut_fromdata || (this.aim_check & MistManager.AimCk.FROM_ST) == MistManager.AimCk.NONE)
					{
						this.aim_check &= (MistManager.AimCk)(-65554);
					}
					break;
				case 2:
					if (!cut_fromdata || (this.aim_check & MistManager.AimCk.FROM_R) == MistManager.AimCk.NONE)
					{
						this.aim_check &= (MistManager.AimCk)(-262213);
					}
					break;
				case 4:
					if (!cut_fromdata || (this.aim_check & MistManager.AimCk.FROM_BH) == MistManager.AimCk.NONE)
					{
						this.aim_check &= (MistManager.AimCk)(-524425);
					}
					break;
				}
				return aimCk != this.aim_check;
			}

			public bool aimAlreadyChecked(MistManager.MistKind K, int sx, int sy, int dx, int dy)
			{
				if (K.mist_aim == MistManager.MISTAIM.NO_AIM)
				{
					return false;
				}
				switch (CAim.get_dif_tetra((AIM)K.mist_aim, CAim.get_aim_tetra(sx, -sy, dx, -dy)))
				{
				case -2:
					return (this.aim_check & MistManager.AimCk.CHECK_L) > MistManager.AimCk.NONE;
				case 0:
					return (this.aim_check & MistManager.AimCk.CHECK_ST) > MistManager.AimCk.NONE;
				case 2:
					return (this.aim_check & MistManager.AimCk.CHECK_R) > MistManager.AimCk.NONE;
				case 4:
					return (this.aim_check & MistManager.AimCk.CHECK_BH) > MistManager.AimCk.NONE;
				}
				return false;
			}

			public bool aimAlreadyChecked()
			{
				return (this.aim_check & (MistManager.AimCk)240) == (MistManager.AimCk)240;
			}

			public bool aimAlreadyCheckedSLR()
			{
				return (this.aim_check & (MistManager.AimCk)112) == (MistManager.AimCk)112;
			}

			public bool aimAlreadyCheckedS()
			{
				return (this.aim_check & MistManager.AimCk.CHECK_ST) == MistManager.AimCk.CHECK_ST;
			}

			public bool aimCanGoBehind()
			{
				return (this.aim_check & (MistManager.AimCk)119) == (MistManager.AimCk)112;
			}

			public bool aimCanGoLR()
			{
				return (this.aim_check & (MistManager.AimCk)17) == MistManager.AimCk.CHECK_ST;
			}

			public bool aimComeFromBehind()
			{
				return (this.aim_check & MistManager.AimCk.FROM_BH) > MistManager.AimCk.NONE;
			}

			public bool aimComeFromSt()
			{
				return (this.aim_check & MistManager.AimCk.FROM_ST) > MistManager.AimCk.NONE;
			}

			public bool aimGoesST()
			{
				return (this.aim_check & (MistManager.AimCk)17) == (MistManager.AimCk)17;
			}

			public bool aimGoesLR()
			{
				return (this.aim_check & (MistManager.AimCk)6) > MistManager.AimCk.NONE;
			}

			public bool aimGoesBehind()
			{
				return (this.aim_check & (MistManager.AimCk)136) == (MistManager.AimCk)136;
			}

			public bool aimBehindBlocked()
			{
				return (this.aim_check & (MistManager.AimCk)136) == MistManager.AimCk.CHECK_BH;
			}

			public bool setWaterBehindFlag(int flag, int influence)
			{
				bool flag2 = false;
				this.aim_check &= (MistManager.AimCk)(-8388609);
				if (flag >= 2 || (flag == 1 && (this.aim_check & MistManager.AimCk.BEHIND_HAS_ROOM) != MistManager.AimCk.NONE))
				{
					if (this.aimAlreadyCheckedSLR())
					{
						this.aim_check &= (MistManager.AimCk)(-6291457);
						if (!this.aimComeFromBehind() && !this.aimGoesBehind())
						{
							this.aim_check &= (MistManager.AimCk)(-137);
						}
						this.aim_check |= MistManager.AimCk.ALLOW_GO_BEHIND;
					}
					else if (this.aimComeFromBehind() && this.aimAlreadyCheckedS())
					{
						if ((this.aim_check & MistManager.AimCk.CHECK_L) == MistManager.AimCk.NONE)
						{
							this.aim_check |= MistManager.AimCk.ALLOW_GO_L;
						}
						if ((this.aim_check & MistManager.AimCk.CHECK_R) == MistManager.AimCk.NONE)
						{
							this.aim_check |= MistManager.AimCk.ALLOW_GO_R;
						}
					}
					else
					{
						flag2 = true;
					}
				}
				else
				{
					this.aim_check &= (MistManager.AimCk)(-14680065);
				}
				if (influence == 1)
				{
					this.aim_check |= MistManager.AimCk.ALLOW_GO_INFLUENCE;
				}
				else if (influence == -1)
				{
					this.aim_check &= (MistManager.AimCk)(-1048577);
				}
				return flag2;
			}

			public bool canBehindProgress(MistManager.MistGenerator G)
			{
				return this.aimCanGoBehind() && G.K.mist_aim != MistManager.MISTAIM.NO_AIM && (G.K.max_behind_influence < 0 || (int)this.step_behind < G.K.max_behind_influence);
			}

			public MistManager.AimCk checkCanStand(Map2d Mp, int x, int y)
			{
				return (CCON.mistPassable(Mp.getConfig(x, y), 2) ? MistManager.AimCk.CFG_PASSABLE_FALL : MistManager.AimCk.NONE) | (CCON.mistPassable(Mp.getConfig(x, y), 3) ? MistManager.AimCk.CFG_PASSABLE : MistManager.AimCk.NONE);
			}

			public unsafe bool canStand(Map2d Mp, MistManager.MistPD* P, int x, int y, int cx, int cy)
			{
				bool flag = x == cx && y < cy;
				return this.canStand(Mp, flag, cx, cy);
			}

			public bool canStand(Map2d Mp, bool fall_flag, int x, int y)
			{
				MistManager.AimCk aimCk = (fall_flag ? MistManager.AimCk.CFG_PASSABLE_FALL : MistManager.AimCk.CFG_PASSABLE);
				if ((this.aim_check & MistManager.AimCk.CFG_CHECKED) == MistManager.AimCk.NONE)
				{
					this.writeCanStandFlag(this.checkCanStand(Mp, x, y));
				}
				return (this.aim_check & aimCk) > MistManager.AimCk.NONE;
			}

			public MistManager.AimCk writeCanStandFlag(MistManager.AimCk canstand)
			{
				this.aim_check = (this.aim_check & (MistManager.AimCk)(-24577)) | MistManager.AimCk.CFG_CHECKED | canstand;
				return canstand;
			}

			public bool isWaterStatic()
			{
				return (this.aim_check & MistManager.AimCk.WATER_STATIC) > MistManager.AimCk.NONE;
			}

			public void assignReduceLock(int floort, int shift)
			{
				this.reduce_lock_assigned = floort;
				this.reduce_lock = floort + shift;
			}

			public int level255;

			public byte id;

			public int step;

			private byte step_behind_;

			private byte angle_var;

			public int reduce_lock;

			public int reduce_lock_assigned;

			public int level_translated;

			public int wind;

			public MistManager.AimCk aim_check;
		}
	}
}
