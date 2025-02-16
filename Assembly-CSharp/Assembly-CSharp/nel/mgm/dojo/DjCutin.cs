using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel.mgm.dojo
{
	public class DjCutin
	{
		public DjCutin(DjGM _GM)
		{
			this.GM = _GM;
			this.FD_FnDraw = new FnEffectRun(this.FnDrawCutin);
			this.APto_dojo_rotate_eye = new EfParticleOnce[3];
			for (int i = 0; i < 3; i++)
			{
				this.APto_dojo_rotate_eye[i] = new EfParticleOnce("dojo_rotate_eye" + i.ToString(), EFCON_TYPE.UI);
			}
			this.Pto_dojo_loseg_piece = new EfParticleOnce("dojo_loseg_piece", EFCON_TYPE.UI);
			this.DrRad = new RadiationDrawer
			{
				fine_intv = 0,
				fix_intv_randomize = 0.15f,
				count_ratio = 1.55f
			};
		}

		public void initMaterial()
		{
			if (this.MtrBeam == null)
			{
				this.MtrBeam = MTRX.newMtr(MTR.MtrBeam);
				this.MtrBeam.EnableKeyword("_DARKCENTER_ON");
				this.MtrGrdMap = MTRX.newMtr(MTR.ShaderGDTGradationMap);
				this.MtrGrdMap.mainTexture = this.GM.MI.Tx;
				this.MtrTransitionBite = MTR.newMtr("Hachan/ShaderBiteTransition");
				MTRX.setMaterialST(this.MtrTransitionBite, "_MainTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			}
		}

		public void destruct()
		{
			if (this.MtrBeam != null)
			{
				Object.Destroy(this.MtrGrdMap);
				Object.Destroy(this.MtrBeam);
			}
			if (this.MtrLoseB != null)
			{
				Object.Destroy(this.MtrLoseB);
			}
			BLIT.nDispose(this.RBake);
			this.RBake = null;
		}

		public void setEffect(DjCutin.CUTIN_TYPE type)
		{
			this.deactivate();
			this.Ef = this.GM.get_EF().setEffectWithSpecificFn("CUTIN", 0f, 0f, 0f, (int)type, 0, this.FD_FnDraw);
			this.DrRad.ran0 = X.xors();
			if (type <= DjCutin.CUTIN_TYPE.LOSEB)
			{
				if (type == DjCutin.CUTIN_TYPE.INITG)
				{
					SND.Ui.play("dojo_drumroll", false);
					return;
				}
				if (type != DjCutin.CUTIN_TYPE.LOSEB)
				{
					return;
				}
				this.GM.PtcST("dojo_loseb0", null);
				this.Ef.z = (float)this.GM.HK.cur_hand;
				this.APto_dojo_rotate_eye[X.Mn(2, (int)this.Ef.z)].shuffle();
				return;
			}
			else
			{
				if (type == DjCutin.CUTIN_TYPE.LOSEG)
				{
					SND.Ui.play("dojo_loseg", false);
					NEL.PadVib("mg_dojo_loseb_0", 1f);
					NEL.PadVib("mg_dojo_loseb_1", 1f);
					this.Pto_dojo_loseg_piece.shuffle();
					return;
				}
				if (type != DjCutin.CUTIN_TYPE.WING)
				{
					return;
				}
				this.GM.playVo("dojo_voice_winb");
				this.GM.PtcST("dojo_wing", null);
				return;
			}
		}

		public void deactivate()
		{
			if (this.Ef != null)
			{
				DjCutin.CUTIN_TYPE time = (DjCutin.CUTIN_TYPE)this.Ef.time;
				if (time - DjCutin.CUTIN_TYPE.CD1 > 1)
				{
					if (time == DjCutin.CUTIN_TYPE.LOSEB)
					{
						this.Ef.z = 4f;
					}
					else
					{
						this.Ef.z = 100f;
						this.Ef.af = 0f;
					}
				}
				this.Ef = null;
			}
		}

		public void deactivate(DjCutin.CUTIN_TYPE type)
		{
			if (this.Ef != null && this.Ef.time == (int)type)
			{
				this.deactivate();
			}
		}

		private bool FnDrawCutin(EffectItem Ef)
		{
			if (Ef != this.Ef && this.Ef != null && this.Ef.time == 14)
			{
				return false;
			}
			bool flag = false;
			switch (Ef.time)
			{
			case 1:
				flag = this.drawCutinInitG(Ef);
				break;
			case 2:
				flag = this.drawCutinCountDownGo(Ef);
				break;
			case 3:
			case 4:
				flag = this.drawCutinCountDown(Ef, Ef.time == 3);
				break;
			case 5:
			case 6:
			case 7:
				flag = this.drawCutinBWin(Ef, Ef.time - 5);
				break;
			case 8:
				flag = this.drawCutinBLose(Ef);
				break;
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
				flag = this.drawCutinBLose1(Ef);
				break;
			case 14:
				flag = this.drawCutinGLose(Ef);
				break;
			case 15:
				flag = this.drawCutinGWin(Ef);
				break;
			}
			if (!flag)
			{
				if (Ef == this.Ef)
				{
					this.Ef = null;
				}
				return false;
			}
			return true;
		}

		private bool drawSplash(EffectItem E, int count, float xsh, float ysh, float size_min = 2f, float size_max = 2.5f, float shift_basez = 0f)
		{
			MeshDrawer meshImg = E.GetMeshImg("", MTR.MIiconL, BLEND.MUL, false);
			if (meshImg.getTriMax() == 0)
			{
				meshImg.base_z += 0.002f + shift_basez;
			}
			return this.drawSplash(meshImg, count, xsh, ysh, E.z >= 100f, E.af, E.f0, size_min, size_max);
		}

		private bool drawSplash(MeshDrawer Md, int count, float xsh, float ysh, bool deactivating, float af, int f0, float size_min = 2f, float size_max = 2.5f)
		{
			float num;
			if (!deactivating)
			{
				Md.Col = C32.d2c(4280756007U);
				num = 1f;
			}
			else
			{
				num = 2.6f;
			}
			int num2 = MTR.SqParticleSplashFixed.countFrames();
			bool flag = false;
			int i = 0;
			while (i < count)
			{
				if (!deactivating)
				{
					if (af - (float)i * num > 0f)
					{
						goto IL_0085;
					}
				}
				else
				{
					Md.Col = C32.MulA(4280756007U, 1f - X.ZLINE(af - (float)i * num, 40f));
					if (Md.Col.a > 0)
					{
						goto IL_0085;
					}
				}
				IL_0158:
				i++;
				continue;
				IL_0085:
				flag = true;
				uint ran = X.GETRAN2(f0 + i * 13, 4 + i % 7);
				float num3 = X.RAN(ran, 2616) * xsh * (float)X.MPF(X.RAN(ran, 2587) < 0.5f);
				float num4 = X.RAN(ran, 1322) * ysh * (float)X.MPF(X.RAN(ran, 4114) < 0.5f);
				float num5 = X.NI(size_min, size_max, X.RAN(ran, 1918));
				int num6 = (int)((ulong)ran % (ulong)((long)num2));
				PxlFrame frame = MTR.SqParticleSplashFixed.getFrame(num6);
				Md.RotaPF(num3, num4, num5, num5, X.RAN(ran, 1382) * 6.2831855f, frame, X.RAN(ran, 2523) < 0.5f, false, false, uint.MaxValue, false, 0);
				goto IL_0158;
			}
			return flag;
		}

		private bool drawCutinInitG(EffectItem E)
		{
			bool flag = this.drawSplash(E, 8, 220f, 5f, 2f, 2.5f, 0f) || E.z < 100f;
			PxlFrame textPF = this.getTextPF(DjCutin.SUBTITLE.INITG);
			if (E.z < 100f)
			{
				MeshDrawer meshImg = E.GetMeshImg("", this.GM.MI, BLEND.NORMAL, false);
				float af = E.af;
				DjCutin.drawFlushing(meshImg, 0f, 0f, textPF, E.af, 22f, 1.3f, 3, 3f, 0.6f, uint.MaxValue);
			}
			else
			{
				float num = 1f - X.ZLINE(E.af, 25f);
				if (num > 0f)
				{
					MeshDrawer meshImg2 = E.GetMeshImg("", this.GM.MI, BLEND.NORMAL, false);
					meshImg2.Col = C32.MulA(uint.MaxValue, num);
					meshImg2.RotaPF(0f, 0f, 1f, 1f, 0f, textPF, false, false, false, uint.MaxValue, false, 0);
					flag = true;
				}
			}
			return flag;
		}

		private bool drawCutinCountDown(EffectItem E, bool is_one)
		{
			if (E.z == 0f)
			{
				E.z = X.Mx(20f, BGM.nextbeattiming + 8f);
			}
			float num = X.ZLINE(E.af, E.z);
			if (num >= 1f)
			{
				return false;
			}
			MeshDrawer meshImg = E.GetMeshImg("", MTRX.MIicon, BLEND.MUL, false);
			if (meshImg.getTriMax() == 0)
			{
				meshImg.base_z += 0.002f;
			}
			float num2 = X.ZSIN2(E.af, 8f) * 1.1f - X.ZCOS(E.af - 4f, 14f) * 0.1f;
			meshImg.Col = C32.MulA(4288453788U, 1f - num);
			meshImg.initForImg(MTRX.EffCircle128, 0);
			float num3 = X.NI(120, 155, num);
			meshImg.Rect(0f, 0f, num3, num3 * num2, false);
			MeshDrawer meshImg2 = E.GetMeshImg("", this.GM.MI, BLEND.NORMAL, false);
			meshImg2.Col = C32.MulA(uint.MaxValue, 1f - X.ZLINE(num - 0.5f, 0.5f));
			meshImg2.RotaPF(0f, 0f, 1f, num2, 0f, this.getTextPF(is_one ? DjCutin.SUBTITLE.CD1 : DjCutin.SUBTITLE.CD2), false, false, false, uint.MaxValue, false, 0);
			return true;
		}

		private bool drawCutinCountDownGo(EffectItem E)
		{
			float num = 1f;
			float num2 = 1f;
			float num3 = 0f;
			if (this.Uni == null)
			{
				this.Uni = new UniKiraDrawer();
			}
			this.Uni.kaku = 9;
			this.Uni.Radius(220f, 250f).Dent(0.3f, 0.6f).Focus(0.3f, 0.7f);
			float num4;
			float num5;
			if (E.z < 100f)
			{
				num4 = X.ZLINE(E.af, 9f);
				num5 = X.ZLINE(E.af, 11f);
				float num6 = X.ZSIN(E.af, 9f);
				num = X.NI(1.5f, 1f, num6);
				num2 = X.NI(0.75f, 1f, num6) * (X.BTW(6f, E.af, 9f) ? 1.5f : 1f);
				num3 = (X.ZSIN(E.af - 4f, 10f) - X.ZSIN(E.af - 13f, 6f)) * 1.5707964f * 0.2f;
			}
			else
			{
				num4 = (num5 = 1f - X.ZLINE(E.af, 25f));
				if (num5 <= 0f)
				{
					return false;
				}
			}
			MeshDrawer mesh = E.GetMesh("GOB", 4278190080U, BLEND.MUL, false);
			MeshDrawer meshImg = E.GetMeshImg("GO", this.GM.MI, BLEND.NORMAL, false);
			mesh.base_z -= 0.03f;
			meshImg.base_z -= 0.04f;
			mesh.Col = C32.MulA(4278190080U, num4);
			mesh.Scale(num * 0.8f, num * 0.4f, false).Rotate(0.18849556f, false);
			this.Uni.drawTo(mesh, 0f, 0f, 0f, false, 0f, 0f);
			meshImg.Col = C32.MulA(uint.MaxValue, num5);
			meshImg.RotaPF(0f, 0f, num2, num2, num3, this.getTextPF(DjCutin.SUBTITLE.GO), false, false, false, uint.MaxValue, false, 0);
			return true;
		}

		private bool drawCutinBWin(EffectItem E, int hand_type)
		{
			if (this.Ef != null && E != this.Ef)
			{
				return false;
			}
			float num = 1f;
			float num2 = E.af;
			if (E.z >= 100f)
			{
				num2 = 100f;
				num = 1f - X.ZLINE(E.af, 10f);
				if (num <= 0f)
				{
					return false;
				}
			}
			E.y = 0.46875f;
			MeshDrawer mesh = E.GetMesh("bwin_b", this.MtrBeam, false);
			MeshDrawer meshImg = E.GetMeshImg("bwin_main", this.GM.MI, BLEND.NORMALP3, false);
			mesh.base_z += 0.04f;
			meshImg.base_z -= 0.04f;
			mesh.Col = MTRX.ColBlack;
			float num3 = X.NI(0.14f, 1f, X.ZSIN2(num2, 13f)) * 140f;
			float num4 = X.ZSIN(num2, 8f);
			mesh.Col = MTRX.ColBlack;
			mesh.initForImg(MTRX.MIicon);
			uint num5 = DjRPC.i2col(hand_type);
			mesh.Scale(1f, X.ZPOW3(num), false);
			Matrix4x4 currentMatrix = mesh.getCurrentMatrix();
			for (int i = 0; i < 10; i++)
			{
				uint ran = X.GETRAN2(E.f0 + i, (int)(1U + E.index % 7U + (uint)(i % 23)));
				mesh.Uv23(mesh.ColGrd.Set(num5).blend(0U, 0.5f + X.COSI(num2 + (float)(i * 11), 150f + X.RAN(ran, 1994) * 20f)).setA(0f)
					.C, false);
				float num6 = -num3 + num3 * 2f * ((float)i + 0.2f + X.RAN(ran, 1360) * 0.6f) / 10f;
				float num7 = X.NI(-1, 1, X.RAN(ran, 2966)) * 30f;
				float num8 = X.NI(-1, 1, X.RAN(ran, 869)) * 3.1415927f * 0.03f;
				float num9 = num4 * X.NI(0.7f, 1f, X.RAN(ran, 2079)) * 240f;
				mesh.setCurrentMatrix(currentMatrix, false);
				mesh.TranslateP(num7, num6, true).Rotate(num8, true);
				mesh.Rect(0f, 0f, IN.w + 240f, num9, false);
				mesh.allocUv23(0, true);
			}
			if (num2 >= 4f)
			{
				float num10 = num2 - 4f;
				MeshDrawer mesh2 = E.GetMesh("bwin_grd", this.MtrGrdMap, false);
				mesh2.Col = MTRX.ColGray;
				this.MtrGrdMap.SetColor("_ColorW", mesh2.ColGrd.Set(uint.MaxValue).blend(num5, X.ZSIN(1f - num)).C);
				this.MtrGrdMap.SetColor("_Color", mesh2.ColGrd.Set(uint.MaxValue).blend(num5, X.ZSIN(num10, 40f) * 0.8f).C);
				this.MtrGrdMap.SetColor("_ColorB", mesh2.ColGrd.Set(uint.MaxValue).blend(meshImg.ColGrd.Set(num5).multiply(X.NI(1f, 0.5f, X.ZSIN(num10, 25f)), false), X.ZSIN(num10, 20f)).C);
				float num11 = (0.125f + X.ZPOW(num10, 5f) * 0.375f + X.ZSIN2(num10 - 3f, 7f) * 0.5f) * 190f * X.ZPOW(num);
				mesh2.Rotate(0.025132744f, false);
				mesh2.BoxPattern(0f, 0f, IN.w + 80f, num11 * 2f, X.ANMP(IN.totalframe, 20, 1f) * 248f, 0f, 2f, 2f, this.GM.SqPat.getImage(0, 0), false);
				PxlFrame frame = this.GM.SqBWinCutin.getFrame(hand_type);
				meshImg.allocUv23(4, false);
				meshImg.Uv23(meshImg.ColGrd.White().multiply(1f - X.ZLINE(num10 - 3f, 14f), false).setA(0f)
					.C, false);
				meshImg.Col = C32.WMulA(num);
				if (X.BTW(10f, num10, 13f))
				{
					meshImg.Scale(1.05f, 1.05f, false);
					meshImg.Rotate(0.03455752f, false);
				}
				meshImg.TranslateP(400f * (1f - X.ZSIN2(num10, 15f)) - 620f * X.ZSIN(1f - num) + (float)X.IntR(3f * X.COSI((float)((int)(this.GM.t_state / 2f)), 4.3f)), (float)X.IntR(3f * X.COSI((float)((int)(this.GM.t_state / 2f)), 5.3f)), false);
				DjCutin.drawFlushing(meshImg, 0f, 0f, frame, num10 - 3f, 12f, 1.15f, 1, 0.5f, 0.7f, uint.MaxValue);
				meshImg.allocUv23(0, true);
			}
			if (num2 < 20f)
			{
				meshImg.allocUv23(24, false);
				meshImg.Uv23(MTRX.ColTrnsp, false);
				meshImg.Col = C32.WMulA((1f - X.ZLINE(num2 - 8f, 12f)) * num);
				DjCutin.drawFlushing(meshImg, 0f, 0f, this.getTextPF(DjCutin.SUBTITLE.GOOD), num2, 12f, 1.3f, 2, 2f, 0.7f, uint.MaxValue);
				meshImg.allocUv23(0, true);
			}
			return true;
		}

		private bool drawCutinBLose(EffectItem E)
		{
			E.y = IN.hh * 0.42f * 0.015625f;
			if (E.af < 70f && E.z < 3f)
			{
				MeshDrawer mesh = E.GetMesh("bwin_b", this.MtrBeam, false);
				mesh.base_z += 0.04f;
				mesh.allocUv23(4, false);
				mesh.Col = MTRX.ColBlack;
				mesh.initForImg(MTRX.MIicon);
				uint num = DjRPC.i2col((int)E.z);
				mesh.Uv23(mesh.ColGrd.Set(num).setA(0f).C, false);
				mesh.Rect(0f, 0f, IN.w, 290f * X.ZSIN2(E.af, 10f), false);
				mesh.allocUv23(0, true);
				if (E.af > 5f)
				{
					float num2 = E.af - 5f;
					float num3 = 250f * X.ZSIN(E.af - 5f, 14f);
					MeshDrawer mesh2 = E.GetMesh("bwin_grd", this.MtrGrdMap, false);
					MeshDrawer mesh3 = E.GetMesh("bwin_grd", MTRX.getMtr(BLEND.MASK, 250), false);
					mesh3.Col = MTRX.ColTrnsp;
					mesh2.Col = MTRX.ColGray;
					this.MtrGrdMap.SetColor("_ColorW", mesh2.ColGrd.Set(num).C);
					this.MtrGrdMap.SetColor("_Color", mesh2.ColGrd.Set(num).multiply(0.5f, false).C);
					this.MtrGrdMap.SetColor("_ColorB", mesh2.ColGrd.Set(0).C);
					mesh3.Rect(0f, 0f, IN.w, num3, false);
					mesh2.BoxPattern(0f, 0f, IN.w + 80f, num3, (1f - X.ANMP(IN.totalframe, 20, 1f)) * 248f, 0f, 2f, 2f, this.GM.SqPat.getImage(0, 0), false);
					MeshDrawer mesh4 = E.GetMesh("bwin_s", this.GM.MI.getMtr(BLEND.NORMAL, 250), false);
					MeshDrawer meshDrawer = null;
					mesh4.base_z -= 0.04f;
					float num4 = IN.wh * (0.9f - 0.3f * X.ZSIN(num2, 11f) - X.ZLINE(E.af, 70f) * 0.1f - 1.58f * X.ZPOW(E.af - 50f, 17f));
					PxlFrame frame = this.GM.SqBLoseCutin.getFrame(0);
					for (int i = 0; i < 3; i++)
					{
						PxlLayer layer = frame.getLayer(i);
						float num5 = num4;
						float num6 = 0f;
						if (i >= 1)
						{
							if (num2 >= 10f)
							{
								float num7 = num2 - 10f;
								if (meshDrawer == null)
								{
									meshDrawer = E.GetMeshImg("blose_eyel", MTR.MIiconP, BLEND.NORMAL, false);
									meshDrawer.base_z -= 0.05f;
								}
								this.APto_dojo_rotate_eye[(int)E.z].drawTo(meshDrawer, meshDrawer.base_px_x + num5 + layer.x, meshDrawer.base_px_y + num6 - layer.y, 60f, 22, num7, 0f);
							}
							num5 += (float)X.IntR(2f * X.COSI(this.GM.t_state + (float)(i * 33), 4.7f));
							num6 += (float)X.IntR(3f * X.COSI(this.GM.t_state + (float)(i * 89), 6.11f));
						}
						mesh4.RotaL(num5, num6, layer, false, false, 0);
					}
					this.drawBiteTrans(E, X.ZPOW(E.af - 78f, -8f), false, 4278190080U);
				}
			}
			else
			{
				if (E.z < 100f)
				{
					DjCutin.CUTIN_TYPE cutin_TYPE;
					if (this.GM.changeCutinLoseB(out cutin_TYPE))
					{
						E.time = (int)cutin_TYPE;
						E.x = (E.y = 0f);
						E.af = 0f;
						E.z = 0f;
						this.drawBiteTrans(E, 1f, false, 4278190080U);
						return true;
					}
					E.z = 100f;
					E.af = X.Mx(E.af, 70f);
				}
				if (E.af >= 98f)
				{
					return false;
				}
				this.drawBiteTrans(E, -1f + X.ZSIN(E.af - 70f, 28f), false, 4278190080U);
			}
			return true;
		}

		private bool drawCutinBLose1(EffectItem E)
		{
			float num = E.af;
			float num2 = 1f;
			if (E.z >= 100f)
			{
				num = 1000f;
				if (E.af >= 70f)
				{
					return false;
				}
				num2 *= 1f - X.ZLINE(E.af, 70f);
			}
			else
			{
				this.drawBiteTrans(E, -1f + X.ZSIN(E.af, 28f), false, 4278190080U);
				if (E.z < 5f)
				{
					E.z = 5f;
					SND.Ui.play("dojo_loseb1", false);
					NEL.PadVib("mg_dojo_loseb_0", 1f);
					NEL.PadVib("mg_dojo_loseb_1", 1f);
				}
			}
			uint lose_bits = this.GM.lose_bits;
			int time = E.time;
			int num3 = this.GM.LifePr.get_life(true);
			MeshDrawer mesh = E.GetMesh("top", uint.MaxValue, BLEND.NORMAL, false);
			mesh.base_z -= 0.04f;
			if (num < 40f)
			{
				mesh.Col = C32.WMulA(1f - X.ZLINE(num, 40f));
				mesh.Rect(0f, 0f, IN.w + 40f, IN.h + 40f, false);
			}
			if (num < 99f)
			{
				mesh.Col = C32.WMulA(X.ZLINE(num, 25f) - X.ZLINE(num - 20f, 79f));
				this.DrRad.drawTo(mesh, 0f, 0f, IN.w + 80f, IN.h + 80f, 18f, 0f, false, 0.7f);
			}
			Color32 color = MTRX.ColTrnsp;
			float num4 = X.ZSIN(num, 44f) * 0.875f + X.ZSIN(num, 120f) * 0.125f;
			float num5 = num4 * num4;
			float num6 = (float)X.IntR(num5 * 18f * X.COSI(this.GM.t_state, 395f));
			float num7 = (float)X.IntR(num5 * 14f * X.COSI(this.GM.t_state + 55f, 473f));
			float num8 = 0f;
			float num9 = 0f;
			float num10 = -0.47123894f * (1f - num4);
			float num11 = X.NI(2, 1, num4) * 0.8f;
			float num12 = 1f;
			if (X.BTW(22f, num, 26f))
			{
				color = MTRX.cola.multiply(0.33f, false).setA(0f).C;
				num10 -= 0.04712389f;
				num11 += 0.019f;
				num12 *= 1.4f;
			}
			if (X.BTW(33f, num, 37f))
			{
				color = MTRX.cola.multiply(0.33f, false).setA(0f).C;
				num10 -= 0.009424779f;
				num11 += 0.004f;
				num12 *= 1.4f;
			}
			if (X.BTW(22f, num, 35f))
			{
				float num13 = X.ZPOW(num - 22f, 13f);
				num12 *= 5f * X.NI(1f, 0.6f, num13);
				num8 = (float)X.IntR(num12 * X.COSI((float)((int)(this.GM.t_state / 2f)), 5.78f));
				num9 = (float)X.IntR(num12 * X.COSI((float)((int)(this.GM.t_state / 2f)), 3.49f));
			}
			PxlFrame frame = this.GM.SqBLoseCutin.getFrame(1 + E.time - 9);
			this.BakeLoseImage(frame, E.time, num6 + num8, num7 + num9, num11, num10);
			MeshDrawer mesh2 = E.GetMesh("blose", this.MtrLoseB, false);
			mesh2.Col = C32.WMulA(num2);
			mesh2.allocUv23(8, false);
			mesh2.Uv23(color, false);
			mesh2.initForImg(this.RBake).Rect(0f, 0f, IN.w, IN.h, false);
			mesh2.allocUv23(0, true);
			if (X.BTW(22f, num, 42f))
			{
				float num14 = X.ZSIN(num - 22f, 20f);
				mesh2.Uv23(mesh2.ColGrd.White().multiply(0.5f, false).setA(0f)
					.C, false);
				float num15 = num11 * X.NI(1f, 1.25f, num14);
				mesh2.Col = C32.WMulA(1f - num14);
				mesh2.RotaGraph(1f, 1f, num15, 0f, null, false);
				mesh2.allocUv23(0, true);
			}
			if (X.BTW(55f, num, 218f))
			{
				float num16 = num - 55f;
				MeshDrawer meshImg = E.GetMeshImg("", MTR.MIiconL, BLEND.NORMAL, false);
				MeshDrawer meshImg2 = E.GetMeshImg("ptt", this.GM.MI, BLEND.NORMAL, false);
				meshImg2.base_z -= 0.25f;
				meshImg.base_z -= 0.007999999f;
				meshImg.Col = MTRX.ColBlack;
				switch (E.time)
				{
				case 9:
				case 10:
					meshImg.base_px_x = -IN.wh * 0.4f;
					meshImg.base_px_y = IN.hh * 0.14f;
					goto IL_061E;
				case 11:
					meshImg.base_px_x = -IN.wh * 0.4f;
					meshImg.base_px_y = -IN.hh * 0.34f;
					goto IL_061E;
				case 13:
					meshImg.base_px_x = IN.wh * 0.4f;
					meshImg.base_px_y = IN.hh * 0.35f;
					goto IL_061E;
				}
				meshImg.base_px_x = IN.wh * 0.4f;
				meshImg.base_px_y = IN.hh * 0.24f;
				IL_061E:
				meshImg2.base_x = meshImg.base_x;
				meshImg2.base_y = meshImg.base_y;
				float num17 = num16;
				if (num16 >= 105f)
				{
					num17 -= 105f;
					meshImg2.Col = C32.WMulA((1f - X.ZLINE(num17, 58f)) * num2);
				}
				else
				{
					meshImg2.Col = C32.WMulA(num2);
				}
				this.drawSplash(meshImg, 8, 70f, 70f, num16 >= 105f, num17, E.f0, 2.7f, 3.5f);
				DjCutin.drawFlushing(meshImg2, 0f, 0f, this.getTextPF(DjCutin.SUBTITLE.LOSEB_FAST + (E.time - 9)), num16, 24f, 1.3f, 2, 6f, 0.7f, uint.MaxValue);
			}
			return true;
		}

		private uint cutinVisibleLayer(int type, out uint sensitive_layer_bits)
		{
			bool flag = !this.GM.LifePr.is_alive_c;
			bool flag2 = (this.GM.lose_bits & (1U << type)) > 0U;
			uint num = 1U;
			sensitive_layer_bits = 0U;
			switch (type)
			{
			case 9:
			case 10:
				if (flag2)
				{
					num |= 4U;
					if (flag)
					{
						num |= 96U;
					}
					else
					{
						num |= 24U;
					}
				}
				else
				{
					num |= 2U;
					if (flag)
					{
						num |= 64U;
					}
				}
				if (X.sensitive_level >= 2)
				{
					sensitive_layer_bits |= 256U;
				}
				else if (X.sensitive_level >= 1 && flag2)
				{
					sensitive_layer_bits |= 128U;
				}
				break;
			case 11:
				num |= ((flag || flag2) ? 4U : 2U);
				if (X.sensitive_level >= 2)
				{
					sensitive_layer_bits |= 8U;
				}
				break;
			case 12:
				num |= ((flag || flag2) ? 2U : 0U);
				if (this.GM.loseb_bra_off)
				{
					num |= 4U;
				}
				if (X.sensitive_level >= 2)
				{
					sensitive_layer_bits |= 8U;
				}
				break;
			case 13:
				num |= (flag ? 4U : (flag2 ? 2U : 0U));
				if (X.sensitive_level >= 2)
				{
					sensitive_layer_bits |= 24U;
				}
				break;
			case 14:
				if (this.GM.loseb_bra_off)
				{
					num |= 2U;
				}
				if (this.GM.loseb_pantsu_off)
				{
					num |= 4U;
				}
				if (X.sensitive_level >= 2)
				{
					sensitive_layer_bits |= 24U;
				}
				break;
			default:
				return uint.MaxValue;
			}
			return num;
		}

		private void BakeLoseImage(PxlFrame PF, int cutin_type, float x, float y, float z, float agR)
		{
			BLIT.Alloc(ref this.RBake, Screen.width, Screen.height, true, RenderTextureFormat.ARGB32, 0);
			if (this.MtrLoseB == null)
			{
				this.MtrLoseB = new Material(MTRX.ShaderGDTP3);
			}
			if (this.prepareBakeMesh(ref this.MdBake))
			{
				this.MdBake.activate("BAKE", this.GM.MI.getMtr(BLEND.NORMAL, -1), false, MTRX.ColWhite, null);
			}
			this.MtrLoseB.mainTexture = this.RBake;
			uint num;
			this.MdBake.RotaPF(x, y, z, z, agR, PF, false, false, false, this.cutinVisibleLayer(cutin_type, out num), false, 0);
			Graphics.SetRenderTarget(this.RBake);
			Camera guicamera = IN.getGUICamera();
			GL.PushMatrix();
			BLIT.RenderToGLMtr(this.MdBake, 0f, 0f, 1f, this.MdBake.getMaterial(), guicamera.projectionMatrix * guicamera.worldToCameraMatrix, -1, false, false);
			if (num > 0U)
			{
				this.MdBake.clearSimple();
				this.MdBake.RotaPF(x, y, z, z, agR, PF, false, false, false, num, false, 0);
				BLIT.RenderToGLMtr(this.MdBake, 0f, 0f, 1f, this.MdBake.getMaterial(), guicamera.projectionMatrix * guicamera.worldToCameraMatrix, -1, false, false);
			}
			GL.PopMatrix();
		}

		private bool prepareBakeMesh(ref MeshDrawer MdBake)
		{
			if (MdBake == null)
			{
				MdBake = new MeshDrawer(null, 24, 36);
				MdBake.draw_gl_only = true;
				return true;
			}
			MdBake.clear(false, false);
			return false;
		}

		private bool drawCutinGLose(EffectItem E)
		{
			float num;
			float num2;
			if (E.z < 100f)
			{
				num = E.af;
				num2 = 1f;
			}
			else
			{
				num = 900f;
				if (E.af >= 60f)
				{
					return false;
				}
				num2 = 1f - X.ZLINE(E.af, 60f);
			}
			if (num < 10f)
			{
				MeshDrawer mesh = E.GetMesh("", uint.MaxValue, BLEND.NORMAL, false);
				mesh.Col = C32.WMulA(X.ZSIN(num, 10f));
				mesh.Rect(0f, 0f, IN.w + 40f, IN.h + 40f, false);
			}
			else
			{
				num -= 10f;
				if (num < 12f)
				{
					MeshDrawer mesh2 = E.GetMesh("", uint.MaxValue, BLEND.NORMAL, false);
					mesh2.Col = MTRX.ColBlack;
					mesh2.Rect(0f, 0f, IN.w + 40f, IN.h + 40f, false);
					MeshDrawer meshImg = E.GetMeshImg("", this.GM.MI, BLEND.ADD, false);
					meshImg.base_z -= 0.005f;
					PxlFrame frame = this.GM.SqGLoseFlash.getFrame((int)((float)this.GM.SqGLoseFlash.countFrames() * num / 12f));
					for (int i = -1; i <= 1; i++)
					{
						meshImg.Identity().Scale(2f, 2f, false);
						meshImg.Col = C32.d2c((i == -1) ? 4294901760U : ((i == 0) ? 4278255360U : 4278190335U));
						if (i != 0)
						{
							meshImg.Scale(1.018f, 1f, false).TranslateP((float)(i * 14), 0f, false);
						}
						meshImg.RotaPF(0f, 0f, 1f, 1f, 0f, frame, false, false, false, uint.MaxValue, false, 0);
					}
				}
				else
				{
					num -= 12f;
					this.drawGWinBg(E, num, num2, uint.MaxValue, 4292825771U, uint.MaxValue, 4291727551U);
					if (num < 32f)
					{
						MeshDrawer mesh3 = E.GetMesh("_top", 4278190080U, BLEND.NORMAL, false);
						mesh3.base_z -= 0.025f;
						mesh3.Col = C32.MulA(MTRX.ColBlack, 1f - X.ZSIN(num, 32f));
						mesh3.Rect(0f, 0f, IN.w + 40f, IN.h + 40f, false);
						this.drawBiteTrans(E, -1f + X.ZSIN(num + 3f, 14f), true, uint.MaxValue);
					}
					PxlFrame frame2 = this.GM.SqBLoseCutin.getFrame(6);
					float num3 = X.ZSIN(num, 48f) * 0.75f + X.ZSIN(num, 240f) * 0.25f;
					float num4 = 1f - X.ZSIN2(num, 30f);
					float num5 = X.NI(-0.25132743f, 0f, num3);
					float num6 = X.NI(1.2f, 0.8f, num3) + 2.8f * num4;
					this.BakeLoseImage(frame2, E.time, 0f, 1820f * (1f - X.ZSIN2(num, 42f)), num6, num5);
					MeshDrawer mesh4 = E.GetMesh("blose", this.MtrLoseB, false);
					mesh4.Col = C32.WMulA(num2);
					mesh4.allocUv23(4, false);
					mesh4.Uv23(mesh4.ColGrd.White().multiply(X.NI(0.5f, 0f, num3), true).setA(0f)
						.C, false);
					mesh4.initForImg(this.RBake);
					mesh4.Rect(0f, 0f, IN.w, IN.h, false);
					mesh4.allocUv23(0, false);
					if (num < this.Pto_dojo_loseg_piece.loop_maxt)
					{
						MeshDrawer meshImg2 = E.GetMeshImg("", MTR.MIiconL, BLEND.NORMAL, false);
						meshImg2.base_z -= 0.04f;
						this.Pto_dojo_loseg_piece.drawTo(meshImg2, 0f, 0f, 0f, 0, num, 0f);
					}
					if (num >= 62f)
					{
						MeshDrawer meshImg3 = E.GetMeshImg("", this.GM.MI, BLEND.NORMALP3, false);
						float num7 = num - 80f;
						PxlFrame textPF = this.getTextPF(DjCutin.SUBTITLE.LOSEG);
						meshImg3.allocUv23(16, false);
						meshImg3.Uv23(MTRX.ColTrnsp, false);
						int num8 = this.drawGameSuccess(E, meshImg3, textPF, num7, num2);
						if (E.z < (float)num8)
						{
							E.z = (float)num8;
							SND.Ui.play("dojo_loseg_taiko", false);
						}
						meshImg3.allocUv23(0, true);
					}
				}
			}
			return true;
		}

		private void drawGWinBg(EffectItem E, float t, float alpha, uint col_s_b, uint col_e_b, uint col_s_uni, uint col_e_uni)
		{
			if (this.Uni == null)
			{
				this.Uni = new UniKiraDrawer();
			}
			this.Uni.kaku = 16;
			MeshDrawer mesh = E.GetMesh("", uint.MaxValue, BLEND.NORMAL, true);
			mesh.Col = mesh.ColGrd.Set(col_s_b).blend(col_e_b, X.ZSIN(t - 20f, 60f)).mulA(alpha)
				.C;
			mesh.Rect(0f, 0f, IN.w + 40f, IN.h + 40f, false);
			mesh.Col = mesh.ColGrd.Set(col_s_uni).blend(col_e_uni, X.ZSIN(t - 4f, 70f)).mulA(alpha)
				.C;
			float num = X.ZSIN(t, 50f);
			float num2 = X.NI(1390, 670, num);
			this.Uni.Radius(num2, num2).Dent(0.3f, 0.3f).Focus(0.5f, 0.5f);
			mesh.Scale(1f, 0.75f, false);
			this.Uni.drawTo(mesh, 0f, 0f, X.ANMPT(210, 1f) * 6.2831855f, false, 0f, 0f);
		}

		private bool drawCutinGWin(EffectItem E)
		{
			float num;
			float num2;
			if (E.z < 100f)
			{
				num = E.af;
				num2 = 1f;
			}
			else
			{
				num = 900f;
				if (E.af >= 40f)
				{
					return false;
				}
				num2 = 1f - X.ZLINE(E.af, 40f);
			}
			this.drawGWinBg(E, num, num2, 4284840176U, 4290763005U, 4282832639U, 4293391871U);
			if (num < 70f)
			{
				MeshDrawer mesh = E.GetMesh("_top", uint.MaxValue, BLEND.NORMAL, false);
				if (num < 32f)
				{
					mesh.base_z -= 0.025f;
					mesh.Col = C32.WMulA(1f - X.ZSIN(num, 32f));
					mesh.Rect(0f, 0f, IN.w + 40f, IN.h + 40f, false);
				}
				mesh.Col = C32.WMulA(X.ZLINE(num, 25f) - X.ZLINE(num - 20f, 50f));
				this.DrRad.drawTo(mesh, 0f, 0f, IN.w + 80f, IN.h + 80f, 18f, 0f, false, 0.7f);
			}
			MeshDrawer meshImg = E.GetMeshImg("wing_cutin", this.GM.MI, BLEND.NORMALP3, false);
			meshImg.base_z += 0.01f;
			meshImg.allocUv23(8, false);
			meshImg.Col = C32.WMulA(num2);
			meshImg.Uv23(meshImg.ColGrd.White().multiply(X.NI(0.5f, 0f, X.ZSIN(num, 60f)), true).setA(0f)
				.C, false);
			float num3 = ((num >= 60f) ? 0f : (1f - ((num >= 5f) ? 0.5f : 0f) - X.ZPOW(num - 5f, 70f) * 0.4f));
			float num4 = (float)X.IntR(num3 * 9f * X.COSI(this.GM.t_state, 6.51f));
			float num5 = (float)X.IntR(num3 * 9f * X.COSI(this.GM.t_state + 74f, 4.14f));
			float num6 = X.ZSIN(num, 45f) * 0.875f + X.ZSIN(num, 140f) * 0.125f;
			float num7 = X.NI(1.25f, 0.8f, num6);
			float num8 = X.NI(1.5f, 1f, X.ZSIN(num, 45f) * 0.75f + X.ZSIN(num, 120f) * 0.25f);
			meshImg.TranslateP(0f, -250f - 600f * (1f - num6) + 520f * X.ZSIN(num, 430f), false).Scale(num7, num7, false);
			meshImg.RotaPF(num4, num5 + IN.hh * 0.7f + X.ZSIN2(num, 36f) * 260f + X.ZSIN(num, 520f) * 70f, num8, num8, (-0.1f + X.ZSIN(num, 76f) * 0.13f + X.ZSIN(num, 420f) * 0.04f) * 3.1415927f, this.GM.SqGWinCutin.getFrame(0), false, false, false, uint.MaxValue, false, 0);
			meshImg.RotaPF(num4, num5, 1f, 1f, 0f, this.GM.SqGWinCutin.getFrame(1), false, false, false, uint.MaxValue, false, 0);
			meshImg.allocUv23(0, true);
			if (num >= 100f)
			{
				float num9 = num - 100f;
				PxlFrame textPF = this.getTextPF(DjCutin.SUBTITLE.WING);
				meshImg.Identity();
				meshImg.allocUv23(16, false);
				meshImg.Uv23(MTRX.ColTrnsp, false);
				int num10 = this.drawGameSuccess(E, meshImg, textPF, num9, num2);
				if (E.z < (float)num10)
				{
					E.z = (float)num10;
					SND.Ui.play("dojo_wing_taiko", false);
				}
				meshImg.allocUv23(0, true);
			}
			return true;
		}

		public void drawBiteTrans(EffectItem E, float level, bool door_open = false, uint col = 4278190080U)
		{
			if (level == 0f)
			{
				return;
			}
			MeshDrawer meshDrawer;
			if (level == 1f)
			{
				meshDrawer = E.GetMesh("bts", MTRX.MtrMeshNormal, false);
			}
			else
			{
				meshDrawer = E.GetMesh("bts", this.MtrTransitionBite, false);
				meshDrawer.setMtrFloat("_Level", level);
			}
			meshDrawer.base_x = (meshDrawer.base_y = 0f);
			meshDrawer.base_z -= 0.08f;
			meshDrawer.Col = C32.d2c(4278190080U);
			meshDrawer.initForImg(MTRX.MIicon);
			if (!door_open)
			{
				meshDrawer.allocUv2(4, false);
				meshDrawer.Rect(0f, 0f, IN.w, IN.h, false);
				meshDrawer.Uv2(1f, 0f, false);
				meshDrawer.allocUv2(0, true);
				return;
			}
			meshDrawer.allocUv2(8, false);
			meshDrawer.Rect(-IN.wh * 0.5f, 0f, IN.wh, IN.h, false);
			meshDrawer.Uv2(-1f, 0f, false);
			meshDrawer.allocUv2(0, true);
			meshDrawer.Rect(IN.wh * 0.5f, 0f, IN.wh, IN.h, false);
			meshDrawer.Uv2(1f, 0f, false);
			meshDrawer.allocUv2(0, true);
		}

		private int drawGameSuccess(EffectItem E, MeshDrawer MdS, PxlFrame PFS, float t, float alpha)
		{
			MeshDrawer meshImg = E.GetMeshImg("", MTR.MIiconL, BLEND.NORMAL, true);
			meshImg.base_z -= 0.008f;
			int num = 0;
			int num2 = PFS.countLayers();
			for (int i = 0; i < num2; i++)
			{
				int num3 = i * 22;
				float num4 = t - (float)num3;
				if (num4 < 0f)
				{
					break;
				}
				float num5 = X.ZPOW(num4, 6f);
				float num6 = X.NI(1.4f, 1f, num5);
				PxlLayer layer = PFS.getLayer(i);
				meshImg.base_px_x = (MdS.base_px_x = layer.x);
				meshImg.base_px_y = (MdS.base_px_y = -layer.y);
				this.drawSplash(meshImg, 3, 10f, 10f, E.z >= 100f, (E.z >= 100f) ? E.af : num4, E.f0 + i * 43, 1.8f, 2.2f);
				MdS.Col = C32.WMulA(alpha);
				MdS.initForImg(layer.Img, 0);
				MdS.RotaGraph(0f, 0f, num6, X.BTW(6f, num4, 9f) ? 0.25132743f : 0f, null, false);
				if (num4 >= 5f)
				{
					num = i + 1;
					float num7 = X.ZSIN(num4 - 8f, 15f);
					MdS.Col = C32.WMulA((1f - num7) * alpha);
					if (MdS.Col.a > 0)
					{
						MdS.RotaGraph(0f, 0f, num6 * X.NI(1f, 1.8f, num7), 0f, null, false);
					}
				}
			}
			return num;
		}

		public static void drawFlushing(MeshDrawer Md, float dx, float dy, PxlFrame PF, float af, float fade_t = 14f, float enlarge_scale = 1.3f, int count = 1, float delay = 4f, float alpha_mul = 1f, uint draw_frame_bits = 4294967295U)
		{
			Color32 col = Md.Col;
			Md.RotaPF(dx, dy, 1f, 1f, 0f, PF, false, false, false, draw_frame_bits, false, 0);
			for (int i = 0; i < count; i++)
			{
				float num = af - delay * (float)i;
				if (num >= 0f && num < fade_t)
				{
					float num2 = X.ZSIN(num, fade_t);
					Md.Col = C32.MulA(Md.Col, (1f - num2) * alpha_mul);
					float num3 = X.NI(1f, enlarge_scale, num2);
					Md.RotaPF(dx, dy, num3, num3, 0f, PF, false, false, false, draw_frame_bits, false, 0);
				}
			}
			Md.Col = col;
		}

		private PxlFrame getTextPF(DjCutin.SUBTITLE t)
		{
			return this.GM.SqTexts.getFrame((int)t);
		}

		public const uint _LOSE_BIT_BRA_OFF = 512U;

		public const uint _LOSE_BIT_PANTSU_OFF = 16384U;

		public readonly DjGM GM;

		private FnEffectRun FD_FnDraw;

		private EffectItem Ef;

		private RenderTexture RBake;

		private MeshDrawer MdBake;

		private MeshDrawer MdBakeAdd;

		private UniKiraDrawer Uni;

		private Material MtrBeam;

		private Material MtrTransitionBite;

		private Material MtrGrdMap;

		private Material MtrLoseB;

		private EfParticleOnce[] APto_dojo_rotate_eye;

		private EfParticleOnce Pto_dojo_loseg_piece;

		private readonly RadiationDrawer DrRad;

		private const int pattern_x = 248;

		public enum CUTIN_TYPE
		{
			OFFLINE,
			INITG,
			GO,
			CD1,
			CD2,
			WINB_RK,
			WINB_SC,
			WINB_PA,
			LOSEB,
			LOSEB_FAST,
			LOSEB_SLOW,
			LOSEB_RK,
			LOSEB_SC,
			LOSEB_PA,
			LOSEG,
			WING
		}

		public enum SUBTITLE
		{
			GO,
			CD1,
			CD2,
			INITG,
			EFF_RK,
			EFF_SC,
			EFF_PA,
			BAD,
			GOOD,
			LOSEG,
			WING,
			LOSEB_FAST,
			LOSEB_SLOW,
			LOSEB_RK,
			LOSEB_SC,
			LOSEB_PA
		}

		private enum LBL
		{
			FS_BASE,
			FS_HAND0,
			FS_MATA,
			FS_HAND1,
			FS_EMO1,
			FS_HAND2,
			FS_EMO2,
			FS_SENSITIVE_0,
			FS_SENSITIVE_1,
			RK_BASE = 0,
			RK_EMO0,
			RK_EMO1,
			RK_SENSITIVE_1,
			SC_BASE = 0,
			SC_EMO1,
			SC_NOBRA,
			SC_SENSITIVE_1,
			PA_BASE = 0,
			PA_EMO1,
			PA_EMO2,
			PA_SENSITIVE_1,
			PA_SENSITIVE_2,
			LG_BASE = 0,
			LG_NOBRA,
			LG_NOPAN,
			LG_SENSITIVE_1,
			LG_SENSITIVE_2
		}
	}
}
