#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Mods.RA.Activities;
using OpenRA.Mods.RA.Buildings;
using OpenRA.Mods.RA.Orders;
using OpenRA.Mods.RA.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.RA
{
	class TransformsInfo : ITraitInfo
	{
		/// <summary>
		/// Defines in what actor to transform
		/// </summary>
		[ActorReference] public readonly string IntoActor = null;
		/// <summary>
		/// X, Y offset
		/// </summary>
		public readonly int2 Offset = int2.Zero;
		/// <summary>
		/// Facing before transforming animation
		/// </summary>
		public readonly int Facing = 96;
		/// <summary>
		/// *.aud file during successful transformation
		/// </summary>
		public readonly string[] TransformSounds = {};
		/// <summary>
		/// *.aud file when transform is impossible
		/// </summary>
		public readonly string[] NoTransformSounds = {};

		public virtual object Create(ActorInitializer init) { return new Transforms(init.self, this); }
	}

	class Transforms : IIssueOrder, IResolveOrder, IOrderVoice
	{
		Actor self;
		TransformsInfo Info;
		BuildingInfo bi;

		/// <summary>
		/// If present, the unit is able to transform
		/// </summary>
		public Transforms(Actor self, TransformsInfo info)
		{
			this.self = self;
			Info = info;
			bi = Rules.Info[info.IntoActor].Traits.GetOrDefault<BuildingInfo>();
		}

		public string VoicePhraseForOrder(Actor self, Order order)
		{
			return (order.OrderString == "DeployTransform") ? "Move" : null;
		}

		bool CanDeploy()
		{
			return (bi == null || self.World.CanPlaceBuilding(Info.IntoActor, bi, self.Location + (CVec)Info.Offset, self));
		}

		public IEnumerable<IOrderTargeter> Orders
		{
			get { yield return new DeployOrderTargeter( "DeployTransform", 5, () => CanDeploy() ); }
		}

		public Order IssueOrder( Actor self, IOrderTargeter order, Target target, bool queued )
		{
			if( order.OrderID == "DeployTransform" )
				return new Order( order.OrderID, self, queued );

			return null;
		}

		public void ResolveOrder( Actor self, Order order )
		{
			if (order.OrderString == "DeployTransform")
			{
				if (!CanDeploy())
				{
					foreach (var s in Info.NoTransformSounds)
						Sound.PlayToPlayer(self.Owner, s);
					return;
				}
				self.CancelActivity();

				if (self.HasTrait<IFacing>())
					self.QueueActivity(new Turn(Info.Facing));

				var rb = self.TraitOrDefault<RenderBuilding>();
				if (rb != null && self.Info.Traits.Get<RenderBuildingInfo>().HasMakeAnimation)
					self.QueueActivity(new MakeAnimation(self, true, () => rb.PlayCustomAnim(self, "make")));

				self.QueueActivity(new Transform(self, Info.IntoActor) { Offset = (CVec)Info.Offset, Facing = Info.Facing, Sounds = Info.TransformSounds });
			}
		}
	}
}
