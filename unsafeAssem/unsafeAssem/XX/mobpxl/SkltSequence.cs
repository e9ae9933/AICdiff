using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;

namespace XX.mobpxl
{
	public class SkltSequence
	{
		public SkltSequence(string _name = "_", string _type = "stand", ByteArray Ba = null, int vers = 10)
		{
			this.name = _name;
			if (Ba != null)
			{
				this.readFromBytes(Ba, vers);
				return;
			}
			this.type = _type;
			this.AFrame = new List<SkltSequence.SkltFrame>(3);
			this.AFrame.Add(new SkltSequence.SkltFrame());
		}

		public SkltSequence.SkltFrame getFrame(int i)
		{
			return this.AFrame[i];
		}

		public int countFrames()
		{
			return this.AFrame.Count;
		}

		public bool temporary_unused
		{
			get
			{
				return this.referred <= 0 && TX.isStart(this.name, "_", 0);
			}
		}

		public void readFromBytes(ByteArray Ba, int vers = 10)
		{
			this.type = Ba.readPascalString("utf-8", false);
			int num = (int)Ba.readUByte();
			if (this.AFrame == null)
			{
				this.AFrame = new List<SkltSequence.SkltFrame>(num);
			}
			while (this.AFrame.Count < num)
			{
				this.AFrame.Add(null);
			}
			for (int i = 0; i < num; i++)
			{
				SkltSequence.SkltFrame skltFrame = this.AFrame[i];
				if (skltFrame == null)
				{
					skltFrame = (this.AFrame[i] = new SkltSequence.SkltFrame());
				}
				skltFrame.readFromBytes(Ba, vers);
			}
			if (vers >= 8)
			{
				this.loop_to = Ba.readUByte();
			}
			if (this.AFrame.Count == 0)
			{
				this.AFrame.Add(new SkltSequence.SkltFrame());
			}
		}

		private List<SkltSequence.SkltFrame> AFrame;

		public string name;

		public string type;

		public byte loop_to;

		public int referred;

		public struct SkltDesc
		{
			public SkltDesc(SkltImage Img)
			{
				this.ptype = Img.Con.type;
				this.lname = Img.Source.Source.name;
				this.fname = Img.Source.Source.pFrm.name;
			}

			public SkltDesc(SkltParts Pi)
			{
				this.ptype = Pi.type;
				this.lname = "";
				this.fname = "";
			}

			public SkltDesc(ByteArray Ba)
			{
				this.ptype = (PARTS_TYPE)Ba.readUByte();
				this.lname = Ba.readPascalString("utf-8", false);
				this.fname = Ba.readPascalString("utf-8", false);
			}

			public string lname;

			public string fname;

			internal PARTS_TYPE ptype;
		}

		public struct PosInfo
		{
			public PosInfo(SkltSequence.PosInfo Src)
			{
				this.Msp = new MobSkltPosition(null, null);
				this.Msp.CopyFromCorrectly(Src.Msp);
				this.ovr_ptype = Src.ovr_ptype;
				this.visible = Src.visible;
				this.order = Src.order;
			}

			public PosInfo(SkltSequence.SkltDesc D, string _ovr_ptype = null)
			{
				this.Msp = new MobSkltPosition(null, null);
				this.ovr_ptype = _ovr_ptype ?? "";
				this.visible = 8;
				this.order = 0;
			}

			public bool valid
			{
				get
				{
					return this.Msp != null;
				}
			}

			public void readFromBytes(ByteArray Ba, int vers = 10)
			{
				if (this.Msp == null)
				{
					this.Msp = new MobSkltPosition(null, null);
				}
				this.Msp = MobSkltPosition.readFromBytes(Ba, this.Msp);
				this.visible = 8;
				this.order = 0;
				if (vers >= 5)
				{
					this.ovr_ptype = Ba.readPascalString("utf-8", false);
					if (vers >= 7)
					{
						this.visible = (byte)Ba.readByte();
					}
					if (vers >= 9)
					{
						this.order = (int)(Ba.readUByte() - 127);
					}
				}
				this.Msp.finePositionBase();
			}

			public MobSkltPosition Msp;

			public string ovr_ptype;

			public byte visible;

			public int order;
		}

