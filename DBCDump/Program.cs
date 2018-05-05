﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WoWFormatLib.DBC;
namespace DBCDump
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Not enough arguments! Require source and target.");
                return;
            }

            if (!File.Exists(args[0]))
            {
                throw new FileNotFoundException("File " + args[0] + " could not be found!");
            }

            DB2Reader reader;

            using (var stream = File.Open(args[0], FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var bin = new BinaryReader(stream))
            {
                var identifier = new string(bin.ReadChars(4));
                stream.Position = 0;
                switch (identifier)
                {
                    case "WDC2":
                        reader = new WDC2Reader(stream);
                        break;
                    case "WDC1":
                        reader = new WDC1Reader(stream);
                        break;
                    default:
                        throw new Exception("DBC type " + identifier + " is not supported!");
                }
            }

            var dbd = new DBDefsLib.Structs.DBDefinition();
            foreach (var file in Directory.GetFiles("definitions/"))
            {
                if (Path.GetFileNameWithoutExtension(file).ToLower() == Path.GetFileNameWithoutExtension(args[0]).ToLower())
                {
                    var dbdef = new DBDefsLib.DBDReader();
                    dbd = dbdef.Read(file);
                }
            }

            var layoutHash = reader.LayoutHash.ToString("X8").ToUpper();

            foreach (var versionDef in dbd.versionDefinitions)
            {
                if (versionDef.layoutHashes.Any(layoutHash.Contains))
                {
                    using (var writer = new CsvHelper.CsvWriter(new StreamWriter(File.OpenWrite(args[1]))))
                    {
                        foreach (var row in reader)
                        {
                            var fieldPos = 0;
                            foreach (var definition in versionDef.definitions)
                            {
                                if (definition.isNonInline && definition.isID)
                                {
                                    writer.WriteField(row.Key);
                                }
                                else
                                {
                                    switch (dbd.columnDefinitions[definition.name].type)
                                    {
                                        case "uint":
                                            switch (definition.size)
                                            {
                                                case 8:
                                                    writer.WriteField(row.Value.GetField<byte>(fieldPos));
                                                    break;
                                                case 16:
                                                    writer.WriteField(row.Value.GetField<ushort>(fieldPos));
                                                    break;
                                                case 32:
                                                    writer.WriteField(row.Value.GetField<uint>(fieldPos));
                                                    break;
                                            }
                                            break;
                                        case "int":
                                            switch (definition.size)
                                            {
                                                case 8:
                                                    writer.WriteField(row.Value.GetField<sbyte>(fieldPos));
                                                    break;
                                                case 16:
                                                    writer.WriteField(row.Value.GetField<short>(fieldPos));
                                                    break;
                                                case 32:
                                                    writer.WriteField(row.Value.GetField<int>(fieldPos));
                                                    break;
                                            }
                                            break;
                                        case "locstring":
                                        case "string":
                                            writer.WriteField(row.Value.GetField<string>(fieldPos));
                                            break;
                                        case "float":
                                            writer.WriteField(row.Value.GetField<float>(fieldPos));
                                            break;
                                        default:
                                            throw new Exception("Unhandled type: " + dbd.columnDefinitions[definition.name].type);
                                    }
                                    fieldPos++;
                                }
                            }
                            writer.NextRecord();
                        }
                        writer.Flush();
                        return;
                    }
                }
            }

            Console.WriteLine("Layouthash " + layoutHash + " not found in definitions file!");
        }
    }
}
