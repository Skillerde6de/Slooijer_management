﻿using Equin.ApplicationFramework;
using Npgsql;
using Slooier_voorraad.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;



namespace Slooier_voorraad
{
	public partial class MainPage : Form
	{
		int[] numbers = new int[] { 5, 8, 9, 12, 8, 2, 96, 8, 1, 5 };

		string CurrentDir = AppDomain.CurrentDomain.BaseDirectory;
		//string InitialDir = "C:\\";
		string InitialDir = "A:\\Red Darkness\\Documents\\Documenten\\Github\\Repositories\\Slooier_management\\Slooier_voorraad\\Slooier_voorraad\\Voorbeeld_Data";
		string ConnString = string.Format("Server=localhost; User Id=postgres; Database=Slooier_VoorraadSysteem; Port=5432; Password=2761");

		//Lists used
		List<MagazijnItems> items = new List<MagazijnItems>();
		List<BestelItems> BestelItemsList = new List<BestelItems>();

		public MainPage()
		{
			InitializeComponent();

			GetData();
		}


		#region Buttons
		private void BtnSearch_Click(object sender, EventArgs e)
		{
			string searchValue = textBox1.Text.ToLower();
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
					MessageBox.Show("Unable to find " + searchValue, "Not Found");
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void BtnVoorraadVerlagen_Click(object sender, EventArgs e)
		{
			try
			{
				int rPos = DgvData.CurrentCell.RowIndex;
				var res = items.ElementAt(rPos);
				int amount = Convert.ToInt32(TxbVoorraad.Text);
				if (amount > 10)
				{
					string message = "Weet je zeker dat je " + amount + " wilt aftrekken van de voorraad?";
					var result = MessageBox.Show(message, "Warning", MessageBoxButtons.YesNo);
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
				MessageBox.Show(ex.Message);
			}
		}

		private void BtnAddFileToDb_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
			{
				openFileDialog1.InitialDirectory = InitialDir;
				openFileDialog1.Filter = "csv files(*.csv)|*.csv";
				openFileDialog1.RestoreDirectory = true;
				if (openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					AddDataToExistingDB(openFileDialog1.FileName);
				}
			}
		}

		private void BtnGet_Click(object sender, EventArgs e)
		{
			GetData();
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
			try
			{
				using (var conn = new NpgsqlConnection(ConnString))
				{
					conn.Open();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = string.Format("SELECT afdelingnaam, nummer, omschrijving, voorraad " +
													"FROM voorraad INNER JOIN afdelingen ON (voorraad.afdeling = afdelingen.id);");

						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								var res = new MagazijnItems()
								{
									Benaming = reader.GetString(0),
									Nummer = reader.GetString(1),
									Omschrijving = reader.GetString(2),
									Voorraad = reader.GetInt32(3)
								};
								Console.WriteLine(res);
								items.Add(res);
							}
						}
						items = items.OrderBy(item => item.Benaming).ToList();
						BindingListView<MagazijnItems> view = new BindingListView<MagazijnItems>(items);
						DgvLoadData(DgvData, view);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void AddDataToExistingDB(string filePath)
		{
			try
			{
				var File = filePath;
				string afdelingNaam = "";
				using (var reader = new StreamReader(File))
				{
					while (!reader.EndOfStream)
					{
						var line = reader.ReadLine();
						var values = line.Split(';');
						if (values[0] != "")
						{
							afdelingNaam = values[0];
							using (var conn = new NpgsqlConnection(ConnString))
							{
								conn.Open();
								bool exists = false;
								using (var cmd = new NpgsqlCommand())
								{
									cmd.Connection = conn;
									cmd.CommandText = string.Format(@"SELECT afdelingnaam FROM afdelingen WHERE afdelingnaam = '{0}';", afdelingNaam.ToLower());
									using (var SqLReader = cmd.ExecuteReader())
									{
										while (SqLReader.Read())
										{
											if (SqLReader.GetString(0).ToLower() == afdelingNaam.ToLower())
											{
												exists = true;
											}
										}
									}
								}
								if (!exists)
								{
									using (var cmd = new NpgsqlCommand())
									{
										cmd.Connection = conn;
										cmd.CommandText = string.Format(@"INSERT INTO afdelingen(afdelingnaam) VALUES('{0}');", values[0]);
										cmd.ExecuteNonQuery();
									}
								}
							}
						}
						if (values[1] != "" || values[3] != "")
						{
							using (var conn = new NpgsqlConnection(ConnString))
							{
								conn.Open();
								bool exists = false;
								using (var cmd = new NpgsqlCommand())
								{
									cmd.Connection = conn;
									cmd.CommandText = string.Format(@"SELECT nummer, omschrijving FROM voorraad WHERE nummer = '{0}' AND omschrijving = '{1}';", values[1], values[3]);
									using (var SqLReader = cmd.ExecuteReader())
									{
										while (SqLReader.Read())
										{
											string res = SqLReader.GetString(0).ToLower() + SqLReader.GetString(1).ToLower();

											if (SqLReader.GetString(0).ToLower() == values[1].ToLower() && SqLReader.GetString(1).ToLower() == values[3].ToLower())
											{
												exists = true;
											}
										}
									}
								}
								if (!exists)
								{
									int IdRef = int.MinValue;
									using (var cmd = new NpgsqlCommand())
									{
										cmd.Connection = conn;
										cmd.CommandText = string.Format(@"SELECT id FROM afdelingen WHERE afdelingnaam = '{0}';", afdelingNaam);
										using (var SqlReader = cmd.ExecuteReader())
										{
											while (SqlReader.Read())
											{
												IdRef = SqlReader.GetInt32(0);
											}
										}
									}
									if (IdRef != int.MinValue)
									{
										using (var cmd = new NpgsqlCommand())
										{
											cmd.Connection = conn;
											cmd.CommandText = string.Format(@"INSERT INTO voorraad(nummer, omschrijving, voorraad, afdeling) VALUES ('{0}', '{1}', {2}, {3});", values[1], values[3], 0, IdRef);
											cmd.ExecuteNonQuery();
										}
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
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
						Benaming = currentIndex.Benaming,
						Nummer = currentIndex.Nummer,
						Omschrijving = currentIndex.Omschrijving,
						Voorraad = currentIndex.Voorraad
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
	}
}