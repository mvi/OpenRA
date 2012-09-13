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
using System.Linq;
using System.Runtime.InteropServices;
using OpenRA.FileFormats.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Tao.Sdl;

namespace OpenRA.Renderer.SdlCommon
{
	public static class SdlGraphics
	{
		public static IntPtr InitializeSdlGl( ref Size size, WindowMode window, string[] requiredExtensions )
		{
			Sdl.SDL_Init( Sdl.SDL_INIT_NOPARACHUTE | Sdl.SDL_INIT_VIDEO );
			Sdl.SDL_GL_SetAttribute( Sdl.SDL_GL_DOUBLEBUFFER, 1 );
			Sdl.SDL_GL_SetAttribute( Sdl.SDL_GL_RED_SIZE, 8 );
			Sdl.SDL_GL_SetAttribute( Sdl.SDL_GL_GREEN_SIZE, 8 );
			Sdl.SDL_GL_SetAttribute( Sdl.SDL_GL_BLUE_SIZE, 8 );
			Sdl.SDL_GL_SetAttribute( Sdl.SDL_GL_ALPHA_SIZE, 0 );

			int windowFlags = 0;
			switch( window )
			{
			case WindowMode.Fullscreen:
				windowFlags |= Sdl.SDL_FULLSCREEN;
				break;
			case WindowMode.PseudoFullscreen:
				windowFlags |= Sdl.SDL_NOFRAME;
				Environment.SetEnvironmentVariable( "SDL_VIDEO_WINDOW_POS", "0,0" );
				break;
			case WindowMode.Windowed:
				Environment.SetEnvironmentVariable( "SDL_VIDEO_CENTERED", "1" );
				break;
			default:
				break;
			}

			var info = (Sdl.SDL_VideoInfo) Marshal.PtrToStructure(
				Sdl.SDL_GetVideoInfo(), typeof(Sdl.SDL_VideoInfo));
			Console.WriteLine("Desktop resolution: {0}x{1}",
				info.current_w, info.current_h);

			if (size.Width == 0 && size.Height == 0)
			{
				Console.WriteLine("No custom resolution provided, using desktop resolution");
				size = new Size( info.current_w, info.current_h );
			}

			Console.WriteLine("Using resolution: {0}x{1}", size.Width, size.Height);

			var surf = Sdl.SDL_SetVideoMode( size.Width, size.Height, 0, Sdl.SDL_OPENGL | windowFlags );
			if (surf == IntPtr.Zero)
				Console.WriteLine("Failed to set video mode.");

			OpenTK.Graphics.GraphicsContext dummyContext = OpenTK.Graphics.GraphicsContext.CreateDummyContext(new OpenTK.ContextHandle(surf));
			OpenTK.Platform.IWindowInfo windowInfo = OpenTK.Platform.Utilities.CreateDummyWindowInfo();
			dummyContext.MakeCurrent(windowInfo);
			OpenTK.Graphics.OpenGL.GL.LoadAll();

			Sdl.SDL_WM_SetCaption( "OpenRA", "OpenRA" );
			Sdl.SDL_ShowCursor( 0 );
			Sdl.SDL_EnableUNICODE( 1 );
			Sdl.SDL_EnableKeyRepeat( Sdl.SDL_DEFAULT_REPEAT_DELAY, Sdl.SDL_DEFAULT_REPEAT_INTERVAL );

			ErrorHandler.CheckGlError();

			var extensions = GL.GetString(StringName.Extensions);
			if (extensions == null)
				Console.WriteLine("Failed to fetch GL_EXTENSIONS, this is bad.");

			var missingExtensions = requiredExtensions.Where(r => !extensions.Contains(r)).ToArray();

			if (missingExtensions.Any())
			{
				ErrorHandler.WriteGraphicsLog("Unsupported GPU: Missing extensions: {0}"
					.F(missingExtensions.JoinWith(",")));
				throw new InvalidProgramException("Unsupported GPU. See graphics.log for details.");
			}

			return surf;
		}

		static BeginMode ModeFromPrimitiveType(PrimitiveType pt)
		{
			switch(pt)
			{
			case PrimitiveType.PointList: return BeginMode.Points;
			case PrimitiveType.LineList: return BeginMode.Lines;
			case PrimitiveType.TriangleList: return BeginMode.Triangles;
			case PrimitiveType.QuadList: return BeginMode.Quads;
			}
			throw new NotImplementedException();
		}

		public static void DrawPrimitives(PrimitiveType pt, int firstVertex, int numVertices)
		{
			GL.DrawArrays(ModeFromPrimitiveType(pt), firstVertex, numVertices);
			ErrorHandler.CheckGlError();
		}

		public static void Clear()
		{
			GL.ClearColor(0, 0, 0, 0);
			ErrorHandler.CheckGlError();
			GL.Clear(ClearBufferMask.ColorBufferBit);
			ErrorHandler.CheckGlError();
		}
	}
}

