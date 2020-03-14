using OpenTK;

namespace OpenGLCSharp {

    public struct Vertex {
        public float x, y, z;
        public float u, v;

        public Vertex(float x, float y, float z, float u, float v) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.u = u;
            this.v = v;
        }

    }
}
