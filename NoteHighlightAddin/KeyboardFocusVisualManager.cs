using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NoteHighlightAddin
{
    /// <summary>
    /// Draws one consistent accent-coloured focus ring around the control
    /// currently focused by keyboard/mouse. The ring is rendered at form
    /// level so it works with standard WinForms controls and nested layouts.
    /// </summary>
    internal sealed class KeyboardFocusVisualManager : IDisposable
    {
        private const int RingThickness = 2;
        private const int RingGap = 2;

        private readonly Form _owner;
        private readonly Panel _top;
        private readonly Panel _bottom;
        private readonly Panel _left;
        private readonly Panel _right;
        private readonly HashSet<Control> _hookedControls;

        private Control _focusedControl;
        private bool _disposed;


        public KeyboardFocusVisualManager(
            Form owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(
                    nameof(owner));
            }

            _owner =
                owner;

            _hookedControls =
                new HashSet<Control>();

            _top =
                CreateRingPanel();

            _bottom =
                CreateRingPanel();

            _left =
                CreateRingPanel();

            _right =
                CreateRingPanel();

            _owner.Controls.Add(
                _top);

            _owner.Controls.Add(
                _bottom);

            _owner.Controls.Add(
                _left);

            _owner.Controls.Add(
                _right);

            HideRing();

            AttachRecursively(
                _owner);

            _owner.ControlAdded +=
                Owner_ControlAdded;

            _owner.Resize +=
                Owner_LayoutChanged;

            _owner.Layout +=
                Owner_LayoutChanged;

            _owner.Deactivate +=
                Owner_Deactivate;

            _owner.Activated +=
                Owner_Activated;

            _owner.Disposed +=
                Owner_Disposed;
        }


        public void RefreshTargets()
        {
            if (_disposed)
            {
                return;
            }

            AttachRecursively(
                _owner);

            UpdateRing();
        }


        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed =
                true;

            _owner.ControlAdded -=
                Owner_ControlAdded;

            _owner.Resize -=
                Owner_LayoutChanged;

            _owner.Layout -=
                Owner_LayoutChanged;

            _owner.Deactivate -=
                Owner_Deactivate;

            _owner.Activated -=
                Owner_Activated;

            _owner.Disposed -=
                Owner_Disposed;

            foreach (Control control
                in _hookedControls)
            {
                if (control == null ||
                    control.IsDisposed)
                {
                    continue;
                }

                control.Enter -=
                    Control_Enter;

                control.Leave -=
                    Control_Leave;

                control.VisibleChanged -=
                    Control_StateChanged;

                control.EnabledChanged -=
                    Control_StateChanged;

                control.LocationChanged -=
                    Control_StateChanged;

                control.SizeChanged -=
                    Control_StateChanged;

                control.ControlAdded -=
                    Container_ControlAdded;
            }

            _hookedControls.Clear();

            DisposePanel(
                _top);

            DisposePanel(
                _bottom);

            DisposePanel(
                _left);

            DisposePanel(
                _right);
        }


        private static Panel CreateRingPanel()
        {
            return new Panel
            {
                BackColor =
                    NoteHighlightUiTheme.Accent,

                TabStop =
                    false,

                Visible =
                    false,

                Enabled =
                    false
            };
        }


        private void AttachRecursively(
            Control root)
        {
            if (root == null ||
                root.IsDisposed)
            {
                return;
            }

            foreach (Control control
                in root.Controls)
            {
                AttachControl(
                    control);

                if (ShouldInspectChildren(
                    control))
                {
                    AttachRecursively(
                        control);
                }
            }
        }


        private void AttachControl(
            Control control)
        {
            if (control == null ||
                control.IsDisposed ||
                _hookedControls.Contains(
                    control))
            {
                return;
            }

            _hookedControls.Add(
                control);

            if (IsFocusTarget(
                control))
            {
                control.Enter +=
                    Control_Enter;

                control.Leave +=
                    Control_Leave;

                control.VisibleChanged +=
                    Control_StateChanged;

                control.EnabledChanged +=
                    Control_StateChanged;

                control.LocationChanged +=
                    Control_StateChanged;

                control.SizeChanged +=
                    Control_StateChanged;
            }

            control.ControlAdded +=
                Container_ControlAdded;
        }


        private static bool ShouldInspectChildren(
            Control control)
        {
            // NumericUpDown contains internal edit/spinner controls. Its own
            // Enter event is enough and gives a cleaner single focus ring.
            return !(control is NumericUpDown);
        }


        private static bool IsFocusTarget(
            Control control)
        {
            if (!control.TabStop)
            {
                return false;
            }

            if (control is Label ||
                control is GroupBox ||
                control is TabPage ||
                control is SplitContainer ||
                control is FlowLayoutPanel ||
                control is TableLayoutPanel)
            {
                return false;
            }

            return true;
        }


        private void Control_Enter(
            object sender,
            EventArgs e)
        {
            _focusedControl =
                sender as Control;

            UpdateRing();
        }


        private void Control_Leave(
            object sender,
            EventArgs e)
        {
            Control leaving =
                sender as Control;

            if (_focusedControl ==
                leaving)
            {
                _focusedControl =
                    null;
            }

            BeginUpdateAfterFocusChange();
        }


        private void BeginUpdateAfterFocusChange()
        {
            if (_owner.IsDisposed ||
                !_owner.IsHandleCreated)
            {
                HideRing();
                return;
            }

            _owner.BeginInvoke(
                new Action(
                    UpdateFromActiveControl));
        }


        private void UpdateFromActiveControl()
        {
            if (_disposed ||
                _owner.IsDisposed)
            {
                return;
            }

            Control active =
                FindDeepestFocusedControl(
                    _owner);

            if (active != null &&
                IsFocusTarget(
                    active))
            {
                _focusedControl =
                    active;
            }

            UpdateRing();
        }


        private static Control FindDeepestFocusedControl(
            Control root)
        {
            ContainerControl container =
                root as ContainerControl;

            Control active =
                container != null
                    ? container.ActiveControl
                    : null;

            if (active == null)
            {
                return root.Focused
                    ? root
                    : null;
            }

            Control current =
                active;

            while (current is ContainerControl)
            {
                ContainerControl nested =
                    (ContainerControl)current;

                if (nested.ActiveControl ==
                    null)
                {
                    break;
                }

                current =
                    nested.ActiveControl;
            }

            return current;
        }


        private void Control_StateChanged(
            object sender,
            EventArgs e)
        {
            UpdateRing();
        }


        private void Owner_LayoutChanged(
            object sender,
            EventArgs e)
        {
            UpdateRing();
        }


        private void Owner_Deactivate(
            object sender,
            EventArgs e)
        {
            HideRing();
        }


        private void Owner_Activated(
            object sender,
            EventArgs e)
        {
            UpdateFromActiveControl();
        }


        private void Owner_Disposed(
            object sender,
            EventArgs e)
        {
            Dispose();
        }


        private void Owner_ControlAdded(
            object sender,
            ControlEventArgs e)
        {
            AttachControl(
                e.Control);

            AttachRecursively(
                e.Control);
        }


        private void Container_ControlAdded(
            object sender,
            ControlEventArgs e)
        {
            AttachControl(
                e.Control);

            AttachRecursively(
                e.Control);
        }


        private void UpdateRing()
        {
            if (_disposed ||
                _focusedControl == null ||
                _focusedControl.IsDisposed ||
                !_focusedControl.Visible ||
                !_focusedControl.Enabled ||
                !_focusedControl.ContainsFocus ||
                !_owner.ContainsFocus)
            {
                HideRing();
                return;
            }

            Rectangle screenRectangle =
                _focusedControl.RectangleToScreen(
                    _focusedControl.ClientRectangle);

            Rectangle bounds =
                _owner.RectangleToClient(
                    screenRectangle);

            int left =
                bounds.Left -
                RingGap -
                RingThickness;

            int top =
                bounds.Top -
                RingGap -
                RingThickness;

            int right =
                bounds.Right +
                RingGap;

            int bottom =
                bounds.Bottom +
                RingGap;

            int horizontalWidth =
                Math.Max(
                    1,
                    right - left);

            int verticalHeight =
                Math.Max(
                    1,
                    bottom - top);

            _top.SetBounds(
                left,
                top,
                horizontalWidth,
                RingThickness);

            _bottom.SetBounds(
                left,
                bottom,
                horizontalWidth,
                RingThickness);

            _left.SetBounds(
                left,
                top,
                RingThickness,
                verticalHeight);

            _right.SetBounds(
                right,
                top,
                RingThickness,
                verticalHeight);

            ShowRingPanel(
                _top);

            ShowRingPanel(
                _bottom);

            ShowRingPanel(
                _left);

            ShowRingPanel(
                _right);
        }


        private static void ShowRingPanel(
            Panel panel)
        {
            panel.Visible =
                true;

            panel.BringToFront();
        }


        private void HideRing()
        {
            _top.Visible =
                false;

            _bottom.Visible =
                false;

            _left.Visible =
                false;

            _right.Visible =
                false;
        }


        private static void DisposePanel(
            Panel panel)
        {
            if (panel != null &&
                !panel.IsDisposed)
            {
                panel.Dispose();
            }
        }
    }
}
