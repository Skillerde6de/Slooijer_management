﻿using Equin.ApplicationFramework;
using Slooier_voorraad.Classes;
using Slooier_voorraad.Classes.CommonFunctions;
using Slooier_voorraad.Classes.CustomMessageBox;
using Slooier_voorraad.Classes.MigraDoc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;


namespace Slooier_voorraad
{
	public partial class BestelScreen : Form
	{
		string ConnString;

		//Lists used
		List<MagazijnItems> items = new List<MagazijnItems>();
		List<BestelItems> BestelItemsList = new List<BestelItems>();

		public BestelScreen(string ConnString, FormWindowState windowState)
		{
			InitializeComponent();
			this.ConnString = ConnString;
			WindowState = windowState;
		}

		private void BestelScreen_Shown(object sender, EventArgs e)
		{
			GetData();
		}

		#region Buttons
		private void BtnSearch_Click(object sender, EventArgs e)
		{
			Search();
		}

		private void BtnVoorraadVerlagen_Click(object sender, EventArgs e)
		{
			if (TxbVoorraad.Text.Length <= 0) { return; }
			try
			{
				int rPos = DgvData.CurrentCell.RowIndex;
				var res = items.ElementAt(rPos);
				int amount = Convert.ToInt32(TxbVoorraad.Text);
				if (amount > 10)
				{
					string message = "Weet je zeker dat je " + amount + " wilt aftrekken van de voorraad?";
					var result = FlexibleMessageBox.Show(message, "Warning", MessageBoxButtons.YesNo);
					if (result == DialogResult.No)
					{
						return;
					}
				}
				res.Voorraad = res.Voorraad - amount;
				BindingListView<MagazijnItems> view = new BindingListView<MagazijnItems>(items);
				DgvLoadData<MagazijnItems>(DgvData, view);
			}
			catch (Exception ex)
			{
				FlexibleMessageBox.Show(ex.Message, "ER IS IETS FOUT GEGAAN!");
			}
		}

		private void BtnRefresh_Click(object sender, EventArgs e)
		{
			GetData();
		}

		private void BtnPDF_Click(object sender, EventArgs e)
		{
			if (BestelItemsList.Count < 1)
			{
				FlexibleMessageBox.Show("Er zijn geen artikelen gekozen om te bestellen", "Geen artikelen te bestellen!");
				return;
			}
			try
			{
				string line = "Aantal--Soort--Nummer--Omschrijving\n";
				foreach (var row in BestelItemsList)
				{
					line = line + $"{row.Bestel_aantal} {row.Soort} -- {row.Nummer} -- {row.Omschrijving}\n";

				}
				FlexibleMessageBox.MAX_HEIGHT_FACTOR = 0.7;
				FlexibleMessageBox.Show(line, "Te bestellen producten:");
			}
			catch (Exception ex)
			{
				FlexibleMessageBox.Show(ex.Message, "ER IS IETS FOUT GEGAAN!");
			}
			try
			{
				// Proberen om een PDF bestand aan te maken
				MigraDocFunctions.MigraDocBeginning();

			}
			catch (Exception ex)
			{
				FlexibleMessageBox.Show(ex.Message, "ER IS IETS FOUT GEGAAN!");
			}
		}
		#endregion


		#region TextBox
		private void TxbVoorraad_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
			{
				e.Handled = true;
			}
		}

