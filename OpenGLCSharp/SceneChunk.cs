using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGLCSharp.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenGLCSharp {
    class SceneChunk : IDisposable {
        private Shader            pShader;
        private ArrayBuffer[] Meshes;
        private Vector3           Position;
        private Matrix4           Transform;
        private Camera            camera;

        public event Func<DrawEventArgs, DrawEventArgs> BeforeDraw;

        public SceneChunk(Shader pShader, ArrayBuffer[] meshes, Vector3 position, Matrix4 transform, Camera camera) {
            this.pShader   = pShader;
            this.Meshes    = meshes;
            this.Position  = position;
            this.Transform = transform;
            this.camera    = camera;
        }

        public void Draw() {
            this.pShader.Use();

            foreach ( ArrayBuffer mesh in this.Meshes ) {
                var defargs = new DrawEventArgs( mesh.TransformationsMatrix, mesh, this.pShader );

                var args = OnBeforeDraw( defargs );
                if ( args == null )
                    args = defargs;

                Matrix4 uModelViewProjection = args.Matrix * this.camera.ViewProjectionMatrix;
                this.pShader.SetMatrix4( "UmodelViewProjection", uModelViewProjection );
                mesh.Bind();
                GL.DrawElements( PrimitiveType.Triangles, mesh.IndicesCount, DrawElementsType.UnsignedInt, 0 );
            }
        }


        public class DrawEventArgs {
            public DrawEventArgs(Matrix4 matrix, ArrayBuffer objectBuffer, Shader shader) {
                this.Matrix       = matrix;
                this.ObjectBuffer = objectBuffer;
                this.Shader       = shader;
            }

            public Matrix4 Matrix { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }

            public ArrayBuffer ObjectBuffer { [DebuggerStepThrough] get; }

            public Shader Shader { [DebuggerStepThrough] get; }
        }
        protected virtual DrawEventArgs OnBeforeDraw(DrawEventArgs arg) { return this.BeforeDraw?.Invoke( arg ); }


        ~SceneChunk() {
            Program.ResourceLeak( this.GetType() );
        }

        #region IDisposable

        private void ReleaseUnmanagedResources() {
            foreach ( var mesh in this.Meshes ) {
                mesh.Dispose();
            }
        }

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
