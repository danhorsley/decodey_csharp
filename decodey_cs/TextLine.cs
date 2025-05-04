using System;
using System.Collections.Generic;

namespace Decodey.Models
{
	/// <summary>
	/// Represents a line of text in the game, with encrypted and display versions
	/// </summary>
	public class TextLine
	{
		/// <summary>
		/// Characters in the encrypted text
		/// </summary>
		public List<string> EncryptedChars { get; set; } = new List<string>();

		/// <summary>
		/// Characters in the display text (with placeholders for unguessed letters)
		/// </summary>
		public List<DisplayChar> DisplayChars { get; set; } = new List<DisplayChar>();
	}

	/// <summary>
	/// Represents a character in the display text
	/// </summary>
	public class DisplayChar
	{
		/// <summary>
		/// The character to display (actual letter if guessed, placeholder if not)
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Whether this is a placeholder (unguessed letter)
		/// </summary>
		public bool IsPlaceholder { get; set; }
	}
}