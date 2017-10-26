using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DropBot
{
	public static class Automaus
	{
		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

		private const int MOUSEEVENTF_LEFTDOWN = 0x02;
		private const int MOUSEEVENTF_LEFTUP = 0x04;
		private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
		private const int MOUSEEVENTF_RIGHTUP = 0x10;

		public static async Task MoveTo(Point position, float duration)
		{
			int xChange = position.X - Cursor.Position.X;
			int yChange = position.Y - Cursor.Position.Y;

			// keep track of the exact current position - without rounding
			float currentX = Cursor.Position.X;
			float currentY = Cursor.Position.Y;
			float currentTime = 0f;
			bool finished = false;

			await Task.Run(() =>
			{
				while (!finished)
				{
					currentTime += 0.01f;
					if (currentTime > duration || (Math.Ceiling(currentX) == position.X && Math.Ceiling(currentY) == position.Y))
					{
						finished = true;
						Cursor.Position = new Point(position.X, position.Y);
						break;
					}
					currentX = Lerp(currentX, position.X, currentTime / duration);
					currentY = Lerp(currentY, position.Y, currentTime / duration);
					Cursor.Position = new Point((int)currentX, (int)currentY);
					Thread.Sleep(10);
				}
				return;
			});
		}

		public static async Task<Point> MoveToRectangle(Rectangle rect, float duration)
		{
			var rand = new Random();
			var point = new Point(
				rect.X + (int)(rand.NextDouble() * rect.Width), 
				rect.Y + (int)(rand.NextDouble() * rect.Height)
			);
			await MoveTo(point, duration);
			return point;
		}

		public static async Task LeftClick(Point point)
		{
			await Task.Run(() =>
			{
				mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)point.X, (uint)point.Y, 0, 0);
				Thread.Sleep(50 + new Random().Next(0, 100));
				mouse_event(MOUSEEVENTF_LEFTUP, (uint)point.X, (uint)point.Y, 0, 0);
				return;
			});
		}

		public static async Task<Point> LeftClickRectangle(Rectangle rect)
		{
			var rand = new Random();
			var point = new Point(
				rect.X + (int)(rand.NextDouble() * rect.Width),
				rect.Y + (int)(rand.NextDouble() * rect.Height)
			);
			await LeftClick(point);
			return point;
		}

		public static async Task RightClick(Point point)
		{
			await Task.Run(() =>
			{
				mouse_event(MOUSEEVENTF_RIGHTDOWN, (uint)point.X, (uint)point.Y, 0, 0);
				Thread.Sleep(50 + new Random().Next(0, 100));
				mouse_event(MOUSEEVENTF_RIGHTUP, (uint)point.X, (uint)point.Y, 0, 0);
				return;
			});
		}

		public static async Task<Point> RightClickRectangle(Rectangle rect)
		{
			var rand = new Random();
			var point = new Point(
				rect.X + (int)(rand.NextDouble() * rect.Width),
				rect.Y + (int)(rand.NextDouble() * rect.Height)
			);
			await RightClick(point);
			return point;
		}

		private static float Lerp(float a, float b, float t)
		{
			return a + ((b-a) * t * t * (3f - 2f * t));
			//return (1f - t) * a + t * b;
		}
	}
}