		private void TxbZoekInput_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				Search();
			}
		}
		#endregion


		#region DatagridView
		private void DgvData_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
		{
			foreach (DataGridViewColumn dgvc in DgvData.Columns)
			{
				dgvc.ReadOnly = true;
			}
			DgvData.Columns["Bestellen"].ReadOnly = false;
		}

		private void DgvBestellen_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
		{
			foreach (DataGridViewColumn dgvc in DgvBestellen.Columns)
			{
				dgvc.ReadOnly = true;
			}
			DgvBestellen.Columns["Bestel_aantal"].ReadOnly = false;
			DgvBestellen.Columns["Soort"].ReadOnly = false;
		}

		private void DgvData_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			if (DgvData.IsCurrentCellDirty)
			{
				DgvData.CommitEdit(DataGridViewDataErrorContexts.Commit);
			}
		}

		private void DgvData_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			AddOrderItem();
		}
		#endregion


		#region Functions
		private void DgvLoadData<T>(DataGridView gridView, BindingListView<T> data)
		{
			gridView.EndEdit();
			gridView.DataSource = data;
			gridView.Refresh();
		}

		private void GetData()
		{
			items = CommonFunctions.GetMagazijnItems(items, ConnString);
			BindingListView<MagazijnItems> view = new BindingListView<MagazijnItems>(items);
			DgvLoadData(DgvData, view);
		}

		private void Search()
		{
			string searchValue = TxbZoekInput.Text.ToLower();
			DgvData.ClearSelection();
			try
			{
				bool valueResult = false;
				foreach (DataGridViewRow row in DgvData.Rows)
				{
					for (int i = 0; i < row.Cells.Count - 1; i++)
					{
						if (row.Cells[i].Value != null && row.Cells[i].Value.ToString().ToLower().Contains(searchValue))
						{
							int rowIndex = row.Index;
							DgvData.Rows[rowIndex].Selected = true;
							DgvData.FirstDisplayedScrollingRowIndex = DgvData.SelectedRows[0].Index;
							valueResult = true;
							return;
						}
					}
				}
				if (!valueResult)
				{
					FlexibleMessageBox.Show("De opgegeven waarde is niet gevonden:\n" + searchValue, "Not Found");
					return;
				}
			}
			catch (Exception ex)
			{
				FlexibleMessageBox.Show(ex.Message, "ER IS IETS FOUT GEGAAN!");
			}
		}

		private void AddOrderItem()
		{
			var checkedElements = new List<BestelItems>();
			foreach (DataGridViewRow row in DgvData.Rows)
			{
				if (Convert.ToBoolean(row.Cells[0].Value))
				{
					var currentIndex = items.ElementAt(row.Index);

					var newValue = new BestelItems()
					{
						Benaming = currentIndex.Afdeling,
						Nummer = currentIndex.Nummer,
						Omschrijving = currentIndex.Omschrijving,
						Voorraad = currentIndex.Voorraad,
						Soort = "Stuks",
						Prijs = currentIndex.Prijs
					};
					checkedElements.Add(newValue);
					BestelItemsComparer comparer = new BestelItemsComparer();
					if (!BestelItemsList.Contains(newValue, comparer))
					{
						BestelItemsList.Add(newValue);
					}
				}
			}
			for (int i = 0; i < BestelItemsList.Count; i++)
			{
				bool Present = false;
				for (int j = 0; j < checkedElements.Count; j++)
				{
					BestelItemsComparer comparer = new BestelItemsComparer();
					if (comparer.Equals(BestelItemsList[i], checkedElements[j]))
					{
						Present = true;
					}
				}
				if (!Present)
				{
					BestelItemsList.RemoveAt(i);
				}
			}
			BestelItemsList = BestelItemsList.OrderBy(item => item.Benaming).ToList();
			BindingListView<BestelItems> view = new BindingListView<BestelItems>(BestelItemsList);
			DgvLoadData(DgvBestellen, view);
		}
		#endregion

		private void BestelScreen_Load(object sender, EventArgs e)
		{
			// Set minimumsize
			MinimumSize = new System.Drawing.Size(Properties.Settings.Default.MinimumSizeX, Properties.Settings.Default.MinimumSizeY);
			BackColor = Properties.Settings.Default.BackGroundColor;
		}

		private void BestelScreen_SizeChanged(object sender, EventArgs e)
		{
			// Set panels to center of the Form
			CommonFunctions.SetPanelDimensions(PMain, ClientSize);
		}
	}
}
