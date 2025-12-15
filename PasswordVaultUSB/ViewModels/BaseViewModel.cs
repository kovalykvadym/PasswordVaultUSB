using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PasswordVaultUSB.ViewModels
{
    // Базовий клас для всіх ViewModel, реалізує механізм сповіщення інтерфейсу про зміни даних
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Викликає подію оновлення для конкретної властивості
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Допоміжний метод: встановлює значення поля тільки якщо воно змінилося,
        // і автоматично сповіщає інтерфейс.
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            // Якщо нове значення таке ж, як старе нічого не робимо
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}