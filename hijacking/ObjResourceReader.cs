using System.Globalization;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace hijacking
{
    internal class ObjResourceReader
    {
        public static unsafe GlObject CreateAirbus(GL Gl)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);
            int wingIndex = 0;

            List<float[]> objVertices;
            List<(int Vertex, int Texture,int Normal)[]> objFaces;
            List<float[]> objNormals;
            List<float[]> objTextures;
            Hitbox hitbox;
            
            ReadObjDataForAirbus(out objVertices, out objFaces, out objNormals, out objTextures, out hitbox);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(objVertices, objFaces, objNormals, objTextures, glVertices, glColors, glIndices);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices, "body.png", hitbox);
        }
        public static unsafe GlObject CreateRoad(GL Gl)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);
            int wingIndex = 0;

            List<float[]> objVertices;
            List<(int Vertex, int Texture,int Normal)[]> objFaces;
            List<float[]> objNormals;
            List<float[]> objTextures;
            Hitbox hitbox;
            
            ReadObjDataForRoad(out objVertices, out objFaces, out objNormals, out objTextures, out hitbox);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(objVertices, objFaces, objNormals, objTextures, glVertices, glColors, glIndices);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices, "road.png", hitbox);
        }

        public static unsafe GlObject CreateFighterJet(GL Gl)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<(int Vertex, int Texture,int Normal)[]> objFaces;
            List<float[]> objNormals;
            List<float[]> objTextures;
            Hitbox hitbox;
            
            ReadObjDataForFighter(out objVertices, out objFaces, out objNormals, out objTextures, out hitbox);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(objVertices, objFaces, objNormals, objTextures, glVertices, glColors, glIndices);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices, "fighter_body.jpg", hitbox);
        }

        private static unsafe GlObject CreateOpenGlObject(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices, String textureFile, Hitbox hitbox = null)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint offsetTexture = offsetNormal + (3 * sizeof(float));
            uint vertexSize = offsetTexture + (2 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();

            // set texture
            // create texture
            uint texture = Gl.GenTexture();
            // activate texture 0
            Gl.ActiveTexture(TextureUnit.Texture0);
            // bind texture
            Gl.BindTexture(TextureTarget.Texture2D, texture);

            var imageBody = ReadTextureImage(textureFile);
            var textureBytes = (ReadOnlySpan<byte>)imageBody.Data.AsSpan();
            // Here we use "result.Width" and "result.Height" to tell OpenGL about how big our texture is.
            Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)imageBody.Width,
                (uint)imageBody.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureBytes);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            // unbinde texture
            Gl.BindTexture(TextureTarget.Texture2D, 0);

            Gl.EnableVertexAttribArray(3);
            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetTexture);


            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.ToArray().Length;


            return new GlObject(vao, vertices, colors, indices, indexArrayLength, Gl, texture, hitbox);
        }

        private static unsafe void CreateGlArraysFromObjArrays(List<float[]> objVertices, List<(int Vertex, int Texture,int Normal)[]> objFaces, List<float[]>objNormals, List<float[]>objTextures, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            foreach (var objFace in objFaces)
            {
                ProcessVertexOnFace(objVertices, objNormals, objTextures, glVertices, glIndices, objFace, 1, glVertexIndices);
                ProcessVertexOnFace(objVertices, objNormals, objTextures, glVertices, glIndices, objFace, 2, glVertexIndices);
                ProcessVertexOnFace(objVertices, objNormals, objTextures, glVertices, glIndices, objFace, 3, glVertexIndices);
                
                ProcessVertexOnFace(objVertices, objNormals, objTextures, glVertices, glIndices, objFace, 0, glVertexIndices);
                ProcessVertexOnFace(objVertices, objNormals, objTextures, glVertices, glIndices, objFace, 1, glVertexIndices);
                ProcessVertexOnFace(objVertices, objNormals, objTextures, glVertices, glIndices, objFace, 3, glVertexIndices);
            }
        }

        private static void ProcessVertexOnFace(List<float[]> objVertices, List<float[]> objNormals, List<float[]> objTextures, List<float> glVertices,
            List<uint> glIndices, (int Vertex, int Texture, int Normal)[] objFace, int i, Dictionary<string, int> glVertexIndices)
        {
            var objVertex = objVertices[objFace[i].Vertex - 1];
            var objTexture = objTextures[objFace[i].Texture - 1];
            var objNormal = objNormals[objFace[i].Normal - 1];
            // create gl description of vertex
            List<float> glVertex = new List<float>();
            glVertex.AddRange(objVertex);
            glVertex.Add(objNormal[0]);
            glVertex.Add(objNormal[1]);
            glVertex.Add(objNormal[2]);
            glVertex.Add(objTexture[0]);
            glVertex.Add(1 - objTexture[1]);
            //glVertex.AddRange(objTexture); // Add texture coordinates
            // add textrure, color

            // check if vertex exists
            var glVertexStringKey = string.Join(" ", glVertex);
            if (!glVertexIndices.ContainsKey(glVertexStringKey))
            {
                glVertices.AddRange(glVertex);
                // glColors.AddRange(objTexture);
                glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
            }

            // add vertex to triangle indices
            glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
        }

        private static unsafe void ReadObjDataForAirbus(out List<float[]> objVertices, out List<(int Vertex, int Texture,int Normal)[]> objFaces, out List<float[]>objNormals, out List<float[]>objTextures, out Hitbox hitbox)
        {
            objVertices = new List<float[]>();
            objFaces = new List<(int Vertex, int Texture,int Normal)[]>();
            objNormals = new List<float[]>();
            objTextures = new List<float[]>();

            int max_x = -100000;
            int max_y = -100000;
            int max_z = -100000;
            int min_x = 100000;
            int min_y = 100000;
            int min_z = 100000;
            

            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("hijacking.Resources.airbus.plane.obj"))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine().Replace("  ", " ");
                    if (line.Trim().StartsWith("# object airplane_body"))
                    {
                        Console.WriteLine("Found airplane_body");
                    } else if (line.Trim().StartsWith("# object airplane_wings"))
                    {
                        Console.WriteLine("Found airplane_wings");
                    }
                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;


                    
                    if (line.Trim().StartsWith("vn") || line.Trim().StartsWith("f") || line.Trim().StartsWith("v") || line.Trim().StartsWith("vt"))
                    {
                        var lineClassifier = line.Substring(0, line.IndexOf(' '));
                        var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                        switch (lineClassifier)
                        {
                            case "v":
                                float[] vertex = new float[3];
                                for (int i = 0; i < vertex.Length; ++i)
                                    vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                                objVertices.Add(vertex);
                                if (vertex[0] > max_x)
                                    max_x = (int)vertex[0];
                                if (vertex[1] > max_y)
                                    max_y = (int)vertex[1];
                                if (vertex[2] > max_z)
                                    max_z = (int)vertex[2];
                                if (vertex[0] < min_x)
                                    min_x = (int)vertex[0];
                                if (vertex[1] < min_y)
                                    min_y = (int)vertex[1];
                                if (vertex[2] < min_z)
                                    min_z = (int)vertex[2];
                                break;
                            case "f":
                                (int Vertex, int Texture,int Normal)[] face = new (int Vertex, int Texture,int Normal)[4];
                                for (int i = 0; i < face.Length; ++i)
                                    face[i] = new (int.Parse(lineData[i].Split('/')[0], CultureInfo.InvariantCulture), int.Parse(lineData[i].Split('/')[1], CultureInfo.InvariantCulture), int.Parse(lineData[i].Split('/')[2], CultureInfo.InvariantCulture));
                                objFaces.Add(face);
                                break;
                            case "vn":
                                float[] normal = new float[3];
                                for (int i = 0; i < normal.Length; i++)
                                    normal[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                                objNormals.Add(normal);
                                break;
                            case "vt":
                                float[] texture = new float[2];
                                for (int i = 0; i < texture.Length; i++)
                                    texture[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                                objTextures.Add(texture);
                                break;
                        }    
                    }
                    
                } 
                
            }
            hitbox = new Hitbox((min_x, min_y, min_z), (max_x, max_y, max_z));
        }
        
        private static unsafe void ReadObjDataForFighter(out List<float[]> objVertices, out List<(int Vertex, int Texture,int Normal)[]> objFaces, out List<float[]>objNormals, out List<float[]>objTextures, out Hitbox hitbox)
        {
            objVertices = new List<float[]>();
            objFaces = new List<(int Vertex, int Texture,int Normal)[]>();
            objNormals = new List<float[]>();
            objTextures = new List<float[]>();
            int max_x = -100000;
            int max_y = -100000;
            int max_z = -100000;
            int min_x = 100000;
            int min_y = 100000;
            int min_z = 100000;


            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("hijacking.Resources.airbus.fighter.obj"))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine().Replace("  ", " ");
                    if (line.Trim().StartsWith("vn") || line.Trim().StartsWith("f") || line.Trim().StartsWith("v") || line.Trim().StartsWith("vt"))
                    {
                        var lineClassifier = line.Substring(0, line.IndexOf(' '));
                        var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                        switch (lineClassifier)
                        {
                            case "v":
                                float[] vertex = new float[3];
                                for (int i = 0; i < vertex.Length; ++i)
                                    vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture) * 100;
                                objVertices.Add(vertex);
                                if (vertex[0] > max_x)
                                    max_x = (int)vertex[0];
                                if (vertex[1] > max_y)
                                    max_y = (int)vertex[1];
                                if (vertex[2] > max_z)
                                    max_z = (int)vertex[2];
                                if (vertex[0] < min_x)
                                    min_x = (int)vertex[0];
                                if (vertex[1] < min_y)
                                    min_y = (int)vertex[1];
                                if (vertex[2] < min_z)
                                    min_z = (int)vertex[2];
                                break;
                            case "f":
                                (int Vertex, int Texture,int Normal)[] face = new (int Vertex, int Texture,int Normal)[4];
                                for (int i = 0; i < face.Length; ++i)
                                    face[i] = new (int.Parse(lineData[i].Split('/')[0], CultureInfo.InvariantCulture), int.Parse(lineData[i].Split('/')[1], CultureInfo.InvariantCulture), int.Parse(lineData[i].Split('/')[2], CultureInfo.InvariantCulture));
                                objFaces.Add(face);
                                break;
                            case "vn":
                                float[] normal = new float[3];
                                for (int i = 0; i < normal.Length; i++)
                                    normal[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                                objNormals.Add(normal);
                                break;
                            case "vt":
                                float[] texture = new float[2];
                                for (int i = 0; i < texture.Length; i++)
                                    texture[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                                objTextures.Add(texture);
                                break;
                        }    
                    }
                    
                } 
                
            }
            hitbox = new Hitbox((min_x, min_y, min_z), (max_x, max_y, max_z));
        }
        private static unsafe void ReadObjDataForRoad(out List<float[]> objVertices, out List<(int Vertex, int Texture,int Normal)[]> objFaces, out List<float[]>objNormals, out List<float[]>objTextures, out Hitbox hitbox)
        {
            objVertices = new List<float[]>();
            objFaces = new List<(int Vertex, int Texture,int Normal)[]>();
            objNormals = new List<float[]>();
            objTextures = new List<float[]>();
            int max_x = -100000;
            int max_y = -100000;
            int max_z = -100000;
            int min_x = 100000;
            int min_y = 100000;
            int min_z = 100000;
            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("hijacking.Resources.airbus.road.obj"))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine().Replace("  ", " ");
                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;


                    
                    if (line.Trim().StartsWith("vn") || line.Trim().StartsWith("f") || line.Trim().StartsWith("v") || line.Trim().StartsWith("vt"))
                    {
                        var lineClassifier = line.Substring(0, line.IndexOf(' '));
                        var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                        switch (lineClassifier)
                        {
                            case "v":
                                float[] vertex = new float[3];
                                for (int i = 0; i < vertex.Length; ++i)
                                    vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture) * 300;
                                objVertices.Add(vertex);
                                if (vertex[0] > max_x)
                                    max_x = (int)vertex[0];
                                if (vertex[1] > max_y)
                                    max_y = (int)vertex[1];
                                if (vertex[2] > max_z)
                                    max_z = (int)vertex[2];
                                if (vertex[0] < min_x)
                                    min_x = (int)vertex[0];
                                if (vertex[1] < min_y)
                                    min_y = (int)vertex[1];
                                if (vertex[2] < min_z)
                                    min_z = (int)vertex[2];
                                break;
                            case "f":
                                (int Vertex, int Texture,int Normal)[] face = new (int Vertex, int Texture,int Normal)[4];
                                for (int i = 0; i < face.Length; ++i)
                                    face[i] = new (int.Parse(lineData[i].Split('/')[0], CultureInfo.InvariantCulture), int.Parse(lineData[i].Split('/')[1], CultureInfo.InvariantCulture), int.Parse(lineData[i].Split('/')[2], CultureInfo.InvariantCulture));
                                objFaces.Add(face);
                                break;
                            case "vn":
                                float[] normal = new float[3];
                                for (int i = 0; i < normal.Length; i++)
                                    normal[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                                objNormals.Add(normal);
                                break;
                            case "vt":
                                float[] texture = new float[2];
                                for (int i = 0; i < texture.Length; i++)
                                    texture[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                                objTextures.Add(texture);
                                break;
                        }    
                    }
                    
                } 
                
            }
            hitbox = new Hitbox((min_x, min_y, min_z), (max_x, max_y, max_z));
        }
        
        private static unsafe ImageResult ReadTextureImage(string textureResource)
        {
            ImageResult result;
            using (Stream skyeboxStream
                   = typeof(GlCube).Assembly.GetManifestResourceStream("hijacking.Resources.airbus." + textureResource))
                result = ImageResult.FromStream(skyeboxStream, ColorComponents.RedGreenBlueAlpha);

            return result;
        }

        

        
    }
}
