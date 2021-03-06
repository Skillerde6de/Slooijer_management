﻿using Slooier_voorraad.Classes.CommonFunctions;
using Slooier_voorraad.Classes.MigraDoc;
using Slooier_voorraad.Forms;
using System;
using System.Windows.Forms;

namespace Slooier_voorraad
{
	public partial class StartingScreen : Form
	{
		public string CurrentDir = AppDomain.CurrentDomain.BaseDirectory;
		//public string InitialDir = "C:\\";
		public string InitialDir = "A:\\Red Darkness\\Documents\\Documenten\\Github\\Repositories\\Slooier_management\\Slooier_voorraad\\Slooier_voorraad\\Voorbeeld_Data";
		//public string ConnString = string.Format("Server=localhost; User Id=postgres; Database=Slooier_VoorraadSysteem; Port=5432; Password=2761");
		public string ConnString = string.Format($"Server={Properties.Settings.Default.Server}; " +
			$"User Id={Properties.Settings.Default.UserName}; " +
			$"Database={Properties.Settings.Default.Database}; " +
			$"Port={Properties.Settings.Default.Port}; " +
			$"Password={Properties.Settings.Default.password}");

		public StartingScreen()
		{
			InitializeComponent();
		}

		private void BtnAddOrRemove_Click(object sender, EventArgs e)
		{
			AddOrRemoveItems AddOrRemoveForm = new AddOrRemoveItems(ConnString, WindowState);
			Hide();
			AddOrRemoveForm.FormClosed += ReactivateWindow;
			AddOrRemoveForm.Show();
		}

		private void BtnBestellen_Click(object sender, EventArgs e)
		{
			BestelScreen BestelForm = new BestelScreen(ConnString, WindowState);
			Hide();
			BestelForm.FormClosed += ReactivateWindow;
			BestelForm.Show();
		}

		private void BtnAlterStock_Click(object sender, EventArgs e)
		{
			VoorraadCorrectie VoorraadForm = new VoorraadCorrectie(ConnString, WindowState);
			Hide();
			VoorraadForm.FormClosed += ReactivateWindow;
			VoorraadForm.Show();
		}

		private void BtnPdFTester_Click(object sender, EventArgs e)
		{
			// Proberen om een PDF bestand aan te maken
			MigraDocFunctions.MigraDocBeginning();
		}

		private void BtnSettings_Click(object sender, EventArgs e)
		{
			SettingForm SettingForm = new SettingForm();
			Enabled = false;
			SettingForm.FormClosed += ReactivateWindow;
			SettingForm.Show();
		}

		#region Forms
		private void ReactivateWindow(object sender, FormClosedEventArgs e)
		{
			Enabled = true;
			Show();
		}
		#endregion
		private void StartingScreen_SizeChanged(object sender, EventArgs e)
		{
			// Set panels to center of the Form
			CommonFunctions.SetPanelDimensions(PMain, ClientSize);
			CommonFunctions.SetPanelDimensions(PButtons, PMain);
		}

		private void StartingScreen_Load(object sender, EventArgs e)
		{
			//this.CenterToScreen();
			// Set minimumsize
			MinimumSize = new System.Drawing.Size(Properties.Settings.Default.MinimumSizeX, Properties.Settings.Default.MinimumSizeY);
			// Set default location
			CenterFormToScreen();
			//Location = new System.Drawing.Point(Properties.Settings.Default.PX, Properties.Settings.Default.PY);
			// Set panel locations
			CommonFunctions.SetPanelDimensions(PButtons, PMain);
			BackColor = Properties.Settings.Default.BackGroundColor;
		}
		protected void CenterFormToScreen()
		{
			Screen screen = Screen.FromControl(this);

			System.Drawing.Rectangle workingArea = screen.WorkingArea;
			Location = new System.Drawing.Point()
			{
				X = Math.Max(workingArea.X, workingArea.X + (workingArea.Width - Width) / 2),
				Y = Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - Height) / 2)
			};
			Properties.Settings.Default.PX = Location.X;
			Properties.Settings.Default.PY = Location.Y;
			Properties.Settings.Default.Save();
		}
	}
}
