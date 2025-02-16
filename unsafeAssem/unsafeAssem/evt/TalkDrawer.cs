using System;
using Better;
using UnityEngine;
using XX;

namespace evt
{
	public class TalkDrawer : EvDrawer
	{
		public bool mp_flip
		{
			get
			{
				return this.PosAttached.mp_flip;
			}
		}

		public TalkDrawer(string _key)
			: base(null, _key, EvDrawerContainer.LAYER.TALKER)
		{
		}

		public override void clearValues(bool clear_position)
		{
			base.clearValues(false);
			base.movetype = "ZSIN3";
			this.base_sx = 0f;
			this.base_sy = 0f;
			this.base_dx = 0f;
			this.base_dy = 0f;
			this.mt_max = EvDrawer.MT_NORMAL;
			this.manual_translated = false;
			this.x = (this.sx = this.PosAttached.sx);
			this.y = (this.sy = this.PosAttached.sy);
			this.dx = this.PosAttached.depx;
			this.dy = this.PosAttached.depy;
			this.t = 0f;
			this.draw_flag |= 4U;
		}

		public void animateDepertPosTo(TalkDrawer.TkDep TkPos)
		{
			this.PosAttached = TkPos;
			if (this.dx_real != TkPos.depx || this.dy_real != TkPos.depy)
			{
				this.sx = this.dx_real;
				this.sy = this.dy_real;
				this.dx = TkPos.depx;
				this.dy = TkPos.depy;
				this.base_dx = 0f;
				this.base_dy = 0f;
				base.movetype = "ZSIN3";
				base.initMove(30);
			}
			if (this.t < 0f)
			{
				this.sx = this.PosAttached.sx;
				this.sy = this.PosAttached.sy;
			}
			this.manual_translated = false;
		}

		public EvPerson activatePerson(EvDrawerContainer _DC, TalkDrawer.TkDep TkPos, string _person = "_", string _show_type = "")
		{
			this.img_center_shift_x = (this.img_center_shift_y = 0f);
			if (_person != "")
			{
				if (this.Person == null || _person != this.Person.key)
				{
					this.release();
					this.Person = EvPerson.getPerson(_person, null);
					if (this.Person == null)
					{
						return null;
					}
					this.DC = _DC;
					this.animateDepertPosTo(TkPos);
					this.MMRD = this.DC.get_MMRD();
					this.Person.activate(_show_type);
					this.person_msg = this.Person.key;
				}
				else
				{
					this.animateDepertPosTo(TkPos);
				}
			}
			else
			{
				this.release();
			}
			return this.Person;
		}

		public override bool setGrp(string grp, string view_key = "")
		{
			base.releaseFader();
			if (this.Person != null)
			{
				uint num = base.stringToViewType(view_key);
				if (!this.Person.setGrp(this, grp, out this.NextPose, out this.next_emotion))
				{
					X.de("Person の中身と異なるキャラクタ画像が指定されました: " + grp, null);
				}
				else
				{
					if ((num & 8192U) == 0U)
					{
						base.deactivateManpu(true, false);
					}
					this.activate(num | 256U | 2048U, true);
					EvEmotVisibility evEmotVisibility = this.get_NextPose(false);
					if (evEmotVisibility == null)
					{
						return false;
					}
					float num2 = evEmotVisibility.get_draw_y(1f);
					float num3 = evEmotVisibility.shift_x;
					if (evEmotVisibility.shift_x_onright != 0f)
					{
						num3 += evEmotVisibility.shift_x_onright * X.ZSIN(X.Abs(this.dx), IN.wh) * (float)X.MPF(this.dx > 0f);
					}
					if (this.img_center_shift_y != num2 || this.img_center_shift_x != num3)
					{
						this.img_center_shift_y = num2;
						this.img_center_shift_x = num3;
						if (this.MdFill != null)
						{
							base.fineFillMeshPos(null);
						}
					}
					return true;
				}
			}
			return false;
		}

		public override Material getImageMaterial(uint gtype)
		{
			int num = ((this.Fader != null) ? base.stencil_ref : (-1));
			Material material;
			if (this.Fader != null)
			{
				material = (this.MtrImageST = ((this.MtrImageST == null) ? MTRX.newMtr(MTRX.ShaderGDTST) : this.MtrImageST));
				material.SetFloat("_StencilRef", (float)num);
				material.SetFloat("_StencilComp", 3f);
			}
			else
			{
				material = (this.MtrImage = ((this.MtrImage == null) ? MTRX.newMtr(MTRX.ShaderGDT) : this.MtrImage));
			}
			return material;
		}

