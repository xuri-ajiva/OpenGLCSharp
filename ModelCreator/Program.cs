using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Assimp;

namespace ModelCreator {
    class Program {

        static List<Mesh>     _meshes    = new List<Mesh>();
        static List<Material> _materials = new List<Material>();

        #region structs

        public struct Material {
            public static readonly int size = Marshal.SizeOf<Vec3>() * 3 + sizeof(float);

            public Vec3   diffuse;
            public Vec3   specular;
            public Vec3   emissive;
            public float  shininess;
            public string diffuseMapName;
            public string normalMapName;

            public byte[] GetBytes() {
                int    sP  = sizeof(Single) * 3;
                byte[] arr = new byte[3 * sP + sizeof(float)];
                Array.Copy( this.diffuse.GetBytes(),                 0, arr, sP * 0,                 sP );
                Array.Copy( this.specular.GetBytes(),                0, arr, sP * 1,                 sP );
                Array.Copy( this.emissive.GetBytes(),                0, arr, sP * 2,                 sP );
                Array.Copy( BitConverter.GetBytes( this.shininess ), 0, arr, sP * 2 + sizeof(float), sizeof(float) );

                return arr;
            }

            #region Overrides of ValueType

            /// <inheritdoc />
            public override string ToString() {
                var str = "";
                str += "["                                       + this.diffuse.ToString();
                str += ", "                                      + this.specular.ToString();
                str += ", "                                      + this.emissive.ToString();
                str += ", " + this.shininess.ToString( "0.000" ) + "]";

                return str;
            }

            #endregion

        }

        public struct Vec2 {
            public float  x;
            public Single y;

            public Vec2(float x, float y) {
                this.x = x;
                this.y = y;
            }

            public Vec2(Single value) {
                this.x = value;
                this.y = value;
            }

            public Vec2(Vector3D v) {
                this.x = v.X;
                this.y = v.Y;
            }

            public byte[] GetBytes() {
                int    s   = sizeof(Single);
                byte[] arr = new byte[2 * s];
                Array.Copy( BitConverter.GetBytes( x ), 0, arr, s * 0, s );
                Array.Copy( BitConverter.GetBytes( y ), 0, arr, s * 1, s );

                return arr;
            }
        }

        public struct Vec3 {
            public Single x;
            public Single y;
            public Single z;

            public Vec3(float x, float y, float z) {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public Vec3(Vec2 vec, Single z) {
                this.x = vec.x;
                this.y = vec.y;
                this.z = z;
            }

            public Vec3(Single value) {
                this.x = value;
                this.y = value;
                this.z = value;
            }

            public Vec3(Vector3D v) {
                this.x = v.X;
                this.y = v.Y;
                this.z = v.Z;
            }

            public byte[] GetBytes() {
                int    s   = sizeof(Single);
                byte[] arr = new byte[3 * s];
                Array.Copy( BitConverter.GetBytes( x ), 0, arr, s * 0, s );
                Array.Copy( BitConverter.GetBytes( y ), 0, arr, s * 1, s );
                Array.Copy( BitConverter.GetBytes( z ), 0, arr, s * 2, s );

                return arr;
            }

            #region Overrides of ValueType

            /// <inheritdoc />
            public override string ToString() => "{" + this.x.ToString( "0.000" ) + ", " + this.y.ToString( "0.000" ) + ", " + this.z.ToString( "0.000" ) + "}";

            #endregion

        }

        public class Mesh {
            public List<Vec3>  positions     = new List<Vec3>();
            public List<Vec3>  normals       = new List<Vec3>();
            public List<Vec3>  tangents      = new List<Vec3>();
            public List<Vec2>  uvs           = new List<Vec2>();
            public List<Int32> indices       = new List<Int32>();
            public int         materialIndex = 0;

        };

        #endregion


        static void ProcessMesh(Assimp.Mesh mesh, Assimp.Scene scene) {
            Mesh m = new Mesh();

            for ( int i = 0; i < mesh.VertexCount; i++ ) {
                if ( mesh.HasVertices ) {
                    Vec3 vec3 = new Vec3( mesh.Vertices[i] );
                    m.positions.Add( vec3 );
                }
                else {
                    Console.WriteLine( "[" + i + "]: " + "No Vertices" );
                }

                if ( mesh.HasNormals ) {
                    Vec3 normal = new Vec3( mesh.Normals[i] );
                    m.normals.Add( normal );
                }
                else {
                    Console.WriteLine( "[" + i + "]: " + "No Normals" );
                }

                if ( mesh.HasTangentBasis ) {
                    Vec3 tangent = new Vec3( mesh.Tangents[i] );
                    m.tangents.Add( tangent );
                }
                else {
                    Console.WriteLine( "[" + i + "]: " + "No Tangents" );
                }

                if ( mesh.HasTextureCoords( 0 ) ) {
                    Vec2 uv = new Vec2( mesh.TextureCoordinateChannels[0][i] );
                    m.uvs.Add( uv );
                }
                else {
                    Console.WriteLine( "[" + i + "]: " + "No TextureCoordinate" );
                }
            }

            foreach ( Face f in mesh.Faces ) {
                for ( int j = 0;
                    j < f.IndexCount;
                    j++ ) {
                    m.indices.Add( f.Indices[j] );
                }
            }

            m.materialIndex = mesh.MaterialIndex;
            _meshes.Add( m );
        }

