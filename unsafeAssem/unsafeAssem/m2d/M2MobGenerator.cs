using System;
using System.Collections.Generic;
using UnityEngine;
using XX;
using XX.mobpxl;

namespace m2d
{
	public sealed class M2MobGenerator : MobGenerator
	{
		public M2MobGenerator(Map2d Mp)
			: base(false)
		{
			this.CalcFinAtlas = new RectAtlasTexture(512, 512, "", false, 0, RenderTextureFormat.ARGB32);
			this.MIFin = new MImage(this.CalcFinAtlas.Tx);
			this.AMga = new List<M2MobGAnimator>(8);
			this.MyMd = new MeshDrawer(null, 4, 6);
			this.MyMd.draw_gl_only = true;
			this.MtrFinalize = MTRX.newMtr(MTRX.ShaderGDT);
			this.MyMd.activate("sklt_render", this.MtrFinalize, false, MTRX.ColWhite, null);
			this.MyMd.setMaterialCloneFlag();
			base.prepareResourcePxl(false);
		}

		public bool ClearTextureOnFlush()
		{
			if (base.ClearTexture(true))
			{
				this.need_refine_fin_atlas = true;
				this.CalcFinAtlas.Clear(512, 512);
				return true;
			}
			return false;
		}

		protected override bool fineMITexture()
		{
			if (this.MtrFinalize.mainTexture != this.CalcAtlas.Tx)
			{
				this.MtrFinalize.mainTexture = this.CalcAtlas.Tx;
				return true;
			}
			return false;
		}

		public void initS(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.AMga.Clear();
			this.ClearFinAtlas();
			base.ClearTexture(true);
		}

		public void ClearFinAtlas()
		{
			this.need_refine_fin_atlas = false;
			this.CalcFinAtlas.Clear(this.CalcFinAtlas.width, this.CalcFinAtlas.height);
			for (int i = this.AMga.Count - 1; i >= 0; i--)
			{
				this.AMga[i].finatlas_created = false;
			}
			this.MIFin.Tx = this.CalcFinAtlas.Tx;
		}

		public SkltRenderTicket createSkltAtlas(Map2d Mp, MobSklt Sklt, string colvari_key)
		{
			SkltRenderTicket skltRenderTicket = base.getAtlasCreatedTicket(Sklt, colvari_key);
			if (skltRenderTicket == null)
			{
				skltRenderTicket = base.createAtlas(Sklt, colvari_key);
				M2MobGenerator.redrawMobgCharacter(Mp);
			}
			return skltRenderTicket;
		}

		public override void destruct()
		{
			base.destruct();
			this.CalcFinAtlas.destruct();
			this.MyMd.destruct();
			this.MIFin.Dispose();
		}

		public M2MobGenerator assignAnimator(M2MobGAnimator Mga)
		{
			this.AMga.Add(Mga);
			return this;
		}

		public M2MobGenerator removeAnimator(M2MobGAnimator Mga)
		{
			this.AMga.Remove(Mga);
			this.need_refine_fin_atlas = true;
			return this;
		}