		public override bool runDraw(float fcnt, bool deleting = false)
		{
			if (!base.runDraw(fcnt, deleting || this.Person == null))
			{
				this.DC.releasePooledTalkDrawer(this);
				return false;
			}
			if (this.Person != null)
			{
				this.Person.run();
			}
			return true;
		}

		public override EvDrawer moveTo(string sxstr, string systr, string dxstr, string dystr, int maxt, string _movetype = "")
		{
			base.moveTo(sxstr, systr, dxstr, dystr, maxt, _movetype);
			if (!this.pos_locked)
			{
				this.manual_translated = true;
			}
			return this;
		}

		public override EvDrawer deactivate()
		{
			base.deactivate();
			if (!this.manual_translated)
			{
				this.base_sx = 0f;
				this.base_sy = 0f;
				this.sx = this.PosAttached.sx;
				this.sy = this.PosAttached.sy;
				this.mt_max = TalkDrawer.MT_HIDE;
				base.movetype = "ZSIN2";
			}
			return this;
		}

		public override EvDrawer release()
		{
			if (this.MtrImage != null)
			{
				this.MtrImage.mainTexture = null;
				IN.DestroyOne(this.MtrImage);
				this.MtrImage = null;
			}
			if (this.MtrImageST != null)
			{
				this.MtrImageST.mainTexture = null;
				IN.DestroyOne(this.MtrImageST);
				this.MtrImageST = null;
			}
			if (this.Person != null)
			{
				this.Person = this.Person.release();
			}
			this.CurPose = (this.NextPose = null);
			this.img_center_shift_y = 0f;
			return base.release();
		}

		protected override void releaseMeshDrawer()
		{
			this.CurPose = (this.NextPose = null);
			base.releaseMeshDrawer();
		}

		public override void redrawMesh(float fcnt)
		{
			if (this.Person == null || this.MdImg == null)
			{
				return;
			}
			float drawAlpha = base.getDrawAlpha();
			int num = -1;
			int num2 = 0;
			if (this.MaImg == null)
			{
				this.MaImg = new MdArranger(this.MdImg).Set(true);
			}
			else
			{
				this.MaImg.revertVerAndTriIndex(ref num, ref num2);
			}
			this.MdImg.Col = this.MdImg.ColGrd.White().setA1(drawAlpha).C;
			base.fadeDrawPrepare(fcnt);
			this.Person.drawTo(this, this.MdImg, ref this.CurPose, ref this.cur_emotion, this.NextPose, this.next_emotion);
			if (num >= 0)
			{
				this.MaImg.revertVerAndTriIndexAfter(num, num2, false);
			}
			else
			{
				this.MaImg.Set(false);
			}
			base.redrawManpu();
		}

		public EvEmotVisibility get_CurPose()
		{
			if (this.Person == null)
			{
				return null;
			}
			return this.CurPose;
		}

		public EvEmotVisibility get_NextPose(bool avoid_empty = false)
		{
			if (this.NextPose == null && avoid_empty)
			{
				return this.CurPose;
			}
			return this.NextPose;
		}

		public string getPersonKey()
		{
			if (this.Person == null)
			{
				return "";
			}
			return this.Person.key;
		}

		public string getPersonMsgKey()
		{
			if (this.Person == null)
			{
				return "";
			}
			return this.person_msg;
		}

		public string getPersonShowType()
		{
			if (this.Person == null)
			{
				return "";
			}
			return this.Person.show_type;
		}

		public static void loadTalkerPos(string lt_text)
		{
			TalkDrawer.OTk = new BDic<string, TalkDrawer.TkDep>();
			CsvReader csvReader = new CsvReader(lt_text, CsvReader.RegSpace, true);
			while (csvReader.read())
			{
				if (csvReader.cmd != "")
				{
					TalkDrawer.TkDep tkDep = new TalkDrawer.TkDep(csvReader);
					TalkDrawer.OTk[csvReader.cmd] = tkDep;
				}
			}
		}

		public override Vector3 getHkdsDepertPos()
		{
			Vector3 vector = base.getHkdsDepertPos();
			vector.x += this.img_center_shift_x;
			vector.y += this.img_center_shift_y;
			if (this.Person != null)
			{
				EvEmotVisibility evEmotVisibility = this.get_NextPose(true);
				if (evEmotVisibility != null)
				{
					vector += evEmotVisibility.getMouthPos();
				}
			}
			return vector;
		}

