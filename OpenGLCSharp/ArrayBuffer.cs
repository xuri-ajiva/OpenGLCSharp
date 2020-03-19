using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenGLCSharp.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using SharedProject.SharedComponents;

namespace OpenGLCSharp {
    public class ArrayBuffer : IDisposable {
        public Vertex[]      Data                  { get; }
        public int[]        Indices               { get; }
        public Shader        Shader                { get; private set; }
        public Matrix4       TransformationsMatrix { get; set; }
        public List<Texture> Textures              { get; } = new List<Texture>();
        public int           IndicesCount          => this.Indices.Length;

        private int _elementBufferObject;
        private int _vertexBufferObject;
        private int _vertexArrayObject;

        public ArrayBuffer(ref Vertex[] data, ref int[] indices) {
            this.Data             = data;
            this.Indices          = indices;
            TransformationsMatrix = Matrix4.Identity;
            Init1( ref data, ref indices );
        }

        private void Init1 <T, U>(ref T[] data, ref U[] indices) where T : struct where U : struct {
            int TSize = Marshal.SizeOf<T>();
            int USize = Marshal.SizeOf<U>();
            Console.WriteLine( "Index Buffer Init1: " );
            Console.WriteLine( "    Type: "                       + typeof(T) );
            Console.WriteLine( "    Length: "                     + data.Length );
            Console.WriteLine( "    Size: " + data.Length * TSize + "B" );

            this._vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.ArrayBuffer, this._vertexBufferObject );
            GL.BufferData( BufferTarget.ArrayBuffer, data.Length * TSize, data, BufferUsageHint.StaticDraw );

            this._elementBufferObject = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.ElementArrayBuffer, this._elementBufferObject );
            GL.BufferData( BufferTarget.ElementArrayBuffer, indices.Length * USize, indices, BufferUsageHint.StaticDraw );

            this._vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray( this._vertexArrayObject );

            GL.BindBuffer( BufferTarget.ArrayBuffer,        this._vertexArrayObject );
            GL.BindBuffer( BufferTarget.ElementArrayBuffer, this._elementBufferObject );
        } 

        public void TextureSetupSetup(string[] listOfTextureLocations) {
            for ( var i = 0; i < listOfTextureLocations.Length; i++ ) {
                string textureLocation = listOfTextureLocations[i];
                var    _texture        = new Texture( textureLocation );
                this.Textures.Add( _texture );
            }

            TextureToShader();
        }

        public void TextureToShader() {
            for ( int i = 0; i < this.Textures.Count; i++ ) {
                string texture = ( "texture" + i );
                this.Shader.SetInt( texture, i );
            }
        }

        public void UseAllTextures() {
            for ( int i = 0; i < this.Textures.Count; i++ ) {
                string texture = ( "Texture" + i );
                this.Textures[i].Use( (TextureUnit) Enum.Parse( typeof(TextureUnit), texture ) );
            }
        }


        public void ShaderSetup(ref Shader shader, IEnumerable<(string, int)> listOfShaderDescriptors) {
            this.Shader = shader;
            int stride      = Marshal.SizeOf<Vertex>();
            var currentSize = 0;

            foreach ( ( string name, int size ) in listOfShaderDescriptors ) {
                int location = shader.GetAttribLocation( name );
                if ( location == -1 ) throw new ArgumentException( "Shader has GetAttribLocation location called " + name, name );

                GL.EnableVertexAttribArray( location );
                GL.VertexAttribPointer( location, size, VertexAttribPointerType.Float, false, stride, currentSize * sizeof(float) );

                currentSize += size;
            }
        }

        public void Delete() {
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
            GL.BindVertexArray( 0 );
            GL.UseProgram( 0 );

            GL.DeleteBuffer( this._vertexBufferObject );
            GL.DeleteVertexArray( this._vertexArrayObject );

            foreach ( Texture texture in Textures ) {
                texture.Dispose();
            }
        }

        public void Bind() {
            GL.BindVertexArray( this._vertexArrayObject );
            UseAllTextures();
        }

        public static void Unbind() { GL.BindVertexArray( 0 ); }


        #region IDisposable

        private void ReleaseUnmanagedResources() { Delete(); }

        private void Dispose(bool disposing) {
            ReleaseUnmanagedResources();

            if ( disposing ) { }
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <inheritdoc />
        ~ArrayBuffer() { Program.ResourceLeak( this.GetType() ); }

        #endregion

    }
}
