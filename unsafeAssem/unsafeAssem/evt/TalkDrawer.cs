using System;
using Better;
using UnityEngine;
using XX;

namespace evt
{
	public class TalkDrawer : EvDrawer
	{
		public TalkDrawer(string _key)
			: base(null, _key, EvDrawerContainer.LAYER.TALKER)
		{
			this.DepertFoc = this;
		}

		public override void clearValues(bool clear_position)
		{
			base.clearValues(false);
			base.movetype = "ZSIN3";
			this.DepertFoc = this;
			this.base_sx = 0f;
			this.base_sy = 0f;
			this.base_dx = 0f;
			this.base_dy = 0f;
			this.mt_max = EvDrawer.MT_NORMAL;
			this.manual_translated = false;
			this.x = (this.sx = this.sx0);
			this.y = (this.sy = this.sy0);
			this.dx = this.dx0;
			this.dy = this.dy0;
			this.t = 0f;
			this.draw_flag |= 4U;
		}

		private void initPositionFirst(string dxk, string dyk, string sxk = "", string syk = "")
		{
			base.initPosition(dxk, dyk, sxk, syk, 0f);
			this.dx0 = this.dx_real;
			this.dy0 = this.dy_real;
			this.sx0 = this.sx_real;
			this.sy0 = this.sy_real;
			this.base_sx = 0f;
			this.base_sy = 0f;
			this.base_dx = 0f;
			this.base_dy = 0f;
			this.manual_translated = false;
		}

		public void animateDepertPosTo(TalkDrawer Tk)
		{
			this.DepertFoc = Tk;
			base.moveTo("C+" + Tk.dx0.ToString(), "C+" + Tk.dy0.ToString(), 30, "");
			this.manual_translated = false;
		}

		public EvPerson activatePerson(EvDrawerContainer _DC, string _person = "_", string _show_type = "")
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
					this.MMRD = this.DC.get_MMRD();
					this.Person.activate(_show_type);
					this.person_msg = this.Person.key;
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
				if (!this.Person.setGrp(this, grp))
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
					EvEmotVisibility evEmotVisibility = this.Person.get_NextPose(false);
					if (evEmotVisibility == null)
					{
						return false;
					}
					float num2 = evEmotVisibility.get_draw_y(1f);
					float shift_x = evEmotVisibility.shift_x;
					if (this.img_center_shift_y != num2 || this.img_center_shift_x != shift_x)
					{
						this.img_center_shift_y = num2;
						this.img_center_shift_x = shift_x;
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
			if (!base.runDraw(fcnt, deleting))
			{
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
				this.sx = this.DepertFoc.sx0;
				this.sy = this.DepertFoc.sy0;
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
			this.img_center_shift_y = 0f;
			return base.release();
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
			this.Person.drawTo(this, this.MdImg);
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
			TalkDrawer.OTk = new BDic<string, TalkDrawer>();
			CsvReader csvReader = new CsvReader(lt_text, CsvReader.RegSpace, true);
			while (csvReader.read())
			{
				if (csvReader.cmd != "")
				{
					TalkDrawer tk = TalkDrawer.getTk(csvReader.cmd, false);
					tk.initPositionFirst(csvReader._1, csvReader._2, csvReader._3, csvReader._4);
					tk.mp_flip = csvReader._B5;
				}
			}
		}

		public static TalkDrawer getTk(string key, bool no_make = true)
		{
			TalkDrawer talkDrawer = X.Get<string, TalkDrawer>(TalkDrawer.OTk, key);
			if (talkDrawer != null)
			{
				return talkDrawer;
			}
			if (no_make)
			{
				return null;
			}
			TalkDrawer talkDrawer2 = new TalkDrawer(key);
			return TalkDrawer.OTk[key] = talkDrawer2;
		}

		public override Vector3 getHkdsDepertPos()
		{
			Vector3 vector = base.getHkdsDepertPos();
			vector.x += this.img_center_shift_x;
			vector.y += this.img_center_shift_y;
			if (this.Person != null)
			{
				EvEmotVisibility evEmotVisibility = this.Person.get_NextPose(true);
				if (evEmotVisibility != null)
				{
					vector += evEmotVisibility.getMouthPos();
				}
			}
			return vector;
		}

		public static BDic<string, TalkDrawer> getDefinedPositionList()
		{
			return TalkDrawer.OTk;
		}

		public static bool getDefinedPosition(string key, out Vector4 Pos)
		{
			TalkDrawer talkDrawer = X.Get<string, TalkDrawer>(TalkDrawer.OTk, key);
			if (talkDrawer != null)
			{
				Pos = talkDrawer.getDefinedFirstPosition();
				return true;
			}
			Pos = default(Vector4);
			return false;
		}

		public override bool getFacePosition(out Vector3 P, string pos_id, float shift_y = 0f)
		{
			if (pos_id == "_")
			{
				if (this.Person == null)
				{
					P = new Vector3(this.x, this.y + 60f, 1f);
				}
				else
				{
					EvEmotVisibility curPose = this.Person.get_CurPose();
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
			TalkDrawer talkDrawer = X.Get<string, TalkDrawer>(TalkDrawer.OTk, key);
			if (talkDrawer != null)
			{
				return new TalkDrawer.IHkdsFollowableFirstPos(talkDrawer);
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

		public Vector4 getDefinedFirstPosition()
		{
			return new Vector4(this.dx0, this.dy0, this.sx0, this.sy0);
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

		public static readonly int MT_HIDE = 34;

		public bool mp_flip;

		private int talker_appeard_index_;

		private float sx0;

		private float sy0;

		private float dx0;

		private float dy0;

		public bool manual_translated;

		private TalkDrawer DepertFoc;

		private Material MtrImage;

		private Material MtrImageST;

		private static BDic<string, TalkDrawer> OTk;

		public class IHkdsFollowableFirstPos : IHkdsFollowable
		{
			public IHkdsFollowableFirstPos(TalkDrawer _Tk)
			{
				this.Tk = _Tk;
			}

			public Vector3 getHkdsDepertPos()
			{
				return new Vector3(this.Tk.dx0, this.Tk.dy0, 1f);
			}

			public bool checkPositionMoved(IMessageContainer Msg)
			{
				return false;
			}

			private TalkDrawer Tk;
		}
	}
}
