#region using

using System.Diagnostics;
using OpenTK;

#endregion

namespace SharedProject.SharedComponents {
    public class Material {
        [DebuggerStepThrough]
        public Material() {
            this.ColorDiffuse              = default;
            this.ColorSpecular             = default;
            this.ColorEmissive             = default;
            this.Shininess                 = default;
            this.DiffuseMapNameFilePath    = default;
            this.NormalMapNameFilePath     = default;
            this.HasNormalMapNameFilePath  = default;
            this.HasDiffuseMapNameFilePath = default;
        }

        public Material(Vector4 colorDiffuse) => this.ColorDiffuse = colorDiffuse;

        public Material(Vector4 colorDiffuse, Vector4 colorSpecular) : this( colorDiffuse ) => this.ColorSpecular = colorSpecular;

        public Material(Vector4 colorDiffuse, Vector4 colorSpecular, Vector4 colorEmissive, float shininess) : this( colorDiffuse, colorSpecular, colorEmissive ) => this.Shininess = shininess;

        public Material(Vector4 colorDiffuse, Vector4 colorSpecular, Vector4 colorEmissive) : this( colorDiffuse, colorSpecular ) => this.ColorEmissive = colorEmissive;

        public Material(Vector4 colorDiffuse, Vector4 colorSpecular, Vector4 colorEmissive, float shininess, string diffuseMapNameFilePath = default, string normalMapNameFilePath = default, bool OverrideHasDiffuseMapNameFilePath = true, bool OverrideHasNormalMapNameFilePath = true) : this( colorDiffuse, colorSpecular, colorEmissive, shininess ) {
            this.DiffuseMapNameFilePath    = diffuseMapNameFilePath;
            this.NormalMapNameFilePath     = normalMapNameFilePath;
            this.HasNormalMapNameFilePath  = OverrideHasNormalMapNameFilePath  && !string.IsNullOrEmpty( normalMapNameFilePath );
            this.HasDiffuseMapNameFilePath = OverrideHasDiffuseMapNameFilePath && !string.IsNullOrEmpty( diffuseMapNameFilePath );
        }


        public Vector4 ColorDiffuse { [DebuggerStepThrough] get; }

        public Vector4 ColorSpecular { [DebuggerStepThrough] get; }

        public Vector4 ColorEmissive { [DebuggerStepThrough] get; }

        public float Shininess { [DebuggerStepThrough] get; }

        public bool HasDiffuseMapNameFilePath { [DebuggerStepThrough] get; }

        public string DiffuseMapNameFilePath { [DebuggerStepThrough] get; }

        public bool HasNormalMapNameFilePath { [DebuggerStepThrough] get; }

        public string NormalMapNameFilePath { [DebuggerStepThrough] get; }
    }

    public class MeshData {
        [DebuggerStepThrough]
        public MeshData() {
            this.Positions     = default;
            this.Normals       = default;
            this.Tangents      = default;
            this.Uvs           = default;
            this.Indices       = default;
            this.MaterialIndex = default;
        }


        public MeshData(Vector3[] positions) : this() => this.Positions = positions;

        public MeshData(Vector3[] positions, int[] indices) : this( positions ) => this.Indices = indices;

        public MeshData(Vector3[] positions, int[] indices, Vector3[] normals, Vector3[] tangents) : this( positions, indices ) {
            this.Normals  = normals;
            this.Tangents = tangents;
        }

        public MeshData(Vector3[] positions, int[] indices, Vector3[] normals, Vector3[] tangents, Vector2[] uvs) : this( positions, indices, normals, tangents ) => this.Uvs = uvs;

        public MeshData(Vector3[] positions, int[] indices, Vector3[] normals, Vector3[] tangents, Vector2[] uvs, int materialIndex) : this( positions, indices, normals, tangents, uvs ) => this.MaterialIndex = materialIndex;


        public Vector3[] Positions { [DebuggerStepThrough] get; }

        public int[] Indices { [DebuggerStepThrough] get; }

        public Vector3[] Normals { [DebuggerStepThrough] get; }

        public Vector3[] Tangents { [DebuggerStepThrough] get; }

        public Vector2[] Uvs { [DebuggerStepThrough] get; }

        public int MaterialIndex { [DebuggerStepThrough] get; }
    }
}
