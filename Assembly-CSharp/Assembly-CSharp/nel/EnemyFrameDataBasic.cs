using System;
using PixelLiner;
using XX;

namespace nel
{
	public class EnemyFrameDataBasic
	{
		public EnemyFrameDataBasic(EnemyAnimator Anm, PxlFrame F)
		{
			if (F == null)
			{
				return;
			}
			int num = F.countLayers();
			for (int i = 0; i < num; i++)
			{
				PxlLayer layer = F.getLayer(i);
				this.checkLayerType(Anm, i, layer);
			}
		}

		public bool isSame(EnemyFrameDataBasic Src)
		{
			return this.eye_bits == Src.eye_bits && this.eye_count_pxl == Src.eye_count_pxl && this.eye_count_other == Src.eye_count_other && this.dark_bits == Src.dark_bits && this.mask_layer == Src.mask_layer && this.normal_render_layer == Src.normal_render_layer;
		}

		public void Set(EnemyFrameDataBasic Src)
		{
			this.eye_bits = Src.eye_bits;
			this.eye_count_pxl = Src.eye_count_pxl;
			this.eye_count_other = Src.eye_count_other;
			this.dark_bits = Src.dark_bits;
			this.mask_layer = Src.mask_layer;
			this.normal_render_layer = Src.normal_render_layer;
		}

		public EnemyFrameDataBasic.FD_RES checkLayerType(EnemyAnimator Anm, int i, PxlLayer L)
		{
			bool flag = true;
			if (TX.isStart(L.name, "mask", 0))
			{
				L.alpha = 0f;
				this.mask_layer |= 1U << i;
				return EnemyFrameDataBasic.FD_RES.MASK;
			}
			if (Anm.is_normal_render(L))
			{
				this.normal_render_layer |= 1U << i;
				return EnemyFrameDataBasic.FD_RES.NORMAL;
			}
			if (TX.isStart(L.name, "point_", 0))
			{
				L.alpha = 0f;
				return EnemyFrameDataBasic.FD_RES.POINT;
			}
			if (L.alpha <= 0f)
			{
				return EnemyFrameDataBasic.FD_RES.NOUSE;
			}
			if (L.isImport())
			{
				PxlLayer importSource = L.Img.getImportSource(L);
				if (importSource != null && TX.isStart(importSource.pFrm.pPose.title, "_eye", 0))
				{
					flag = false;
				}
			}
			if (flag && TX.isStart(L.name, "eye", 0))
			{
				flag = false;
			}
			if (flag)
			{
				this.dark_bits |= 1U << i;
				return EnemyFrameDataBasic.FD_RES.DARK;
			}
			this.eye_bits |= 1U << i;
			this.eye_count_pxl++;
			return EnemyFrameDataBasic.FD_RES.EYE;
		}

		public int eye_count_total
		{
			get
			{
				return this.eye_count_pxl + this.eye_count_other;
			}
		}

		public bool isDark(int layer_index)
		{
			return ((ulong)this.dark_bits & (ulong)(1L << (layer_index & 31))) > 0UL;
		}

		public bool isEye(int layer_index)
		{
			return ((ulong)this.eye_bits & (ulong)(1L << (layer_index & 31))) > 0UL;
		}

		public uint eye_bits;

		public int eye_count_pxl;

		public int eye_count_other;

		public uint dark_bits;

		public uint mask_layer;

		public uint normal_render_layer;

		public static EnemyAnimator.FnCreate Create = (EnemyAnimator Anm, PxlFrame F) => new EnemyFrameDataBasic(Anm, F);

		public enum FD_RES : byte
		{
			NOUSE,
			MASK,
			NORMAL,
			POINT,
			EYE,
			DARK
		}
	}
}
