using System;
using UnityEngine;
using XX;

namespace nel
{
	public class NelBookDrawer : IRunAndDestroy, IValotileSetable
	{
		public NelBookDrawer(string _name, Texture Tx = null, Shader S = null)
		{
			this.name = _name;
			this.MtiBook = MTI.LoadContainer("MTI_book", this.name);
			Tx = Tx ?? this.MtiBook.Load<Texture2D>("book_aricle_dark");
			if (S == null)
			{
				S = this.MtiBook.LoadShader("GuineaLion/LightweightBook/Glow");
			}
			this.Mtr = MTRX.newMtr(S);
			this.Mtr.SetTexture("_MainTex", Tx);
			this.Mtr.SetInt("_Pages", 2);
			this.Mtr.SetInt("_Columns", 2);
			this.Mtr.SetInt("_Rows", 1);
			this.Mtr.SetFloat("_WaveHeight", 0.025f);
			this.Mtr.SetFloat("_WaveLength", 0.68f);
			this.Mtr.SetFloat("_WaveAntecipation", 0.3f);
			this.Mtr.renderQueue = 3000;
		}

		public bool use_valotile
		{
			get
			{
				return this.Valot != null && this.Valot.enabled;
			}
			set
			{
				if (value)
				{
					if (this.Valot == null)
					{
						this.prepareValot();
					}
				}
				else if (this.Valot == null)
				{
					return;
				}
				this.Valot.enabled = value;
			}
		}

		public void destruct()
		{
			try
			{
				if (this.Gob != null)
				{
					IN.DestroyOne(this.Gob);
				}
			}
			catch
			{
			}
			this.Gob = (this.GobBook = null);
			IN.remRunner(this);
			if (this.Mtr != null)
			{
				IN.DestroyOne(this.Mtr);
			}
			if (this.TxPaper != null)
			{
				BLIT.nDispose(this.TxPaper);
				this.TxPaper = null;
			}
			this.MtiBook = MTI.ReleaseContainer("MTI_book", this.name);
		}

		public RenderTexture fineContentTexture()
		{
			if (this.TxPaper == null)
			{
				this.TxPaper = new RenderTexture(128, 64, 0, RenderTextureFormat.ARGB32);
			}
			this.fine_page_content = false;
			BLIT.Clear(this.TxPaper, 0U, true);
			Graphics.SetRenderTarget(this.TxPaper);
			MTRX.MtrMeshNormal.SetPass(0);
			GL.PushMatrix();
			GL.LoadOrtho();
			float num = ((this.parapara_count > 0) ? this.MAXT_PAGE_PARAPARA : this.MAXT_PAGE);
			float num2 = X.ZLINE((this.t_page >= 0f) ? this.t_page : (-this.t_page), num);
			GL.Begin(7);
			float num3;
			if (this.pre_forward >= 0)
			{
				num3 = X.ZLINE(1f - num2 * 2.5f);
				if (num3 > 0f)
				{
					object obj = ((this.t_page < 0f) ? 0f : 0.5f);
					GL.Color(C32.MulA(4278190080U, num3));
					object obj2 = obj;
					GL.Vertex3(obj2 + 0f, 0.04f, 0f);
					GL.Vertex3(obj2 + 0f, 0.96f, 0f);
					GL.Vertex3(obj2 + 0.5f - 0f, 0.96f, 0f);
					GL.Vertex3(obj2 + 0.5f - 0f, 0.04f, 0f);
				}
			}
			num3 = X.ZLINE((num2 - 0.5f) * 2f);
			if (num3 > 0f)
			{
				GL.Color(C32.MulA(4278190080U, num3));
				float num4 = ((this.t_page >= 0f) ? 0f : 0.5f);
				GL.Vertex3(num4 + 0f, 0.04f, 0f);
				GL.Vertex3(num4 + 0f, 0.96f, 0f);
				GL.Vertex3(num4 + 0.5f - 0f, 0.96f, 0f);
				GL.Vertex3(num4 + 0.5f - 0f, 0.04f, 0f);
			}
			GL.End();
			GL.Flush();
			Graphics.SetRenderTarget(null);
			GL.PopMatrix();
			if (this.Mtr != null)
			{
				this.Mtr.SetTexture("_PaperContent", this.TxPaper);
			}
			return this.TxPaper;
		}

