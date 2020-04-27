using Prism.Mvvm;

namespace CHI.Infrastructure
{
    public class SelectedObject<T> : BindableBase
    {
        bool isSelected;
        T obj;

        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
        public T Object { get => obj; set => SetProperty(ref obj, value); }

        public SelectedObject()
        {

        }

        public SelectedObject(bool isSelected, T obj)
        {
            IsSelected = isSelected;
            Object = obj;
        }
    }
}
