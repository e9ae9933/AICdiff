using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class M2ChaserAir : M2ChaserBase<M2AirRegion>
	{
		public M2ChaserAir(M2Mover _Mv, M2AirRegionContainer _WCon)
			: base(_Mv)
		{
			this.WCon = _WCon;
			this.Bdest = new Bool1024(this.WCon.getLength(), null);
			this.ADest = new List<M2ChaserAir.ChaserDest>(1);
			this.ACs = new List<M2ChaserAir.ChaserRootAir>(5);
			this.ACsPool = new List<M2ChaserAir.ChaserRootAir>(5);
			this.ACurReg = new List<M2AirRegion>(8);
			this.ADepert = new List<M2ChaserAir.ChaserReachedDepert>(1);
			this.reset();
		}

		public override bool reset()
		{
			if (!this.WCon.recognize_finished)
			{
				return false;
			}
			this.Bdest.All0();
			this.ADest.Clear();
			this.ACurReg.Clear();
			this.resetDepert();
			this.WCon.getRegByXy((int)this.Mv.x, (int)this.Mv.y, this.ACurReg);
			if (this.ACurReg == null || this.ACurReg.Count == 0)
			{
				return false;
			}
			if (this.Btouched == null)
			{
				this.Btouched = new Bool1024(this.WCon.getLength(), null);
			}
			else
			{
				this.Btouched.Alloc(this.WCon.getLength());
				this.Btouched.All0();
			}
			this.TouchB(this.Btouched, this.ACurReg);
			int count = this.ACurReg.Count;
			for (int i = 0; i < count; i++)
			{
				this.PopCs().Init(this.Mv.x, this.Mv.y, this.ACurReg[i]);
			}
			this.check_depert_over_count = 0;
			return true;
		}

		public override bool setDest(float depx, float depy, List<M2AirRegion> AReg = null)
		{
			if (AReg != null)
			{
				AReg.Clear();
			}
			this.WCon.getRegByXy((int)depx, (int)depy, AReg);
			if (AReg == null || AReg.Count == 0)
			{
				return false;
			}
			for (int i = AReg.Count - 1; i >= 0; i--)
			{
				M2AirRegion m2AirRegion = AReg[i];
				if ((float)m2AirRegion.w < (this.Mv.sizex + 0.125f) * 2f || (float)m2AirRegion.h < (this.Mv.sizey + 0.125f) * 2f)
				{
					AReg.RemoveAt(i);
				}
			}
			if (AReg.Count == 0)
			{
				return false;
			}
			this.ADest.Add(new M2ChaserAir.ChaserDest(depx, depy, AReg.ToArray()));
			this.TouchB(this.Bdest, AReg);
			return true;
		}

		private void resetDepert()
		{
			this.ACs.AddRange(this.ACsPool);
			this.ACsPool.Clear();
			this.ADepert.Clear();
			this.cs_max_i = 0;
		}

		public bool Warp()
		{
			if (this.Mv == null)
			{
				return false;
			}
			this.resetDepert();
			this.ACurReg.Clear();
			this.WCon.getRegByXy((int)this.Mv.x, (int)this.Mv.y, this.ACurReg);
			if (this.ACurReg == null || this.ACurReg.Count == 0)
			{
				return false;
			}
			this.check_depert_over = 0;
			int count = this.ADest.Count;
			for (int i = 0; i < count; i++)
			{
				M2ChaserAir.ChaserRootAir chaserRootAir = this.PopCs();
				chaserRootAir.Init(this.Mv.x, this.Mv.y, this.ACurReg[0]);
				this.ADepert.Add(new M2ChaserAir.ChaserReachedDepert(chaserRootAir, this.ADest[i].depx, this.ADest[i].depy));
				List<M2ChaserAir.ChaserRootAir> acs = this.ACs;
				int num = this.cs_max_i - 1;
				this.cs_max_i = num;
				acs.RemoveAt(num);
			}
			return true;
		}

		private M2ChaserAir.ChaserRootAir PopCs()
		{
			if (this.cs_max_i < this.ACs.Count)
			{
				List<M2ChaserAir.ChaserRootAir> acs = this.ACs;
				int num = this.cs_max_i;
				this.cs_max_i = num + 1;
				return acs[num];
			}
			M2ChaserAir.ChaserRootAir chaserRootAir = new M2ChaserAir.ChaserRootAir();
			this.cs_max_i++;
			this.ACs.Add(chaserRootAir);
			return chaserRootAir;
		}

		public override M2ChaserBaseFuncs.CHSRES chaseProgress()
		{
			for (int i = this.cs_max_i - 1; i >= 0; i--)
			{
				M2ChaserAir.ChaserRootAir chaserRootAir = this.ACs[i];
				if (this.Bdest[chaserRootAir.Region.index])
				{
					float num = -1000f;
					float num2 = -1000f;
					M2ChaserAir.ChaserDest chaserDest = null;
					for (int j = this.ADest.Count - 1; j >= 0; j--)
					{
						M2ChaserAir.ChaserDest chaserDest2 = this.ADest[j];
						if (X.isinC<M2AirRegion>(chaserDest2.AWr, chaserRootAir.Region) >= 0)
						{
							num = chaserDest2.depx;
							num2 = chaserDest2.depy;
							chaserDest = chaserDest2;
							break;
						}
					}
					if (num != -1000f)
					{
						M2ChaserAir.ChaserReachedDepert chaserReachedDepert = chaserRootAir.finalize(this.Mp, this.Mv.sizex + 0.125f, this.Mv.sizey + 0.125f, num, num2, chaserDest);
						if (chaserReachedDepert.valid && this.fnGetPointAddition != null)
						{
							this.fnGetPointAddition(ref chaserReachedDepert);
						}
						if (chaserReachedDepert.valid)
						{
							for (int k = 0; k < this.ADepert.Count; k++)
							{
								M2ChaserAir.ChaserReachedDepert chaserReachedDepert2 = this.ADepert[k];
								if (chaserReachedDepert.total_score < chaserReachedDepert2.total_score)
								{
									this.ADepert.Insert(k, chaserReachedDepert);
									chaserReachedDepert = default(M2ChaserAir.ChaserReachedDepert);
									break;
								}
							}
							if (chaserReachedDepert.valid)
							{
								this.ADepert.Add(chaserReachedDepert);
							}
							this.ACsPool.Add(chaserRootAir);
							this.ACs.RemoveAt(i);
							this.cs_max_i--;
						}
						else
						{
							this.ACs.RemoveAt(i);
							List<M2ChaserAir.ChaserRootAir> acs = this.ACs;
							int num3 = this.cs_max_i - 1;
							this.cs_max_i = num3;
							acs.Insert(num3, chaserRootAir);
						}
						this.dupeCheck();
					}
				}
				else
				{
					int overWR_count = chaserRootAir.Region.overWR_count;
					for (int l = 0; l < overWR_count; l++)
					{
						M2AirRegion overRegByIndex = chaserRootAir.Region.getOverRegByIndex(l);
						if (!this.Btouched[overRegByIndex.index])
						{
							M2ChaserRootBase<M2AirRegion> m2ChaserRootBase = this.PopCs().Init(overRegByIndex, chaserRootAir, this.Mv.sizex + 0.125f, this.Mv.sizey + 0.125f);
							if (m2ChaserRootBase != null && chaserRootAir.Parent == null && !this.Mp.canThroughBcc(chaserRootAir.x, chaserRootAir.y, m2ChaserRootBase.x, m2ChaserRootBase.y, this.Mv.sizex + 0.3f, this.Mv.sizey + 0.35f, -1, false, false, null, false, null))
							{
								m2ChaserRootBase = null;
							}
							if (m2ChaserRootBase == null)
							{
								this.cs_max_i--;
							}
							this.dupeCheck();
						}
					}
					this.ACs.RemoveAt(i);
					this.cs_max_i--;
					this.ACsPool.Add(chaserRootAir);
					this.dupeCheck();
				}
			}
			for (int m = this.cs_max_i - 1; m >= 0; m--)
			{
				M2ChaserAir.ChaserRootAir chaserRootAir2 = this.ACs[m];
				this.Btouched[chaserRootAir2.Region.index] = true;
			}
			if (this.ADepert.Count > 0)
			{
				if (this.cs_max_i > 0)
				{
					int num3 = this.check_depert_over_count + 1;
					this.check_depert_over_count = num3;
					if (num3 >= this.check_depert_over)
					{
						this.cs_max_i = 0;
					}
				}
				if (this.cs_max_i == 0)
				{
					return M2ChaserBaseFuncs.CHSRES.FOUND;
				}
			}
			else if (this.cs_max_i == 0)
			{
				return M2ChaserBaseFuncs.CHSRES.ERROR;
			}
			return M2ChaserBaseFuncs.CHSRES.FINDING;
		}

		public bool depert_defined
		{
			get
			{
				return this.ADepert.Count > 0;
			}
		}

		public void dupeCheck()
		{
		}

		public M2ChaserBaseFuncs.CHSRES Walk(float fcnt, M2ChaserAir.FnProgressWalk fnProgress, bool no_chaseprogress = false)
		{
			return M2ChaserBaseFuncs.CHSRES.REACHED;
		}

		private void TouchB(Bool1024 B, List<M2AirRegion> AReg)
		{
			int count = AReg.Count;
			for (int i = 0; i < count; i++)
			{
				B[AReg[i].index] = true;
			}
		}

		public override bool hasDestPoint()
		{
			return this.ADest.Count > 0;
		}

		public List<M2ChaserAir.ChaserReachedDepert> getDepertArray()
		{
			return this.ADepert;
		}

		public readonly M2AirRegionContainer WCon;

		private Bool1024 Btouched;

		private Bool1024 Bdest;

		public int cs_max_i;

		public readonly List<M2AirRegion> ACurReg;

		private readonly List<M2ChaserAir.ChaserDest> ADest;

		private readonly List<M2ChaserAir.ChaserRootAir> ACs;

		private readonly List<M2ChaserAir.ChaserRootAir> ACsPool;

		public List<M2ChaserAir.ChaserReachedDepert> ADepert;

		public int check_depert_over;

		private int check_depert_over_count;

		public M2ChaserAir.FnGetSortPrepare fnSortPrepare;

		public M2ChaserAir.FnGetPointAddition fnGetPointAddition;

		public float wall_hitted;

		private float fine_t;

		public const int WALK_FINE_T = 8;

		public delegate bool FnGetSortPrepare(M2ChaserAir Chaser, List<M2ChaserAir.ChaserRootAir> ADepert);

		public delegate bool FnProgressWalk(M2ChaserAir Chaser, M2ChaserAir.ChaserRootAir Root, Vector2 Next, ref float fine_t);

		public delegate void FnGetPointAddition(ref M2ChaserAir.ChaserReachedDepert Depert);

		public sealed class ChaserRootAir : M2ChaserRootBase<M2AirRegion>
		{
			public M2ChaserAir.ChaserReachedDepert finalize(Map2d Mp, float margin_x, float margin_y, float depx, float depy, M2ChaserAir.ChaserDest TargetDest)
			{
				if (this.Parent == null && !Mp.canThroughBcc(this.x, this.y, depx, depy, margin_x + 0.25f, margin_y + 0.25f, -1, false, false, null, false, null))
				{
					return default(M2ChaserAir.ChaserReachedDepert);
				}
				return new M2ChaserAir.ChaserReachedDepert(this, depx, depy);
			}
		}

		public sealed class ChaserDest
		{
			public ChaserDest(float _depx, float _depy, M2AirRegion[] _AWr)
			{
				this.depx = _depx;
				this.depy = _depy;
				this.AWr = _AWr;
			}

			public bool isTouching(M2AirRegion WR)
			{
				return X.isinC<M2AirRegion>(this.AWr, WR) >= 0;
			}

			public float depx;

			public float depy;

			public M2AirRegion[] AWr;
		}

		public struct ChaserReachedDepert
		{
			public ChaserReachedDepert(M2ChaserAir.ChaserRootAir _Last, float _x, float _y)
			{
				this.Last = _Last;
				this.x = _x;
				this.y = _y;
				try
				{
					this.generation = this.Last.calcGeneration();
				}
				catch (Exception)
				{
					this.generation = -1;
					X.de("不明なgeneration", null);
				}
				this.walk_len = this.Last.walk_len_total;
				this.point_addition = 0f;
			}

			public float total_score
			{
				get
				{
					return this.walk_len + this.point_addition;
				}
			}

			public bool valid
			{
				get
				{
					return this.Last != null;
				}
			}

			public void getDepert(int gen_index, out float depx, out float depy)
			{
				if (gen_index <= 0)
				{
					depx = this.x;
					depy = this.y;
					return;
				}
				M2ChaserRootBase<M2AirRegion> m2ChaserRootBase = null;
				M2ChaserRootBase<M2AirRegion> m2ChaserRootBase2 = this.Last;
				while (--gen_index != 0)
				{
					m2ChaserRootBase = m2ChaserRootBase2;
					m2ChaserRootBase2 = m2ChaserRootBase2.Parent;
				}
				M2ChaserRootBase<M2AirRegion> parent = m2ChaserRootBase2.Parent;
				if (parent == null)
				{
					depx = m2ChaserRootBase2.x;
					depy = m2ChaserRootBase2.y;
					return;
				}
				float num = this.x;
				float num2 = this.y;
				if (m2ChaserRootBase != null)
				{
					num = m2ChaserRootBase.x;
					num2 = m2ChaserRootBase.y;
				}
				depx = m2ChaserRootBase2.x;
				depy = m2ChaserRootBase2.y;
				if (parent.x == depx)
				{
					depy = num2;
					return;
				}
				if (parent.y == depy)
				{
					depx = num;
				}
			}

			public bool isSame(M2ChaserAir.ChaserReachedDepert O)
			{
				return this.Last == O.Last;
			}

			public readonly M2ChaserAir.ChaserRootAir Last;

			public readonly float walk_len;

			public float point_addition;

			public readonly int generation;

			public readonly float x;

			public readonly float y;
		}
	}
}
