using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace OpenGLCSharp {

    internal static class Program {
        private static int Leks = 0;

        public static string ResourcePath => @"C:\0EE934262B1635B7\source\repos\OpenGLCSharp\OpenGLCSharp\";

        static Program() { }

        public static void ResourceLeak(Type type) {
            Console.WriteLine( "[Warning] Resource leaked: {0}.", type );
            Leks++;
        }

        private static void Main(string[] args) {
            //try {
                using ( Window w = new Window( 1000, 800, "Xuri´s OpenGl in C#" ) ) {
                    w.Run();
                }
            //} catch (Exception e) {
            //    Console.WriteLine( "Error: " + e.GetType() );
            //    Console.WriteLine( "Error: " + e.Message );
            //    //Console.WriteLine( string.Join(" ", e.Data.Keys ) );
            //    //Console.WriteLine( string.Join(" ", e.Data.Values ) );
            //    Console.WriteLine( e.Source );
            //    //Console.WriteLine( e.StackTrace );
            //    Console.ReadLine();
            //}
        }

        static readonly Finalizer finalizer = new Finalizer();

        sealed class Finalizer {
            ~Finalizer() {
                if ( Leks <= 0 ) return;
                
                GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced, true, false );
                ResourceLeak( typeof(Console) );
                Console.ReadLine();
            }
        }
    }
}
