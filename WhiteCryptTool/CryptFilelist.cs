using System;
using System.IO;
using WhiteCryptTool.CryptoClasses;
using WhiteCryptTool.SupportClasses;
using static WhiteCryptTool.SupportClasses.ToolEnums;

namespace WhiteCryptTool
{
    internal class CryptFilelist
    {
        public static void ProcessFilelist(CryptActions cryptAction, string inFile)
        {
            using (var inFileReader = new BinaryReader(File.Open(inFile, FileMode.Open, FileAccess.Read)))
            {
                Console.WriteLine("Performing initial setup....");
                Console.WriteLine("");

                inFileReader.BaseStream.Position = 16;
                var cryptBodySizeVal = inFileReader.ReadBytes(4);
                Array.Reverse(cryptBodySizeVal);

                var cryptBodySize = BitConverter.ToUInt32(cryptBodySizeVal, 0);
                cryptBodySize += 8;
                cryptBodySize.CryptoLengthCheck();


                Checks.FilelistState(cryptAction, inFileReader, cryptBodySize);

                var fileLength = (uint)inFileReader.BaseStream.Length;
                fileLength -= cryptBodySize;
                fileLength -= 32;

                uint remainderBytes = 0;

                if (fileLength > 0)
                {
                    remainderBytes = fileLength;
                }

                uint readPos = 32;
                uint writePos = 32;

                Console.WriteLine("Generating XOR table....");
                Console.WriteLine("");

                inFileReader.BaseStream.Position = 0;
                var baseSeedArray = inFileReader.ReadBytes(16);
                var seedArray8Bytes = (ulong)((baseSeedArray[9] << 24) | (baseSeedArray[12] << 16) | (baseSeedArray[2] << 8) | (baseSeedArray[0]));
                var seedArray = BitConverter.GetBytes(seedArray8Bytes);

                var xorTable = Generator.GenerateXORtable(seedArray, false);

                switch (cryptAction)
                {
                    case CryptActions.d:
                        Console.WriteLine("Decrypting filelist....");
                        Console.WriteLine("");

                        (inFile + ".dec").IfFileExistsDel();

                        using (var decryptedStreamBinWriter = new BinaryWriter(File.Open(inFile + ".dec", FileMode.Append, FileAccess.Write)))
                        {
                            inFileReader.BaseStream.Position = 0;
                            inFileReader.BaseStream.ExCopyTo(decryptedStreamBinWriter.BaseStream, writePos);

                            var blockCount = cryptBodySize / 8;
                            Decryption.DecryptBlocks(xorTable, blockCount, readPos, writePos, inFileReader, decryptedStreamBinWriter, false);

                            inFileReader.BaseStream.Position = decryptedStreamBinWriter.BaseStream.Length;
                            inFileReader.BaseStream.ExCopyTo(decryptedStreamBinWriter.BaseStream, remainderBytes);
                        }

                        inFileReader.Dispose();

                        inFile.CreateFinalFile(inFile + ".dec");

                        bool isDecryptedCorrectly = inFile.CheckPostDecryption(ref cryptBodySize, 32);

                        if (isDecryptedCorrectly)
                        {
                            ExitType.Success.ExitProgram($"Finished decrypting '{Path.GetFileName(inFile)}'.");
                        }
                        else
                        {
                            ExitType.Error.ExitProgram("Filelist file was not decrypted correctly.");
                        }
                        break;

                    case CryptActions.e:
                        Console.WriteLine("Computing filelist checksum....");
                        Console.WriteLine("");

                        (inFile + ".tmp").IfFileExistsDel();

                        using (var chkSumStreamBinWriter = new BinaryWriter(File.Open(inFile + ".tmp", FileMode.Append, FileAccess.Write)))
                        {
                            inFileReader.BaseStream.Position = 0;
                            inFileReader.BaseStream.ExCopyTo(chkSumStreamBinWriter.BaseStream, readPos);

                            inFileReader.BaseStream.Position = readPos;
                            inFileReader.BaseStream.ExCopyTo(chkSumStreamBinWriter.BaseStream, cryptBodySize - 8);

                            inFileReader.BaseStream.Position = readPos + cryptBodySize - 8;
                            inFileReader.BaseStream.ExCopyTo(chkSumStreamBinWriter.BaseStream, 4);

                            var checkSum = inFileReader.ComputeCheckSum((cryptBodySize - 8) / 4, readPos);

                            chkSumStreamBinWriter.Write(checkSum);

                            inFileReader.BaseStream.Position = chkSumStreamBinWriter.BaseStream.Length;
                            inFileReader.BaseStream.ExCopyTo(chkSumStreamBinWriter.BaseStream, remainderBytes);
                        }

                        inFileReader.Dispose();

                        Console.WriteLine("Encrypting filelist....");
                        Console.WriteLine("");

                        (inFile + ".enc").IfFileExistsDel();

                        using (var inFileReaderTmp = new BinaryReader(File.Open(inFile + ".tmp", FileMode.Open, FileAccess.Read)))
                        {
                            using (var encryptedStreamBinWriter = new BinaryWriter(File.Open(inFile + ".enc", FileMode.Append, FileAccess.Write)))
                            {
                                inFileReaderTmp.BaseStream.Position = 0;
                                inFileReaderTmp.BaseStream.ExCopyTo(encryptedStreamBinWriter.BaseStream, writePos);

                                var blockCount = cryptBodySize / 8;
                                Encryption.EncryptBlocks(xorTable, blockCount, readPos, writePos, inFileReaderTmp, encryptedStreamBinWriter, false);

                                inFileReaderTmp.BaseStream.Position = encryptedStreamBinWriter.BaseStream.Length;
                                inFileReaderTmp.BaseStream.CopyTo(encryptedStreamBinWriter.BaseStream);
                            }
                        }

                        (inFile + ".tmp").IfFileExistsDel();

                        inFile.CreateFinalFile(inFile + ".enc");

                        ExitType.Success.ExitProgram($"Finished encrypting '{Path.GetFileName(inFile)}'.");
                        break;
                }
            }
        }
    }
}