using System;
using m2d;
using XX;

namespace nel
{
	public class M2LpPuzzTreasure : M2LabelPointEffect, ILinerReceiver, ISfListener
	{
		public M2LpPuzzTreasure(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
			this.Trbox = new NelTreasureBoxDrawer(this, -1f);
			this.set_effect_on_initaction = false;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			this.sf_key = this.Meta.GetS("sf_key");
			string text = this.Meta.GetS("skill") ?? "";
			PrSkill prSkill = null;
			if (TX.valid(text))
			{
				prSkill = SkillManager.Get(text);
				if (prSkill == null)
				{
					X.de("不明なスキル: " + text, null);
				}
				else if (prSkill.first_visible)
				{
					X.de("最初から入手済のスキル: " + text, null);
					prSkill = null;
				}
			}
			this.flush = this.Meta.GetI("flush", 0, 0) != 0;
			if (this.Meta.GetB("no_anchor", false))
			{
				this.no_anchor = true;
			}
			bool b = this.Meta.GetB("no_enable", false);
			string text2 = this.Meta.GetS("type");
			this.money_count = this.Meta.GetI("money", 0, 0);
			string s = this.Meta.GetS("mgkind");
			if (this.Ev != null)
			{
				this.Ev.destruct();
				this.Ev = null;
			}
			this.effect_2_top = false;
			NelItem nelItem = null;
			string text3 = null;
			if (normal_map)
			{
				this.Trbox.getted = COOK.getSF(this.box_sf_key) != 0;
				if (prSkill != null && !this.Trbox.getted && prSkill.visible)
				{
					this.money_count = 150;
					text3 = this.getEventContent(null, this.money_count, 0, null, false);
				}
				if (TX.valid(s))
				{
					if (!FEnum<MGKIND>.TryParse(s.ToUpper(), out this.mgkind, true))
					{
						X.de("MGKIND パースエラー:" + s, null);
					}
					else
					{
						text3 = this.getEventContent("%MAGIC", 0, (int)this.mgkind, null, false);
					}
				}
				if (this.mgkind == MGKIND.NONE && text3 == null)
				{
					if (prSkill != null)
					{
						this.Trbox.getted = prSkill.visible;
						text3 = this.getEventContent(null, 0, 0, prSkill, b);
					}
					else if (this.money_count == 0)
					{
						string s2 = this.Meta.GetS("item");
						if (s2 == "%%MONEY")
						{
							this.money_count = this.Meta.GetI("item", 1, 1);
							text3 = this.getEventContent(null, this.money_count, 0, null, false);
						}
						else
						{
							nelItem = NelItem.GetById(s2, false);
							if (nelItem == null)
							{
								X.de("アイテムキーが不明: " + this.key, null);
							}
							else
							{
								if ((nelItem.is_precious || nelItem.is_enhancer) && this.flush)
								{
									X.de("重要アイテムはflushに設定できません: " + this.key, null);
								}
								int i = this.Meta.GetI("item", 0, 2);
								text3 = this.getEventContent(nelItem.key, this.Meta.GetI("item", 1, 1), i, null, false);
							}
						}
					}
					else
					{
						text3 = this.getEventContent(null, this.money_count, 0, null, false);
					}
				}
				if (!this.Trbox.getted)
				{
					if (text3 != null)
					{
						this.Ev = this.Mp.getEventContainer().CreateAndAssign(this.key);
						float num;
						if ((float)(this.mapy + this.maph) - base.mapfocy > 3f)
						{
							num = base.mapfocy + 2.3f;
						}
						else
						{
							num = base.mapfocy - 0.125f;
						}
						this.Ev.setToArea((float)this.mapx, num, (float)this.mapw, (float)(this.mapy + this.maph) - num);
						this.Ev.event_center_shift_y = base.mapfocy - this.Ev.y;
						this.Ev.assign("CHECK", text3, true);
						this.Ev.check_desc_name = "EV_access_open_box";
					}
					this.PtcAula = new EfParticleLooper("itembox_aula");
					this.PtcAulaKira = new EfParticleLooper("itembox_aula_kira");
					int num2 = this.Meta.GetI("pre_on", -1, 0);
					if (num2 == -1)
					{
						num2 = ((PUZ.IT.isBelongTo(this) != null) ? 0 : 1);
					}
					this.pre_on = num2 > 0;
				}
				else
				{
					this.pre_on = true;
				}
			}
			if (TX.noe(text2) || text2.ToUpper() == "DEACTIVATE")
			{
				if (this.mgkind != MGKIND.NONE)
				{
					text2 = "M2D_MAGIC";
				}
				else if (prSkill != null || (nelItem != null && TX.isStart(nelItem.key, "skillbook_", 0)))
				{
					if ((prSkill.category & SkillManager.SKILL_CTG.SPECIAL) != (SkillManager.SKILL_CTG)0)
					{
						text2 = "FORBIDDEN";
					}
					else if ((prSkill.category & SkillManager.SKILL_CTG.HPMP) == SkillManager.SKILL_CTG.HP)
					{
						text2 = "M2D_SKILL_HP";
					}
					else if ((prSkill.category & SkillManager.SKILL_CTG.HPMP) == SkillManager.SKILL_CTG.MP)
					{
						text2 = "M2D_SKILL_MP";
					}
					else
					{
						text2 = "M2D_SKILL";
					}
				}
				else if (this.money_count > 0)
				{
					text2 = (this.flush ? "M2D_MONEY_FLUSH" : "M2D_MONEY");
				}
				else if (nelItem != null && nelItem.is_enhancer)
				{
					text2 = "M2D_ENHANCER";
				}
				else
				{
					text2 = (this.flush ? "M2D_FLUSH" : "M2D_NORMAL");
				}
			}
			if (!this.no_anchor)
			{
				this.WmIco = new WMIconCreator(this, WMIcon.TYPE.TREASURE, this.box_sf_key);
			}
			FEnum<NelTreasureBoxDrawer.BOXTYPE>.TryParse(text2, out this.boxtype, true);
			if (this.Mp.floort >= 10f)
			{
				this.Trbox.initBright();
			}
			base.setEffect();
			this.SetEnabledBox(this.pre_on, false);
			this.fineSF(false);
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			this.Trbox.quitOpening();
			if (this.Ev != null)
			{
				this.Ev.destruct();
			}
			this.Ev = null;
			if (this.summoner_created)
			{
				this.Lay.LP.Rem(this.key);
			}
		}