		public GameObject attachGob(GameObject GobBase, bool use_valotile = false)
		{
			if (this.Gob != null)
			{
				return this.Gob;
			}
			this.Gob = new GameObject(this.name);
			GameObject gameObject = this.MtiBook.Load<GameObject>("Book_HistoryOfArt");
			this.GobBook = Object.Instantiate<GameObject>(gameObject, Vector3.zero, Quaternion.identity);
			this.GobBook.transform.SetParent(this.Gob.transform, false);
			if (GobBase != null)
			{
				this.Gob.layer = GobBase.layer;
				this.Gob.transform.SetParent(GobBase.transform, false);
			}
			this.GobBook.transform.localScale = this.VBookScale_;
			this.GobBook.transform.localPosition = this.VBookPos_;
			this.GobBook.transform.localRotation = this.QBookAngle_;
			IN.setLayerWithChildren(this.GobBook.transform, this.Gob.layer);
			this.m_meshRenderer = this.GobBook.transform.Find("OpenBook_Skinned").gameObject.GetComponent<SkinnedMeshRenderer>();
			this.m_meshRenderer.sharedMaterial = this.Mtr;
			GameObject gameObject2 = this.GobBook.transform.Find("Armature").gameObject;
			gameObject2.SetActive(false);
			Transform transform = gameObject2.transform.Find("Book_Root");
			Transform transform2 = transform.Find("Book_Back");
			Transform transform3 = transform.Find("Book_Front");
			this.m_frontCover = transform3;
			this.m_backCover = transform2;
			this.m_boneRoot = gameObject2.transform;
			this.t_open = (this.t_page = 0f);
			this.fine_page_content = true;
			this.parapara_count = 6;
			this.pre_forward = -1;
			IN.addRunner(this);
			if (use_valotile)
			{
				this.prepareValot();
			}
			this.run(0f);
			return this.Gob;
		}

		public void prepareValot()
		{
			if (this.Gob == null || this.Valot != null)
			{
				return;
			}
			this.Valot = this.Gob.AddComponent<ValotileRendererSkinnedMesh>();
			this.Valot.InitSk(this.m_meshRenderer, true);
		}

		public void finePage()
		{
			if (this.GobBook == null)
			{
				return;
			}
			float num = ((this.parapara_count > 0) ? this.MAXT_PAGE_PARAPARA : this.MAXT_PAGE);
			float num2 = X.ZLINE((this.t_page >= 0f) ? this.t_page : (num + this.t_page), num) * 0.9999f;
			this.Mtr.SetFloat("_Progress", num2);
			this.fine_page = false;
		}

		private void fineBookOpen(float value)
		{
			if (this.GobBook == null)
			{
				return;
			}
			this.fine_book_open = false;
			float num = 1f - (X.ZSIN2(value, 0.4f) - X.ZCOS(value - 0.4f, 0.6f)) * 0.7f;
			float num2 = (-num + 1f) * 0.5f;
			float num3 = num * -90f;
			this.m_boneRoot.localPosition = this.VclosedOffset_ * (1f - Mathf.Abs(num)) * (1f - value);
			this.m_backCover.localEulerAngles = new Vector3(0f, 0f, num3 - 180f * value * num2);
			this.m_frontCover.localEulerAngles = new Vector3(0f, 0f, num3 + 180f * value * (1f - num2));
			this.Mtr.SetFloat("_Open", value * value);
		}

		public Vector3 VBookScale
		{
			get
			{
				return this.VBookScale_;
			}
			set
			{
				this.VBookScale_ = value;
				if (this.GobBook != null)
				{
					this.GobBook.transform.localScale = value;
				}
			}
		}

		public Vector3 VBookPos
		{
			get
			{
				return this.VBookPos_;
			}
			set
			{
				this.VBookPos_ = value;
				if (this.GobBook != null)
				{
					this.GobBook.transform.localPosition = value;
				}
			}
		}

		public Quaternion QBookAngle
		{
			get
			{
				return this.QBookAngle_;
			}
			set
			{
				this.QBookAngle_ = value;
				if (this.GobBook != null)
				{
					this.GobBook.transform.localRotation = value;
				}
			}
		}

		public Vector3 VclosedOffset
		{
			get
			{
				return this.VclosedOffset_;
			}
			set
			{
				this.VclosedOffset_ = value;
				if (this.GobBook != null)
				{
					this.fine_book_open = true;
				}
			}
		}

		public Color32 Col
		{
			get
			{
				return this.Mtr.color;
			}
			set
			{
				this.Mtr.color = value;
			}
		}

		public void activate()
		{
			if (this.t_open < 0f)
			{
				this.t_open = X.Mx(0f, this.MAXT_OPEN + this.t_open);
				this.fine_book_open = true;
				SND.Ui.play(this.sound_page_progress, false);
			}
		}

