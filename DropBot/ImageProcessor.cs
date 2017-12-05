using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DropBot
{
	public static class ImageProcessor
	{
		/// <summary>
		/// Removes the background colors from the input image.
		/// </summary>
		public static Bitmap FilterBackground(Bitmap bmp)
		{
			var reference = Properties.Resources.blank_inventory;
			if(bmp.Width != reference.Width || bmp.Height != reference.Height)
				throw new Exception("Invalid inventory size!");
			
			for(var i = 0; i < bmp.Width; i++)
			{
				for(var j = 0; j < bmp.Height; j++)
				{
					var color = bmp.GetPixel(i, j);
					if(IsSameColor(color, reference.GetPixel(i, j)))
					{
						bmp.SetPixel(i, j, Color.Transparent);
					}
				}
			}

			return bmp;
		}

		/// <summary>
		/// Computers the aHash of an image.
		/// </summary>
		public static ulong HashImage(Image input)
		{
			var scaled = new Bitmap(8, 8);
			var graphics = Graphics.FromImage(scaled);
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.DrawImage(input, 0, 0, scaled.Width, scaled.Height);

			var totalColor = 0;
			for(var i = 0; i < scaled.Width; i++)
			{
				for(var j = 0; j < scaled.Height; j++)
				{
					var px = scaled.GetPixel(i, j);
					var avg = (px.R + px.G + px.B) / 3;
					totalColor += avg;
					scaled.SetPixel(i, j, Color.FromArgb(avg, avg, avg));
				}
			}

			var binStr = "";
			var averageColor = totalColor / (scaled.Width * scaled.Height);
			for(var i = 0; i < scaled.Width; i++)
			{
				for(var j = 0; j < scaled.Height; j++)
				{
					var c = scaled.GetPixel(i, j);
					binStr += (c.R > averageColor) ? "1" : "0";
				}
			}

			return Convert.ToUInt64(binStr, 2);
		}

		/// <summary>
		/// Crops a bitmap.
		/// </summary>
		public static Bitmap CropBitmap(Bitmap b, Rectangle cropRect)
		{
			var bmp2 = new Bitmap(cropRect.Width, cropRect.Height);
			var g = Graphics.FromImage(bmp2);
			g.DrawImage(b, new Rectangle(0, 0, cropRect.Width, cropRect.Height), cropRect, GraphicsUnit.Pixel);
			return bmp2;
		}

		/// <summary>
		/// Analyzes an image to find the rect containing the inventory.
		/// </summary>
		public static Rectangle FindInventoryRect(Bitmap inputImage)
		{
			var gameWindowRect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);
			if(IsBlack(inputImage.GetPixel(0, 0)))
				gameWindowRect = FindGameWindowRect(inputImage);

			var invOffset = gameWindowRect.X == 0 ? 0 : -2;
			var invBox = new Rectangle(
				new Point(gameWindowRect.X + gameWindowRect.Width - 235 + invOffset, gameWindowRect.Height - 334),
				new Size(230, 334)
			);

			var invSelectColor = inputImage.GetPixel(invBox.X + invBox.Width / 2, invBox.Y + 3);
			var redDiff = (invSelectColor.R - invSelectColor.G) + (invSelectColor.R - invSelectColor.B) / 2f;

			// inventory not selected
			if(redDiff < 50)
				return Rectangle.Empty;

			var reference = Properties.Resources.blank_inventory;
			var invInterior = new Rectangle(
				new Point(invBox.X + 31, invBox.Y + 45),
				new Size(reference.Width, reference.Height)
			);

			return invInterior;
		}

		/// <summary>
		/// Finds the rectangle that includes the right click menu at or around the given point.
		/// </summary>
		public static Rectangle FindMenuRect(Bitmap inputImage, Point mousePoint)
		{
			var point = FindImageInRect(
				inputImage,
				Properties.Resources.drop_reference,
				new Rectangle(mousePoint.X - 100, mousePoint.Y - 10, 100, 100));

			if(point.IsEmpty)
				return Rectangle.Empty;

			var buttonRect = new Rectangle(point, Properties.Resources.drop_reference.Size);

			// find end of menu item
			var borderColor = Color.FromArgb(93, 84, 71);
			var currentX = buttonRect.X;
			var y = buttonRect.Y + buttonRect.Height / 2;
			while(
				!(IsBlack(inputImage.GetPixel(currentX, y)) &&
				IsSameColor(inputImage.GetPixel(currentX - 1, y), borderColor)))
				currentX++;

			if(currentX == inputImage.Width)
				return Rectangle.Empty;

			// find last filled position
			while(!IsBlack(inputImage.GetPixel(currentX - 1, y)))
				currentX--;

			var menuItemRect = new Rectangle(point, new Size(currentX - buttonRect.X, buttonRect.Height));
			CropBitmap(inputImage, menuItemRect).Save("test.png");

			return menuItemRect;
		}

		private static Point FindImageInRect(Bitmap sourceImage, Bitmap targetImage, Rectangle rect)
		{
			for(var i = rect.X; i < rect.X + rect.Width; i++)
			{
				for(var j = rect.Y; j < rect.Y + rect.Height; j++)
				{
					if(IsImageAtPoint(sourceImage, targetImage, new Point(i, j)))
						return new Point(i, j);
				}
			}

			return Point.Empty;
		}

		private static bool IsImageAtPoint(Bitmap sourceImage, Bitmap targetImage, Point point)
		{
			for(var i = 0; i < targetImage.Width; i++)
			{
				for(var j = 0; j < targetImage.Height; j++)
				{
					if(!IsSameColor(sourceImage.GetPixel(point.X + i, point.Y + j), targetImage.GetPixel(i, j)))
						return false;
				}
			}

			return true;
		}

		private static Rectangle FindGameWindowRect(Bitmap inputImage)
		{
			// find first x position where the color isn't black
			var leftX = 0;
			while(IsBlack(inputImage.GetPixel(leftX, 0)))
				leftX++;

			// find last x position where the color isn't black
			var rightX = inputImage.Width - 1;
			while(IsBlack(inputImage.GetPixel(rightX, 0)))
				rightX--;

			// find last Y position where the color isn't black
			var bottomY = inputImage.Height - 1;
			while(IsBlack(inputImage.GetPixel(inputImage.Width / 2, bottomY)))
				bottomY--;

			return new Rectangle(new Point(leftX, 0), new Size(inputImage.Width - leftX - (inputImage.Width - rightX), bottomY));
		}

		private static bool IsBlack(Color c) => c.R == 0 && c.G == 0 && c.B == 0;
		private static bool IsSameColor(Color a, Color b) => a.R == b.R && a.G == b.G && a.B == b.B;
	}
}
