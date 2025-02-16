using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

namespace XX
{
	public class MProperty
	{
		public MProperty(MeshRenderer _Mrd = null, int _mpb_target_id = -1)
		{
			this.Mpb = new MaterialPropertyBlock();
			this.OSrc = new BDic<string, MProperty.PropEntry>();
			this.AChecked = new MProperty.PropEntryChecked[1];
			this.Mrd = _Mrd;
			this.mpb_target_id = _mpb_target_id;
		}

		public Texture GetTexture(string key)
		{
			MProperty.PropEntry propEntry;
			if (this.OSrc.TryGetValue(key, out propEntry))
			{
				return propEntry.PObj as Texture;
			}
			return null;
		}

		public MProperty SetTexture(string key, Texture T, bool fine_mrd = true)
		{
			this.checked_maxi = 0;
			this.OSrc[key] = new MProperty.PropEntry
			{
				type = MProperty.EType.TEXTURE,
				PObj = T
			};
			if (T != null)
			{
				this.Mpb.SetTexture(key, T);
			}
			if (fine_mrd)
			{
				this.fineMpb();
			}
			return this;
		}

		public MProperty SetTexture(string key, MImage MI, bool fine_mrd = true)
		{
			return this.SetTexture(key, (MI != null) ? MI.Tx : null, fine_mrd);
		}

		public MProperty SetColor(string key, Color32 C, bool fine_mrd = true)
		{
			this.checked_maxi = 0;
			this.OSrc[key] = new MProperty.PropEntry
			{
				type = MProperty.EType.COLOR,
				PVec = new Vector4((float)C.r, (float)C.g, (float)C.b, (float)C.a)
			};
			this.Mpb.SetColor(key, C);
			if (fine_mrd)
			{
				this.fineMpb();
			}
			return this;
		}

		public MProperty SetFloat(string key, float v, bool fine_mrd = true)
		{
			this.checked_maxi = 0;
			this.OSrc[key] = new MProperty.PropEntry
			{
				type = MProperty.EType.FLOAT,
				PVec = new Vector4(v, 0f, 0f, 0f)
			};
			this.Mpb.SetFloat(key, v);
			if (fine_mrd)
			{
				this.fineMpb();
			}
			return this;
		}

		public MProperty SetVector(string key, Vector4 V, bool fine_mrd = true)
		{
			this.checked_maxi = 0;
			this.OSrc[key] = new MProperty.PropEntry
			{
				type = MProperty.EType.VECTOR,
				PVec = V
			};
			this.Mpb.SetVector(key, V);
			if (fine_mrd)
			{
				this.fineMpb();
			}
			return this;
		}

		public bool TryGetValue(string key, out Vector4 V)
		{
			MProperty.PropEntry propEntry;
			if (this.OSrc.TryGetValue(key, out propEntry))
			{
				V = propEntry.PVec;
				return true;
			}
			V = Vector4.zero;
			return false;
		}

		public bool TryGetValue(string key, out Color32 C)
		{
			MProperty.PropEntry propEntry;
			if (this.OSrc.TryGetValue(key, out propEntry) && propEntry.type == MProperty.EType.COLOR)
			{
				C = new Color32((byte)propEntry.PVec[0], (byte)propEntry.PVec[1], (byte)propEntry.PVec[2], (byte)propEntry.PVec[3]);
				return true;
			}
			C = MTRX.ColWhite;
			return false;
		}

		public MProperty Clear(bool fine_mrd = true)
		{
			this.Mpb.Clear();
			this.OSrc.Clear();
			this.checked_maxi = 0;
			if (fine_mrd)
			{
				this.fineMpb();
			}
			return this;
		}

		public MProperty fineMpb()
		{
			if (this.Mrd != null)
			{
				if (this.mpb_target_id == -1)
				{
					this.Mrd.SetPropertyBlock(this.Mpb);
				}
				else
				{
					using (BList<Material> blist = ListBuffer<Material>.Pop(0))
					{
						this.Mrd.GetSharedMaterials(blist);
						if (X.BTW(0f, (float)this.mpb_target_id, (float)blist.Count))
						{
							this.Mrd.SetPropertyBlock(this.Mpb, this.mpb_target_id);
						}
					}
				}
			}
			return this;
		}

