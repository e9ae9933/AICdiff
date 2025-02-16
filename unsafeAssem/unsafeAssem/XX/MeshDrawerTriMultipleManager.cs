using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class MeshDrawerTriMultipleManager
	{
		private MeshDrawerTriMultipleManager()
		{
			if (MeshDrawerTriMultipleManager.AtriDummy == null)
			{
				MeshDrawerTriMultipleManager.AtriDummy = new int[3];
			}
		}

		public MeshDrawerTriMultipleManager(MeshDrawer _Md, int first_i)
			: this()
		{
			this.Md = _Md;
			this.ATm = new List<MeshDrawerTriMultipleManager.TMItem>(first_i + 1);
			this.cur = first_i;
			this.tm_max = first_i + 1;
			while (--first_i >= 0)
			{
				this.ATm.Add(null);
			}
			this.ATm.Add(new MeshDrawerTriMultipleManager.TMItem(null, 0, null).Save(this.Md, false, false));
		}

		public MeshDrawerTriMultipleManager(MeshDrawer _Md, MeshDrawerTriMultipleManager CopyFrom)
			: this()
		{
			this.Md = _Md;
			this.ATm = new List<MeshDrawerTriMultipleManager.TMItem>(CopyFrom.ATm.Count);
			int num = CopyFrom.tm_max;
			for (int i = 0; i < num; i++)
			{
				MeshDrawerTriMultipleManager.TMItem tmitem = CopyFrom.ATm[i];
				this.ATm.Add((tmitem != null) ? new MeshDrawerTriMultipleManager.TMItem(CopyFrom.ATm[i]) : null);
			}
			this.cur = CopyFrom.cur;
			while (this.ATm.Count <= this.cur)
			{
				this.ATm.Add(null);
			}
			if (this.ATm[this.cur] == null)
			{
				this.ATm[this.cur] = new MeshDrawerTriMultipleManager.TMItem(null, 0, null).Save(this.Md, false, false);
			}
		}

		public MeshDrawerTriMultipleManager MergeSubMeshAll(MeshDrawerTriMultipleManager Src)
		{
			return this;
		}

		public void resetVertexCount()
		{
			for (int i = 0; i < this.tm_max; i++)
			{
				MeshDrawerTriMultipleManager.TMItem tmitem = this.ATm[i];
				if (tmitem != null)
				{
					tmitem.tri_max = 0;
				}
			}
		}

		public MeshDrawerTriMultipleManager clear(bool destruct_material = false)
		{
			if (this.Md != null)
			{
				this.Md.chooseSubMesh(0, false, false);
			}
			this.cur = 0;
			for (int i = 0; i < this.tm_max; i++)
			{
				MeshDrawerTriMultipleManager.TMItem tmitem = this.ATm[i];
				if (tmitem != null)
				{
					tmitem.secure_tri_ver_length = false;
					if (destruct_material && i > 0)
					{
						tmitem.destruct();
						this.ATm[i] = null;
					}
					else if (tmitem.tri_max > 0)
					{
						tmitem.tri_max = 0;
						if (tmitem.Atri == null)
						{
							tmitem.Atri = new int[6];
						}
					}
				}
			}
			this.tm_max = 1;
			return this;
		}

		public void destruct()
		{
			for (int i = 0; i < this.ATm.Count; i++)
			{
				MeshDrawerTriMultipleManager.TMItem tmitem = this.ATm[i];
				if (tmitem != null)
				{
					tmitem.destruct();
				}
			}
		}

		public void copyMaterialOnlyMainTex(Material Mtr)
		{
			for (int i = 0; i < this.tm_max; i++)
			{
				MeshDrawerTriMultipleManager.TMItem tmitem = this.ATm[i];
				if (tmitem != null)
				{
					Material material = MTRX.newMtr(Mtr);
					material.SetTexture("_MainTex", tmitem.Mtr.GetTexture("_MainTex"));
					tmitem.Mtr = material;
				}
			}
		}

		public MeshDrawerTriMultipleManager Save()
		{
			if (this.cur < 0)
			{
				return this;
			}
			MeshDrawerTriMultipleManager.TMItem tmitem = this.ATm[this.cur];
			tmitem.Save(this.Md, false, false);
			if (tmitem != null && !tmitem.secure_tri_ver_length && tmitem.Ma != null)
			{
				tmitem.Ma.Set(false);
			}
			return this;
		}

		public MeshDrawerTriMultipleManager.TMItem Add(int i, MeshDrawerTriMultipleManager.TMItem Src, int tri_add = 0)
		{
			while (this.ATm.Count <= i)
			{
				this.ATm.Add(null);
			}
			MeshDrawerTriMultipleManager.TMItem tmitem = this.ATm[i];
			int j = 0;
			if (tmitem == null)
			{
				tmitem = (this.ATm[i] = new MeshDrawerTriMultipleManager.TMItem(Src));
			}
			else
			{
				j = tmitem.tri_max;
				X.pushA<int>(ref tmitem.Atri, Src.Atri, tmitem.tri_max, Src.tri_max);
				tmitem.tri_max += Src.tri_max;
			}
			if (tri_add != 0)
			{
				while (j < tmitem.tri_max)
				{
					tmitem.Atri[j] += tri_add;
					j++;
				}
			}
			return tmitem;
		}

		public MeshDrawerTriMultipleManager.TMItem Choose(int i, bool clearing)
		{
			MeshDrawerTriMultipleManager.TMItem tmitem;
			if (i == this.cur)
			{
				tmitem = this.ATm[i];
			}
			else
			{
				if (this.ATm.Capacity <= i)
				{
					this.ATm.Capacity = i + 1;
				}
				while (this.ATm.Count <= i)
				{
					this.ATm.Add(null);
				}
				tmitem = this.ATm[i];
				if (tmitem == null)
				{
					tmitem = (this.ATm[i] = new MeshDrawerTriMultipleManager.TMItem(null, 0, this.Md.getMaterial()));
					tmitem.Save(this.Md, true, true);
				}
				this.cur = i;
			}
			this.tm_max = X.Mx(this.cur + 1, this.tm_max);
			return tmitem;
		}

		public void setMaterial(Material Mtr)
		{
			this.ATm[this.cur].Mtr = Mtr;
		}

		public int get_cur()
		{
			return this.cur;
		}

		public Material[] getMaterialArray(bool force_clone = false)
		{
			int num = 0;
			for (int i = 0; i < this.tm_max; i++)
			{
				if (MeshDrawerTriMultipleManager.TMItem.isActive(this.ATm[i], this.AssignedMeshRenderer))
				{
					num++;
				}
			}
			if (this.AMtr == null || force_clone)
			{
				this.AMtr = new Material[num];
			}
			else if (this.AMtr.Length != num)
			{
				Array.Resize<Material>(ref this.AMtr, num);
			}
			num = 0;
			for (int j = 0; j < this.tm_max; j++)
			{
				if (MeshDrawerTriMultipleManager.TMItem.isActive(this.ATm[j], this.AssignedMeshRenderer))
				{
					if (j == this.cur)
					{
						this.ATm[this.cur].Mtr = this.Md.getMaterial();
					}
					this.AMtr[num++] = this.ATm[j].Mtr;
				}
			}
			return this.AMtr;
		}

		public void arraySubtraction(Mesh Ms, int[] Atriangle_cache)
		{
			this.Save();
			if (this.Md.draw_gl_only)
			{
				return;
			}
			int num = 0;
			uint num2 = 0U;
			for (int i = 0; i < this.tm_max; i++)
			{
				if (MeshDrawerTriMultipleManager.TMItem.isActive(this.ATm[i], this.AssignedMeshRenderer))
				{
					num++;
					num2 |= 1U << i;
				}
			}
			if (Ms != null)
			{
				Ms.subMeshCount = num;
			}
			Material[] array = null;
			if (this.pre_meshrenderer_mtr_use != num2 && this.AssignedMeshRenderer != null)
			{
				this.pre_meshrenderer_mtr_use = num2;
				if (this.AMtr == null)
				{
					this.AMtr = new Material[num];
				}
				else if (this.AMtr.Length != num)
				{
					Array.Resize<Material>(ref this.AMtr, num);
				}
				array = this.AMtr;
			}
			num = 0;
			for (int j = 0; j < this.tm_max; j++)
			{
				MeshDrawerTriMultipleManager.TMItem tmitem = this.ATm[j];
				if (MeshDrawerTriMultipleManager.TMItem.isActive(tmitem, this.AssignedMeshRenderer))
				{
					if (tmitem.tri_max > 0 && tmitem.Atri.Length != tmitem.tri_max)
					{
						Array.Resize<int>(ref tmitem.Atri, tmitem.tri_max);
					}
					if (array != null)
					{
						array[num] = tmitem.Mtr;
					}
					if (Ms != null)
					{
						Ms.SetTriangles((tmitem.tri_max > 0) ? tmitem.Atri : MeshDrawerTriMultipleManager.AtriDummy, num++);
					}
				}
			}
			if (array != null)
			{
				this.AssignedMeshRenderer.sharedMaterials = array;
			}
		}

		public Material[] getAttachedMaterialArray()
		{
			return this.AMtr;
		}

		public void connectRendererToTriMulti(MeshRenderer Mrd)
		{
			this.AssignedMeshRenderer = Mrd;
			this.pre_meshrenderer_mtr_use = 0U;
		}

		public int Count
		{
			get
			{
				return this.ATm.Count;
			}
		}

		public int use_count
		{
			get
			{
				return this.tm_max;
			}
		}

		public bool isMaterialActive(int i)
		{
			return MeshDrawerTriMultipleManager.TMItem.isActive(this.Get(i), this.AssignedMeshRenderer);
		}

		public MeshDrawerTriMultipleManager.TMItem getCurrent()
		{
			return this.ATm[this.cur];
		}

		public MeshDrawerTriMultipleManager.TMItem Get(int i)
		{
			if (i < this.ATm.Count)
			{
				return this.ATm[i];
			}
			return null;
		}

		public int getCurrentTriangleTotal()
		{
			int num = 0;
			for (int i = 0; i < this.cur; i++)
			{
				MeshDrawerTriMultipleManager.TMItem tmitem = this.ATm[i];
				if (tmitem != null)
				{
					num += tmitem.tri_max;
				}
			}
			return num;
		}

		public readonly MeshDrawer Md;

		private List<MeshDrawerTriMultipleManager.TMItem> ATm;

		private int tm_max;

		private int cur;

		private Material[] AMtr;

		private static int[] AtriDummy;

		private MeshRenderer AssignedMeshRenderer;

		private uint pre_meshrenderer_mtr_use;

		public class TMItem
		{
			public TMItem(int[] _Atri, int _tri_max, Material _Mtr)
			{
				this.Atri = ((_Atri == null) ? new int[6] : _Atri);
				this.tri_max = _tri_max;
				this.Mtr = _Mtr;
			}

			public TMItem(MeshDrawerTriMultipleManager.TMItem Tmi)
			{
				this.Atri = X.concat<int>(Tmi.Atri, null, -1, -1);
				this.tri_max = Tmi.tri_max;
				this.base_z = Tmi.base_z;
				this.Mtr = Tmi.Mtr;
				this.render_queue0 = Tmi.render_queue0;
				this.render_queue = Tmi.render_queue;
				this.gradation = Tmi.gradation;
				this.mtr_cloned = Tmi.mtr_cloned;
			}

			public void destruct()
			{
				if (this.Mtr != null && this.mtr_cloned)
				{
					IN.DestroyOne(this.Mtr);
				}
			}

			public MeshDrawerTriMultipleManager.TMItem clearIndex()
			{
				this.tri_max = 0;
				return this;
			}

			public MeshDrawerTriMultipleManager.TMItem Save(MeshDrawer Md, bool replace_tri_array = false, bool first = false)
			{
				Md.writeToTriMultiItem(this, replace_tri_array);
				if (first)
				{
					this.mtr_cloned = false;
				}
				return this;
			}

			public static bool isActive(MeshDrawerTriMultipleManager.TMItem I, MeshRenderer AssignedMeshRenderer)
			{
				return I != null && (I.tri_max > 0 || AssignedMeshRenderer == null);
			}

			public int[] Atri;

			public int tri_max;

			public float base_z;

			public Material Mtr;

			public int render_queue0 = 3000;

			public int render_queue = 3000;

			public bool gradation;

			public bool mtr_cloned;

			public bool secure_tri_ver_length;

			public MdArranger Ma;
		}
	}
}
