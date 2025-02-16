using System;
using System.Text.RegularExpressions;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public class PrSkill
	{
		public PrSkill(string _key, ushort _id)
		{
			this.key = _key;
			this.id = _id;
			SkillManager.SKILL_TYPE skill_TYPE;
			if (FEnum<SkillManager.SKILL_TYPE>.TryParse(_key.ToLower(), out skill_TYPE, true))
			{
				this.skill_type_bit = 1UL << (int)skill_TYPE;
			}
		}

		public string title
		{
			get
			{
				WholeMapItem mapForWhole = this.MapForWhole;
				if (mapForWhole != null)
				{
					return TX.GetA("SkillItem_Map_title", mapForWhole.localized_name);
				}
				return TX.Get("Skill_title_" + this.desc_key, "");
			}
		}

		public string tutorial_desc
		{
			get
			{
				if (this.is_sp_map)
				{
					return "";
				}
				return TX.Get("Skill_title_" + this.desc_key, "") + ": " + TX.Get("Skill_manipulate_" + this.desc_key, "");
			}
		}

		public string manipulate
		{
			get
			{
				if (this.is_sp_map)
				{
					return "";
				}
				return TX.Get("Skill_manipulate_" + this.desc_key, "");
			}
		}

		public string desc_key
		{
			get
			{
				return this.desc_key_replace ?? this.key;
			}
		}

		public string descript
		{
			get
			{
				WholeMapItem mapForWhole = this.MapForWhole;
				if (mapForWhole != null)
				{
					return TX.GetA("SkillItem_Map_desc", mapForWhole.localized_name);
				}
				return TX.Get("Skill_desc_" + this.desc_key, "");
			}
		}

		public bool is_sp_map
		{
			get
			{
				return (this.category & SkillManager.SKILL_CTG.SPECIAL) != (SkillManager.SKILL_CTG)0 && TX.isStart(this.key, "sp_map_", 0);
			}
		}

		public WholeMapItem MapForWhole
		{
			get
			{
				if (this.is_sp_map)
				{
					NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
					if (nelM2DBase != null && nelM2DBase.WM != null)
					{
						return nelM2DBase.WM.GetByTextKey(TX.slice(this.key, "sp_map_".Length));
					}
				}
				return null;
			}
		}

		public PrSkill Obtain(bool do_not_enable = false)
		{
			if (!this.first_visible && !this.visible)
			{
				this.visible = (this.new_icon = true);
				if (!do_not_enable)
				{
					this.enabled = true;
					(M2DBase.Instance as NelM2DBase).getPrNoel().Skill.resetSkillConnection(false, true, true);
				}
				else
				{
					this.enabled = false;
				}
				WholeMapItem mapForWhole = this.MapForWhole;
				if (mapForWhole != null)
				{
					mapForWhole.item_revealed |= WholeMapItem.REVEALED.MAP;
				}
			}
			return this;
		}

		public PrSkill ReleaseObtain()
		{
			if (!this.first_visible && this.visible)
			{
				this.visible = (this.enabled = (this.new_icon = false));
				(M2DBase.Instance as NelM2DBase).getPrNoel().Skill.resetSkillConnection(false, false, false);
			}
			return this;
		}

		public bool validate()
		{
			bool flag = true;
			if (TX.getTX("Skill_title_" + this.desc_key, true, false, null) == null)
			{
				X.de("PrSkill: テキスト Skill_title_" + this.desc_key + " がありません", null);
				flag = false;
			}
			if (TX.getTX("Skill_manipulate_" + this.desc_key, true, false, null) == null)
			{
				X.de("PrSkill: テキスト Skill_manipulate_" + this.desc_key + " がありません", null);
				flag = false;
			}
			if (TX.getTX("Skill_desc_" + this.desc_key, true, false, null) == null)
			{
				X.de("PrSkill: テキスト Skill_desc_" + this.desc_key + " がありません", null);
				flag = false;
			}
			if (this.getPF() == null)
			{
				X.de("PrSkill: nel_skill ポーズ内にフレーム " + this.key + " がありません", null);
				flag = false;
			}
			return flag;
		}

		public void Show(bool auto_enable = true)
		{
			if (this.visible)
			{
				return;
			}
			this.visible = true;
			if (auto_enable)
			{
				this.enabled = true;
			}
		}

		public bool isUseable(PR Pr)
		{
			return (X.DEBUGALLSKILL && this.id < 30000) || ((X.DEBUGALLSKILL || this.visible) && this.enabled);
		}

		public NelItem GetBookItem()
		{
			return NelItem.GetById("skillbook_" + this.key, false);
		}

		public PxlFrame getPF()
		{
			return SkillManager.PPose.getSequence(0).getFrameByName(this.desc_key);
		}

		public PxlFrame getThumbPF()
		{
			return SkillManager.PPose.getSequence(0).getFrameByName(this.desc_key + "_thumb") ?? this.getPF();
		}

		public readonly string key;

		public SkillManager.SKILL_CTG category;

		public readonly ulong skill_type_bit;

		public bool always_enable;

		public bool first_visible;

		public ushort id;

		public string desc_key_replace;

		public string map_at;

		public bool visible;

		public bool enabled;

		public bool new_icon = true;

		public static Regex RegHpGain = new Regex("^hp(\\d+)");

		public static Regex RegMpGain = new Regex("^mp(\\d+)");
	}
}
