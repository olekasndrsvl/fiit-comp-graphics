
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Lab6.Rendering;

internal readonly record struct ScreenPoint(float X, float Y, float Z);

public sealed class ZBuffer
{
	private float minDepth;
	private float maxDepth;

	private float[]? depthBuffer;
	private int[]? colorBuffer;

	private int width;
	private int height;

	public void Initialize(Size viewport, float minDepth, float maxDepth)
	{
		width = viewport.Width;
		height = viewport.Height;

		this.minDepth = minDepth;
		this.maxDepth = maxDepth;

		int bufferSize = width * height;

		if (depthBuffer == null || depthBuffer.Length != bufferSize)
		{
			depthBuffer = new float[bufferSize];
			colorBuffer = new int[bufferSize];
		}

		Array.Fill(depthBuffer, float.PositiveInfinity);
		Array.Fill(colorBuffer!, Color.White.ToArgb());
	}

	private int GetDepthColor(float depth)
	{
		float range = maxDepth - minDepth;

		float normalizedDepth = range > 1e-6f
			? (depth - minDepth) / range
			: 0.5f;

		float intensity = 1f - Math.Clamp(normalizedDepth, 0f, 1f);

		int channel = (int)(25 + intensity * 230);

		return Color.FromArgb(channel, channel, channel).ToArgb();
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

		float inverseArea = 1f / area;

		float w1StepX = -(c.Y - b.Y) * inverseArea;
		float w2StepX = -(a.Y - c.Y) * inverseArea;
		float w3StepX = -(b.Y - a.Y) * inverseArea;

		float w1StepY = (c.X - b.X) * inverseArea;
		float w2StepY = (a.X - c.X) * inverseArea;
		float w3StepY = (b.X - a.X) * inverseArea;

		for (int y = minY; y <= maxY; y++)
		{
			float py = y + 0.5f;
			float px = minX + 0.5f;

			float w1 =
				((b.X - px) * (c.Y - py) -
				 (b.Y - py) * (c.X - px)) * inverseArea;

			float w2 =
				((c.X - px) * (a.Y - py) -
				 (c.Y - py) * (a.X - px)) * inverseArea;

			float w3 = 1f - w1 - w2;

			int rowOffset = y * width;

			for (int x = minX; x <= maxX; x++)
			{
				if (w1 >= 0 && w2 >= 0 && w3 >= 0)
				{
					float z = w1 * a.Z + w2 * b.Z + w3 * c.Z;

					int index = rowOffset + x;

					if (z < depthBuffer![index])
					{
						depthBuffer[index] = z;
						colorBuffer![index] = GetDepthColor(z);
					}
				}

				w1 += w1StepX;
				w2 += w2StepX;
				w3 += w3StepX;
			}
		}
	}


	public void Draw(Graphics graphics, Size viewport)
	{
		if (colorBuffer == null)
			return;

		using var bitmap = new Bitmap(
			width,
			height,
			PixelFormat.Format32bppArgb);

		var rectangle = new Rectangle(0, 0, width, height);

		BitmapData bitmapData = bitmap.LockBits(
			rectangle,
			ImageLockMode.WriteOnly,
			PixelFormat.Format32bppArgb);

		try
		{
			int stride = bitmapData.Stride;

			if (stride == width * sizeof(int))
			{
				Marshal.Copy(
					colorBuffer,
					0,
					bitmapData.Scan0,
					colorBuffer.Length);
			}
			else
			{
				for (int y = 0; y < height; y++)
				{
					IntPtr row = IntPtr.Add(
						bitmapData.Scan0,
						y * stride);

					Marshal.Copy(
						colorBuffer,
						y * width,
						row,
						width);
				}
			}
		}
		finally
		{
			bitmap.UnlockBits(bitmapData);
		}

		graphics.DrawImageUnscaled(bitmap, 0, 0);
	}
}
