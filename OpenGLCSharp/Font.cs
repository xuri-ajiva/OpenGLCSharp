using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenGLCSharp.Common;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenGLCSharp {
    /// <summary>
    /// Uses System.Drawing for 2d text rendering.
    /// </summary>
    internal static class FontManager {
        public static void LoadFonts(string resourcePath) {
            var files = new DirectoryInfo( resourcePath ).GetFiles().Where( x => x.Extension == ".ttf" ).ToList();
            foreach ( var fontFile in files ) {
                AddFont( fontFile.FullName );
                Thread.Sleep( 100 );
            }
        }

        private static PrivateFontCollection _privateFontCollection = new PrivateFontCollection();

        public static FontFamily GetFontFamilyByName(string name) {
            if ( _privateFontCollection.Families.Any( x => x.Name == name ) )
                return _privateFontCollection.Families.FirstOrDefault( x => x.Name == name );
            else throw new AccessViolationException("Font loading Error");
        }

        private static void AddFont(string fullFileName) { AddFont( File.ReadAllBytes( fullFileName ) ); }

        private static void AddFont(byte[] fontBytes) {
            var    handle  = GCHandle.Alloc( fontBytes, GCHandleType.Pinned );
            IntPtr pointer = handle.AddrOfPinnedObject();

            try {
                _privateFontCollection.AddMemoryFont( pointer, fontBytes.Length );
            } finally {
                handle.Free();
            }
        }
    }

    public class TextRenderer : IDisposable {
        private readonly Bitmap    _bmp;
        private readonly Graphics  _gfx;
        private readonly int       _texture;
        private          Rectangle _dirtyRegion;
        private          bool      _disposed;


        public void DrawDirect(Func<Graphics, Rectangle> yourAction) {
            var rec = yourAction.Invoke( this._gfx );

            if ( rec == Rectangle.Empty )
                this._dirtyRegion = new Rectangle( 0, 0, this._bmp.Width, this._bmp.Height );
        }


        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="width">The width of the backing store in pixels.</param>
        /// <param name="height">The height of the backing store in pixels.</param>
        public TextRenderer(int width, int height) {
            if ( width <= 0 )
                throw new ArgumentOutOfRangeException( nameof(width) );
            if ( height <= 0 )
                throw new ArgumentOutOfRangeException( nameof(height) );
            if ( GraphicsContext.CurrentContext == null )
                throw new InvalidOperationException( "No GraphicsContext is current on the calling thread." );

            this._bmp                   = new Bitmap( width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
            this._gfx                   = Graphics.FromImage( this._bmp );
            this._gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            this._texture = GL.GenTexture();
            GL.BindTexture( TextureTarget.Texture2D, this._texture );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear );
            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero );
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Clears the backing store to the specified color.
        /// </summary>
        /// <param name="color">A <see cref="System.Drawing.Color"/>.</param>
        public void Clear(Color color) {
            this._gfx.Clear( color );
            this._dirtyRegion = new Rectangle( 0, 0, this._bmp.Width, this._bmp.Height );
        }


        /// <summary>
        /// Draws the specified string to the backing store.
        /// </summary>
        /// <param name="text">The <see cref="System.String"/> to draw.</param>
        /// <param name="font">The <see cref="System.Drawing.Font"/> that will be used.</param>
        /// <param name="brush">The <see cref="System.Drawing.Brush"/> that will be used.</param>
        /// <param name="point">The location of the text on the backing store, in 2d pixel coordinates.
        /// The origin (0, 0) lies at the top-left corner of the backing store.</param>
        public void DrawString(string text, Font font, Brush brush, PointF point) {
            this._gfx.DrawString( text, font, brush, point );

            SizeF size = this._gfx.MeasureString( text, font );
            this._dirtyRegion = Rectangle.Round( RectangleF.Union( this._dirtyRegion, new RectangleF( point, size ) ) );
            this._dirtyRegion = Rectangle.Intersect( this._dirtyRegion, new Rectangle( 0, 0, this._bmp.Width, this._bmp.Height ) );
        }

        /// <summary>
        /// Gets a <see cref="System.Int32"/> that represents an OpenGL 2d texture handle.
        /// The texture contains a copy of the backing store. Bind this texture to TextureTarget.Texture2d
        /// in order to render the drawn text on screen.
        /// </summary>
        public int Texture {
            get {
                UploadBitmap();
                return this._texture;
            }
        }

        #endregion

        #region Private Members

        // Copy the bitmap, rotate it, and return the result.
        private Bitmap ModifiedBitmap(Bitmap originalImage, RotateFlipType rotateFlipType) {
            originalImage.RotateFlip( rotateFlipType );
            return originalImage;
        }

        // Uploads the dirty regions of the backing store to the OpenGL texture.
        private void UploadBitmap() {
            if ( this._dirtyRegion != RectangleF.Empty ) {
                var bm = ModifiedBitmap( this._bmp, RotateFlipType.Rotate180FlipX );

                System.Drawing.Imaging.BitmapData data = bm.LockBits( this._dirtyRegion, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

                GL.BindTexture( TextureTarget.Texture2D, this._texture );
                GL.TexSubImage2D( TextureTarget.Texture2D, 0, this._dirtyRegion.X, this._dirtyRegion.Y, this._dirtyRegion.Width, this._dirtyRegion.Height, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0 );

                bm.UnlockBits( data );

                this._dirtyRegion = Rectangle.Empty;
            }
        }

        #endregion

        #region IDisposable Members

        private void Dispose(bool manual) {
            if ( !this._disposed ) {
                if ( manual ) {
                    this._bmp.Dispose();
                    this._gfx.Dispose();
                    if ( GraphicsContext.CurrentContext != null )
                        GL.DeleteTexture( this._texture );
                }

                this._disposed = true;
            }
        }

        public void Dispose() {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        ~TextRenderer() { Program.ResourceLeak( this.GetType() ); }

        #endregion


        public void RenderText() {
            GL.MatrixMode( MatrixMode.Modelview );
            GL.LoadIdentity();

            GL.Enable( EnableCap.Texture2D );
            GL.BindTexture( TextureTarget.Texture2D, this.Texture );
            GL.Begin( PrimitiveType.Polygon );

            GL.TexCoord2( 0.0f, 1.0f );
            GL.Vertex2( -1f, -1f );
            GL.TexCoord2( 1.0f, 1.0f );
            GL.Vertex2( 1f, -1f );
            GL.TexCoord2( 1.0f, 0.0f );
            GL.Vertex2( 1f, 1f );
            GL.TexCoord2( 0.0f, 0.0f );
            GL.Vertex2( -1f, 1f );

            GL.End();
        }
    }

}
