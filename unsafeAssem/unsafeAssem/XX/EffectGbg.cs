using System;
using UnityEngine;

namespace XX
{
	public class EffectGbg : Effect<EffectItemGbg>
	{
		public EffectGbg(GameObject _Gob)
			: base(_Gob, 240)
		{
		}

		public override Effect<EffectItemGbg> initEffect(string _key, Camera _CameraForMesh, Effect<EffectItemGbg>.FnCreateEffectItem _fnCreateEffectItem, EFCON_TYPE _ef_type = EFCON_TYPE.NORMAL)
		{
			this.useMeshDrawer = false;
			base.initEffect("GBG", _CameraForMesh, _fnCreateEffectItem, _ef_type);
			if (this.BmBuffer == null)
			{
				this.BmBuffer = new BM();
			}
			return this;
		}

		public EffectItem setEGbg(string etype, float _x, float _y, float _z, int _time, int _saf = 0, bool convert_pos = true)
		{
			if (this.LEN >= this.MaxCount)
			{
				if (!this.no_overset_error)
				{
					base.dl("量が多い", true);
				}
				return null;
			}
			if (convert_pos)
			{
				_x = (_x - this.base_pos_x) * this.ppu;
				_y = (_y - this.base_pos_y) * this.ppu;
			}
			return base.setE(new EffectItemGbg(this, etype, _x, _y, _z, _time, _saf));
		}

		public unsafe void runDrawPointer(Color32* _ptr0, int fcnt = 1)
		{
			EffectItemGbg.G = this.BmBuffer;
			this.BmBuffer.setPointer(_ptr0, this.bmw, this.bmh);
			if (this.auto_clear)
			{
				this.BmBuffer.clear();
			}
			this.runDraw((float)fcnt, true);
		}

		public int bmw;

		public int bmh;

		public float base_pos_x;

		public float base_pos_y;

		public bool auto_clear;

		public BM BmBuffer;
	}
}
