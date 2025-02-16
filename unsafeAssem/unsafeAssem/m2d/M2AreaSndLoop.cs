using System;
using XX;

namespace m2d
{
	public sealed class M2AreaSndLoop : RBase<M2SndLoopItem>
	{
		public M2AreaSndLoop(M2DBase _M2D)
			: base(4, false, true, false)
		{
			this.M2D = _M2D;
		}

		public override M2SndLoopItem Create()
		{
			return new M2SndLoopItem(this);
		}

		public override void clear()
		{
			base.clear();
			this.need_pos_fine = false;
		}

		public float pre_x
		{
			get
			{
				return this.M2D.Snd.pre_x;
			}
		}

		public float pre_y
		{
			get
			{
				return this.M2D.Snd.pre_y;
			}
		}

		public M2SndLoopItem AddLoop(string sname, string unique_key, float cx, float cy, float listenx = 8f, float listeny = 6f, float areax = 0f, float areay = 0f, IPosLitener PosLsn = null)
		{
			if (sname == null)
			{
				return null;
			}
			M2SndLoopItem m2SndLoopItem = null;
			for (int i = 0; i < this.LEN; i++)
			{
				M2SndLoopItem m2SndLoopItem2 = this.AItems[i];
				if (m2SndLoopItem2.clipIs(sname))
				{
					m2SndLoopItem = m2SndLoopItem2;
					break;
				}
			}
			if (m2SndLoopItem == null)
			{
				m2SndLoopItem = base.Pop(4).SetM2(sname);
			}
			m2SndLoopItem.AddArea(unique_key, cx, cy, listenx, listeny, areax, areay, PosLsn);
			return m2SndLoopItem;
		}

		public M2SndLoopItem RemLoop(string sname, string unique_key)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				M2SndLoopItem m2SndLoopItem = this.AItems[i];
				if (m2SndLoopItem.clipIs(sname))
				{
					m2SndLoopItem.removeUniqueKey(unique_key);
					return m2SndLoopItem;
				}
			}
			return null;
		}

		public override bool run(float fcnt)
		{
			if (this.LEN == 0)
			{
				return false;
			}
			base.run(fcnt);
			this.need_pos_fine_item = false;
			return this.LEN > 0;
		}

		public bool need_pos_fine;

		public bool need_pos_fine_item;

		private const int FINE_CNT = 10;

		public readonly M2DBase M2D;
	}
}
