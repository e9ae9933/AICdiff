using System;
using System.Collections.Generic;
using Better;
using m2d;
using XX;

namespace nel
{
	public sealed class CarrierRailBlock
	{
		public static void initS()
		{
			CarrierRailBlock.OARailCache = new BDic<M2MapLayer, List<M2Puts>>();
			if (CarrierRailBlock.ABlocks != null)
			{
				for (int i = CarrierRailBlock.ABlocks.Count - 1; i >= 0; i--)
				{
					CarrierRailBlock.ABlocks[i].destruct();
				}
			}
			CarrierRailBlock.ABlocks = new List<CarrierRailBlock>();
		}

		public static CarrierRailBlock getRailRoad(M2MapLayer Lay, int cx, int cy, int start_aim, bool create_if_not_exist = true)
		{
			int count = CarrierRailBlock.ABlocks.Count;
			for (int i = 0; i < count; i++)
			{
				if (CarrierRailBlock.ABlocks[i].hasPoint(Lay, cx, cy, start_aim))
				{
					return CarrierRailBlock.ABlocks[i];
				}
			}
			if (!create_if_not_exist)
			{
				return null;
			}
			List<M2Puts> list = X.Get<M2MapLayer, List<M2Puts>>(CarrierRailBlock.OARailCache, Lay);
			if (list == null)
			{
				list = Lay.Mp.getAllPointMetaPutsTo((int)Lay.Bounds.x, (int)Lay.Bounds.y, (int)Lay.Bounds.width, (int)Lay.Bounds.h, CarrierRailBlock.OARailCache[Lay] = new List<M2Puts>(), (M2Puts V, List<M2Puts> _List) => V.Lay == Lay && V.getMeta().Get("rail") != null && V is NelChipCarrierRail);
			}
			if (list.Count == 0)
			{
				return null;
			}
			CarrierRailBlock carrierRailBlock = new CarrierRailBlock(Lay);
			List<CarrierRailBlock.CarrierRail> list2 = new List<CarrierRailBlock.CarrierRail>();
			int num = list.Count;
			BDic<uint, CarrierRailBlock.RailBuffer> bdic = new BDic<uint, CarrierRailBlock.RailBuffer>();
			for (int j = 0; j < num; j++)
			{
				NelChipCarrierRail nelChipCarrierRail = list[j] as NelChipCarrierRail;
				uint num2 = Map2d.Cp2b(nelChipCarrierRail);
				CarrierRailBlock.RailBuffer railBuffer = X.Get<uint, CarrierRailBlock.RailBuffer>(bdic, num2);
				if (railBuffer == null)
				{
					bdic[num2] = new CarrierRailBlock.RailBuffer(nelChipCarrierRail);
				}
				else
				{
					railBuffer.Add(nelChipCarrierRail);
				}
			}
			CarrierRailBlock.RailBuffer railBuffer2 = X.Get<uint, CarrierRailBlock.RailBuffer>(bdic, Map2d.xy2b(cx, cy));
			if (railBuffer2 == null)
			{
				return null;
			}
			CarrierRailBlock.CarrierRail carrierRail = new CarrierRailBlock.CarrierRail(cx, cy, start_aim);
			new CarrierRailBlock.CarrierRail(cx, cy, start_aim);
			list2.Add(carrierRail);
			List<CarrierRailBlock.RailBuffer> list3 = new List<CarrierRailBlock.RailBuffer>(num);
			int num3 = 0;
			list3.Add(railBuffer2);
			if (railBuffer2.hasStop())
			{
				carrierRail.stop = true;
			}
			int num4 = (int)((start_aim == -1) ? ((AIM)4294967295U) : CAim.get_opposite((AIM)start_aim));
			List<int> list4 = railBuffer2.Adir;
			for (;;)
			{
				num = list4.Count;
				if (carrierRail.aim == -1)
				{
					int num5 = 0;
					for (int k = 1; k < num; k += 2)
					{
						if (list4[k] >= 0)
						{
							num5 |= 1 << list4[k];
						}
					}
					if (num5 == 0)
					{
						break;
					}
					for (int l = 0; l < 4; l++)
					{
						int num6 = (l + 2) % 4;
						if ((num5 & (1 << num6)) != 0)
						{
							carrierRail.aim = num6;
							break;
						}
					}
					if (carrierRail.aim == -1)
					{
						break;
					}
					if (num4 == -1)
					{
						num4 = (int)CAim.get_opposite((AIM)carrierRail.aim);
					}
				}
				uint num7 = Map2d.xy2b(carrierRail.mapx + CAim._XD(carrierRail.aim, carrierRail.len + 1), carrierRail.mapy - CAim._YD(carrierRail.aim, carrierRail.len + 1));
				CarrierRailBlock.RailBuffer railBuffer3 = X.Get<uint, CarrierRailBlock.RailBuffer>(bdic, num7);
				List<int> list5 = null;
				int num8 = -1;
				if (railBuffer3 != null)
				{
					list5 = railBuffer3.Adir;
					if (list5 != null)
					{
						int count2 = list5.Count;
						for (int m = 0; m < count2; m += 2)
						{
							if (list5[m] == num4)
							{
								num8 = list5[m + 1];
								break;
							}
						}
					}
				}
				if (num8 == -1)
				{
					num3 |= 1 << carrierRail.aim;
					CarrierRailBlock.AssignRailChips(bdic, list3, carrierRail);
					list3.Clear();
					int num9 = 0;
					for (int n = 0; n < num; n += 2)
					{
						if (list4[n] >= 0 && (num3 & (1 << list4[n])) == 0)
						{
							num9 |= 1 << list4[n + 1];
						}
					}
					if (num9 == 0)
					{
						break;
					}
					CarrierRailBlock.CarrierRail carrierRail2 = new CarrierRailBlock.CarrierRail(carrierRail.mapx + CAim._XD(carrierRail.aim, carrierRail.len), carrierRail.mapy - CAim._YD(carrierRail.aim, carrierRail.len), -1);
					for (int num10 = 1; num10 <= 4; num10++)
					{
						int num11 = (num10 + carrierRail.aim) % 4;
						if ((num9 & (1 << num11)) != 0)
						{
							carrierRail2.aim = num11;
							break;
						}
					}
					if (carrierRail2.aim == -1 || !CarrierRailBlock.checkRailConnection(carrierRailBlock, list2, ref carrierRail, carrierRail2, ref num4))
					{
						break;
					}
					list3.Add(railBuffer2);
				}
				else
				{
					num3 = 0;
					carrierRail.len++;
					bool flag = railBuffer3.hasStop();
					if (num8 != carrierRail.aim || flag)
					{
						CarrierRailBlock.AssignRailChips(bdic, list3, carrierRail);
						list3.Clear();
						CarrierRailBlock.CarrierRail carrierRail3 = new CarrierRailBlock.CarrierRail(carrierRail.mapx + CAim._XD(carrierRail.aim, carrierRail.len), carrierRail.mapy - CAim._YD(carrierRail.aim, carrierRail.len), num8);
						if (!CarrierRailBlock.checkRailConnection(carrierRailBlock, list2, ref carrierRail, carrierRail3, ref num4))
						{
							break;
						}
						carrierRail3.stop = flag;
					}
					list4 = list5;
					railBuffer2 = railBuffer3;
					list3.Add(railBuffer2);
				}
			}
			carrierRailBlock.ACorner = list2.ToArray();
			if (carrierRailBlock.loop_to == -1)
			{
				carrierRailBlock.loop_to = carrierRailBlock.Length - 1;
			}
			num = carrierRailBlock.ACorner.Length;
			for (int num12 = 0; num12 < num; num12++)
			{
				CarrierRailBlock.CarrierRail carrierRail4 = carrierRailBlock.ACorner[num12];
				carrierRailBlock.Rc.Expand((float)carrierRail4.mapx, (float)carrierRail4.mapy, (float)(carrierRail4.len * CAim._XD(carrierRail4.aim, 1)), (float)(carrierRail4.len * CAim._YD(carrierRail4.aim, 1)), false);
			}
			CarrierRailBlock.ABlocks.Add(carrierRailBlock);
			return carrierRailBlock;
		}

