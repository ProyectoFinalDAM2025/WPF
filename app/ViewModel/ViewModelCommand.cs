using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace app.ViewModel
{
    public class ViewModelCommand : ICommand
    {

        //Campo que permite pasar una funcion como parametro.
        private readonly Action<object> _executeAction;

        //Campo predicate que determina si una accion se puede ejecutar o no.
        private readonly Predicate<object> _canExecuteAction;

        //Se tiene que crear los constructores de los dos campos de arriba.

        public ViewModelCommand(Action<object> executeAction, Predicate<object> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        //Se define un segundo constructor porque no siempre se requiere validar una accion para se ejecutada... 
        public ViewModelCommand(Action<object> executeAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = null; //...Por lo que _canExcecute se inicializa a null

        }


        //Se subcribe o se da de baja en el evento segun si es necesario por lo que se implementa de la siguite manera
        //(asi aparece cuando se implementa la clase: public event EventHandler CanExecuteChanged;)
        public event EventHandler CanExecuteChanged 
        {
            //RequerySuggested se produce cuando se detecta que el comando ha cambiado.
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        //Si _canExecuteAction es nullo se retorna verdadero ya que no se establecio el predicado de valor
        //En caso contrareo retornamos el valor del delagado;es decir el metodo que se va a recibir 
        public bool CanExecute(object parameter)
        {
            return _canExecuteAction == null ? true : _canExecuteAction(parameter);
        }

        //Aqui solo ejecutamos la accion que resivamos.
        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }
    }
}
