using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace apogean.Content.Backgrounds
{
	/// <summary>
	/// Client-only compositor that replaces Terraria's five-layer Hell panorama with the
	/// Ruined Deep while leaving world tiles, liquids, entities, and lighting untouched.
	/// </summary>
	public sealed class RuinedUnderworldSky : CustomSky
	{
		internal const string VisualKey = "apogean:RuinedUnderworld";
		private const string TextureRoot = "apogean/Content/Backgrounds/Underworld/PanoramaV0_";

		private bool active;
		private float intensity;
		private Asset<Texture2D> farLayer;
		private Asset<Texture2D> midLayer;
		private Asset<Texture2D> closeLayer;

		internal bool RequestedActive => active;

		internal static void EnsureRegistered()
		{
			if (Main.dedServ)
				return;

			CustomSky sky = SkyManager.Instance[VisualKey];
			if (sky == null)
			{
				sky = new RuinedUnderworldSky();
				SkyManager.Instance[VisualKey] = sky;
			}

			// Some loader versions initialize the manager after Mod.Load. Loading a
			// late-bound diagnostic sky here keeps activation safe across that ordering.
			if (SkyManager.Instance.IsLoaded && !sky.IsLoaded)
				sky.Load();
		}

		public override void Activate(Vector2 position, params object[] args)
		{
			active = true;
		}
		public override void Deactivate(params object[] args) => active = false;
		public override bool IsActive() => active || intensity > 0f;
		public override bool IsVisible() => intensity > 0f;
		public override float GetCloudAlpha() => 1f;

		public override void Reset()
		{
			active = false;
			intensity = 0f;
		}

		public override void Update(GameTime gameTime)
		{
			float target = active ? 1f : 0f;
			intensity = MathHelper.Clamp(MathHelper.Lerp(intensity, target, 0.08f), 0f, 1f);
			if (Math.Abs(intensity - target) < 0.002f)
				intensity = target;
		}

		public override Color OnTileColor(Color inColor) =>
			Color.Lerp(inColor, new Color(150, 108, 72), intensity * 0.12f);

		public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
		{
			// DrawRemainingDepth supplies float.MinValue after all five vanilla Hell
			// layers. An opaque base in this final background band replaces that
			// panorama while remaining behind tiles, liquids, NPCs, and the player.
			if (minDepth > float.MinValue || intensity <= 0f)
				return;

			EnsureTextures();
			float viewportScale = Math.Max(1f, (float)Main.screenHeight / farLayer.Value.Height);
			DrawTiledLayer(spriteBatch, farLayer.Value, 0.025f, viewportScale, 0,
				Color.White * intensity);
			DrawTiledLayer(spriteBatch, midLayer.Value, 0.095f, viewportScale, 10,
				new Color(232, 209, 190) * intensity);
			DrawTiledLayer(spriteBatch, closeLayer.Value, 0.20f, viewportScale * 1.04f, 20,
				new Color(255, 235, 211) * intensity);
		}

		private void EnsureTextures()
		{
			farLayer ??= ModContent.Request<Texture2D>(TextureRoot + "Far");
			midLayer ??= ModContent.Request<Texture2D>(TextureRoot + "Mid");
			closeLayer ??= ModContent.Request<Texture2D>(TextureRoot + "Close");
		}

		private static void DrawTiledLayer(SpriteBatch spriteBatch, Texture2D texture,
			float parallax, float scale, int baselineOffset, Color color)
		{
			int scaledWidth = Math.Max(1, (int)Math.Ceiling(texture.Width * scale));
			int horizontalOffset = (int)(Main.screenPosition.X * parallax) % scaledWidth;
			if (horizontalOffset < 0)
				horizontalOffset += scaledWidth;

			float y = Main.screenHeight - texture.Height * scale + baselineOffset;
			for (int x = -horizontalOffset; x < Main.screenWidth; x += scaledWidth)
				spriteBatch.Draw(texture, new Vector2(x, y), null, color, 0f,
					Vector2.Zero, scale, SpriteEffects.None, 0f);
		}
	}

	public sealed class RuinedUnderworldSceneEffect : ModSceneEffect
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

		public override bool IsSceneEffectActive(Player player) =>
			player.ZoneUnderworldHeight &&
			RuinedBackgroundSelectionSystem.Instance.UnderworldSkyRenderLabEnabled;

		public override void SpecialVisuals(Player player, bool isActive)
		{
			RuinedUnderworldSky.EnsureRegistered();
			RuinedUnderworldSky sky = SkyManager.Instance[RuinedUnderworldSky.VisualKey] as RuinedUnderworldSky;
			if (sky == null || isActive == sky.RequestedActive)
				return;

			if (isActive)
			{
				SkyManager.Instance.Activate(RuinedUnderworldSky.VisualKey, player.Center);
			}
			else
			{
				SkyManager.Instance.Deactivate(RuinedUnderworldSky.VisualKey);
			}
		}
	}
}