		public MProperty ReleaseGLCache()
		{
			this.checked_maxi = 0;
			return this;
		}

		public MProperty Push(Material Mtr)
		{
			if (this.checked_maxi == 0)
			{
				if (this.OSrc.Count == 0)
				{
					return this;
				}
				if (this.AChecked.Length < this.OSrc.Count)
				{
					Array.Resize<MProperty.PropEntryChecked>(ref this.AChecked, this.OSrc.Count);
				}
				foreach (KeyValuePair<string, MProperty.PropEntry> keyValuePair in this.OSrc)
				{
					MProperty.PropEntry value = keyValuePair.Value;
					if (Mtr.HasProperty(keyValuePair.Key))
					{
						MProperty.PropEntryChecked[] achecked = this.AChecked;
						int num = this.checked_maxi;
						this.checked_maxi = num + 1;
						achecked[num] = new MProperty.PropEntryChecked(keyValuePair.Key, value);
					}
				}
			}
			for (int i = 0; i < this.checked_maxi; i++)
			{
				this.AChecked[i].Push(Mtr);
			}
			return this;
		}

		public MProperty Pop(Material Mtr)
		{
			for (int i = 0; i < this.checked_maxi; i++)
			{
				this.AChecked[i].Pop(Mtr);
			}
			return this;
		}

		public bool isUseable(int id)
		{
			return this.mpb_target_id < 0 || this.mpb_target_id == id;
		}

		private MaterialPropertyBlock Mpb;

		public MeshRenderer Mrd;

		private BDic<string, MProperty.PropEntry> OSrc;

		private MProperty.PropEntryChecked[] AChecked;

		public int mpb_target_id = -1;

		private int checked_maxi;

		private enum EType
		{
			TEXTURE,
			FLOAT,
			VECTOR,
			COLOR
		}

		private struct PropEntry
		{
			public MProperty.EType type;

			public Vector4 PVec;

			public object PObj;
		}

		private struct PropEntryChecked
		{
			public PropEntryChecked(string _key, MProperty.PropEntry _Src)
			{
				this.key = _key;
				this.Src = _Src;
				this.PObj = null;
				this.PVec = Vector4.zero;
			}

			public void Push(Material Mtr)
			{
				switch (this.Src.type)
				{
				case MProperty.EType.TEXTURE:
					this.PObj = Mtr.GetTexture(this.key);
					Mtr.SetTexture(this.key, this.Src.PObj as Texture);
					return;
				case MProperty.EType.FLOAT:
					this.PVec = new Vector4(Mtr.GetFloat(this.key), 0f, 0f, 0f);
					Mtr.SetFloat(this.key, this.Src.PVec.x);
					return;
				case MProperty.EType.VECTOR:
					this.PVec = Mtr.GetVector(this.key);
					Mtr.SetVector(this.key, this.Src.PVec);
					return;
				case MProperty.EType.COLOR:
					Mtr.GetColor(this.key);
					Mtr.SetColor(this.key, new Color32((byte)this.Src.PVec.x, (byte)this.Src.PVec.y, (byte)this.Src.PVec.z, (byte)this.Src.PVec.w));
					return;
				default:
					return;
				}
			}

			public void Pop(Material Mtr)
			{
				switch (this.Src.type)
				{
				case MProperty.EType.TEXTURE:
					Mtr.SetTexture(this.key, this.PObj as Texture);
					this.PObj = null;
					return;
				case MProperty.EType.FLOAT:
					Mtr.SetFloat(this.key, this.PVec.x);
					return;
				case MProperty.EType.VECTOR:
					Mtr.SetVector(this.key, this.PVec);
					return;
				case MProperty.EType.COLOR:
					Mtr.SetColor(this.key, new Color32((byte)this.PVec.x, (byte)this.PVec.y, (byte)this.PVec.z, (byte)this.PVec.w));
					return;
				default:
					return;
				}
			}

			public string key;

			public Vector4 PVec;

			public object PObj;

			public MProperty.PropEntry Src;
		}
	}
}
