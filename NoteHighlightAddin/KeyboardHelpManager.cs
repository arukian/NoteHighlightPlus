using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NoteHighlightAddin
{
    /// <summary>
    /// Keeps a contextual keyboard legend synchronized with the control that
    /// currently owns focus. This class only reports existing keyboard
    /// behavior; it does not change navigation rules.
    /// </summary>
    internal sealed class KeyboardHelpManager : IDisposable
    {
        private readonly Form _owner;
        private readonly Label _helpLabel;
        private readonly Func<Control, string> _helpResolver;
        private readonly string _defaultHelp;
        private readonly HashSet<Control> _hookedControls;
        private bool _disposed;

        public KeyboardHelpManager(
            Form owner,
            Label helpLabel,
            Func<Control, string> helpResolver,
            string defaultHelp)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (helpLabel == null)
            {
                throw new ArgumentNullException(nameof(helpLabel));
            }

            if (helpResolver == null)
            {
                throw new ArgumentNullException(nameof(helpResolver));
            }

            _owner = owner;
            _helpLabel = helpLabel;
            _helpResolver = helpResolver;
            _defaultHelp = defaultHelp ?? string.Empty;
            _hookedControls = new HashSet<Control>();

            _helpLabel.Text = _defaultHelp;

            AttachRecursively(_owner);

            _owner.ControlAdded += Owner_ControlAdded;
            _owner.Activated += Owner_Activated;
            _owner.Disposed += Owner_Disposed;
        }

        public void RefreshTargets()
        {
            if (_disposed)
            {
                return;
            }

            AttachRecursively(_owner);
            UpdateFromActiveControl();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _owner.ControlAdded -= Owner_ControlAdded;
            _owner.Activated -= Owner_Activated;
            _owner.Disposed -= Owner_Disposed;

            foreach (Control control in _hookedControls)
            {
                if (control == null || control.IsDisposed)
                {
                    continue;
                }

                control.Enter -= Control_Enter;
                control.ControlAdded -= Container_ControlAdded;
            }

            _hookedControls.Clear();
        }

        private void AttachRecursively(Control root)
        {
            if (root == null || root.IsDisposed)
            {
                return;
            }

            foreach (Control control in root.Controls)
            {
                AttachControl(control);

                // NumericUpDown owns private edit/spinner controls. The parent
                // Enter event is enough for one clear help message.
                if (!(control is NumericUpDown))
                {
                    AttachRecursively(control);
                }
            }
        }

        private void AttachControl(Control control)
        {
            if (control == null ||
                control.IsDisposed ||
                _hookedControls.Contains(control))
            {
                return;
            }

            _hookedControls.Add(control);

            if (IsHelpTarget(control))
            {
                control.Enter += Control_Enter;
            }

            control.ControlAdded += Container_ControlAdded;
        }

        private static bool IsHelpTarget(Control control)
        {
            if (control is Label ||
                control is GroupBox ||
                control is TabPage ||
                control is SplitContainer ||
                control is FlowLayoutPanel ||
                control is TableLayoutPanel)
            {
                return false;
            }

            return
                control.TabStop ||
                control is Button ||
                control is CheckBox ||
                control is ComboBox ||
                control is ListBox ||
                control is TextBoxBase ||
                control is NumericUpDown;
        }

        private void Control_Enter(object sender, EventArgs e)
        {
            UpdateHelp(sender as Control);
        }

        private void UpdateHelp(Control control)
        {
            if (_disposed || _helpLabel.IsDisposed)
            {
                return;
            }

            string text = control == null
                ? null
                : _helpResolver(control);

            _helpLabel.Text = string.IsNullOrWhiteSpace(text)
                ? _defaultHelp
                : text;
        }

        private void UpdateFromActiveControl()
        {
            if (_disposed || _owner.IsDisposed)
            {
                return;
            }

            UpdateHelp(FindDeepestActiveControl(_owner));
        }

        private static Control FindDeepestActiveControl(Control root)
        {
            ContainerControl container = root as ContainerControl;

            if (container == null || container.ActiveControl == null)
            {
                return null;
            }

            Control current = container.ActiveControl;

            while (current is ContainerControl)
            {
                ContainerControl nested = (ContainerControl)current;

                if (nested.ActiveControl == null)
                {
                    break;
                }

                current = nested.ActiveControl;
            }

            return current;
        }

        private void Owner_ControlAdded(object sender, ControlEventArgs e)
        {
            AttachControl(e.Control);
            AttachRecursively(e.Control);
        }

        private void Container_ControlAdded(object sender, ControlEventArgs e)
        {
            AttachControl(e.Control);
            AttachRecursively(e.Control);
        }

        private void Owner_Activated(object sender, EventArgs e)
        {
            UpdateFromActiveControl();
        }

        private void Owner_Disposed(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
