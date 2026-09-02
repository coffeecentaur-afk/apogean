using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace apogean.Content.Tiles
{
	public abstract class ProtectedCorporateFurniture : ModTile
	{
		protected abstract Color MapColor { get; }
		protected abstract int TileDust { get; }

		protected void ApplySharedDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			DustType = TileDust;
			AddMapEntry(MapColor);
		}

		public override bool CanExplode(int i, int j) => false;
		public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
	}

	public abstract class CorporatePlatform : ProtectedCorporateFurniture
	{
		public override void SetStaticDefaults()
		{
			ApplySharedDefaults();
			Main.tileSolidTop[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileTable[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.Platforms[Type] = true;
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
			AdjTiles = [TileID.Platforms];

			TileObjectData.newTile.CoordinateHeights = [16];
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 27;
			TileObjectData.newTile.StyleWrapLimit = 27;
			TileObjectData.newTile.UsesCustomCanPlace = false;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.addTile(Type);
		}

		public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;
	}

	public abstract class CorporateChair : ProtectedCorporateFurniture
	{
		public override void SetStaticDefaults()
		{
			ApplySharedDefaults();
			TileID.Sets.CanBeSatOnForNPCs[Type] = true;
			TileID.Sets.CanBeSatOnForPlayers[Type] = true;
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
			AdjTiles = [TileID.Chairs];

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.addAlternate(1);
			TileObjectData.addTile(Type);
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) =>
			settings.player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance);

		public override bool RightClick(int i, int j)
		{
			Player player = Main.LocalPlayer;
			if (player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
			{
				player.GamepadEnableGrappleCooldown();
				player.sitting.SitDown(player, i, j);
			}
			return true;
		}

		public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			info.TargetDirection = tile.TileFrameX == 0 ? -1 : 1;
			info.AnchorTilePosition.X = i;
			info.AnchorTilePosition.Y = j + (tile.TileFrameY % 40 == 0 ? 1 : 0);
		}
	}

	public abstract class CorporateTable : ProtectedCorporateFurniture
	{
		public override void SetStaticDefaults()
		{
			ApplySharedDefaults();
			Main.tileTable[Type] = true;
			Main.tileSolidTop[Type] = true;
			TileID.Sets.IgnoredByNpcStepUp[Type] = true;
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
			AdjTiles = [TileID.Tables];
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			TileObjectData.addTile(Type);
		}
	}

	public abstract class CorporateWorkbench : ProtectedCorporateFurniture
	{
		public override void SetStaticDefaults()
		{
			ApplySharedDefaults();
			Main.tileTable[Type] = true;
			Main.tileSolidTop[Type] = true;
			TileID.Sets.IgnoredByNpcStepUp[Type] = true;
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
			AdjTiles = [TileID.WorkBenches];
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
			TileObjectData.newTile.CoordinateHeights = [18];
			TileObjectData.addTile(Type);
		}
	}

	public abstract class CorporateLight : ProtectedCorporateFurniture
	{
		protected abstract Color EmittedLight { get; }

		public override void SetStaticDefaults()
		{
			ApplySharedDefaults();
			Main.tileLighted[Type] = true;
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.AnchorWall = true;
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.newTile.AnchorTop = AnchorData.Empty;
			TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
			TileObjectData.newTile.AnchorRight = AnchorData.Empty;
			TileObjectData.addTile(Type);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = EmittedLight.R / 255f * 0.72f;
			g = EmittedLight.G / 255f * 0.72f;
			b = EmittedLight.B / 255f * 0.72f;
		}
	}

	public abstract class CorporateConsole : ProtectedCorporateFurniture
	{
		protected abstract Color EmittedLight { get; }

		public override void SetStaticDefaults()
		{
			ApplySharedDefaults();
			Main.tileLighted[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			TileObjectData.addTile(Type);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = EmittedLight.R / 255f * 0.36f;
			g = EmittedLight.G / 255f * 0.36f;
			b = EmittedLight.B / 255f * 0.36f;
		}
	}

	public abstract class CorporateLocker : ProtectedCorporateFurniture
	{
		public override void SetStaticDefaults()
		{
			ApplySharedDefaults();
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(0, 2);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.addTile(Type);
		}
	}

	public sealed class KesslerPlatform : CorporatePlatform { protected override Color MapColor => new(99, 76, 61); protected override int TileDust => DustID.Titanium; }
	public sealed class HelixPlatform : CorporatePlatform { protected override Color MapColor => new(164, 174, 168); protected override int TileDust => DustID.SilverCoin; }
	public sealed class SentrixPlatform : CorporatePlatform { protected override Color MapColor => new(44, 91, 111); protected override int TileDust => DustID.Electric; }

	public sealed class KesslerChair : CorporateChair { protected override Color MapColor => new(112, 72, 48); protected override int TileDust => DustID.Titanium; }
	public sealed class HelixChair : CorporateChair { protected override Color MapColor => new(170, 181, 174); protected override int TileDust => DustID.SilverCoin; }
	public sealed class SentrixChair : CorporateChair { protected override Color MapColor => new(42, 94, 118); protected override int TileDust => DustID.Electric; }

	public sealed class KesslerTable : CorporateTable { protected override Color MapColor => new(104, 77, 58); protected override int TileDust => DustID.Titanium; }
	public sealed class HelixTable : CorporateTable { protected override Color MapColor => new(164, 176, 169); protected override int TileDust => DustID.SilverCoin; }
	public sealed class SentrixTable : CorporateTable { protected override Color MapColor => new(39, 88, 111); protected override int TileDust => DustID.Electric; }

	public sealed class KesslerWorkbench : CorporateWorkbench { protected override Color MapColor => new(111, 72, 49); protected override int TileDust => DustID.Titanium; }
	public sealed class HelixWorkbench : CorporateWorkbench { protected override Color MapColor => new(170, 183, 173); protected override int TileDust => DustID.SilverCoin; }
	public sealed class SentrixWorkbench : CorporateWorkbench { protected override Color MapColor => new(43, 98, 121); protected override int TileDust => DustID.Electric; }

	public sealed class KesslerLight : CorporateLight { protected override Color MapColor => new(180, 80, 36); protected override int TileDust => DustID.Titanium; protected override Color EmittedLight => new(255, 92, 34); }
	public sealed class HelixLight : CorporateLight { protected override Color MapColor => new(100, 196, 108); protected override int TileDust => DustID.SilverCoin; protected override Color EmittedLight => new(120, 255, 132); }
	public sealed class SentrixLight : CorporateLight { protected override Color MapColor => new(55, 171, 218); protected override int TileDust => DustID.Electric; protected override Color EmittedLight => new(80, 205, 255); }

	public sealed class KesslerConsole : CorporateConsole { protected override Color MapColor => new(132, 67, 45); protected override int TileDust => DustID.Titanium; protected override Color EmittedLight => new(255, 70, 30); }
	public sealed class HelixConsole : CorporateConsole { protected override Color MapColor => new(112, 177, 119); protected override int TileDust => DustID.SilverCoin; protected override Color EmittedLight => new(105, 232, 116); }
	public sealed class SentrixConsole : CorporateConsole { protected override Color MapColor => new(48, 121, 151); protected override int TileDust => DustID.Electric; protected override Color EmittedLight => new(74, 197, 242); }

	public sealed class KesslerLocker : CorporateLocker { protected override Color MapColor => new(96, 70, 57); protected override int TileDust => DustID.Titanium; }
	public sealed class HelixLocker : CorporateLocker { protected override Color MapColor => new(153, 167, 158); protected override int TileDust => DustID.SilverCoin; }
	public sealed class SentrixLocker : CorporateLocker { protected override Color MapColor => new(38, 80, 101); protected override int TileDust => DustID.Electric; }
}
