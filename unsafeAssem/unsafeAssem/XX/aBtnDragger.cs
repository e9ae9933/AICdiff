using System;
using UnityEngine;

namespace XX
{
	public class aBtnDragger : aBtn
	{
		private void initDragger()
		{
			if (this.moveable_x < 0f)
			{
				this.moveable_x = this.w;
			}
			if (this.moveable_y < 0f)
			{
				this.moveable_y = this.h;
			}
		}

		public override ButtonSkin makeButtonSkin(string key)
		{
			this.initDragger();
			if (key != null && key == "colorchooser")
			{
				return this.Skin = new ButtonSkinDraggerColorChooser(this, this.w, this.h);
			}
			return base.makeButtonSkin(key);
		}

		public override void OnPointerUp(bool clicking)
		{
			base.OnPointerUp(clicking);
			this.runMouseDragging(true);
		}

		public override bool OnPointerDown()
		{
			if (base.OnPointerDown())
			{
				this.initDragger();
				this.MousePos0 = IN.getMousePos(null);
				if (this.fix_to_first_position && this.Skin is ButtonSkinDraggerColorChooser)
				{
					ButtonSkinDraggerColorChooser buttonSkinDraggerColorChooser = this.Skin as ButtonSkinDraggerColorChooser;
					Vector2 vector = buttonSkinDraggerColorChooser.getDrawPositionPixel() * 0.015625f;
					Vector3 vector2 = buttonSkinDraggerColorChooser.local2global(vector, false);
					if (!X.chkLEN(this.MousePos0.x, this.MousePos0.y, vector2.x, vector2.y, 0.1875f))
					{
						this.MousePos0 = vector2;
					}
				}
				this.MousePos0.z = 1f;
				this.FirstVal0 = this.getValueV(false);
				return true;
			}
			return false;
		}

		public Vector2 getValueV(bool is_01 = false)
		{
			this.initDragger();
			return new Vector2((is_01 && this.moveable_x > 0f) ? (this.val_x / this.moveable_x) : this.val_x, (is_01 && this.moveable_y > 0f) ? (this.val_y / this.moveable_y) : this.val_y);
		}

		public void setValue(Vector2 V, bool is_01 = false)
		{
			this.initDragger();
			this.val_x = ((this.moveable_x <= 0f) ? 0f : (is_01 ? (V.x * this.moveable_x) : V.x));
			this.val_y = ((this.moveable_y <= 0f) ? 0f : (is_01 ? (V.y * this.moveable_y) : V.y));
			base.fine_flag = true;
		}

		public aBtn addDraggingFn(FnBtnBindings Fn)
		{
			return base.addFn(ref this.AFnDragging, Fn);
		}

		public override void hide()
		{
			base.hide();
			this.runMouseDragging(true);
		}

		public override bool run(float fcnt)
		{
			if (base.run(fcnt))
			{
				if (base.isActive())
				{
					this.runMouseDragging(false);
				}
				return true;
			}
			return false;
		}

		public void runMouseDragging(bool quit)
		{
			if (this.MousePos0.z > 0f)
			{
				Vector2 vector = IN.getMousePos(null) - this.MousePos0;
				float num = this.val_x;
				float num2 = this.val_y;
				if (this.moveable_x > 0f && base.transform.localScale.x != 0f)
				{
					num = vector.x * 64f / base.transform.localScale.x + this.FirstVal0.x;
					if (this.drag_loop_x)
					{
						while (num < 0f)
						{
							num += this.moveable_x;
						}
						num %= this.moveable_x;
					}
					else
					{
						num = X.MMX(0f, num, this.moveable_x);
					}
				}
				if (this.moveable_y > 0f)
				{
					num2 = vector.y * 64f / base.transform.localScale.y + this.FirstVal0.y;
					if (this.drag_loop_y)
					{
						while (num2 < 0f)
						{
							num2 += this.moveable_y;
						}
						num2 %= this.moveable_y;
					}
					else
					{
						num2 = X.MMX(0f, num2, this.moveable_y);
					}
				}
				if (num != this.val_x || num2 != this.val_y)
				{
					float num3 = this.val_x;
					float num4 = this.val_y;
					this.val_x = num;
					this.val_y = num2;
					if (base.runFn(this.AFnDragging))
					{
						base.fine_flag = true;
					}
					else
					{
						this.val_x = num3;
						this.val_y = num4;
					}
				}
				if (quit)
				{
					this.MousePos0.z = 0f;
				}
			}
		}

		public float moveable_x = -1f;

		public float moveable_y = -1f;

		public bool fix_to_first_position = true;

		public Vector3 MousePos0;

		public Vector2 FirstVal0;

		private float val_x;

		private float val_y;

		public bool drag_loop_x;

		public bool drag_loop_y;

		private FnBtnBindings[] AFnDragging;
	}
}
