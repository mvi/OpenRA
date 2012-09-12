#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenRA.FileFormats;
using OpenRA.FileFormats.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenRA.Renderer.SdlCommon
{
	public class Texture : ITexture
	{
		public int texture;		/* temp: can be internal again once shaders are in shared code */

		public Texture()
		{
			GL.GenTextures(1, out texture);
			ErrorHandler.CheckGlError();
		}

		public Texture(Bitmap bitmap)
		{
			GL.GenTextures(1, out texture);
			ErrorHandler.CheckGlError();
			SetData(bitmap);
		}

		void FinalizeInner() { GL.DeleteTextures(1, ref texture); }
		~Texture() { Game.RunAfterTick(FinalizeInner); }

		void PrepareTexture()
		{
			ErrorHandler.CheckGlError();
			GL.BindTexture(TextureTarget.Texture2D, texture);
			ErrorHandler.CheckGlError();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMinFilter.Nearest);
			ErrorHandler.CheckGlError();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
			ErrorHandler.CheckGlError();

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
			ErrorHandler.CheckGlError();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
			ErrorHandler.CheckGlError();
		}

		public void SetData(byte[] colors, int width, int height)
		{
			if (!IsPowerOf2(width) || !IsPowerOf2(height))
				throw new InvalidDataException("Non-power-of-two array {0}x{1}".F(width, height));

			unsafe
			{
				fixed (byte* ptr = &colors[0])
				{
					IntPtr intPtr = new IntPtr((void*)ptr);
					PrepareTexture();
					GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height,
						0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, intPtr);
					ErrorHandler.CheckGlError();
				}
			}
		}

		// An array of RGBA
		public void SetData(uint[,] colors)
		{
			int width = colors.GetUpperBound(1) + 1;
			int height = colors.GetUpperBound(0) + 1;

			if (!IsPowerOf2(width) || !IsPowerOf2(height))
				throw new InvalidDataException("Non-power-of-two array {0}x{1}".F(width,height));

			unsafe
			{
				fixed (uint* ptr = &colors[0,0])
				{
					IntPtr intPtr = new IntPtr((void *) ptr);
					PrepareTexture();
					GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height,
						0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, intPtr);
					ErrorHandler.CheckGlError();
				}
			}
		}

		public void SetData(Bitmap bitmap)
		{
			if (!IsPowerOf2(bitmap.Width) || !IsPowerOf2(bitmap.Height))
			{
				//throw new InvalidOperationException( "non-power-of-2-texture" );
				bitmap = new Bitmap(bitmap, bitmap.Size.NextPowerOf2());
			}

			var bits = bitmap.LockBits(bitmap.Bounds(),
				ImageLockMode.ReadOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			PrepareTexture();
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bits.Width, bits.Height,
				0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bits.Scan0);        // todo: weird strides
			ErrorHandler.CheckGlError();
			bitmap.UnlockBits(bits);
		}

		bool IsPowerOf2(int v)
		{
			return (v & (v - 1)) == 0;
		}
	}
}
