using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2LpBreakable : NelLp, IPuzzRevertable
	{
		public M2LpBreakable(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
			this.FD_fnDrawChips = new M2DropObject.FnDropObjectDraw(this.fnDrawChips);
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.APuts = null;
			this.Meta = new META(this.comment);
			this.HiddenLayer = this.Mp.getLayer(this.Meta.GetS("hidden_layer"));
			if (this.HiddenLayer == this.Lay)
			{
				X.de("hidden_layer で Lp と同じレイヤーが指定されている", null);
				this.HiddenLayer = null;
			}
			if (this.HiddenLayer != null)
			{
				this.HiddenLayer.is_chip_arrangeable = true;
			}
			int mapx = this.mapx;
			int mapw = this.mapw;
			int mapy = this.mapy;
			int maph = this.maph;
			this.RcCpExist = new DRect("_");
			this.prepareChips();
		}

		protected virtual void prepareChips()
		{
			for (int i = 0; i < this.mapw; i++)
			{
				for (int j = 0; j < this.maph; j++)
				{
					M2Pt pointPuts = this.Mp.getPointPuts(this.mapx + i, this.mapy + j, false, false);
					if (pointPuts != null && pointPuts.count != 0)
					{
						int count = pointPuts.count;
						for (int k = 0; k < count; k++)
						{
							M2Puts m2Puts = pointPuts[k];
							if (m2Puts.Lay == this.Lay && base.isContainingMapXy((float)m2Puts.mapx, (float)m2Puts.mapy, (float)((int)m2Puts.mright), (float)((int)m2Puts.mbottom), 0f))
							{
								if (this.APuts == null)
								{
									this.APuts = new List<M2Puts>();
								}
								this.APuts.Add(m2Puts);
								this.RcCpExist.Expand(m2Puts.mleft, m2Puts.mtop, m2Puts.mright - m2Puts.mleft, m2Puts.mbottom - m2Puts.mtop, false);
								m2Puts.arrangeable = true;
							}
						}
					}
				}
			}
		}

		protected virtual void initActionMeta(out int maxhp)
		{
			this.type = M2LpBreakable.BREAKT.NORMAL;
			maxhp = 3;
			this.pre_on = true;
			if (this.Meta.GetI("magic", 0, 0) > 0)
			{
				this.type = M2LpBreakable.BREAKT.ALLOW_MAGIC;
			}
			else if (this.Meta.GetB("fall", false))
			{
				this.type = M2LpBreakable.BREAKT.FALL;
				maxhp = 6;
			}
			else if (this.Meta.GetB("fallpunch", false))
			{
				this.type = M2LpBreakable.BREAKT.FALLPUNCH;
				maxhp = 6;
			}
			else if (this.Meta.GetB("bomb", false))
			{
				this.type = M2LpBreakable.BREAKT.BOMB;
				maxhp = 1;
			}
			else if (this.Meta.GetB("powerbomb", false))
			{
				this.type = M2LpBreakable.BREAKT.POWERBOMB;
				maxhp = 1;
			}
			else if (this.Meta.GetB("fire", false))
			{
				this.type = M2LpBreakable.BREAKT.FIRE;
				maxhp = 1;
			}
			maxhp = this.Meta.GetI("hp", maxhp, 0);
			this.do_not_bind_BCC_ = this.Meta.GetB("do_not_bind_BCC", false);
		}

		public override void initAction(bool normal_map)
		{
			if (!normal_map)
			{
				return;
			}
			int num;
			this.initActionMeta(out num);
			if (!this.Meta.GetB("no_consider_managearea", false))
			{
				this.Managed = PUZ.IT.isBelongTo(this);
			}
			this.under_revert = this.Meta.GetB("under_revert", false);
			this.revert_time = this.Meta.GetI("revert_time", 0, 0);
			int i = this.Meta.GetI("no_write_sf", -1, 0);
			if (i == -1)
			{
				this.no_write_sf = this.revert_time > 0 || this.Managed != null;
			}
			else
			{
				this.no_write_sf = i != 0;
			}
			this.aim = this.Meta.getDirsI("aim", 0, false, 0, -1);
			if (this.APuts == null || this.APuts.Count == 0)
			{
				if (this.pre_on)
				{
					X.de("M2LpBreakable::initAcion " + base.unique_key + " のマップチップの抽出に失敗しました", null);
				}
				return;
			}
			int num2 = this.Meta.GetI("no_hint", -1, 0);
			if (num2 == -1)
			{
				num2 = ((this.revert_time > 0 && this.fallbreak) ? 1 : 0);
			}
			this.no_hint = num2 != 0;
			if (!this.revertable && COOK.getSF(this.sf_key) > 0)
			{
				this.pastingChipManual(false, true);
				this.initBreak(true);
				return;
			}
			if (this.pre_on)
			{
				this.MvHit = this.Mp.createMover<M2BreakableWallMover>(this.key + "-hit", base.mapcx, base.mapcy, false, false);
				this.MvHit.initLp(this, num);
				this.MvHit.attractChips(this.APuts, false);
				if (this.Meta.GetB("auto_appear", false))
				{
					this.appearTypeSkin(null);
					this.full_appeared_skin = true;
				}
				if (this.HiddenLayer != null)
				{
					this.setLayerContentVisible(false, false);
				}
			}
		}

		private bool checkForAimBcc(M2BlockColliderContainer.BCCLine Bcc)
		{
			if (Bcc.is_lift || this.aim == -1 || M2LpBreakable.AtkCheckAim == null)
			{
				return false;
			}
			AIM foot_aim = Bcc.foot_aim;
			if (CAim.get_opposite(foot_aim) == (AIM)this.aim || (foot_aim == AIM.T && Bcc.is_naname && CAim._XD(this.aim, 1) == -Bcc._xd))
			{
				M2Attackable attackFrom = M2LpBreakable.AtkCheckAim.AttackFrom;
				float num = 0f;
				float num2 = 0f;
				if (M2LpBreakable.AtkCheckAim.PublishMagic != null && !M2LpBreakable.AtkCheckAim.PublishMagic.is_normal_attack)
				{
					num = 5f;
					num2 = 3f;
				}
				switch (this.aim)
				{
				case 0:
					return X.BTW(Bcc.shifted_x - 2f - num, attackFrom.mright, Bcc.shifted_right + 0.1f) && X.BTW(Bcc.shifted_y - 0.5f - num2, attackFrom.y, Bcc.shifted_bottom + 0.5f + num2);
				case 1:
					return X.BTW(Bcc.shifted_y - 3f - num2, attackFrom.mbottom, Bcc.shifted_bottom + 0.1f) && X.BTW(Bcc.shifted_x - 1.5f - num, attackFrom.x, Bcc.shifted_right + 1.5f + num);
				case 2:
					return X.BTW(Bcc.shifted_x - 0.1f, attackFrom.mleft, Bcc.shifted_right + 2f + num) && X.BTW(Bcc.shifted_y - 0.5f - num2, attackFrom.y, Bcc.shifted_bottom + 0.5f + num2);
				case 3:
					return X.BTW(Bcc.shifted_y - 0.1f, attackFrom.mtop, Bcc.shifted_bottom + 4f + num2) && X.BTW(Bcc.shifted_x - 1.5f - num, attackFrom.x, Bcc.shifted_right + 1.5f + num);
				}
			}
			return false;
		}

		public int check_damage(NelAttackInfo Atk)
		{
			if (Atk == null || (Atk.PublishMagic != null && !Atk.PublishMagic.hit_en) || this.MvHit == null)
			{
				return 1;
			}
			if (Atk.PublishMagic == null && this.aim != -1)
			{
				return 0;
			}
			if (this.aim != -1 && Atk.AttackFrom != null)
			{
				M2Ray ray = Atk.PublishMagic.Ray;
				if (ray == null || Atk.AttackFrom == null)
				{
					return 0;
				}
				Vector2 mapPos = ray.getMapPos(0f);
				float num = 0f;
				if (Atk.PublishMagic != null && !Atk.PublishMagic.is_normal_attack)
				{
					num = 6f;
				}
				if (!Atk.AttackFrom.isCovering(mapPos.x, mapPos.x, mapPos.y, mapPos.y, 2f + num))
				{
					return 0;
				}
				M2BlockColliderContainer bcccon = this.MvHit.getBCCCon();
				if (bcccon == null)
				{
					return 0;
				}
				M2LpBreakable.AtkCheckAim = Atk;
				if (this.FD_checkForAimBcc == null)
				{
					this.FD_checkForAimBcc = new Func<M2BlockColliderContainer.BCCLine, bool>(this.checkForAimBcc);
				}
				bool flag = bcccon.findBcc(this.FD_checkForAimBcc) == null;
				M2LpBreakable.AtkCheckAim = null;
				if (flag)
				{
					return 0;
				}
			}
			switch (this.type)
			{
			case M2LpBreakable.BREAKT.FALL:
				this.appearTypeSkin(Atk);
				return 0;
			case M2LpBreakable.BREAKT.FALLPUNCH:
				if (Atk.PublishMagic == null)
				{
					return 0;
				}
				return 1;
			case M2LpBreakable.BREAKT.BOMB:
				if (Atk.PublishMagic == null)
				{
					return 0;
				}
				if (Atk.attr != MGATTR.BOMB)
				{
					this.appearTypeSkin(Atk);
					return 0;
				}
				break;
			case M2LpBreakable.BREAKT.POWERBOMB:
				if (Atk.PublishMagic == null)
				{
					return 0;
				}
				if (Atk.attr != MGATTR.BOMB || Atk.PublishMagic.kind != MGKIND.POWERBOMB)
				{
					this.appearTypeSkin(Atk);
					return 0;
				}
				break;
			case M2LpBreakable.BREAKT.FIRE:
				return 0;
			}
			if (Atk.PublishMagic == null || !Atk.PublishMagic.is_chanted_magic || Atk.PublishMagic.is_normal_attack)
			{
				return 1;
			}
			if (!(Atk.Caster is PR))
			{
				return 0;
			}
			return X.IntC((float)this.MvHit.get_maxhp());
		}

		public override void closeAction(bool when_map_close = false)
		{
			this.setLayerContentVisible(true, true);
			if (this.MvHit != null)
			{
				this.MvHit.close_action_destruction = true;
				this.Mp.removeMover(this.MvHit);
				this.MvHit = null;
			}
			if (this.Ed != null)
			{
				this.Ed = this.Mp.remED(this.Ed);
			}
			this.Mtr = null;
			this.full_appeared_skin = false;
			this.pastingChipManual(true, false);
		}

		public void pastingChipManual(bool visible = true, bool reconsider_config = true)
		{
			if (this.APuts != null)
			{
				int count = this.APuts.Count;
				for (int i = 0; i < count; i++)
				{
					if (visible)
					{
						this.APuts[i].remActiveRemoveKey(base.unique_key, false);
					}
					else
					{
						this.APuts[i].addActiveRemoveKey(base.unique_key, false);
					}
				}
				if (reconsider_config)
				{
					this.Mp.considerConfig4(this.mapx, this.mapy, this.mapx + this.mapw, this.mapy + this.maph);
					this.Mp.need_update_collider = true;
				}
			}
		}

		public virtual void initBreak(bool on_initaction = false)
		{
			this.setLayerContentVisible(true, !this.do_not_bind_BCC);
			if (!on_initaction && !this.revertable)
			{
				COOK.setSF(this.sf_key, 1);
			}
			if (!this.revertable)
			{
				List<M2LabelPoint> labelPointAll = this.Lay.getLabelPointAll((M2LabelPoint Lp) => Lp.isCovering(this, 0f), null);
				if (labelPointAll != null)
				{
					int count = labelPointAll.Count;
					for (int i = 0; i < count; i++)
					{
						labelPointAll[i].deactivate();
					}
				}
			}
			if (!on_initaction)
			{
				string s = this.Meta.GetS("release_item");
				if (TX.valid(s))
				{
					NelItem byId = NelItem.GetById(s, false);
					if (byId != null)
					{
						float mapfocx = base.mapfocx;
						float num = this.Mp.getFootableY(mapfocx, (int)base.mapfocy, 12, true, -1f, false, true, true, 0f) - 0.5f;
						base.nM2D.IMNG.dropManual(byId, this.Meta.GetI("release_item", 1, 1), this.Meta.GetI("release_item", 0, 2), mapfocx, num, 0f, -0.125f, null, false, NelItemManager.TYPE.NORMAL);
					}
				}
			}
		}

		public void destructedMover(M2Mover _MvHit)
		{
			if (this.MvHit == _MvHit)
			{
				this.MvHit = null;
			}
		}

		public void setLayerContentVisible(bool flag, bool no_consider_config = false)
		{
			if (this.HiddenLayer == null || flag == this.hidden_layer_visible)
			{
				return;
			}
			int count_chips = this.HiddenLayer.count_chips;
			DRect drect = ((!no_consider_config) ? new DRect("_") : null);
			for (int i = 0; i < count_chips; i++)
			{
				M2Puts chipByIndex = this.HiddenLayer.getChipByIndex(i);
				if (flag)
				{
					chipByIndex.remActiveRemoveKey("breakable_" + this.key, no_consider_config);
				}
				else
				{
					chipByIndex.addActiveRemoveKey("breakable_" + this.key, false);
				}
				if (!no_consider_config && chipByIndex is M2Chip)
				{
					M2Chip m2Chip = chipByIndex as M2Chip;
					if (drect.isEmpty())
					{
						drect.Set((float)m2Chip.mapx, (float)m2Chip.mapy, (float)m2Chip.clms, (float)m2Chip.rows);
					}
					else
					{
						X.rectExpand(drect, (float)m2Chip.mapx, (float)m2Chip.mapy, (float)m2Chip.clms, (float)m2Chip.rows);
					}
				}
			}
			this.hidden_layer_visible = flag;
			if (!no_consider_config)
			{
				this.Mp.considerConfig4((int)drect.x, (int)drect.y, (int)drect.right, (int)drect.bottom);
				this.Mp.need_update_collider = true;
			}
			if (flag)
			{
				if (this.FD_fnFlashHiddenLayerChips == null)
				{
					this.FD_fnFlashHiddenLayerChips = new M2DrawBinder.FnEffectBind(this.fnFlashHiddenLayerChips);
				}
				this.Ed = this.Mp.setED("flashHiddenLayer", this.FD_fnFlashHiddenLayerChips, 0f);
			}
		}

		public void appearTypeSkin(NelAttackInfo Atk)
		{
			if (this.no_hint || this.full_appeared_skin)
			{
				return;
			}
			int count = this.APuts.Count;
			PxlSequence sequence = MTR.PxlM2DGeneral.getPoseByName("_anim_breakable_skin").getSequence(0);
			M2ImageAtlas.AtlasRect atlasRect;
			switch (this.type)
			{
			case M2LpBreakable.BREAKT.FALL:
			case M2LpBreakable.BREAKT.FALLPUNCH:
				atlasRect = this.Mp.M2D.IMGS.Atlas.getAtlasData(sequence.getFrameByName("type_fall").getLayer(0).Img);
				goto IL_0128;
			case M2LpBreakable.BREAKT.BOMB:
				atlasRect = this.Mp.M2D.IMGS.Atlas.getAtlasData(sequence.getFrameByName("type_bomb").getLayer(0).Img);
				goto IL_0128;
			case M2LpBreakable.BREAKT.POWERBOMB:
				atlasRect = this.Mp.M2D.IMGS.Atlas.getAtlasData(sequence.getFrameByName("type_powerbomb").getLayer(0).Img);
				goto IL_0128;
			}
			atlasRect = this.Mp.M2D.IMGS.Atlas.getAtlasData(sequence.getFrameByName("type_normal").getLayer(0).Img);
			IL_0128:
			if (Atk != null)
			{
				CAim._XD(Atk.Caster.getAimForCaster(), 1);
			}
			for (int i = 0; i < count; i++)
			{
				M2Puts m2Puts = this.APuts[i];
				if (!m2Puts.getMeta().GetB("_do_not_replace_temporary", false))
				{
					bool flag = m2Puts is M2Picture;
					if (!flag && m2Puts is M2Chip)
					{
						M2Chip m2Chip = m2Puts as M2Chip;
						if (m2Chip.clms != 1 || m2Chip.rows != 1)
						{
							goto IL_02E2;
						}
					}
					if (!flag)
					{
						if (Atk == null)
						{
							m2Puts.temporaryDrawerReplaceImage(atlasRect);
						}
						else
						{
							float num = m2Puts.mleft + ((this.mapx < m2Puts.mapx) ? (-1f) : 0f);
							float num2 = m2Puts.mtop + ((this.mapy < m2Puts.mapy) ? (-1f) : 0f);
							float num3 = m2Puts.mright + ((this.mapx + this.mapw - 1 > m2Puts.mapx) ? 1f : 0f);
							float num4 = m2Puts.mbottom + ((this.mapy + this.maph - 1 > m2Puts.mapy) ? 1f : 0f);
							if (X.isCovering(Atk.hit_x - 0.85f, Atk.hit_x + 0.85f, num, num3, 0f) && X.isCovering(Atk.hit_y - 0.85f, Atk.hit_y + 0.85f, num2, num4, 0f) && m2Puts.temporaryDrawerReplaceImage(atlasRect))
							{
								this.breakEffect(m2Puts.mapcx, m2Puts.mapcy, -Atk.burst_vx, Atk.burst_vy, 4);
							}
						}
					}
				}
				IL_02E2:;
			}
		}

		public void makeSnapShot(PuzzSnapShot.RevertItem Rvi)
		{
			Rvi.z = (float)((this.MvHit != null) ? this.MvHit.get_hp() : 0);
		}

		public void puzzleRevert(PuzzSnapShot.RevertItem Rvi)
		{
			if (this.MvHit != null)
			{
				this.MvHit.revertPuzzRevertHp(Rvi.z, true);
			}
		}

		public void breakPrepareEffect()
		{
			if (this.type == M2LpBreakable.BREAKT.FIRE)
			{
				this.Mp.PtcSTsetVar("x", (double)this.RcCpExist.cx).PtcSTsetVar("y", (double)this.RcCpExist.cy).PtcSTsetVar("w", (double)(this.RcCpExist.w * base.CLENB))
					.PtcSTsetVar("h", (double)(this.RcCpExist.h * base.CLENB))
					.PtcST("breakable_fire_burn_start", null, PTCThread.StFollow.NO_FOLLOW);
			}
		}

		public string breakEffect(float cx, float cy, float vx, float vy, int ARMX = -1)
		{
			if (ARMX < 0)
			{
				ARMX = 26;
			}
			cx += (float)X.MPF(vx > 0f);
			float num = 0.01f;
			float num2 = 0.11f;
			float num3 = -0.05f;
			float num4 = -0.26f;
			string text = "kill_breakable_wall";
			float num5 = -1f;
			float num6 = 1f;
			float num7 = -1f;
			float num8 = 0f;
			M2LpBreakable.BREAKT breakt = this.type;
			if (breakt - M2LpBreakable.BREAKT.FALL > 1)
			{
				if (breakt == M2LpBreakable.BREAKT.FIRE)
				{
					this.Mp.PtcSTsetVar("x", (double)this.RcCpExist.cx).PtcSTsetVar("y", (double)this.RcCpExist.cy).PtcSTsetVar("w", (double)(this.RcCpExist.w * base.CLENB))
						.PtcSTsetVar("h", (double)(this.RcCpExist.h * base.CLENB))
						.PtcST("breakable_fire_burn_end", null, PTCThread.StFollow.NO_FOLLOW);
					text = "";
				}
			}
			else
			{
				vx = 0f;
				cx = this.RcCpExist.cx;
				cy = this.RcCpExist.cy;
				num = 0.002f;
				num2 = 0.03f;
				num3 = 0f;
				num4 = 0.015f;
				num6 = this.RcCpExist.width / 2f;
				num5 = -num6;
				num8 = this.RcCpExist.height / 2f;
				num7 = -num8;
				text = "fall_breakable_wall_dot";
				this.Mp.PtcSTsetVar("x", (double)cx).PtcSTsetVar("y", (double)cy).PtcSTsetVar("w", (double)(this.RcCpExist.w * base.CLENB))
					.PtcSTsetVar("h", (double)(this.RcCpExist.h * base.CLENB))
					.PtcST2Base("fall_breakable_wall", 0f, PTCThread.StFollow.NO_FOLLOW);
			}
			for (int i = 0; i < ARMX; i++)
			{
				M2DropObject m2DropObject = this.Mp.DropCon.Add(this.FD_fnDrawChips, cx + X.NIXP(num5, num6), cy + X.NIXP(num7, num8), (float)((vx == 0f) ? X.MPF(X.XORSP() > 0.5f) : (X.MPF(vx > 0f) * X.MPF(X.XORSP() > 0.15f))) * X.NIXP(num, num2), X.NIXP(num3, num4), -1f, 60f);
				m2DropObject.size = 0.04f;
				m2DropObject.gravity_scale = 0.3f;
				m2DropObject.bounce_y_reduce = 0.5f;
				m2DropObject.bounce_x_reduce_when_ground = 0.6f;
			}
			return text;
		}

		private bool fnFlashHiddenLayerChips(EffectItem Ef, M2DrawBinder Ed)
		{
			float num = X.ZLINE(Ef.af - 2f, 14f);
			if (num >= 1f || this.HiddenLayer == null || !this.Mp.M2D.Cam.isCoveringMp((float)this.mapx, (float)this.mapy, (float)(this.mapx + this.mapw), (float)(this.mapy + this.maph), base.CLEN))
			{
				this.Ed = null;
				this.Mtr = null;
				return false;
			}
			if (this.Mtr == null)
			{
				this.Mtr = this.Mp.M2D.MIchip.getMtr(BLEND.ADDP, -1);
			}
			MeshDrawer mesh = Ef.GetMesh("", this.Mtr, false);
			mesh.base_x = (mesh.base_y = 0f);
			this.Mtr.SetColor("_Color", mesh.ColGrd.Set(4286874248U).blend(8684168U, num).C);
			mesh.Col = MTRX.ColWhite;
			int count_chips = this.HiddenLayer.count_chips;
			float base_scale = this.Mp.base_scale;
			for (int i = 0; i < count_chips; i++)
			{
				M2Puts chipByIndex = this.HiddenLayer.getChipByIndex(i);
				if (chipByIndex.Img.initAtlasMd(mesh, 0U))
				{
					mesh.RotaGraph(this.Mp.map2meshx(chipByIndex.mapcx) * base_scale, this.Mp.map2meshy(chipByIndex.mapcy) * base_scale, base_scale, chipByIndex.draw_rotR, null, chipByIndex.flip);
				}
			}
			return true;
		}

		private bool fnDrawChips(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.APuts == null)
			{
				return false;
			}
			uint ran = X.GETRAN2(Dro.index, Dro.index % 7);
			return this.APuts[(int)((ulong)ran % (ulong)((long)this.APuts.Count))].Img.fnDrawForDropObject(Dro, Ef, Ed);
		}

		public bool fallbreak
		{
			get
			{
				return this.type == M2LpBreakable.BREAKT.FALL || this.type == M2LpBreakable.BREAKT.FALLPUNCH;
			}
		}

		public bool revertable
		{
			get
			{
				return this.no_write_sf;
			}
		}

		public bool auto_revertable
		{
			get
			{
				return this.no_write_sf && this.revert_time > 0;
			}
		}

		public bool do_not_bind_BCC
		{
			get
			{
				return !this.auto_revertable || this.do_not_bind_BCC_;
			}
		}

		public string sf_key
		{
			get
			{
				return "M2D_LP_BRK_" + this.key;
			}
		}

		protected List<M2Puts> APuts;

		private M2BreakableWallMover MvHit;

		private DRect RcCpExist;

		protected META Meta;

		private M2MapLayer HiddenLayer;

		private M2DrawBinder Ed;

		private Material Mtr;

		protected int aim = -1;

		public bool no_write_sf;

		public bool under_revert;

		public int revert_time;

		protected bool do_not_bind_BCC_;

		protected bool pre_on;

		private bool no_hint;

		public M2LpPuzzManageArea Managed;

		private bool hidden_layer_visible = true;

		public const string breakable_sf_head = "M2D_LP_BRK_";

		private bool full_appeared_skin;

		public M2LpBreakable.BREAKT type;

		private readonly M2DropObject.FnDropObjectDraw FD_fnDrawChips;

		private Func<M2BlockColliderContainer.BCCLine, bool> FD_checkForAimBcc;

		private static NelAttackInfo AtkCheckAim;

		private M2DrawBinder.FnEffectBind FD_fnFlashHiddenLayerChips;

		public enum BREAKT
		{
			NORMAL,
			FALL,
			FALLPUNCH,
			ALLOW_MAGIC,
			BOMB,
			POWERBOMB,
			FIRE
		}
	}
}
