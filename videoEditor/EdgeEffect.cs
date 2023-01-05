using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace videoEditor
{
	class EdgeEffect
	{
		// 輝度取得
		public static float GetLuminance(byte r, byte g, byte b)
		{
			return 0.299f * (float)r + 0.587f * (float)g + 0.114f * (float)b;
		}


		// 0～255の値を伸長する
		public static void SetLuminanceRate(byte[] pixcel, int index, float minPower, float maxPower)
		{
			var scale = 255.0f / (maxPower - minPower);

			pixcel[index] = (byte)Math.Min(255.0f, Math.Max(0.0f, (float)pixcel[index] - minPower) * scale);
			pixcel[index+1] = (byte)Math.Min(255.0f, Math.Max(0.0f, (float)pixcel[index+1] - minPower) * scale);
			pixcel[index+2] = (byte)Math.Min(255.0f, Math.Max(0.0f, (float)pixcel[index+2] - minPower) * scale);
		}

		public static void Effect(byte[] pixels, int stride, int width, int hight)
		{
			// 範囲内の最大、最小輝度を取得
			var minPower = 255.0f;
			var maxPower = 0.0f;
			for (int py = 0; py < hight; py++)
			{
				for (int px = 0; px < width; px++)
				{
					int pos = py * stride + px * 4;

					var cr = pixels[pos];
					var cg = pixels[pos + 1];
					var cb = pixels[pos + 2];
					var power = GetLuminance(cr, cg, cb);

					minPower = Math.Min(minPower, power);
					maxPower = Math.Max(maxPower, power);
				}
			}

			// 輝度を伸長する
			for (int py = 0; py < hight; py++)
			{
				for (int px = 0; px < width; px++)
				{
					int pos = py * stride + px * 4;
					SetLuminanceRate(pixels, pos, minPower, maxPower);
				}
			}
		}
	}
}
