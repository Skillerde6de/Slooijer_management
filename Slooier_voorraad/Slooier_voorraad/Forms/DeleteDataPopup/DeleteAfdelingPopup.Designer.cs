﻿namespace Slooier_voorraad.Forms.DeleteDataPopup
{
	partial class DeleteAfdelingPopup
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
			this.defaultBackground1 = new Slooier_voorraad.Controls.DefaultBackground();
			this.SuspendLayout();
			// 
			// defaultBackground1
			// 
			this.defaultBackground1.AutoSize = true;
			this.defaultBackground1.BackColor = System.Drawing.Color.Gainsboro;
			this.defaultBackground1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.defaultBackground1.Location = new System.Drawing.Point(0, 0);
			this.defaultBackground1.Name = "defaultBackground1";
			this.defaultBackground1.Size = new System.Drawing.Size(800, 450);
			this.defaultBackground1.TabIndex = 0;
			// 
			// DeleteAfdelingPopup
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.defaultBackground1);
			this.Name = "DeleteAfdelingPopup";
			this.Text = "DeleteAfdelingPopup";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Controls.DefaultBackground defaultBackground1;
	}
}