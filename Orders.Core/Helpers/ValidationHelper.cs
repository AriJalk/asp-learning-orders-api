using System.ComponentModel.DataAnnotations;

namespace Orders.Core.Helpers
{
	public class ValidationHelper
	{
		internal static void ValidateModel(object obj)
		{
			ValidationContext validationContext = new ValidationContext(obj);
			List<ValidationResult> validationResults = new List<ValidationResult>();
			bool isValid = Validator.TryValidateObject(
				obj, validationContext, validationResults, true);
			if (isValid == false)
			{
				//throw new ArgumentException(validationResults.FirstOrDefault()?.ErrorMessage);

				List<string> errors = new List<string>();
				foreach (ValidationResult validationResult in validationResults)
				{
					if (!string.IsNullOrEmpty(validationResult.ErrorMessage))
					{
						errors.Add(validationResult.ErrorMessage);
					}
				}
				throw new ArgumentException(string.Join("\n", errors));
			}
		}
	}
}
