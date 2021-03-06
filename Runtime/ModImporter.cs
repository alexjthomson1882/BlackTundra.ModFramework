using BlackTundra.Foundation;
using BlackTundra.Foundation.IO;

using System;

namespace BlackTundra.ModFramework {

    /// <summary>
    /// Class responsible for importing mods at runtime.
    /// </summary>
    public static class ModImporter {

        #region constant

        internal static readonly ConsoleFormatter ConsoleFormatter = new ConsoleFormatter(nameof(ModImporter));

        /// <summary>
        /// <see cref="FileSystemReference"/> to the mods directory.
        /// </summary>
        internal static readonly FileSystemReference ModDirectoryFSR = new FileSystemReference(
            FileSystem.LocalModsDirectory,
            true,
            true
        );

        /// <summary>
        /// Length of the <see cref="ModDirectoryFSR"/> absolute path.
        /// </summary>
        internal static readonly int ModDirectoryFSRLength = ModDirectoryFSR.AbsolutePath.Length;

        #endregion

        #region variable

        #endregion

        #region logic

        #region Initialise

        [CoreInitialise(-100000)]
        private static void Initialise() {
            ImportAll();
        }

        #endregion

        #region ReimportAll

        public static void ReimportAll() {
            ModInstance.UnloadAll();
            ImportAll();
        }

        #endregion

        #region ImportAll

        private static void ImportAll() {
            // find all subdirectories of the mods directory:
            FileSystemReference[] modDirectories = ModDirectoryFSR.GetDirectories();
            // estimate the number of mods by counting the subdirectories in the mods directory:
            int modCount = modDirectories.Length;
            if (modCount == 0) return; // no mods found
            // log total number of mods identified:
            ConsoleFormatter.Info($"{modCount} mods identified.");
            // try import each mod:
            FileSystemReference modFsr;
            int importCount = 0;
            for (int i = 0; i < modCount; i++) {
                modFsr = modDirectories[i];
                if (TryImport(modFsr, out _)) importCount++;
            }
            // log total number of successful imports:
            ConsoleFormatter.Info($"Imported {importCount} mods.");
            // validate dependencies of imported mods:
            ModInstance.ValidateDependencies();
            // recalculate mod processing order:
            ModInstance.RecalculateModProcessingOrder();
            // begin importing mod content:
            ModInstance.ReloadAllAssets();
        }

        #endregion

        #region TryImport

        public static bool TryImport(in string name, out ModInstance mod) {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (!ModInstance.ValidateName(name)) throw new ArgumentException("Invalid mod name.");
            return TryImport(GetModFSR(name), out mod);
        }

        public static bool TryImport(in FileSystemReference fsr, out ModInstance mod) {
            if (fsr == null) throw new ArgumentNullException(nameof(fsr));
            if (!fsr.IsDirectory) throw new ArgumentException($"{nameof(fsr)} is not a directory.");
            try {
                mod = new ModInstance(fsr);
            } catch (Exception exception) {
                ConsoleFormatter.Error($"Failed to import mod `{fsr.DirectoryName}`.", exception);
                mod = null;
                return false;
            }
            int assetCount = mod.AssetCount;
            int dependencyCount = mod.DependencyCount;
            ConsoleFormatter.Info(
                $"Imported mod `{fsr.DirectoryName}` with {dependencyCount} {(dependencyCount == 1 ? "dependency" : "dependencies")} and {assetCount} {(assetCount == 1 ? "asset" : "assets")}."
            );
            return true;
        }

        #endregion

        #region GetModFSR

        /// <summary>
        /// Creates a <see cref="FileSystemReference"/> to a mod directory.
        /// </summary>
        /// <param name="name">Name of the mod.</param>
        public static FileSystemReference GetModFSR(in string name) {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (!ModInstance.ValidateName(name)) throw new ArgumentException("Invalid mod name.");
            return new FileSystemReference(
                FileSystem.LocalModsDirectory + name + '/', // construct local mod directory path
                true, // is local
                true  // is directory
            );
        }

        #endregion

        #endregion

    }

}