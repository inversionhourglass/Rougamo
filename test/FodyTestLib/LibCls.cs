using System.ComponentModel;

namespace FodyTestLib
{
    public class LibCls : INotifyPropertyChanged
    {
        public LibCls()
        {
            PropertyChanged += LibCls_PropertyChanged;
        }

        private void LibCls_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            System.Console.WriteLine($"[LibCls] {e.PropertyName} value changed");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
    }
}
