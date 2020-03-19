#region using

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using OpenGLCSharp.Common;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using SharedProject.SharedComponents;
using static OpenGLCSharp.Program;

#endregion

namespace OpenGLCSharp {

    public class Window : GameWindow {
        private Vertex[] _vertices = {
            // Position                Texture coordinates
            new Vertex( 0.5f,  0.5f,  0.5f,  1.0f, 1.0f ), // top right
            new Vertex( 0.5f,  -0.5f, 0.5f,  1.0f, 0.0f ), // bottom right
            new Vertex( -0.5f, -0.5f, 0.5f,  0.0f, 0.0f ), // bottom left
            new Vertex( -0.5f, 0.5f,  0.5f,  0.0f, 1.0f ), // top left  
            new Vertex( 0.5f,  0.5f,  -0.5f, 1.0f, 1.0f ), // top right   back
            new Vertex( 0.5f,  -0.5f, -0.5f, 1.0f, 0.0f ), // bottom right  back
            new Vertex( -0.5f, -0.5f, -0.5f, 0.0f, 0.0f ), // bottom left   back
            new Vertex( -0.5f, 0.5f,  -0.5f, 0.0f, 1.0f )  // top left   back
        };

        private uint[] _indices = {
            // front
            0, 1, 2,
            2, 3, 0,
            // right
            1, 5, 6,
            6, 2, 1,
            // back
            7, 6, 5,
            5, 4, 7,
            // left
            4, 0, 3,
            3, 7, 4,
            // bottom
            4, 5, 1,
            1, 0, 4,
            // top
            3, 2, 6,
            6, 7, 3
        };


        private Shader               _shader;
        private PostProcessingShader _postProcessingShader;
        private PostProcessingShader _postProcessingShaderNoEffect;
        private Camera               _camera;
        private bool                 _firstMove = true;

        private Vector2 _lastPos;

        private double _time;
        public  int    totalFramesRendert = 0;
        public  int    totalFramesUpdated = 0;
        public  int    numFrames          = 0;
        public  double framesTime         = 0.0;
        public  int    frameRate          = 0;


        //ArrayBuffer _mainBuffer;

        //private ArrayBuffer[] _objectBuffers = new ArrayBuffer[10];

        private SceneChunk  sceneChunk;
        private FrameBuffer Buffer;

        TextRenderer renderer;
        Font         mono_BIG = new Font( FontFamily.GenericMonospace, 60 );
        private Font ka1;

        public Window(int width, int height, string title) : base( width, height, GraphicsMode.Default, title ) { }

        protected override void OnLoad(EventArgs e) {
            GL.ClearColor( .1f, .1f, .1f, 1.0f );

            GL.Enable( EnableCap.DepthTest );

            // ################################## Shader Setup
            this._shader                       = new Shader( ResourcePath               + "Shaders\\shader.vert", ResourcePath              + "Shaders\\shader.frag" );
            this._postProcessingShaderNoEffect = new PostProcessingShader( ResourcePath + "Shaders\\postprocessNoEffect.vert", ResourcePath + "Shaders\\postprocessNoEffect.frag" );
            this._postProcessingShader         = new PostProcessingShader( ResourcePath + "Shaders\\postprocess.vert",         ResourcePath + "Shaders\\postprocess.frag" );
            this._shader.Use();

            // ################################## Font Setup
            FontManager.LoadFonts( ResourcePath + "Fonts" );
            this.ka1 = new Font( FontManager.GetFontFamilyByName( "Karmatic Arcade" ), 24 );

            renderer = new TextRenderer( Width, Height );
            this.renderer.Clear( Color.Transparent );

            // ################################## Screen Setup

            this.Buffer  = FrameBuffer.Create( new Size( this.Width, this.Height ) );
            this._camera = new Camera( Vector3.UnitZ * 3, this.Width / (float) this.Height ) { zFar = 1000f };

            // ################################## Scene Setup

            //this._mainBuffer = new ArrayBuffer( ref this._vertices, ref this._indices );
            //this._mainBuffer.ShaderSetup( ref this._shader, new[] { ( "aPosition", 3 ), ( "aTexCoord", 2 ) } );
            //this._mainBuffer.TextureSetupSetup( new[] { ResourcePath + "Resources\\container.png", ResourcePath + "Resources\\awesomeface.png" } );

            //Random r = new Random();
            //
            //for ( int i = 0; i < this._objectBuffers.Length; i++ ) {
            //    this._objectBuffers[i]                       =  new ArrayBuffer( ref this._vertices, ref this._indices );
            //    this._objectBuffers[i].TransformationsMatrix *= Matrix4.CreateTranslation( r.Next( 0, 20 ), r.Next( 0, 20 ), r.Next( 0, 20 ) );
            //    this._objectBuffers[i].ShaderSetup( ref this._shader, new[] { ( "aPosition", 3 ), ( "aTexCoord", 2 ) } );
            //    this._objectBuffers[i].TextureSetupSetup( new[] { ResourcePath + "Resources\\container.png", ResourcePath + "Resources\\awesomeface.png" } );
            //}
            var       bf = new BinaryFormatter();
            var       fs = File.Open( @"C:\Users\admin\Desktop\untitled.bmf", FileMode.Open );
            SceneData sd = (SceneData) bf.Deserialize( fs );
            fs.Close();

            List<ArrayBuffer> buffers = new List<ArrayBuffer>();

            foreach ( var bufferInfo in sd.Meshes1 ) {
                var vt = bufferInfo.Data;
                var id = bufferInfo.Indices;

                ArrayBuffer buf = new ArrayBuffer( ref vt, ref id);
                buf.TransformationsMatrix = sd.TransFormMatrix;
                buf.ShaderSetup( ref this._shader, new[] { ( "aPosition", 3 ), ( "aTexCoord", 2 ) } );   
                buf.TextureSetupSetup( /*new[] { ResourcePath + "Resources\\container.png", ResourcePath + "Resources\\awesomeface.png" }*/  sd.TextureIdList.Select( x=> x.Item2 ).ToArray() );

                buffers.Add( buf );
            }

            this.sceneChunk = new SceneChunk( this._shader,buffers.ToArray(), Vector3.Zero, Matrix4.Identity, this._camera );
            this.sceneChunk.BeforeDraw += delegate(SceneChunk.DrawEventArgs args) {
                args.Matrix *= Matrix4.Identity * Matrix4.CreateRotationX( (float) MathHelper.DegreesToRadians( this._time * 10 ) );

                return args;
            };

            this.CursorVisible = false;
            base.OnLoad( e );
        }


