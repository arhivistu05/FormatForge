using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FormatForge.App;

internal sealed class ThemedProgressBar : ProgressBar
{
    private Color trackColor = Color.FromArgb(226, 232, 240);
    private Color fillColor = Color.FromArgb(35, 117, 238);
    private Color borderColor = Color.FromArgb(214, 224, 236);

    public ThemedProgressBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color TrackColor
    {
        get => trackColor;
        set
        {
            trackColor = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color FillColor
    {
        get => fillColor;
        set
        {
            fillColor = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor
    {
        get => borderColor;
        set
        {
            borderColor = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath backgroundPath = CreatePath(bounds, Math.Min(Height / 2, 8));
        using SolidBrush trackBrush = new SolidBrush(trackColor);
        using Pen borderPen = new Pen(borderColor);
        e.Graphics.FillPath(trackBrush, backgroundPath);

        double ratio = Maximum <= Minimum ? 0 : (Value - Minimum) / (double)(Maximum - Minimum);
        int fillWidth = (int)Math.Round(bounds.Width * Math.Clamp(ratio, 0.0, 1.0));
        if (fillWidth > 0)
        {
            Rectangle fillBounds = new Rectangle(bounds.Left, bounds.Top, fillWidth, bounds.Height);
            using GraphicsPath fillPath = CreatePath(fillBounds, Math.Min(Height / 2, 8));
            using SolidBrush fillBrush = new SolidBrush(fillColor);
            e.Graphics.FillPath(fillBrush, fillPath);
        }

        e.Graphics.DrawPath(borderPen, backgroundPath);
    }

    private static GraphicsPath CreatePath(Rectangle rectangle, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        int diameter = Math.Min(radius * 2, Math.Min(rectangle.Width, rectangle.Height));
        if (diameter <= 0)
        {
            path.AddRectangle(rectangle);
            path.CloseFigure();
            return path;
        }

        Rectangle arc = new Rectangle(rectangle.Location, new Size(diameter, diameter));
        path.AddArc(arc, 180, 90);
        arc.X = rectangle.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = rectangle.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = rectangle.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
