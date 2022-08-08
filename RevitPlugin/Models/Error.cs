using System;

namespace IFCtoRevit.Models
{

	/// <summary>
	/// Class Error.
	/// Implements the <see cref="System.IEquatable{Sanveo.Common.Models.Error}" />
	/// </summary>
	/// <seealso cref="System.IEquatable{Sanveo.Common.Models.Error}" />
	public class Error : IEquatable<Error>
	{

		#region Constructor

		/// <summary>
		/// Initialize a new object of the error class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="errorType">Type of the error.</param>
		public Error(string message, ErrorType errorType = ErrorType.Error)
		{
			Message = message;
			ErrorType = errorType;
		}


		/// <summary>
		/// Initialize a new object of the error class.
		/// </summary>
		public Error() { }

		#endregion

		#region Properties

		/// <summary>
		/// Get or set the error type.
		/// </summary>
		public ErrorType ErrorType { get; set; }


		/// <summary>
		/// Get or set the error message.
		/// </summary>
		public string Message { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>A <see cref="System.String" /> that represents this instance.</returns>
		public override string ToString()
		{
			return ErrorType.ToString() + "\n" + Message;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
		public bool Equals(Error other)
		{
			if (this == null || other == null) return false;
			if (ReferenceEquals(this, other)) return true;
			return ToString().Equals(other.ToString());
		}

		#endregion 

	}
}