		public class SkltFrame
		{
			public SkltFrame()
			{
				this.ORewritePos = new BDic<SkltSequence.SkltDesc, SkltSequence.PosInfo>();
			}

			public SkltFrame(SkltSequence.SkltFrame Src)
				: this()
			{
				foreach (KeyValuePair<SkltSequence.SkltDesc, SkltSequence.PosInfo> keyValuePair in Src.ORewritePos)
				{
					this.ORewritePos[keyValuePair.Key] = new SkltSequence.PosInfo(keyValuePair.Value);
				}
				this.order_writed = Src.order_writed;
			}

			public SkltSequence.SkltFrame Clear()
			{
				this.ORewritePos.Clear();
				this.order_writed = false;
				return this;
			}

			public int countInfo()
			{
				return this.ORewritePos.Count;
			}

			public SkltSequence.PosInfo Add(MobSkltPosition.IPosSyncable Img)
			{
				SkltSequence.SkltDesc sqDescKey = Img.getSqDescKey();
				bool flag = false;
				return this.Add(sqDescKey, ref flag);
			}

			public SkltSequence.PosInfo Add(SkltSequence.SkltDesc D, ref bool created)
			{
				SkltSequence.PosInfo posInfo;
				if (!this.ORewritePos.TryGetValue(D, out posInfo))
				{
					Dictionary<SkltSequence.SkltDesc, SkltSequence.PosInfo> orewritePos = this.ORewritePos;
					SkltSequence.PosInfo posInfo2 = new SkltSequence.PosInfo(D, null);
					orewritePos[D] = posInfo2;
					posInfo = posInfo2;
					created = true;
				}
				return posInfo;
			}

			public SkltSequence.PosInfo Get(MobSkltPosition.IPosSyncable Img)
			{
				return this.Get(Img.getSqDescKey());
			}

			internal SkltSequence.PosInfo Get(SkltSequence.SkltDesc D)
			{
				SkltSequence.PosInfo posInfo;
				if (this.ORewritePos.TryGetValue(D, out posInfo))
				{
					return posInfo;
				}
				return default(SkltSequence.PosInfo);
			}

			public SkltSequence.PosInfo Set(MobSkltPosition.IPosSyncable Img, SkltSequence.PosInfo Mrp)
			{
				return this.Set(Img.getSqDescKey(), Mrp);
			}

			public SkltSequence.PosInfo Set(SkltSequence.SkltDesc D, SkltSequence.PosInfo Mrp)
			{
				this.ORewritePos[D] = Mrp;
				if (Mrp.order != 0)
				{
					this.order_writed = true;
				}
				return default(SkltSequence.PosInfo);
			}

			public bool isChangedOrder(SkltSequence.SkltFrame Src)
			{
				if (Src == this || (!Src.order_writed && !this.order_writed))
				{
					return false;
				}
				if (Src.order_writed != this.order_writed)
				{
					return true;
				}
				foreach (KeyValuePair<SkltSequence.SkltDesc, SkltSequence.PosInfo> keyValuePair in this.ORewritePos)
				{
					int num = 0;
					SkltSequence.PosInfo posInfo;
					if (Src.ORewritePos.TryGetValue(keyValuePair.Key, out posInfo))
					{
						num = posInfo.order;
					}
					if (keyValuePair.Value.order != num)
					{
						return true;
					}
				}
				return false;
			}

			public bool readFromBytes(ByteArray Ba, int vers = 10)
			{
				this.crf60 = (int)Ba.readUByte();
				this.ORewritePos.Clear();
				int num = (int)Ba.readUShort();
				bool flag = false;
				for (int i = 0; i < num; i++)
				{
					SkltSequence.SkltDesc skltDesc = new SkltSequence.SkltDesc(Ba);
					SkltSequence.PosInfo posInfo = this.Add(skltDesc, ref flag);
					posInfo.readFromBytes(Ba, vers);
					this.ORewritePos[skltDesc] = posInfo;
					if (posInfo.order != 0)
					{
						this.order_writed = true;
					}
				}
				return flag;
			}

			public int crf60 = 11;

			private readonly BDic<SkltSequence.SkltDesc, SkltSequence.PosInfo> ORewritePos;

			internal bool order_writed;
		}
	}
}