		private static bool checkRailConnection(CarrierRailBlock Blk, List<CarrierRailBlock.CarrierRail> ACorner, ref CarrierRailBlock.CarrierRail RR, CarrierRailBlock.CarrierRail nRR, ref int aim_opposit)
		{
			int count = ACorner.Count;
			for (int i = 0; i < count; i++)
			{
				CarrierRailBlock.CarrierRail carrierRail = ACorner[i];
				if (carrierRail.mapx == nRR.mapx && carrierRail.mapy == nRR.mapy && carrierRail.aim == nRR.aim)
				{
					Blk.loop_to = i;
					nRR.stop = carrierRail.stop;
					return false;
				}
			}
			if (RR.len == 0)
			{
				ACorner.Remove(RR);
				nRR.stop = RR.stop;
			}
			RR = nRR;
			aim_opposit = (int)CAim.get_opposite((AIM)nRR.aim);
			ACorner.Add(nRR);
			return true;
		}

		public CarrierRailBlock(M2MapLayer _Lay)
		{
			this.Lay = _Lay;
			this.ARegistered = new List<M2PuzzCarrierMover>();
			this.Actv = new FlaggerC<NelChipCarrierRail>(delegate(FlaggerC<NelChipCarrierRail> V)
			{
				this.setEffect(V.Get1());
				this.fineSF(true);
			}, delegate(FlaggerC<NelChipCarrierRail> V)
			{
				if (this.Ed != null)
				{
					this.Ed.destruct();
				}
				this.Ed = null;
				this.fineSF(true);
			});
			this.Rc = new DRect("_bounds");
		}

