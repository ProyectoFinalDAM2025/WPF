using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.ViewModel
{
    //Se crea como abstracta para que solo pueda heredar.
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        // Evento definido por INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;


        // Método para disparar el evento PropertyChanged
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