		public override void activate()
		{
			this.Trbox.initOpening();
		}

		public override void deactivate()
		{
			this.Trbox.quitOpening();
			if (this.WmIco != null)
			{
				this.WmIco.recheck();
			}
		}

		protected override bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			if (!Ed.isinCamera(base.mapfocx, base.mapfocy, 4f, 6f))
			{
				return true;
			}
			if (this.WmIco != null)
			{
				this.WmIco.notice();
			}
			float openingLevel = this.Trbox.getOpeningLevel();
			if (openingLevel >= 2f)
			{
				return true;
			}
			Ef.x = base.mapfocx;
			Ef.y = base.mapfocy;
			if (this.enabled_box && !this.Trbox.getted && this.PtcAula != null)
			{
				Ef.z = 1f - X.ZLINE(openingLevel - 1f);
				this.PtcAula.Draw(Ef, this.PtcAula.total_delay);
				float kira_open_effect_af = this.Trbox.kira_open_effect_af;
				if (kira_open_effect_af >= 0f)
				{
					float af = Ef.af;
					Ef.af = kira_open_effect_af;
					Ef.z = 40f;
					this.PtcAulaKira.particle_key = "itembox_open_kira";
					this.PtcAulaKira.Draw(Ef, 0f);
				}
				else if (!this.Trbox.isDisappear())
				{
					Ef.z = 10f;
					this.PtcAulaKira.Draw(Ef, this.PtcAulaKira.total_delay);
				}
			}
			this.Trbox.drawOnEffect(Ef, 1f);
			return true;
		}

