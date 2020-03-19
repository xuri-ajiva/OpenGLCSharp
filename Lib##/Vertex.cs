using System;
using System.Diagnostics;

namespace SharedProject.SharedComponents {
    [Serializable]
    public struct Vertex {
        private float x, y, z;
        private float u, v;

        public Vertex(float x, float y, float z, float u, float v) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.u = u;
            this.v = v;
        }

        public float X {
            [DebuggerStepThrough] get => this.x;
        }

        public float Y {
            [DebuggerStepThrough] get => this.y;
        }

        public float Z {
            [DebuggerStepThrough] get => this.z;
        }

        public float U {
            [DebuggerStepThrough] get => this.u;
        }

        public float V {
            [DebuggerStepThrough] get => this.v;
        }
    }
}
