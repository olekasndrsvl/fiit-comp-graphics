
using System.Drawing;

namespace Lab6.Rendering;

internal readonly record struct ScreenPoint(float X, float Y, float Z);

public sealed class ZBuffer
{
	private float minDepth;
	private float maxDepth;

	private float[,]? depthBuffer;
	private Color[,]? colorBuffer;

	public void Initialize(Size viewport, float minDepth = 0, float maxDepth = 1)
	{
		this.minDepth = minDepth;
		this.maxDepth = maxDepth;

		depthBuffer = new float[viewport.Width, viewport.Height];
		colorBuffer = new Color[viewport.Width, viewport.Height];

		for (int x = 0; x < viewport.Width; x++)
		{
			for (int y = 0; y < viewport.Height; y++)
			{
				depthBuffer[x, y] = float.PositiveInfinity;
				colorBuffer[x, y] = Color.White;
			}
		}
	}

	private Color GetDepthColor(float depth)
	{
		float range = maxDepth - minDepth;

		float normalizedDepth = range > 1e-6f
			? (depth - minDepth) / range
			: 0.5f;

		float intensity = 1f - Math.Clamp(normalizedDepth, 0f, 1f);

		int channel = (int)(25 + intensity * 230);

		return Color.FromArgb(channel, channel, channel);
	}

	internal void RasterizePolygon(
		IReadOnlyList<ScreenPoint> vertices,
		Size viewport)
	{
		if (vertices.Count < 3 || depthBuffer == null || colorBuffer == null)
			return;

		for (int i = 1; i < vertices.Count - 1; i++)
		{
			RasterizeTriangle(
				vertices[0],
				vertices[i],
				vertices[i + 1],
				viewport);
		}
	}

	private void RasterizeTriangle(
		ScreenPoint a,
		ScreenPoint b,
		ScreenPoint c,
		Size viewport)
	{
		float area =
			(b.X - a.X) * (c.Y - a.Y) -
			(b.Y - a.Y) * (c.X - a.X);

		if (Math.Abs(area) < 1e-6f)
			return;

		int minX = Math.Max(0,
			(int)Math.Floor(Math.Min(a.X, Math.Min(b.X, c.X))));

		int maxX = Math.Min(viewport.Width - 1,
			(int)Math.Ceiling(Math.Max(a.X, Math.Max(b.X, c.X))));

		int minY = Math.Max(0,
			(int)Math.Floor(Math.Min(a.Y, Math.Min(b.Y, c.Y))));

		int maxY = Math.Min(viewport.Height - 1,
			(int)Math.Ceiling(Math.Max(a.Y, Math.Max(b.Y, c.Y))));

		for (int y = minY; y <= maxY; y++)
		{
			for (int x = minX; x <= maxX; x++)
			{
				float px = x + 0.5f;
				float py = y + 0.5f;

				float w1 =
					((b.X - px) * (c.Y - py) -
					 (b.Y - py) * (c.X - px)) / area;

				float w2 =
					((c.X - px) * (a.Y - py) -
					 (c.Y - py) * (a.X - px)) / area;

				float w3 = 1f - w1 - w2;

				if (w1 < 0 || w2 < 0 || w3 < 0)
					continue;

				float z = w1 * a.Z + w2 * b.Z + w3 * c.Z;

				if (z < depthBuffer![x, y])
				{
					depthBuffer[x, y] = z;
					colorBuffer![x, y] = GetDepthColor(z);
				}
			}
		}
	}

	public void Draw(Graphics graphics, Size viewport)
	{
		if (colorBuffer == null)
			return;

		using var bitmap = new Bitmap(viewport.Width, viewport.Height);

		for (int x = 0; x < viewport.Width; x++)
		{
			for (int y = 0; y < viewport.Height; y++)
			{
				bitmap.SetPixel(x, y, colorBuffer[x, y]);
			}
		}

		graphics.DrawImageUnscaled(bitmap, 0, 0);
	}
}
