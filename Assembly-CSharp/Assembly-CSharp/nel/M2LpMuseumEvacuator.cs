using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public sealed class M2LpMuseumEvacuator : NelLp
	{
		public M2LpMuseumEvacuator(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initActionPre()
		{
			this.Acateg_id = new List<int>(1);
			this.APreLoad = new List<M2ChipImage>(1);
			META meta = new META(this.comment);
			string[] array = meta.Get("category_id");
			if (array != null)
			{
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					this.Acateg_id.Add(X.NmI(array[i], -1, false, false));
				}
			}
			M2ImageContainer.ImgDir imgDir = base.nM2D.IMGS.GetImgDir(meta.GetS("check_dir"));
			if (imgDir == null)
			{
				X.de("M2LpMuseumEvacuator::initActionPre check_dir 未定義", null);
				return;
			}
			foreach (KeyValuePair<string, M2ChipImage> keyValuePair in imgDir)
			{
				M2ChipImage value = keyValuePair.Value;
				if (value.Meta.Get("museum") != null)
				{
					int i2 = value.Meta.GetI("museum", 0, 2);
					if (this.isEvacTarget(i2))
					{
						this.APreLoad.Add(value);
					}
				}
			}
		}

		public int preload_image_count
		{
			get
			{
				return this.APreLoad.Count;
			}
		}

		public M2ChipImage GetPreLoadImg(int i)
		{
			return this.APreLoad[i];
		}

		public bool isEvacTarget(int categ_id)
		{
			return this.Acateg_id.IndexOf(categ_id) >= 0;
		}

		private List<int> Acateg_id;

		private List<M2ChipImage> APreLoad;
	}
}
