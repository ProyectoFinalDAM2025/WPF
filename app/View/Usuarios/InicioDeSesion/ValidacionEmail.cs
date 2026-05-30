using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace app.View.Usuarios.InicioDeSesion
{
    public class ValidacionEmail : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string email = value as string;
            string patternEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (string.IsNullOrWhiteSpace(email))
                return new ValidationResult(false, "*Campo vacío");
            if (!Regex.IsMatch(email, patternEmail))
                return new ValidationResult(false, "*No es un mail válido.");

            return ValidationResult.ValidResult;
        }
    }
}
