using System;
using UnityEngine;
using XX;

namespace m2d
{
	public abstract class M2MvColliderCreator
	{
		public M2MvColliderCreator(M2Mover _Mv)
		{
			this.Mv = _Mv;
			this.Cld = IN.GetOrAdd<PolygonCollider2D>(this.gameObject);
		}

		public M2MvColliderCreator fineRecreate()
		{
			if (!this.Mv.isDestructed())
			{
				this.need_recreate = false;
				this.recreateExecute();
			}
			return this;
		}

		protected abstract void recreateExecute();

		public virtual void destruct()
		{
		}

		public M2DBase M2D
		{
			get
			{
				return this.Mv.M2D;
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.Mv.Mp;
			}
		}

		public float CLEN
		{
			get
			{
				return this.Mv.CLEN;
			}
		}

		public float sizex
		{
			get
			{
				return this.Mv.sizex;
			}
		}

		public float sizey
		{
			get
			{
				return this.Mv.sizey;
			}
		}

		public float x
		{
			get
			{
				return this.Mv.x;
			}
		}

		public float y
		{
			get
			{
				return this.Mv.y;
			}
		}

		public GameObject gameObject
		{
			get
			{
				return this.Mv.gameObject;
			}
		}

		public Transform transform
		{
			get
			{
				return this.Mv.gameObject.transform;
			}
		}

		public M2Phys Phys
		{
			get
			{
				return this.Mv.getPhysic();
			}
		}

		public bool hasFoot()
		{
			return this.Mv.hasFoot();
		}

		public bool enabled
		{
			get
			{
				return this.Cld.enabled;
			}
			set
			{
				this.Cld.enabled = value;
			}
		}

		public bool isTrigger
		{
			get
			{
				return this.Cld.isTrigger;
			}
			set
			{
				this.Cld.isTrigger = value;
			}
		}

		public readonly M2Mover Mv;

		public readonly PolygonCollider2D Cld;

		public bool need_recreate = true;
	}
}
