﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace JekyllLibrary.Library
{
    public partial class WWII
    {
        public class Localize : IXAssetPool
        {
            public override string Name => "Localize Entry";

            public override int Index => (int)XAssetType.localize;

            /// <summary>
            /// Structure of a WWII LocalizeEntry.
            /// </summary>
            private struct LocalizeEntry
            {
                public long Value { get; set; }
                public long Name { get; set; }
            }

            /// <summary>
            /// Load the valid XAssets for the Localize XAsset Pool.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>List of Localize XAsset objects.</returns>
            public override List<GameXAsset> Load(JekyllInstance instance)
            {
                Entries = instance.Reader.ReadStruct<long>(instance.Game.DBAssetPools + (Marshal.SizeOf<DBAssetPool>() * Index));
                PoolSize = instance.Reader.ReadStruct<uint>(instance.Game.DBAssetPoolSizes + (Marshal.SizeOf<DBAssetPoolSize>() * Index));

                Dictionary<string, string> entries = new Dictionary<string, string>();

                for (int i = 0; i < PoolSize; i++)
                {
                    LocalizeEntry header = instance.Reader.ReadStruct<LocalizeEntry>(Entries + Marshal.SizeOf<DBAssetPool>() + (i * Marshal.SizeOf<LocalizeEntry>()));

                    if (IsNullXAsset(header.Name))
                    {
                        continue;
                    }

                    string key = instance.Reader.ReadNullTerminatedString(header.Name).ToUpper();

                    if (entries.TryGetValue(key, out string _))
                    {
                        continue;
                    }

                    string value = instance.Reader.ReadNullTerminatedString(header.Value);
                    entries.Add(key, value);

                    Console.WriteLine($"Exported {Name} {key}");
                }

                string path = Path.Combine(instance.ExportPath, "localize.json");
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (StreamWriter file = File.CreateText(path))
                {
                    file.Write(JsonConvert.SerializeObject(entries, Formatting.Indented));
                }

                return new List<GameXAsset>();
            }

            /// <summary>
            /// Exports the specified Localize XAsset.
            /// </summary>
            /// <param name="xasset"></param>
            /// <param name="instance"></param>
            /// <returns>Status of the export operation.</returns>
            public override JekyllStatus Export(GameXAsset xasset, JekyllInstance instance)
            {
                return JekyllStatus.Success;
            }
        }
    }
}