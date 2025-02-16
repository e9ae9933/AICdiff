using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2CImgDrawerMorusSignal : M2CImgDrawer
	{
		public M2CImgDrawerMorusSignal(MeshDrawer Md, int _lay, M2Puts _Cp, META Meta)
			: base(Md, _lay, _Cp, true)
		{
			this.char_index = 0;
			this.fine_delay = 30f;
			this.morus_str = Meta.GetSi(1, "morus_signal").ToUpper();
			if (M2CImgDrawerMorusSignal.AMorus == null)
			{
				M2CImgDrawerMorusSignal.AMorus = new ushort[]
				{
					18, 8465, 8481, 529, 1, 4385, 545, 4369, 17, 4642,
					530, 4625, 34, 33, 546, 4641, 8722, 289, 273, 2,
					274, 4370, 290, 8466, 8482, 8721
				};
				M2CImgDrawerMorusSignal.AMorusNum = new uint[] { 139810U, 74274U, 70178U, 69922U, 69906U, 69905U, 135441U, 139537U, 139793U, 139809U };
			}
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			this.setVisible(false);
			return true;
		}

		public override int redraw(float fcnt)
		{
			int num = 0;
			bool flag = false;
			if (this.visible_)
			{
				if (base.Mp.floort >= this.hide_delay)
				{
					flag = this.setVisible(false);
				}
			}
			else if (base.Mp.floort >= this.fine_delay)
			{
				while (this.char_index < this.morus_str.Length)
				{
					char c = this.morus_str[this.char_index];
					int num2 = (int)(c - 'A');
					if (X.BTW(0f, (float)num2, (float)M2CImgDrawerMorusSignal.AMorus.Length))
					{
						flag = this.defineBlinkPat((uint)M2CImgDrawerMorusSignal.AMorus[num2]) || flag;
					}
					else
					{
						if (!X.BTW(48f, (float)c, 57f))
						{
							this.bit_index = 0;
							this.char_index++;
							this.fine_delay = base.Mp.floort + 48f;
							goto IL_0124;
						}
						num2 = (int)(c - '0');
						flag = this.defineBlinkPat(M2CImgDrawerMorusSignal.AMorusNum[num2]) || flag;
					}
				}
				this.fine_delay = base.Mp.floort + 72f;
				this.char_index = (this.bit_index = 0);
			}
			IL_0124:
			if (flag)
			{
				num |= base.layer2update_flag;
			}
			return base.redraw(fcnt) | num;
		}

		private bool defineBlinkPat(uint m_all)
		{
			int num = (int)((float)X.beki_cnt(m_all) / 4f) + 1;
			if (this.bit_index >= num)
			{
				this.bit_index = 0;
				this.char_index++;
				this.fine_delay = base.Mp.floort + 16f;
				return false;
			}
			float num2 = (float)((((m_all >> (num - 1 - this.bit_index) * 4) & 3U) == 1U) ? 8 : 24);
			bool flag = this.setVisible(true);
			this.hide_delay = base.Mp.floort + num2;
			this.fine_delay = this.hide_delay + 8f;
			this.bit_index++;
			return flag;
		}

		private bool setVisible(bool value)
		{
			if (this.visible_ == value)
			{
				return false;
			}
			this.visible_ = value;
			Color32[] colorArray = this.Md.getColorArray();
			int length = base.Length;
			Color32 color = (this.visible_ ? base.Lay.LayerColor.C : MTRX.ColTrnsp);
			int index_first = this.index_first;
			for (int i = 0; i < length; i++)
			{
				colorArray[index_first++] = color;
			}
			this.Cp.light_alpha = (float)(value ? 1 : 0);
			return true;
		}

		private bool visible_ = true;

		private string morus_str;

		private int char_index;

		private int bit_index;

		private float hide_delay;

		private float fine_delay;

		private static ushort[] AMorus;

		private static uint[] AMorusNum;

		private const string key_list = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	}
}
