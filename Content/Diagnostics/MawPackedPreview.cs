using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
    // Explicit QA build switch, default OFF. Reuses the same registered assets
    // and maps rather than duplicating 246MiB of study atlases into shipping.
    internal static class MawPackedPreview
    {
        internal static bool Enabled {
            get {
#if APOGEAN_MAW_PACKED_QA
                return true;
#else
                return false;
#endif
            }
        }
        internal static string TileKey(string name) => name switch {
            "MawDirt"=>"soil", "Mawstone"=>"stone", "MawGrass"=>"grass",
            "EngraftTurf"=>"grass", "MawSand"=>"sand", "MawMud"=>"mud",
            "MawClay"=>"clay", "MawSnow"=>"snow", "MawIce"=>"ice",
            "OssuaryBone"=>"bone", _=>null
        };
        internal static string WallKey(string name) => name switch {
            "MawDirtWallUnsafe" or "MawWallUnsafe"=>"soil", "MawStoneWallUnsafe"=>"stone",
            "MawGrassWallUnsafe"=>"grass", "MawSandWallUnsafe"=>"sand",
            "MawMudWallUnsafe"=>"mud", "MawSnowWallUnsafe"=>"snow", "MawIceWallUnsafe"=>"ice", _=>null
        };
        internal static string Texture(string key, bool wall, string fallback) => Enabled && key!=null ?
            "apogean/Content/Diagnostics/Materials/"+key+(wall?"/Wall":"/Tile") : fallback;
        internal static void TileDraw(string key,int i,int j,ref short fx,ref short fy)
        {
            if(Enabled && key!=null && MawTerrainStudies.Tile(key).Map.TryMap(i,j,fx,fy,out short x,out short y)){fx=x;fy=y;}
        }
        internal static bool WallDraw(string key,int type,int i,int j,SpriteBatch batch)
        {
            if(!Enabled || key==null)return true;
            Tile tile=Main.tile[i,j]; var map=MawTerrainStudies.Wall(key).Map;
            if(!map.TryMap(i,j,tile.WallFrameX,tile.WallFrameY,out short x,out short y))return true;
            Texture2D texture=TextureAssets.Wall[type].Value;
            if(tile.WallColor!=PaintID.None)texture=Main.instance.TilePaintSystem.TryGetWallAndRequestIfNotReady(type,tile.WallColor)??texture;
            Color color=tile.IsWallFullbright?Color.White:Lighting.GetColor(i,j);
            Vector2 offset=Main.drawToScreen?Vector2.Zero:new Vector2(Main.offScreenRange);
            batch.Draw(texture,new Vector2(i*16-8,j*16-8)-Main.screenPosition+offset,new Rectangle(x,y,32,32),color);
            return false;
        }
        internal static int TileType(string key) => key switch {
            "soil"=>ModContent.TileType<Tiles.MawDirt>(),"stone"=>ModContent.TileType<Tiles.Mawstone>(),
            "grass"=>ModContent.TileType<Tiles.MawGrass>(),"sand"=>ModContent.TileType<Tiles.MawSand>(),
            "mud"=>ModContent.TileType<Tiles.MawMud>(),"clay"=>ModContent.TileType<Tiles.MawClay>(),
            "snow"=>ModContent.TileType<Tiles.MawSnow>(),"ice"=>ModContent.TileType<Tiles.MawIce>(),
            "bone"=>ModContent.TileType<Tiles.OssuaryBone>(), _=>MawTerrainStudies.Tile(key).Type
        };
        internal static ModWall Wall(string key) => key switch {
            "soil"=>ModContent.GetInstance<Walls.MawDirtWallUnsafe>(),"stone"=>ModContent.GetInstance<Walls.MawStoneWallUnsafe>(),
            "grass"=>ModContent.GetInstance<Walls.MawGrassWallUnsafe>(),"sand"=>ModContent.GetInstance<Walls.MawSandWallUnsafe>(),
            "mud"=>ModContent.GetInstance<Walls.MawMudWallUnsafe>(),"snow"=>ModContent.GetInstance<Walls.MawSnowWallUnsafe>(),
            "ice"=>ModContent.GetInstance<Walls.MawIceWallUnsafe>(),_=>MawTerrainStudies.Wall(key)
        };
    }
}
