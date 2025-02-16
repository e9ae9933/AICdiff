using System;
using PixelLiner.PixelLinerLib;
using XX;

namespace m2d
{
	public class M2LabelPoint : M2Rect, IActivatable
	{
		public virtual string comment
		{
			get
			{
				return this.comment_;
			}
			set
			{
				this.comment_ = value;
			}
		}

		public M2LabelPoint(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, (_Lay != null) ? _Lay.Mp : null)
		{
			this.Lay = _Lay;
			this.finePos();
		}

		public override DRect finePos()
		{
			base.finePos();
			this._tostring = null;
			return this;
		}

		public virtual void activate()
		{
		}

		public virtual void deactivate()
		{
		}

		public string getCommentForSave()
		{
			return TX.escape(this.comment_);
		}

		public virtual void initActionPre()
		{
		}

		public virtual void initAction(bool normal_map)
		{
		}

		public virtual void closeAction(bool when_map_close)
		{
			if (when_map_close)
			{
				this.unique_key_ = null;
				this.closeEvent();
				this.Mp.cmd_reload_flg |= 16U;
			}
		}

		public virtual void initEvent()
		{
			if (TX.noe(this.command))
			{
				return;
			}
			if (this.EvSetter == null)
			{
				this.EvSetter = new M2EventSetterLP(this);
			}
			if ((this.Mp.cmd_reload_flg & 3U) == 0U)
			{
				this.EvSetter.setEvent();
			}
		}

		public void closeEvent()
		{
			if (this.EvSetter != null)
			{
				this.EvSetter.closeEvent();
				this.EvSetter = null;
			}
		}

		public virtual void considerConfig4(int _l, int _t, int _r, int _b, M2Pt[,] AAconfig)
		{
		}

		public void reconsiderConfig(bool need_update_collider = true)
		{
			this.Mp.considerConfig4(this.mapx, this.mapy, this.mapx + this.mapw, this.mapy + this.maph);
			if (need_update_collider)
			{
				this.Mp.need_update_collider = true;
			}
		}

