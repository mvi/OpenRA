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
using OpenRA.FileFormats;
using OpenRA.Mods.RA.Air;
using OpenRA.Traits;

namespace OpenRA.Mods.RA
{
	class IceWoodsShellmapScriptInfo : TraitInfo<IceWoodsShellmapScript> { }

	class IceWoodsShellmapScript: IWorldLoaded, ITick
	{
		Dictionary<string, Actor> Actors;
		static int2 ViewportOrigin;

		public void WorldLoaded(World w)
		{
			var b = w.Map.Bounds;
			ViewportOrigin = new int2(b.Left + b.Width/2, b.Top + b.Height/2);
			Game.MoveViewport(ViewportOrigin);

			Actors = w.WorldActor.Trait<SpawnMapActors>().Actors;
			Sound.SoundVolumeModifier = 0.25f;
		}

		int ticks = 0;
		float speed = 8f;

		public void Tick(Actor self)
		{
			var loc = new float2(
				(float)(System.Math.Sin((ticks + 45) % (360f * speed) * (Math.PI / 180) * 1f / speed) * 20f + ViewportOrigin.X),
				(float)(System.Math.Cos((ticks + 45) % (360f * speed) * (Math.PI / 180) * 1f / speed) * 30f + ViewportOrigin.Y));

			Game.MoveViewport(loc);

			ticks++;
		}
	}
}
