namespace DropBot
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.windowBox = new System.Windows.Forms.ComboBox();
			this.refreshButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.findButton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.inventoryView = new System.Windows.Forms.ListView();
			this.label4 = new System.Windows.Forms.Label();
			this.dropButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(106, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Step 1: Find Window";
			// 
			// windowBox
			// 
			this.windowBox.FormattingEnabled = true;
			this.windowBox.Location = new System.Drawing.Point(12, 25);
			this.windowBox.Name = "windowBox";
			this.windowBox.Size = new System.Drawing.Size(245, 21);
			this.windowBox.TabIndex = 1;
			// 
			// refreshButton
			// 
			this.refreshButton.Location = new System.Drawing.Point(182, 52);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = new System.Drawing.Size(75, 21);
			this.refreshButton.TabIndex = 2;
			this.refreshButton.Text = "Refresh";
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 74);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(139, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Step 2: Find Inventory Items";
			// 
			// findButton
			// 
			this.findButton.Location = new System.Drawing.Point(12, 90);
			this.findButton.Name = "findButton";
			this.findButton.Size = new System.Drawing.Size(242, 23);
			this.findButton.TabIndex = 4;
			this.findButton.Text = "Find";
			this.findButton.UseVisualStyleBackColor = true;
			this.findButton.Click += new System.EventHandler(this.findButton_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 116);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(135, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Step 3: Select Item to Drop";
			// 
			// inventoryView
			// 
			this.inventoryView.GridLines = true;
			this.inventoryView.Location = new System.Drawing.Point(12, 132);
			this.inventoryView.MultiSelect = false;
			this.inventoryView.Name = "inventoryView";
			this.inventoryView.Size = new System.Drawing.Size(242, 102);
			this.inventoryView.TabIndex = 6;
			this.inventoryView.UseCompatibleStateImageBehavior = false;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(9, 237);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(70, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Step 4: Drop!";
			// 
			// dropButton
			// 
			this.dropButton.Location = new System.Drawing.Point(12, 253);
			this.dropButton.Name = "dropButton";
			this.dropButton.Size = new System.Drawing.Size(242, 23);
			this.dropButton.TabIndex = 8;
			this.dropButton.Text = "Drop";
			this.dropButton.UseVisualStyleBackColor = true;
			this.dropButton.Click += new System.EventHandler(this.dropButton_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(269, 289);
			this.Controls.Add(this.dropButton);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.inventoryView);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.findButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.refreshButton);
			this.Controls.Add(this.windowBox);
			this.Controls.Add(this.label1);
			this.Name = "MainForm";
			this.Text = "Runescape Drop Bot";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox windowBox;
		private System.Windows.Forms.Button refreshButton;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button findButton;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView inventoryView;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button dropButton;
	}
}

