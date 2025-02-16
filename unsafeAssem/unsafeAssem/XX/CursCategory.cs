using System;
using UnityEngine;

namespace XX
{
	public class CursCategory
	{
		public CursCategory(string _key)
		{
			this.key = _key;
		}

		public void defineLoc(float _x = -1000f, float _y = -1000f, Transform _BasePosTrs = null, int _anim_t = -1)
		{
			this.mvx = _x;
			this.mvy = _y;
			this.BasePosTrs = _BasePosTrs;
			if (_anim_t >= 0)
			{
				this.anim_mt = _anim_t;
			}
		}

		public float defineAnimation(SlideAnim.SLIDEANIM animpat = SlideAnim.SLIDEANIM.NORMAL)
		{
			int num = this.isAnimatingByAnother();
			if (this.mvx == -1000f)
			{
				return (float)num;
			}
			Transform baseTransform = CURS.getBaseTransform();
			if (!baseTransform)
			{
				return 0f;
			}
			if (CursCategory.CursAnim != null)
			{
				CursCategory.CursAnim.destruct(false, true);
			}
			CursCategory.CursAnim = null;
			SlideAnim.killRelease(baseTransform, false);
			bool flag = animpat == SlideAnim.SLIDEANIM.IMMEDIATE;
			if (this.anim_mt <= 0 || flag)
			{
				baseTransform.localPosition = this.getAnimationDep();
				return (float)X.Mx(0, num);
			}
			CursCategory.CursAnim = SlideAnim.define(baseTransform, this.anim_mt, this.mvx, this.mvy, -1000f, -1000f, this.BasePosTrs, animpat);
			return (float)X.Mx(1, num);
		}

		public float defineVibrateAnimation(AIM a)
		{
			this.anim_mt = 15;
			float num = this.defineAnimation(SlideAnim.SLIDEANIM.VIB);
			if (CursCategory.CursAnim != null && CursCategory.CursAnim.type == SlideAnim.SLIDEANIM.VIB)
			{
				Vector2 animationDep = this.getAnimationDep();
				CursCategory.CursAnim.sx = animationDep.x + 0.15625f * (float)CAim._XD(a, 1);
				CursCategory.CursAnim.sy = animationDep.y + 0.15625f * (float)CAim._YD(a, 1);
			}
			return num;
		}

		public Vector2 getAnimationDep()
		{
			CursCategory.BfPt.x = this.mvx;
			CursCategory.BfPt.y = this.mvy;
			if (this.BasePosTrs != null)
			{
				Vector3 position = this.BasePosTrs.position;
				CursCategory.BfPt.x = CursCategory.BfPt.x + position.x;
				CursCategory.BfPt.y = CursCategory.BfPt.y + position.y;
			}
			return CursCategory.BfPt;
		}

		public bool is_animating
		{
			get
			{
				return this.mvx != -1000f;
			}
		}

		public int isAnimatingByAnother()
		{
			return 0;
		}

		public bool refineAnimDep(Transform Trs = null)
		{
			if (Trs == null)
			{
				Trs = CURS.getBaseTransform();
			}
			float num = this.mvx;
			float num2 = this.mvy;
			if (this.anim_mt <= 0 || CursCategory.CursAnim == null)
			{
				Trs.position = new Vector3(num, num2, -9.5f);
			}
			else if (CursCategory.CursAnim.rest_time() <= 1f)
			{
				Trs.position = new Vector3(num, num2, -9.5f);
				CursCategory.CursAnim = null;
			}
			else
			{
				CursCategory.CursAnim.dx = num;
				CursCategory.CursAnim.dy = num2;
			}
			return true;
		}

		public readonly string key;

		public int priority;

		public float mvx = -1000f;

		public float mvy = -1000f;

		public int anim_mt;

		private Transform BasePosTrs;

		private static SlideAnim CursAnim;

		private static Vector2 BfPt;
	}
}
