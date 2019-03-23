using System;
using System.ComponentModel;
namespace Schubert
{
    public interface IProfileStorage : INotifyPropertyChanged
    {
        string PropertyNames { get; set; }
        byte[] PropertyValuesBinary { get; set; }
        string PropertyValuesString { get; set; }
    }
}
