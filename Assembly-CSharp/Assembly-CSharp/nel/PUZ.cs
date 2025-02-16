using System;
using System.Collections.Generic;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class PUZ
	{
		public PUZ(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.ALp = new List<M2LpPuzzManageArea>(2);
			this.ALpPre = new List<M2LpPuzzManageArea>();
			this.ADrAll = new List<M2CImgDrawerBarrierLit>(4);
			this.Asnap_manage_layer = new List<string>();
			this.APaListener = new List<IPuzzActivationListener>(4);
			this.ASwitchListener = new List<IPuzzSwitchListener>(4);
			this.ASnapShot = new List<PuzzSnapShot>(1);
			this.ARevertable = new List<IPuzzRevertable>();
			PUZ.IT = this;
		}

		public PUZ destruct()
		{
			if (PUZ.IT == this)
			{
				PUZ.IT = null;
			}
			if (this.MtrBarriearImage != null)
			{
				IN.DestroyOne(this.MtrBarriearImage);
			}
			return null;
		}

		public void initS(Map2d Mp)
		{
			this.Rc = new M2Rect("_", 0, Mp);
			this.NearLit = null;
			this.ALp.Clear();
			this.ALpPre.Clear();
			this.ADrAll.Clear();
			this.APaListener.Clear();
			this.ASwitchListener.Clear();
			this.Asnap_manage_layer.Clear();
			this.ASnapShot.Clear();
			this.efST = null;
			PUZ.connector_activated = 0;
			this.barrier_active_ = false;
			this.puzz_magic_count_max = -2;
			this.Ef = (this.EfM = null);
			this.ARevertable.Clear();
			if (this.MtrBarriearImage == null)
			{
				this.MtrBarriearImage = M2DBase.newMtr("M2d/PuzzleBarrier");
			}
		}

		public void initMapEditor()
		{
			if (this.ALp == null)
			{
				return;
			}
			for (int i = this.ALp.Count - 1; i >= 0; i--)
			{
				this.deactivateBarrier(this.ALp[i], false);
			}
			this.ALp.Clear();
			this.APaListener.Clear();
			this.ARevertable.Clear();
			this.ADrAll.Clear();
			this.ASwitchListener.Clear();
			this.Asnap_manage_layer.Clear();
			this.NearLit = null;
		}

		public void quitMapEditor()
		{
			if (this.barrier_active_)
			{
				this.createSnapShot(false);
			}
		}

		public void initRevertableObject(Map2d Mp, M2LpPuzzManageArea Lp)
		{
			Mp.EachLP(delegate(M2LabelPoint _L)
			{
				if (Lp.isCovering(_L, 0f))
				{
					this.checkLpListenerAddition(_L, false);
				}
			});
		}

		public void addRevertableObject(IPuzzRevertable _Lpr, bool check_dupe = false)
		{
			if (_Lpr != null && (!check_dupe || this.ARevertable.IndexOf(_Lpr) == -1))
			{
				this.ARevertable.Add(_Lpr);
			}
		}

		public void remRevertableObject(IPuzzRevertable _Lpr)
		{
			if (this.ARevertable != null)
			{
				this.ARevertable.Remove(_Lpr);
			}
		}

		private void checkLpListenerAddition(M2LabelPoint _L, bool check_dupe = false)
		{
			if (_L is IPuzzRevertable && !(_L is M2LpPuzzManageArea))
			{
				this.addRevertableObject(_L as IPuzzRevertable, false);
			}
			if (_L is IPuzzSwitchListener)
			{
				IPuzzSwitchListener puzzSwitchListener = _L as IPuzzSwitchListener;
				if (!check_dupe || this.ASwitchListener.IndexOf(puzzSwitchListener) == -1)
				{
					this.ASwitchListener.Add(puzzSwitchListener);
				}
			}
		}

		public M2LpPuzzManageArea isBelongTo(M2LabelPoint Lp)
		{
			if (this._Mp == null)
			{
				return null;
			}
			List<M2LabelPoint> labelPointAll = this._Mp.getLabelPointAll((M2LabelPoint V) => V is M2LpPuzzManageArea, null);
			if (labelPointAll == null)
			{
				return null;
			}
			int count = labelPointAll.Count;
			for (int i = 0; i < count; i++)
			{
				if (labelPointAll[i].isCovering(Lp, 0f))
				{
					if (Lp is IPuzzActivationListener)
					{
						this.APaListener.Add(Lp as IPuzzActivationListener);
					}
					return labelPointAll[i] as M2LpPuzzManageArea;
				}
			}
			return null;
		}

		public M2LpPuzzManageArea isBelongTo(M2Puts Cp)
		{
			if (this._Mp == null)
			{
				return null;
			}
			List<M2LabelPoint> labelPointAll = this._Mp.getLabelPointAll((M2LabelPoint V) => V is M2LpPuzzManageArea, null);
			if (labelPointAll == null)
			{
				return null;
			}
			int count = labelPointAll.Count;
			for (int i = 0; i < count; i++)
			{
				if (labelPointAll[i].isCoveringXy((float)Cp.drawx, (float)Cp.drawy, (float)(Cp.drawx + Cp.iwidth), (float)(Cp.drawx + Cp.iheight), 0f, -1000f))
				{
					if (Cp is IPuzzActivationListener)
					{
						this.APaListener.Add(Cp as IPuzzActivationListener);
					}
					return labelPointAll[i] as M2LpPuzzManageArea;
				}
			}
			return null;
		}

		public M2LpPuzzManageArea isBelongTo(M2CImgDrawer Dr)
		{
			if (this._Mp == null)
			{
				return null;
			}
			List<M2LabelPoint> labelPointAll = this._Mp.getLabelPointAll((M2LabelPoint V) => V is M2LpPuzzManageArea, null);
			if (labelPointAll == null)
			{
				return null;
			}
			int count = labelPointAll.Count;
			for (int i = 0; i < count; i++)
			{
				if (labelPointAll[i].isCoveringXy((float)Dr.Cp.drawx, (float)Dr.Cp.drawy, (float)(Dr.Cp.drawx + Dr.Cp.iwidth), (float)(Dr.Cp.drawx + Dr.Cp.iheight), 0f, -1000f))
				{
					if (Dr is IPuzzActivationListener)
					{
						this.APaListener.Add(Dr as IPuzzActivationListener);
						if (Dr is M2CImgDrawerBarrierLit)
						{
							this.ADrAll.Add(Dr as M2CImgDrawerBarrierLit);
						}
					}
					return labelPointAll[i] as M2LpPuzzManageArea;
				}
			}
			return null;
		}

		public void removePaListener(IPuzzActivationListener Pa)
		{
			this.APaListener.Remove(Pa);
		}

		public void addPaListener(IPuzzActivationListener Pa)
		{
			if (this.APaListener.IndexOf(Pa) == -1)
			{
				this.APaListener.Add(Pa);
			}
		}

		public void activateBarrier(M2LpPuzzManageArea Lp)
		{
			if (Lp != null)
			{
				this.ALp.Add(Lp);
			}
			if (!this.barrier_active_ && this.ALp.Count > 0)
			{
				this.ALpPre.Clear();
				this.barrier_active_ = true;
				this.Ef = M2DBase.Instance.curMap.setE("puzzle_barrier", (EffectItem Ef) => PUZ.IT.fnDrawBarrierActivate(Ef), 0f, 0f, 0f, 0, 0);
				this.EfM = M2DBase.Instance.curMap.setET("puzzle_barrier_mirrage", (EffectItem Ef) => PUZ.IT.fnDrawBarrierMirrage(Ef), 0f, 0f, 0f, 0, 0);
				this.efST = this._Mp.PtcST("puzzle_barrier_activate", null, PTCThread.StFollow.NO_FOLLOW);
				this.puzz_magic_count_max = Lp.magic_count;
				M2MoverPr keyPr = this._Mp.getKeyPr();
				int count = this.ADrAll.Count;
				this.clear_mist = Lp.clear_mist;
				M2CImgDrawerBarrierLit m2CImgDrawerBarrierLit = null;
				float num = 0f;
				for (int i = 0; i < count; i++)
				{
					M2CImgDrawerBarrierLit m2CImgDrawerBarrierLit2 = this.ADrAll[i];
					float num2 = X.LENGTHXYS(keyPr.x, keyPr.y, m2CImgDrawerBarrierLit2.Cp.mapcx, m2CImgDrawerBarrierLit2.Cp.mapcy);
					if (m2CImgDrawerBarrierLit == null || num > num2)
					{
						num = num2;
						m2CImgDrawerBarrierLit = m2CImgDrawerBarrierLit2;
					}
				}
				this.NearLit = m2CImgDrawerBarrierLit;
				if (this.NearLit != null)
				{
					this.NearLit.touchPlayer(true);
				}
				int num3 = this.APaListener.Count;
				for (int j = 0; j < num3; j++)
				{
					this.APaListener[j].changePuzzleActivation(true);
				}
				string[] array = Lp.Meta.Get("snap_manage_layer");
				if (array != null)
				{
					for (int k = array.Length - 1; k >= 0; k--)
					{
						string text = array[k];
						if (this.Asnap_manage_layer.IndexOf(text) == -1)
						{
							this.Asnap_manage_layer.Add(text);
							M2MapLayer layer = Lp.Mp.getLayer(text);
							if (layer != null)
							{
								num3 = layer.LP.Length;
								for (int l = 0; l < num3; l++)
								{
									this.checkLpListenerAddition(layer.LP.Get(l), true);
								}
							}
						}
					}
				}
				this.createSnapShot(false);
				this.defineDeclineArea(Lp, this.get_mirrage_shift_width(Lp) * 1.5f * 8f * 0.88f, this.get_mirrage_shift_height(Lp) * 1.5f * 8f * 0.88f);
				this.M2D.MGC.killAllPlayerMagic(null, null);
			}
			this.ALpPre.Add(Lp);
		}

		public void createSnapShot(bool check_state)
		{
			if (this.ALp == null || this.ALp.Count == 0)
			{
				return;
			}
			bool flag = this.ASnapShot.Count == 0;
			PuzzSnapShot puzzSnapShot = new PuzzSnapShot(2 + ((this.ARevertable == null) ? this.ARevertable.Count : 0));
			M2LpPuzzManageArea m2LpPuzzManageArea = this.ALp[0];
			PuzzSnapShot.RevertItem revertItem = new PuzzSnapShot.RevertItem(m2LpPuzzManageArea);
			m2LpPuzzManageArea.makeSnapShot(revertItem);
			if (flag)
			{
				m2LpPuzzManageArea.puzzleRevert(revertItem);
			}
			puzzSnapShot.Add(revertItem);
			puzzSnapShot.Add(this.ARevertable);
			if (this.clear_mist)
			{
				puzzSnapShot.Add(this.M2D.MIST);
			}
			if (check_state && !flag && puzzSnapShot.isSame(this.ASnapShot[this.ASnapShot.Count - 1]))
			{
				return;
			}
			int count_players = this._Mp.count_players;
			for (int i = 0; i < count_players; i++)
			{
				PR pr = this._Mp.getPr(i) as PR;
				if (pr != null)
				{
					if (flag)
					{
						pr.Skill.initPuzzleManagingMp(-1);
					}
					puzzSnapShot.Add(pr);
				}
			}
			this.ASnapShot.Add(puzzSnapShot);
		}

		public void revertGimmickActivation()
		{
			if (this.ASnapShot.Count == 0)
			{
				return;
			}
			PuzzLiner.finalizeAnimation(true, -1, false);
			this.ASnapShot[this.ASnapShot.Count - 1].revertExecute(this.M2D);
			this.M2D.MGC.killAllPlayerMagic(null, null);
		}

		private void defineDeclineArea(M2LabelPoint Lp, float extend_x = 0f, float extend_y = 0f)
		{
			M2Camera cam = M2DBase.Instance.Cam;
			for (int i = 0; i < 4; i++)
			{
				M2LpCamDecline m2LpCamDecline = M2LpCamDecline.createBounds("PUZZAREA_", i, Lp.x, Lp.y, Lp.right, Lp.bottom, extend_x, extend_y);
				cam.addCropping(m2LpCamDecline);
			}
		}

		private void quitDeclineArea()
		{
			M2Camera cam = M2DBase.Instance.Cam;
			for (int i = 0; i < 4; i++)
			{
				cam.remCropping("PUZZAREA_" + i.ToString());
			}
		}

		public static bool isActivePuzzleId(int id)
		{
			return (PUZ.connector_activated & (1 << id)) != 0;
		}

		public void FineLitConnect()
		{
			int count = this.ADrAll.Count;
			for (int i = 0; i < count; i++)
			{
				this.ADrAll[i].CpB.FineLitConnect(true);
			}
		}

		public NelChipBarrierLit getLitWorpDestination(NelChipBarrierLit Src, M2CImgDrawerBarrierLit Dr)
		{
			int count = this.ADrAll.Count;
			if (count <= 1 || Dr == null)
			{
				return null;
			}
			int num = this.ADrAll.IndexOf(Dr);
			if (num == -1)
			{
				return null;
			}
			for (int i = 1; i < count; i++)
			{
				M2CImgDrawerBarrierLit m2CImgDrawerBarrierLit = this.ADrAll[(num + i) % count];
				if (m2CImgDrawerBarrierLit.isTouched())
				{
					return m2CImgDrawerBarrierLit.CpB;
				}
			}
			return null;
		}

		public void connectorActivated(int puzzle_id, bool activated)
		{
			if (activated)
			{
				PUZ.connector_activated |= 1 << puzzle_id;
			}
			else
			{
				PUZ.connector_activated &= ~(1 << puzzle_id);
			}
			for (int i = this.ASwitchListener.Count - 1; i >= 0; i--)
			{
				this.ASwitchListener[i].changePuzzleSwitchActivation(puzzle_id, activated);
			}
		}

		public void callRevertEvent(bool is_irisout, string event_key)
		{
			if (!this.barrier_active_)
			{
				return;
			}
			int count = this.ALp.Count;
			for (int i = 0; i < count; i++)
			{
				string s = this.ALp[i].Meta.GetS("revert_event");
				if (TX.valid(s))
				{
					EV.stack(s, 0, -1, new string[] { event_key }, null);
				}
			}
			if (this.M2D.MIST != null)
			{
				this.M2D.MIST.initFirstProgress(20);
			}
		}

		public void deactivateBarrier(M2LpPuzzManageArea Lp, bool set_effect = true)
		{
			if (!this.barrier_active_)
			{
				return;
			}
			if (Lp != null)
			{
				this.ALp.Remove(Lp);
			}
			if (this.ALp.Count == 0)
			{
				this.barrier_active_ = false;
				int num = this.APaListener.Count;
				for (int i = 0; i < num; i++)
				{
					this.APaListener[i].changePuzzleActivation(false);
				}
				if (set_effect)
				{
					this._Mp.PtcST("puzzle_barrier_deactivate", null, PTCThread.StFollow.NO_FOLLOW);
				}
				if (this.Ef != null)
				{
					this.Ef.af = 0f;
					this.Ef.f0 = (int)this._Mp.floort;
				}
				if (this.efST != null)
				{
					this.efST.kill(false);
				}
				PuzzLiner.finalizeAnimation(true, -1, false);
				if (this.ASnapShot.Count > 0)
				{
					this.ASnapShot[0].revertExecute(this.M2D);
				}
				this.ASnapShot.Clear();
				this.puzz_magic_count_max = -2;
				num = this._Mp.count_players;
				for (int j = 0; j < num; j++)
				{
					PR pr = this._Mp.getPr(j) as PR;
					if (pr != null)
					{
						pr.Skill.quitPuzzleManagingMp();
					}
				}
				this.quitDeclineArea();
				this.M2D.MGC.killAllPlayerMagic(null, null);
				bool flag = this.clear_mist;
				this.clear_mist = false;
			}
		}

		private bool fnDrawBarrierMirrage(EffectItem Ef)
		{
			if (!this.barrier_active_)
			{
				Ef.destruct();
				this.EfM = null;
			}
			M2Camera cam = M2DBase.Instance.Cam;
			float base_scale = this._Mp.base_scale;
			if (this.ALp.Count == 0)
			{
				return true;
			}
			int num = 1;
			Map2d mp = this.ALp[0].Mp;
			this.MtrBarriearImage.SetTexture("_MainTex", cam.getFinalizedTexture());
			Ef.x = cam.x / mp.CLEN;
			Ef.y = cam.y / mp.CLEN;
			MeshDrawer mesh = Ef.GetMesh("", this.MtrBarriearImage, false);
			float num2 = cam.viewable_pixel_width / 2f;
			float num3 = cam.viewable_pixel_height / 2f;
			float num4 = X.ZSIN(Ef.af, 40f);
			mesh.base_z += 6f;
			for (int i = 0; i < num; i++)
			{
				M2LpPuzzManageArea m2LpPuzzManageArea = this.ALp[i];
				this.Rc.Set(m2LpPuzzManageArea);
				float num5 = (m2LpPuzzManageArea.x - cam.x) * base_scale;
				float num6 = (m2LpPuzzManageArea.right - cam.x) * base_scale;
				float num7 = (m2LpPuzzManageArea.y - cam.y) * base_scale;
				float num8 = (m2LpPuzzManageArea.bottom - cam.y) * base_scale;
				float num9 = num5 / (IN.w + 16f);
				float num10 = -num8 / (IN.h + 16f);
				float num11 = num6 / (IN.w + 16f);
				float num12 = -num7 / (IN.h + 16f);
				if (num5 >= -num2 || num7 >= -num3 || num6 <= num2 || num8 <= num3)
				{
					float num13 = this.get_mirrage_shift_width(m2LpPuzzManageArea);
					float num14 = this.get_mirrage_shift_height(m2LpPuzzManageArea);
					Color32 color = mesh.ColGrd.Set(4290692351U).mulA(0f).C;
					for (int j = 1; j <= 8; j++)
					{
						uint ran = X.GETRAN3(j + i * 33, j, 164721 + 3567 * i);
						float num15 = (float)j * (1f + 0.5f * X.ZPOW((float)(j - 1), 7f));
						float num16 = X.NI(1.05f, 0.75f, (float)(j - 1) / 8f);
						float num17 = (m2LpPuzzManageArea.x - num13 * num15 - cam.x) * base_scale;
						float num18 = (m2LpPuzzManageArea.right + num13 * num15 - cam.x) * base_scale;
						float num19 = (m2LpPuzzManageArea.y - num14 * num15 - cam.y) * base_scale;
						float num20 = (m2LpPuzzManageArea.bottom + num14 * num15 - cam.y) * base_scale;
						float num21 = num17 - 12f + 6f * X.COSI(Ef.af, X.NI(110, 228, X.RAN(ran, 2971)));
						float num22 = num20 + 10f + 5f * X.COSI(Ef.af, X.NI(110, 228, X.RAN(ran, 1455)));
						float num23 = num18 + 12f + 6f * X.COSI(Ef.af, X.NI(110, 228, X.RAN(ran, 1774)));
						float num24 = num19 - 12f + 6f * X.COSI(Ef.af, X.NI(110, 228, X.RAN(ran, 3187)));
						float num25 = num9 * num16;
						float num26 = num11 * num16;
						float num27 = num10 * num16;
						float num28 = num12 * num16;
						mesh.uvRect(num21 * 0.015625f, -num22 * 0.015625f, (num23 - num21) * 0.015625f, (num22 - num24) * 0.015625f, num25 + 0.5f, num27 + 0.5f, num26 - num25, num28 - num27, false, true);
						float num29 = (num17 + num18) / 2f;
						float num30 = (num19 + num20) / 2f;
						mesh.Col = mesh.ColGrd.Set(4290692351U).multiply(1f - num4 * X.ZSINV((float)j, 8f), true).setA1(X.ZSIN2((float)j, 4f))
							.C;
						mesh.ColGrd.Set(color);
						mesh.RectDoughnut(num29, -num30, num18 - num17, num20 - num19, num29, -num30, num6 - num5, num8 - num7, false, 0f, 1f, false);
						color = mesh.Col;
						num5 = num17;
						num6 = num18;
						num7 = num19;
						num8 = num20;
						if (num17 < -num2 && num19 < -num3 && num18 > num2 && num20 > num3)
						{
							break;
						}
					}
				}
			}
			return true;
		}

		private float get_mirrage_shift_width(M2LabelPoint _Lp)
		{
			return X.MMX(0.5f * this._CLEN, _Lp.width * 0.0375f, 2f * this._CLEN);
		}

		private float get_mirrage_shift_height(M2LabelPoint _Lp)
		{
			return X.MMX(0.35f * this._CLEN, _Lp.height * 0.0375f, 1.5f * this._CLEN);
		}

		private bool fnDrawBarrierActivate(EffectItem Ef)
		{
			M2Camera cam = M2DBase.Instance.Cam;
			float base_scale = this._Mp.base_scale;
			if (!this.barrier_active_)
			{
				if (Ef.af >= 67f)
				{
					Ef.destruct();
					this.Ef = null;
					return false;
				}
				int count = this.ALpPre.Count;
				Ef.x = cam.x * this._Mp.rCLEN;
				Ef.y = cam.y * this._Mp.rCLEN;
				float num = cam.viewable_pixel_width / 2f;
				float num2 = cam.viewable_pixel_height / 2f;
				MeshDrawer mesh = Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
				float num3 = 1f - 0.7f * X.ZSIN2(Ef.af - 2f, 4f) - 0.3f * X.ZSIN(Ef.af - 3f, 11f);
				float num4 = 1f - X.ZSIN2(Ef.af, 40f);
				for (int i = 0; i < count; i++)
				{
					M2LpPuzzManageArea m2LpPuzzManageArea = this.ALpPre[i];
					if (Ef.af < 14f)
					{
						mesh.Col = mesh.ColGrd.Set(4290692351U).mulA(num3).C;
						float num5 = (m2LpPuzzManageArea.cx - cam.x) * base_scale;
						float num6 = (m2LpPuzzManageArea.cy - cam.y) * base_scale;
						mesh.Rect(num5, -num6, m2LpPuzzManageArea.width * base_scale, m2LpPuzzManageArea.height * base_scale, false);
						mesh.Box(num5, -num6, m2LpPuzzManageArea.width * base_scale, m2LpPuzzManageArea.height * base_scale, 1f, false);
					}
					if (num4 > 0f)
					{
						int num7 = (int)((float)(m2LpPuzzManageArea.mapw * m2LpPuzzManageArea.maph) * 0.15f * X.EF_LEVEL_NORMAL);
						mesh.Col = mesh.ColGrd.Set(4290692351U).mulA(num4).C;
						float num8 = 5f - 4f * X.ZLINE(1f - num4, 0.45f);
						for (int j = 0; j < num7; j++)
						{
							uint ran = X.GETRAN3(j + 88, j, i * 3113 + 515441);
							float num9 = (m2LpPuzzManageArea.x + m2LpPuzzManageArea.width * X.RAN(ran, 1749 + j * 4) - cam.x) * base_scale;
							float num10 = (m2LpPuzzManageArea.y + m2LpPuzzManageArea.height * X.RAN(ran, 1296 + j * 6) - cam.y) * base_scale;
							float num11 = X.RAN(ran, 902 + j * 5) * 6.2831855f;
							float num12 = X.NI(30f, 55f, X.RAN(ran, 2349)) * (1f - num4);
							float num13 = num9 + num12 * X.Cos(num11);
							float num14 = -num10 + num12 * X.Sin(num11);
							float num15 = num8 * X.NI(4, 7, X.RAN(ran, 915)) * (0.6f + 0.4f * X.COSI(Ef.af, X.NI(6.76f, 9.22f, X.RAN(ran, 1789))));
							if (this.M2D.Cam.isCoveringEffectPixel(num13, num13, num14, num14, num15 + 2f, mesh.base_x, mesh.base_y))
							{
								mesh.Daia2(num13, num14, num15, 0f, false);
							}
						}
					}
				}
				if (this.NearLit != null)
				{
					Vector2 crystalMapPoint = this.NearLit.getCrystalMapPoint(-1f);
					mesh.base_x = this._Mp.ux2effectScreenx(this._Mp.map2ux(crystalMapPoint.x));
					mesh.base_y = this._Mp.uy2effectScreeny(this._Mp.map2uy(crystalMapPoint.y));
					int num16 = (int)(60f * X.NI(0.4f, 1f, X.EF_LEVEL_NORMAL));
					float num17 = X.ZLINE(Ef.af, 67f);
					float num18 = 1f - X.ZPOW(num17 - 0.75f, 0.25f);
					mesh.allocVT(4, 6, num16 + 1);
					mesh.Col = mesh.ColGrd.Set(4290692351U).mulA(num18 * 0.33f).C;
					for (int k = 0; k <= num16; k++)
					{
						float num19 = X.ZLINE((float)k, (float)num16);
						float num20 = cam.get_h() / 2f * num19;
						float num21 = 1.5707964f - 6.2831855f * (1f + 1f * num19) * num17;
						float num22 = (13f * X.ZPOW(1f - X.Abs(1f - num19 - num17) * 3.3f) + 4f) * num18;
						float num23 = num20 * X.Cos(num21);
						float num24 = num20 * X.Sin(num21);
						if (this.M2D.Cam.isCoveringEffectPixel(num23, num23, num24, num24, num22 + 2f, mesh.base_x, mesh.base_y))
						{
							mesh.Daia2(num23, num24, num22, 0f, false);
						}
					}
				}
			}
			else
			{
				if (this.ALp.Count == 0)
				{
					return true;
				}
				int num25 = 1;
				Map2d mp = this.ALp[0].Mp;
				Ef.x = cam.x * mp.rCLEN;
				Ef.y = cam.y * mp.rCLEN;
				float num26 = cam.viewable_pixel_width / 2f;
				float num27 = cam.viewable_pixel_height / 2f;
				MeshDrawer mesh2 = Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
				for (int l = 0; l < num25; l++)
				{
					M2LpPuzzManageArea m2LpPuzzManageArea2 = this.ALp[l];
					if (Ef.af < 22f)
					{
						mesh2.Col = mesh2.ColGrd.Set(4285626770U).mulA(1f - X.ZSIN2(Ef.af, 22f)).C;
						float num28 = (m2LpPuzzManageArea2.cx - cam.x) * base_scale;
						float num29 = (m2LpPuzzManageArea2.cy - cam.y) * base_scale;
						mesh2.Rect(num28, -num29, m2LpPuzzManageArea2.width * base_scale, m2LpPuzzManageArea2.height * base_scale, false);
					}
					int num30 = X.IntC((m2LpPuzzManageArea2.width + m2LpPuzzManageArea2.height) * base_scale * 2f / 50f * X.EF_LEVEL_NORMAL);
					float num31 = 265f / (float)num30;
					for (int m = 0; m < num30; m++)
					{
						float num32 = Ef.af - num31 * (float)m;
						if (num32 >= 0f)
						{
							int num33 = (int)(num32 / 265f);
							num32 -= (float)num33 * 265f;
							if (num32 < 125f)
							{
								float num34 = X.ZLINE(num32, 125f);
								uint ran2 = X.GETRAN3(m + num33 * 53, m, 517961);
								Vector3 vector = X.RANBORDER(m2LpPuzzManageArea2.width - 3f, m2LpPuzzManageArea2.height - 3f, X.RAN(ran2, 1241 + m * 3));
								float num35 = (m2LpPuzzManageArea2.cx + vector.x - cam.x) * base_scale + ((CAim._XD((int)vector.z, 1) != 0) ? (X.RAN(ran2, 1190 + m * 4) * 30f - 15f) : 0f);
								float num36 = -(m2LpPuzzManageArea2.cy + vector.y - cam.y) * base_scale + X.NI(40, 60, X.RAN(ran2, 2864 + m * 2)) * num34;
								float num37 = (m2LpPuzzManageArea2.cx - vector.x - cam.x) * base_scale;
								float num38 = -(m2LpPuzzManageArea2.cy - vector.y - cam.y) * base_scale;
								float num39 = (2f + 7f * X.RAN(ran2, 580 + m)) * (0.75f + 0.25f * X.COSI(num32, X.NI(6.76f, 9.22f, X.RAN(ran2, 1789 + m * 5))));
								bool flag = X.BTW(-num26 - 40f, num35, num26 + 40f) && X.BTW(-num27 - 40f, num36, num27 + 40f);
								bool flag2 = X.BTW(-num26 - 130f, num37, num26 + 130f) && X.BTW(-num27 - 130f, num38, num27 + 130f);
								if (flag || flag2)
								{
									mesh2.Col = mesh2.ColGrd.Set(4290692351U).mulA(X.ZLINE(num34, 0.25f) - X.ZLINE(num34 - 0.55f, 0.45f)).C;
									if (flag)
									{
										mesh2.Daia2(num35, num36, num39, 0f, false);
									}
									if (flag2 && m % 3 == 0)
									{
										mesh2.Col = mesh2.ColGrd.mulA(0.4f).C;
										mesh2.ColGrd.setA(0f);
										num39 = X.NI(40, 140, X.RAN(ran2, 2270)) * 0.015625f;
										float num40 = 0.0078125f;
										mesh2.Tri(0, 2, 1, false).Tri(1, 2, 3, false).Tri(2, 4, 3, false)
											.Tri(3, 4, 5, false);
										if (vector.z == 2f || vector.z == 0f)
										{
											num38 = (num38 + (float)X.MPFXP(X.RAN(ran2, 1926)) * X.NI(10, 25, X.RAN(ran2, 1766)) * num34) * 0.015625f;
											num37 /= 64f;
											mesh2.Pos(num37 - num40, num38 - num39, mesh2.ColGrd).Pos(num37 + num40, num38 - num39, mesh2.ColGrd);
											mesh2.Pos(num37 - num40, num38, null).Pos(num37 + num40, num38, null);
											mesh2.Pos(num37 - num40, num38 + num39, mesh2.ColGrd).Pos(num37 + num40, num38, mesh2.ColGrd);
										}
										else
										{
											num37 = (num37 + (float)X.MPFXP(X.RAN(ran2, 1986)) * X.NI(10, 25, X.RAN(ran2, 2705)) * num34) * 0.015625f;
											num38 /= 64f;
											mesh2.Pos(num37 - num39, num38 + num40, mesh2.ColGrd).Pos(num37 - num39, num38 - num40, mesh2.ColGrd);
											mesh2.Pos(num37, num38 + num40, null).Pos(num37, num38 - num40, null);
											mesh2.Pos(num37 + num39, num38 + num40, mesh2.ColGrd).Pos(num37 + num39, num38 - num40, mesh2.ColGrd);
										}
									}
								}
							}
						}
					}
				}
			}
			return true;
		}

		public float _CLEN
		{
			get
			{
				return this.Rc.Mp.CLEN;
			}
		}

		public Map2d _Mp
		{
			get
			{
				return this.Rc.Mp;
			}
		}

		public bool isPuzzleManagingMp()
		{
			return this.barrier_active_ && this.puzz_magic_count_max > -2;
		}

		public bool hasSnapShot()
		{
			return this.ASnapShot.Count > 0;
		}

		public bool isActivePuzzleArea(M2LpPuzzManageArea Lp)
		{
			return this.barrier_active_ && this.ALp.IndexOf(Lp) >= 0;
		}

		public M2LpPuzzManageArea getCalledLp()
		{
			if (this.ALp.Count <= 0)
			{
				return null;
			}
			return this.ALp[0];
		}

		public bool barrier_active
		{
			get
			{
				return this.barrier_active_;
			}
		}

		public readonly NelM2DBase M2D;

		public static PUZ IT;

		private List<M2LpPuzzManageArea> ALp;

		private List<M2LpPuzzManageArea> ALpPre;

		private List<M2CImgDrawerBarrierLit> ADrAll;

		private List<string> Asnap_manage_layer;

		private List<IPuzzActivationListener> APaListener;

		private List<IPuzzSwitchListener> ASwitchListener;

		private M2CImgDrawerBarrierLit NearLit;

		private bool barrier_active_;

		private bool clear_mist;

		private EffectItem Ef;

		private EffectItem EfM;

		private Material MtrBarriearImage;

		private M2Rect Rc;

		private PTCThread efST;

		public int puzz_magic_count_max = -2;

		private const int mirrage_max = 8;

		public const int ID_MAX = 5;

		public static int connector_activated;

		private List<IPuzzRevertable> ARevertable;

		private List<PuzzSnapShot> ASnapShot;
	}
}
