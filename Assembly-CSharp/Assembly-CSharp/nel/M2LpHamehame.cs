using System;
using System.Collections.Generic;
using Better;
using m2d;
using XX;

namespace nel
{
	public sealed class M2LpHamehame : NelLp
	{
		public M2LpHamehame(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
			if (M2LpHamehame.ABuf == null)
			{
				M2LpHamehame.ABuf = new List<M2Puts>(3);
			}
		}

		public override void initActionPre()
		{
			base.initActionPre();
		}

		public override void initAction(bool normal_map)
		{
			META meta = new META(this.comment);
			this.switch_id = meta.GetI("switch_id", -1, 0);
			if (this.switch_id < 0)
			{
				X.de("M2LpHamehame::initAction switch_id を設定して下さい", null);
				return;
			}
			BDic<uint, List<M2Puts>> bdic = new BDic<uint, List<M2Puts>>(2);
			this.AAHame = new List<NelChipHamehame[]>(2);
			this.AHameRc = new List<DRect>(2);
			for (int i = this.mapx + this.mapw - 1; i >= this.mapx; i--)
			{
				for (int j = this.mapy + this.maph - 1; j >= this.mapy; j--)
				{
					M2Pt pointPuts = this.Mp.getPointPuts(i, j, false, false);
					if (pointPuts != null)
					{
						M2LpHamehame.ABuf.Clear();
						pointPuts.getPointMetaPutsTo(M2LpHamehame.ABuf, 1UL << (this.Lay.index & 31), "hame");
						for (int k = M2LpHamehame.ABuf.Count - 1; k >= 0; k--)
						{
							M2Puts m2Puts = M2LpHamehame.ABuf[k];
							if (!bdic.ContainsKey(Map2d.xy2b(m2Puts.mapx, m2Puts.mapy)))
							{
								List<M2Puts> list = this.Mp.findPutsFilling(m2Puts.mapx, m2Puts.mapy, (M2Puts __Cp, List<M2Puts> APt) => __Cp is NelChipHamehame && __Cp.Lay == this.Lay && __Cp.Img.Meta.Get("hame") != null, null, null);
								if (list != null)
								{
									List<NelChipHamehame> list2 = new List<NelChipHamehame>(list.Count);
									DRect drect = new DRect(list.Count.ToString());
									for (int l = list.Count - 1; l >= 0; l--)
									{
										NelChipHamehame nelChipHamehame = list[l] as NelChipHamehame;
										list2.Add(nelChipHamehame);
										drect.Expand((float)nelChipHamehame.mapx, (float)nelChipHamehame.mapy, (float)(nelChipHamehame.mapx + nelChipHamehame.clms), (float)(nelChipHamehame.mapy + nelChipHamehame.rows), false);
										nelChipHamehame.ManageTo = this;
										nelChipHamehame.mng_id = this.AAHame.Count;
										for (int m = nelChipHamehame.mapx + nelChipHamehame.clms - 1; m >= nelChipHamehame.mapx; m--)
										{
											for (int n = nelChipHamehame.mapy + nelChipHamehame.rows - 1; n >= nelChipHamehame.mapy; n--)
											{
												bdic[Map2d.xy2b(m, n)] = list;
											}
										}
									}
									this.AAHame.Add(list2.ToArray());
									this.AHameRc.Add(drect);
								}
							}
						}
					}
				}
			}
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			this.decided_bits = 0U;
			this.AAHame = null;
			this.AHameRc = null;
		}

		public NelChipHamehame[] getConnection(int mng_id)
		{
			if (this.AAHame == null)
			{
				return null;
			}
			if (!X.BTW(0f, (float)mng_id, (float)this.AAHame.Count))
			{
				return null;
			}
			return this.AAHame[mng_id];
		}

		public DRect getConnectionRect(int mng_id)
		{
			if (this.AHameRc == null)
			{
				return null;
			}
			if (!X.BTW(0f, (float)mng_id, (float)this.AHameRc.Count))
			{
				return null;
			}
			return this.AHameRc[mng_id];
		}

		public void setPtcAll(string ptcst_key, int except_mng_id = -1)
		{
			if (this.AAHame == null)
			{
				return;
			}
			for (int i = this.AAHame.Count - 1; i >= 0; i--)
			{
				if (except_mng_id < 0 || i != except_mng_id)
				{
					NelChipHamehame[] array = this.AAHame[i];
					int num = array.Length;
					for (int j = 0; j < num; j++)
					{
						NelChipHamehame nelChipHamehame = array[j];
						this.Mp.PtcSTsetVar("cx", (double)nelChipHamehame.mapcx).PtcSTsetVar("cy", (double)nelChipHamehame.mapcy).PtcST(ptcst_key, null, PTCThread.StFollow.NO_FOLLOW);
					}
				}
			}
		}

		public bool DecideBox(int mng_id)
		{
			bool fullfilled = this.fullfilled;
			this.decided_bits |= 1U << mng_id;
			if (!fullfilled && this.fullfilled)
			{
				PuzzLiner.activateConnector(this.switch_id, false);
				return true;
			}
			return false;
		}

		public void RemoveBox(int mng_id)
		{
			bool fullfilled = this.fullfilled;
			this.decided_bits &= ~(1U << mng_id);
			if (fullfilled && !this.fullfilled)
			{
				PuzzLiner.deactivateConnector(this.switch_id, false, false);
				PuzzLiner.finalizeAnimation(true, this.switch_id, true);
			}
		}

		public bool fullfilled
		{
			get
			{
				if (this.AAHame == null)
				{
					return false;
				}
				uint num = (1U << this.AAHame.Count) - 1U;
				return num == (this.decided_bits & num);
			}
		}

		public int switch_id;

		private static List<M2Puts> ABuf;

		private List<NelChipHamehame[]> AAHame;

		private List<DRect> AHameRc;

		private uint decided_bits;
	}
}
