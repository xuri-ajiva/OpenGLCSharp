using OpenTK.Graphics.OpenGL4;

namespace OpenGLCSharp.Common {
    class PostProcessingShader : Shader {
        /// <inheritdoc />
        public PostProcessingShader(string vertPath, string fragPath) : base( vertPath, fragPath ) { }


        public void DrawBitmap(float time, int textureId) {
            this.Use();
            if ( this._uniformLocations.ContainsKey( "time" ) )
                this.SetFloat( "time", time );

            GL.ActiveTexture( TextureUnit.Texture0 );

            GL.BindTexture( TextureTarget.Texture2D, textureId );
            if ( this._uniformLocations.ContainsKey( "u_texture" ) )
                this.SetInt( "u_texture", 0 );
            GL.DrawArrays( PrimitiveType.Triangles, 0, 3 );
        }
    }
}
