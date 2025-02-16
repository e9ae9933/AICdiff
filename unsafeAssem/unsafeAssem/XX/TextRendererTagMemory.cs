using System;
using System.Collections.Generic;

namespace XX
{
	public class TextRendererTagMemory
	{
		private static void Init()
		{
			if (TextRendererTagMemory.AItems == null)
			{
				TextRendererTagMemory.AItems = new List<TextRendererTagMemory>(1);
				TextRendererTagMemory.AAVarContentMem = new List<List<TextRendererHtmlTag.TagVarMem>>(6);
			}
		}

		public static void Stock(int cnt = 1)
		{
			TextRendererTagMemory.Init();
			while (--cnt >= 0)
			{
				TextRendererTagMemory.AItems.Add(new TextRendererTagMemory());
			}
		}

		public static TextRendererTagMemory Pop()
		{
			TextRendererTagMemory.Init();
			while (TextRendererTagMemory.AItems.Count <= TextRendererTagMemory.using_cnt)
			{
				TextRendererTagMemory.Stock(X.Mx(TextRendererTagMemory.using_cnt - TextRendererTagMemory.AItems.Count + 1, 1));
			}
			return TextRendererTagMemory.AItems[TextRendererTagMemory.using_cnt++];
		}

		public static TextRendererTagMemory Release(TextRendererTagMemory Target)
		{
			if (Target == null || TextRendererTagMemory.AItems == null)
			{
				return null;
			}
			Target.Dispose();
			int num = TextRendererTagMemory.AItems.IndexOf(Target);
			if (num >= 0 && num < TextRendererTagMemory.using_cnt)
			{
				TextRendererTagMemory.AItems.RemoveAt(num);
				TextRendererTagMemory.AItems.Insert(--TextRendererTagMemory.using_cnt, Target);
			}
			return null;
		}

		public static List<TextRendererHtmlTag.TagVarMem> PopVarMem()
		{
			TextRendererTagMemory.Init();
			while (TextRendererTagMemory.AAVarContentMem.Count <= TextRendererTagMemory.varconmem_cnt)
			{
				TextRendererTagMemory.AAVarContentMem.Add(new List<TextRendererHtmlTag.TagVarMem>());
			}
			List<TextRendererHtmlTag.TagVarMem> list = TextRendererTagMemory.AAVarContentMem[TextRendererTagMemory.varconmem_cnt++];
			list.Clear();
			return list;
		}

		public static void ReleaseVarMem(List<TextRendererHtmlTag.TagVarMem> Target)
		{
			if (Target == null || TextRendererTagMemory.AAVarContentMem == null)
			{
				return;
			}
			int num = TextRendererTagMemory.AAVarContentMem.IndexOf(Target);
			Target.Clear();
			if (num >= 0 && num < TextRendererTagMemory.varconmem_cnt)
			{
				TextRendererTagMemory.AAVarContentMem.RemoveAt(num);
				TextRendererTagMemory.AAVarContentMem.Insert(--TextRendererTagMemory.varconmem_cnt, Target);
			}
		}

		private TextRendererTagMemory()
		{
			this.ATags = new List<TextRendererHtmlTag>(2);
			this.ATagsRemoved = new List<TextRendererHtmlTag>(2);
		}

		public void Dispose()
		{
			for (int i = this.tag_max - 1; i >= 0; i--)
			{
				this.ATags[i].Dispose();
			}
			int count = this.ATagsRemoved.Count;
			for (int j = 0; j < count; j++)
			{
				TextRendererHtmlTag textRendererHtmlTag = this.ATagsRemoved[j];
				textRendererHtmlTag.Dispose();
				List<TextRendererHtmlTag> atags = this.ATags;
				int num = this.tag_max;
				this.tag_max = num + 1;
				atags.Insert(num, textRendererHtmlTag);
			}
			this.tag_max = 0;
			this.ATagsRemoved.Clear();
			this.CurTag = null;
		}

		public TextRendererHtmlTag MakeNewTag(STB Stb)
		{
			while (this.ATags.Count <= this.tag_max)
			{
				this.ATags.Add(new TextRendererHtmlTag());
			}
			List<TextRendererHtmlTag> atags = this.ATags;
			int num = this.tag_max;
			this.tag_max = num + 1;
			this.CurTag = atags[num];
			this.CurTag.InitTag(Stb);
			this.st = TextRendererHtmlTag.STATE.START;
			return this.CurTag;
		}

		public int TagCount
		{
			get
			{
				return this.tag_max;
			}
		}

		public TextRendererHtmlTag getTag(int i)
		{
			return this.ATags[i];
		}

		public void RemoveTagAt(int i)
		{
			if (i >= 0)
			{
				TextRendererHtmlTag textRendererHtmlTag = this.ATags[i];
				this.ATags.RemoveAt(i);
				this.tag_max--;
				this.ATagsRemoved.Add(textRendererHtmlTag);
			}
		}

		public void RemoveTag(TextRendererHtmlTag Tag)
		{
			this.RemoveTagAt(this.ATags.IndexOf(Tag));
		}

		public bool is_closer
		{
			get
			{
				return this.st == TextRendererHtmlTag.STATE.CLOSE || this.st == TextRendererHtmlTag.STATE.CLOSE_TAGNAME;
			}
		}

		private static List<TextRendererTagMemory> AItems;

		private static int using_cnt;

		private static List<List<TextRendererHtmlTag.TagVarMem>> AAVarContentMem;

		private static int varconmem_cnt;

		public TextRendererHtmlTag CurTag;

		private List<TextRendererHtmlTag> ATags;

		private List<TextRendererHtmlTag> ATagsRemoved;

		private int tag_max;

		public TextRendererHtmlTag.STATE st;
	}
}
