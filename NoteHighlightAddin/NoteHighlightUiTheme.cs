using System.Drawing;

namespace NoteHighlightAddin
{
    /// <summary>
    /// Shared visual tokens for the NoteHighlight+ WinForms UI.
    ///
    /// Step 24.1 only defines the visual language. Forms start consuming
    /// these values in the following UI refresh steps.
    /// </summary>
    internal static class NoteHighlightUiTheme
    {
        public static readonly Color WindowBackground =
            Color.FromArgb(24, 25, 31);

        public static readonly Color Surface =
            Color.FromArgb(31, 33, 41);

        public static readonly Color SurfaceRaised =
            Color.FromArgb(39, 41, 51);

        public static readonly Color SurfaceHover =
            Color.FromArgb(48, 50, 62);

        public static readonly Color Border =
            Color.FromArgb(64, 67, 82);

        public static readonly Color BorderStrong =
            Color.FromArgb(83, 86, 104);

        public static readonly Color TextPrimary =
            Color.FromArgb(242, 243, 247);

        public static readonly Color TextSecondary =
            Color.FromArgb(177, 181, 195);

        public static readonly Color TextMuted =
            Color.FromArgb(127, 131, 148);

        public static readonly Color Accent =
            Color.FromArgb(124, 93, 152);

        public static readonly Color AccentHover =
            Color.FromArgb(143, 109, 174);

        public static readonly Color AccentPressed =
            Color.FromArgb(104, 76, 132);

        public static readonly Color Selection =
            Color.FromArgb(73, 57, 92);

        public static readonly Color Danger =
            Color.FromArgb(203, 82, 82);

        public static readonly Color DisabledBackground =
            Color.FromArgb(45, 47, 56);

        public static readonly Color DisabledText =
            Color.FromArgb(104, 108, 120);

        public const string FontFamily =
            "Segoe UI";

        public const float BodyFontSize =
            9.0f;

        public const float SmallFontSize =
            8.25f;

        public const float SectionFontSize =
            9.5f;

        public const int ControlHeight =
            30;

        public const int SmallControlHeight =
            28;

        public const int CornerRadius =
            6;

        public const int SpacingSmall =
            6;

        public const int Spacing =
            10;

        public const int SpacingLarge =
            16;

        public static Font CreateBodyFont()
        {
            return new Font(
                FontFamily,
                BodyFontSize,
                FontStyle.Regular,
                GraphicsUnit.Point);
        }

        public static Font CreateSectionFont()
        {
            return new Font(
                FontFamily,
                SectionFontSize,
                FontStyle.Bold,
                GraphicsUnit.Point);
        }

        public static Font CreateSmallFont()
        {
            return new Font(
                FontFamily,
                SmallFontSize,
                FontStyle.Regular,
                GraphicsUnit.Point);
        }
    }
}
