﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using ImageGlass.Base;
using ImageGlass.Gallery;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using View = ImageGlass.Gallery.View;

namespace ImageGlass.UI;


/// <summary>
/// Displays items with large tiles.
/// </summary>
public class ModernGalleryRenderer : ImageListViewRenderer
{
    private IgTheme Theme { get; set; }


    /// <summary>
    /// Initializes a new instance of the ModernGalleryRenderer class.
    /// </summary>
    /// <param name="theme"></param>
    public ModernGalleryRenderer(IgTheme theme)
    {
        Theme = theme;
    }

    private int BorderRadius(int itemHeight)
    {
        if (Helpers.IsOS(WindowsOS.Win10))
        {
            return 0;
        }

        const int DEFAULT_SIZE = 70;
        var radius = (int)(itemHeight * 1.0f / DEFAULT_SIZE * 3);

        // min border radius = 5
        return Math.Max(radius, 5);
    }


    /// <summary>
    /// Returns item size for the given view mode.
    /// </summary>
    /// <param name="view">The view mode for which the item measurement should be made.</param>
    /// <returns>The item size.</returns>
    public override Size MeasureItem(View view)
    {
        var sz = base.MeasureItem(view);
        var textHeight = ImageListView.Font.Height;

        sz.Width += textHeight * 2 / 5;
        sz.Height -= textHeight / 2;

        return sz;
    }

    /// <summary>
    /// Draws the specified item on the given graphics.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="item">The <see cref="ImageListViewItem"/> to draw.</param>
    /// <param name="state">The current view state of item.</param>
    /// <param name="bounds">The bounding rectangle of item in client coordinates.</param>
    public override void DrawItem(Graphics g, ImageListViewItem item, ItemState state, Rectangle bounds)
    {
        g.SmoothingMode = SmoothingMode.HighQuality;

        var itemPadding = new Size(5, 5);
        var itemMargin = new Size(5, 5);
        var itemBounds = new Rectangle(
            bounds.X,
            bounds.Y + itemMargin.Height,
            bounds.Width - itemMargin.Width,
            bounds.Height - 2 * itemMargin.Width);

        // background
        #region Draw background

        using var bgBrush = new SolidBrush(Color.Transparent);
        using var bgPath = ThemeUtils.GetRoundRectanglePath(itemBounds, BorderRadius(itemBounds.Height));

        // on pressed
        if (state.HasFlag(ItemState.Pressed))
        {
            bgBrush.Color = Theme.Settings.ThumbnailItemActiveColor;
        }
        // on hover
        else if (state.HasFlag(ItemState.Hovered))
        {
            bgBrush.Color = Theme.Settings.ThumbnailItemHoverColor;
        }
        // on selected
        else if (state.HasFlag(ItemState.Selected))
        {
            bgBrush.Color = Theme.Settings.ThumbnailItemSelectedColor;
        }

        using var penBorder = new Pen(Color.FromArgb(bgBrush.Color.A, bgBrush.Color));

        // draw background
        g.FillPath(bgBrush, bgPath);
        g.DrawPath(penBorder, bgPath);

        #endregion


        // display text
        #region Display text

        var textSize = new Size(0, 0);
        if (ImageListView.ShowItemText)
        {
            textSize = TextRenderer.MeasureText(item.Text, ImageListView.Font);

            var foreColor = Theme.Settings.ThumbnailBarTextColor;

            if (state.HasFlag(ItemState.Disabled))
            {
                // light background color
                if (Theme.Settings.MenuBgColor.GetBrightness() > 0.5)
                {
                    foreColor = ThemeUtils.DarkenColor(Theme.Settings.ThumbnailBarBgColor, 0.5f);
                }
                // dark background color
                else
                {
                    foreColor = ThemeUtils.LightenColor(Theme.Settings.ThumbnailBarBgColor, 0.5f);
                }
            }

            var text = item.Text;
            var textRegion = new Rectangle(
                bounds.Left + itemMargin.Width,
                bounds.Bottom - textSize.Height - itemMargin.Height,
                bounds.Width - itemMargin.Width * 2,
                textSize.Height);

            if (textSize.Width > textRegion.Width)
            {
                text = Helpers.EllipsisText(text, textRegion.Width, g);
            }

            TextRenderer.DrawText(g, text, ImageListView.Font, textRegion, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }

        #endregion


        // thumbnail image
        #region Thumbnail image

        // get image
        var img = item.GetCachedImage(CachedImageType.Thumbnail);
        if (img is null) return;


        // image bound
        var imgRect = Utility.GetSizedImageBounds(img, new(
            itemBounds.X + itemPadding.Width,
            itemBounds.Y + itemPadding.Height,
            itemBounds.Width - 2 * itemPadding.Width,
            itemBounds.Height - (2 * itemPadding.Width) - textSize.Height));

        if (state.HasFlag(ItemState.Pressed) || state.HasFlag(ItemState.Disabled))
        {
            // change opacity of the image
            var cMatrix = new ColorMatrix { Matrix33 = 0.7f };
            var imgAttrs = new ImageAttributes();
            imgAttrs.SetColorMatrix(cMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // pressed style
            if (state.HasFlag(ItemState.Pressed))
            {
                imgRect.Y += 1;
            }

            g.DrawImage(img, imgRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttrs);
        }
        else
        {
            g.DrawImage(img, imgRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
        }

        #endregion

    }
}
