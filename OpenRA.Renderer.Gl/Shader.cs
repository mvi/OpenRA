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
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenRA.FileFormats;
using OpenRA.FileFormats.Graphics;
using OpenRA.Renderer.SdlCommon;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Compatibility;

namespace OpenRA.Renderer.Glsl
{
	public class Shader : IShader
	{
		int program;
		readonly Dictionary<string, int> samplers = new Dictionary<string, int>();

		public Shader (GraphicsDevice dev, string type)
		{
			// Vertex shader
			string vertexCode;
			using (var file = new StreamReader(FileSystem.Open("glsl{0}{1}.vert".F(Path.DirectorySeparatorChar, type))))
				vertexCode = file.ReadToEnd ();

			int v = GL.CreateShader(ShaderType.VertexShader);
			ErrorHandler.CheckGlError ();
			unsafe {
				GL.Arb.ShaderSource (v, 1, new string[]{vertexCode}, null);
			}
			ErrorHandler.CheckGlError ();
			GL.Arb.CompileShader (v);
			ErrorHandler.CheckGlError ();

			int success;
			GL.Arb.GetObjectParameter (v, ArbShaderObjects.ObjectCompileStatusArb, out success);
			ErrorHandler.CheckGlError ();
			if (success == 0)
				throw new InvalidProgramException ("Compile error in {0}{1}.vert".F (Path.DirectorySeparatorChar, type));

			// Fragment shader
			string fragmentCode;
			using (var file = new StreamReader(FileSystem.Open("glsl{0}{1}.frag".F(Path.DirectorySeparatorChar, type))))
				fragmentCode = file.ReadToEnd ();
			int f = GL.CreateShader(ShaderType.FragmentShader);
			ErrorHandler.CheckGlError ();
			unsafe { GL.Arb.ShaderSource (f, 1, new string[]{fragmentCode}, null); }
			ErrorHandler.CheckGlError();
			GL.Arb.CompileShader(f);
			ErrorHandler.CheckGlError();

			GL.Arb.GetObjectParameter(f, ArbShaderObjects.ObjectCompileStatusArb, out success);
			ErrorHandler.CheckGlError();
			if (success == 0)
				throw new InvalidProgramException("Compile error in glsl{0}{1}.frag".F(Path.DirectorySeparatorChar, type));


			// Assemble program
			program = GL.Arb.CreateProgramObject();
			ErrorHandler.CheckGlError();
			GL.Arb.AttachObject(program,v);
			ErrorHandler.CheckGlError();
			GL.Arb.AttachObject(program,f);
			ErrorHandler.CheckGlError();

			GL.Arb.LinkProgram(program);
			ErrorHandler.CheckGlError();

			GL.Arb.GetObjectParameter(program, ArbShaderObjects.ObjectLinkStatusArb, out success);
			ErrorHandler.CheckGlError();
			if (success == 0)
				throw new InvalidProgramException("Linking error in {0} shader".F(type));


			GL.Arb.UseProgramObject(program);
			ErrorHandler.CheckGlError();

			int numUniforms;
			GL.Arb.GetObjectParameter( program, ArbShaderObjects.ObjectActiveUniformsArb, out numUniforms );
			ErrorHandler.CheckGlError();

			int nextTexUnit = 1;
			for( int i = 0 ; i < numUniforms ; i++ )
			{
				int uLen, uSize, loc;
				OpenTK.Graphics.OpenGL.ArbShaderObjects uType;
				var sb = new StringBuilder(128);
				GL.Arb.GetActiveUniform( program, i, 128, out uLen, out uSize, out uType, sb );
				var sampler = sb.ToString();
				ErrorHandler.CheckGlError();
				if( uType == ArbShaderObjects.Sampler2DArb )
				{
					samplers.Add( sampler, nextTexUnit );
					loc = GL.Arb.GetUniformLocation(program, sampler);
					ErrorHandler.CheckGlError();
					GL.Arb.Uniform1( loc, nextTexUnit );
					ErrorHandler.CheckGlError();
					++nextTexUnit;
				}
			}
		}

		public void Render(Action a)
		{
			GL.Arb.UseProgramObject(program);
			ErrorHandler.CheckGlError();
			// Todo: Only enable alpha blending if we need it
			GL.Enable(EnableCap.Blend);
			ErrorHandler.CheckGlError();
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			ErrorHandler.CheckGlError();
			a();
			ErrorHandler.CheckGlError();
			GL.Disable(EnableCap.Blend);
			ErrorHandler.CheckGlError();
		}

		public void SetValue(string name, ITexture t)
		{
			if( t == null ) return;
			GL.Arb.UseProgramObject(program);
			ErrorHandler.CheckGlError();
			var texture = (Texture)t;
			int texUnit;
			if( samplers.TryGetValue( name, out texUnit ) )
			{
				GL.Arb.ActiveTexture( TextureUnit.Texture0 + texUnit );
				ErrorHandler.CheckGlError();
				GL.BindTexture( TextureTarget.Texture2D, texture.texture );
				ErrorHandler.CheckGlError();
				GL.Arb.ActiveTexture( TextureUnit.Texture0 );
			}
		}

		public void SetValue(string name, float x, float y)
		{
			GL.Arb.UseProgramObject(program);
			ErrorHandler.CheckGlError();
			int param = GL.Arb.GetUniformLocation(program, name);
			ErrorHandler.CheckGlError();
			GL.Arb.Uniform2(param,x,y);
			ErrorHandler.CheckGlError();
		}
	}
}
