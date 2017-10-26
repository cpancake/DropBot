using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DropBot
{
	public partial class MainForm : Form
	{
		const int GAME_WIDTH = 765;
		const int GAME_HEIGHT = 503;
		const int INV_WIDTH = 192;
		const int INV_HEIGHT = 266;
		const int INV_X = 546;
		const int INV_Y = 202;
		const int INV_ITEM_WIDTH = 32;
		const int INV_ITEM_HEIGHT = 31;

		List<int> windowBoxIds = new List<int>();
		Image[,] inventoryItems = new Image[4, 7];
		ulong[,] inventoryHashes = new ulong[4, 7];

		public int WindowID
		{
			get { return windowBoxIds[windowBox.SelectedIndex]; }
		}

		public MainForm()
		{
			InitializeComponent();
			FindWindows();
		}

		private void FindWindows()
		{
			windowBox.Items.Clear();
			windowBoxIds.Clear();
			var processList = Process.GetProcesses();
			int possibleRunescape = -1;
			foreach(var process in processList)
			{
				if(!string.IsNullOrEmpty(process.MainWindowTitle))
				{
					var item = new ListViewItem();
					windowBox.Items.Add(string.Format("{0} [{1}]", process.MainWindowTitle, process.ProcessName));
					windowBoxIds.Add(process.Id);
					if(process.MainWindowTitle.ToLower().IndexOf("old school runescape") != -1)
					{
						possibleRunescape = windowBox.Items.Count - 1;
					}
				}
			}
			windowBox.SelectedIndex = (possibleRunescape == -1 ? 0 : possibleRunescape);
		}

		private Rectangle GetInventoryRect(int runescapeId)
		{
			var process = Process.GetProcessById(runescapeId);
			var windowRef = new HandleRef(this, process.MainWindowHandle);
			PInvoke.RECT boundsRect;
			if (!PInvoke.GetWindowRect(windowRef, out boundsRect))
			{
				throw new Exception("Failed to get bounds.");
			}

			Rectangle windowRect = new Rectangle(
				boundsRect.Left + 7,
				boundsRect.Top,
				boundsRect.Right - boundsRect.Left - 14,
				boundsRect.Bottom - boundsRect.Top + 1
			);

			// crop to game window
			windowRect.Y += 31 + INV_Y;
			windowRect.Height = INV_HEIGHT;
			windowRect.X = windowRect.X + (windowRect.Width / 2 - GAME_WIDTH / 2) + INV_X;
			windowRect.Width = INV_WIDTH;

			return windowRect;
		}

		private Image ScreenshotWindow(int runescapeId, Rectangle windowRect)
		{
			Process process = Process.GetProcessById(runescapeId);
			var screenCapture = new Bitmap(windowRect.Width, windowRect.Height);
			var graphics = Graphics.FromImage(screenCapture);
			PInvoke.FocusWindow(process.MainWindowHandle);
			graphics.CopyFromScreen(windowRect.Left, windowRect.Top, 0, 0, screenCapture.Size, CopyPixelOperation.SourceCopy);
			PInvoke.FocusWindow(Process.GetCurrentProcess().MainWindowHandle);

			return screenCapture;
		}

		private Point GetInventoryItemPosition(int i, int j)
		{
			return new Point((INV_ITEM_WIDTH + 10) * i + 17, (INV_ITEM_HEIGHT + 5) * j + 11);
		}

		private async Task DropItems(IEnumerable<Rectangle> items)
		{
			var random = new Random();
			var process = Process.GetProcessById(WindowID);
			foreach (var itemRect in items)
			{
				PInvoke.FocusWindow(process.MainWindowHandle);

				var realItemRect = new Rectangle(itemRect.X + 10, itemRect.Y + 10, 10, 10);
				var pointInItem = await Automaus.MoveToRectangle(realItemRect, 0.1f + 0.5f * (float)random.NextDouble());
				await Automaus.RightClick(pointInItem);
				var dropRect = new Rectangle(pointInItem.X - 5, pointInItem.Y + 36, 10, 5);
				var pointInDrop = await Automaus.MoveToRectangle(dropRect, 0.1f + 0.5f * (float)random.NextDouble());
				await Automaus.LeftClick(pointInDrop);
			}

			return;
		}

		private void refreshButton_Click(object sender, EventArgs e)
		{
			FindWindows();
		}

		private void findButton_Click(object sender, EventArgs e)
		{
			var windowRect = GetInventoryRect(WindowID);
			var screenImage = (Bitmap)ScreenshotWindow(WindowID, windowRect);

			inventoryView.Items.Clear();
			inventoryView.LargeImageList = new ImageList();
			inventoryView.LargeImageList.ImageSize = new Size(INV_ITEM_WIDTH, INV_ITEM_HEIGHT);

			var hashes = new HashSet<ulong>();
			
			for (var i = 0; i < 4; i++)
			{
				for(var j = 0; j < 7; j++)
				{
					inventoryItems[i, j] = screenImage.Clone(
						new Rectangle(GetInventoryItemPosition(i, j), new Size(INV_ITEM_WIDTH, INV_ITEM_HEIGHT)), 
						screenImage.PixelFormat
					);
					inventoryItems[i, j] = ImageProcessor.FilterBackground(inventoryItems[i, j]);
					inventoryItems[i, j].Save("test" + i + "." + j + ".png");
					inventoryHashes[i, j] = ImageProcessor.HashImage(inventoryItems[i, j]);

					if(inventoryHashes[i, j] == 0)
					{
						continue;
					}

					if (!hashes.Contains(inventoryHashes[i, j]))
					{
						inventoryView.LargeImageList.Images.Add(inventoryItems[i, j]);
						var item = new ListViewItem();
						item.ImageIndex = inventoryView.LargeImageList.Images.Count - 1;
						item.Tag = inventoryHashes[i, j];
						inventoryView.Items.Add(item);
						hashes.Add(inventoryHashes[i, j]);
					}
				}
			}
		}

		private void dropButton_Click(object sender, EventArgs e)
		{
			// first, find all the items we need to drop
			var hash = Convert.ToUInt64(inventoryView.SelectedItems[0].Tag);
			var rects = new List<Rectangle>();
			var invRect = GetInventoryRect(WindowID);

			for(var i = 0; i < 4; i++)
			{
				for(var j = 0; j < 7; j++)
				{
					if(inventoryHashes[i, j] == hash)
					{
						var pos = GetInventoryItemPosition(i, j);
						rects.Add(
							new Rectangle(
								new Point(invRect.X + pos.X, invRect.Y + pos.Y), 
								new Size(INV_ITEM_WIDTH, INV_ITEM_HEIGHT)
							));
					}
				}
			}

			DropItems(rects);
		}
	}
}