		public string unique_key
		{
			get
			{
				if (this.unique_key_ == null)
				{
					STB stb = TX.PopBld(null, 0);
					stb.Add(this.Lay.name, "_", this.key);
					this.unique_key_ = stb.ToString();
					TX.ReleaseBld(stb);
				}
				return this.unique_key_;
			}
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				stb += this.unique_key;
				stb += " ";
				stb += this.mapx;
				stb += ", ";
				stb += this.mapy;
				stb += ", ";
				stb += this.mapw;
				stb += ", ";
				stb += this.maph;
				this._tostring = stb.ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		public string getActivateKey()
		{
			return this.unique_key;
		}

		public bool isContainingMover(M2Mover Mv, float _extend_px = 0f)
		{
			return global::XX.X.isContaining(this.x, base.right, Mv.mleft * base.CLEN, Mv.mright * base.CLEN, _extend_px) && global::XX.X.isContaining(base.y, base.bottom, Mv.mtop * base.CLEN, Mv.mbottom * base.CLEN, _extend_px);
		}

		public bool isCoveringMover(M2Mover Mv, float _extend_px = 0f)
		{
			return global::XX.X.isCovering(this.x, base.right, Mv.mleft * base.CLEN, Mv.mright * base.CLEN, _extend_px) && global::XX.X.isCovering(base.y, base.bottom, Mv.mtop * base.CLEN, Mv.mbottom * base.CLEN, _extend_px);
		}

		public static M2LabelPoint readBytesContentLp(ByteArray Ba, M2MapLayer Lay, bool key_replace = false, int shift_drawx = 0, int shift_drawy = 0)
		{
			Map2d.LpLoader lpLoader;
			return M2LabelPoint.readBytesContentLp(Ba, Lay, out lpLoader, false, key_replace, shift_drawx, shift_drawy);
		}

		public static M2LabelPoint readBytesContentLp(ByteArray Ba, M2MapLayer Lay, out Map2d.LpLoader _Ld, bool create_string = true, bool key_replace = false, int shift_drawx = 0, int shift_drawy = 0)
		{
			bool flag = Lay == null && !create_string;
			string text = Ba.readPascalString("utf-8", flag);
			if (key_replace && Lay != null)
			{
				text = Lay.LP.fineIndividualName(text, null);
			}
			int num = (int)Ba.readShort() + shift_drawx;
			int num2 = (int)Ba.readShort() + shift_drawy;
			int num3 = (int)Ba.readShort();
			int num4 = (int)Ba.readShort();
			float num5 = Ba.readFloat();
			float num6 = Ba.readFloat();
			string text2 = Ba.readString("utf-8", flag);
			string text3 = Ba.readString("utf-8", flag);
			_Ld = new Map2d.LpLoader
			{
				key = text,
				command = text2,
				comment = text3
			};
			if (Lay != null)
			{
				M2LabelPoint m2LabelPoint = Lay.LP.fnCreateLabelPoint(text, Lay);
				m2LabelPoint.Set((float)num, (float)num2, (float)num3, (float)num4);
				m2LabelPoint.comment_ = text3;
				m2LabelPoint.command = text2;
				m2LabelPoint.focx = num5;
				m2LabelPoint.focy = num6;
				m2LabelPoint.index = Lay.LP.Length;
				Lay.LP.Add(m2LabelPoint);
				return m2LabelPoint;
			}
			return null;
		}

		public void writeSaveBytesTo(ByteArray Ba)
		{
			Ba.writeByte(84);
			Ba.writePascalString(this.key, "utf-8");
			Ba.writeShort((short)this.x);
			Ba.writeShort((short)base.y);
			Ba.writeShort((short)base.w);
			Ba.writeShort((short)base.h);
			Ba.writeFloat(this.focx);
			Ba.writeFloat(this.focy);
			Ba.writeString(this.command, "utf-8");
			Ba.writeString(this.comment_, "utf-8");
		}

		public readonly M2MapLayer Lay;

		public string command = "";

		private string comment_ = "";

		public bool chip_extracting;

		public M2EventSetterLP EvSetter;

		private const string head_LpDecline = "Decline";

		private const string head_LpWalkDecline = "WalkDecline";

		private const string head_LpFootTypeReplace = "FootTypeRepl";

		private const string head_LpFocus = "Focus";

		protected const string head_Tuto = "Tuto";

		private const string head_FakeDeclare = "FakeDeclare";

		private const string head_ChipSwitcher = "ChipSw";

		public static M2LabelPoint.FnCreateLp CreateLpDefault = delegate(string cmd, M2MapLayer Lay)
		{
			if (cmd == null)
			{
				return new M2LabelPoint("", 0, Lay);
			}
			if (TX.headerIs(cmd, "Decline", 0, '_', true))
			{
				return new M2LpCamDecline(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "WalkDecline", 0, '_', true))
			{
				return new M2LpWalkDecline(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "FootTypeRepl", 0, '_', true))
			{
				return new M2LpFootTypeReplacer(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Tuto", 0, '_', true))
			{
				return new M2LpTutoArea(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "FakeDeclare", 0, '_', true))
			{
				return new M2LpFakeReveal(cmd, 0, Lay, false);
			}
			if (TX.headerIs(cmd, "Focus", 0, '_', true))
			{
				return new M2LpCamFocus(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "ChipSw", 0, '_', true))
			{
				return new M2LpChipSwitcher(cmd, 0, Lay);
			}
			return new M2LabelPoint(cmd, 0, Lay);
		};

		private string unique_key_;

		private string _tostring;

		public delegate bool fnCheckLP(M2LabelPoint V);

		public delegate void fnEachLP(M2LabelPoint V);

		public delegate M2LabelPoint FnCreateLp(string cmd, M2MapLayer Lay);
	}
}