		public bool prepareAnimation(M2MobGAnimator Mga)
		{
			if (this.need_refine_fin_atlas)
			{
				this.ClearFinAtlas();
			}
			if (!Mga.need_draw)
			{
				return false;
			}
			int prepare_scale = Mga.prepare_scale;
			bool flag = false;
			if (!Mga.finatlas_created)
			{
				int num;
				RenderTexture renderTexture;
				Mga.FinAtlas = this.CalcFinAtlas.createRect(Mga.sklt_atlas_w * prepare_scale, Mga.sklt_atlas_h * prepare_scale, out num, out renderTexture, true);
				Mga.changed = true;
				Mga.need_fine_finalize_mesh = true;
				if (renderTexture != this.CalcFinAtlas.Tx)
				{
					flag = true;
					for (int i = this.AMga.Count - 1; i >= 0; i--)
					{
						this.AMga[i].need_fine_finalize_mesh = true;
					}
					this.MIFin.Tx = this.CalcFinAtlas.Tx;
				}
			}
			if (Mga.changed)
			{
				Mga.changed = false;
				RectInt finAtlas = Mga.FinAtlas;
				Material mtrSolidColor = BLIT.MtrSolidColor;
				mtrSolidColor.mainTexture = null;
				Graphics.SetRenderTarget(this.CalcFinAtlas.Tx);
				mtrSolidColor.SetPass(0);
				GL.PushMatrix();
				float num2 = 1f / (float)finAtlas.width;
				float num3 = 1f / (float)finAtlas.height;
				GL.LoadProjectionMatrix(Matrix4x4.Ortho(0f, (float)this.CalcFinAtlas.Tx.width, 0f, (float)this.CalcFinAtlas.Tx.height, -1f, 100f));
				GL.Begin(7);
				GL.Color(MTRX.ColTrnsp);
				GL.Vertex3((float)finAtlas.x, (float)finAtlas.y, 0f);
				GL.Vertex3((float)finAtlas.x, (float)finAtlas.yMax, 0f);
				GL.Vertex3((float)finAtlas.xMax, (float)finAtlas.yMax, 0f);
				GL.Vertex3((float)finAtlas.xMax, (float)finAtlas.y, 0f);
				GL.End();
				this.MyMd.clearSimple();
				this.MyMd.Identity();
				this.MyMd.Scale(64f * (float)prepare_scale, 64f * (float)prepare_scale, false).Translate((float)finAtlas.x + (float)finAtlas.width * 0.5f, (float)finAtlas.y + (float)finAtlas.height * 0.5f, false);
				Mga.draw(this.MyMd, base.GetAtlasTexture(), Mga.GetRenderTicket(), Mga.Sklt.Size.x, Mga.Sklt.Size.y);
				BLIT.RenderToGLImmediateSP(this.MyMd, this.MtrFinalize, -1);
				GL.PopMatrix();
				GL.Flush();
			}
			return flag;
		}

		public static bool instance_prepared
		{
			get
			{
				if (M2MobGenerator.Instance == null)
				{
					M2MobGenerator.Instance = new M2MobGenerator(M2DBase.Instance.curMap);
				}
				if (!M2MobGenerator.Instance.sklt_loaded)
				{
					if (!M2MobGenerator.Instance.getBaseCharacter().isLoadCompleted())
					{
						return false;
					}
					M2MobGenerator.Instance.prepareParts();
				}
				return true;
			}
		}

		public static void recreateMobgCharacter(Map2d Mp)
		{
			if (M2MobGenerator.Instance == null || !M2MobGenerator.Instance.sklt_loaded)
			{
				return;
			}
			M2MobGenerator.Instance.ClearTexture(false);
			List<M2MobGAnimator> amga = M2MobGenerator.Instance.AMga;
			for (int i = amga.Count - 1; i >= 0; i--)
			{
				amga[i].recreateSkltAtlas();
			}
		}

		public static void prepareAnimationWhole()
		{
			if (M2MobGenerator.Instance == null || !M2MobGenerator.Instance.sklt_loaded)
			{
				return;
			}
			List<M2MobGAnimator> amga = M2MobGenerator.Instance.AMga;
			int count = amga.Count;
			for (int i = 0; i < count; i++)
			{
				M2MobGenerator.Instance.prepareAnimation(amga[i]);
			}
		}

		public static void redrawMobgCharacter(Map2d Mp)
		{
			if (M2MobGenerator.Instance == null || !M2MobGenerator.Instance.sklt_loaded)
			{
				return;
			}
			List<M2MobGAnimator> amga = M2MobGenerator.Instance.AMga;
			for (int i = amga.Count - 1; i >= 0; i--)
			{
				amga[i].changed = true;
			}
		}

		public static bool ClearTextureS()
		{
			if (M2MobGenerator.Instance != null)
			{
				M2MobGenerator.Instance.ClearTextureOnFlush();
				return true;
			}
			return false;
		}

		public static void destructMOBG()
		{
			if (M2MobGenerator.Instance != null)
			{
				M2MobGenerator.Instance.destruct();
				M2MobGenerator.Instance = null;
			}
		}

		public static M2MobGenerator Instance;

		private Map2d Mp;

		private Material MtrFinalize;

		private RectAtlasTexture CalcFinAtlas;

		private RenderTexture TxAnimDrawn;

		private MeshDrawer MyMd;

		private readonly List<M2MobGAnimator> AMga;

		public bool need_refine_fin_atlas;

		public readonly MImage MIFin;
	}
}
