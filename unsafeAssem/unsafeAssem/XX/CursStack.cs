using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class CursStack
	{
		public CursStack(CursKind cKind, CursCategory CC)
		{
			this.curs_key = cKind.key;
			this.CK = cKind;
			this.Categ = CC;
		}

		public int priority
		{
			get
			{
				return this.Categ.priority;
			}
		}

		public string categ_key
		{
			get
			{
				return this.Categ.key;
			}
		}

		public PxlMeshDrawer PMesh
		{
			get
			{
				return this.CK.PF.getDrawnMeshGenerator(false, 0f, 0f, false);
			}
		}

		public Transform MvTrs
		{
			get
			{
				return this.MvTrs;
			}
		}

		public int isAnimatingByAnother()
		{
			return this.Categ.isAnimatingByAnother();
		}

		public string curs_key;

		public readonly CursKind CK;

		public readonly CursCategory Categ;
	}
}
