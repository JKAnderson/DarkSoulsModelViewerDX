﻿using DarkSoulsModelViewerDX.GFXShaders;
using MeowDSIO;
using MeowDSIO.DataTypes.FLVER;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{


    public class FlverSubmeshRenderer
    {
        public BoundingBox Bounds;

        private struct FlverSubmeshRendererFaceSet
        {
            public int IndexCount;
            public IndexBuffer IndexBuffer;
            public bool BackfaceCulling;
            public bool IsTriangleStrip;
            public byte LOD;
        }

        public bool IsVisible = true;
        List<FlverSubmeshRendererFaceSet> MeshFacesets = new List<FlverSubmeshRendererFaceSet>();

        private bool HasNoLODs = true;

        VertexBuffer VertBuffer;

        public Texture2D TexDiffuse = null;
        public Texture2D TexSpecular = null;
        public Texture2D TexNormal = null;

        public GFXDrawStep DrawStep { get; private set; }

        public int VertexCount { get; private set; }

        public FlverSubmeshRenderer(FlverSubmesh f, TexturePool texPool)
        {
            var shortMaterialName = MiscUtil.GetFileNameWithoutDirectoryOrExtension(f.Material.MTDName);
            if (shortMaterialName.EndsWith("_Alp") || shortMaterialName.EndsWith("_Edge"))
            {
                DrawStep = GFXDrawStep._3_AlphaEdge;
            }
            else
            {
                DrawStep = GFXDrawStep._1_Opaque;
            }

            foreach (var matParam in f.Material.Parameters)
            {
                if (matParam.Name.ToUpper() == "G_DIFFUSE")
                    TexDiffuse = texPool.FetchTexture(matParam.Value);
                else if (matParam.Name.ToUpper() == "G_SPECULAR")
                    TexSpecular = texPool.FetchTexture(matParam.Value);
                else if (matParam.Name.ToUpper() == "G_BUMPMAP")
                    TexNormal = texPool.FetchTexture(matParam.Value);
            }

            var MeshVertices = new VertexPositionColorNormalTangentTexture[f.Vertices.Count];
            for (int i = 0; i < f.Vertices.Count; i++)
            {
                var vert = f.Vertices[i];
                MeshVertices[i] = new VertexPositionColorNormalTangentTexture();

                MeshVertices[i].Position = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);

                if (vert.Normal != null && vert.BiTangent != null)
                {
                    MeshVertices[i].Normal = Vector3.Normalize(new Vector3(vert.Normal.X, vert.Normal.Y, vert.Normal.Z));
                    MeshVertices[i].Tangent = Vector3.Normalize(new Vector3(vert.BiTangent.X, vert.BiTangent.Y, vert.BiTangent.Z));
                    MeshVertices[i].Binormal = Vector3.Cross(Vector3.Normalize(MeshVertices[i].Normal), Vector3.Normalize(MeshVertices[i].Tangent)) * vert.BiTangent.W;
                }

                if (vert.UVs.Count > 0)
                {
                    MeshVertices[i].TextureCoordinate = vert.UVs[0];
                }
                else
                {
                    MeshVertices[i].TextureCoordinate = Vector2.Zero;
                }
            }

            VertexCount = MeshVertices.Length;

            MeshFacesets = new List<FlverSubmeshRendererFaceSet>();

            foreach (var faceset in f.FaceSets)
            {
                var newFaceSet = new FlverSubmeshRendererFaceSet()
                {
                    BackfaceCulling = faceset.CullBackfaces,
                    IsTriangleStrip = faceset.IsTriangleStrip,
                    IndexBuffer = new IndexBuffer(
                                GFX.Device,
                                IndexElementSize.SixteenBits,
                                sizeof(short) * faceset.VertexIndices.Count,
                                BufferUsage.None),
                    IndexCount = faceset.VertexIndices.Count,
                };

                if (faceset.FlagsLOD1)
                {
                    newFaceSet.LOD = (byte)1;
                    HasNoLODs = false;
                }
                else if (faceset.FlagsLOD2)
                {
                    newFaceSet.LOD = (byte)2;
                    HasNoLODs = false;
                }

                newFaceSet.IndexBuffer.SetData(faceset.VertexIndices
                    .Select(x =>
                    {
                        if (x == ushort.MaxValue)
                            return (short)(-1);
                        else
                            return (short)x;
                    })
                    .ToArray());
                MeshFacesets.Add(newFaceSet);
            }

            Bounds = BoundingBox.CreateFromPoints(MeshVertices.Select(x => x.Position));

            VertBuffer = new VertexBuffer(GFX.Device,
                typeof(VertexPositionColorNormalTangentTexture), MeshVertices.Length, BufferUsage.WriteOnly);
            VertBuffer.SetData(MeshVertices);
        }

        public void Draw(int lod)
        {
            if (IsVisible && GFX.CurrentStep == DrawStep)
            {
                if (GFX.EnableTextures)
                {
                    GFX.FlverShader.ColorMap = TexDiffuse ?? MODEL_VIEWER_MAIN.DEFAULT_TEXTURE_DIFFUSE;
                    GFX.FlverShader.SpecularMap = TexSpecular ?? MODEL_VIEWER_MAIN.DEFAULT_TEXTURE_SPECULAR;
                    GFX.FlverShader.NormalMap = TexNormal ?? MODEL_VIEWER_MAIN.DEFAULT_TEXTURE_NORMAL;
                }
                

                foreach (var technique in GFX.CurrentFlverRenderEffect.Techniques)
                {
                    GFX.CurrentFlverRenderEffect.CurrentTechnique = technique;
                    foreach (EffectPass pass in GFX.CurrentFlverRenderEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        GFX.Device.SetVertexBuffer(VertBuffer);

                        foreach (var faceSet in MeshFacesets)
                        {
                            if (!HasNoLODs && faceSet.LOD != lod)
                                continue;

                            GFX.Device.Indices = faceSet.IndexBuffer;

                            GFX.BackfaceCulling = faceSet.BackfaceCulling;

                            GFX.Device.DrawIndexedPrimitives(faceSet.IsTriangleStrip ? PrimitiveType.TriangleStrip : PrimitiveType.TriangleList, 0, 0,
                                faceSet.IsTriangleStrip ? (faceSet.IndexCount - 2) : (faceSet.IndexCount / 3));

                        }
                    }
                }

                

                
            }
        }
    }
}
