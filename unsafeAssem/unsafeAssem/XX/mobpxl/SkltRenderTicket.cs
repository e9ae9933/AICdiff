using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

namespace XX.mobpxl
{
	public class SkltRenderTicket
	{
		public SkltRenderTicket()
		{
			this.OAImgRect = new BDic<SkltImage, RectInt[]>();
		}

		public void Clear()
		{
			foreach (KeyValuePair<SkltImage, RectInt[]> keyValuePair in this.OAImgRect)
			{
				for (int i = keyValuePair.Value.Length - 1; i >= 0; i--)
				{
					keyValuePair.Value[i] = new RectInt(0, 0, 0, 0);
				}
			}
			this.atlas_created = false;
		}

		internal void createAtlas(RectAtlasTexture CalcAtlas, MobSklt Sklt)
		{
			if (this.atlas_created)
			{
				return;
			}
			int count = Sklt.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = Sklt.AParts[i];
				if (skltParts != null)
				{
					int count2 = skltParts.AImage.Count;
					for (int j = 0; j < count2; j++)
					{
						SkltImage skltImage = skltParts.AImage[j];
						this.createAtlasImage(skltImage, CalcAtlas);
					}
				}
			}
			this.atlas_created = true;
		}

		internal RectInt[] createAtlasImage(SkltImage Si, RectAtlasTexture CalcAtlas)
		{
			RectInt[] array2;
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				Si.Source.CopyAllPType(blist);
				RectInt[] array;
				if (!this.OAImgRect.TryGetValue(Si, out array))
				{
					array2 = (this.OAImgRect[Si] = new RectInt[blist.Count]);
					array = array2;
				}
				else if (array.Length != blist.Count)
				{
					Array.Resize<RectInt>(ref array, blist.Count);
					this.OAImgRect[Si] = array;
				}
				else if (array[0].width > 0)
				{
					return array;
				}
				if (CalcAtlas == null)
				{
					array2 = null;
				}
				else
				{
					for (int i = 0; i < blist.Count; i++)
					{
						SkltImageSrc.ISrcPat forAnm = Si.Source.GetForAnm(blist[i]);
						int num;
						RenderTexture renderTexture;
						RectInt rectInt = CalcAtlas.createRect(2 + forAnm.Img.width, 2 + forAnm.Img.height, out num, out renderTexture, true);
						array[i] = new RectInt(rectInt.x + 1, rectInt.y + 1, rectInt.width - 2, rectInt.height - 2);
					}
					array2 = array;
				}
			}
			return array2;
		}

		public bool atlas_created;

		private readonly BDic<SkltImage, RectInt[]> OAImgRect;
	}
}
