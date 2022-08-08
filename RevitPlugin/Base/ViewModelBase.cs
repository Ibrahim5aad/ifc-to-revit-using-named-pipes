using IFCtoRevit.Models;
using IFCtoRevit.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IFCtoRevit.Base
{
	/// <summary>
	/// Class ViewModelBase.
	/// Implements the <see cref="System.ComponentModel.INotifyPropertyChanged" />
	/// Implements the <see cref="System.ComponentModel.INotifyDataErrorInfo" />
	/// </summary>
	/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
	/// <seealso cref="System.ComponentModel.INotifyDataErrorInfo" />
	public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
	{

		#region Fields

		private readonly Dictionary<string, List<Error>> _validationErrors = new Dictionary<string, List<Error>>();

		#endregion

		#region Events

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;


		/// <summary>
		/// Occurs when the validation errors have changed for a property or for the entire entity.
		/// </summary>
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value that indicates whether the view model has validation errors.
		/// </summary>
		/// <value><c>true</c> if this instance has errors; otherwise, <c>false</c>.</value>
		public virtual bool HasErrors
		{
			get
			{
				//return ValidationErrors.Values.Any(p => p.Any());
				foreach (var errorList in _validationErrors.Values)
				{
					if (errorList == null) continue;
					foreach (var _ in errorList)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Implement this method with the required validation logic of
		/// any property you want to add validation for.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public abstract bool Validate(string propertyName, object value);


		/// <summary>
		/// Notify that property has changed.
		/// </summary>
		/// <param name="property">The property.</param>
		protected virtual void OnPropertyChanged([CallerMemberName] string property = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));


		/// <summary>
		/// Notify that property has changed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="property">The property.</param>
		/// <param name="propertyName">Name of the property.</param>
		protected virtual void OnPropertyChanged<T>(Func<T> property, [CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


		/// <summary>
		/// Notify that Property has changed if the property is valid and also set the value of backing field.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="backingField">Backing Field of the Property</param>
		/// <param name="value">New Value</param>
		/// <param name="propertyName">Name of The Property</param>
		/// <returns><c>true</c> if changed and valid, <c>false</c> otherwise.</returns>
		protected virtual bool RaisePropertyChangedIfValid<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
		{
			if (EqualityComparer<T>.Default.Equals(backingField, value))
				return false;

			if (Validate(propertyName, value))
			{
				backingField = value;
				OnPropertyChanged(propertyName);
				return true;
			}
			return false;
		}



		protected virtual bool RaisePropertyChangedWithValidation<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
		{
			if (EqualityComparer<T>.Default.Equals(backingField, value))
				return false;

			var valid = Validate(propertyName, value);
			backingField = value;
			OnPropertyChanged(propertyName);
			return valid;
		}



		/// <summary>
		/// Gets the validation errors for a specified property or for the entire entity.
		/// </summary>
		/// <param name="propertyName">The name of the property to retrieve validation errors for; or <see langword="null" /> or <see cref="F:System.String.Empty" />, to retrieve entity-level errors.</param>
		/// <returns>The validation errors for the property or entity.</returns>
		public virtual IEnumerable GetErrors(string propertyName)
		{
			IEnumerable result;
			if (string.IsNullOrEmpty(propertyName))
			{
				List<Error> allErrors = new List<Error>();

				foreach (var keyValuePair in this._validationErrors)
				{
					allErrors.AddRange(keyValuePair.Value);
				}
				result = allErrors;
			}
			else
			{
				if (_validationErrors.TryGetValue(propertyName, out var propertyErrors))
				{
					result = propertyErrors;
				}
				else
				{
					result = Enumerable.Empty<Error>();
				}
			}
			return result;
		}


		/// <summary>
		/// Called when [errors changed].
		/// </summary>
		/// <param name="propertyName">The property name.</param>
		protected virtual void OnErrorsChanged(string propertyName)
		{
			ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
		}


		/// <summary>
		/// Adds an error to the errors dictionary.
		/// </summary>
		/// <param name="propertyName"> The property name of the faulty property. </param>
		/// <param name="error"> The error object. </param>
		public virtual void AddError(string propertyName, Error error)
		{
			if (!_validationErrors.ContainsKey(propertyName))
				_validationErrors[propertyName] = new List<Error>();

			if (!_validationErrors[propertyName].Contains(error))
			{
				_validationErrors[propertyName].Add(error);
				OnErrorsChanged(propertyName);
			}
		}


		/// <summary>
		/// Adds an error to the errors dictionary.
		/// </summary>
		/// <param name="propertyName"> The property name of the faulty property. </param>
		/// <param name="message"> The error messge. </param>
		/// <param name="errorType"> The error type. </param>
		public virtual void AddError(string propertyName, string message, ErrorType errorType = ErrorType.Error)
		{
			var error = new Error(message, errorType);
			if (!_validationErrors.ContainsKey(propertyName))
				_validationErrors[propertyName] = new List<Error>();

			if (!_validationErrors[propertyName].Contains(error))
			{
				_validationErrors[propertyName].Add(error);
				OnErrorsChanged(propertyName);
			}
		}


		/// <summary>
		/// Clears the error of a specified property. If the property is not specified
		/// Clears all the errors.
		/// </summary>
		/// <param name="propertyName"> The name of the associated property. </param>
		public virtual void ClearErrors(string propertyName = "")
		{
			if (propertyName == "")
			{
				var keyList = _validationErrors.Keys.ToList();
				foreach (var key in keyList)
				{
					ClearErrors(key);
				}
			}
			else if (_validationErrors.ContainsKey(propertyName))
			{
				_validationErrors.Remove(propertyName);
				OnErrorsChanged(propertyName);
			}
		}


		/// <summary>
		/// Runs a command if the updating flag is not set.
		/// if the flag is true, the function is already running  then the action not run
		/// if the flag is true, No function is running  then the action run
		/// Once action is finished if it was run, then the flag is reset to false
		/// </summary>
		/// <param name="updatingFlag">The updating flag.</param>
		/// <param name="action">The action.</param>
		/// <param name="onFinished">The on finished action.</param>
		protected async Task RunCommand(Expression<Func<bool>> updatingFlag, Func<Task> action, Func<Task> onFinished)
		{
			if (updatingFlag.GetPropertyValue())
				return;

			//set to true
			updatingFlag.SetPropertyValue(true);
			try
			{
				await action();
			}
			finally
			{
				updatingFlag.SetPropertyValue(false);
				await onFinished();
			}
		}


		#endregion

	}
}
