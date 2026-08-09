using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using static LilyOfValley.EditorTools.EditorAssetUtility;

namespace LilyOfValley.EditorTools
{
    public static class GroundGridFactory
    {
        #region Field

        public const float SizeUnits = 100f;

        private const float PlanePrimitiveSize = 10f;
        private const string LitShaderName = "Universal Render Pipeline/Lit";
        private const string TextureFolder = "Assets/_Assets/Texture";
        private const string MaterialFolder = "Assets/_Assets/Material";
        private const string TexturePath = TextureFolder + "/GridCell.png";
        private const string MaterialPath = MaterialFolder + "/Ground_Grid.mat";
        private const string ApplyMaterialUndoName = "Apply Grid Material";

        private const int Resolution = 256;
        private const int MajorLineWidth = 6;
        private const int MinorLineWidth = 2;
        private const int GridAnisoLevel = 8;

        private const float GridSmoothness = 0.08f;
        private const float GridMetallic = 0f;

        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");

        private static readonly Color32 CellColor = (Color32)new Color(0.24f, 0.25f, 0.27f, 1f);
        private static readonly Color32 MinorLineColor = (Color32)new Color(0.31f, 0.32f, 0.35f, 1f);
        private static readonly Color32 MajorLineColor = (Color32)new Color(0.62f, 0.64f, 0.68f, 1f);

        #endregion

        #region Property

        public static float PlaneScale => SizeUnits / PlanePrimitiveSize;

        #endregion

        #region Ground Creation

        public static GameObject CreateGround(string name)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = name;
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(PlaneScale, 1f, PlaneScale);
            ground.isStatic = true;
            ApplyGridMaterial(ground);

            return ground;
        }

        public static void ApplyGridMaterial(GameObject target)
        {
            if (target == null) return;

            if (!target.TryGetComponent<MeshRenderer>(out var renderer)) return;

            var material = EnsureGridMaterial();
            if (material == null) return;

            Undo.RecordObject(renderer, ApplyMaterialUndoName);
            renderer.sharedMaterial = material;
        }

        #endregion

        #region Grid Material

        public static Material EnsureGridMaterial()
        {
            EnsureFolder(MaterialFolder);

            var texture = EnsureGridTexture();
            if (texture == null) return null;

            var material = LoadOrCreateGridMaterial();
            if (material == null) return null;

            material.SetTexture(BaseMapId, texture);
            material.SetTextureScale(BaseMapId, new Vector2(SizeUnits, SizeUnits));
            material.SetColor(BaseColorId, Color.white);
            material.SetFloat(SmoothnessId, GridSmoothness);
            material.SetFloat(MetallicId, GridMetallic);

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();

            return material;
        }

        private static Material LoadOrCreateGridMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null) return material;

            var shader = Shader.Find(LitShaderName);
            if (shader == null)
            {
                Debug.LogError($"{nameof(GroundGridFactory)}: shader '{LitShaderName}' not found; is URP installed?");
                return null;
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, MaterialPath);

            return material;
        }

        #endregion

        #region Grid Texture

        private static Texture2D EnsureGridTexture()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (existing != null) return existing;

            EnsureFolder(TextureFolder);
            WriteGridTexture();
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);
            ConfigureImporter();

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (texture == null) Debug.LogError($"{nameof(GroundGridFactory)}: failed to import '{TexturePath}'.");

            return texture;
        }

        private static void WriteGridTexture()
        {
            var center = Resolution / 2;
            var pixels = new Color32[Resolution * Resolution];

            for (var y = 0; y < Resolution; y++)
            {
                for (var x = 0; x < Resolution; x++)
                {
                    pixels[(y * Resolution) + x] = ResolveCellColor(x, y, center);
                }
            }

            var texture = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();

            var bytes = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);
            File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), TexturePath), bytes);
        }

        private static Color32 ResolveCellColor(int x, int y, int center)
        {
            if (x < MajorLineWidth || y < MajorLineWidth) return MajorLineColor;

            if (Mathf.Abs(x - center) < MinorLineWidth || Mathf.Abs(y - center) < MinorLineWidth) return MinorLineColor;

            return CellColor;
        }

        private static void ConfigureImporter()
        {
            if (AssetImporter.GetAtPath(TexturePath) is not TextureImporter importer) return;

            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = GridAnisoLevel;
            importer.mipmapEnabled = true;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.SaveAndReimport();
        }

        #endregion
    }
}
