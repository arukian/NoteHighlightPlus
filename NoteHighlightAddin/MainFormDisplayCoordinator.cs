using System;
using System.Windows.Forms;

namespace NoteHighlightAddin
{
    public class MainFormDisplayCoordinator
    {
        private readonly WindowForegroundService
            _foregroundService;

        public MainFormDisplayCoordinator(
            WindowForegroundService foregroundService)
        {
            _foregroundService =
                foregroundService ??
                throw new ArgumentNullException(
                    nameof(foregroundService));
        }

        public void HandleShown(
            Form form,
            bool quickStyle,
            Button highlightButton)
        {
            if (form == null)
            {
                throw new ArgumentNullException(
                    nameof(form));
            }

            if (highlightButton == null)
            {
                throw new ArgumentNullException(
                    nameof(highlightButton));
            }

            if (quickStyle)
            {
                highlightButton.PerformClick();
                return;
            }

            form.WindowState =
                FormWindowState.Minimized;

            form.WindowState =
                FormWindowState.Normal;

            _foregroundService.BringToFront(
                form.Handle);
        }
    }
}