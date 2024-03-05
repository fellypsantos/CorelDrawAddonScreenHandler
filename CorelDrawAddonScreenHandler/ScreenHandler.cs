using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CorelDrawAddonScreenHandler
{
    public class ScreenHandler
    {
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetFocus();

        // Reference to CorelDraw main window
        private static IntPtr _corelDrawMainHandler { get; set; }

        protected static List<Window> _openedScreens = new List<Window>();

        public static void Init(IntPtr corelDrawMainHandler)
        {
            _corelDrawMainHandler = corelDrawMainHandler;
        }

        public static void OpenWindow(Window windowToOpen, bool isModal = false, bool closeOtherWindows = false)
        {
            string windowName = windowToOpen.GetType().Name;

            int windowIndexOnList = _openedScreens.FindIndex(window => window.GetType().Name == windowName);

            bool alreadyOpen = windowIndexOnList > -1;

            if (alreadyOpen)
            {
                Window openedWindow = _openedScreens[windowIndexOnList];

                openedWindow.WindowState = WindowState.Normal;

                return;
            }

            new WindowInteropHelper(windowToOpen)
            {
                Owner = _corelDrawMainHandler
            };

            _openedScreens.Add(windowToOpen);

            if (isModal) windowToOpen.ShowDialog();

            else windowToOpen.Show();

            if (!closeOtherWindows) return;

            HandleCloseOtherWindows(windowToOpen);
        }

        private static void HandleCloseOtherWindows(Window windowToOpen)
        {
            List<Window> snapshotOfOpenedWindows = new List<Window>();

            snapshotOfOpenedWindows.AddRange(_openedScreens);

            foreach (Window window in snapshotOfOpenedWindows)
            {
                if (window.GetType().Name != windowToOpen.GetType().Name)
                {
                    window.Close();
                }
            }
        }

        public static void CloseWindow(Window windowToClose)
        {
            _openedScreens.Remove(windowToClose);

            // Prevents CorelDraw main window from going to the background
            SetForegroundWindow(_corelDrawMainHandler);
        }
    }
}
