using System;
using XX;

namespace nel.smnp
{
	public sealed class SmnEnemyKind
	{
		public SmnEnemyKind(string _enemyid, int _def_count, int _splitter_id, float _mp_ratio_min, float _mp_ratio_max, string _zyouken, float _count_add_weight = 0f, float _mp_add_weight = 0f, ENATTR _nattr = ENATTR.NORMAL)
		{
			this.enemyid = _enemyid;
			this.EnemyDesc = NDAT.getTypeAndId(this.enemyid);
			this.def_count = _def_count;
			this.splitter_id = _splitter_id;
			this.mp_ratio_min = _mp_ratio_min;
			this.mp_ratio_max = X.Mx(this.mp_ratio_min, _mp_ratio_max);
			this.mp_ratio_addable_max = 1f;
			this.zyouken = (TX.valid(_zyouken) ? _zyouken : "");
			this.count_add_weight = _count_add_weight;
			this.mp_add_weight = _mp_add_weight;
			this.nattr = _nattr;
			this.smn_xorsp = NightController.XORSP();
		}

		public SmnEnemyKind(SmnEnemyKind Src)
		{
			this.enemyid = Src.enemyid;
			this.EnemyDesc = Src.EnemyDesc;
			this.def_count = Src.def_count;
			this.pre_overdrive = Src.pre_overdrive;
			this.splitter_id = Src.splitter_id;
			this.mp_ratio_min = Src.mp_ratio_min;
			this.mp_ratio_max = Src.mp_ratio_max;
			this.mp_ratio_addable_max = Src.mp_ratio_addable_max;
			this.zyouken = Src.zyouken;
			this.count_add_weight = Src.count_add_weight;
			this.mp_add_weight = Src.mp_add_weight;
			this.temporary_adding_count = Src.temporary_adding_count;
			this.smn_xorsp = NightController.XORSP();
			this.nattr = Src.nattr;
		}

		public bool count_fix
		{
			get
			{
				return this.count_add_weight == 0f;
			}
		}

		public bool isSame(string s)
		{
			if (TX.isStart(s, "OD_", 0))
			{
				s = TX.slice(s, 3);
				if (!this.pre_overdrive)
				{
					return false;
				}
			}
			return NDAT.isSame(this.enemyid, s, false);
		}

		public bool isSame(ENEMYID id)
		{
			if ((id & (ENEMYID)2147483648U) != (ENEMYID)0U)
			{
				id &= (ENEMYID)2147483647U;
				if (!this.pre_overdrive)
				{
					return false;
				}
			}
			ENEMYID enemyid;
			return FEnum<ENEMYID>.TryParse(this.enemyid, out enemyid, true) && id == enemyid;
		}

		public string check_str
		{
			get
			{
				if (this.check_str_ == null)
				{
					this.check_str_ = this.enemyid + (this.thunder_overdrive ? ":T" : "");
				}
				return this.check_str_;
			}
		}

		public bool isOverDrive()
		{
			return this.pre_overdrive || this.thunder_overdrive;
		}

		public float NCXORSP(int val)
		{
			return X.frac((float)val * this.smn_xorsp);
		}

		public string enemyid;

		public NDAT.EnemyDescryption EnemyDesc;

		public int splitter_id;

		public float mp_ratio_min;

		public float mp_ratio_max;

		public float mp_ratio_addable_max;

		public string zyouken = "";

		public float count_add_weight = 1f;

		public float mp_add_weight = 1f;

		public byte mp_added;

		public bool thunder_overdrive;

		public bool pre_overdrive;

		public int def_count;

		public SmnEnemyKind DupeConnect;

		public ENATTR nattr;

		public bool temporary_adding_count;

		public float smn_xorsp;

		private string check_str_;

		public ReelManager.ItemReelDrop SpecialDropReel;

		public int drop_reel;
	}
}
