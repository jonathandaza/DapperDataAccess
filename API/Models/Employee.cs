using Console.Models.Validators;
using FluentValidation.Attributes;
using WebApi.Controllers.Validators;

namespace WebApi.Models
{
	public class Employee
	{
		public string Name { get; set; }
		public double Salary { get; set; }
		public int Age { get; set; }
		public string Email { get; set; }
	}
}