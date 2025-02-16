using System;
using UnityEngine;

namespace XX.mobpxl
{
	public class SkltJoint
	{
		public static void Sort(SkltParts _SortParts, SkltJoint[] A)
		{
			SkltJoint.SortParts = _SortParts;
		}

		internal SkltJoint(string _name, float _x, float _y, float agR, MobSklt Sklt, PARTS_TYPE parts_type_d)
		{
			this.name = _name;
			this.Pos = new MobSkltPosition(null, null);
			this.Pos.PosAbs = new Vector2(_x, _y);
			this.Pos.rotateR_abs = agR;
			this.Pos.need_fine_abs2pos = true;
			PARTS_TYPE parts_TYPE;
			switch (parts_type_d)
			{
			case PARTS_TYPE.LARM2:
				parts_TYPE = PARTS_TYPE.LARM;
				goto IL_0079;
			case PARTS_TYPE.LARM:
			case PARTS_TYPE.LFOOT:
				break;
			case PARTS_TYPE.LFOOT2:
				parts_TYPE = PARTS_TYPE.LFOOT;
				goto IL_0079;
			case PARTS_TYPE.RFOOT2:
				parts_TYPE = PARTS_TYPE.RFOOT;
				goto IL_0079;
			default:
				if (parts_type_d == PARTS_TYPE.RARM2)
				{
					parts_TYPE = PARTS_TYPE.RARM;
					goto IL_0079;
				}
				break;
			}
			parts_TYPE = PARTS_TYPE.BODY;
			IL_0079:
			this.BelongS = Sklt.MakeParts(parts_TYPE);
			this.BelongD = Sklt.MakeParts(parts_type_d);
			this.BelongS.AddJoint(this);
			this.BelongD.AddJoint(this);
		}

		public SkltJoint(SkltJoint Src, MobSklt Sklt)
		{
			this.name = Src.name;
			this.Pos = new MobSkltPosition(null, null);
			this.Pos.CopyFrom(Src.Pos, false);
			this.BelongS = Sklt.MakeParts(Src.BelongS.type);
			this.BelongD = Sklt.MakeParts(Src.BelongD.type);
			this.BelongS.AddJoint(this);
			this.BelongD.AddJoint(this);
		}

		internal static int fnSort(SkltParts Con, SkltJoint A, SkltJoint B)
		{
			if (Con == A.BelongS != (Con == B.BelongS))
			{
				if (Con != A.BelongS)
				{
					return 1;
				}
				return -1;
			}
			else
			{
				if (Con == A.BelongS)
				{
					return A.BelongD.type - B.BelongD.type;
				}
				return A.BelongS.type - B.BelongS.type;
			}
		}

		public readonly string name;

		public readonly MobSkltPosition Pos;

		public readonly SkltParts BelongS;

		public readonly SkltParts BelongD;

		internal static SkltParts SortParts;
	}
}
