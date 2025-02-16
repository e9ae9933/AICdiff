using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class ReelExecuter : IEfPInteractale
	{
		public ReelExecuter(ReelManager _Con, ReelExecuter.ETYPE _etype)
		{
			this.Con = _Con;
			this.etype = _etype;
			if (this.etype > ReelExecuter.ETYPE.ITEMKIND)
			{
				this.IconPF = MTR.SqReelIcon.getFrame(this.etype - ReelExecuter.ETYPE.GRADE1);
			}
			if (ReelExecuter.PFFrm0L == null)
			{
				ReelExecuter.DrOutline = new ReelOutlineDrawer();
				ReelExecuter.DrOutline.point_lgt = 0.03125f;
				ReelExecuter.PFFrm0L = MTRX.getPF("reel_l0");
				ReelExecuter.PFFrm0R = MTRX.getPF("reel_l1");
				ReelExecuter.PFFrm1L = MTRX.getPF("reel_s0");
				ReelExecuter.PFFrm1R = MTRX.getPF("reel_s1");
				ReelExecuter.PtcCircle = new EfParticleOnce("reel_normal_circle", EFCON_TYPE.UI);
			}
		}

		public void initUi(UiReelManager _Ui, int _index)
		{
			this.Ui = _Ui;
			this.index = _index;
			this.af = 0;
			this.initState(ReelExecuter.ESTATE.NORMAL, 0);
		}

		public ReelExecuter position(float _sx, float _sy, float _dx = -1000f, float _dy = -1000f, bool no_reset_time = false)
		{
			if (this.af >= 0)
			{
				if (this.af < 2)
				{
					if (_dx == -1000f)
					{
						_dx = (this.dx = _sx);
						_dy = (this.dy = _sy);
					}
					this.x = _sx;
					this.y = _sy;
				}
				else
				{
					if (_dx == -1000f)
					{
						_dx = (this.dx = _sx);
						_dy = (this.dy = _sy);
					}
					_sx = this.x;
					_sy = this.y;
				}
			}
			else if (this.af < -2 || _dx == -1000f)
			{
				_dx = this.x;
				_dy = this.y;
			}
			else
			{
				this.x = _sx;
				this.y = _sy;
			}
			if (_dx == -1000f)
			{
				_dx = (this.dx = _sx);
				_dy = (this.dy = _sy);
			}
			else
			{
				this.sx = _sx;
				this.dx = _dx;
				this.sy = _sy;
				this.dy = _dy;
			}
			this.maxt_pos = ((this.stt == ReelExecuter.ESTATE.OBTAIN_APPEAR) ? 70 : 40);
			if (!no_reset_time || this.post == -1)
			{
				this.post = 0;
			}
			if (no_reset_time && this.post >= this.maxt_pos)
			{
				this.post = this.maxt_pos;
			}
			return this;
		}

		public ReelExecuter posSetA(float _dx, float _dy, bool no_reset_time = false)
		{
			return this.posSetA(_dx, _dy, false);
		}

		public ReelExecuter posSetA(float _sx, float _sy, float _dx, float _dy, bool no_reset_time = false)
		{
			if (_sx != -1000f)
			{
				this.sx = _sx;
			}
			if (_sy != -1000f)
			{
				this.sy = _sy;
			}
			if (_dx != -1000f)
			{
				this.dx = _dx;
			}
			if (_dy != -1000f)
			{
				this.dy = _dy;
			}
			this.maxt_pos = ((this.stt == ReelExecuter.ESTATE.OBTAIN_APPEAR) ? 70 : 40);
			if (!no_reset_time || this.post == -1)
			{
				this.post = 0;
			}
			if (no_reset_time && this.post >= this.maxt_pos)
			{
				this.post = this.maxt_pos;
			}
			return this;
		}

		private void calcPosition(float tzs = -1f)
		{
			if (tzs < 0f)
			{
				if (this.af >= 0)
				{
					tzs = X.ZSIN2((float)this.post, (float)this.maxt_pos);
				}
				else
				{
					tzs = 1f - X.ZSIN((float)this.post, (float)this.maxt_pos);
				}
			}
			this.x = X.NAIBUN_I(this.sx, this.dx, tzs);
			this.y = X.NAIBUN_I(this.sy, this.dy, tzs);
		}

		public void initState(ReelExecuter.ESTATE _stt, int delay = 0)
		{
			if (this.stt != _stt)
			{
				this.stt = _stt;
				this.t_state = -delay;
				this.fineMeshId();
				Effect<EffectItem> effect = this.Ui.getEffect();
				ReelExecuter.ESTATE estate = this.stt;
				if (estate != ReelExecuter.ESTATE.OBTAIN_APPEAR)
				{
					if (estate == ReelExecuter.ESTATE.DISAPPEARING)
					{
						effect.PtcST("reel_disappear", this, PTCThread.StFollow.NO_FOLLOW, null);
					}
				}
				else
				{
					effect.PtcSTsetVar("delay", (double)delay).PtcSTsetVar("maxt", 24.0).PtcST("reel_obtain_appear", this, PTCThread.StFollow.NO_FOLLOW, null);
				}
			}
			if (this.stt >= ReelExecuter.ESTATE.OPENING && this.Acontent == null)
			{
				this.initReelContent(null);
			}
		}

		public void rotateInit()
		{
			this.content_id_dec = -1;
			this.IKRow = null;
		}

		public void fineMeshId()
		{
			switch (this.stt)
			{
			case ReelExecuter.ESTATE.OBTAIN_APPEAR:
				this.mid0 = this.Ui.getMaterialId(MTRX.MtrMeshNormal, false);
				return;
			case ReelExecuter.ESTATE.NORMAL:
				this.mid0 = this.Ui.getMaterialId(MTRX.MtrMeshNormal, false);
				this.mid_aula = this.Ui.getMaterialId(MTR.ShaderGDTGradationMapAdd, this.etype);
				this.mid_frame = this.Ui.getMaterialId(MTR.ShaderGDTGradationMap, this.etype);
				this.mid_icon = this.Ui.getMaterialId(MTR.ShaderGDTWaveColor, this.etype);
				return;
			case ReelExecuter.ESTATE.DISAPPEARING:
				break;
			default:
				this.mid0 = this.Ui.getMaterialId(MTRX.MtrMeshNormal, false);
				this.mid_mask = this.Ui.getMaterialId(MTRX.getMtr(BLEND.MASK, 225), false);
				this.mid_aula = this.Ui.getMaterialId(MTR.ShaderGDTGradationMapAdd, this.etype);
				this.mid_frame = this.Ui.getMaterialId(MTR.ShaderGDTGradationMap, this.etype);
				this.mid_icon = this.Ui.getMaterialId(MTR.ShaderGDTWaveColor, this.etype);
				break;
			}
		}

		public bool decideRotate(bool randomise = false)
		{
			if (this.Acontent == null)
			{
				this.initReelContent(null);
			}
			if (this.content_id_dec >= 0)
			{
				return true;
			}
			if (this.t_state < 0 || (this.etype == ReelExecuter.ETYPE.ITEMKIND && this.t_state < 24))
			{
				return false;
			}
			if (randomise)
			{
				this.content_id_dec = X.xors(this.Acontent.Length);
			}
			else
			{
				this.content_id_dec = X.IntR(this.content_id);
			}
			this.stt = ReelExecuter.ESTATE.OPEN;
			this.t_state = 0;
			this.Ui.playSnd("slot_stop");
			this.ATx[1].text_content = "";
			this.Ui.getEffect().PtcSTsetVar("w", (double)this.reel_width_px).PtcSTsetVar("h", (double)(this.reel_height_px * 2f))
				.PtcST("reel_decided", this, PTCThread.StFollow.NO_FOLLOW, null);
			this.Ui.PadVib("reel_decide_0", 1f);
			if (this.etype == ReelExecuter.ETYPE.ITEMKIND)
			{
				ReelManager.ItemReelContainer currentItemReel = this.Con.getCurrentItemReel();
				currentItemReel.touchObtainCountAll();
				NelItemEntry nelItemEntry = currentItemReel[this.content_id_dec % this.Acontent.Length];
				this.IKRow = new NelItemEntry(nelItemEntry.Data, nelItemEntry.count, nelItemEntry.grade);
			}
			return true;
		}

		public void runDraw(int fcnt, MeshDrawer Md, Effect<EffectItem> EF)
		{
			if (this.post >= 0 && this.post < this.maxt_pos)
			{
				this.post += fcnt;
				this.calcPosition(-1f);
			}
			this.t_state += fcnt;
			Md.base_px_x = this.x;
			Md.base_px_y = this.y;
			int num = ((this.af >= 0) ? this.af : (30 + this.af));
			switch (this.stt)
			{
			case ReelExecuter.ESTATE.OBTAIN_APPEAR:
				this.drawOutline(Md, X.ZLINE((float)num, 30f), 1f - X.ZCOS((float)this.t_state, 24f));
				if (this.t_state >= 24)
				{
					this.initState(ReelExecuter.ESTATE.NORMAL, 0);
				}
				break;
			case ReelExecuter.ESTATE.NORMAL:
				this.drawNormal(Md, X.ZLINE((float)this.t_state, 25f), 0f);
				break;
			case ReelExecuter.ESTATE.OPENING:
				if (this.t_state >= 0)
				{
					this.progressRotate(fcnt);
				}
				this.drawNormal(Md, X.ZLINE((float)num, 30f), X.ZLINE((float)this.t_state, 22f));
				break;
			case ReelExecuter.ESTATE.OPEN:
				this.progressRotate(fcnt);
				this.drawNormal(Md, X.ZLINE((float)num, 30f), 1f);
				break;
			}
			if (this.af >= 0)
			{
				this.af += fcnt;
				return;
			}
			this.af -= fcnt;
		}

		private void progressRotate(int fcnt)
		{
			if (this.content_id_dec == -1)
			{
				this.fineReelContent(this.content_id + this.reel_speed * this.Ui.reel_speed * (float)fcnt);
				return;
			}
			int num = this.Acontent.Length;
			int num2 = X.IntR((float)this.content_id_dec);
			if (this.t_state >= 30)
			{
				this.fineReelContent((float)num2);
				return;
			}
			this.fineReelContent((float)(num + num2) + (float)X.MPF((float)num2 > this.content_id) * ((this.t_state < 13) ? (-0.3f + X.ZLINE((float)this.t_state, 7f) * 0.5f - X.ZLINE((float)(this.t_state - 7), 6f) * 0.35f) : (-X.COSI((float)(this.t_state - 13), 8.8f) * 0.15f * (1f - X.ZSIN((float)(this.t_state - 15), 17f)))));
		}

		private void drawOutline(MeshDrawer Md, float alpha, float xy_rot_lv)
		{
			Md.chooseSubMesh(this.mid0, false, false);
			ReelOutlineDrawer drOutline = ReelExecuter.DrOutline;
			drOutline.col_hen = (drOutline.col_point = Md.ColGrd.White().setA1(alpha * 0.75f).rgba);
			Matrix4x4 matrix4x = Matrix4x4.Rotate(Quaternion.Euler(0f, 90f * (1f - xy_rot_lv), 0f));
			drOutline.drawTo(Md, 0f, 0f, matrix4x, 25f, X.GETRAN2(this.index, 2), xy_rot_lv);
		}

		public float reel_width_px
		{
			get
			{
				return (float)((this.etype == ReelExecuter.ETYPE.ITEMKIND) ? 470 : 114);
			}
		}

		public float reel_height_px
		{
			get
			{
				return (float)((this.etype == ReelExecuter.ETYPE.ITEMKIND) ? 88 : 48);
			}
		}

		private void drawNormal(MeshDrawer Md, float alpha, float tz_reel = 0f)
		{
			float num = this.reel_height_px * 0.5f;
			int num2 = 2;
			float num3;
			if (this.etype != ReelExecuter.ETYPE.ITEMKIND)
			{
				if (tz_reel < 1f)
				{
					Md.chooseSubMesh(this.mid_aula, false, false);
					ReelExecuter.PtcCircle.index = X.GETRAN2(this.index, 2);
					ReelExecuter.PtcCircle.z = alpha * 255f;
					ReelExecuter.PtcCircle.time = ReelExecuter.getParticleColor(this.etype);
					ReelExecuter.PtcCircle.drawTo(Md, (float)this.af, ReelExecuter.PtcCircle.loop_time);
				}
				Md.chooseSubMesh(this.mid_aula, false, false);
				Md.Col = Md.ColGrd.Gray().setA1(alpha * (0.75f + 0.25f * X.COSI((float)(IN.totalframe + this.index * 7), 31f))).C;
				Md.RotaPF(0f, 0f, 2f, 2f, (float)X.ANMT(4, 3f) * 1.5707964f, MTRX.AEff[9], false, false, false, uint.MaxValue, false, 0);
				num3 = X.NI(24f, this.reel_width_px, X.ZCOS(tz_reel)) * 0.5f;
			}
			else
			{
				num3 = X.NI(0.675f, 1f, X.ZSIN(tz_reel)) * this.reel_width_px * 0.5f;
				num2 = 4;
			}
			float num4 = (this.shiftx = 3.5f * X.COSI((float)(IN.totalframe + this.index * 43), 195f));
			float num5 = (this.shifty = 3.5f * X.COSI((float)(IN.totalframe + this.index * 88), 237f));
			if (tz_reel > 0f)
			{
				num *= X.ZSIN2(tz_reel);
				this.drawMask(Md, num4, num5, num3, num);
			}
			Md.chooseSubMesh(this.mid_frame, false, false);
			Md.Col = Md.ColGrd.Gray().setA1(alpha).C;
			this.drawFrame(Md, num4, num5, num3, (float)num2, alpha);
			if (this.IconPF != null)
			{
				Md.chooseSubMesh(this.mid_icon, false, false);
				if (tz_reel < 0.5f)
				{
					this.drawIcon(Md, num4 * 0.5f, num5 * 0.5f, num3, 2f, tz_reel, alpha);
					return;
				}
				this.drawIcon(Md, num4, num5, num3, 1f, tz_reel, alpha);
			}
		}

		public void drawIcon(MeshDrawer Md, float x, float y, float wh, float scale, float tz_reel, float alpha = 1f)
		{
			int num = X.IntR((0.5f + (float)X.ANMT(2, 8f) * 0.2f) * 255f);
			Md.Col = Md.ColGrd.Set((float)num, (float)num, (float)num, 255f).C;
			if (tz_reel < 0.5f)
			{
				Md.Col = Md.ColGrd.setA1(alpha * (1f - tz_reel * 2f)).C;
				Md.RotaPF(x, y, scale, scale, 0f, this.IconPF, false, false, false, uint.MaxValue, false, 0);
			}
			if (tz_reel > 0.5f)
			{
				Md.Col = Md.ColGrd.setA1(alpha * (tz_reel * 2f - 1f)).C;
				Md.RotaPF(-wh + x - 18f, y + 1f, scale, scale, 0f, this.IconPF, false, false, false, uint.MaxValue, false, 0);
			}
		}

		public void drawFrame(MeshDrawer Md, float x, float y, float wh, float frm_scale, float alpha = 1f)
		{
			PxlFrame pxlFrame;
			PxlFrame pxlFrame2;
			if (this.etype != ReelExecuter.ETYPE.ITEMKIND)
			{
				pxlFrame = ReelExecuter.PFFrm1L;
				pxlFrame2 = ReelExecuter.PFFrm1R;
			}
			else
			{
				pxlFrame = ReelExecuter.PFFrm0L;
				pxlFrame2 = ReelExecuter.PFFrm0R;
			}
			Md.Col = Md.ColGrd.Gray().setA1(alpha).C;
			Md.RotaPF(-wh + x, y, frm_scale, frm_scale, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
			Md.RotaPF(wh + x, y, frm_scale, frm_scale, 0f, pxlFrame2, false, false, false, uint.MaxValue, false, 0);
		}

		private void drawMask(MeshDrawer Md, float x, float y, float wh, float hh)
		{
			Md.chooseSubMesh(this.mid_mask, false, false);
			Md.ColGrd.Set((this.etype == ReelExecuter.ETYPE.ITEMKIND) ? 4293321948U : 3436960458U);
			if (this.isRotating())
			{
				float num = X.ANMP((int)(X.RAN(X.GETRAN2(this.index, 2), 2591) * 40f) + IN.totalframe, 53, 3f);
				if (num < 1f)
				{
					Md.ColGrd.blend(4294620384U, num);
				}
				else if (num < 2f)
				{
					Md.ColGrd.Set(4294620384U).blend(4290309887U, num - 1f);
				}
				else
				{
					Md.ColGrd.blend(4290309887U, 3f - num);
				}
				if (this.etype == ReelExecuter.ETYPE.ITEMKIND)
				{
					Md.ColGrd.blend(4293321948U, 0.5f);
				}
			}
			Md.Col = Md.ColGrd.C;
			Md.ColGrd.blend((this.etype == ReelExecuter.ETYPE.ITEMKIND) ? 2864626864U : 2871601170U, 0.75f);
			Md.BlurLine(-wh + x, y, x + wh, y, hh * 2f, 0, 0f, false);
		}

		public static void fineMaterialCol(Material Mtr, ReelExecuter.ETYPE etype)
		{
			try
			{
				string name = Mtr.shader.name;
				if (name != null)
				{
					if (!(name == "Hachan/ShaderGDTGradationMap") && !(name == "Hachan/ShaderGDTGradationMapAdd"))
					{
						if (name == "Hachan/ShaderGDTWaveColor")
						{
							if (etype == ReelExecuter.ETYPE.GRADE1 || etype == ReelExecuter.ETYPE.GRADE2 || etype == ReelExecuter.ETYPE.GRADE3)
							{
								ReelExecuter.setColorWCB(Mtr, 13224703, 8614911, 619);
							}
							else if (etype == ReelExecuter.ETYPE.COUNT_ADD1 || etype == ReelExecuter.ETYPE.COUNT_ADD2 || etype == ReelExecuter.ETYPE.COUNT_ADD3)
							{
								ReelExecuter.setColorWCB(Mtr, 16773510, 16736321, 5713152);
							}
							else if (etype == ReelExecuter.ETYPE.COUNT_MUL1)
							{
								ReelExecuter.setColorWCB(Mtr, 13172584, 16770864, 5199616);
							}
							else if (etype == ReelExecuter.ETYPE.ADD_MONEY)
							{
								ReelExecuter.setColorWCB(Mtr, 16777215, 14608054, 0);
							}
							else if (etype == ReelExecuter.ETYPE.RANDOM)
							{
								ReelExecuter.setColorWCB(Mtr, 16763635, 7636223, 9502790);
							}
						}
					}
					else if (etype == ReelExecuter.ETYPE.GRADE1 || etype == ReelExecuter.ETYPE.GRADE2 || etype == ReelExecuter.ETYPE.GRADE3)
					{
						ReelExecuter.setColorWCB(Mtr, 10645500, 3277052, 9918);
					}
					else if (etype == ReelExecuter.ETYPE.COUNT_ADD1 || etype == ReelExecuter.ETYPE.COUNT_ADD2 || etype == ReelExecuter.ETYPE.COUNT_ADD3)
					{
						ReelExecuter.setColorWCB(Mtr, 16238674, 13602348, 8923904);
					}
					else if (etype == ReelExecuter.ETYPE.COUNT_MUL1)
					{
						ReelExecuter.setColorWCB(Mtr, 14483307, 13033472, 6707715);
					}
					else if (etype == ReelExecuter.ETYPE.ADD_MONEY)
					{
						ReelExecuter.setColorWCB(Mtr, 14211010, 10132642, 7632726);
					}
					else if (etype == ReelExecuter.ETYPE.RANDOM)
					{
						ReelExecuter.setColorWCB(Mtr, 16751095, 15415987, 7798784);
					}
				}
			}
			catch
			{
			}
		}

		private static int getParticleColor(ReelExecuter.ETYPE etype)
		{
			if (etype == ReelExecuter.ETYPE.GRADE1 || etype == ReelExecuter.ETYPE.GRADE2 || etype == ReelExecuter.ETYPE.GRADE3)
			{
				return 8614911;
			}
			if (etype == ReelExecuter.ETYPE.COUNT_ADD1 || etype == ReelExecuter.ETYPE.COUNT_ADD2 || etype == ReelExecuter.ETYPE.COUNT_ADD3)
			{
				return 16736321;
			}
			if (etype == ReelExecuter.ETYPE.COUNT_MUL1)
			{
				return 16770864;
			}
			return 30617;
		}

		public static void setColorWCB(Material Mtr, int w, int c, int b)
		{
			Mtr.SetColor("_ColorW", C32.d2c((uint)(-16777216 | w)));
			Mtr.SetColor("_Color", C32.d2c((uint)(2130706432 | c)));
			Mtr.SetColor("_ColorB", C32.d2c((uint)b));
		}

		public static void setColorWCBa(Material Mtr, uint w, uint c, uint b)
		{
			Mtr.SetColor("_ColorW", C32.d2c(w));
			Mtr.SetColor("_Color", C32.d2c(c));
			Mtr.SetColor("_ColorB", C32.d2c(b));
		}

		public static int getShaderId(string shader_key)
		{
			if (shader_key != null)
			{
				if (shader_key == "Hachan/ShaderMeshAdd")
				{
					return 5;
				}
				if (shader_key == "Hachan/ShaderGDTAdd")
				{
					return 6;
				}
				if (shader_key == "Hachan/ShaderGDTWaveColor")
				{
					return 4;
				}
				if (shader_key == "Hachan/ShaderGDTGradationMap")
				{
					return 3;
				}
				if (shader_key == "Hachan/ShaderMeshMask")
				{
					return 2;
				}
				if (shader_key == "Hachan/ShaderGDTGradationMapAdd")
				{
					return 1;
				}
			}
			return 0;
		}

		public void initReelContent(ReelManager.ItemReelContainer _AMainItm = null)
		{
			if (this.etype == ReelExecuter.ETYPE.ITEMKIND)
			{
				if (_AMainItm == null)
				{
					return;
				}
				int count = _AMainItm.Count;
				this.Acontent = new string[count];
				for (int i = 0; i < count; i++)
				{
					NelItemEntry nelItemEntry = _AMainItm[i];
					this.Acontent[i] = this.row2content_text(nelItemEntry);
				}
				this.af = 0;
				this.t_state = 0;
			}
			else
			{
				this.Acontent = ReelManager.OAreel_content[(int)this.etype];
			}
			this.content_id = (float)X.xors(this.Acontent.Length);
			this.rotateInit();
			if (this.ATx == null)
			{
				this.ATx = new TextRenderer[3];
				for (int j = 0; j < 3; j++)
				{
					TextRenderer textRenderer = (this.ATx[j] = IN.CreateGob(this.Ui.gameObject, "-Tx" + this.index.ToString() + "-" + j.ToString()).AddComponent<TextRenderer>());
					textRenderer.max_swidth_px = this.reel_width_px - 20f;
					textRenderer.auto_condense = true;
					textRenderer.html_mode = true;
					textRenderer.size = 22f;
					textRenderer.alignx = ALIGN.CENTER;
					textRenderer.aligny = ALIGNY.MIDDLE;
					textRenderer.StencilRef(225);
					textRenderer.color_apply_to_image = true;
					textRenderer.use_valotile = true;
					if (this.etype == ReelExecuter.ETYPE.ITEMKIND)
					{
						textRenderer.TextColor = MTRX.ColBlack;
						textRenderer.BorderColor = MTRX.ColWhite;
					}
					else
					{
						textRenderer.TextColor = MTRX.ColWhite;
					}
					IN.setZ(textRenderer.transform, -0.3f);
				}
			}
			this.fineReelContent(this.content_id);
			this.activate();
		}

		public void fineReelContent(float lv)
		{
			int num = this.Acontent.Length;
			int num2 = X.IntR(lv) % num;
			int num3 = X.IntR(this.content_id) % num;
			float num4 = X.frac(lv);
			if (num3 != num2 || this.ATx[1].textIs(""))
			{
				num3 = num2;
				using (STB stb = TX.PopBld(null, 0))
				{
					this.ATx[0].Txt(this.content2text(stb, (num3 + num - 1) % num));
					stb.Clear();
					this.ATx[1].Txt(this.content2text(stb, num3));
					stb.Clear();
					this.ATx[2].Txt(this.content2text(stb, (num3 + 1) % num));
				}
				for (int i = 0; i < 3; i++)
				{
					this.ATx[i].MustRedraw();
				}
			}
			this.content_id = lv;
			while (this.content_id >= (float)num)
			{
				this.content_id -= (float)num;
			}
			if (num4 < 0.5f)
			{
				float num5 = X.ZLINE(num4 * 2f);
				float num6 = X.ZSIN(num4 * 2f);
				this.fineTxPos(0, 0.75f + 0.25f * num6, 0.75f - 0.5f * num6);
				this.fineTxPos(1, 0.45f * num5, 1f - 0.125f * num5);
				this.fineTxPos(2, -0.75f + 0.25f * num5, 0.75f + 0.125f * num5);
				return;
			}
			float num7 = X.ZLINE(num4 * 2f - 1f);
			float num8 = X.ZSIN(num4 * 2f - 1f);
			this.fineTxPos(0, 0.45f + 0.3f * num7, 0.875f - 0.125f * num7);
			this.fineTxPos(1, -0.5f * (1f - num7), 0.875f + 0.125f * num7);
			this.fineTxPos(2, -1f + 0.25f * num8, 0.25f + 0.5f * num8);
		}

		private void fineTxPos(int _txid, float yp, float scale)
		{
			TextRenderer textRenderer = this.ATx[_txid];
			Transform transform = textRenderer.transform;
			transform.localScale = new Vector3(1f, scale, 1f);
			IN.PosP2(transform, this.shiftx + this.x, this.shifty + this.y + yp * 38f);
			textRenderer.alpha = 1f - X.ZSINN(0.875f - scale, 3f);
		}

		private STB content2text(STB Stb, int i)
		{
			return this.content2text(Stb, this.Acontent[i], -1);
		}

		private STB content2text(STB Stb, string t, int id = -1)
		{
			if (this.etype == ReelExecuter.ETYPE.ITEMKIND)
			{
				Stb.Set(t);
			}
			else
			{
				Stb.Clear();
				if (id != -1)
				{
					Stb.Add("", id, ":");
				}
				ReelExecuter.effect2text(Stb, t);
			}
			return Stb;
		}

		public static STB effect2text(STB Stb, string t)
		{
			ReelExecuter.EFFECT effect;
			if (!FEnum<ReelExecuter.EFFECT>.TryParse(t, out effect, true))
			{
				return Stb;
			}
			Stb.Add("<img mesh=\"nel_reel_effect.", (int)effect, "\" width=\"30\" scale=\"2\" />");
			return Stb;
		}

		private string row2content_text(NelItemEntry _Entry)
		{
			string text;
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add(_Entry.Data.getLocalizedName((int)_Entry.grade, null));
				stb.Add("<img mesh=\"nel_item_grade.", (int)_Entry.grade, "\" width=\"68\" tx_color /> <font size=\"24\">x").Add("", _Entry.count.ToString() + "</font>");
				text = stb.ToString();
			}
			return text;
		}

		public ReelExecuter.EFFECT applyEffectToIK(ReelExecuter Reel)
		{
			if (this.IKRow == null)
			{
				return ReelExecuter.EFFECT.GRADE0;
			}
			ReelExecuter.EFFECT effect;
			if (!FEnum<ReelExecuter.EFFECT>.TryParse(Reel.Acontent[Reel.content_id_dec % Reel.Acontent.Length], out effect, true))
			{
				return ReelExecuter.EFFECT.GRADE0;
			}
			int num = (int)effect;
			bool flag = true;
			switch (effect)
			{
			case ReelExecuter.EFFECT.GRADE1:
			case ReelExecuter.EFFECT.GRADE2:
			case ReelExecuter.EFFECT.GRADE3:
			case ReelExecuter.EFFECT.GRADE4:
				this.IKRow.grade = (byte)X.Mn(4, (int)this.IKRow.grade + num);
				this.Ui.playSnd("reel_decide_grade");
				this.Ui.getEffect().PtcSTsetVar("cx", (double)(0.015625f * (float)((int)(this.x + (this.ATx[1].get_swidth_px() * 0.5f - 85f))))).PtcSTsetVar("cy", (double)(this.y * 0.015625f))
					.PtcST("reel_decide_ik_grade", null, PTCThread.StFollow.NO_FOLLOW, null);
				goto IL_02C4;
			case ReelExecuter.EFFECT.COUNT_ADD1:
			case ReelExecuter.EFFECT.COUNT_ADD2:
			case ReelExecuter.EFFECT.COUNT_ADD3:
			case ReelExecuter.EFFECT.COUNT_ADD4:
			case ReelExecuter.EFFECT.COUNT_ADD5:
				this.IKRow.count = this.IKRow.count + (num - 5);
				this.Ui.playSnd("reel_decide_count");
				this.Ui.getEffect().PtcSTsetVar("cx", (double)(0.015625f * (float)((int)(this.x + (this.ATx[1].get_swidth_px() * 0.5f - 12f))))).PtcSTsetVar("cy", (double)(this.y * 0.015625f))
					.PtcST("reel_decide_ik_count_add", null, PTCThread.StFollow.NO_FOLLOW, null);
				goto IL_02C4;
			case ReelExecuter.EFFECT.COUNT_MUL2:
				this.IKRow.count *= 2;
				this.Ui.playSnd("reel_decide_mul");
				this.Ui.getEffect().PtcSTsetVar("cx", (double)(0.015625f * (float)((int)(this.x + (this.ATx[1].get_swidth_px() * 0.5f - 12f))))).PtcSTsetVar("cy", (double)(this.y * 0.015625f))
					.PtcST("reel_decide_ik_count_mul", null, PTCThread.StFollow.NO_FOLLOW, null);
				goto IL_02C4;
			case ReelExecuter.EFFECT.ADD_MONEY10:
			case ReelExecuter.EFFECT.ADD_MONEY20:
			case ReelExecuter.EFFECT.ADD_MONEY30:
			case ReelExecuter.EFFECT.ADD_MONEY100:
			{
				UiReelManager ui = this.Ui;
				int added_money = ui.added_money;
				ReelExecuter.EFFECT effect2 = (ReelExecuter.EFFECT)num;
				ui.added_money = added_money + X.NmI(TX.slice(effect2.ToString(), "ADD_MONEY".Length), 0, false, false);
				this.Ui.playSnd("store_checkout");
				this.Ui.getEffect().PtcSTsetVar("cx", (double)(0.015625f * Reel.x)).PtcSTsetVar("cy", (double)(Reel.y * 0.015625f))
					.PtcST("reel_decide_ik_add_money", null, PTCThread.StFollow.NO_FOLLOW, null);
				goto IL_02C4;
			}
			}
			flag = false;
			IL_02C4:
			this.IKRow.count = X.Mn(this.IKRow.count, 99);
			if (flag)
			{
				this.ATx[1].text_content = (this.Acontent[this.content_id_dec % this.Acontent.Length] = this.row2content_text(this.IKRow));
			}
			return (ReelExecuter.EFFECT)num;
		}

		public bool isReelUseState()
		{
			return this.stt == ReelExecuter.ESTATE.OPENING || this.stt == ReelExecuter.ESTATE.OPEN;
		}

		public bool isReelObtainState()
		{
			return this.stt == ReelExecuter.ESTATE.OBTAIN_APPEAR;
		}

		public bool isDisappearingState()
		{
			return this.stt == ReelExecuter.ESTATE.DISAPPEARING;
		}

		public bool isRotating()
		{
			return this.content_id_dec == -1;
		}

		public void activate()
		{
			if (this.af < 0)
			{
				this.af = 0;
				this.post = 0;
			}
		}

		public void deactivate()
		{
			if (this.af >= 0)
			{
				this.af = -1;
				this.post = 0;
				this.sx = this.dx;
				this.sy = this.dy + (float)(900 * X.MPF(this.index < 0));
				this.maxt_pos = 35;
			}
		}

		public void destruct()
		{
			this.stt = ReelExecuter.ESTATE.NONE;
			this.af = 0;
			this.shiftx = (this.shifty = 0f);
			this.Acontent = null;
			this.Ui = null;
			this.rotateInit();
			if (this.ATx != null)
			{
				for (int i = 0; i < 3; i++)
				{
					IN.DestroyOne(this.ATx[i]);
				}
				this.ATx = null;
			}
		}

		public bool getEffectReposition(PTCThread St, PTCThread.StFollow follow, float fcnt, out Vector3 V)
		{
			V = new Vector3(this.x * 0.015625f, this.y * 0.015625f, 1.5707964f);
			return true;
		}

		public bool readPtcScript(PTCThread rER)
		{
			return false;
		}

		public bool isSoundActive(SndPlayer S)
		{
			M2SoundPlayerItem m2SoundPlayerItem = S as M2SoundPlayerItem;
			return m2SoundPlayerItem != null && TX.isStart(m2SoundPlayerItem.key, this.getSoundKey(), 0);
		}

		public override string ToString()
		{
			return "<ReelExecuter> " + this.index.ToString() + ":" + this.etype.ToString();
		}

		public string getSoundKey()
		{
			return "ReelExecuter";
		}

		public bool initSetEffect(PTCThread Thread, EffectItem Ef)
		{
			return true;
		}

		public ReelExecuter.ETYPE getEType()
		{
			return this.etype;
		}

		private ReelExecuter.ETYPE etype;

		private ReelExecuter.ESTATE stt;

		public int index;

		protected float x;

		protected float y;

		protected float sx;

		protected float sy;

		protected float dx;

		protected float dy;

		private float shiftx;

		private float shifty;

		protected int post = -1;

		protected int maxt_pos = 12;

		protected int af;

		protected int t_state;

		private readonly ReelManager Con;

		private UiReelManager Ui;

		private PxlFrame IconPF;

		private static PxlFrame PFFrm0L;

		private static PxlFrame PFFrm0R;

		private static PxlFrame PFFrm1L;

		private static PxlFrame PFFrm1R;

		private static ReelOutlineDrawer DrOutline;

		private const int obtain_appear_delay = 50;

		public const float reel_close_small_px = 24f;

		private static EfParticleOnce PtcCircle;

		private string[] Acontent;

		private float content_id;

		private float reel_speed = 0.11111111f;

		public int content_id_dec = -1;

		public NelItemEntry IKRow;

		private TextRenderer[] ATx;

		private const int appear_changet = 24;

		private int mid0;

		private int mid_mask;

		private int mid_aula;

		private int mid_frame;

		private int mid_icon;

		public enum ETYPE
		{
			ITEMKIND,
			GRADE1,
			COUNT_ADD1,
			COUNT_MUL1,
			ADD_MONEY,
			RANDOM,
			RARE_ADD1,
			GRADE2,
			COUNT_ADD2,
			GRADE3,
			COUNT_ADD3,
			_MAX
		}

		public enum EFFECT
		{
			GRADE0,
			GRADE1,
			GRADE2,
			GRADE3,
			GRADE4,
			COUNT_ADD0,
			COUNT_ADD1,
			COUNT_ADD2,
			COUNT_ADD3,
			COUNT_ADD4,
			COUNT_ADD5,
			COUNT_MUL1,
			COUNT_MUL2,
			ADD_MONEY10,
			ADD_MONEY20,
			ADD_MONEY30,
			ADD_MONEY100
		}

		public enum ESTATE
		{
			NONE,
			OBTAIN_APPEAR,
			NORMAL,
			DISAPPEARING,
			OPENING,
			OPEN
		}
	}
}
