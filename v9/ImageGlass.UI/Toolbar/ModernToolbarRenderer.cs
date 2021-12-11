﻿using System.Drawing.Drawing2D;

namespace ImageGlass.UI.Toolbar;

public class ModernToolbarRenderer : ToolStripSystemRenderer
{
    private IgTheme Theme { get; set; }
    private const int BORDER_RADIUS = 5;

    public ModernToolbarRenderer(IgTheme theme)
    {
        Theme = theme;
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        // Disable the base() method here to remove unwanted border of toolbar
        // base.OnRenderToolStripBorder(e);
    }

    protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
        var font = new Font(FontFamily.GenericSerif, 10, FontStyle.Bold);
        var fontSize = e.Graphics.MeasureString("…", font);

        #region Draw Background
        var brushBg = new SolidBrush(Color.Black);

        var rect = new RectangleF(
            0,
            e.Item.Padding.All + 1,
            fontSize.Width + e.Item.Padding.All / 2,
            e.Item.ContentRectangle.Height - (e.Item.Padding.All * 4)
        );


        using var path = ThemeUtils.GetRoundRectanglePath(rect, BORDER_RADIUS);

        // on pressed
        if (e.Item.Pressed)
        {
            brushBg = new SolidBrush(Theme.Settings.AccentSelectedColor);
            e.Graphics.FillPath(brushBg, path);
        }
        // on hover
        else if (e.Item.Selected)
        {
            brushBg = new SolidBrush(Theme.Settings.AccentHoverColor);
            e.Graphics.FillPath(brushBg, path);
        }

        brushBg.Dispose();
        #endregion


        #region Draw "..."
        var brushFont = new SolidBrush(Theme.Settings.TextColor);

        e.Graphics.DrawString("…",
            font,
            brushFont,
            (e.Item.Bounds.Width / 2) - (fontSize.Width / 2) - (e.Item.Padding.All / 2),
            (e.Item.Bounds.Height / 2) - (fontSize.Height / 2)
        );

        font.Dispose();
        brushFont.Dispose();
        #endregion

    }


    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        var isBtn = e.Item.GetType().Name == nameof(ToolStripButton);

        if (isBtn)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            var btn = e.Item as ToolStripButton;
            var rect = btn.ContentRectangle;
            rect.Inflate(-btn.Padding.All, -btn.Padding.All);
            rect.Location = new(1, 1);

            using var path = ThemeUtils.GetRoundRectanglePath(rect, BORDER_RADIUS);

            // on pressed
            if (btn.Pressed)
            {
                using var brush = new SolidBrush(Theme.Settings.AccentSelectedColor);
                e.Graphics.FillPath(brush, path);
            }
            // on hover
            else if (btn.Selected)
            {
                using var brush = new SolidBrush(Theme.Settings.AccentHoverColor);
                e.Graphics.FillPath(brush, path);
            }
            // on checked
            else if (btn.Checked)
            {
                if (e.Item.Enabled)
                {
                    using var brush = new SolidBrush(Theme.Settings.AccentColor);
                    e.Graphics.FillPath(brush, path);
                }
                // on checked + disabled
                else
                {
                    using var brush = new SolidBrush(Color.FromArgb(80, Theme.Settings.AccentColor));
                    e.Graphics.FillPath(brush, path);
                }
            }

            return;
        }


        base.OnRenderButtonBackground(e);
    }

}