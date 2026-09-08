using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FormatForge.App;

public class RoundedPanel : Panel
{
    private int borderRadius = 10;
    private int borderSize = 1;
    private Color borderColor = Color.FromArgb(220, 226, 235);

    public RoundedPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        BackColor = Color.White;
    }

    [DefaultValue(10)]
    public int BorderRadius
    {
        get => borderRadius;
        set
        {
            borderRadius = Math.Max(0, value);
            UpdateRegion();
            Invalidate();
        }
    }

    [DefaultValue(1)]
    public int BorderSize
    {
        get => borderSize;
        set
        {
            borderSize = Math.Max(0, value);
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BorderColor
    {
        get => borderColor;
        set
        {
            borderColor = value;
            Invalidate();
        }
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        UpdateRegion();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using Brush brush = new SolidBrush(BackColor);
        using GraphicsPath path = CreatePath(ClientRectangle, borderRadius);
        e.Graphics.FillPath(brush, path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (borderSize <= 0)
        {
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath path = CreatePath(new Rectangle(0, 0, Width - 1, Height - 1), borderRadius);
        using Pen pen = new Pen(borderColor, borderSize);
        e.Graphics.DrawPath(pen, path);
    }

    private void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        if (borderRadius <= 0)
        {
            Region = null;
            return;
        }

        using GraphicsPath path = CreatePath(ClientRectangle, borderRadius);
        Region? oldRegion = Region;
        Region = new Region(path);
        oldRegion?.Dispose();
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
