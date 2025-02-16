using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class CLICK
	{
		public CLICK(int alloc = 64)
		{
			this.AClk = new List<IClickable>(alloc);
		}

		public CLICK addClickable(IClickable Clk)
		{
			if (this.AClk.IndexOf(Clk) == -1)
			{
				this.AClk.Add(Clk);
				this.need_sort = (this.need_fine = true);
			}
			return this;
		}

		public void Blur(IClickable Clk, bool do_not_call_clicking = false)
		{
			if (Clk == this.Dragging)
			{
				try
				{
					this.quitDraggingManual(do_not_call_clicking);
				}
				catch
				{
				}
			}
			if (Clk != null && Clk == this.CurrentHover)
			{
				this.CurrentHover.OnPointerExit();
				this.CurrentHover = null;
				this.need_fine = true;
			}
		}

		public void Blur()
		{
			this.Blur(this.CurrentHover, false);
		}

		public int isin(IClickable Clk)
		{
			return this.AClk.IndexOf(Clk);
		}

		public bool remClickable(IClickable Clk, bool do_not_call_clicking = false)
		{
			int num = this.AClk.IndexOf(Clk);
			if (num > -1)
			{
				this.AClk.RemoveAt(num);
				this.Blur(Clk, do_not_call_clicking);
				if (this != IN.Click)
				{
					IN.Click.Blur(Clk, do_not_call_clicking);
				}
				return true;
			}
			return false;
		}

		public void run(Vector2 PosU)
		{
			if (this.Dragging != null)
			{
				IClickable currentHover;
				if (this.can_hover_other_object_on_dragging)
				{
					if (!this.need_fine)
					{
						currentHover = this.CurrentHover;
					}
					else
					{
						this.need_fine = false;
						this.checkAt(PosU, out currentHover);
					}
				}
				else
				{
					this.Dragging.getClickable(PosU, out currentHover);
				}
				if (currentHover != this.CurrentHover)
				{
					if (this.CurrentHover != null)
					{
						this.CurrentHover.OnPointerExit();
					}
					this.CurrentHover = currentHover;
					if (this.CurrentHover != null)
					{
						currentHover.OnPointerEnter();
					}
				}
				if (!IN.isMouseOn())
				{
					this.need_fine = true;
					this.quitDraggingManual(false);
					this.run(PosU);
					return;
				}
			}
			else
			{
				if (this.need_fine)
				{
					this.need_fine = false;
					IClickable clickable;
					this.checkAt(PosU, out clickable);
					if (clickable != this.CurrentHover)
					{
						if (this.CurrentHover != null)
						{
							this.CurrentHover.OnPointerExit();
						}
						this.CurrentHover = clickable;
						if (this.CurrentHover != null)
						{
							clickable.OnPointerEnter();
						}
					}
				}
				if (IN.isMousePushDown(1) && this.CurrentHover != null)
				{
					this.can_hover_other_object_on_dragging = false;
					if (this.CurrentHover.OnPointerDown())
					{
						if (this.Dragging == null)
						{
							this.initDragManual(this.CurrentHover);
							return;
						}
					}
					else
					{
						this.Dragging = null;
					}
				}
			}
		}

		public void initDragManual(IClickable _Dragging)
		{
			this.Dragging = _Dragging;
		}

		public void quitDraggingManual(bool do_not_call_clicking = false)
		{
			if (this.Dragging != null)
			{
				bool flag = this.Dragging == this.CurrentHover;
				IClickable dragging = this.Dragging;
				this.Dragging = null;
				if (!do_not_call_clicking)
				{
					dragging.OnPointerUp(flag);
				}
			}
		}

		public bool checkAt(Vector2 PosU, out IClickable Res)
		{
			if (this.need_sort)
			{
				this.need_sort = false;
				try
				{
					this.AClk.Sort(delegate(IClickable a, IClickable b)
					{
						float num2 = 1000f;
						float num3 = 1000f;
						try
						{
							num2 = a.getFarLength();
						}
						catch (Exception ex)
						{
							throw ex;
						}
						try
						{
							num3 = b.getFarLength();
						}
						catch (Exception ex2)
						{
							throw ex2;
						}
						if (num2 == num3)
						{
							return 0;
						}
						if (num2 - num3 >= 0f)
						{
							return -1;
						}
						return 1;
					});
				}
				catch
				{
					this.need_sort = (this.need_fine = true);
				}
			}
			uint num = this.mouse_checkng_frame_id + 1U;
			this.mouse_checkng_frame_id = num;
			if (num == 0U)
			{
				this.mouse_checkng_frame_id = 1U;
			}
			for (int i = this.AClk.Count - 1; i >= 0; i--)
			{
				if (this.AClk[i].getClickable(PosU, out Res))
				{
					return true;
				}
			}
			Res = null;
			return false;
		}

		public IClickable get_CurrentHover()
		{
			return this.CurrentHover;
		}

		public static bool getClickableRect(Vector2 PosU, Transform Trs, float wu, float hu)
		{
			wu *= 0.5f;
			hu *= 0.5f;
			Vector2 vector = Trs.TransformPoint(-wu, -hu, 0f);
			Vector2 vector2 = Trs.TransformPoint(-wu, hu, 0f);
			Vector2 vector3 = Trs.TransformPoint(wu, hu, 0f);
			Vector2 vector4 = Trs.TransformPoint(wu, -hu, 0f);
			float num = X.Mn(X.Mn(X.Mn(vector.x, vector2.x), vector3.x), vector4.x);
			float num2 = X.Mn(X.Mn(X.Mn(vector.y, vector2.y), vector3.y), vector4.y);
			float num3 = X.Mx(X.Mx(X.Mx(vector.x, vector2.x), vector3.x), vector4.x);
			float num4 = X.Mx(X.Mx(X.Mx(vector.y, vector2.y), vector3.y), vector4.y);
			return X.BTW(num, PosU.x, num3) && X.BTW(num2, PosU.y, num4);
		}

		public static bool getClickableRectSimple(Vector2 PosU, Transform Trs, float wu, float hu)
		{
			wu *= 0.5f;
			hu *= 0.5f;
			Vector3 position = Trs.position;
			return X.BTW(position.x - wu, PosU.x, position.x + wu) && X.BTW(position.y - hu, PosU.y, position.y + hu);
		}

		private readonly List<IClickable> AClk;

		public bool need_fine = true;

		public bool can_hover_other_object_on_dragging;

		private bool need_sort;

		private IClickable CurrentHover;

		private IClickable Dragging;

		public uint mouse_checkng_frame_id = 1U;
	}
}
