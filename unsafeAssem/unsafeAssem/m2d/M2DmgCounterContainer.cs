using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2DmgCounterContainer : RBase<M2DmgCounterItem>, IRunAndDestroy, ICameraRenderBinder
	{
		public M2DmgCounterContainer(Map2d _Mp)
			: base(32, true, false, false)
		{
			this.Mp = _Mp;
			this.AEntry = new List<M2DmgCounterContainer.DCEntry>(4);
			this.MtrChar = MTRX.MIicon.getMtr(BLEND.NORMAL, -1);
			this.MtrCharBorder = MTRX.MIicon.getMtr(BLEND.NORMALBORDER8, -1);
			this.FD_drawDmgCounter = new M2DrawBinder.FnEffectBind(this.drawDmgCounter);
			IN.addRunner(this);
		}

		public override void clear()
		{
			base.clear();
			this.draw_delay_ = 0f;
			this.entry_count = 0;
			IN.remRunner(this);
			if (this.Ed != null)
			{
				this.Ed = this.Mp.remED(this.Ed);
			}
		}

		private M2DmgCounterContainer.DCEntry PopEntry()
		{
			while (this.AEntry.Count <= this.entry_count)
			{
				this.AEntry.Add(new M2DmgCounterContainer.DCEntry());
			}
			List<M2DmgCounterContainer.DCEntry> aentry = this.AEntry;
			int num = this.entry_count;
			this.entry_count = num + 1;
			return aentry[num];
		}

		private void ReleaseEntry(M2DmgCounterContainer.DCEntry E)
		{
			int num = this.AEntry.IndexOf(E);
			if (num >= 0 && num < this.entry_count)
			{
				this.AEntry.RemoveAt(num);
				List<M2DmgCounterContainer.DCEntry> aentry = this.AEntry;
				int num2 = this.entry_count - 1;
				this.entry_count = num2;
				aentry.Insert(num2, E);
			}
		}

		public override M2DmgCounterItem Create()
		{
			return new M2DmgCounterItem(this);
		}

		public void MakeAbsorb(M2Attackable Mv, M2Attackable MvAbsorbFrom, int delta_hp, int delta_mp, M2DmgCounterItem.DC et = M2DmgCounterItem.DC.NORMAL)
		{
			for (int i = this.entry_count - 1; i >= 0; i--)
			{
				M2DmgCounterContainer.DCEntry dcentry = this.AEntry[i];
				if (dcentry.Mv == Mv && dcentry.MvAnother == MvAbsorbFrom && (dcentry.hp == 0 || delta_hp == 0) && (dcentry.mp == 0 || delta_mp == 0))
				{
					dcentry.hp += delta_hp;
					dcentry.mp += delta_mp;
					dcentry.et |= et;
					return;
				}
			}
			this.PopEntry().Set(Mv, MvAbsorbFrom, delta_hp, delta_mp, et);
		}

		public void Make(M2Attackable Mv, int delta_hp, int delta_mp, M2DmgCounterItem.DC et = M2DmgCounterItem.DC.NORMAL, bool no_add_new_entry = false)
		{
			for (int i = this.entry_count - 1; i >= 0; i--)
			{
				M2DmgCounterContainer.DCEntry dcentry = this.AEntry[i];
				if (dcentry.Mv == Mv)
				{
					if ((dcentry.hp == 0 || delta_hp == 0) && (dcentry.mp == 0 || delta_mp == 0))
					{
						dcentry.hp += delta_hp;
						dcentry.mp += delta_mp;
						dcentry.et |= et;
						return;
					}
				}
				else if (dcentry.MvAnother == Mv)
				{
					dcentry.addHpa(ref delta_hp);
					dcentry.addMpa(ref delta_mp);
					if (delta_hp == 0 && delta_mp == 0)
					{
						return;
					}
				}
			}
			for (int j = this.LEN - 1; j >= 0; j--)
			{
				M2DmgCounterItem m2DmgCounterItem = this.AItems[j];
				if (m2DmgCounterItem.addAnotherCache(Mv))
				{
					m2DmgCounterItem.addHpa(ref delta_hp);
					m2DmgCounterItem.addMpa(ref delta_mp);
					if (delta_hp == 0 && delta_mp == 0)
					{
						return;
					}
				}
			}
			if (!no_add_new_entry)
			{
				this.PopEntry().Set(Mv, null, delta_hp, delta_mp, et);
			}
		}

		public override bool run(float fcnt)
		{
			if (this.draw_delay_ > 0f)
			{
				this.draw_delay_ -= fcnt;
			}
			else if (X.D_EF && (this.runable >= 2 || (this.runable == 1 && this.Mp.M2D.pre_map_active)))
			{
				int num = this.entry_count;
				if (num > 0)
				{
					fcnt *= (float)(X.AF_EF * X.Mx(1, (int)(this.runable - 1))) * this.TS;
					if (this.LEN == 0)
					{
						if (this.Ed != null)
						{
							this.Ed = this.Mp.remED(this.Ed);
						}
						this.Ed = this.Mp.setED("dmgcounter", this.FD_drawDmgCounter, 0f);
					}
					for (int i = num - 1; i >= 0; i--)
					{
						this.AEntry[i].init(base.Pop(64));
					}
					this.entry_count = 0;
				}
				base.run(fcnt);
			}
			return true;
		}

		public Vector2 reposit(M2DmgCounterItem CI, float mapcx, float mapcy, bool set_ci_pos = false)
		{
			float num = mapcx;
			float num2 = mapcy + CI.map_center_y_shift_checking;
			float num3 = 1f;
			bool flag = CI.Mv is M2MoverPr;
			if (!(CI.Mv is M2MoverPr))
			{
				num3 += 0f;
			}
			float num4 = (flag ? (X.XORSP() * 0.4f) : (X.Mn(CI.Mv.sizex, CI.Mv.sizey) * X.NIXP(0.6f, 0.8f))) * num3;
			float num5 = X.XORSPS() * 3.1415927f;
			float num6 = (flag ? 1.1f : 1.63f);
			float num7 = (flag ? 0.65f : 1.33f);
			num6 *= num3;
			num7 *= num3;
			int count_players = this.Mp.count_players;
			for (int i = 0; i < 30; i++)
			{
				mapcx = num + X.Cos(num5) * num4;
				mapcy = num2 - X.Sin(num5) * num4;
				if (mapcy > CI.Mv.mbottom)
				{
					mapcy = CI.Mv.mbottom - (mapcy - CI.Mv.mbottom);
				}
				bool flag2 = false;
				for (int j = count_players - 1; j >= 0; j--)
				{
					if (this.Mp.getPr(j).isCovering(mapcx - num6, mapcx + num6, mapcy - num7, mapcy + num7, 0f))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					for (int k = this.LEN - 1; k >= 0; k--)
					{
						M2DmgCounterItem m2DmgCounterItem = this.AItems[k];
						if (m2DmgCounterItem != CI && m2DmgCounterItem.isCovering(mapcx, mapcy, CI, num3))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						num = mapcx;
						num2 = mapcy;
						break;
					}
				}
				if (flag || i % 4 == 3)
				{
					num4 += X.NIXP(0.2f, 0.4f) * num3;
				}
				num5 += X.NIXP(0.3f, 0.8f) * 3.1415927f;
			}
			if (set_ci_pos)
			{
				CI.SetPos(num, num2);
			}
			return new Vector2(num, num2);
		}

		public bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
		{
			return true;
		}

		public float getFarLength()
		{
			return 439.875f;
		}

		private bool drawDmgCounter(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.LEN == 0)
			{
				this.Ed = null;
				return false;
			}
			if (this.FnEffectEnable != null && !this.FnEffectEnable())
			{
				return true;
			}
			MeshDrawer meshDrawer = null;
			MeshDrawer meshDrawer2 = null;
			int num = -1;
			for (int i = 0; i < this.LEN; i++)
			{
				M2DmgCounterItem m2DmgCounterItem = this.AItems[i];
				if (!this.mvignore_assigned || !(m2DmgCounterItem.Mv == this.MvIgnore_))
				{
					m2DmgCounterItem.draw(Ef, Ed, ref meshDrawer, ref meshDrawer2, ref num, false);
				}
			}
			return true;
		}

		public void drawDmgCounterForSpecificMv(M2Mover Mv, EffectItem Ef, float dif_scale, float pos_scale, float base_z = -1000f, bool on_ui = false)
		{
			M2Camera cam = Mv.M2D.Cam;
			Mv.Mp.DmgCntCon.drawDmgCounterForSpecificMv(Mv, Ef, cam.x * Mv.Mp.rCLEN, cam.y * Mv.Mp.rCLEN, dif_scale, pos_scale, base_z, on_ui);
		}

		public void drawDmgCounterForSpecificMv(M2Mover Mv, EffectItem Ef, float c_mapx, float c_mapy, float dif_scale, float pos_scale, float base_z = -1000f, bool on_ui = false)
		{
			MeshDrawer meshDrawer = null;
			MeshDrawer meshDrawer2 = null;
			int num = -1;
			for (int i = 0; i < this.LEN; i++)
			{
				M2DmgCounterItem m2DmgCounterItem = this.AItems[i];
				if (m2DmgCounterItem.Mv == Mv)
				{
					m2DmgCounterItem.draw(Ef, null, ref meshDrawer, ref meshDrawer2, ref num, c_mapx, c_mapy, m2DmgCounterItem.x * dif_scale, m2DmgCounterItem.y * dif_scale, pos_scale, base_z, on_ui);
				}
			}
		}

		public float draw_delay
		{
			get
			{
				return this.draw_delay_;
			}
			set
			{
				this.draw_delay_ = X.Mx(this.draw_delay_, value);
			}
		}

		public M2Attackable MvIgnore
		{
			get
			{
				return this.MvIgnore_;
			}
			set
			{
				if (this.MvIgnore == value)
				{
					return;
				}
				this.MvIgnore_ = value;
				this.mvignore_assigned = this.MvIgnore_ != null;
			}
		}

		private M2DrawBinder Ed;

		public readonly Map2d Mp;

		private List<M2DmgCounterContainer.DCEntry> AEntry;

		public readonly Material MtrChar;

		public readonly Material MtrCharBorder;

		public Func<bool> FnEffectEnable;

		private float draw_delay_;

		public byte runable = 1;

		private int entry_count;

		public float TS = 1f;

		private bool mvignore_assigned;

		private M2Attackable MvIgnore_;

		private M2DrawBinder.FnEffectBind FD_drawDmgCounter;

		private class DCEntry
		{
			public M2DmgCounterContainer.DCEntry Set(M2Attackable _Mv, M2Attackable _MvA, int _hp, int _mp, M2DmgCounterItem.DC _et = M2DmgCounterItem.DC.NORMAL)
			{
				this.Mv = _Mv;
				this.MvAnother = _MvA;
				this.hp = _hp;
				this.mp = _mp;
				this.et = _et;
				this.hpa = (this.mpa = 0);
				return this;
			}

			public void addHpa(ref int delta_hp)
			{
				this.addHpa(this.hp, ref this.hpa, ref delta_hp);
			}

			public void addMpa(ref int delta_mp)
			{
				this.addHpa(this.mp, ref this.mpa, ref delta_mp);
			}

			private void addHpa(int hp, ref int hpa, ref int delta_hp)
			{
				if (hp * delta_hp >= 0)
				{
					return;
				}
				int num = ((delta_hp >= 0) ? X.Mn(delta_hp, -hp - hpa) : X.Mx(-hp - hpa, delta_hp));
				hpa += num;
				delta_hp -= num;
			}

			public M2DmgCounterItem init(M2DmgCounterItem T)
			{
				T.Mv = this.Mv;
				T.MvAnother = this.MvAnother;
				T.et = this.et;
				T.hp = this.hp;
				T.mp = this.mp;
				T.hpa = this.hpa;
				T.mpa = this.mpa;
				Vector2 damageCounterShiftMapPos = this.Mv.getDamageCounterShiftMapPos();
				float num = this.Mv.drawx_map + damageCounterShiftMapPos.x;
				float num2 = this.Mv.drawy_map + damageCounterShiftMapPos.y;
				float num3;
				float num4;
				if (this.MvAnother == null)
				{
					num3 = num;
					num4 = num2;
				}
				else
				{
					float drawx_map = this.MvAnother.drawx_map;
					float drawy_map = this.MvAnother.drawy_map;
					num3 = X.NI(num, drawx_map, 0.7f);
					float num5 = X.Mx(this.Mv.sizex * 0.7f, 1.5f);
					num3 = ((drawx_map < num) ? X.Mx(num3, num - num5) : X.Mn(num3, num + num5));
					num4 = X.NI(num2, drawy_map, 0.7f);
					num5 = X.Mx(this.Mv.sizey * 0.7f, 1.5f);
					num4 = ((drawy_map < num2) ? X.Mx(num4, num2 - num5) : X.Mn(num4, num2 + num5));
				}
				T.Con.reposit(T, num3, num4, true);
				return T.FineMv();
			}

			public M2Attackable Mv;

			public M2Attackable MvAnother;

			public M2DmgCounterItem.DC et;

			public int hp;

			public int mp;

			public int hpa;

			public int mpa;
		}
	}
}
