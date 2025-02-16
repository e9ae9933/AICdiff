using System;
using System.Collections.Generic;
using Better;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using XX;

namespace evt
{
	public class EvTextLog
	{
		public EvTextLog(int max = 192)
		{
			this.Operson2PF = new BDic<string, EvTextLog.IcoData>(8);
			this.OAinj = new BDic<ushort, string[]>(2);
			this.OAinjBuf = new BDic<ushort, string[]>(2);
			this.ALog = new EvTextLog.LogItem[max];
			this.ABuf = new List<EvTextLog.LogItem>(64);
			this.DefaultIco = new EvTextLog.IcoData(MTRX.getPF("IconMob"), 4292072403U);
			this.prepareDsn();
			this.initPerson();
		}

		public void initPerson()
		{
			BDic<string, EvPerson> personDictionary = EvPerson.getPersonDictionary();
			if (personDictionary == null)
			{
				return;
			}
			foreach (KeyValuePair<string, EvPerson> keyValuePair in personDictionary)
			{
				EvPerson value = keyValuePair.Value;
				if (value.personal_color != 0U)
				{
					this.Operson2PF[value.key] = new EvTextLog.IcoData(value.SmallIconPF, value.personal_color);
				}
			}
		}

		public virtual void destruct()
		{
			this.Clear();
		}

		public void Clear()
		{
			this.use_ptr = (this.max_i = 0);
			this.OAinj.Clear();
			this.OAinjBuf.Clear();
			this.slice_index = -1;
		}

		public EvTextLog DefineIco(string person, PxlFrame PF, uint col = 4291085508U)
		{
			this.Operson2PF[person] = new EvTextLog.IcoData(PF, col);
			return this;
		}

		public EvTextLog AddBuffer(string person, string key)
		{
			this.ABuf.Add(new EvTextLog.LogItem(person, key));
			return this;
		}

		public EvTextLog AddBufferInjection(string[] A)
		{
			this.OAinjBuf[(ushort)(this.ABuf.Count - 1)] = A;
			return this;
		}

		public EvTextLog AddSelection(string key, bool force = false)
		{
			if (this.record_selection || force)
			{
				this.ABuf.Add(new EvTextLog.LogItem("%SELECT", key));
			}
			return this;
		}

		public virtual void evInit()
		{
			this.record_selection = true;
			this.allow_msglog = true;
		}

		public virtual void evQuit()
		{
			this.ExplodeBuffer();
		}