        static void ProcessNode(Node node, Scene scene) {
            for ( int i = 0; i < node.MeshCount; i++ ) {
                Assimp.Mesh mesh = scene.Meshes[node.MeshIndices[i]];
                ProcessMesh( mesh, scene );
            }

            for ( int i = 0; i < node.ChildCount; i++ ) {
                ProcessNode( node.Children[i], scene );
            }
        }

        static void ProcessMaterials(Scene scene, string directoryName) {
            for ( int i = 0; i < scene.MaterialCount; i++ ) {
                Material        mat      = new Material();
                Assimp.Material material = scene.Materials[i];

                var diffuse = material.ColorDiffuse;
                mat.diffuse = new Vec3( diffuse.R, diffuse.G, diffuse.B );

                var specular = material.ColorSpecular;
                mat.specular = new Vec3( specular.R, specular.G, specular.B );

                var emissive = material.ColorEmissive;
                mat.emissive = new Vec3( emissive.R, emissive.G, emissive.B );

                float shininess = material.Shininess;
                mat.shininess = shininess;

                float shininessStrength = material.ShininessStrength;

                mat.specular.x *= shininessStrength;
                mat.specular.y *= shininessStrength;
                mat.specular.z *= shininessStrength;

                int numDiffuseMaps = material.GetMaterialTextureCount( TextureType.Diffuse );
                int numNormalMaps  = material.GetMaterialTextureCount( TextureType.Normals );

                if ( numDiffuseMaps > 0 ) {
                    material.GetMaterialTexture( TextureType.Diffuse, 0, out var diffuseMapName );
                    Console.WriteLine( diffuseMapName.FilePath );

                    if ( !File.Exists( diffuseMapName.FilePath ) ) {
                        //TODO: open new map
                        //var a = new ModelWorkerEventArgs() { Args = diffuseMapName.FilePath, EventType = ModelWorkerEventArgs.EventArgsType.TextureNotExists, Context = directoryName };
                        //this.OnActionCallback?.Invoke( this, a );
                        //diffuseMapName.FilePath = a.Args.Replace( directoryName, "" );
                    }

                    mat.diffuseMapName = diffuseMapName.FilePath;
                }

                if ( numNormalMaps > 0 ) {
                    material.GetMaterialTexture( TextureType.Normals, 0, out var normalMapName );
                    Console.WriteLine( normalMapName.FilePath );

                    if ( !File.Exists( normalMapName.FilePath ) ) {
                        //TODO: Open Texture
                        //var a = new ModelWorkerEventArgs() { Args = normalMapName.FilePath, EventType = ModelWorkerEventArgs.EventArgsType.TextureNotExists, Context = directoryName };
                        //this.OnActionCallback?.Invoke( this, a );
                        //normalMapName.FilePath = a.Args.Replace( directoryName, "" );
                    }

                    mat.normalMapName = normalMapName.FilePath;
                }

                _materials.Add( mat );
            }
        }

        static byte[] getBytes(Material material) {
            int    size = Marshal.SizeOf( material );
            int    s    = Material.size;
            byte[] arr  = new byte[size];

            try {
                IntPtr ptr = Marshal.AllocHGlobal( size );
                Marshal.StructureToPtr( material, ptr, false );
                Marshal.Copy( ptr, arr, 0, s );
                Marshal.FreeHGlobal( ptr );
            } catch (Exception e) {
                Console.WriteLine( e.Message );
            }

            return arr.Take( s ).ToArray();

            //return material.GetBytes();
        }

