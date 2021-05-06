using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameboyZelda
{
    internal class Sprite
    {
        public Vector2 origin;
        public Texture2D tex;
        public Vector2 position;
        private int actualScale = 1;
        public int indexOfCollisionSquare = 0;

        public enum Versions
        {
            basic,
            Bullet,
            Meteor,
            Ship
        }

        public Versions spriteVersion = Versions.basic;

        public int playerScale
        {
            get
            {
                return actualScale;
            }
            set
            {
                actualScale = value;
                Initialize();
            }
        }

        public float rotation = 0;

        public Rectangle rectangle;
        public Color[] TextureData;

        public Matrix Transform
        {
            get
            {
                return Matrix.CreateTranslation(new Vector3(-new Vector2(origin.X * tex.Width, origin.Y * tex.Height), 0)) *
                  Matrix.CreateRotationZ(rotation) *
                  Matrix.CreateTranslation(new Vector3(position, 0));
            }
        }

        public Sprite(Texture2D _tex, Vector2 _position)
        {
            tex = _tex;
            position = _position;
            origin = new Vector2(0.5f, 0.5f);
            Initialize();
        }

        public Sprite(Texture2D _tex, Vector2 _position, Vector2 _origin)
        {
            tex = _tex;
            position = _position;
            origin = _origin;
            Initialize();
        }

        public Sprite(Texture2D _tex, GameWindow window)
        {
            tex = _tex;
            position = new Vector2(window.ClientBounds.Width / 2, window.ClientBounds.Height / 2);
            origin = new Vector2(0.5f, 0.5f);
            Initialize();
            //origin = new Vector2();
        }

        public virtual void Initialize()
        {
            TextureData = new Color[(tex.Width) * (tex.Height)];
            tex.GetData(TextureData);
            rectangle = new Rectangle((int)position.X, (int)position.Y, tex.Width * playerScale, tex.Height * playerScale);
        }

        public virtual void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(tex, position, null, Color.White, rotation, new Vector2(origin.X, origin.Y), playerScale, SpriteEffects.None, 1);
        }

        public virtual void Draw(SpriteBatch _spriteBatch, Color color)
        {
            _spriteBatch.Draw(tex, position, null, color, rotation, new Vector2(origin.X * tex.Width, origin.Y * tex.Height), playerScale, SpriteEffects.None, 1);
        }

        public void DrawBox(SpriteBatch _spriteBatch, Color color, Texture2D debugTex)
        {
            //_spriteBatch.Draw(debugTex, position, null, color, rotation, new Vector2(origin.X * tex.Width, origin.Y * tex.Height), playerScale, SpriteEffects.None, 2);
            _spriteBatch.Draw(debugTex, rectangle, color);
        }

        public virtual void UpdatePos(float deltaTime)
        {
            rectangle.X = (int)(position.X - (tex.Width * origin.X));
            rectangle.Y = (int)(position.Y - (tex.Height * origin.Y));
        }

        public virtual void UpdatePos(float deltaTime, GameWindow window)
        {
            rectangle.X = (int)(position.X - (tex.Width * origin.X));
            rectangle.Y = (int)(position.Y - (tex.Height * origin.Y));
        }

        public virtual bool Intersects(Sprite sprite)
        {
            // Calculate a matrix which transforms from A's local space into
            // world space and then into B's local space
            var transformAToB = this.Transform * Matrix.Invert(sprite.Transform);

            // When a point moves in A's local space, it moves in B's local space with a
            // fixed direction and distance proportional to the movement in A.
            // This algorithm steps through A one pixel at a time along A's X and Y axes
            // Calculate the analogous steps in B:
            var stepX = Vector2.TransformNormal(Vector2.UnitX, transformAToB);
            var stepY = Vector2.TransformNormal(Vector2.UnitY, transformAToB);

            // Calculate the top left corner of A in B's local space
            // This variable will be reused to keep track of the start of each row
            var yPosInB = Vector2.Transform(Vector2.Zero, transformAToB);

            for (int yA = 0; yA < this.rectangle.Height; yA++)
            {
                // Start at the beginning of the row
                var posInB = yPosInB;

                for (int xA = 0; xA < this.rectangle.Width; xA++)
                {
                    // Round to the nearest pixel
                    var xB = (int)Math.Round(posInB.X);
                    var yB = (int)Math.Round(posInB.Y);

                    if (0 <= xB && xB < sprite.rectangle.Width &&
                        0 <= yB && yB < sprite.rectangle.Height)
                    {
                        // Get the colors of the overlapping pixels
                        try
                        {
                            var colourA = this.TextureData[xA + yA * this.rectangle.Width];
                            var colourB = sprite.TextureData[xB + yB * sprite.rectangle.Width];

                            // If both pixel are not completely transparent
                            if (colourA.A != 0 && colourB.A != 0)
                            {
                                return true;
                            }
                        }
                        catch (Exception)
                        {
                            return true;
                        }
                    }

                    // Move to the next pixel in the row
                    posInB += stepX;
                }

                // Move to the next row
                yPosInB += stepY;
            }

            // No intersection found
            return false;
        }

        public virtual bool BasicIntersects(Sprite sprite)
        {
            return sprite.rectangle.Intersects(rectangle);
        }
    }
}