using System;
using UnityEngine;

namespace XX
{
	public class SlideAnim
	{
		public SlideAnim(Transform _Trs, Vector2 SPos, float _dx, float _dy, int _mt = -1, Transform _BasePosTrs = null, SlideAnim.SLIDEANIM _type = SlideAnim.SLIDEANIM.NORMAL)
		{
			if (_mt < 0)
			{
				_mt = 12;
			}
			this.Trs = _Trs;
			this.sx = SPos.x;
			this.sy = SPos.y;
			this.dx = ((_dx == -1000f) ? this.sx : _dx);
			this.dy = ((_dy == -1000f) ? this.sx : _dy);
			this.mt = (float)_mt;
			this.type = _type;
			this.BasePosTrs = _BasePosTrs;
		}

		public bool run(float fcnt = 1f)
		{
			if (this.destructed)
			{
				return false;
			}
			this.t += fcnt;
			float num = ((this.type == SlideAnim.SLIDEANIM.IMMEDIATE) ? 1f : X.ZSIN2(this.t, this.mt));
			float num2 = this.dx;
			float num3 = this.dy;
			if (this.BasePosTrs != null)
			{
				Vector3 position = this.BasePosTrs.position;
				num2 += position.x;
				num3 += position.y;
				if (this.t >= this.mt - 2f && !this.PreBasePosition.Equals(position) && this.mt < 120f)
				{
					this.mt += fcnt;
				}
				this.PreBasePosition = position;
			}
			bool flag = num >= 1f;
			Vector2 vector;
			if (this.type == SlideAnim.SLIDEANIM.VIB)
			{
				num = X.ZSIN(num, 0.18f) - 1.6f * X.ZCOS(num - 0.16f, 0.2f) + 0.6f * X.ZPOW(num - 0.35f, 0.65f);
				vector = new Vector2(X.NAIBUN_I(num2, this.sx, num), X.NAIBUN_I(num3, this.sy, num));
			}
			else
			{
				vector = new Vector2(X.NAIBUN_I(this.sx, num2, num), X.NAIBUN_I(this.sy, num3, num));
			}
			this.Trs.localPosition = vector;
			return !flag;
		}

		public float rest_time()
		{
			return this.mt - this.t;
		}

		public void destruct(bool confirm_to_last = false, bool subtract_from_list = true)
		{
			if (this.destructed)
			{
				return;
			}
			if (confirm_to_last)
			{
				float num = this.dx;
				float num2 = this.dy;
				if (this.BasePosTrs != null)
				{
					Vector3 position = this.BasePosTrs.position;
					num += position.x;
					num2 += position.y;
				}
				this.Trs.localPosition = new Vector2(num, num2);
			}
			if (subtract_from_list && SlideAnim.ASli != null)
			{
				int num3 = X.isinC<SlideAnim>(SlideAnim.ASli, this);
				if (num3 >= 0)
				{
					X.shiftEmpty<SlideAnim>(SlideAnim.ASli, num3, 0, -1);
					SlideAnim.sli_i--;
				}
			}
			this.t = -1000f;
			this.Trs = (this.BasePosTrs = null);
		}

		public bool destructed
		{
			get
			{
				return this.t == -1000f;
			}
		}

		public static int findAnim(Transform Trs)
		{
			for (int i = 0; i < SlideAnim.sli_i; i++)
			{
				if (SlideAnim.ASli[i].Trs == Trs)
				{
					return i;
				}
			}
			return -1;
		}

		public static void runSlideAnimator(float fcnt = 1f)
		{
			int i = 0;
			while (i < SlideAnim.sli_i)
			{
				if (!SlideAnim.ASli[i].run(fcnt))
				{
					X.shiftEmpty<SlideAnim>(SlideAnim.ASli, 1, i, -1);
					SlideAnim.sli_i--;
				}
				else
				{
					i++;
				}
			}
			if (SlideAnim.sli_i == 0)
			{
				SlideAnim.ASli = null;
			}
		}

		public static SlideAnim defineRelease(Transform Trs, bool force = true, float _x = -1000f, float _y = -1000f, Transform BasePosTrs = null)
		{
			if (SlideAnim.ASli == null)
			{
				SlideAnim.ASli = new SlideAnim[16];
			}
			else
			{
				int num = SlideAnim.findAnim(Trs);
				if (num >= 0)
				{
					if (!force)
					{
						return null;
					}
					SlideAnim.ASli[num].destruct(false, false);
					X.shiftEmpty<SlideAnim>(SlideAnim.ASli, 1, num, -1);
					SlideAnim.sli_i--;
				}
			}
			SlideAnim slideAnim = new SlideAnim(Trs, Trs.localPosition, _x, _y, -1, BasePosTrs, SlideAnim.SLIDEANIM.NORMAL);
			X.pushToEmptyS<SlideAnim>(ref SlideAnim.ASli, slideAnim, ref SlideAnim.sli_i, 16);
			if (CURS.isCursBaseObject(Trs))
			{
				CURS.reserveFineFrames(12);
			}
			return slideAnim;
		}

		public static SlideAnim define(Transform Trs, int maxtime, float sx, float sy, float dx = -1000f, float dy = -1000f, Transform BasePosTrs = null, SlideAnim.SLIDEANIM _type = SlideAnim.SLIDEANIM.NORMAL)
		{
			if (SlideAnim.ASli == null)
			{
				SlideAnim.ASli = new SlideAnim[16];
			}
			else
			{
				int num = SlideAnim.findAnim(Trs);
				if (num >= 0)
				{
					SlideAnim.ASli[num].destruct(false, false);
					X.shiftEmpty<SlideAnim>(SlideAnim.ASli, 1, num, -1);
					SlideAnim.sli_i--;
				}
			}
			Vector2 vector;
			if (dx == -1000f)
			{
				dx = sx;
				dy = sy;
				vector = Trs.localPosition;
			}
			else
			{
				vector.x = sx;
				vector.y = sy;
			}
			SlideAnim slideAnim = new SlideAnim(Trs, vector, dx, dy, maxtime, BasePosTrs, _type);
			X.pushToEmptyS<SlideAnim>(ref SlideAnim.ASli, slideAnim, ref SlideAnim.sli_i, 16);
			if (CURS.isCursBaseObject(Trs))
			{
				CURS.reserveFineFrames(maxtime);
			}
			return slideAnim;
		}

		public static bool killRelease(Transform Trs, bool _confirm_to_last = false)
		{
			if (SlideAnim.ASli != null)
			{
				int num = SlideAnim.findAnim(Trs);
				if (num >= 0)
				{
					SlideAnim.ASli[num].destruct(_confirm_to_last, false);
					X.shiftEmpty<SlideAnim>(SlideAnim.ASli, 1, num, -1);
					SlideAnim.sli_i--;
					return true;
				}
			}
			return false;
		}

		public Transform Trs;

		public SlideAnim.SLIDEANIM type;

		public float sx;

		public float sy;

		public float dx;

		public float dy;

		private float t;

		private float mt;

		private Transform BasePosTrs;

		private Vector3 PreBasePosition;

		private const int T_RELEASE = 12;

		public static int sli_i;

		private static SlideAnim[] ASli;

		public enum SLIDEANIM
		{
			IMMEDIATE,
			NORMAL,
			VIB
		}
	}
}
