using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenGLCSharp {
    class FrameBuffer : IDisposable {

        int           fbo      = 0;
        private int[] textures = new int[2];


        public static FrameBuffer Create(Size size) {
            var f = new FrameBuffer();
            f.Create( size.Width, size.Height );
            return f;
        }

        public void Create(int width, int height) {
            this.fbo = GL.GenFramebuffer();

            GL.GenTextures( 2, this.textures );

            GL.BindTexture( TextureTarget.Texture2D, this.textures[0] );
            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMinFilter.Linear );

            GL.BindTexture( TextureTarget.Texture2D, this.textures[1] );
            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, width, height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMinFilter.Linear );
            GL.BindTexture( TextureTarget.Texture2D, 0 );

            Bind();
            GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,       TextureTarget.Texture2D, this.textures[0], 0 );
            GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, this.textures[1], 0 );
            UnbindS();
        }

        public void Destroy() {
            GL.DeleteFramebuffer( this.fbo );
            GL.DeleteTextures( 2, this.textures );
        }

        public void Bind() { GL.BindFramebuffer( FramebufferTarget.Framebuffer, this.fbo ); }

        public int GetTextureId() { return this.textures[0]; }

        public        void Unbind()  { FrameBuffer.UnbindS(); }
        public static void UnbindS() { GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0 ); }

        ~FrameBuffer() { Program.ResourceLeak( this.GetType() ); }

        #region IDisposable

        private void ReleaseUnmanagedResources() { Destroy(); }

        private void Dispose(bool disposing) {
            ReleaseUnmanagedResources();

            if ( disposing ) { }
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        #endregion

    }
}
