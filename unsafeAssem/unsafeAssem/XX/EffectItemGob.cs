using System;
using Better;
using UnityEngine;

namespace XX
{
	public class EffectItemGob : EffectItem
	{
		public static void initEffectItemGob()
		{
			if (EffectItemGob.OPref == null)
			{
				EffectItemGob.OPref = new BDic<string, GameObject>();
			}
		}

		public EffectItemGob(IEffectSetter _EF, string _title = "", float _x = 0f, float _y = 0f, float _z = 0f, int _time = 0, int _saf = 0)
			: base(_EF, _title, _x, _y, _z, _time, _saf)
		{
		}

		public override void destruct()
		{
			this.EF.RemoveObject(this.Gob);
			base.destruct();
		}

		public override EffectItem initEffect(string type_name = "")
		{
			base.FnDef = new FnEffectRun(this.runBasic);
			string title = this.title;
			if (title != null && title == "star_shockwave")
			{
				this.atype = (EffectItemGob.ATYPE)9U;
			}
			else if (base.initEffect((type_name == "") ? "XX.EffectItemGob,unsafeAssem" : type_name) == null)
			{
				return null;
			}
			this.Spr = this.Gob.GetComponent<SpriteRenderer>();
			return this;
		}

		public bool runBasic(EffectItem EF)
		{
			if (this.af >= (float)this.time)
			{
				return false;
			}
			float num = this.af / (float)this.time;
			if ((this.atype & EffectItemGob.ATYPE.EXTEND) > (EffectItemGob.ATYPE)0U)
			{
				float num2 = (0.2f + 0.8f * X.ZSIN(num)) * this.z;
				this.Gob.transform.localScale = new Vector3(num2, num2, 1f);
			}
			else if ((this.atype & EffectItemGob.ATYPE.SMALLEN) > (EffectItemGob.ATYPE)0U)
			{
				float num3 = (0.1f + 0.9f * X.ZSIN(1f - num)) * this.z;
				this.Gob.transform.localScale = new Vector3(num3, num3, 1f);
			}
			if ((this.atype & EffectItemGob.ATYPE.FADEIN) > (EffectItemGob.ATYPE)0U)
			{
				X.setSpriteAlpha(this.Spr, X.ZSIN(num));
			}
			else if ((this.atype & EffectItemGob.ATYPE.FADEOUT) > (EffectItemGob.ATYPE)0U)
			{
				X.setSpriteAlpha(this.Spr, 1f - num);
			}
			return true;
		}

		private GameObject Gob;

		private EffectItemGob.ATYPE atype;

		private SpriteRenderer Spr;

		public static BDic<string, GameObject> OPref;

		public enum ATYPE : uint
		{
			EXTEND = 1U,
			SMALLEN,
			FADEIN = 4U,
			FADEOUT = 8U
		}
	}
}
