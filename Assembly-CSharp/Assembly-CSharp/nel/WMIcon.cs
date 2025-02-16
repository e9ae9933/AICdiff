using System;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class WMIcon
	{
		public int sort_variable
		{
			get
			{
				int num = (int)this.type;
				if (this.type == WMIcon.TYPE.TREASURE)
				{
					num += (this.cleared ? 0 : 20);
				}
				if (this.type == WMIcon.TYPE.BENCH)
				{
					num++;
				}
				if (this.type == WMIcon.TYPE.ENEMY)
				{
					num--;
				}
				return num;
			}
		}

		public WMIcon(WMIcon.TYPE _type)
		{
			this.type = _type;
		}

		public WMIcon(ByteReader Ba, int vers)
		{
			this.readBinaryFrom(Ba, vers);
		}

		public bool cleared
		{
			get
			{
				this.fineCleared();
				return (this.flags_ & 3) == 1;
			}
			set
			{
				this.flags_ = (byte)(((int)this.flags_ & -4) | 2);
			}
		}

		public bool noticed
		{
			get
			{
				return (this.flags_ & 4) == 0;
			}
			set
			{
				this.flags_ = (byte)(((int)this.flags_ & -5) | (value ? 0 : 4));
			}
		}

		private void fineCleared()
		{
			if ((this.flags_ & 3) == 2)
			{
				this.flags_ = (byte)((int)this.flags_ & -4);
				if (this.sf_key != null)
				{
					this.flags_ |= ((COOK.getSF(this.sf_key) != 0) ? 1 : 0);
				}
			}
		}

		public Vector2 getMapWmPos(WholeMapItem.WMItem Wmi)
		{
			Vector2 positionOnWm = Wmi.getPositionOnWm((float)this.x, (float)this.y, true, false);
			positionOnWm.x -= Wmi.Rc.x;
			positionOnWm.y -= Wmi.Rc.y;
			return positionOnWm;
		}

		public void drawTo(MeshDrawer Md, WholeMapItem.WMItem Wmi, float lx, float ty, float cell_size, float alpha)
		{
			Vector2 mapWmPos = this.getMapWmPos(Wmi);
			lx += mapWmPos.x * cell_size;
			ty -= mapWmPos.y * cell_size;
			Md.Col = MTRX.ColWhite;
			bool flag = true;
			switch (this.type)
			{
			case WMIcon.TYPE.TREASURE:
				Md.RotaPF(lx, ty, 1f, 1f, 0f, MTRX.getPF(this.cleared ? "wmap_treasure_cleared" : "wmap_treasure"), false, false, false, uint.MaxValue, false, 0);
				break;
			case WMIcon.TYPE.BENCH:
				Md.RotaPF(lx, ty, 1f, 1f, 0f, MTRX.getPF("wmap_bench"), false, false, false, uint.MaxValue, false, 0);
				break;
			case WMIcon.TYPE.ENEMY:
			{
				if (TX.noe(this.sf_key))
				{
					return;
				}
				NightController nightCon = (M2DBase.Instance as NelM2DBase).NightCon;
				NightController.SummonerData lpInfo = nightCon.GetLpInfo(this.sf_key, null, false);
				if (nightCon.isSuddenOnMap(lpInfo, Wmi.SrcMap))
				{
					return;
				}
				bool summoner_is_night = lpInfo.summoner_is_night;
				if (lpInfo.defeat_count == 0)
				{
					Md.RotaPF(lx, ty, 1f, 1f, 0f, MTRX.getPF("wmap_enemy"), false, false, false, uint.MaxValue, false, 0);
				}
				else if (lpInfo.defeated_in_session)
				{
					Md.RotaPF(lx, ty, 1f, 1f, 0f, MTRX.getPF(summoner_is_night ? "wmap_enemy_defeated_nombox_night" : "wmap_enemy_defeated_nombox"), false, false, false, uint.MaxValue, false, 0);
				}
				else
				{
					Md.RotaPF(lx, ty, 1f, 1f, 0f, MTRX.getPF(summoner_is_night ? "wmap_enemy_defeated_night" : "wmap_enemy_defeated"), false, false, false, uint.MaxValue, false, 0);
				}
				flag = false;
				break;
			}
			case WMIcon.TYPE.MATERIAL:
				Md.RotaPF(lx, ty, 1f, 1f, 0f, MTRX.getPF("wmap_material"), false, false, false, uint.MaxValue, false, 0);
				break;
			}
			if (flag && this.cleared)
			{
				Md.RotaPF(lx + 4f, ty - 7f, 1f, 1f, 0f, MTRX.getPF("nel_check"), false, false, false, uint.MaxValue, false, 0);
				Md.Col = C32.d2c(4283780170U);
				Md.RotaPF(lx + 4f, ty - 6f, 1f, 1f, 0f, MTRX.getPF("nel_check"), false, false, false, uint.MaxValue, false, 0);
			}
		}

		public static WMIcon createFromLabelPoint(M2LabelPoint Lp)
		{
			return null;
		}

		public void readBinaryFrom(ByteReader Ba, int vers)
		{
			this.x = Ba.readUShort();
			this.y = Ba.readUShort();
			this.flags_ = (byte)Ba.readByte();
			this.type = (WMIcon.TYPE)Ba.readByte();
			if (vers >= 1)
			{
				string text = Ba.readString("utf-8", false);
				this.sppos_key = (TX.valid(text) ? text : null);
				if (vers >= 3)
				{
					string text2 = Ba.readString("utf-8", false);
					this.sf_key = (TX.valid(text2) ? text2 : null);
					return;
				}
			}
			else
			{
				this.sppos_key = null;
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeUShort(this.x);
			Ba.writeUShort(this.y);
			this.fineCleared();
			Ba.writeByte((int)this.flags_);
			Ba.writeByte((int)this.type);
			Ba.writeString(this.sppos_key, "utf-8");
			if (this.type == WMIcon.TYPE.ENEMY)
			{
				Ba.writeString(this.sf_key, "utf-8");
				return;
			}
			Ba.writeString(null, "utf-8");
		}

		public bool isForSummoner(string smn_key)
		{
			return this.type == WMIcon.TYPE.ENEMY && (TX.isEnd(this.sf_key, smn_key) && TX.isStart(this.sf_key, "..", this.sf_key.Length - smn_key.Length - 2));
		}

		public ushort x;

		public ushort y;

		private byte flags_ = 6;

		public string sf_key;

		public string sppos_key;

		public WMIcon.TYPE type;

		public enum TYPE
		{
			OTHER,
			TREASURE,
			BENCH,
			ENEMY,
			MATERIAL
		}
	}
}