		public static BDic<string, TalkDrawer.TkDep> getDefinedPositionList()
		{
			return TalkDrawer.OTk;
		}

		public static bool getDefinedPosition(string key, out Vector4 Pos)
		{
			TalkDrawer.TkDep tkDep;
			if (TalkDrawer.OTk.TryGetValue(key, out tkDep))
			{
				Pos = tkDep.V4;
				return true;
			}
			Pos = default(Vector4);
			return false;
		}

		public static bool getDefinedPositionRaw(string key, out TalkDrawer.TkDep Pos)
		{
			if (TalkDrawer.OTk.TryGetValue(key, out Pos))
			{
				return true;
			}
			Pos = default(TalkDrawer.TkDep);
			return false;
		}

		public override bool getFacePosition(out Vector3 P, string pos_id, float shift_y = 0f)
		{
			if (pos_id == "_")
			{
				if (this.Person == null || this.CurPose == null)
				{
					P = new Vector3(this.x, this.y + 60f, 1f);
				}
				else
				{
					EvEmotVisibility curPose = this.get_CurPose();
					P = curPose.getFacePos(shift_y);
					P.x += this.x + this.img_center_shift_x;
					P.y += this.y + this.img_center_shift_y;
					P.z = curPose.draw_scale;
				}
				return true;
			}
			return base.getFacePosition(out P, pos_id, shift_y);
		}

		public override bool isOnRightFor(ManpuDrawer Mp)
		{
			return this.dx > 0f;
		}

		public static IHkdsFollowable getVpTalkerFirstPositionFollowable(string key)
		{
			TalkDrawer.TkDep tkDep;
			if (TalkDrawer.OTk.TryGetValue(key, out tkDep))
			{
				return new TalkDrawer.IHkdsFollowableFirstPos(tkDep);
			}
			X.de("不明な vp_talker ポジション: " + key, null);
			return null;
		}

		public override void addTemporaryTerm(string check_expression, string repl, string term)
		{
			if (this.Person != null)
			{
				this.Person.addTemporaryTerm(check_expression, repl, term);
			}
		}

		public override bool checkTempTermReplace(ref string s)
		{
			return this.Person != null && EvPerson.EmotReplaceTerm.checkTermReplace(this.Person.ATermTemp, ref s);
		}

		public EvPerson get_Person()
		{
			return this.Person;
		}

		public override int id_in_layer
		{
			get
			{
				return this.talker_appeard_index_;
			}
			set
			{
				this.talker_appeard_index_ = value;
			}
		}

		public EvPerson Person;

		public string person_msg = "";

		public TalkDrawer.TkDep PosAttached;

		public static readonly int MT_HIDE = 34;

		private int talker_appeard_index_;

		public bool manual_translated;

		private Material MtrImage;

		private Material MtrImageST;

		private EvEmotVisibility CurPose;

		private string cur_emotion;

		private EvEmotVisibility NextPose;

		private string next_emotion;

		private static BDic<string, TalkDrawer.TkDep> OTk;

		public class IHkdsFollowableFirstPos : IHkdsFollowable
		{
			public IHkdsFollowableFirstPos(TalkDrawer.TkDep _Tk)
			{
				this.Tk = _Tk;
			}

			public Vector3 getHkdsDepertPos()
			{
				return new Vector3(this.Tk.depx, this.Tk.depy, 1f);
			}

			public bool checkPositionMoved(IMessageContainer Msg)
			{
				return false;
			}

			private TalkDrawer.TkDep Tk;
		}

		public struct TkDep
		{
			public TkDep(CsvReader CR)
			{
				this.mp_flip = CR._B5;
				float num;
				float num2;
				float num3;
				float num4;
				float num5;
				float num6;
				float num7;
				float num8;
				EvDrawer.initPositionS(CR._1, CR._2, CR._3, CR._4, out num, out num2, out num3, out num4, out num5, out num6, out num7, out num8, 0f, null);
				this.depx = EvDrawer.realX(num5, num);
				this.depy = EvDrawer.realY(num6, num2);
				this.sx = EvDrawer.realX(num7, num3);
				this.sy = EvDrawer.realY(num8, num4);
			}

			public Vector4 V4
			{
				get
				{
					return new Vector4(this.depx, this.depy, this.sx, this.sy);
				}
			}

			public float depx;

			public float depy;

			public float sx;

			public float sy;

			public bool mp_flip;
		}
	}
}
