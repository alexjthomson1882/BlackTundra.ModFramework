using System;

using Version = BlackTundra.Foundation.Version;

namespace BlackTundra.ModFramework {

    /// <summary>
    /// Describes a dependency for a <see cref="ModInstance"/>.
    /// </summary>
    public struct ModDependency {

        #region variable

        /// <summary>
        /// Name of the <see cref="ModInstance"/>.
        /// </summary>
        public readonly string name;

        /// <summary>
        /// Version of the <see cref="ModInstance"/>.
        /// </summary>
        public readonly Version version;

        #endregion

        #region constructor

        internal ModDependency(in string name, in Version version) {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (!ModInstance.ValidateName(name)) throw new FormatException("Invalid dependency name.");
            this.name = name;
            this.version = version;
        }

        #endregion

        #region logic

        public override string ToString() => $"{name}: {version}";

        #endregion

    }

}