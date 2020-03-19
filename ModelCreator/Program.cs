using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Assimp;
using OpenTK;
using SharedProject.SharedComponents;
using mat = SharedProject.SharedComponents.Material;
using meshData = SharedProject.SharedComponents.MeshData;

namespace ModelCreator {
    public static class Extentions {
        public static OpenTK.Vector4 V(this  Color4D  color) => new OpenTK.Vector4( color.R, color.G, color.B, color.A );
        public static OpenTK.Vector3 V(this  Vector3D vec)   => new OpenTK.Vector3( vec.X, vec.Y, vec.Z );
        public static OpenTK.Vector2 VX(this Vector3D vec)   => new OpenTK.Vector2( vec.X, vec.Y );
    }

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


        static meshData ProcessMesh(Assimp.Mesh mesh, Assimp.Scene scene) {
            Vector3[] post = mesh.Vertices.Select( x => x.V() ).ToArray();
            Vector3[] norm = mesh.Normals.Select( x => x.V() ).ToArray();
            Vector3[] tang = mesh.Tangents.Select( x => x.V() ).ToArray();
            Vector2[] uves = mesh.TextureCoordinateChannels[0].Select( x => x.VX() ).ToArray();
            int[]     inde = mesh.GetIndices();

            var materialIndex = mesh.MaterialIndex;

            return new meshData( post, inde, norm, tang, uves );
        }

        static IEnumerable<MeshData> ProcessNode(Node node, Scene scene) {
            for ( int i = 0; i < node.MeshCount; i++ ) {
                var mesh = scene.Meshes[node.MeshIndices[i]];
                yield return ProcessMesh( mesh, scene );
            }

            for ( int i = 0; i < node.ChildCount; i++ ) {
                foreach ( var data in ProcessNode( node.Children[i], scene ) ) {
                    yield return data;
                }
            }
        }

        static IEnumerable<mat> ProcessMaterials(Scene scene, string directoryName, Func<string, string, string> textureNotFoundCallback = default, bool forceUseDiffuseMap = true, bool forceUseNormalMap = true) {
            foreach ( var material in scene.Materials ) {
                var diffuse = material.ColorDiffuse;

                var   specular  = material.ColorSpecular;
                var   emissive  = material.ColorEmissive;
                float shininess = material.Shininess;

                float shininessStrength = material.ShininessStrength;

                specular.R *= shininessStrength;
                specular.G *= shininessStrength;
                specular.B *= shininessStrength;

                string normalMapPath;
                string diffuseMapPath;

                if ( material.HasTextureDiffuse ) {
                    material.GetMaterialTexture( TextureType.Diffuse, 0, out var diffuseMapName );

                    if ( File.Exists(directoryName+ diffuseMapName.FilePath ) ) {
                        diffuseMapPath = directoryName + diffuseMapName.FilePath;
                    }
                    else {
                        diffuseMapPath = textureNotFoundCallback?.Invoke( diffuseMapName.FilePath, directoryName );
                    }
                }
                else if ( forceUseDiffuseMap ) {
                    diffuseMapPath = textureNotFoundCallback?.Invoke( nameof(diffuseMapPath), directoryName );
                }
                else {
                    Console.WriteLine( "No Diffuse Map" );
                    diffuseMapPath = default;
                }

                if ( material.HasTextureNormal ) {
                    material.GetMaterialTexture( TextureType.Normals, 0, out var normalMapName );

                    if ( File.Exists(directoryName+ normalMapName.FilePath ) ) {
                        normalMapPath = directoryName + normalMapName.FilePath;
                    }
                    else {
                        normalMapPath = textureNotFoundCallback?.Invoke( normalMapName.FilePath, directoryName );
                    }
                }
                else if ( forceUseNormalMap ) {
                    normalMapPath = textureNotFoundCallback?.Invoke( nameof(normalMapPath), directoryName );
                }
                else {
                    Console.WriteLine( "No Normal Map" );
                    normalMapPath = default;
                }

                var materialNew = new mat( diffuse.V(), specular.V(), emissive.V(), shininess, diffuseMapPath, normalMapPath );

                yield return materialNew;
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

        [STAThread]
        static void Main(string[] args) {
            var model = @"C:\Users\admin\Desktop\untitled.fbx";

            string        directory = Path.GetDirectoryName( model );
            AssimpContext importer  = new AssimpContext();

            var scene = importer.ImportFile( model, PostProcessSteps.PreTransformVertices | PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals | PostProcessSteps.OptimizeMeshes | PostProcessSteps.OptimizeGraph | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.ImproveCacheLocality | PostProcessSteps.CalculateTangentSpace );

            var sceneData = CreateSceneData( scene, model );

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension( model );
            string outputFilename           = directory + "\\" + fileNameWithoutExtension + ".bmf";
            Console.WriteLine( "OutputFile: "                                             + outputFilename );

            WriteSceneData( sceneData, outputFilename );
        }

        private static void WriteSceneData(SceneData sceneData, string outputFilename) {
            File.Delete( outputFilename );
            var fex = File.Open( outputFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite );
            var bf = new BinaryFormatter();
            
            bf.Serialize( fex, sceneData );
            
            fex.Close();
            Console.WriteLine( "Finished!" );
            Console.ReadLine();

            _meshes.Clear();
            _materials.Clear();
        }

        private static SceneData CreateSceneData(Scene scene, string model) {
            var mats  = ProcessMaterials( scene, Path.GetDirectoryName( model ) + "\\", TextureNotFoundCallback);
            var nodes = ProcessNode( scene.RootNode, scene );

            SceneData sceneData = SceneData.Converter(mats, nodes);

            var ext = Path.GetExtension( model );

            if ( ext == ".fbx" ) {
               sceneData.TransFormMatrix *= Matrix4.CreateScale( .1f );
            }


            return sceneData;
        }

        private static string TextureNotFoundCallback(string arg1, string arg2) {
            Console.WriteLine("TextureNotFound: " + arg1 );
            Console.WriteLine("Location for search: " + arg2 );


            OpenFileDialog o = new OpenFileDialog { Title = arg1, InitialDirectory = arg2 };

            if ( o.ShowDialog() == DialogResult.OK ) {
                return o.FileName;
            }


            return arg1;
        }
    }
}
