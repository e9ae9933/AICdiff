using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2MvColliderCreatorEn : M2MvColliderCreatorAtk
	{
		public M2MvColliderCreatorEn(NelEnemy _Mv, float _collider_fs_ratio_x = 0.56f, float _collider_fs_ratio_y = 0.4f)
			: base(_Mv)
		{
			this.En = _Mv;
			this.collider_fs_ratio_x = _collider_fs_ratio_x;
			this.collider_fs_ratio_y = _collider_fs_ratio_y;
		}

		public override void setToRect()
		{
			base.setToRect();
			this.collider_fs_ratio_x = 0f;
			this.collider_fs_ratio_y = 0f;
		}

		protected override void recreateExecute()
		{
			if (this.En.is_flying)
			{
				this.collider_foot_slice_px_x = (this.collider_foot_slice_px_y = 0f);
				if (this.CldInner != null)
				{
					this.CldInner.enabled = false;
				}
			}
			else
			{
				if (this.collider_fs_ratio_x >= 0f)
				{
					this.collider_foot_slice_px_x = base.sizex * this.collider_fs_ratio_x * base.CLEN;
				}
				if (this.collider_fs_ratio_y >= 0f)
				{
					this.collider_foot_slice_px_y = base.sizey * this.collider_fs_ratio_y * base.CLEN;
				}
				if (this.GobInnerCollider == null)
				{
					this.GobInnerCollider = IN.CreateGob(base.gameObject, "-inner_collider");
					this.CldInner = this.GobInnerCollider.AddComponent<PolygonCollider2D>();
					this.CldInner.pathCount = 1;
				}
				Vector2 vector = base.transform.localScale;
				float num = base.CLEN * 0.015625f;
				float num2 = base.sizex * num / vector.x;
				float num3 = base.sizey * num / vector.y;
				float num4 = 0.125f * num / vector.x;
				float num5 = 0.8f;
				Vector2 vector2 = ((this.NestTarget != null) ? this.NestTarget.getUShift(base.Mp) : Vector2.zero);
				Vector2[] array = new Vector2[4];
				array[0].Set(vector2.x - num4, vector2.y);
				array[1].Set(vector2.x, vector2.y - num3);
				array[2].Set(vector2.x + num4, vector2.y);
				array[3].Set(vector2.x, vector2.y - num3 + num3 * num5 * 2f);
				this.CldInner.SetPath(0, array);
				this.fineHittingLayerInner();
			}
			base.recreateExecute();
			this.Cld.enabled = true;
		}

		public virtual void fineHittingLayerInner()
		{
			if (this.Mv.isDestructed() || this.GobInnerCollider == null)
			{
				return;
			}
			this.GobInnerCollider.layer = ((base.gameObject.layer != base.Phys.default_layer) ? base.gameObject.layer : ((base.hasFoot() || !this.En.nohit_to_enemy_jumping) ? LayerMask.NameToLayer("EnemySelf") : 2));
			if (!base.hasFoot() && this.En.nohit_to_enemy_jumping)
			{
				this.GobInnerCollider.SetActive(false);
				return;
			}
			this.GobInnerCollider.SetActive(true);
		}

		public bool hasInnerCollider()
		{
			return this.CldInner != null;
		}

		protected readonly NelEnemy En;

		public float collider_fs_ratio_x = 0.56f;

		public float collider_fs_ratio_y = 0.4f;

		private GameObject GobInnerCollider;

		private PolygonCollider2D CldInner;
	}
}