        protected override void OnRenderFrame(FrameEventArgs e) {
            this._time      += e.Time;
            this.numFrames  += 1;
            this.framesTime += e.Time;
            this.totalFramesRendert++;

            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            this.renderer.Clear( Color.Transparent );
            this.renderer.DrawString( "XURI", ka1, Brushes.White, new PointF( 5, Height - ka1.Height - 5 ) );
            var infoText = "FPS: " + this.frameRate;
            this.renderer.DrawString( infoText, this.ka1, Brushes.Firebrick, new PointF( 0, 0 ) );

            this.Buffer.Bind();
            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            this.sceneChunk.Draw();

            this.Buffer.Unbind();

            this._postProcessingShader.DrawBitmap( (float) this._time, this.Buffer.GetTextureId() );
            this._postProcessingShaderNoEffect.DrawBitmap( (float) this._time, this.renderer.Texture ); //font

            SwapBuffers();
            base.OnRenderFrame( e );
        }

        private const float FRAME_UPDATE_INTERVAL = 5;

        protected override void OnUpdateFrame(FrameEventArgs e) {
            this.totalFramesUpdated++;

            if ( this.framesTime >= 1.0f / FRAME_UPDATE_INTERVAL ) {
                this.frameRate  = (int) ( ( Convert.ToDouble( this.numFrames ) / this.framesTime ) );
                this.framesTime = 0.0;
                this.numFrames  = 0;
            }

            if ( !this.Focused ) { // check to see if the window is focused
                return;
            }

            var input = Keyboard.GetState();

            if ( input.IsKeyDown( Key.Escape ) ) {
                this._shader.Dispose();
                Exit();
            }

            const float cameraSpeed = 15f;
            const float sensitivity = 0.2f;

            if ( input.IsKeyDown( Key.W ) ) {
                this._camera.Position += this._camera.Front * cameraSpeed * (float) e.Time; // Forward
            }

            if ( input.IsKeyDown( Key.S ) ) {
                this._camera.Position -= this._camera.Front * cameraSpeed * (float) e.Time; // Backwards
            }

            if ( input.IsKeyDown( Key.A ) ) {
                this._camera.Position -= this._camera.Right * cameraSpeed * (float) e.Time; // Left
            }

            if ( input.IsKeyDown( Key.D ) ) {
                this._camera.Position += this._camera.Right * cameraSpeed * (float) e.Time; // Right
            }

            if ( input.IsKeyDown( Key.Space ) ) {
                this._camera.Position += this._camera.Up * cameraSpeed * (float) e.Time; // Up
            }

            if ( input.IsKeyDown( Key.LShift ) ) {
                this._camera.Position -= this._camera.Up * cameraSpeed * (float) e.Time; // Down
            }

            // Get the mouse state
            var mouse = Mouse.GetState();

            if ( this._firstMove ) { // this bool variable is initially set to true

                this._lastPos   = new Vector2( mouse.X, mouse.Y );
                this._firstMove = false;
            }
            else {
                // Calculate the offset of the mouse position
                float deltaX = mouse.X - this._lastPos.X;
                float deltaY = mouse.Y - this._lastPos.Y;
                this._lastPos = new Vector2( mouse.X, mouse.Y );

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                this._camera.Yaw   += deltaX * sensitivity;
                this._camera.Pitch -= deltaY * sensitivity; // reversed since y-coordinates range from bottom to top
            }

            base.OnUpdateFrame( e );
        }

        protected override void OnMouseMove(MouseMoveEventArgs e) {
            if ( this.Focused ) {
                Mouse.SetPosition( this.X + this.Width / 2f, this.Y + this.Height / 2f );
            }

            base.OnMouseMove( e );
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) {
            this._camera.Fov -= e.DeltaPrecise;
            base.OnMouseWheel( e );
        }

        protected override void OnResize(EventArgs e) {
            GL.Viewport( 0, 0, this.Width, this.Height );
            this.Buffer.Dispose();
            this.Buffer              = FrameBuffer.Create( new Size( this.Width, this.Height ) );
            this._camera.AspectRatio = this.Width / (float) this.Height;

            base.OnResize( e );
        }

        protected override void OnUnload(EventArgs e) { base.OnUnload( e ); }

        #region Overrides of GameWindow

        /// <inheritdoc />
        protected override void Dispose(bool manual) {
            this.sceneChunk?.Dispose();
            this.renderer?.Dispose();
            this._shader?.Dispose();
            this._postProcessingShader?.Dispose();
            this._postProcessingShaderNoEffect?.Dispose();
            this.Buffer?.Dispose();
            this._camera?.Dispose();

            base.Dispose( manual );
        }

        #endregion

    }
}
