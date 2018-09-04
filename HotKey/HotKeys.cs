using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace HotkeyMusicPlayer
{
    static class HotKeys
    {
        public const int WmHotKey = 0x0312;

        private static Dictionary<int, HotKey> _dictHotKeyToCalBackProc;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static bool Register(HotKey hk)
        {
            if (hk == null) return false;

            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(hk.Key);
            hk.Id = virtualKeyCode + ((int)hk.KeyModifiers * 0x10000);
            hk.RegistrationSucessful = RegisterHotKey(IntPtr.Zero, hk.Id, (uint)hk.KeyModifiers, (uint)virtualKeyCode);

            if (_dictHotKeyToCalBackProc == null)
            {
                _dictHotKeyToCalBackProc = new Dictionary<int, HotKey>();
                ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
            }

            _dictHotKeyToCalBackProc.Add(hk.Id, hk);

            return hk.RegistrationSucessful;
        }

        public static void Unregister(HotKey hk)
        {
            bool containsKey = _dictHotKeyToCalBackProc?.ContainsKey(hk.Id) ?? false;
            if (hk != null && containsKey) UnregisterHotKey(IntPtr.Zero, hk.Id);
        }

        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (handled || msg.message != WmHotKey) return;

            HotKey hotKey;
            if (!_dictHotKeyToCalBackProc.TryGetValue((int)msg.wParam, out hotKey)) return;

            hotKey.Raise();
            handled = true;
        }

        public static IEnumerable<HotKey> GetRegisteredHotKeys(string[] allKeyProperties, IEnumerable<HotKeySource> keySources)
        {
            foreach(HotKeySource source in keySources)
            {
                HotKey hk = GetHotKey(allKeyProperties, source.SearchKey);
                hk.Pressed += source.Target;
                Register(hk);

                yield return hk;
            }
        }

        private static HotKey GetHotKey(string[] hotKeyLines, string keyKey)
        {
            string hotKeyLine = hotKeyLines.FirstOrDefault(l => l.StartsWith(keyKey));

            if (hotKeyLine == null) return null;

            string[] parameter = hotKeyLine.Remove(0, keyKey.Length).Split(',');

            try
            {
                Key key;
                string keyString = parameter[0].Trim();
                int allModifier = 0;

                if (!Enum.TryParse(keyString, true, out key)) return null;

                for (int i = 1; i < parameter.Length; i++)
                {
                    KeyModifier modifier;
                    string modifierString = parameter[i].Trim().ToLower();

                    if (Enum.TryParse(modifierString, true, out modifier)) allModifier += (int)modifier;
                }

                return new HotKey(key, (KeyModifier)allModifier);
            }
            catch { }

            return null;
        }
    }
}
