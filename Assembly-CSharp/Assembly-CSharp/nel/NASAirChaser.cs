using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NASAirChaser : NelEnemyAssist
	{
		public NASAirChaser(NelEnemy _En, NASAirChaser.FnProgressWalk _fnProgressWalk)
			: base(_En)
		{
			this.WRC_Air = this.En.Summoner.Summoner.getRegionCheckerAir();
			this.Cs = new M2ChaserAir(this.En, this.WRC_Air);
			this.fnProgressWalk = _fnProgressWalk;
			this.ADepert = new List<Vector2>(2);
			this.ABuf = new List<M2AirRegion>();
			this.hitwall_counter = new RevCounter();
		}

		public NASAirChaser Clear(bool _warp_flag = false)
		{
			this.need_init = true;
			this.warp_flag = _warp_flag;
			this.ADepert.Clear();
			this.Dest = default(M2ChaserAir.ChaserReachedDepert);
			this.t_error = 150f;
			float x = base.x;
			float y = base.y;
			this.startx = x;
			this.starty = y;
			this.hitwall_counter.Clear();
			return this;
		}

		public NASAirChaser setDest(float depx, float depy)
		{
			this.ADepert.Add(new Vector2(depx, depy));
			return this;
		}

		public bool hasDestPoint()
		{
			return this.ADepert.Count > 0;
		}

		public int dest_count
		{
			get
			{
				return this.ADepert.Count;
			}
		}

		public M2ChaserBaseFuncs.CHSRES Walk(float fcnt)
		{
			Bench.P("AirChaser_Walk");
			M2ChaserBaseFuncs.CHSRES chsres = this.WalkInner(fcnt);
			Bench.Pend("AirChaser_Walk");
			return chsres;
		}

		private M2ChaserBaseFuncs.CHSRES WalkInner(float fcnt)
		{
			float num = -1000f;
			bool flag = false;
			float num2 = base.x;
			float num3 = base.y;
			if (!this.WRC_Air.recognize_finished)
			{
				M2ChaserAir.ChaserReachedDepert chaserReachedDepert = this.Cs.getDepertArray()[0];
				num2 = chaserReachedDepert.x;
				num3 = chaserReachedDepert.y;
				num = base.Mp.GAR(base.x, base.y, num2, num3);
			}
			else
			{
				if (this.need_init)
				{
					if (!this.Cs.reset())
					{
						return M2ChaserBaseFuncs.CHSRES.ERROR;
					}
					this.need_init = false;
					for (int i = this.ADepert.Count - 1; i >= 0; i--)
					{
						Vector2 vector = this.ADepert[i];
						this.Cs.setDest(vector.x, vector.y, this.ABuf);
					}
					if (this.warp_flag && !this.Cs.Warp())
					{
						return M2ChaserBaseFuncs.CHSRES.ERROR;
					}
					float x = base.x;
					float y = base.y;
					this.startx = x;
					this.starty = y;
					this.dl("airchaser initted", -1000);
				}
				if (this.Cs.chaseProgress() == M2ChaserBaseFuncs.CHSRES.ERROR)
				{
					return M2ChaserBaseFuncs.CHSRES.ERROR;
				}
				if (this.Cs.depert_defined)
				{
					List<M2ChaserAir.ChaserReachedDepert> depertArray = this.Cs.getDepertArray();
					M2ChaserAir.ChaserReachedDepert chaserReachedDepert2 = depertArray[0];
					if (!this.Dest.isSame(chaserReachedDepert2))
					{
						int num4 = 0;
						int num5 = (int)base.x;
						int num6 = (int)base.y;
						for (M2ChaserRootBase<M2AirRegion> m2ChaserRootBase = chaserReachedDepert2.Last; m2ChaserRootBase != null; m2ChaserRootBase = m2ChaserRootBase.Parent)
						{
							if (m2ChaserRootBase.Region.isContains(num5, num6))
							{
								num4 = X.Abs(num4) + 1;
								break;
							}
							num4--;
						}
						if (num4 <= 0)
						{
							depertArray.RemoveAt(0);
							this.dl("airchaser removed one dest", -1000);
						}
						else
						{
							flag = true;
							this.dest_index = num4;
							this.Dest = chaserReachedDepert2;
							this.dl("airchaser dest defined", this.dest_index);
						}
					}
				}
				if (this.Dest.valid)
				{
					if (flag)
					{
						this.t_throhgh_next = 0f;
					}
					if (this.t_throhgh_next <= 0f && this.dest_index >= 0)
					{
						this.t_throhgh_next = 30f;
						this.Dest.getDepert(this.dest_index - 1, out num2, out num3);
						if (base.Mp.canThroughBcc(base.x, base.y, num2, num3, base.sizex + 0.8f, base.sizey + 0.8f, -1, false, false, null, false, null))
						{
							this.dest_index = ((this.dest_index <= 1) ? (-1) : (this.dest_index - 1));
							this.dl("airchaser skiped", this.dest_index);
							flag = true;
						}
					}
					this.t_throhgh_next -= fcnt;
					for (;;)
					{
						this.Dest.getDepert(this.dest_index, out num2, out num3);
						if (!X.chkLEN(base.x, base.y, num2, num3, 0.1f))
						{
							goto IL_032D;
						}
						if (this.dest_index <= 0)
						{
							break;
						}
						this.dest_index--;
						flag = true;
						this.dl("airchaser pos updated", this.dest_index);
					}
					this.dl("airchaser reached", -1000);
					return M2ChaserBaseFuncs.CHSRES.REACHED;
					IL_032D:
					if (this.dest_index == 0)
					{
						num = CAim.get_agR(CAim.get_aim_r(base.x, -base.y, num2, -num3), 0f);
					}
					else
					{
						num = base.Mp.GAR(base.x, base.y, num2, num3);
					}
					if (flag)
					{
						this.t_throhgh_next = 0f;
						if (this.run_speed > 0f)
						{
							float num7;
							float num8;
							this.Dest.getDepert(this.dest_index, out num7, out num8);
							this.t_error = X.LENGTHXYS(base.x, base.y, num7, num8) / this.run_speed * 2.5f + 50f;
						}
					}
				}
			}
			if (this.fnProgressWalk != null)
			{
				this.fnProgressWalk(this.Cs, this.Dest, new Vector2(num2, num3), num, flag);
			}
			else if (num != -1000f)
			{
				this.Phy.addFoc(FOCTYPE.WALK, X.Cos(num) * this.run_speed, -X.Sin(num) * this.run_speed, -1f, -1, 1, 0, -1, 0);
			}
			bool flag2 = false;
			if (num != -1000f && this.run_speed > 0f && this.Phy.main_updated_count == 0 && X.chkLEN(this.Phy.prex, this.Phy.prey, base.x, base.y, this.run_speed * 0.4f))
			{
				flag2 = true;
			}
			if (flag2 || this.Phy.hit_wall_collider || (num2 != base.x && this.En.wallHitted((base.x < num2) ? AIM.R : AIM.L)) || (num3 != base.y && this.En.wallHitted((base.y < num3) ? AIM.B : AIM.T)))
			{
				this.hitwall_counter.Add(fcnt, false, false);
				if (this.hitwall_counter >= 15f)
				{
					if (this.dest_index > 0 || !this.Dest.valid || X.LENGTHXYS(base.x, base.y, this.startx, this.starty) <= 1.5f)
					{
						return M2ChaserBaseFuncs.CHSRES.ERROR;
					}
					return M2ChaserBaseFuncs.CHSRES.REACHED;
				}
			}
			if (this.t_error > 0f)
			{
				this.t_error -= fcnt;
				if (this.t_error <= 0f)
				{
					if (this.dest_index > 0 || !this.Dest.valid || X.LENGTHXYS(base.x, base.y, this.startx, this.starty) <= 1.5f)
					{
						return M2ChaserBaseFuncs.CHSRES.ERROR;
					}
					return M2ChaserBaseFuncs.CHSRES.REACHED;
				}
			}
			this.hitwall_counter.Update(fcnt);
			if (!this.Dest.valid)
			{
				return M2ChaserBaseFuncs.CHSRES.PROGRESS;
			}
			return M2ChaserBaseFuncs.CHSRES.FOUND;
		}

		public void skipThisDepert()
		{
			List<M2ChaserAir.ChaserReachedDepert> depertArray = this.Cs.getDepertArray();
			if (depertArray.Count > 0)
			{
				depertArray.RemoveAt(0);
				this.Dest = default(M2ChaserAir.ChaserReachedDepert);
			}
		}

		public int getCalcedDepertCound()
		{
			return this.Cs.getDepertArray().Count;
		}

		public bool getCalcedDepert(out M2ChaserAir.ChaserReachedDepert _Dest)
		{
			_Dest = this.Dest;
			return this.Dest.valid;
		}

		public M2ChaserAir.FnGetPointAddition fnGetPointAddition
		{
			get
			{
				return this.Cs.fnGetPointAddition;
			}
			set
			{
				this.Cs.fnGetPointAddition = value;
			}
		}

		public bool depert_defined
		{
			get
			{
				return this.Dest.valid;
			}
		}

		public void dl(string s, int count = -1000)
		{
		}

		public float run_speed = 0.08f;

		private M2AirRegionContainer WRC_Air;

		private M2ChaserAir Cs;

		private List<Vector2> ADepert;

		public float t_throhgh_next;

		private bool need_init;

		public bool warp_flag;

		private M2ChaserAir.ChaserReachedDepert Dest;

		private int dest_index;

		private readonly List<M2AirRegion> ABuf;

		private const float HITWALL_ERROR_MAXT = 15f;

		public float t_error;

		private float startx;

		private float starty;

		public readonly RevCounter hitwall_counter;

		public NASAirChaser.FnProgressWalk fnProgressWalk;

		public delegate bool FnProgressWalk(M2ChaserAir Chaser, M2ChaserAir.ChaserReachedDepert Depert, Vector2 Next, float agR, bool pos_updated);
	}
}
