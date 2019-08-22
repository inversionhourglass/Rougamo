using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FodyTestConsole
{
    class ConsoleCls : INotifyPropertyChanged
    {
        public ConsoleCls()
        {
            PropertyChanged += ConsoleCls_PropertyChanged;
        }

        private void ConsoleCls_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            System.Console.WriteLine($"[ConsoleCls] {e.PropertyName} value changed");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; set; }
    }
}