		public EvTextLog ExplodeBuffer()
		{
			this.popCache();
			int num = X.Mn(this.ABuf.Count, this.ALog.Length);
			int num2;
			if (this.max_i >= num)
			{
				int i = num;
				num2 = this.use_ptr;
				bool flag = true;
				while (--i >= 0)
				{
					if (--num2 < 0)
					{
						num2 = this.max_i - 1;
					}
					if (!this.ABuf[this.ABuf.Count - num + i].Equals(this.ALog[num2]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					this.ABuf.Clear();
					this.OAinjBuf.Clear();
					return this;
				}
			}
			num2 = this.ABuf.Count - num;
			for (int i = 0; i < num; i++)
			{
				this.AddLogExecute(this.ABuf[num2]);
				this.AddLogExecuteInjection(X.Get<ushort, string[]>(this.OAinjBuf, (ushort)num2));
				this.AddLogExecuteAfter();
				num2++;
			}
			this.ABuf.Clear();
			this.OAinjBuf.Clear();
			return this;
		}

		private EvTextLog AddLogExecute(EvTextLog.LogItem Src)
		{
			this.ALog[this.use_ptr] = Src;
			this.max_i = X.Mx(this.use_ptr + 1, this.max_i);
			return this;
		}

		private EvTextLog AddLogExecuteInjection(string[] A)
		{
			if (A == null)
			{
				this.OAinj.Remove((ushort)this.use_ptr);
			}
			else
			{
				this.OAinj[(ushort)this.use_ptr] = A;
			}
			return this;
		}

		private void AddLogExecuteAfter()
		{
			this.use_ptr++;
			if (this.use_ptr >= this.ALog.Length)
			{
				this.use_ptr = 0;
			}
		}

		private int pointerPos(int i)
		{
			if (this.use_ptr < this.max_i)
			{
				return (this.use_ptr + i) % this.max_i;
			}
			return X.MMX(0, this.use_ptr - this.max_i + i, this.max_i - 1);
		}

		public void createToDesigner(Designer Ds, int start = 0, int create_count = -1)
		{
			if (start < 0)
			{
				start = X.Mx(start, -this.ALog.Length);
				start = this.max_i + start;
			}
			if (create_count < 0)
			{
				create_count = this.max_i;
			}
			else
			{
				create_count = X.Mn(this.max_i - start, create_count);
			}
			Ds.Br();
			this.use_w = Ds.use_w;
			int num = this.pointerPos(start);
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				while (--create_count >= 0)
				{
					EvTextLog.LogItem logItem = this.ALog[num];
					string[] array = X.Get<ushort, string[]>(this.OAinj, (ushort)num);
					string person = logItem.person;
					if (person != null && person == "%SELECT")
					{
						this.createSelection(Ds, logItem);
						Ds.Br();
					}
					else
					{
						blist.Clear();
						this.getSentence(logItem.person, logItem.key, blist, array);
						int count = blist.Count;
						if (count != 0)
						{
							int num2 = 0;
							while (num2 < count && (this.slice_index < 0 || !(this.slice_person_key == logItem.person) || !(this.slice_label == logItem.key) || num2 < this.slice_index))
							{
								this.createMainOne(Ds, num, num2, logItem, blist[num2]);
								Ds.Br();
								num2++;
							}
						}
					}
					if (++num >= this.max_i)
					{
						num = 0;
					}
				}
			}
		}

		protected void prepareDsn()
		{
			this.DsnSelection = new DsnDataImg
			{
				alignx = ALIGN.LEFT,
				TxCol = MTRX.ColWhite,
				text_margin_y = 6f,
				text_margin_x = 24f,
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.drawSelection),
				MI = MTRX.MIicon,
				html = true,
				stencil_lessequal = false,
				text_auto_wrap = true
			};
			this.DsnMain = new DsnDataImg
			{
				alignx = ALIGN.LEFT,
				TxCol = MTRX.ColBlack,
				text_margin_y = 12f,
				text_margin_x = 32f,
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.drawSentence),
				text_auto_wrap = true,
				stencil_lessequal = false,
				do_not_error_unknown_tag = true,
				MI = MTRX.MIicon,
				html = true
			};
		}

		protected FillImageBlock createSelection(Designer Ds, EvTextLog.LogItem Log)
		{
			string text = Log.key;
			if (text.IndexOf("&&") == 0)
			{
				text = TX.Get(TX.slice(text, 2), "");
				if (TX.noe(text))
				{
					return null;
				}
			}
			Ds.XSh(50f);
			this.DsnSelection.swidth = Ds.use_ws;
			this.DsnSelection.Text(TX.GetA("TextLog_Selected", text));
			FillImageBlock fillImageBlock = Ds.addImg(this.DsnSelection);
			fillImageBlock.text_italic = (fillImageBlock.text_bold = true);
			return fillImageBlock;
		}

		protected FillImageBlock createMainOne(Designer Ds, int index, int sentence_index, EvTextLog.LogItem Log, string tx)
		{
			EvTextLog.IcoData defaultIco;
			if (!this.Operson2PF.TryGetValue(Log.person, out defaultIco))
			{
				defaultIco = this.DefaultIco;
			}
			Ds.XSh(40f);
			this.DsnMain.Col = C32.d2c(defaultIco.col);
			this.DsnMain.swidth = Ds.use_ws - 12f;
			this.DsnMain.Text(tx.Replace("\n", TX.isSpaceDelimiterLang() ? " " : "\b"));
			FillImageBlock fillImageBlock = Ds.addImg(this.DsnMain);
			fillImageBlock.name = "TextLog-" + index.ToString() + "_" + sentence_index.ToString();
			return fillImageBlock;
		}