        static void Main(string[] args) {
            var model = @"C:\0EE934262B1635B7\source\repos\OpenGLCSharp\3d we.obj";
            int hf    = 0;

            string        directory = Path.GetDirectoryName( model );
            AssimpContext importer  = new AssimpContext();

            var scene = importer.ImportFile( model, PostProcessSteps.PreTransformVertices | PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals | PostProcessSteps.OptimizeMeshes | PostProcessSteps.OptimizeGraph | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.ImproveCacheLocality | PostProcessSteps.CalculateTangentSpace );

            ProcessMaterials( scene, Path.GetDirectoryName( model )+ "\\" );
            ProcessNode( scene.RootNode, scene );

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension( model );
            string outputFilename           = directory + "\\" + fileNameWithoutExtension + ".bmf";
            Console.WriteLine( "OutputFile: "                                             + outputFilename );

            File.Delete( outputFilename );
            var fex = File.Open( outputFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite );

            using ( BinaryWriter br = new BinaryWriter( fex ) ) {
                Console.WriteLine( "Writing bmf file..." );

                Console.WriteLine( "Writing Materials:" );

                // Materials
                UInt64 numMaterials = (ulong) _materials.Count;
                br.Write( numMaterials );

                foreach ( Material material in _materials ) {
                    br.Write( getBytes( material ) );

                    const string pathPrefix           = "models/";
                    UInt64       diffuesMapNameLength = 0;
                    UInt64       normalMapNameLength  = 0;

                    // Diffuse map
                    {
                        if ( !string.IsNullOrEmpty( material.diffuseMapName ) )
                            diffuesMapNameLength = (UInt64) ( material.diffuseMapName.Length + pathPrefix.Length );
                    }

                    br.Write( diffuesMapNameLength );

                    if ( diffuesMapNameLength > 0 ) {
                        br.Write( Encoding.UTF8.GetBytes( pathPrefix + material.diffuseMapName ) );
                        Console.WriteLine( pathPrefix + material.diffuseMapName );
                    }

                    // Normal map
                    {
                        if ( !string.IsNullOrEmpty( material.normalMapName ) )
                            normalMapNameLength = (UInt64) ( material.normalMapName.Length + pathPrefix.Length );
                    }

                    br.Write( normalMapNameLength );

                    if ( normalMapNameLength > 0 ) {
                        br.Write( Encoding.UTF8.GetBytes( pathPrefix + material.normalMapName ) );
                        Console.WriteLine( pathPrefix + material.normalMapName );
                    }

                    Console.WriteLine( "   - [" + ( hf++ ) + "/" + _materials.Count + "]: Material" );
                }

                hf = 1;
                Console.WriteLine( "Writing Meshes : " );
                // Meshes
                UInt64 numMeshes = (UInt64) _meshes.Count;
                br.Write( numMeshes );

                foreach ( Mesh mesh in _meshes ) {
                    UInt64 numVertices   = (UInt64) mesh.positions.Count;
                    UInt64 numIndices    = (UInt64) mesh.indices.Count;
                    UInt64 materialIndex = (UInt64) mesh.materialIndex;

                    br.Write( materialIndex );
                    br.Write( numVertices );
                    br.Write( numIndices );

                    for ( UInt64 i = 0; i < numVertices; i++ ) {
                        br.Write( mesh.positions[(int) i].x );
                        br.Write( mesh.positions[(int) i].y );
                        br.Write( mesh.positions[(int) i].z );

                        if ( mesh.normals.Count > (int) i ) {
                            br.Write( mesh.normals[(int) i].x );
                            br.Write( mesh.normals[(int) i].y );
                            br.Write( mesh.normals[(int) i].z );
                        }
                        //else {
                        //    br.Write( 0.0F );
                        //    br.Write( 0.0F );
                        //    br.Write( 0.0F );
                        //}

                        {
                            if ( mesh.tangents.Count > (int) i ) {
                                br.Write( mesh.tangents[(int) i].x );
                                br.Write( mesh.tangents[(int) i].y );
                                br.Write( mesh.tangents[(int) i].z );
                            }

                            //else {
                            //    br.Write( 0.0F );
                            //    br.Write( 0.0F );
                            //    br.Write( 0.0F );
                            //}
                        }

                        {
                            if ( mesh.uvs.Count > (int) i ) {
                                br.Write( mesh.uvs[(int) i].x );
                                br.Write( mesh.uvs[(int) i].y );
                            }

                            //else {
                            //    br.Write( 0.0F );
                            //    br.Write( 0.0F );
                            //}
                        }
                    }

                    for ( UInt64 i = 0; i < numIndices; i++ ) {
                        br.Write( mesh.indices[(int) i] );
                    }

                    Console.WriteLine( "   - [" + ( hf++ ) + "/" + _meshes.Count + "]: Mesh" );
                }
            }

            fex.Close();
            Console.WriteLine( "Finished!" );
            Console.ReadLine();

            _meshes.Clear();
            _materials.Clear();
        }
    }
}
