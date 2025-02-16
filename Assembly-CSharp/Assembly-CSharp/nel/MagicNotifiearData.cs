using System;
using System.Collections.Generic;
using Better;
using XX;

namespace nel
{
	public class MagicNotifiearData
	{
		public MagicNotifiearData(BDic<MGKIND, MgFDHolder> OHoldFD)
		{
			this.OMn = new BDic<MGKIND, MagicNotifiear>();
			this.OMn4Mv = new BDic<M2MagicCaster, MagicNotifiearData.MagicNotifiearForMv>();
			this.OMn[MGKIND.FIREBALL] = OHoldFD[MGKIND.FIREBALL].GetNotifiear();
			this.OMn[MGKIND.WHITEARROW] = OHoldFD[MGKIND.WHITEARROW].GetNotifiear();
			this.OMn[MGKIND.DROPBOMB] = OHoldFD[MGKIND.DROPBOMB].GetNotifiear();
			this.OMn[MGKIND.THUNDERBOLT] = OHoldFD[MGKIND.THUNDERBOLT].GetNotifiear();
			this.OMn[MGKIND.POWERBOMB] = OHoldFD[MGKIND.POWERBOMB].GetNotifiear();
			this.OMn[MGKIND.ITEMBOMB_NORMAL] = OHoldFD[MGKIND.ITEMBOMB_NORMAL].GetNotifiear();
		}

		public MagicNotifiear Get(MGKIND name, bool no_error = false)
		{
			MagicNotifiear magicNotifiear = X.Get<MGKIND, MagicNotifiear>(this.OMn, name);
			if (magicNotifiear != null)
			{
				return magicNotifiear;
			}
			if (!no_error)
			{
				X.de("不明な MagicNotifiear :" + name.ToString(), null);
			}
			return null;
		}

		public static MagicNotifiear.TARGETTING fnManipulateTargettingDefault(MagicItem Mg, PR Pr, ref int dx, ref int dy, bool is_first)
		{
			return MagicNotifiear.TARGETTING._AIM_ALL | (Mg.Mn._0.auto_target ? MagicNotifiear.TARGETTING._AUTO_TARGET : ((MagicNotifiear.TARGETTING)0));
		}

		public void initS()
		{
			this.OMn4Mv.Clear();
		}

		public bool isWrongMagic(MagicItem Mg, M2MagicCaster Mv)
		{
			MagicNotifiearData.MagicNotifiearForMv magicNotifiearForMv;
			return !this.OMn4Mv.TryGetValue(Mv, out magicNotifiearForMv) || Mg.killed || magicNotifiearForMv.Mg == null || magicNotifiearForMv.magic_id != Mg.id || magicNotifiearForMv.Mn != Mg.Mn;
		}

		public MagicNotifiear GetForCaster(MagicItem _Mg)
		{
			return this.GetForCaster(_Mg, this.Get(_Mg.kind, false));
		}

		public void RemoveCaster(M2MagicCaster Mv)
		{
			if (Mv != null)
			{
				this.OMn4Mv.Remove(Mv);
			}
		}

		public MagicNotifiear GetForCaster(MagicItem _Mg, MGKIND kind)
		{
			return this.GetForCaster(_Mg, this.Get(kind, false));
		}

		public MagicNotifiear GetForCaster(MagicItem _Mg, MagicNotifiear MnDict)
		{
			M2MagicCaster caster = _Mg.Caster;
			MagicNotifiearData.MagicNotifiearForMv magicNotifiearForMv = null;
			if (!this.OMn4Mv.TryGetValue(caster, out magicNotifiearForMv))
			{
				Dictionary<M2MagicCaster, MagicNotifiearData.MagicNotifiearForMv> omn4Mv = this.OMn4Mv;
				M2MagicCaster m2MagicCaster = caster;
				MagicNotifiearData.MagicNotifiearForMv magicNotifiearForMv2 = new MagicNotifiearData.MagicNotifiearForMv();
				magicNotifiearForMv2.Mg = _Mg;
				magicNotifiearForMv2.magic_id = _Mg.id;
				magicNotifiearForMv2.MnDict = MnDict;
				MagicNotifiearData.MagicNotifiearForMv magicNotifiearForMv3 = magicNotifiearForMv2;
				omn4Mv[m2MagicCaster] = magicNotifiearForMv2;
				magicNotifiearForMv = magicNotifiearForMv3;
				magicNotifiearForMv.Mn = new MagicNotifiear(4);
			}
			else
			{
				magicNotifiearForMv.Mn = new MagicNotifiear(4);
				magicNotifiearForMv.Mg = _Mg;
				magicNotifiearForMv.magic_id = _Mg.id;
				magicNotifiearForMv.MnDict = MnDict;
			}
			_Mg.Mn = magicNotifiearForMv.Mn.CopyFrom(magicNotifiearForMv.MnDict);
			return _Mg.Mn;
		}

		private readonly BDic<MGKIND, MagicNotifiear> OMn;

		private readonly BDic<M2MagicCaster, MagicNotifiearData.MagicNotifiearForMv> OMn4Mv;

		private class MagicNotifiearForMv
		{
			public MagicItem Mg;

			public int magic_id;

			public MagicNotifiear MnDict;

			public MagicNotifiear Mn;
		}
	}
}
