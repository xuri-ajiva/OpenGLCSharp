#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;

#endregion

namespace SharedProject.SharedComponents {
    [Serializable]
    public class SceneData {
        private readonly List<BufferInfo>    Meshes = new List<BufferInfo>();
        private          Vector3             position;
        private          List<(int, string)> textureIdList = new List<(int, string)>();
        private          Matrix4             transFormMatrix;


        public static SceneData Converter(IEnumerable<Material> mats, IEnumerable<MeshData> nodes) {
            var tmp = new SceneData();

            foreach ( var meshData in nodes ) {
                var vertexte = meshData.Positions.Select( (vector3, i) => new Vertex( vector3.X, vector3.Y, vector3.Z, meshData.Uvs[i].X, meshData.Uvs[i].Y ) ).ToArray();

                var info = new BufferInfo( vertexte, meshData.Indices );
                tmp.Meshes.Add( info );
            }

            tmp.textureIdList.AddRange( mats.Select( (x,i)=> (i,x.DiffuseMapNameFilePath) ).ToArray() );

            return tmp;
        }

        public SceneData() {
            transFormMatrix = Matrix4.Identity;
            position = Vector3.Zero;
            ;
        }

        public List<BufferInfo> Meshes1 {
            [DebuggerStepThrough] get => this.Meshes;
        }

        public Vector3 Position {
            [DebuggerStepThrough] get => this.position;
        }

        public List<(int, string)> TextureIdList {
            [DebuggerStepThrough] get => this.textureIdList;
        }

        public Matrix4 TransFormMatrix {
            [DebuggerStepThrough] get => this.transFormMatrix;
            [DebuggerStepThrough] set => this.transFormMatrix = value;
        }
    }
}
