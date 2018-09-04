using System;
using System.Windows.Input;

namespace HotkeyMusicPlayer
{
    public delegate void KeyPressedEventHandler(object sender, KeyPressedEventArgs e);

    public class HotKey : IDisposable
    {
        public bool _disposed = false;

        public event KeyPressedEventHandler Pressed;

        public bool RegistrationSucessful { get; set; }

        public Key Key { get; private set; }

        public KeyModifier KeyModifiers { get; private set; }

        public int Id { get; set; }

        public HotKey(Key k, KeyModifier keyModifiers)
        {
            Key = k;
            KeyModifiers = keyModifiers;
        }

        public void Raise()
        {
            Pressed?.Invoke(this, new KeyPressedEventArgs(Key));
        }

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be _disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be _disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    HotKeys.Unregister(this);
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }

    // ******************************************************************
    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }
}
