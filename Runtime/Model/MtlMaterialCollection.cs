using BlackTundra.Foundation.IO;
using BlackTundra.Foundation.Utility;
using BlackTundra.ModFramework.Media;

using System;
using System.IO;

using UnityEngine;

namespace BlackTundra.ModFramework.Model {

    public sealed class MtlMaterialCollection : ModMaterialCollection {

        #region variable

        #endregion

        #region property

        #endregion

        #region constructor

        internal MtlMaterialCollection(
            in ModInstance modInstance,
            in ulong guid,
            in FileSystemReference fsr,
            in string path
            ) : base(modInstance, guid, ModAssetType.MaterialMtlCollection, fsr, path) {
        }

        #endregion

        #region logic

        #region Import

        protected internal override void Import() {
            // remove exting material asset:
            DisposeOfMaterials();
            // load material data:
            if (!FileSystem.Read(fsr, out string mtl, FileFormat.Standard)) {
                throw new IOException($"Failed to read MTL file at `{fsr}`.");
            }
            // parse material data:
            ParseMaterialData(guid.ToHex(), mtl);
        }

        #endregion

        #region ParseMaterialData

        private void ParseMaterialData(in string name, in string mtl) {

            // Useful MTL Specifications & Documentation:
            // https://people.sc.fsu.edu/~jburkardt/data/mtl/mtl.html
            // MTL Example:
            // https://people.sc.fsu.edu/~jburkardt/data/mtl/example.mtl

            // read each line:
            string[] lines = mtl.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.RemoveEmptyEntries
            );
            // find number of lines:
            int lineCount = lines.Length;
            // validate there are at least 2 lines:
            if (lineCount < 2) return;
            // create temporary variables:
            string[] lineData;
            int lineDataCount;
            MtlMaterial currentMaterial = null;
            // iterate each line:
            for (int i = 0; i < lineCount; i++) {
                // get line data:
                lineData = lineData = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                lineDataCount = lineData.Length;
                // validate number of entries:
                if (lineDataCount < 2) continue; // ignore line
                // process line command:
                switch (lineData[0].ToLower()) {
                    case "newmtl": { // define a new material
                        if (currentMaterial != null) {
                            RegisterMaterial(currentMaterial);
                        }
                        string materialName = lineData[1];
                        currentMaterial = new MtlMaterial(
                            modInstance,
                            unchecked(guid + (ulong)i),
                            materialName
                        );
                        break;
                    }
                    case "ka": { // ambient colour (r=0.2,g=0.2,b=0.2)
                        // ambient colour is the colour of the material under ambient light (this is not used)
                        if (currentMaterial == null) throw new FormatException($"No material defined at line `{i}`.");
                        if (lineDataCount != 4) throw new FormatException($"3 arguments required for a colour.");
                        currentMaterial._ambientColour = new Color(
                            Mathf.Clamp01(float.Parse(lineData[1])),
                            Mathf.Clamp01(float.Parse(lineData[2])),
                            Mathf.Clamp01(float.Parse(lineData[3]))
                        );
                        break;
                    }
                    case "kd": { // diffuse colour (r=0.8,g=0.8,b=0.8)
                        // diffuse colour is the same as the base colour
                        if (currentMaterial == null) throw new FormatException($"No material defined at line `{i}`.");
                        if (lineDataCount != 4) throw new FormatException($"3 arguments required for a colour.");
                        currentMaterial._baseColour = new Color(
                            Mathf.Clamp01(float.Parse(lineData[1])),
                            Mathf.Clamp01(float.Parse(lineData[2])),
                            Mathf.Clamp01(float.Parse(lineData[3]))
                        );
                        break;
                    }
                    case "ks": { // specular colour (r=1.0,g=1.0,b=1.0)
                        if (currentMaterial == null) throw new FormatException($"No material defined at line `{i}`.");
                        if (lineDataCount != 4) throw new FormatException($"3 arguments required for a colour.");
                        currentMaterial._specularColour = new Color(
                            Mathf.Clamp01(float.Parse(lineData[1])),
                            Mathf.Clamp01(float.Parse(lineData[2])),
                            Mathf.Clamp01(float.Parse(lineData[3]))
                        );
                        break;
                    }
                    case "a": { // alpha (a=1.0) (opposite of `tr`), a=1.0 is not transparent at all, a=0.0 is completely transparent
                        if (currentMaterial == null) throw new FormatException($"No material defined at line `{i}`.");
                        currentMaterial._alpha = Mathf.Clamp01(float.Parse(lineData[1]));
                        break;
                    }
                    case "tr": { // transparency (t=0.0) (opposite or `a`), t=0.0 is not transparent at all, t=1.0 is completely transparent
                        if (currentMaterial == null) throw new FormatException($"No material defined at line `{i}`.");
                        currentMaterial._alpha = Mathf.Clamp01(1.0f - float.Parse(lineData[1]));
                        break;
                    }
                    case "ns": { // shininess of material (s=0.0)
                        if (currentMaterial == null) throw new FormatException($"No material defined at line `{i}`.");
                        currentMaterial._shininess = Mathf.Clamp01(float.Parse(lineData[1]));
                        break;
                    }
                    case "illum": { // illumination model (i=1)
                        /*
                         * i=1      flat material with no specular highlights (ks not used)
                         * i=2      material that uses specular light (ks is used)
                         */
                        if (currentMaterial == null) throw new FormatException($"No material defined at line `{i}`.");
                        currentMaterial._illuminationModel = (MtlIlluminationModel)int.Parse(lineData[1]);
                        break;
                    }
                    case "map_ka": { // names a file containing a texture map
                        if (currentMaterial == null) throw new FormatException($"No material defined at line `{i}`.");
                        FileSystemReference textureMapFsr = new FileSystemReference(string.Concat(fsr.GetParent().AbsolutePath, lineData[1]), false, false); // create new fsr for texture map
                        if (ModInstance.TryGetAsset(textureMapFsr, out ModTexture texture)) {
                            currentMaterial._baseMap = texture;
                        }
                        break;
                    }
                    case "#": break; // comment
                    default: break; // unknown command
                }
            }
            if (currentMaterial != null) {
                RegisterMaterial(currentMaterial);
            }
        }

        #endregion

        #endregion

    }

}