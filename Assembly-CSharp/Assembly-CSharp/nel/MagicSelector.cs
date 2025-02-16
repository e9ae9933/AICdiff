using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class MagicSelector : ActiveSelector
	{
		public static void refineDecideTypeText()
		{
			MagicSelector.pre_decide_type = byte.MaxValue;
		}

		public static void reloadKindData()
		{
			if (MagicSelector.OKindData == null)
			{
				int num = 0;
				MagicSelector.OKindData = new BDic<MGKIND, MagicSelector.KindData>();
				CsvReader csvReader = new CsvReader(Resources.Load<TextAsset>("Data/_magic_kind").text, CsvReader.RegSpace, false);
				MagicSelector.KindData kindData = null;
				while (csvReader.read())
				{
					if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
					{
						string text = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1).ToUpper();
						MGKIND mgkind;
						if (!FEnum<MGKIND>.TryParse(text, out mgkind, true))
						{
							X.de("不明な MGKIND : " + text, null);
							kindData = null;
							continue;
						}
						if (MagicSelector.OKindData.ContainsKey(mgkind))
						{
							X.de("MGKIND 重複: " + mgkind.ToString(), null);
						}
						else
						{
							kindData = (MagicSelector.OKindData[mgkind] = new MagicSelector.KindData(num++));
						}
					}
					if (kindData != null)
					{
						if (csvReader.cmd == "%FLIP")
						{
							kindData.flip = true;
						}
						else if (csvReader.cmd == "%ICON_ID")
						{
							kindData.icon_index = csvReader.Int(1, 0);
						}
						else if (csvReader.cmd == "mp_crystalize")
						{
							kindData.mp_crystalize = csvReader.Nm(1, 0f);
						}
						else if (csvReader.cmd == "reduce_mp")
						{
							kindData.reduce_mp = csvReader.Int(1, 0);
						}
						else if (csvReader.cmd == "casttime")
						{
							kindData.casttime = csvReader.Int(1, 0);
						}
						else if (csvReader.cmd == "crystalize_neutral_ratio")
						{
							kindData.crystalize_neutral_ratio = csvReader.Nm(1, 0f);
						}
						else if (csvReader.cmd == "shotgun_ratio")
						{
							kindData.shotgun_ratio = csvReader.Nm(1, 0f);
						}
						else if (csvReader.cmd == "tired_time_to_super_armor")
						{
							kindData.tired_time_to_super_armor = csvReader.Nm(1, 0f);
						}
						else if (csvReader.cmd == "projectile_power")
						{
							kindData.projectile_power = csvReader.Int(1, 0);
						}
						else if (csvReader.cmd == "mana_drain_lock")
						{
							kindData.mana_drain_lock = csvReader.Nm(1, 0f);
						}
					}
				}
			}
		}

		public static int getKind2IconId(MGKIND k)
		{
			MagicSelector.KindData kindData;
			if (MagicSelector.OKindData.TryGetValue(k, out kindData))
			{
				return kindData.icon_index;
			}
			return -1;
		}

		public static int getReduceMp(MGKIND k)
		{
			MagicSelector.KindData kindData;
			if (MagicSelector.OKindData.TryGetValue(k, out kindData))
			{
				return kindData.reduce_mp;
			}
			return -1;
		}

		public static void initMagic(MagicItem Mg)
		{
			MagicSelector.KindData kindData;
			if (MagicSelector.OKindData.TryGetValue(Mg.kind, out kindData))
			{
				kindData.initMagic(Mg);
			}
		}

		public MagicSelector(PR _Pr, M2PrSkill _Skill)
			: base("MAGSEL", "magsel_init")
		{
			this.Pr = _Pr;
			this.Skill = _Skill;
			int num = 5;
			this.OMaga = new BDic<MagicSelector.MAGA, MagicSelector.Enchant>(num);
			this.OMagaDef = new BDic<MagicSelector.MAGA, MagicSelector.Enchant>(num);
			for (int i = 0; i < num; i++)
			{
				this.OMaga[(MagicSelector.MAGA)i] = new MagicSelector.Enchant();
				this.OMagaDef[(MagicSelector.MAGA)i] = new MagicSelector.Enchant();
			}
		}

		public void newGame()
		{
			this.OMagaDef[MagicSelector.MAGA.NORMAL].kind = MGKIND.WHITEARROW;
			this.OMagaDef[MagicSelector.MAGA.LR].kind = MGKIND.FIREBALL;
			this.OMagaDef[MagicSelector.MAGA.B].kind = MGKIND.DROPBOMB;
			this.OMagaDef[MagicSelector.MAGA.T].kind = MGKIND.THUNDERBOLT;
			this.OMagaDef[MagicSelector.MAGA.BURST].kind = MGKIND.PR_BURST;
			foreach (KeyValuePair<MGKIND, MagicSelector.KindData> keyValuePair in MagicSelector.OKindData)
			{
				keyValuePair.Value.obtain_flag = false;
			}
			this.fineCurrentSelection();
			MagicSelector.refineDecideTypeText();
			this.deactivate();
		}

		private void fineCurrentSelection()
		{
			this.exist_count = 0;
			int num = 5;
			for (int i = 0; i < num; i++)
			{
				MagicSelector.Enchant enchant = this.OMaga[(MagicSelector.MAGA)i];
				enchant.clear();
				MGKIND kind = this.OMagaDef[(MagicSelector.MAGA)i].kind;
				if (kind != MGKIND.NONE && (kind == MGKIND.WHITEARROW || MagicSelector.OKindData[kind].obtain_flag || X.DEBUGALLSKILL))
				{
					enchant.CopyFrom(this.OMagaDef[(MagicSelector.MAGA)i]);
					this.exist_count += ((enchant.kind != MGKIND.NONE) ? 1 : 0);
					if (i == 4)
					{
						PrSkill prSkill = SkillManager.Get("burst");
						if (prSkill != null)
						{
							prSkill.Obtain(false);
						}
					}
				}
			}
		}

		public void setObtainFlag(MGKIND mg, bool flag = true)
		{
			MagicSelector.OKindData[mg].obtain_flag = flag;
			if (mg == MGKIND.PR_BURST)
			{
				PrSkill prSkill = SkillManager.Get("burst");
				if (prSkill != null)
				{
					if (flag)
					{
						prSkill.Obtain(false);
					}
					else
					{
						prSkill.ReleaseObtain();
					}
				}
			}
			this.fineCurrentSelection();
		}

		public bool hasBurst()
		{
			return this.OMaga[MagicSelector.MAGA.BURST].kind > MGKIND.NONE;
		}

		public MagicSelector.KindData getBurst()
		{
			MagicSelector.Enchant enchant = this.OMaga[MagicSelector.MAGA.BURST];
			if (enchant.kind != MGKIND.NONE)
			{
				return MagicSelector.getKindData(enchant.kind);
			}
			return null;
		}

		private void prepareTx()
		{
			bool flag = false;
			if (this.Tx == null)
			{
				this.Tx = new GameObject("Tx-BurstDesc").AddComponent<TextRenderer>();
				this.Tx.html_mode = true;
				this.Tx.size = 14f;
				this.Tx.alignx = ALIGN.CENTER;
				this.Tx.BorderCol(4278190080U);
				this.Tx.Col(MTRX.ColWhite);
				flag = true;
			}
			this.Tx.gameObject.layer = IN.gui_layer;
			this.Tx.use_valotile = true;
			this.Tx.gameObject.SetActive(true);
			this.Tx.alpha = 0f;
			this.fineText(flag);
		}

		public void fineText(bool force = false)
		{
			byte b = (byte)CFG.magsel_decide_type;
			if (force || MagicSelector.pre_decide_type != b)
			{
				MagicSelector.pre_decide_type = b;
				using (STB stb = TX.PopBld(null, 0))
				{
					STB stb2 = stb;
					CFG.MAGSEL_TYPE magsel_decide_type = CFG.magsel_decide_type;
					string text;
					if (magsel_decide_type != CFG.MAGSEL_TYPE.SUBMIT)
					{
						if (magsel_decide_type != CFG.MAGSEL_TYPE.Z)
						{
							text = "<key c/> ";
						}
						else
						{
							text = "<key z/> ";
						}
					}
					else
					{
						text = "<key submit/> ";
					}
					stb2.Add(text);
					stb.AddTxA("Submit", false);
					this.Tx.Txt(stb);
				}
			}
		}

		public void initS()
		{
			this.disablelog_floort = 0f;
		}

		private void drawButton(MeshDrawer Md, MeshDrawer MdIco, MeshDrawer MdMul, float x, float y, float scale, bool aim_right, MagicSelector.MAGA aim, float submit_z = 0f, MagicSelector.MAGA curs = MagicSelector.MAGA.NORMAL, bool is_overcharge_exist = false, bool check_disable = false)
		{
			Md.Col = Md.ColGrd.C;
			bool flag = submit_z <= 0f && aim == curs;
			float num = scale * 256f + (float)(flag ? 8 : 0);
			float num2 = (float)(flag ? 6 : 1);
			Md.Daia3(x, y, num, num, num2, num2, false);
			if (flag)
			{
				float num3 = num + 10f + X.COSIT(24f) * 5.5f;
				Md.Daia3(x, y, num3, num3, 3f, 3f, false);
			}
			if (MdMul != null)
			{
				MdMul.Poly(x, y, num * 1.3f, 0f, 4, 0f, false, 1f, 0f);
			}
			MagicSelector.Enchant enchant = this.OMaga[aim];
			MGKIND kind = enchant.kind;
			MagicSelector.KindData kindData;
			if (!MagicSelector.OKindData.TryGetValue(kind, out kindData))
			{
				return;
			}
			float num4 = 1f;
			if (submit_z > 0f)
			{
				num4 = 0.5f + 0.5f * X.ANMPT(12, 1f);
				if (submit_z < 1f)
				{
					Md.StripedDaia(x, y, scale * 256f, X.ANMPT(24, 1f), 1f - X.ZSIN(submit_z), X.Mx(4f, 16f * scale), false);
				}
			}
			if (flag)
			{
				Md.Col = C32.MulA(Md.ColGrd.C, 0.195f + 0.0625f * X.COSIT(24f));
				Md.StripedDaia(x, y, scale * 256f, X.ANMPT(12, 1f), 0.33f, X.Mx(4f, 16f * scale), false);
			}
			MTRX.cola.Set(Md.ColGrd.C);
			if (check_disable && enchant.current_disable)
			{
				MTRX.cola.Set(2298478591U);
				float num5 = scale * 256f * 0.5f * (flag ? 0.62f : 0.4f);
				float num6 = (float)(flag ? 30 : 24) * scale;
				Md.Col = MTRX.cola.mulA(num4).C;
				Md.Line(x - num5, y - num5, x + num5, y + num5, num6, false, 0f, 0f);
				Md.Line(x - num5, y + num5, x + num5, y - num5, num6, false, 0f, 0f);
			}
			else if (is_overcharge_exist)
			{
				MTRX.cola.Set(4283957152U);
			}
			else if (this.Pr.get_mp() < (float)kindData.reduce_mp)
			{
				MTRX.cola.blend(2294415360U, 0.5f + (1f - X.ZLINE(this.Pr.get_mp(), (float)kindData.reduce_mp)) * 0.5f);
			}
			MdIco.Col = MTRX.cola.mulA(num4).C;
			num = scale * (flag ? 1.125f : 1f);
			MdIco.RotaPF(x, y, num, num, 0f, MTR.AMagicIconL[kindData.icon_index], !aim_right && kindData.flip, false, false, uint.MaxValue, false, 0);
		}

		public void drawWholeButtonsTo(MeshDrawer Md, MeshDrawer MdIco, MeshDrawer MdMul, float x, float y, float scale, bool aim_right, float anmz = 1f, bool draw_burst = true, float submit_z = 0f, MagicSelector.MAGA curs = MagicSelector.MAGA.NORMAL, bool is_overcharge_exist = false, bool check_disable = false)
		{
			float num = 256f * ((anmz >= 1f) ? 1f : (0.4f + X.ZSIN2(anmz) * 0.6f)) * scale;
			Md.ColGrd.Set(Md.Col);
			if (submit_z <= 0f || curs == MagicSelector.MAGA.NORMAL)
			{
				this.drawButton(Md, MdIco, MdMul, x, y, scale, aim_right, MagicSelector.MAGA.NORMAL, submit_z, curs, is_overcharge_exist, check_disable);
			}
			if (submit_z <= 0f || curs == MagicSelector.MAGA.T)
			{
				this.drawButton(Md, MdIco, MdMul, x, y + num, scale, aim_right, MagicSelector.MAGA.T, submit_z, curs, is_overcharge_exist, check_disable);
			}
			if (submit_z <= 0f || curs == MagicSelector.MAGA.B)
			{
				this.drawButton(Md, MdIco, MdMul, x, y - num, scale, aim_right, MagicSelector.MAGA.B, submit_z, curs, is_overcharge_exist, check_disable);
			}
			if (submit_z <= 0f || curs == MagicSelector.MAGA.LR)
			{
				this.drawButton(Md, MdIco, MdMul, x + num * (float)X.MPF(aim_right), y, scale, aim_right, MagicSelector.MAGA.LR, submit_z, curs, is_overcharge_exist, check_disable);
			}
			if (draw_burst && (submit_z <= 0f || curs == MagicSelector.MAGA.BURST))
			{
				this.drawButton(Md, MdIco, MdMul, x - (num / 2f + 16f) * (float)X.MPF(aim_right), y + num * 0.5f, scale, aim_right, MagicSelector.MAGA.BURST, submit_z, curs, is_overcharge_exist, check_disable);
			}
		}

		public void selectInit()
		{
			this.disablelog_floort = 0f;
			float num = (float)((this.exist_count <= 1) ? 0 : CFG.magsel_slow);
			this.stop_time = ((num <= 0f) ? 0 : X.IntR((float)this.Skill.MAGIC_CHANT_DELAY * num));
			this.deactivateEffect(true);
			this.cur_curs = MagicSelector.MAGA._ALL;
			for (int i = 0; i < 4; i++)
			{
				MagicSelector.Enchant enchant = this.OMaga[(MagicSelector.MAGA)i];
				if (enchant.kind == MGKIND.NONE)
				{
					enchant.current_disable = false;
				}
				else
				{
					enchant.current_disable = !SCN.canUseableMagic(enchant.kind);
					if (!enchant.current_disable && this.cur_curs == MagicSelector.MAGA._ALL)
					{
						this.cur_curs = (MagicSelector.MAGA)i;
					}
				}
			}
			if (this.cur_curs == MagicSelector.MAGA._ALL)
			{
				this.cur_curs = MagicSelector.MAGA.NORMAL;
			}
			this.push_a_count = 0f;
			this.input_aim = AIM.T;
			this.CurEnchant.Set(this.OMaga[this.cur_curs]);
			this.is_right = this.Pr.mpf_is_right > 0f;
			if (this.stop_time > 0)
			{
				base.selectInit(this.Pr.Mp.setEDT("magic_selector", this.FD_drawEd, 0f), (float)this.stop_time * 0.125f, (float)(this.stop_time + 65), 1f / num);
			}
			if (CFG.magsel_decide_type != CFG.MAGSEL_TYPE.NORMAL)
			{
				this.prepareTx();
			}
			else if (this.Tx != null)
			{
				this.Tx.gameObject.SetActive(false);
			}
			this.fineCurs();
		}

		public bool isAcceptPushdown()
		{
			if (CFG.magsel_decide_type == CFG.MAGSEL_TYPE.NORMAL)
			{
				return true;
			}
			if (this.CurEnchant.isActive() && this.isSelecting())
			{
				switch (CFG.magsel_decide_type)
				{
				case CFG.MAGSEL_TYPE.SUBMIT:
					return !this.Pr.isSubmitPD(26);
				case CFG.MAGSEL_TYPE.Z:
					return !this.Pr.isAtkPD(26);
				case CFG.MAGSEL_TYPE.C:
					return !this.Pr.isTargettingPD(26);
				}
			}
			return true;
		}

		public void DisableLog()
		{
			if (this.disablelog_floort <= this.Pr.Mp.floort)
			{
				UILogRow uilogRow = UILog.Instance.AddAlertTX("KD_ItemSel_use_disable", UILogRow.TYPE.ALERT_GRAY);
				if (uilogRow != null)
				{
					uilogRow.setIcon(null, uint.MaxValue);
				}
			}
			this.disablelog_floort = this.Pr.Mp.floort + 60f;
			if (this.Pr.isMagicO(0))
			{
				IN.getCurrentKeyAssignObject().clearMagicPushDown(true);
			}
		}

		public bool fineCurrent(MagicSelector.MAGA d, MagicItem Mg, M2PrSkill MPr)
		{
			MagicSelector.Enchant enchant = this.OMaga[d];
			if (d != MagicSelector.MAGA.NORMAL && enchant.kind == MGKIND.NONE)
			{
				d = MagicSelector.MAGA.NORMAL;
				enchant = this.OMaga[d];
			}
			if (SCN.canUseableMagic(enchant.kind))
			{
				if (Mg != null && Mg.kind != enchant.kind)
				{
					MPr.killHoldMagic(false, false);
				}
				this.cur_curs = d;
				this.CurEnchant.Set(enchant);
				return true;
			}
			if (Mg != null)
			{
				this.CurEnchant.Set(Mg.kind);
				return true;
			}
			this.DisableLog();
			return false;
		}

		public bool run(float fcnt, bool force_selection = true, MagicItem Mg = null)
		{
			base.runPE();
			if (!this.CurEnchant.isActive())
			{
				if (Mg != null)
				{
					this.CurEnchant.Set(Mg.kind);
				}
				this.selectInit();
				return true;
			}
			if (this.push_a_count >= 15f)
			{
				return false;
			}
			if (!this.isSelecting())
			{
				if (force_selection)
				{
					this.fineCurs();
				}
				return false;
			}
			this.fineCurs();
			if (this.PeSlow == null)
			{
				return false;
			}
			MagicSelector.Enchant enchant = this.OMaga[this.cur_curs];
			bool flag = enchant.kind != MGKIND.NONE && !enchant.current_disable;
			if (CFG.magsel_decide_type > CFG.MAGSEL_TYPE.NORMAL && flag && this.isSlowActive())
			{
				bool flag2 = false;
				switch (CFG.magsel_decide_type)
				{
				case CFG.MAGSEL_TYPE.SUBMIT:
					flag2 = this.Pr.isSubmitPD(26);
					break;
				case CFG.MAGSEL_TYPE.Z:
					flag2 = this.Pr.isAtkPD(26);
					break;
				case CFG.MAGSEL_TYPE.C:
					flag2 = this.Pr.isTargettingPD(26);
					break;
				}
				if (flag2)
				{
					if (this.Pr.isAtkPD(1) && this.Skill.punch_progressing)
					{
						this.Skill.forcePunchQuit();
					}
					this.selectDelayQuit(false);
					return false;
				}
			}
			if (this.cur_curs > MagicSelector.MAGA.NORMAL && flag)
			{
				this.push_a_count += fcnt;
				if (this.push_a_count >= 15f)
				{
					this.selectDelayQuit(true);
					return false;
				}
			}
			else
			{
				this.push_a_count = 0f;
			}
			return true;
		}

		public bool selectDelayQuit(bool force = false)
		{
			if (!this.isActive())
			{
				return false;
			}
			if (this.push_a_count < 15f || force)
			{
				this.push_a_count = 15f;
				this.drawEffectFinalize(true, false);
				if (this.PeSlow != null)
				{
					this.PeSlow.deactivate(false);
					this.PeSlow = null;
				}
			}
			return true;
		}

		public bool isSlowActive()
		{
			return this.PeSlow != null;
		}

		protected override bool drawEd(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.Pr.x;
			Ef.y = this.Pr.y;
			if (!base.drawEd(Ef, Ed))
			{
				return false;
			}
			MeshDrawer mesh = Ef.GetMesh("magic_selector", MTRX.MtrMeshAdd, false);
			MeshDrawer mesh2 = Ef.GetMesh("magic_selector", MTRX.MtrMeshMul, false);
			MeshDrawer meshImg = Ef.GetMeshImg("magic_selector", MTR.MIiconL, BLEND.ADD, false);
			float scale = this.Pr.M2D.Cam.getScale(true);
			float num = 1f / scale;
			mesh.base_z -= 1f;
			meshImg.base_z -= 1f;
			if (Ed.t > 65536f && this.PeSlow == null)
			{
				float num2 = X.ZLINE(Ed.t - 65536f, 40f);
				if (num2 >= 1f || (this.EdDraw != null && Ed != this.EdDraw))
				{
					return false;
				}
				mesh.Col = mesh.ColGrd.White().C;
				mesh2.Col = mesh2.ColGrd.Set(4288258213U).mulA(1f - num2).C;
				mesh2.ColGrd.setA(0f);
				this.drawWholeButtonsTo(mesh, meshImg, mesh2, 0f, 0f, num * 0.75f, this.is_right, 1f, false, num2, this.cur_curs, this.Pr.Skill.isOverChargeUseable(), true);
			}
			else
			{
				float num3 = X.ZLINE(Ed.t, 12f);
				mesh.Col = mesh.ColGrd.White().mulA(num3).C;
				mesh2.Col = mesh2.ColGrd.Set(4288258213U).mulA(num3).C;
				mesh2.ColGrd.setA(0f);
				this.drawWholeButtonsTo(mesh, meshImg, mesh2, 0f, 0f, num * 0.75f, this.is_right, num3, false, -1f, this.cur_curs, this.Pr.Skill.isOverChargeUseable(), true);
				if (this.Tx != null && this.Tx.gameObject.activeSelf)
				{
					float num4 = -(this.Pr.M2D.Cam.x - Ef.x * this.Pr.CLEN) * this.Pr.Mp.base_scale * scale;
					float num5 = (this.Pr.M2D.Cam.y - Ef.y * this.Pr.CLEN) * this.Pr.Mp.base_scale * scale - 76.8f;
					switch (this.cur_curs)
					{
					case MagicSelector.MAGA.LR:
						num4 += (float)X.MPF(this.is_right) * 0.75f * 256f;
						break;
					case MagicSelector.MAGA.T:
						num5 += 192f;
						break;
					case MagicSelector.MAGA.B:
						num5 -= 192f;
						break;
					}
					this.Tx.transform.position = new Vector3(this.Pr.M2D.ui_shift_x * 0.015625f + num4 * 0.015625f, num5 * 0.015625f, 25f);
					this.Tx.alpha = num3;
				}
			}
			return true;
		}

		public void selectQuit(bool play_snd = true, bool mg_enable = true)
		{
			if (this.Pr.isLO(0))
			{
				this.Pr.setAim(AIM.L, false);
			}
			else if (this.Pr.isRO(0))
			{
				this.Pr.setAim(AIM.R, false);
			}
			else if (this.EdDraw != null)
			{
				this.Pr.setAim(this.is_right ? AIM.R : AIM.L, false);
			}
			this.drawEffectFinalize(play_snd && mg_enable, true);
			if (!mg_enable && play_snd)
			{
				base.playSnd("locked");
			}
			if (this.PeDarken != null)
			{
				this.deactivateEffect(false);
			}
		}

		public void drawEffectFinalize(bool play_snd = true, bool assign = false)
		{
			if (this.EdDraw != null)
			{
				this.is_right = this.Pr.mpf_is_right > 0f;
				if (this.EdDraw.t < 65536f)
				{
					this.EdDraw.t = 65536f;
				}
				else
				{
					play_snd = false;
				}
			}
			else if (assign)
			{
				this.EdDraw = this.Pr.Mp.setEDT("magic_selector", this.FD_drawEd, 0f);
				this.EdDraw.t = 65536f;
			}
			else
			{
				play_snd = false;
			}
			if (this.Tx != null)
			{
				this.Tx.gameObject.SetActive(false);
			}
			if (play_snd)
			{
				base.playSnd("magsel_select");
			}
		}

		public override void deactivate()
		{
			base.deactivate();
			if (this.Tx != null)
			{
				this.Tx.gameObject.SetActive(false);
			}
			this.CurEnchant.clear();
		}

		public bool fineCurs()
		{
			this.input_aim = (this.Pr.isLO(0) ? AIM.L : (this.Pr.isRO(0) ? AIM.R : AIM.T));
			MagicSelector.MAGA maga = ((this.input_aim != AIM.T) ? MagicSelector.MAGA.LR : (this.Pr.isBO(0) ? MagicSelector.MAGA.B : (this.Pr.isTO(0) ? MagicSelector.MAGA.T : MagicSelector.MAGA.NORMAL)));
			if (this.cur_curs != maga && this.OMaga[maga].kind == MGKIND.NONE)
			{
				maga = this.cur_curs;
			}
			bool flag = false;
			if (this.cur_curs != maga)
			{
				flag = true;
				this.cur_curs = maga;
				this.CurEnchant.Set(this.OMaga[this.cur_curs]);
				this.push_a_count = 0f;
			}
			if (this.input_aim != AIM.T && this.EdDraw != null)
			{
				bool flag2 = CAim._XD(this.input_aim, 1) > 0;
				if (flag2 != this.is_right)
				{
					this.is_right = flag2;
					flag = true;
				}
			}
			if (flag)
			{
				base.playSnd("magsel_change_target");
			}
			return this.cur_curs > MagicSelector.MAGA.NORMAL;
		}

		public bool isActive()
		{
			return this.CurEnchant.kind > MGKIND.NONE;
		}

		public bool isSelecting()
		{
			return this.EdDraw != null && this.EdDraw.t < (float)this.stop_time;
		}

		public MagicSelector fineLR()
		{
			this.is_right = !this.Pr.isLO(0) && (this.Pr.isRO(0) || this.Pr.mpf_is_right > 0f);
			this.Pr.setAim(this.is_right ? AIM.R : AIM.L, false);
			return this;
		}

		public MGKIND GetCurent()
		{
			return this.CurEnchant.kind;
		}

		public MagicSelector.MAGA get_cursor()
		{
			return this.cur_curs;
		}

		public static MagicSelector.KindData getKindData(MGKIND Mk)
		{
			return X.Get<MGKIND, MagicSelector.KindData>(MagicSelector.OKindData, Mk);
		}

		public static bool isObtained(MGKIND Mk)
		{
			return MagicSelector.OKindData[Mk].obtain_flag;
		}

		public MagicSelector.Enchant getDataAt(MagicSelector.MAGA curs)
		{
			return this.OMaga[curs];
		}

		public string getEffectAfForDebug()
		{
			if (this.EdDraw == null)
			{
				return "";
			}
			return " (E:" + this.EdDraw.t.ToString() + ")";
		}

		public void readBinaryFrom(ByteReader Ba, int vers_noel)
		{
			if (Ba.readByte() >= 1)
			{
				int num = Ba.readByte();
				for (int i = 0; i < num; i++)
				{
					ushort num2 = Ba.readUShort();
					try
					{
						MGKIND mgkind = (MGKIND)num2;
						MagicSelector.KindData kindData;
						if (MagicSelector.OKindData.TryGetValue(mgkind, out kindData))
						{
							if (vers_noel >= 4 || mgkind - MGKIND.FIREBALL > 1)
							{
								kindData.obtain_flag = true;
							}
						}
					}
					catch
					{
					}
				}
				this.fineCurrentSelection();
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(1);
			int num = 0;
			foreach (KeyValuePair<MGKIND, MagicSelector.KindData> keyValuePair in MagicSelector.OKindData)
			{
				num += (keyValuePair.Value.obtain_flag ? 1 : 0);
			}
			Ba.writeByte(num);
			foreach (KeyValuePair<MGKIND, MagicSelector.KindData> keyValuePair2 in MagicSelector.OKindData)
			{
				if (keyValuePair2.Value.obtain_flag)
				{
					Ba.writeUShort((ushort)keyValuePair2.Key);
				}
			}
		}

		public readonly PR Pr;

		public readonly M2PrSkill Skill;

		private TextRenderer Tx;

		private static byte pre_decide_type = byte.MaxValue;

		private const float Z_TX = 25f;

		private BDic<MagicSelector.MAGA, MagicSelector.Enchant> OMaga;

		private BDic<MagicSelector.MAGA, MagicSelector.Enchant> OMagaDef;

		private static BDic<MGKIND, MagicSelector.KindData> OKindData;

		public int exist_count;

		public const float DEF_WH = 256f;

		private MagicSelector.MAGA cur_curs;

		private readonly MagicSelector.Enchant CurEnchant = new MagicSelector.Enchant();

		private AIM input_aim = AIM.T;

		private float push_a_count;

		private bool is_right;

		private float disablelog_floort;

		private int stop_time;

		public class KindData
		{
			public KindData(int _index)
			{
				this.index = _index;
				this.icon_index = _index;
			}

			public void initMagic(MagicItem Mg)
			{
				Mg.reduce_mp = (float)this.reduce_mp;
				Mg.mp_crystalize = this.mp_crystalize;
				Mg.casttime = (float)this.casttime;
				Mg.crystalize_neutral_ratio = this.crystalize_neutral_ratio;
				Mg.projectile_power = this.projectile_power;
				Mg.Atk0.knockback_len = this.knockback_len;
				Mg.Atk0.tired_time_to_super_armor = this.tired_time_to_super_armor;
			}

			public int index;

			public int icon_index;

			public bool flip;

			public bool obtain_flag;

			public int reduce_mp;

			public float mp_crystalize;

			public int casttime;

			public float crystalize_neutral_ratio;

			public float tired_time_to_super_armor;

			public float shotgun_ratio = 1.5f;

			public int projectile_power = 100;

			public float knockback_len;

			public float mana_drain_lock = 5f;
		}

		public class Enchant
		{
			public MagicSelector.Enchant CopyFrom(MagicSelector.Enchant Maga)
			{
				this.kind = Maga.kind;
				return this;
			}

			public MagicSelector.Enchant clear()
			{
				this.kind = MGKIND.NONE;
				return this;
			}

			public bool isActive()
			{
				return this.kind > MGKIND.NONE;
			}

			public MagicSelector.Enchant Set(MagicSelector.Enchant Src)
			{
				this.kind = Src.kind;
				this.current_disable = Src.current_disable;
				return this;
			}

			public MagicSelector.Enchant Set(MGKIND _kind)
			{
				this.kind = _kind;
				this.current_disable = false;
				return this;
			}

			public MGKIND kind;

			public bool current_disable;
		}

		public enum MAGA
		{
			NORMAL,
			LR,
			T,
			B,
			BURST,
			_ALL
		}
	}
}
