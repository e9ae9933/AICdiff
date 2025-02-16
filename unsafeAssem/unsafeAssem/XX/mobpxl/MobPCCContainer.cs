using System;
using System.Collections.Generic;
using System.IO;
using Better;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;

namespace XX.mobpxl
{
	public class MobPCCContainer
	{
		public MobPCCContainer()
		{
			this.OACC = new BDic<string, MobPCCContainer.ACC>();
			if (MobPCCContainer.Pcc_dir == null)
			{
				MobPCCContainer.Pcc_dir = Path.Combine(Application.streamingAssetsPath, "mobpcc");
			}
			this.ATexture0 = new List<Texture>(1);
			this.ATextureP = new List<Texture>(1);
			this.ATxApplied = new List<RenderTexture>(1);
		}

		public MobPCCContainer Clear()
		{
			this.OACC.Clear();
			return this;
		}

		internal MobPCCContainer MergeOACC(BDic<string, MobPCCContainer.ACC> _OAcc)
		{
			if (_OAcc != null)
			{
				X.objMerge<string, MobPCCContainer.ACC>(this.OACC, _OAcc, false);
			}
			return this;
		}

		internal MobPCCContainer MergeOACC(string _key, MobPCCContainer.ACC Acc)
		{
			this.OACC[_key] = Acc;
			return this;
		}

		internal BDic<string, MobPCCContainer.ACC> getWholePCCObject()
		{
			return this.OACC;
		}

		public PxlCharacter Init(string _name, PxlCharacter Chr)
		{
			this.name = _name;
			this.chr_name = Chr.title;
			this.Gen = null;
			this.ATexture0.Clear();
			this.ATextureP.Clear();
			this.AddChr(Chr);
			return Chr;
		}

		public void Init(MobGenerator _Gen, Texture TxPArts)
		{
			this.ATexture0.Clear();
			this.ATextureP.Clear();
			this.ATextureP.Add(TxPArts);
			this.Gen = _Gen;
		}

		public PxlCharacter AddChr(PxlCharacter Chr)
		{
			PxlsTexture[] externalTextureArray = Chr.getExternalTextureArray();
			this.ATexture0.Add((externalTextureArray != null) ? externalTextureArray[0].Image : null);
			this.ATextureP.Add((externalTextureArray != null && externalTextureArray.Length != 0) ? externalTextureArray[1].Image : null);
			if (this.ATexture0.Count == 1 && this.OACC.Count > 0)
			{
				MobPCCContainer.replacePartsInfoForOACC(Chr, this.OACC);
			}
			return Chr;
		}

		internal static void replacePartsInfoForOACC(PxlCharacter Chr, BDic<string, MobPCCContainer.ACC> OACC)
		{
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				foreach (KeyValuePair<string, MobPCCContainer.ACC> keyValuePair in OACC)
				{
					int partsInfoIndex = Chr.getPartsInfoIndex(keyValuePair.Key, true);
					if (partsInfoIndex < 0)
					{
						blist.Add(keyValuePair.Key);
					}
					else
					{
						keyValuePair.Value.PI = Chr.APartsInfo[partsInfoIndex];
					}
				}
				if (blist.Count > 0)
				{
					X.dl(blist.Count.ToString() + "個のパーツが見つかりませんでした。", null, false, true);
					for (int i = blist.Count - 1; i >= 0; i--)
					{
						OACC.Remove(blist[i]);
					}
				}
			}
		}

