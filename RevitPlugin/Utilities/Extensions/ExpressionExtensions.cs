using System;
using System.Linq.Expressions;
using System.Reflection;

namespace IFCtoRevit.Utilities.Extensions
{
	
	/// <summary>
	/// Class ExpressionExtensions.
	/// </summary>
	public static class ExpressionExtensions
	{

		/// <summary>
		/// Compiles an Expression and get function return value
		/// </summary>
		/// <typeparam name="T">type of return value</typeparam>
		/// <param name="lambda">Expression to compile</param>
		/// <returns>T.</returns>
		public static T GetPropertyValue<T>(this Expression<Func<T>> lambda)
		{
			return lambda.Compile().Invoke();
		}


		/// <summary>
		/// Sets the underlying property value to the given value
		/// from and expression that contains the property.
		/// </summary>
		/// <typeparam name="T">>type of return value</typeparam>
		/// <param name="lambda">Expression.</param>
		/// <param name="value">The value.</param>
		public static void SetPropertyValue<T>(this Expression<Func<T>> lambda, T value)
		{
			//converts a lambda  [() => some.Property], to [some.Property] "get red of lambda"
			var expression = (lambda as LambdaExpression).Body as MemberExpression;

			//Get the property information
			var propertyInfo = (PropertyInfo)expression.Member;

			//get the view model that carry the property
			var target = Expression.Lambda(expression.Expression).Compile().DynamicInvoke();

			//set the property value
			propertyInfo.SetValue(target, value);

		}
	}


}
