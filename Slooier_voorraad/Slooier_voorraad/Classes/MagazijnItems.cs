﻿using System.ComponentModel;

namespace Slooier_voorraad.Classes
{
	/// <summary>
	/// Contains a list of all articles in the database
	/// </summary>
	public class MagazijnItems
	{
		[Browsable(false)]
		public int Id { get; set; }
		public bool Bestellen { get; set; }
		public string Afdeling { get; set; }
		public string Nummer { get; set; }
		public string Omschrijving { get; set; }
		public int Voorraad { get; set; }
		public double Prijs { get; set; }
	}
}