		public bool fineSF(bool play_effect = true)
		{
			if (this.sf_key != null)
			{
				this.sf_active = COOK.getSF(this.sf_key) != 0;
			}
			else
			{
				this.sf_active = true;
			}
			bool flag = this.sf_active && this.liner_activated == !this.pre_on;
			if (flag != this.enabled_box)
			{
				this.SetEnabledBox(flag, play_effect);
				return true;
			}
			return false;
		}

		public void activateLiner(bool immediate)
		{
			if (this.Meta == null)
			{
				this.initAction(true);
			}
			if (!immediate && this.Ed != null)
			{
				this.Ed.t = 0f;
				this.Ed.f0 = IN.totalframe;
			}
			this.liner_activated = true;
			this.fineSF(true);
		}

		public void deactivateLiner(bool immediate)
		{
			if (this.Meta == null)
			{
				this.initAction(true);
			}
			if (!immediate && this.Ed != null)
			{
				this.Ed.t = 0f;
				this.Ed.f0 = IN.totalframe;
			}
			this.liner_activated = false;
			this.fineSF(true);
		}

		public void SetEnabledBox(bool f, bool play_effect = true)
		{
			if (this.Meta == null)
			{
				this.initAction(true);
			}
			this.enabled_box = f;
			if (!this.Trbox.isDisappear())
			{
				if (f)
				{
					this.Trbox.Set(this.boxtype, false);
					if (play_effect)
					{
						this.Mp.PtcSTsetVar("x", (double)base.mapfocx).PtcSTsetVar("y", (double)base.mapfocy).PtcST("itembox_appear", null, PTCThread.StFollow.NO_FOLLOW);
						this.Trbox.initBright();
					}
				}
				else
				{
					this.Trbox.Set((this.mgkind != MGKIND.NONE) ? NelTreasureBoxDrawer.BOXTYPE.DEACTIVATE_MAGIC : NelTreasureBoxDrawer.BOXTYPE.DEACTIVATE, false);
				}
			}
			if (this.Ev != null)
			{
				this.Ev.setExecutable(M2EventItem.CMD.CHECK, f);
			}
		}

		private string getEventContent(string item_key, int count = 1, int grade = 0, PrSkill Sk = null, bool no_enable = false)
		{
			string text = string.Concat(new string[]
			{
				"#NO_DECLINE_AREA_CAMERA\nDENY_SKIP\nVALOTIZE 1\nHIDE_LOGBOX\nLP_ACTIVATE ",
				this.Lay.name,
				" ",
				this.key,
				"\n",
				NelTreasureBoxDrawer.getEventMoveScript(base.mapfocx * base.CLEN, this.mgkind > MGKIND.NONE, true)
			});
			if (item_key == "%MAGIC")
			{
				text = "STOP_BGM 200 0\n" + text;
				string text2 = text;
				string text3 = "GETMAGIC ";
				MGKIND mgkind = this.mgkind;
				text = text2 + text3 + mgkind.ToString() + "\n";
			}
			else if (Sk != null)
			{
				text = string.Concat(new string[]
				{
					text,
					"GETSKILL ",
					Sk.key,
					" ",
					(no_enable ? 1 : 0).ToString(),
					"\n"
				});
			}
			else if (this.money_count > 0)
			{
				text = text + "GETMONEY_BOX " + this.money_count.ToString() + "\n";
			}
			else
			{
				text = string.Concat(new string[]
				{
					text,
					"GETITEM_BOX ",
					item_key,
					" ",
					count.ToString(),
					" ",
					grade.ToString(),
					"\n"
				});
			}
			text = string.Concat(new string[]
			{
				text,
				"SF_SET ",
				this.box_sf_key,
				" 1\nWAIT_FN ITEMDESC \nLP_DEACTIVATE ",
				this.Lay.name,
				" ",
				this.key,
				"\n#DECLINE_AREA_CAMERA\nCLEAR_TREASURE_BOX_WM_CACHE\nALLOW_SKIP\n#VANISH\n"
			});
			if (this.mgkind != MGKIND.NONE)
			{
				text += "\nSTART_BGM 120";
			}
			string s = this.Meta.GetS("ev_end");
			if (TX.valid(s))
			{
				text = string.Concat(new string[]
				{
					text,
					"\nVALOTIZE 0\nCHANGE_EVENT2 ",
					s,
					" ",
					this.Meta.slice_join("ev_end", 1, " "),
					"\n"
				});
			}
			return text;
		}