		public void deactivate()
		{
			if (this.t_open >= 0f)
			{
				this.t_open = X.Mn(-1f, -this.MAXT_OPEN + this.t_open);
				this.fine_book_open = true;
				if (this.parapara_count > 1)
				{
					this.parapara_count = 1;
				}
			}
		}

		public void progressPage(bool forward = true)
		{
			this.pre_forward = ((this.t_page >= 0f) ? 1 : 0);
			this.t_page = (float)(forward ? 0 : (-1));
			SND.Ui.play(this.sound_page_progress, false);
		}

		public void showImmediate()
		{
			if (this.t_open >= 0f)
			{
				if (this.t_open < this.MAXT_OPEN)
				{
					this.t_open = this.MAXT_OPEN;
					this.fine_book_open = true;
				}
			}
			else if (this.t_open > -this.MAXT_OPEN)
			{
				this.t_open = -this.MAXT_OPEN;
				this.fine_book_open = true;
			}
			if (this.parapara_count > 1)
			{
				this.parapara_count = 1;
				this.fine_page = (this.fine_page_content = true);
			}
			if (this.t_page >= 0f)
			{
				if (this.t_page < this.MAXT_PAGE)
				{
					this.t_page += this.MAXT_PAGE;
					this.fine_page = (this.fine_page_content = true);
					return;
				}
			}
			else if (this.t_page > -this.MAXT_PAGE)
			{
				this.t_page = -this.MAXT_PAGE;
				this.fine_page = (this.fine_page_content = true);
			}
		}

		public bool run(float fcnt)
		{
			if (this.t_open >= 0f)
			{
				if (this.t_open < this.MAXT_OPEN)
				{
					this.t_open += fcnt;
					this.fine_book_open = true;
				}
			}
			else if (this.t_open > -this.MAXT_OPEN)
			{
				this.t_open -= fcnt;
				this.fine_book_open = true;
			}
			float num = ((this.parapara_count > 0) ? this.MAXT_PAGE_PARAPARA : this.MAXT_PAGE);
			if (this.t_page >= 0f)
			{
				if (this.t_page < num)
				{
					this.t_page += fcnt;
					this.fine_page = (this.fine_page_content = true);
					if (this.t_page >= num && this.parapara_count > 0)
					{
						this.parapara_count--;
						if (this.parapara_count == 0)
						{
							this.t_page = X.Mx(this.MAXT_PAGE_PARAPARA, this.MAXT_PAGE);
						}
						else
						{
							this.progressPage(true);
						}
					}
				}
			}
			else if (this.t_page > -num)
			{
				this.t_page -= fcnt;
				this.fine_page = (this.fine_page_content = true);
			}
			if (this.fine_book_open)
			{
				this.fineBookOpen((this.t_open >= 0f) ? X.ZLINE(this.t_open, this.MAXT_OPEN) : X.ZLINE(this.MAXT_OPEN + this.t_open, this.MAXT_OPEN));
			}
			if (this.fine_page)
			{
				this.finePage();
			}
			if (this.fine_page_content)
			{
				this.fineContentTexture();
			}
			return true;
		}

		private MTI MtiBook;

		public readonly string name;

		private Material Mtr;

		private GameObject Gob;

		private GameObject GobBook;

		private SkinnedMeshRenderer m_meshRenderer;

		private Transform m_frontCover;

		private Transform m_backCover;

		private Transform m_boneRoot;

		private Vector3 VclosedOffset_ = Vector3.zero;

		private Vector3 VBookScale_ = new Vector3(16f, 16f, 8f);

		private Vector3 VBookPos_ = new Vector3(0f, -0.34f, 0f);

		private Quaternion QBookAngle_ = Quaternion.Euler(-21.67f, 173f, -1.87f);

		private const string Material_OpenAmmount = "_Open";

		private const string Material_Progress = "_Progress";

		private const float closing_book_center_pos = 1f;

		public uint base_col = 4278190080U;

		public uint line_col = uint.MaxValue;

		private bool fine_book_open;

		private bool fine_page;

		private bool fine_page_content;

		private bool fine_bg_aula;

		private int parapara_count = -1;

		private int pre_forward = -1;

		private float t_open;

		public float MAXT_OPEN = 30f;

		private float t_page;

		public float MAXT_PAGE_PARAPARA = 7f;

		public float MAXT_PAGE = 24f;

		private RenderTexture TxPaper;

		public string sound_open = "book_open";

		public string sound_page_progress = "book_next";

		private ValotileRendererSkinnedMesh Valot;
	}
}
