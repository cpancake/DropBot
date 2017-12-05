using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DropBot
{
	public partial class MainForm : Form
	{
		private const int GAME_WIDTH = 765;
		private const int GAME_HEIGHT = 503;
		private const int INV_WIDTH = 192;
		private const int INV_HEIGHT = 266;
		private const int INV_X = 546;
		private const int INV_Y = 202;
		private const int INV_ITEM_WIDTH = 42;
		private const int INV_ITEM_HEIGHT = 36;

		private List<int> _windowBoxIds = new List<int>();
		private Image[,] _inventoryItems = new Image[4, 7];
		private ulong[,] _inventoryHashes = new ulong[4, 7];
		private CancellationTokenSource _currentDropCancel = null;

		public int WindowID => _windowBoxIds[windowBox.SelectedIndex];

		public MainForm()
		{
			KeyboardIntercept.Init();
			KeyboardIntercept.AddHook(Keys.Escape, () =>
			{
				if(_currentDropCancel != null && 
					!_currentDropCancel.IsCancellationRequested)
					_currentDropCancel.Cancel();
			});

			InitializeComponent();
			FindWindows();
		}

		private void FindWindows()
		{
			windowBox.Items.Clear();
			_windowBoxIds.Clear();
			var processList = Process.GetProcesses();
			var possibleRunescape = -1;
			foreach(var process in processList)
			{
				if(!string.IsNullOrEmpty(process.MainWindowTitle))
				{
					var item = new ListViewItem();
					windowBox.Items.Add(string.Format("{0} [{1}]", process.MainWindowTitle, process.ProcessName));
					_windowBoxIds.Add(process.Id);
					if(process.MainWindowTitle.ToLower().IndexOf("old school runescape") != -1)
					{
						possibleRunescape = windowBox.Items.Count - 1;
					}
				}
			}
			windowBox.SelectedIndex = (possibleRunescape == -1 ? 0 : possibleRunescape);
		}

		private Rectangle GetWindowRect(int windowId)
		{
			var process = Process.GetProcessById(windowId);
			var windowRef = new HandleRef(this, process.MainWindowHandle);
			if(!PInvoke.GetClientRect(windowRef, out var boundsRect))
			{
				throw new Exception("Failed to get bounds.");
			}

			var point = new Point(0, 0);
			var startPos = PInvoke.ClientToScreen(windowRef, ref point);

			var windowRect = new Rectangle(
				point,
				new Size(boundsRect.Right - boundsRect.Left, boundsRect.Bottom - boundsRect.Top)
			);

			return windowRect;
		}

		private Rectangle GetInventoryRect(int runescapeId)
		{
			var inventoryRect = GetWindowRect(runescapeId);

			// crop to game window
			inventoryRect.Y += 31 + INV_Y;
			inventoryRect.Height = INV_HEIGHT;
			inventoryRect.X = inventoryRect.X + (inventoryRect.Width / 2 - GAME_WIDTH / 2) + INV_X;
			inventoryRect.Width = INV_WIDTH;

			return inventoryRect;
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
			return new Point(INV_ITEM_WIDTH * i, INV_ITEM_HEIGHT * j);
		}

		private async Task DropItems(IEnumerable<Rectangle> items, CancellationToken token)
		{
			var random = new Random();
			var process = Process.GetProcessById(WindowID);
			foreach(var itemRect in items)
			{
				PInvoke.FocusWindow(process.MainWindowHandle);

				var realItemRect = new Rectangle(itemRect.X + 10, itemRect.Y + 10, 10, 10);
				var pointInItem = await Automaus.MoveToRectangle(realItemRect, 0.1f + 0.5f * (float)random.NextDouble(), token);
				await Automaus.RightClick(pointInItem);
				token.ThrowIfCancellationRequested();

				// find drop rect by screenshotting and analyzing image
				var rect = GetWindowRect(WindowID);
				var windowImage = (Bitmap)ScreenshotWindow(WindowID, rect);
				windowImage.Save("window.png");
				var dropRect = ImageProcessor.FindMenuRect(windowImage, new Point(Cursor.Position.X - rect.X, Cursor.Position.Y - rect.Y));
				if(dropRect.IsEmpty)
				{
					MessageBox.Show("Couldn't find drop button. Aborted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				dropRect = new Rectangle(dropRect.X + rect.X + 1, dropRect.Y + rect.Y + 1, dropRect.Width - 2, dropRect.Height - 5);
				var pointInDrop = await Automaus.MoveToRectangle(dropRect, 0.1f + 0.5f * (float)random.NextDouble(), token);
				token.ThrowIfCancellationRequested();
				await Automaus.LeftClick(pointInDrop);
			}
		}

		private void RefreshButton_Click(object sender, EventArgs e)
		{
			FindWindows();
		}

		private void FindButton_Click(object sender, EventArgs e)
		{
			var windowRect = GetWindowRect(WindowID);
			var windowImage = (Bitmap)ScreenshotWindow(WindowID, windowRect);
			var invRect = ImageProcessor.FindInventoryRect(windowImage);
			if(invRect == Rectangle.Empty)
			{
				MessageBox.Show("Inventory isn't selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			var screenImage = ImageProcessor.FilterBackground(ImageProcessor.CropBitmap(windowImage, invRect));
#if DEBUG
			screenImage.Save("debug.png");
#endif

			inventoryView.Items.Clear();
			inventoryView.LargeImageList = new ImageList
			{
				ImageSize = new Size(INV_ITEM_WIDTH, INV_ITEM_HEIGHT)
			};

			var hashes = new HashSet<ulong>();

			for(var i = 0; i < 4; i++)
			{
				for(var j = 0; j < 7; j++)
				{
					_inventoryItems[i, j] = screenImage.Clone(
						new Rectangle(GetInventoryItemPosition(i, j), new Size(INV_ITEM_WIDTH, INV_ITEM_HEIGHT)),
						screenImage.PixelFormat
					);

#if DEBUG
					_inventoryItems[i, j].Save("debug." + i + "." + j + ".png");
#endif
					_inventoryHashes[i, j] = ImageProcessor.HashImage(_inventoryItems[i, j]);

					// if we're empty, don't bother
					if(_inventoryHashes[i, j] == 0)
					{
						continue;
					}

					// if we don't already have this item, add it to the listview
					if(!hashes.Contains(_inventoryHashes[i, j]))
					{
						inventoryView.LargeImageList.Images.Add(_inventoryItems[i, j]);
						var item = new ListViewItem
						{
							ImageIndex = inventoryView.LargeImageList.Images.Count - 1,
							Tag = _inventoryHashes[i, j]
						};
						inventoryView.Items.Add(item);
						hashes.Add(_inventoryHashes[i, j]);
					}
				}
			}
		}

		private void DropButton_Click(object sender, EventArgs e)
		{
			// first, find all the items we need to drop
			var hash = Convert.ToUInt64(inventoryView.SelectedItems[0].Tag);
			var rects = new List<Rectangle>();
			var windowRect = GetWindowRect(WindowID);
			var windowImage = (Bitmap)ScreenshotWindow(WindowID, windowRect);
			var invRect = ImageProcessor.FindInventoryRect(windowImage);
			if(invRect == Rectangle.Empty)
			{
				MessageBox.Show("Inventory isn't selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			for(var i = 0; i < 4; i++)
			{
				for(var j = 0; j < 7; j++)
				{
					if(_inventoryHashes[i, j] == hash)
					{
						var pos = GetInventoryItemPosition(i, j);
						rects.Add(
							new Rectangle(
								new Point(windowRect.X + invRect.X + pos.X, windowRect.Y + invRect.Y + pos.Y),
								new Size(INV_ITEM_WIDTH, INV_ITEM_HEIGHT)
							));
					}
				}
			}

			var cancelSource = new CancellationTokenSource();
			_currentDropCancel = cancelSource;
			DropItems(rects, cancelSource.Token);
		}
	}
}
