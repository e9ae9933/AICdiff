using System;
using System.Collections.Generic;
using Better;
using UnityEngine.InputSystem;

namespace XX
{
	public class HoverCursManager
	{
		public HoverCursManager(string _curs_category, string _curs_kind_def = null)
		{
			this.OCt = new BDic<string, HoverCursManager.HvItem>();
			this.curs_category = _curs_category;
			this.curs_kind_def = _curs_kind_def;
		}

		public HoverCursManager Def(string _curs_kind_def = null)
		{
			this.curs_kind_def = _curs_kind_def;
			return this;
		}

		public HoverCursManager Add(string _key, Key _kc, string _curs_kind)
		{
			this.OCt[_key] = new HoverCursManager.HvItem
			{
				key = _key,
				kc = _kc,
				modif = MODIF.ALL,
				curs_kind = _curs_kind
			};
			return this;
		}

		public HoverCursManager Add(string _key, MODIF _md, string _curs_kind, bool slip_another_modifier = false)
		{
			Dictionary<string, HoverCursManager.HvItem> oct = this.OCt;
			string text = "modifier_";
			int num = (int)_md;
			oct[text + num.ToString()] = new HoverCursManager.HvItem
			{
				key = _key,
				modif = _md,
				curs_kind = _curs_kind
			};
			if (slip_another_modifier)
			{
				switch (_md)
				{
				case MODIF.SHIFT:
					this.Add(_key, MODIF.SH_OP, _curs_kind, true);
					this.Add(_key, MODIF.SH_CT, _curs_kind, true);
					this.Add(_key, MODIF.SH_CM, _curs_kind, true);
					break;
				case MODIF.OPT:
					this.Add(_key, MODIF.SH_OP, _curs_kind, true);
					this.Add(_key, MODIF.OP_CT, _curs_kind, true);
					this.Add(_key, MODIF.OP_CM, _curs_kind, true);
					break;
				case MODIF.SH_OP:
					this.Add(_key, MODIF.SH_OP_CT, _curs_kind, true);
					this.Add(_key, MODIF.SH_OP_CM, _curs_kind, true);
					break;
				case MODIF.CTRL:
					this.Add(_key, MODIF.SH_CT, _curs_kind, true);
					this.Add(_key, MODIF.OP_CT, _curs_kind, true);
					this.Add(_key, MODIF.CT_CM, _curs_kind, true);
					break;
				case MODIF.SH_CT:
					this.Add(_key, MODIF.SH_OP_CT, _curs_kind, true);
					this.Add(_key, MODIF.SH_CT_CM, _curs_kind, true);
					break;
				case MODIF.OP_CT:
					this.Add(_key, MODIF.SH_OP_CT, _curs_kind, true);
					this.Add(_key, MODIF.OP_CT_CM, _curs_kind, true);
					break;
				case MODIF.SH_OP_CT:
				case MODIF.SH_OP_CM:
				case MODIF.SH_CT_CM:
				case MODIF.OP_CT_CM:
					this.Add(_key, MODIF.SH_OP_CT_CM, _curs_kind, true);
					break;
				case MODIF.CMD:
					this.Add(_key, MODIF.SH_CM, _curs_kind, true);
					this.Add(_key, MODIF.OP_CM, _curs_kind, true);
					this.Add(_key, MODIF.CT_CM, _curs_kind, true);
					break;
				case MODIF.SH_CM:
					this.Add(_key, MODIF.SH_OP_CM, _curs_kind, true);
					this.Add(_key, MODIF.SH_CT_CM, _curs_kind, true);
					break;
				case MODIF.OP_CM:
					this.Add(_key, MODIF.SH_OP_CM, _curs_kind, true);
					this.Add(_key, MODIF.OP_CT_CM, _curs_kind, true);
					break;
				case MODIF.CT_CM:
					this.Add(_key, MODIF.SH_CT_CM, _curs_kind, true);
					this.Add(_key, MODIF.OP_CT_CM, _curs_kind, true);
					break;
				}
			}
			return this;
		}

		public string checkStateCursManager(bool change_curs = false)
		{
			if (change_curs)
			{
				foreach (KeyValuePair<string, HoverCursManager.HvItem> keyValuePair in this.OCt)
				{
					HoverCursManager.HvItem value = keyValuePair.Value;
					bool flag = false;
					if (value.modif == MODIF.ALL)
					{
						flag = IN.getK(value.kc, -1);
					}
					else if (value.modif != MODIF.ALL)
					{
						if (value.kc != Key.None)
						{
							flag = IN.getKD(value.kc, (int)value.modif);
						}
						else
						{
							flag = IN.getModif((int)value.modif);
						}
					}
					if (flag)
					{
						this.checked_str = value.key;
						if (change_curs)
						{
							this.setCurs(this.checked_str, value.curs_kind);
							CURS.Set(this.curs_category, value.curs_kind);
						}
						return this.checked_str;
					}
				}
				if (this.curs_kind_def != null)
				{
					if (change_curs)
					{
						this.setCurs("", this.curs_kind_def);
					}
					return this.checked_str = this.curs_kind_def;
				}
				this.checked_str = "";
				return this.checked_str;
			}
			if (!(this.checked_str == "") || this.curs_kind_def == null)
			{
				return this.checked_str;
			}
			return this.curs_kind_def;
		}

		private void setCurs(string id, string curs_kind)
		{
			if (this.changed_curs == id)
			{
				return;
			}
			this.changed_curs = id;
			CURS.Set(this.curs_category, curs_kind);
		}

		public void blur()
		{
			if (this.changed_curs != null)
			{
				CURS.Rem(this.curs_category, "");
			}
			this.changed_curs = null;
			this.checked_str = "";
		}

		public readonly string curs_category;

		private string curs_kind_def;

		private string changed_curs;

		private readonly BDic<string, HoverCursManager.HvItem> OCt;

		public string checked_str = "";

		private struct HvItem
		{
			public string key;

			public Key kc;

			public MODIF modif;

			public string curs_kind;
		}
	}
}
