using System;
using UnityEngine;
using XX;

namespace nel
{
	[RequireComponent(typeof(TextRenderer))]
	public class SandText : MonoBehaviour
	{
		public void Start()
		{
		}

		public void Update()
		{
			if (this.t == 0 && MTR.preparedT && MTR.prepare1)
			{
				this.t++;
				TX.changeFamily("en");
				this.Tx = base.GetComponent<TextRenderer>();
				this.Tx.size = MTRX.StorageCabin.def_size;
				this.Tx.enabled = true;
				this.Tx.Start();
				this.Tx.max_swidth_px = IN.wh - 30f;
				this.Tx.text_content = "I can't use any items because I'm casting a spell...";
				GameObject gameObject = IN.CreateGob(base.gameObject, "-baseline");
				this.Md = MeshDrawer.prepareMeshRenderer(gameObject, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
				this.Md.base_z = 0.1f;
			}
			if (this.t >= 1)
			{
				this.Md.Col = MTRX.ColWhite;
				this.Md.Line(-IN.wh, 0f, IN.wh, 0f, 1f, false, 0f, 0f);
				this.Md.Line(0f, -IN.hh, 0f, IN.hh, 1f, false, 0f, 0f);
				if (!this.Tx.textIs(""))
				{
					this.Md.Col = C32.d2c(2013265919U);
					float swidth_px = this.Tx.get_swidth_px();
					float sheight_px = this.Tx.get_sheight_px();
					this.Md.Box(-swidth_px * 0.5f * (float)this.Tx.alignx, sheight_px * 0.5f * (float)this.Tx.aligny, swidth_px, sheight_px, 1f, false);
				}
				this.Md.updateForMeshRenderer(false);
			}
		}

		private int t;

		private TextRenderer Tx;

		private MeshDrawer Md;
	}
}
