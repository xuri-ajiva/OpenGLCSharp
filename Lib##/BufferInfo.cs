using System;
using System.Collections.Generic;
using OpenTK;

namespace SharedProject.SharedComponents {
    [Serializable]
    public class BufferInfo {
        public Vertex[]  Data                  { get; }
        public int[]     Indices               { get; }
        public Matrix4   TransformationsMatrix { get; set; }
        public List<int> TextureIds            { get; } = new List<int>();
        public int       IndicesCount          => this.Indices.Length;

        public BufferInfo(Vertex[] data, int[] indices) {
            this.Data                  = data;
            this.Indices               = indices;
            this.TransformationsMatrix = Matrix4.Identity;
        }

    }
}
