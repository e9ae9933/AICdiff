using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class PlayerCheckPoint
	{
		public PlayerCheckPoint()
		{
			this.ADeclineFootLp = new List<M2LabelPoint>(1);
			this.FD_drawCircle = new M2DrawBinder.FnEffectBind(this.drawCircle);
		}

		public void newGame()
		{
			this.LastCheckMp = (this.LastCheckTransfering = null);
			this.CurCheck = null;
			this.LastFootMp = null;
		}

		public void initS(Map2d _Mp)
		{
			if (_Mp == null)
			{
				return;
			}
			if (this.LastFootMp != null || this.CurCheck != null)
			{
				this.LastCheckMp = this.Mp;
				this.LastReturnPosPreMap = this.getPos(null);
			}
			this.flags = 0;
			this.ADeclineFootLp.Clear();
			this.Mp = _Mp;
			if (this.LastCheckTransfering != this.Mp)
			{
				this.LastCheckTransfering = null;
			}
			this.LastFootMp = null;
			this.CurCheck = null;
			this.AChecks = null;
			this.Ed = null;
			this.Col = MTRX.ColWhite;
			this.Col.a = 0;
			this.pixel_x = (this.pixel_y = 0);
		}

		public void appearPlayer(PR Pr)
		{
			if (!this.map_appeared)
			{
				this.map_appeared = true;
				if (this.LastCheckTransfering != null && Pr.Mp == this.LastCheckTransfering)
				{
					Pr.setTo(this.LastReturnPosPreMap.x, this.LastReturnPosPreMap.y - Pr.sizey);
					this.LastCheckTransfering = null;
				}
				this.LastPt.Set(Pr.x, Pr.mbottom);
			}
		}

		public void addDeclineArea(M2LabelPoint Lp)
		{
			this.ADeclineFootLp.Add(Lp);
		}

		public void remDeclineArea(M2LabelPoint Lp)
		{
			this.ADeclineFootLp.Remove(Lp);
		}

		public bool fineFoot(PR Pr, M2BlockColliderContainer.BCCLine Bcl, bool near_bench = false)
		{
			float num = (float)(this.Mp.crop + 1);
			if (Bcl == null || this.isSameFoot(Bcl) || Bcl.BCC != this.Mp.BCC || (!near_bench && (Bcl.is_lift || Bcl.calcUpperDanger(true))))
			{
				return false;
			}
			if (!X.isCovering(Bcl.x, Bcl.right, num, (float)this.Mp.clms - num, 0f))
			{
				return false;
			}
			if (!near_bench)
			{
				for (int i = this.ADeclineFootLp.Count - 1; i >= 0; i--)
				{
					if (this.ADeclineFootLp[i].isConveringMapXy(Bcl.x, Bcl.y, Bcl.right, Bcl.bottom))
					{
						return false;
					}
				}
			}
			this.LastFootMp = Bcl.BCC.Mp;
			this.last_foot_cx = X.NI(X.MMX(num, Bcl.x, (float)this.Mp.clms - num), X.MMX(num, Bcl.right, (float)this.Mp.clms - num), 0.5f);
			this.last_foot_cy = Bcl.slopeBottomY(this.last_foot_cx);
			if (this.CurCheck == null)
			{
				this.need_fine_pos = true;
			}
			return true;
		}

		private bool isSameFoot(M2BlockColliderContainer.BCCLine Bcl)
		{
			return this.LastFootMp == Bcl.BCC.Mp && Bcl.cx == this.last_foot_cx && Bcl.cy == this.last_foot_cy;
		}

		public void setCheckPointManual(ICheckPointListener _Ck)
		{
			if (_Ck == null)
			{
				if (this.CurCheck != null)
				{
					this.removeCheckPointManual(this.CurCheck);
				}
				return;
			}
			if (this.AChecks == null)
			{
				this.AChecks = new List<ICheckPointListener>(1);
				this.AChecks.Add(_Ck);
			}
			else
			{
				int num = this.AChecks.IndexOf(_Ck);
				if (num >= 0)
				{
					this.AChecks.RemoveAt(num);
				}
				this.AChecks.Add(_Ck);
			}
			this.Fine();
		}

		public void removeCheckPointManual(ICheckPointListener _Ck)
		{
			if (this.CurCheck == _Ck && this.AChecks != null)
			{
				this.AChecks.Remove(_Ck);
				this.Fine();
			}
		}

		private void Fine()
		{
			if (this.Ed == null)
			{
				this.Ed = this.Mp.setED("check_circle", this.FD_drawCircle, 0f);
			}
			bool flag = this.CurCheck == null && this.LastFootMp != null;
			if (this.AChecks != null)
			{
				if (this.AChecks.Count == 0)
				{
					this.AChecks = null;
					this.CurCheck = null;
					flag = true;
				}
				else
				{
					int num = 0;
					ICheckPointListener checkPointListener = null;
					for (int i = this.AChecks.Count - 1; i >= 0; i--)
					{
						int checkPointPriority = this.AChecks[i].getCheckPointPriority();
						if (num < checkPointPriority)
						{
							num = checkPointPriority;
							checkPointListener = this.AChecks[i];
						}
						else if (num == checkPointPriority)
						{
							this.AChecks.RemoveAt(i);
						}
					}
					if (checkPointListener != null)
					{
						if (checkPointListener != this.CurCheck)
						{
							this.CurCheck = checkPointListener;
							this.CurCheck.activateCheckPoint();
							flag = true;
						}
					}
					else
					{
						this.CurCheck = null;
						flag = true;
					}
				}
			}
			if (flag)
			{
				this.need_fine_pos = true;
				if (this.CurCheck != null)
				{
					this.Col = this.CurCheck.getDrawEffectPositionAndColor(ref this.pixel_x, ref this.pixel_y);
					return;
				}
				this.Col.a = 0;
			}
		}

		public static Color32 defaultAssignPositionAndColor(M2Chip Cp, ref int pixel_x, ref int pixel_y)
		{
			if (Cp == null)
			{
				return MTRX.ColTrnsp;
			}
			METACImg meta = Cp.getMeta();
			MTRX.colb.Set(X.NmUI(meta.GetS("check_point_effect_color"), 16777215U, true, true));
			if (MTRX.colb.a > 0)
			{
				pixel_x = meta.GetI("check_point_effect_pixel", Cp.iwidth / 2, 0);
				pixel_y = meta.GetI("check_point_effect_pixel", Cp.iheight / 2, 1);
			}
			return MTRX.colb.C;
		}

		public Vector3 getPos(PR Pr_send_return_to_checkpoint = null)
		{
			if (this.need_fine_pos || Pr_send_return_to_checkpoint != null)
			{
				this.need_fine_pos = false;
				if (this.CurCheck != null)
				{
					this.CurCheck.getReturnPos();
					this.LastPt = this.CurCheck.getReturnPos();
				}
				else if (this.LastFootMp != null)
				{
					this.LastPt.Set(this.last_foot_cx, this.last_foot_cy);
				}
				else if (Pr_send_return_to_checkpoint != null)
				{
					if (this.LastCheckMp == null)
					{
						SVD.sFile currentFile = COOK.getCurrentFile();
						this.LastCheckMp = this.Mp.M2D.Get(currentFile.last_map_key, false);
						if (currentFile.last_saved_x > 0f)
						{
							this.LastPt = new Vector2(currentFile.last_saved_x, currentFile.last_saved_y);
						}
						else
						{
							this.LastPt = new Vector2(Pr_send_return_to_checkpoint.x, Pr_send_return_to_checkpoint.y);
						}
						if (this.LastCheckMp != null && this.LastCheckMp != M2DBase.Instance.curMap)
						{
							currentFile.revert_pos = true;
						}
					}
					else
					{
						this.LastPt = this.LastReturnPosPreMap;
					}
					if (this.LastCheckMp != null && this.LastCheckMp != M2DBase.Instance.curMap)
					{
						this.Mp.M2D.initMapMaterialASync(this.LastCheckMp, 1, false);
						this.LastCheckTransfering = this.LastCheckMp;
						this.Mp.M2D.transferring_game_stopping = true;
					}
				}
				if (Pr_send_return_to_checkpoint != null)
				{
					if (this.CurCheck != null)
					{
						this.CurCheck.returnChcekPoint(Pr_send_return_to_checkpoint);
					}
					if (!this.Mp.M2D.transferring_game_stopping)
					{
						Pr_send_return_to_checkpoint.setTo(this.LastPt.x, this.LastPt.y - Pr_send_return_to_checkpoint.sizey);
					}
				}
				return new Vector3(this.LastPt.x, this.LastPt.y, (float)(this.Mp.M2D.transferring_game_stopping ? 0 : 1));
			}
			return this.LastPt;
		}

		private bool drawCircle(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.need_fine_pos)
			{
				this.getPos(null);
			}
			if (this.CurCheck == null || !this.CurCheck.drawCheckPoint(Ef, (float)this.pixel_x, (float)this.pixel_y, this.Col))
			{
				this.Ed = null;
				return false;
			}
			return true;
		}

		public static bool defaultDrawChipPC(EffectItem Ef, M2Chip _Cp, float px, float py, Color32 _Col)
		{
			if (_Cp == null)
			{
				return false;
			}
			Vector2 vector = _Cp.PixelToMapPoint(px, py);
			Ef.x = vector.x;
			Ef.y = vector.y;
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
			meshImg.Col = _Col;
			PlayerCheckPoint.DrawBubble(meshImg, 0f, 0f);
			return true;
		}

		public static void DrawBubble(MeshDrawer Md, float x, float y)
		{
			PxlSequence sqCheckPointBubble = MTR.SqCheckPointBubble;
			int num = sqCheckPointBubble.countFrames();
			PxlFrame frame = sqCheckPointBubble.getFrame(X.Abs(X.ANMT(num * 2 - 1, 8f) - (num - 1)));
			Md.initForImg(frame.getLayer(0).Img, 0);
			Md.RotaGraph(0f, 0f, 2f, 0f, null, false);
		}

		public bool map_appeared
		{
			get
			{
				return (this.flags & 1) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 1) : (this.flags & -2));
			}
		}

		public bool need_fine_pos
		{
			get
			{
				return (this.flags & 2) > 0;
			}
			set
			{
				this.flags = (value ? (this.flags | 2) : (this.flags & -3));
			}
		}

		public ICheckPointListener get_CurCheck()
		{
			return this.CurCheck;
		}

		public int getCurPriority()
		{
			if (this.CurCheck == null)
			{
				return 0;
			}
			return this.CurCheck.getCheckPointPriority();
		}

		private Vector2 LastPt;

		private Map2d LastFootMp;

		private float last_foot_cx;

		private float last_foot_cy;

		private List<ICheckPointListener> AChecks;

		private ICheckPointListener CurCheck;

		private Map2d LastCheckMp;

		private Map2d LastCheckTransfering;

		private Vector2 LastReturnPosPreMap;

		private List<M2LabelPoint> ADeclineFootLp;

		private int flags;

		private M2DrawBinder Ed;

		private Map2d Mp;

		private Color32 Col;

		private int pixel_x;

		private int pixel_y;

		public const int CHECK_POINT_STAND = 5;

		public const int CHECK_POINT_PUZZLE = 500;

		public const int CHECK_POINT_SUMMONER = 1000;

		private M2DrawBinder.FnEffectBind FD_drawCircle;
	}
}