		public int Length
		{
			get
			{
				if (this.ACorner == null)
				{
					return 0;
				}
				return this.ACorner.Length;
			}
		}

		public CarrierRailBlock.CarrierRail this[int index]
		{
			get
			{
				return this.ACorner[index];
			}
		}

		public void RegisterMover(M2PuzzCarrierMover Mv)
		{
			if (this.ARegistered.IndexOf(Mv) == -1)
			{
				this.ARegistered.Add(Mv);
			}
		}

		public void UnregisterMover(M2PuzzCarrierMover Mv)
		{
			this.ARegistered.Remove(Mv);
		}

		public bool hasPoint(M2MapLayer _Lay, int cx, int cy, int start_aim)
		{
			if (_Lay != this.Lay || this.ACorner == null)
			{
				return false;
			}
			int num = this.ACorner.Length;
			for (int i = 0; i < num; i++)
			{
				if (this.ACorner[i].isOnRail(cx, cy, start_aim) >= 0)
				{
					return true;
				}
			}
			return false;
		}

		public bool fineSF(bool apply_effect)
		{
			int count = this.ARegistered.Count;
			for (int i = 0; i < count; i++)
			{
				this.ARegistered[i].fineSF(apply_effect);
			}
			return apply_effect;
		}

		public bool liner_activated
		{
			get
			{
				return this.Actv.isActive();
			}
		}

		public int getReturnPos(int cx, int cy, int ci)
		{
			int num = this.ACorner.Length;
			for (int i = 0; i < num; i++)
			{
				if (i != ci && this.ACorner[i].isOnRail(cx, cy, -1) >= 0)
				{
					return i;
				}
			}
			return -1;
		}

		private static void AssignRailChips(BDic<uint, CarrierRailBlock.RailBuffer> OPos, List<CarrierRailBlock.RailBuffer> ARb, CarrierRailBlock.CarrierRail R)
		{
			List<NelChipCarrierRail> list = new List<NelChipCarrierRail>();
			int count = ARb.Count;
			for (int i = 0; i < count; i++)
			{
				ARb[i].ExtractChips(OPos, list, (AIM)R.aim, i == 0, i == count - 1);
			}
			R.ARailCp = list.ToArray();
		}

		public M2DrawBinder setEffect(NelChipCarrierRail Cp)
		{
			if (this.FD_drawActivatingEffect == null)
			{
				this.FD_drawActivatingEffect = new M2DrawBinder.FnEffectBind(this.drawActivatingEffect);
			}
			if (this.Ed == null)
			{
				this.Ed = Cp.Mp.setEDC("rail_activating", this.FD_drawActivatingEffect, 0f);
			}
			return this.Ed;
		}

