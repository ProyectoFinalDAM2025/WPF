using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using app.ViewModel.Usuarios;

namespace app.View.Usuarios.Pre_Registros
{
    public class ValidacionPreRegistro : ValidationRule
    {
        private readonly UsuarioViewModel _viewModel = UsuarioViewModel.Instance;
        //Datos Pre-Registros

        public bool EsConfirmacion { get; set; } // Para saber si es el campo de confirmación

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string email = value.ToString();
            if(EsConfirmacion)
                _viewModel.EmailPreregistroConfirmacion2 = email;

            //Debug.WriteLine(email);
            //Debug.WriteLine("P"+_viewModel.EmailPreRegistro);
            //Debug.WriteLine("Pre"+_viewModel.EmailPreregistroConfirmacion2);

            //Si no es nula regreso false
            if (string.IsNullOrWhiteSpace(email))
                return new ValidationResult(false, "*Campo vacío");

            // Expresión regular para validar un email, se agrega using System.Text.RegularExpressions;
            //Explicación del patter al final del documento.
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, pattern))
                return new ValidationResult(false, $"*Email no válido");



            // Solo validar la coincidencia si es el campo de confirmación
            if (EsConfirmacion)
            {
                //Debug.WriteLine($"EmailPreRegistro: {_viewModel.EmailPreRegistro}");
                //Debug.WriteLine($"EmailPreregistroConfirmacion: {_viewModel.EmailPreregistroConfirmacion2}");
                //Debug.WriteLine($"Equals: {_viewModel.EmailPreRegistro.ToString() != _viewModel.EmailPreregistroConfirmacion2.ToString()}");


                if (_viewModel.EmailPreRegistro != _viewModel.EmailPreregistroConfirmacion2)
                    return new ValidationResult(false, "*Los correos no coinciden");
            }

            //return _viewModel.EmailPreRegistro != _viewModel.EmailPreregistroConfirmacion?
            //     new ValidationResult(false, $"*Los correos no coinciden  {_viewModel.EmailPreRegistro} - {_viewModel.EmailPreregistroConfirmacion}") :
            //        new ValidationResult(true, null);

            //Regresa una intacia válida
            return ValidationResult.ValidResult;


        }
    }
}
