using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using app.ViewModel.Usuarios;

namespace app.View.Usuarios.CambiarContraseña
{
    public class ValidacionPassword : ValidationRule
    {

        private readonly UsuarioViewModel _viewModel = UsuarioViewModel.Instance;

        public bool EsConfirmacion { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string password = value as string;

            if(EsConfirmacion)
                _viewModel.PasswordConfirm2 = password;

            if (string.IsNullOrEmpty(password))
                return new ValidationResult(false, "*Campo vacio");

            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$";
            if (!Regex.IsMatch(password, pattern))
                return new ValidationResult(false, "Se requiere 1 minúscula, 1 mayúscula y un número");

            if (EsConfirmacion) {
                if (_viewModel.Passwordd != _viewModel.PasswordConfirm2)
                    return new ValidationResult(false, "Las contraseñas no coinciden.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
