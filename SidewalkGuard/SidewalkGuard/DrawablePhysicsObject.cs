using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SidewalkFuard
{
    public class DrawablePhysicsObject
    {
        /// <summary>
        /// Because Farseer uses 1 unit = 1 meter we need to convert
        /// between pixel coordinates and physics coordinates.
        /// I've chosen to use the rule that 100 pixels is one meter.
        /// We have to take box to convert between these two
        /// coordinate-sets wherever we mix them!
        /// </summary>

        public const float unitToPixel = 100.0f;
        public const float pixelToUnit = 1 / unitToPixel;

        public Body body;
        public Vector2 Position
        {
            get { return body.Position * unitToPixel; }
            set { body.Position = value * pixelToUnit; }
        }

        public Texture2D texture;

        private Vector2 size;
        public Vector2 Size
        {
            get { return size * unitToPixel; }
            set { size = value * pixelToUnit; }
        }

        /// <summary>
        ///The farseer simulation this object should be part of
        ///The image that will be drawn at the place of the body
        ///The size in pixels
        ///The mass in kilograms
        /// </summary>
        /// <param name="world"></param>
        /// <param name="texture"></param>
        /// <param name="size"></param>
        /// <param name="mass"></param>
        public DrawablePhysicsObject(World world, Texture2D texture, Vector2 size, float mass)
        {
            body = BodyFactory.CreateRectangle(world, size.X * pixelToUnit, size.Y * pixelToUnit, 1);
            body.BodyType = BodyType.Dynamic;

            this.Size = size;
            this.texture = texture;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 scale = new Vector2(Size.X / (float)texture.Width, Size.Y / (float)texture.Height);
            spriteBatch.Draw(texture, Position, null, Color.White, body.Rotation, new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), scale, SpriteEffects.None, 0);
        }
    }
}