		protected virtual bool drawSelection(MeshDrawer Md, FillImageBlock Fi, float alpha, ref bool update_meshdrawer)
		{
			Md.Col = C32.MulA(4286677115U, alpha);
			float num = Fi.get_swidth_px() - 80f;
			float num2 = -20f;
			Shape.kadomaruRectExtImg(Md, num2, 0f, num, X.Mx(Fi.get_sheight_px(), 24f), 24f, null, false);
			num2 -= num * 0.5f + 26f;
			Md.Rect(num2, 0f, 10f, 10f, false);
			Md.Rect(num2 + 16f, 0f, 16f, 16f, false);
			return true;
		}

		protected virtual bool drawSentence(MeshDrawer Md, FillImageBlock Fi, float alpha, ref bool update_meshdrawer)
		{
			string name = Fi.name;
			if (!TX.isStart(name, "TextLog-", 0))
			{
				return false;
			}
			STB stb = TX.PopBld(name, 0);
			int num2;
			int num = stb.NmI("TextLog-".Length, out num2, -1, -1);
			int num4;
			int num3 = stb.NmI(num2 + 1, out num4, -1, -1);
			TX.ReleaseBld(stb);
			if (num < 0)
			{
				return false;
			}
			string person = this.ALog[num].person;
			EvTextLog.IcoData defaultIco;
			if (!this.Operson2PF.TryGetValue(person, out defaultIco))
			{
				defaultIco = this.DefaultIco;
			}
			float num5 = Fi.get_swidth_px() - 42f;
			float num6 = 0f;
			float num7 = 16f;
			Shape.kadomaruRectExtImg(Md, num6, 0f, num5, X.Mx(Fi.get_sheight_px(), 30f), 24f, null, false);
			num6 += 4f;
			Shape.TriangleImg(Md, num6 - num5 * 0.5f - num7, 0f, num6 - num5 * 0.5f, num7 * 0.5f, num6 - num5 * 0.5f, -num7 * 0.5f, null, AIM.L, false);
			if (num3 == 0)
			{
				float num8 = -num5 * 0.5f - 28f;
				Md.initForImg(MTRX.EffCircle128, 0).Rect(num8, 0f, 32f, 32f, false);
				if (defaultIco.PF != null)
				{
					Md.Col = C32.MulA(uint.MaxValue, alpha);
					Md.RotaPF(num8, 0f, 1f, 1f, 0f, defaultIco.PF, true, false, false, uint.MaxValue, false, 0);
				}
			}
			return true;
		}

		protected virtual bool getSentence(string person, string key, List<string> Adest, string[] Ainj = null)
		{
			Adest.Clear();
			TX tx = TX.getTX(key, true, true, null);
			if (tx != null)
			{
				Adest.Add(tx.text);
				return true;
			}
			return false;
		}

		public int LEN
		{
			get
			{
				return this.max_i;
			}
		}

		public virtual void runMesh(float fcnt)
		{
		}

		public virtual bool runInEvent(IMessageContainer Msg, EvMsgCommand Mtl)
		{
			return false;
		}

		public void puchCache()
		{
			if (this.BaCache != null)
			{
				this.popCache();
			}
			this.BaCache = null;
			ByteArray byteArray = new ByteArray(0U);
			this.writeBinaryTo(byteArray, true);
			byteArray.position = 0UL;
			this.ExplodeBuffer();
			this.BaCache = byteArray;
		}

		public void popCache()
		{
			if (this.BaCache != null)
			{
				this.readBinaryFrom(this.BaCache, true);
			}
			this.BaCache = null;
		}