		public bool initEffect(bool activating, ref DRect RcEffect)
		{
			RcEffect = new DRect("", base.mapfocx - 1.25f, base.mapfocy - 1.25f, 2.5f, 2.5f, 0f);
			return false;
		}

		public string box_sf_key
		{
			get
			{
				return M2LpPuzzTreasure.getBoxSfKey(this, this.flush);
			}
		}

		public static string getBoxSfKey(M2LabelPoint Lp, bool flush = false)
		{
			return (flush ? "TBOX_FLUSH_" : "TBOX_") + Lp.unique_key;
		}

		public static M2LpPuzzTreasure createTreasureBox(M2MapLayer Lay, string _key, float mapx, float mapy, string item_key, int item_count, MGKIND mgkind = MGKIND.NONE, string add_str = "", NelTreasureBoxDrawer.BOXTYPE boxtype = NelTreasureBoxDrawer.BOXTYPE.DEACTIVATE)
		{
			M2LpPuzzTreasure m2LpPuzzTreasure = new M2LpPuzzTreasure(_key, -1, Lay);
			Lay.LP.reindex();
			m2LpPuzzTreasure.x = (mapx - 1f) * Lay.CLEN;
			m2LpPuzzTreasure.y = (mapy - 0.5f) * Lay.CLEN;
			m2LpPuzzTreasure.height = X.Mx(Lay.Mp.getFootableY(mapx, (int)mapy + 1, 6, false, -1f, false, true, true, 0f), mapy + 2f) * Lay.CLEN - m2LpPuzzTreasure.y;
			m2LpPuzzTreasure.width = 2f * Lay.CLEN;
			m2LpPuzzTreasure.focx = -1f;
			m2LpPuzzTreasure.focy = mapy - m2LpPuzzTreasure.y / Lay.CLEN;
			m2LpPuzzTreasure.finePos();
			if (mgkind != MGKIND.NONE)
			{
				m2LpPuzzTreasure.comment = "mgkind " + mgkind.ToString();
			}
			else
			{
				m2LpPuzzTreasure.comment = "item " + item_key + " " + item_count.ToString();
			}
			M2LpPuzzTreasure m2LpPuzzTreasure2 = m2LpPuzzTreasure;
			m2LpPuzzTreasure2.comment = m2LpPuzzTreasure2.comment + "\ntype " + boxtype.ToString() + "\npre_on 1\n";
			if (TX.valid(add_str))
			{
				m2LpPuzzTreasure.comment = m2LpPuzzTreasure.comment + "\n" + add_str;
			}
			m2LpPuzzTreasure.summoner_created = true;
			Lay.LP.Add(m2LpPuzzTreasure);
			m2LpPuzzTreasure.initAction(true);
			return m2LpPuzzTreasure;
		}

		private bool enabled_box = true;

		private string sf_key;

		private int money_count;

		private NelTreasureBoxDrawer.BOXTYPE boxtype;

		private bool sf_active;

		private bool pre_on = true;

		private bool flush;

		private bool liner_activated;

		private bool no_hint;

		public MGKIND mgkind;

		private M2EventItem Ev;

		private NelTreasureBoxDrawer Trbox;

		private bool no_anchor;

		private bool summoner_created;

		private EfParticleLooper PtcAula;

		private EfParticleLooper PtcAulaKira;

		public const string sf_key_box_flush_header = "TBOX_FLUSH_";

		private const string BOX_MAGIC = "%MAGIC";

		public const string meta_no_calc_whole_map_key = "no_calc_whole_map";

		private WMIconCreator WmIco;
	}
}
