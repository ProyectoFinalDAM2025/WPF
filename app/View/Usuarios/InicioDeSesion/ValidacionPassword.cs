using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace app.View.Usuarios.InicioDeSesion
{
    public class ValidacionPassword : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string password = SecureStringToString(value as SecureString);
            string patternPassword = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$";

            Debug.WriteLine(password +" E Validation");

            //if (string.IsNullOrEmpty(LogInMail) || LogInMail.Length < 3 || !Regex.IsMatch(LogInMail, patternEmail) ||
            //   Password == null || Password.Length < 3 || !Regex.IsMatch(new NetworkCredential(string.Empty, Password).Password, patternPassword))
            //    return false;

            if (string.IsNullOrWhiteSpace(password))
                return new ValidationResult(false,"*Campo vácio");
            if (!Regex.IsMatch(password, patternPassword))
                return new ValidationResult(false, "*La contraseña tiene que conterner 1 mayúscula, 1 minúscula y 1 número.");

            return  ValidationResult.ValidResult;

        }

        private string SecureStringToString(SecureString secureString)
        {
            if (secureString == null)
                return string.Empty;

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }
    }
}