		public void readBinaryFrom(ByteReader Ba, bool record_buffer = false)
		{
			this.Clear();
			int num = Ba.readByte();
			this.max_i = (int)Ba.readUShort();
			this.use_ptr = (int)Ba.readUShort();
			for (int i = 0; i < this.max_i; i++)
			{
				EvTextLog.LogItem logItem = default(EvTextLog.LogItem);
				logItem.key = Ba.readPascalString("utf-8", false);
				logItem.person = Ba.readPascalString("utf-8", false);
				this.ALog[i] = logItem;
			}
			if (num >= 1)
			{
				this.readInjection(Ba, this.OAinj);
			}
			if (record_buffer)
			{
				int num2 = (int)Ba.readUShort();
				List<EvTextLog.LogItem> abuf = this.ABuf;
				this.ABuf = new List<EvTextLog.LogItem>(num2 + abuf.Count);
				for (int j = 0; j < num2; j++)
				{
					EvTextLog.LogItem logItem2 = default(EvTextLog.LogItem);
					logItem2.key = Ba.readPascalString("utf-8", false);
					logItem2.person = Ba.readPascalString("utf-8", false);
					this.ABuf.Add(logItem2);
				}
				this.ABuf.AddRange(abuf);
				if (num >= 1)
				{
					this.readInjection(Ba, this.OAinjBuf);
				}
			}
		}

		public void writeBinaryTo(ByteArray Ba, bool record_buffer = false)
		{
			Ba.writeByte(1);
			Ba.writeUShort((ushort)this.max_i);
			Ba.writeUShort((ushort)this.use_ptr);
			for (int i = 0; i < this.max_i; i++)
			{
				EvTextLog.LogItem logItem = this.ALog[i];
				Ba.writePascalString(logItem.key, "utf-8");
				Ba.writePascalString(logItem.person, "utf-8");
			}
			this.writeInjection(Ba, this.OAinj);
			if (record_buffer)
			{
				int num = (int)((ushort)this.ABuf.Count);
				Ba.writeUShort((ushort)num);
				for (int j = 0; j < num; j++)
				{
					EvTextLog.LogItem logItem2 = this.ABuf[j];
					Ba.writePascalString(logItem2.key, "utf-8");
					Ba.writePascalString(logItem2.person, "utf-8");
				}
				this.writeInjection(Ba, this.OAinjBuf);
			}
		}

		private void readInjection(ByteReader Ba, BDic<ushort, string[]> OAinj)
		{
			int num = (int)Ba.readUShort();
			for (int i = 0; i < num; i++)
			{
				ushort num2 = Ba.readUShort();
				int num3 = (int)Ba.readUByte();
				string[] array = new string[num3];
				for (int j = 0; j < num3; j++)
				{
					array[j] = Ba.readPascalString("utf-8", false);
				}
				OAinj[num2] = array;
			}
		}

		private void writeInjection(ByteArray Ba, BDic<ushort, string[]> OAinj)
		{
			Ba.writeUShort((ushort)OAinj.Count);
			foreach (KeyValuePair<ushort, string[]> keyValuePair in OAinj)
			{
				Ba.writeUShort(keyValuePair.Key);
				int num = keyValuePair.Value.Length;
				Ba.writeByte(num);
				for (int i = 0; i < num; i++)
				{
					Ba.writePascalString(keyValuePair.Value[i], "utf-8");
				}
			}
		}

		private const string sp_selection = "%SELECT";

		private const string textlog_name_header = "TextLog-";

		private readonly BDic<string, EvTextLog.IcoData> Operson2PF;

		private readonly EvTextLog.LogItem[] ALog;

		private readonly BDic<ushort, string[]> OAinj;

		private readonly BDic<ushort, string[]> OAinjBuf;

		private int use_ptr;

		private int max_i;

		public bool allow_msglog = true;

		private List<EvTextLog.LogItem> ABuf;

		protected EvTextLog.IcoData DefaultIco;

		protected DsnDataImg DsnSelection;

		protected DsnDataImg DsnMain;

		public bool record_selection;

		protected string slice_person_key;

		protected string slice_label;

		protected int slice_index = -1;

		private float use_w;

		private ByteArray BaCache;

		protected struct LogItem
		{
			public LogItem(string _person, string _key)
			{
				this.person = _person;
				this.key = _key;
			}

			public void Set(string _person, string _key)
			{
				this.person = _person;
				this.key = _key;
			}

			public bool Equals(EvTextLog.LogItem I)
			{
				return this.person == I.person && this.key == I.key;
			}

			public string person;

			public string key;
		}

		protected struct IcoData
		{
			public IcoData(PxlFrame _PF, uint _col)
			{
				this.PF = _PF;
				this.col = _col;
			}

			public PxlFrame PF;

			public uint col;
		}
	}
}
