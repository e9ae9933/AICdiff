using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NelChipManaWeed : NelChip
	{
		public NelChipManaWeed(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			base.arrangeable = true;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.Mw != null)
			{
				return;
			}
			if (this.GobCld != null)
			{
				IN.DestroyOne(this.GobCld);
			}
			this.GobCld = this.Mp.createMoverGob<M2ManaWeed>("-mana_weed-" + this.index.ToString(), this.mapcx, this.collider_cenpos_mapy, false);
			this.GobCld.layer = LayerMask.NameToLayer("Ignore Raycast");
			BoxCollider2D boxCollider2D;
			if (IN.GetOrAdd<BoxCollider2D>(this.GobCld, out boxCollider2D))
			{
				this.Mw = this.GobCld.AddComponent<M2ManaWeed>();
				this.is_active = true;
			}
			else
			{
				this.Mw = this.GobCld.GetComponent<M2ManaWeed>();
			}
			boxCollider2D.size = new Vector2((float)this.iwidth * this.cld_mapw_scale * this.Mp.base_scale * 0.015625f, this.cld_maph * this.Mp.CLENB * 0.015625f);
			this.Mw.appear(this, this.Mp);
			this.BccChecker = new M2BCCPosCheckerCp(this, null);
		}

		public float collider_cenpos_mapy
		{
			get
			{
				return this.mbottom - this.cld_maph * 0.5f;
			}
		}

		public override void translateByChipMover(float ux, float uy, C32 AddCol, int drawx0, int drawy0, int move_drawx = 0, int move_drawy = 0, bool stabilize_move_map = false)
		{
			base.translateByChipMover(ux, uy, AddCol, drawx0, drawy0, move_drawx, move_drawy, stabilize_move_map);
			if (this.Mw != null)
			{
				this.Mw.finePos(ux, uy);
			}
		}

		protected override void initActiveRemoveKey()
		{
			if (this.GobCld != null)
			{
				IN.DestroyOne(this.GobCld);
				this.GobCld = null;
				this.Mw = null;
			}
			base.initActiveRemoveKey();
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			if (this.GobCld != null)
			{
				IN.DestroyOne(this.GobCld);
				this.GobCld = null;
				this.Mw = null;
			}
		}

		public M2BlockColliderContainer.BCCLine GetBcc()
		{
			return this.BccChecker.Get();
		}

		public bool isActive()
		{
			return this.Mw != null && this.Mw.isActive();
		}

		public float sizex
		{
			get
			{
				return (float)this.iwidth * this.cld_mapw_scale * 0.5f * this.Mp.rCLEN;
			}
		}

		public float sizey
		{
			get
			{
				return this.cld_maph * 0.5f;
			}
		}

		private float cld_maph = 1.6f;

		private float cld_mapw_scale = 1.2f;

		private bool is_active = true;

		private float grow_anim_t;

		private const float GROW_ANIM_MAXT = 24f;

		private M2ManaWeed Mw;

		private GameObject GobCld;

		private M2DrawBinder Ed;

		private M2BCCPosCheckerCp BccChecker;
	}
}
