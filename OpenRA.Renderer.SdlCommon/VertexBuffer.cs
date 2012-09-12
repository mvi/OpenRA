﻿#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Runtime.InteropServices;
using OpenRA.FileFormats.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenRA.Renderer.SdlCommon
{
	public class VertexBuffer<T> : IVertexBuffer<T>
			where T : struct
	{
		int buffer;
		static readonly int vertexSize = Marshal.SizeOf(typeof(T));

		public VertexBuffer(int size)
		{
			GL.Arb.GenBuffers(1, out buffer);
			ErrorHandler.CheckGlError();
			Bind();
			GL.Arb.BufferData(BufferTargetArb.ArrayBuffer,
				new IntPtr(vertexSize * size),
				new T[ size ],
				BufferUsageArb.DynamicDraw);
			ErrorHandler.CheckGlError();
		}

		public void SetData(T[] data, int length)
		{
			Bind();
			GL.Arb.BufferSubData(BufferTargetArb.ArrayBuffer,
				IntPtr.Zero,
				new IntPtr(vertexSize * length),
				data);
			ErrorHandler.CheckGlError();
		}

		public void Bind()
		{
			GL.Arb.BindBuffer(BufferTargetArb.ArrayBuffer, buffer);
			ErrorHandler.CheckGlError();
			GL.VertexPointer(3, VertexPointerType.Float, vertexSize, IntPtr.Zero);
			ErrorHandler.CheckGlError();
			GL.TexCoordPointer(4, TexCoordPointerType.Float, vertexSize, new IntPtr(12));
			ErrorHandler.CheckGlError();
		}

		void FinalizeInner() { GL.Arb.DeleteBuffers( 1, ref buffer ); }
		~VertexBuffer() { Game.RunAfterTick( FinalizeInner ); }
	}
}
