using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Enums;
using Terraria.GameContent.Metadata;

namespace apogean.Content.Tiles
{
	public sealed class DeadTuft : ModTile
	{
		public override void SetStaticDefaults()
		{
			WastesPlantRegistration.ApplyCommon(this, new Color(121, 91, 54), sways: true);

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.Origin = Point16.Zero;
			TileObjectData.newTile.AnchorBottom = WastesPlantRegistration.GroundAnchor(1);
			TileObjectData.newTile.AnchorValidTiles = WastesPlantRegistration.ValidGround();
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 4;
			TileObjectData.newTile.RandomStyleRange = 4;
			TileObjectData.newTile.DrawFlipHorizontal = true;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.addTile(Type);
		}
	}
}