		public void destruct()
		{
			if (this.Ed != null)
			{
				this.Ed.destruct();
				this.Ed = null;
			}
		}

		private bool drawActivatingEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.effect_duration == 0)
			{
				for (int i = this.Length - 1; i >= this.loop_to; i--)
				{
					this.effect_duration += this.ACorner[i].len;
				}
				if (this.effect_duration == 0)
				{
					this.Ed = null;
					return false;
				}
			}
			if (!Ed.isinCamera(this.Rc.cx, this.Rc.cy, this.Rc.w / 2f + 1f, this.Rc.h / 2f + 1f))
			{
				return true;
			}
			int num = X.Mx(1, this.effect_duration * 4 / 70);
			M2DBase m2D = Ed.Mp.M2D;
			MeshDrawer meshDrawer = null;
			int num2;
			if (num == 0)
			{
				num2 = 70;
			}
			else
			{
				num2 = this.effect_duration * 4 / num;
			}
			for (int j = 0; j < 2; j++)
			{
				int num3 = (int)Ef.af;
				for (int k = this.Length - 1; k >= this.loop_to; k--)
				{
					CarrierRailBlock.CarrierRail carrierRail = this.ACorner[(j == 0) ? k : (this.Length - 1 - k + this.loop_to)];
					int num4 = carrierRail.ARailCp.Length;
					for (int l = 0; l < num4; l++)
					{
						int num5 = num3 % num2;
						num3 += 4;
						float num6 = X.ZLINE((float)num5, 30f);
						if (num6 < 1f)
						{
							NelChipCarrierRail nelChipCarrierRail = carrierRail.ARailCp[l];
							if (Ed.isinCamera(nelChipCarrierRail.mapcx, nelChipCarrierRail.mapcy, (float)nelChipCarrierRail.clms * 0.5f + 1f, (float)nelChipCarrierRail.rows * 0.5f + 1f))
							{
								if (meshDrawer == null)
								{
									meshDrawer = Ef.GetMeshImg("", m2D.MIchip, BLEND.ADD, true);
								}
								meshDrawer.Col = meshDrawer.ColGrd.White().setA1((1f - num6) * 0.4f).C;
								meshDrawer.base_x = Ed.Mp.map2ux(nelChipCarrierRail.mapcx);
								meshDrawer.base_y = Ed.Mp.map2uy(nelChipCarrierRail.mapcy);
								if (nelChipCarrierRail.Img.initAtlasMd(meshDrawer, 0U))
								{
									meshDrawer.RotaGraph(0f, 0f, 1f, nelChipCarrierRail.draw_rotR, null, nelChipCarrierRail.flip);
								}
							}
						}
					}
				}
			}
			return true;
		}

		private static List<CarrierRailBlock> ABlocks;

		private static BDic<M2MapLayer, List<M2Puts>> OARailCache;

		private CarrierRailBlock.CarrierRail[] ACorner;

		public readonly M2MapLayer Lay;

		public int loop_to;

		public int effect_duration;

		private M2DrawBinder Ed;

		public DRect Rc;

		private readonly List<M2PuzzCarrierMover> ARegistered;

		public FlaggerC<NelChipCarrierRail> Actv;

		private M2DrawBinder.FnEffectBind FD_drawActivatingEffect;

		public class CarrierRail
		{
			public bool stop
			{
				get
				{
					return this.stop_;
				}
				set
				{
					this.stop_ = value;
				}
			}

			public CarrierRail(int _mapx, int _mapy, int _aim = -1)
			{
				this.mapx = _mapx;
				this.mapy = _mapy;
				this.aim = _aim;
			}

			public int isOnRail(int _mapx, int _mapy, int _aim = -1)
			{
				if (_aim != -1 && this.aim != _aim)
				{
					return -1;
				}
				switch (this.aim)
				{
				case 0:
					if (this.mapy != _mapy || !X.BTWW((float)(this.mapx - this.len), (float)_mapx, (float)this.mapx))
					{
						return -1;
					}
					return this.mapx - _mapx;
				case 1:
					if (this.mapx != _mapx || !X.BTWW((float)(this.mapy - this.len), (float)_mapy, (float)this.mapy))
					{
						return -1;
					}
					return this.mapy - _mapy;
				case 2:
					if (this.mapy != _mapy || !X.BTWW((float)this.mapx, (float)_mapx, (float)(this.mapx + this.len)))
					{
						return -1;
					}
					return _mapx - this.mapx;
				case 3:
					if (this.mapx != _mapx || !X.BTWW((float)this.mapy, (float)_mapy, (float)(this.mapy + this.len)))
					{
						return -1;
					}
					return _mapy - this.mapy;
				default:
				{
					int num = this.mapx;
					int num2 = this.mapy;
					for (int i = 0; i <= this.len; i++)
					{
						if (num == _mapx && num2 == _mapy)
						{
							return i;
						}
						num += CAim._XD(this.aim, 1);
						num2 -= CAim._YD(this.aim, 1);
					}
					return -1;
				}
				}
			}

			public int mapx;

			public int mapy;

			public int aim = -1;

			public int len;

			private bool stop_;

			public NelChipCarrierRail[] ARailCp;
		}

		private class RailBuffer
		{
			public RailBuffer(NelChipCarrierRail _RailCp = null)
			{
				this.Adir = new List<int>();
				this.ACp = new List<NelChipCarrierRail>();
				if (_RailCp != null)
				{
					this.Add(_RailCp);
				}
			}

			public CarrierRailBlock.RailBuffer Add(NelChipCarrierRail _RailCp)
			{
				if (this.ACp.IndexOf(_RailCp) == -1)
				{
					this.Adir.AddRange(_RailCp.getMeta().getDirs("rail", _RailCp.rotation, _RailCp.flip, 0));
					this.ACp.Add(_RailCp);
				}
				return this;
			}

			public CarrierRailBlock.RailBuffer ExtractChips(BDic<uint, CarrierRailBlock.RailBuffer> OPos, List<NelChipCarrierRail> ATo, AIM aim, bool is_first, bool is_last)
			{
				int num = CAim._XD(aim, 1);
				int num2 = CAim._YD(aim, 1);
				int count = this.ACp.Count;
				ATo.Capacity += count;
				int num3 = (int)CAim.get_opposite(aim);
				for (int i = 0; i < count; i++)
				{
					NelChipCarrierRail nelChipCarrierRail = this.ACp[i];
					int[] dirs = nelChipCarrierRail.getMeta().getDirs("rail", nelChipCarrierRail.rotation, nelChipCarrierRail.flip, 0);
					int num4 = dirs.Length;
					bool flag = false;
					for (int j = 0; j < num4; j += 2)
					{
						if ((is_first || dirs[j] == num3) && (is_last || dirs[j + 1] == (int)aim))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						ATo.Add(nelChipCarrierRail);
						if (CAim.is_naname(aim))
						{
							string join = nelChipCarrierRail.getMeta().GetJoin(",", "rail");
							for (int k = 0; k < 2; k++)
							{
								uint num5 = Map2d.xy2b(nelChipCarrierRail.mapx + ((k == 0) ? num : 0), nelChipCarrierRail.mapy - ((k == 1) ? num2 : 0));
								CarrierRailBlock.RailBuffer railBuffer = X.Get<uint, CarrierRailBlock.RailBuffer>(OPos, num5);
								if (railBuffer != null && railBuffer.ACp.Count > 0)
								{
									NelChipCarrierRail nelChipCarrierRail2 = railBuffer.ACp[0];
									if (nelChipCarrierRail2 != null && nelChipCarrierRail2.getMeta().GetJoin(",", "rail") == join)
									{
										ATo.Add(nelChipCarrierRail2);
									}
								}
							}
						}
					}
				}
				return this;
			}

			public bool hasStop()
			{
				int count = this.ACp.Count;
				for (int i = 0; i < count; i++)
				{
					if (this.ACp[i].getMeta().GetI("rail_stop", 0, 0) != 0)
					{
						return true;
					}
				}
				return false;
			}

			public List<int> Adir;

			private readonly List<NelChipCarrierRail> ACp;
		}
	}
}
