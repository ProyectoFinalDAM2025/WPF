using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace app.View.Usuarios.Pre_Registros
{
    public class ComboBoxSelectionValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            //Debug.WriteLine("Combovalue: "+value.ToString());
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "*Debes seleccionar un rol.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
