using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GitCodeSearch.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged([CallerMemberName] string? property = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    protected void SetField<T>(ref T field, T value, [CallerMemberName] string? property = null)
    {
        field = value;
        RaisePropertyChanged(property);
    }
}
