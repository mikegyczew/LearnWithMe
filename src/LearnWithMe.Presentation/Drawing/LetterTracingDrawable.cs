using Microsoft.Maui.Graphics;

namespace LearnWithMe.Presentation.Drawing;

public sealed class LetterTracingDrawable : IDrawable
{
    public string Letter { get; set; } = "?";

    public IReadOnlyList<PointF> Points { get; set; } = [];

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromArgb("#FFFDF9");
        canvas.FillRectangle(dirtyRect);

        var widthRatio = Letter.Length > 1 ? 0.48f : 0.8f;
        canvas.FontSize = Math.Min(dirtyRect.Height * 0.88f, dirtyRect.Width * widthRatio);
        canvas.FontColor = Color.FromArgb("#E6EDE8");
        canvas.DrawString(
            Letter,
            dirtyRect,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        canvas.StrokeColor = Color.FromArgb("#276749");
        canvas.StrokeSize = 7;
        canvas.StrokeLineCap = LineCap.Round;
        PointF? previousPoint = null;
        foreach (var point in Points)
        {
            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
            {
                previousPoint = null;
                continue;
            }

            if (previousPoint is not null)
            {
                canvas.DrawLine(previousPoint.Value, point);
            }

            previousPoint = point;
        }
    }
}
