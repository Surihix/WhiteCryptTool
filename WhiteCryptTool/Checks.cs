using WhiteCryptTool.SupportClasses;
using System.IO;
using static WhiteCryptTool.SupportClasses.ToolEnums;

namespace WhiteCryptTool
{
    internal class Checks
    {
        public static void FilelistState(CryptActions cryptActions, BinaryReader inFileReader, uint cryptBodySize)
        {
            inFileReader.BaseStream.Position = 20;
            if (inFileReader.ReadUInt32() != 501232760)
            {
                ExitType.Error.ExitProgram("Specified filelist file does not require any crypt operations.");
            }

            var cryptSizeOffset = 32 + cryptBodySize - 8;
            cryptBodySize -= 8;

            switch (cryptActions)
            {
                case CryptActions.e:
                    inFileReader.BaseStream.Position = cryptSizeOffset;
                    if (inFileReader.ReadUInt32() != cryptBodySize)
                    {
                        ExitType.Error.ExitProgram("Specified filelist file is not decrypted correctly for encryption operation.");
                    }
                    break;

                case CryptActions.d:
                    inFileReader.BaseStream.Position = cryptSizeOffset;
                    if (inFileReader.ReadUInt32() == cryptBodySize)
                    {
                        ExitType.Error.ExitProgram("Specified filelist file is already decrypted.");
                    }
                    break;
            }
        }

        public static void ClbState(CryptActions cryptActions, BinaryReader inFileReader, uint cryptBodySize)
        {
            inFileReader.BaseStream.Position = 0;
            if (inFileReader.ReadUInt32() != 1414812756)
            {
                ExitType.Error.ExitProgram("Specified file is not a valid clb file.");
            }

            cryptBodySize -= 8;
            var cryptSizeOffset = (uint)inFileReader.BaseStream.Length - 8;

            switch (cryptActions)
            {
                case CryptActions.e:
                    inFileReader.BaseStream.Position = cryptSizeOffset;
                    if (inFileReader.ReadUInt32() != cryptBodySize)
                    {
                        ExitType.Error.ExitProgram("Specified clb file is not decrypted correctly for encryption operation.");
                    }
                    break;

                case CryptActions.d:
                    inFileReader.BaseStream.Position = cryptSizeOffset;
                    if (inFileReader.ReadUInt32() == cryptBodySize)
                    {
                        ExitType.Error.ExitProgram("Specified clb file is already decrypted.");
                    }
                    break;
            }
        }
    }
}