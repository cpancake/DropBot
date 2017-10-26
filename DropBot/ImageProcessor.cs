using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DropBot
{
	public static class ImageProcessor
	{
		private static HashSet<Color> backgroundColors = new HashSet<Color>
		{
			Color.FromArgb(unchecked((int)0xFF50483E)),
			Color.FromArgb(unchecked((int)0xFF4B423A)),
			Color.FromArgb(unchecked((int)0xFF494035)),
			Color.FromArgb(unchecked((int)0xFF4B423A)),
			Color.FromArgb(unchecked((int)0xFF453C33)),
			Color.FromArgb(unchecked((int)0xFF464035)),
			Color.FromArgb(unchecked((int)0xFF483E35)),
			Color.FromArgb(unchecked((int)0xFF433A2E)),
			Color.FromArgb(unchecked((int)0xFF41362D)),
			Color.FromArgb(unchecked((int)0xFF3B342B)),
			Color.FromArgb(unchecked((int)0xFF50463D)),
			Color.FromArgb(unchecked((int)0xFF514941)),
			Color.FromArgb(unchecked((int)0xFF483D2E)),
			Color.FromArgb(unchecked((int)0xFF464038)),
			Color.FromArgb(unchecked((int)0xFF493E38)),
			Color.FromArgb(unchecked((int)0xFF463E38)),
			Color.FromArgb(unchecked((int)0xFF4D483B)),
			Color.FromArgb(unchecked((int)0xFF48402F)),
			Color.FromArgb(unchecked((int)0xFF4E4840)),
			Color.FromArgb(unchecked((int)0xFF4E4640)),
			Color.FromArgb(unchecked((int)0xFF504740)),
			Color.FromArgb(unchecked((int)0xFF413630)),
			Color.FromArgb(unchecked((int)0xFF3E382D)),
			Color.FromArgb(unchecked((int)0xFF504536)),
			Color.FromArgb(unchecked((int)0xFF383838)),
			Color.FromArgb(unchecked((int)0xFF3E3830)),
			Color.FromArgb(unchecked((int)0xFF333333))
		};

		public static Image FilterBackground(Image input)
		{
			var bmp = (Bitmap)input;
			for(var i = 0; i < bmp.Width; i++)
			{
				for(var j = 0; j < bmp.Height; j++)
				{
					var color = bmp.GetPixel(i, j);
					if (backgroundColors.Contains(color))
					{
						bmp.SetPixel(i, j, Color.Transparent);
					}
				}
			}
			return bmp;
		}

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

			string binStr = "";
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

		public static bool IsImageEmpty(Bitmap image)
		{
			bool foundPixel = false;
			for(var i = 0; i < image.Width; i++)
			{
				for(var j = 0; j < image.Height; j++)
				{
					var c = image.GetPixel(i, j);
					if(c.A != 255)
					{
						foundPixel = true;
						break;
					}
				}

				if (foundPixel) break;
			}

			return !foundPixel;
		}
	}
}
