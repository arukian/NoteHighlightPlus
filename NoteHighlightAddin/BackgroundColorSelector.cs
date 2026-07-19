using System.Drawing;
using System.Windows.Forms;

namespace NoteHighlightAddin
{
    public class BackgroundColorSelector
    {
        public void ShowMenu(
            Button backgroundButton,
            ContextMenuStrip contextMenu)
        {
            contextMenu.Show(
                backgroundButton,
                new Point(
                    0,
                    backgroundButton.Height));
        }

        public void PickColor(
            Button backgroundButton,
            ColorDialog colorDialog)
        {
            if (colorDialog.ShowDialog() ==
                DialogResult.OK)
            {
                backgroundButton.BackColor =
                    colorDialog.Color;
            }
        }

        public void SetTransparent(
            Button backgroundButton)
        {
            backgroundButton.BackColor =
                Color.Transparent;
        }
    }
}