		public string getFirstParts()
		{
			using (Dictionary<string, MobPCCContainer.ACC>.Enumerator enumerator = this.OACC.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					KeyValuePair<string, MobPCCContainer.ACC> keyValuePair = enumerator.Current;
					return keyValuePair.Key;
				}
			}
			return null;
		}

		private void initApplyEffect(int index, Texture Tx0, out Texture TxP, out RenderTexture Dest, out RenderTexture TxBuffer, out MeshDrawer MdForPc)
		{
			TxP = this.ATextureP[index];
			while (this.ATxApplied.Count < this.ATexture0.Count)
			{
				this.ATxApplied.Add(null);
			}
			Texture texture = null;
			MdForPc = null;
			Dest = null;
			if (this.Gen == null)
			{
				texture = this.ATexture0[index];
				Dest = this.ATxApplied[index];
				BLIT.Alloc(ref Dest, texture.width, texture.height, false, RenderTextureFormat.ARGB32, 0);
				this.ATxApplied[index] = Dest;
			}
			else
			{
				Dest = this.Gen.GetAtlasTextureForPccApplyInit(out MdForPc, null, Tx0 as RenderTexture);
			}
			GL.PushMatrix();
			BLIT.Alloc(ref MobPCCContainer.TxBuffer, Dest.width, Dest.height, false, RenderTextureFormat.ARGB32, 0);
			TxBuffer = MobPCCContainer.TxBuffer;
			Dest.name = "Dest";
			TxBuffer.name = "Buffer";
			if (Tx0 == null)
			{
				if (this.Gen != null)
				{
					GL.PopMatrix();
					return;
				}
				Tx0 = texture;
			}
			BLIT.JustPaste(Tx0, Dest);
			GL.PopMatrix();
		}

		private void endApplyEffect(ref RenderTexture Dest, RenderTexture TxBuffer)
		{
			if (Dest == MobPCCContainer.TxBuffer)
			{
				BLIT.JustPaste(Dest, TxBuffer);
				Dest = TxBuffer;
			}
		}

		private MobPCCContainer applyEffect(MobPCCContainer.ACC Acc, Texture TxP, ref RenderTexture Dest, ref RenderTexture Buffer, MeshDrawer MdForPc)
		{
			if (TxP == null)
			{
				return this;
			}
			int count = Acc.A.Count;
			for (int i = 0; i < count; i++)
			{
				MobPCC mobPCC = Acc.A[i];
				if (mobPCC.visible)
				{
					mobPCC.executeToImage(Acc.PI, TxP, Buffer, Dest, MdForPc);
					RenderTexture renderTexture = Dest;
					RenderTexture renderTexture2 = Buffer;
					Buffer = renderTexture;
					Dest = renderTexture2;
				}
			}
			return this;
		}

		public void applyEffectWhole(string skip_name = null)
		{
			int count = this.ATextureP.Count;
			for (int i = 0; i < count; i++)
			{
				this.applyEffectWhole(i, skip_name);
			}
		}

		private RenderTexture applyEffectWhole(int index, string skip_name = null)
		{
			Texture texture;
			RenderTexture renderTexture;
			RenderTexture renderTexture2;
			MeshDrawer meshDrawer;
			this.initApplyEffect(index, null, out texture, out renderTexture, out renderTexture2, out meshDrawer);
			foreach (KeyValuePair<string, MobPCCContainer.ACC> keyValuePair in this.OACC)
			{
				if (!(keyValuePair.Key == skip_name))
				{
					this.applyEffect(keyValuePair.Value, texture, ref renderTexture, ref renderTexture2, meshDrawer);
				}
			}
			this.endApplyEffect(ref renderTexture, renderTexture2);
			return renderTexture;
		}

		public RenderTexture getAppliedImage(int index = 0)
		{
			return this.ATxApplied[index];
		}

		public void readFromBytes(ByteArray Ba, PxlCharacter Pcr)
		{
			this.Clear();
			Ba.readByte();
			this.name = Ba.readString("utf-8", false);
			this.chr_name = Ba.readString("utf-8", false);
			Ba.readByte();
			MobPCCContainer.readFromBytes(Ba, ref this.OACC, Pcr);
		}

		internal static void readFromBytes(ByteArray Ba, ref BDic<string, MobPCCContainer.ACC> OACC, PxlCharacter Pcr)
		{
			bool flag = false;
			if (OACC != null)
			{
				OACC.Clear();
				flag = true;
			}
			int num = Ba.readByte();
			if (num == 0)
			{
				return;
			}
			if (OACC == null)
			{
				OACC = new BDic<string, MobPCCContainer.ACC>(num);
			}
			for (int i = 0; i < num; i++)
			{
				string text = Ba.readPascalString("utf-8", false);
				MobPCCContainer.ACC acc = MobPCCContainer.ACC.readFromBytesACC(Ba, flag ? X.Get<string, MobPCCContainer.ACC>(OACC, text) : null);
				if (acc != null)
				{
					if (acc.PI == null && Pcr != null)
					{
						int partsInfoIndex = Pcr.getPartsInfoIndex(text, false);
						if (partsInfoIndex < 0)
						{
							goto IL_0086;
						}
						acc.PI = Pcr.APartsInfo[partsInfoIndex];
					}
					OACC[text] = acc;
				}
				IL_0086:;
			}
		}

		private BDic<string, MobPCCContainer.ACC> OACC;

		public string chr_name = "_";

		public string name = "";

		public const string dat_extention = ".mpcc.bytes";

		public const string mob_pxl_dir = "MapChars/";

		public const string dat_extention_nodot = "mpcc.bytes";

		public static string Pcc_dir;

		private readonly List<Texture> ATexture0;

		private readonly List<Texture> ATextureP;

		private readonly List<RenderTexture> ATxApplied;

		private static RenderTexture TxBuffer;

		private MobGenerator Gen;

		internal class ACC
		{
			public ACC(PxlPartsInfo _PI, int capacity = 1)
			{
				this.PI = _PI;
				this.A = new List<MobPCC>(capacity);
			}

			public bool is_empty
			{
				get
				{
					return this.A.Count == 0;
				}
			}

			public MobPCCHsv getTopHsv(bool create_if_empty = true)
			{
				int count = this.A.Count;
				for (int i = 0; i < count; i++)
				{
					MobPCCHsv mobPCCHsv = this.A[i] as MobPCCHsv;
					if (mobPCCHsv != null && mobPCCHsv.visible)
					{
						return mobPCCHsv;
					}
				}
				if (create_if_empty)
				{
					MobPCCHsv mobPCCHsv2 = new MobPCCHsv();
					this.A.Add(mobPCCHsv2);
					return mobPCCHsv2;
				}
				return null;
			}

			public void CopyFrom(MobPCCContainer.ACC Src)
			{
				this.A.Clear();
				this.A.AddRange(Src.A);
			}

			internal static MobPCCContainer.ACC readFromBytesACC(ByteArray Ba, MobPCCContainer.ACC Target = null)
			{
				if (Target != null)
				{
					Target.A.Clear();
				}
				int num = Ba.readByte();
				if (num <= 0)
				{
					return null;
				}
				MobPCCContainer.ACC acc = Target ?? new MobPCCContainer.ACC(null, num);
				for (int i = 0; i < num; i++)
				{
					MobPCC mobPCC = MobPCC.readFromBytes(Ba);
					if (mobPCC != null)
					{
						acc.A.Add(mobPCC);
					}
				}
				return acc;
			}

			public readonly List<MobPCC> A;

			public PxlPartsInfo PI;
		}
	}
}
