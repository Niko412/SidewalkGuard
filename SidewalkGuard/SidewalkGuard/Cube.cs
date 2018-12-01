using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SidewalkFuard
{
    public class Cube
    {
        /// <summary>
        /// In this section I'm spawning boexs' each box comes in random x position
        /// </summary>
        private Random random;
        World world;
        public DrawablePhysicsObject SpawnCube(World world, Game game)
        {
            random = new Random();
            this.world = world;
            DrawablePhysicsObject crate = new DrawablePhysicsObject(world, game.Content.Load<Texture2D>("Crate"), new Vector2(50.0f, 50.0f), 0.1f);
            crate.Position = new Vector2(random.Next(50, game.GraphicsDevice.Viewport.Width - 50), 1);
            crate.body.CollidesWith = Category.All;
            crate.body.CollisionCategories = Category.Cat2;
            crate.body.CollisionGroup = 3;
            return crate;
        }

    }
}
