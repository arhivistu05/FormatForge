using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FormatForge.App;

public enum UiIconKind
{
    Convert,
    AddFile,
    AddFolder,
    Remove,
    Clear,
    Settings,
    Info,
    Tags,
    Image,
    Audio,
    Video,
    Document,
    Pdf,
    Check,
    Warning,
    More,
    Folder,
    Advanced,
    Plus,
    Search
}

public static class UiIconFactory
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr handle);

    public static Icon TryLoadApplicationIcon()
    {
        string[] candidates =
        {
            Path.Combine(AppContext.BaseDirectory, "Assets", "formatforge.ico"),
            Path.Combine(Application.StartupPath, "Assets", "formatforge.ico"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "formatforge.ico")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Assets", "formatforge.ico"))
        };

        foreach (string candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(candidate))
            {
                return new Icon(candidate);
            }
        }

        using Bitmap bitmap = CreateAppLogoBitmap(256);
        IntPtr handle = bitmap.GetHicon();
        try
        {
            using Icon icon = Icon.FromHandle(handle);
            return (Icon)icon.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    public static Bitmap CreateAppLogoBitmap(int size)
    {
        Bitmap bitmap = new Bitmap(size, size);
        bitmap.SetResolution(96, 96);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        float s = size / 256f;
        PointF[] shadow = LogoPoints(s, 8 * s, 9 * s);
        PointF[] points = LogoPoints(s, 0, 0);
        using GraphicsPath shadowPath = new GraphicsPath();
        shadowPath.AddPolygon(shadow);
        using GraphicsPath logoPath = new GraphicsPath();
        logoPath.AddPolygon(points);
        using Brush shadowBrush = new SolidBrush(Color.FromArgb(92, 23, 29, 38));
        using Brush fillBrush = new LinearGradientBrush(new RectangleF(30 * s, 34 * s, 176 * s, 188 * s), Color.FromArgb(255, 255, 132, 39), Color.FromArgb(255, 117, 38, 14), LinearGradientMode.ForwardDiagonal);
        using Pen darkPen = new Pen(Color.FromArgb(230, 43, 51, 65), 8 * s) { LineJoin = LineJoin.Round };
        using Pen lightPen = new Pen(Color.FromArgb(190, 255, 224, 168), 3 * s) { LineJoin = LineJoin.Round };
        graphics.FillPath(shadowBrush, shadowPath);
        graphics.FillPath(fillBrush, logoPath);
        graphics.DrawPath(darkPen, logoPath);
        graphics.DrawPath(lightPen, logoPath);

        using Pen sparkPen = new Pen(Color.FromArgb(220, 250, 112, 25), Math.Max(1.3f, 2.8f * s)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        graphics.DrawLine(sparkPen, 34 * s, 38 * s, 49 * s, 31 * s);
        graphics.DrawLine(sparkPen, 203 * s, 70 * s, 224 * s, 75 * s);
        graphics.DrawLine(sparkPen, 189 * s, 160 * s, 207 * s, 170 * s);
        graphics.DrawLine(sparkPen, 50 * s, 218 * s, 68 * s, 220 * s);
        using Brush dotBrush = new SolidBrush(Color.FromArgb(220, 248, 99, 20));
        graphics.FillEllipse(dotBrush, 214 * s, 94 * s, 9 * s, 9 * s);
        graphics.FillEllipse(dotBrush, 26 * s, 176 * s, 7 * s, 7 * s);
        graphics.FillEllipse(dotBrush, 209 * s, 184 * s, 6 * s, 6 * s);
        return bitmap;
    }

    public static Bitmap CreateIcon(UiIconKind kind, Color color, int size = 24)
    {
        Bitmap bitmap = new Bitmap(size, size);
        bitmap.SetResolution(96, 96);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        float s = size / 24f;
        using Pen pen = new Pen(color, Math.Max(1.6f, 2.1f * s)) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        using Brush brush = new SolidBrush(color);

        switch (kind)
        {
            case UiIconKind.Convert:
                DrawArrow(graphics, pen, 5 * s, 8 * s, 18 * s, 8 * s, s);
                DrawArrow(graphics, pen, 19 * s, 16 * s, 6 * s, 16 * s, s);
                break;
            case UiIconKind.AddFile:
                DrawDocument(graphics, pen, 5 * s, 3 * s, 12 * s, 16 * s, s);
                DrawPlus(graphics, pen, 18 * s, 17 * s, 4 * s);
                break;
            case UiIconKind.AddFolder:
                DrawFolder(graphics, pen, 3 * s, 6 * s, 18 * s, 13 * s, s);
                DrawPlus(graphics, pen, 19 * s, 16 * s, 3 * s);
                break;
            case UiIconKind.Remove:
                graphics.DrawLine(pen, 8 * s, 7 * s, 16 * s, 7 * s);
                graphics.DrawLine(pen, 10 * s, 4 * s, 14 * s, 4 * s);
                graphics.DrawRectangle(pen, 7 * s, 8 * s, 10 * s, 12 * s);
                graphics.DrawLine(pen, 10 * s, 11 * s, 10 * s, 18 * s);
                graphics.DrawLine(pen, 14 * s, 11 * s, 14 * s, 18 * s);
                break;
            case UiIconKind.Clear:
                graphics.DrawLine(pen, 6 * s, 6 * s, 18 * s, 18 * s);
                graphics.DrawLine(pen, 18 * s, 6 * s, 6 * s, 18 * s);
                break;
            case UiIconKind.Settings:
            case UiIconKind.Advanced:
                graphics.DrawEllipse(pen, 7 * s, 7 * s, 10 * s, 10 * s);
                for (int i = 0; i < 8; i++)
                {
                    double a = Math.PI * 2 * i / 8;
                    graphics.DrawLine(pen, 12 * s + (float)Math.Cos(a) * 7 * s, 12 * s + (float)Math.Sin(a) * 7 * s, 12 * s + (float)Math.Cos(a) * 9 * s, 12 * s + (float)Math.Sin(a) * 9 * s);
                }
                break;
            case UiIconKind.Info:
                graphics.DrawEllipse(pen, 4 * s, 4 * s, 16 * s, 16 * s);
                graphics.FillEllipse(brush, 11 * s, 7 * s, 2 * s, 2 * s);
                graphics.DrawLine(pen, 12 * s, 11 * s, 12 * s, 16 * s);
                break;
            case UiIconKind.Tags:
                graphics.DrawPolygon(pen, new[] { new PointF(4 * s, 5 * s), new PointF(14 * s, 5 * s), new PointF(21 * s, 12 * s), new PointF(12 * s, 21 * s), new PointF(4 * s, 13 * s) });
                graphics.FillEllipse(brush, 8 * s, 8 * s, 3 * s, 3 * s);
                break;
            case UiIconKind.Image:
                graphics.DrawRectangle(pen, 4 * s, 5 * s, 16 * s, 14 * s);
                graphics.FillEllipse(brush, 7 * s, 8 * s, 3 * s, 3 * s);
                graphics.DrawLines(pen, new[] { new PointF(6 * s, 17 * s), new PointF(10 * s, 13 * s), new PointF(13 * s, 16 * s), new PointF(16 * s, 11 * s), new PointF(19 * s, 17 * s) });
                break;
            case UiIconKind.Audio:
                graphics.DrawLine(pen, 10 * s, 6 * s, 10 * s, 17 * s);
                graphics.DrawLine(pen, 10 * s, 6 * s, 18 * s, 4 * s);
                graphics.DrawLine(pen, 18 * s, 4 * s, 18 * s, 14 * s);
                graphics.FillEllipse(brush, 5 * s, 15 * s, 6 * s, 4.5f * s);
                graphics.FillEllipse(brush, 13 * s, 12 * s, 6 * s, 4.5f * s);
                break;
            case UiIconKind.Video:
                graphics.DrawRectangle(pen, 4 * s, 5 * s, 16 * s, 14 * s);
                graphics.FillPolygon(brush, new[] { new PointF(10 * s, 8 * s), new PointF(10 * s, 16 * s), new PointF(16 * s, 12 * s) });
                break;
            case UiIconKind.Pdf:
                DrawDocument(graphics, pen, 5 * s, 3 * s, 14 * s, 18 * s, s);
                using (Font font = new Font("Segoe UI", Math.Max(5.5f, 6.5f * s), FontStyle.Bold, GraphicsUnit.Point))
                {
                    graphics.DrawString("PDF", font, brush, 5.5f * s, 10.5f * s);
                }
                break;
            case UiIconKind.Document:
                DrawDocument(graphics, pen, 5 * s, 3 * s, 14 * s, 18 * s, s);
                graphics.DrawLine(pen, 8 * s, 12 * s, 16 * s, 12 * s);
                graphics.DrawLine(pen, 8 * s, 16 * s, 15 * s, 16 * s);
                break;
            case UiIconKind.Check:
                graphics.DrawLine(pen, 5 * s, 13 * s, 10 * s, 18 * s);
                graphics.DrawLine(pen, 10 * s, 18 * s, 20 * s, 6 * s);
                break;
            case UiIconKind.Warning:
                graphics.DrawPolygon(pen, new[] { new PointF(12 * s, 3 * s), new PointF(21 * s, 20 * s), new PointF(3 * s, 20 * s) });
                graphics.DrawLine(pen, 12 * s, 9 * s, 12 * s, 14 * s);
                graphics.FillEllipse(brush, 11 * s, 17 * s, 2 * s, 2 * s);
                break;
            case UiIconKind.More:
                graphics.FillEllipse(brush, 5 * s, 10.5f * s, 3 * s, 3 * s);
                graphics.FillEllipse(brush, 10.5f * s, 10.5f * s, 3 * s, 3 * s);
                graphics.FillEllipse(brush, 16 * s, 10.5f * s, 3 * s, 3 * s);
                break;
            case UiIconKind.Folder:
                DrawFolder(graphics, pen, 3 * s, 6 * s, 18 * s, 13 * s, s);
                break;
            case UiIconKind.Plus:
                DrawPlus(graphics, pen, 12 * s, 12 * s, 7 * s);
                break;
            case UiIconKind.Search:
                graphics.DrawEllipse(pen, 5 * s, 5 * s, 10 * s, 10 * s);
                graphics.DrawLine(pen, 14 * s, 14 * s, 20 * s, 20 * s);
                break;
        }

        return bitmap;
    }

    private static PointF[] LogoPoints(float s, float dx, float dy)
    {
        return new[]
        {
            new PointF(50 * s + dx, 34 * s + dy),
            new PointF(206 * s + dx, 34 * s + dy),
            new PointF(192 * s + dx, 75 * s + dy),
            new PointF(106 * s + dx, 75 * s + dy),
            new PointF(96 * s + dx, 116 * s + dy),
            new PointF(178 * s + dx, 116 * s + dy),
            new PointF(164 * s + dx, 154 * s + dy),
            new PointF(87 * s + dx, 154 * s + dy),
            new PointF(72 * s + dx, 222 * s + dy),
            new PointF(30 * s + dx, 222 * s + dy)
        };
    }

    private static void DrawPlus(Graphics graphics, Pen pen, float x, float y, float r)
    {
        graphics.DrawLine(pen, x - r, y, x + r, y);
        graphics.DrawLine(pen, x, y - r, x, y + r);
    }

    private static void DrawArrow(Graphics graphics, Pen pen, float x1, float y1, float x2, float y2, float s)
    {
        graphics.DrawLine(pen, x1, y1, x2, y2);
        float direction = x2 >= x1 ? 1 : -1;
        graphics.DrawLine(pen, x2, y2, x2 - direction * 4 * s, y2 - 4 * s);
        graphics.DrawLine(pen, x2, y2, x2 - direction * 4 * s, y2 + 4 * s);
    }

    private static void DrawDocument(Graphics graphics, Pen pen, float x, float y, float width, float height, float s)
    {
        using GraphicsPath path = new GraphicsPath();
        path.AddLine(x, y, x + width - 5 * s, y);
        path.AddLine(x + width - 5 * s, y, x + width, y + 5 * s);
        path.AddLine(x + width, y + 5 * s, x + width, y + height);
        path.AddLine(x + width, y + height, x, y + height);
        path.CloseFigure();
        graphics.DrawPath(pen, path);
        graphics.DrawLine(pen, x + width - 5 * s, y, x + width - 5 * s, y + 5 * s);
        graphics.DrawLine(pen, x + width - 5 * s, y + 5 * s, x + width, y + 5 * s);
    }

    private static void DrawFolder(Graphics graphics, Pen pen, float x, float y, float width, float height, float s)
    {
        using GraphicsPath path = new GraphicsPath();
        path.AddLine(x, y + 3 * s, x + 6 * s, y + 3 * s);
        path.AddLine(x + 6 * s, y + 3 * s, x + 8 * s, y);
        path.AddLine(x + 8 * s, y, x + width * 0.65f, y);
        path.AddLine(x + width * 0.65f, y, x + width * 0.78f, y + 3 * s);
        path.AddLine(x + width * 0.78f, y + 3 * s, x + width, y + 3 * s);
        path.AddLine(x + width, y + 3 * s, x + width, y + height);
        path.AddLine(x + width, y + height, x, y + height);
        path.CloseFigure();
        graphics.DrawPath(pen, path);
    }
}
