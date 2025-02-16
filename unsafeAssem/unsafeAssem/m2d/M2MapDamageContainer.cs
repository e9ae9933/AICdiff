using System;
using System.Collections.Generic;
using Better;
using XX;

namespace m2d
{
	public sealed class M2MapDamageContainer
	{
		public M2MapDamageContainer()
		{
			this.OAtkData = new BDic<MAPDMG, M2MapDamageContainer.MDIInfo>(1);
			this.PoolDmg = new ClsPool<M2MapDamageContainer.M2MapDamageItem>(() => new M2MapDamageContainer.M2MapDamageItem(), 24);
		}

		public M2MapDamageContainer AssignAtkData(MAPDMG kind, AttackInfo Atk, params M2MapDamageContainer.FnIsMDIUseable[] AfnIsMDIUseable)
		{
			this.OAtkData[kind] = new M2MapDamageContainer.MDIInfo(Atk, AfnIsMDIUseable);
			return this;
		}

		public void clear()
		{
			this.PoolDmg.releaseAll(false, false);
		}

		public M2MapDamageContainer.M2MapDamageItem Release(M2MapDamageContainer.M2MapDamageItem Created)
		{
			return this.PoolDmg.Release(Created);
		}

		public void Release(List<M2MapDamageContainer.M2MapDamageItem> ACreated)
		{
			this.PoolDmg.Release(ACreated);
		}

		public M2MapDamageContainer.M2MapDamageItem Create(MAPDMG kind, float x, float y, float w, float h, M2MapDamageContainer.FnIsMDIUseable Fn = null)
		{
			M2MapDamageContainer.MDIInfo mdiinfo = X.Get<MAPDMG, M2MapDamageContainer.MDIInfo>(this.OAtkData, kind);
			if (mdiinfo == null)
			{
				X.de("不明な MAPDMG : " + kind.ToString(), null);
				return null;
			}
			M2MapDamageContainer.FnIsMDIUseable[] array = mdiinfo.AfnIsMDIUseable;
			if (Fn != null)
			{
				if (array != null)
				{
					X.push<M2MapDamageContainer.FnIsMDIUseable>(ref array, Fn, -1);
				}
				else
				{
					array = new M2MapDamageContainer.FnIsMDIUseable[] { Fn };
				}
			}
			return this.PoolDmg.Pool().Init(kind, mdiinfo.Atk, x, y, w, h, array);
		}

		public M2MapDamageContainer.M2MapDamageItem Create(string s, float x, float y, float w, float h, M2MapDamageContainer.FnIsMDIUseable Fn = null)
		{
			MAPDMG mapdmg = MAPDMG.SPIKE;
			if (TX.valid(s) && s != "_" && !FEnum<MAPDMG>.TryParse(s.ToUpper(), out mapdmg, true))
			{
				X.de("不明なMAPDMG: " + s, null);
			}
			return this.Create(mapdmg, x, y, w, h, Fn);
		}

		public M2MapDamageContainer.M2MapDamageItem Create(string s, DRect Rc, M2MapDamageContainer.FnIsMDIUseable Fn = null)
		{
			return this.Create(s, Rc.x, Rc.y, Rc.w, Rc.h, Fn);
		}

		private BDic<MAPDMG, M2MapDamageContainer.MDIInfo> OAtkData;

		private ClsPool<M2MapDamageContainer.M2MapDamageItem> PoolDmg;

		public delegate AttackInfo FnIsMDIUseable(M2MapDamageContainer.M2MapDamageItem MDI, MAPDMG kind, AttackInfo Atk, M2BlockColliderContainer.BCCLine Bcc, M2Attackable MvA, float efx, float efy);

		public sealed class M2MapDamageItem : DRect
		{
			public AttackInfo Atk { get; private set; }

			public M2MapDamageItem()
				: base("", 0f, 0f, 0f, 0f, 0f)
			{
			}

			public M2MapDamageContainer.M2MapDamageItem Init(MAPDMG _kind, AttackInfo _Atk, float x, float y, float w, float h, M2MapDamageContainer.FnIsMDIUseable[] _AfnIsMDIUseable)
			{
				this.kind = _kind;
				this.ui_fade_key = null;
				this.key = FEnum<MAPDMG>.ToStr(this.kind);
				base.Set(x, y, w, h);
				this.Atk = _Atk;
				this.AfnIsMDIUseable = _AfnIsMDIUseable;
				return this;
			}

			public AttackInfo GetAtk(M2BlockColliderContainer.BCCLine Bcc, M2Attackable MvA, out float efx, out float efy)
			{
				if (Bcc != null)
				{
					Bcc.getNearPointMul(MvA.x + (float)CAim._XD(Bcc.aim, 1) * MvA.sizex, MvA.y - (float)CAim._YD(Bcc.aim, 1) * MvA.sizey, 0.5f, out efx, out efy);
				}
				else
				{
					efx = base.cx;
					efy = base.cy;
				}
				this.Atk.hpdmg_current = this.Atk.hpdmg0;
				this.Atk.mpdmg_current = this.Atk.mpdmg0;
				if (this.AfnIsMDIUseable == null || this.AfnIsMDIUseable.Length == 0)
				{
					return this.Atk;
				}
				MAPDMG mapdmg = this.kind;
				int num = this.AfnIsMDIUseable.Length;
				AttackInfo attackInfo = this.Atk;
				for (int i = 0; i < num; i++)
				{
					attackInfo = this.AfnIsMDIUseable[i](this, mapdmg, attackInfo, Bcc, MvA, efx, efy);
					if (attackInfo == null)
					{
						return null;
					}
				}
				return attackInfo;
			}

			public MAPDMG kind;

			private M2MapDamageContainer.FnIsMDIUseable[] AfnIsMDIUseable;

			public string ui_fade_key;
		}

		private sealed class MDIInfo
		{
			public MDIInfo(AttackInfo _Atk, params M2MapDamageContainer.FnIsMDIUseable[] _AfnIsMDIUseable)
			{
				this.Atk = _Atk;
				this.AfnIsMDIUseable = _AfnIsMDIUseable;
			}

			public AttackInfo Atk;

			public M2MapDamageContainer.FnIsMDIUseable[] AfnIsMDIUseable;
		}
	}
}
