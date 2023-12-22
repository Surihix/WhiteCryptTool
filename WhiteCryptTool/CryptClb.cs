using WhiteCryptTool.CryptoClasses;
using WhiteCryptTool.SupportClasses;
using System;
using System.IO;
using static WhiteCryptTool.SupportClasses.ToolEnums;

namespace WhiteCryptTool
{
    internal class CryptClb
    {
        public static void ProcessClb(CryptActions cryptAction, string inFile)
        {
            using (var inFileReader = new BinaryReader(File.Open(inFile, FileMode.Open, FileAccess.Read)))
            {
                Console.WriteLine("Performing initial setup....");
                Console.WriteLine("");

                inFileReader.BaseStream.Position = 0;
                var seedArray = inFileReader.ReadBytes(8);
                var cryptBodySize = (uint)inFileReader.BaseStream.Length - 8;
                cryptBodySize.CryptoLengthCheck();

                Checks.ClbState(cryptAction, inFileReader, cryptBodySize);

                uint readPos = 8;
                uint writePos = 8;

                Console.WriteLine("Generating XOR table....");
                Console.WriteLine("");

                var xorTable = Generator.GenerateXORtable(seedArray, false);


                switch (cryptAction)
                {
                    case CryptActions.d:
                        Console.WriteLine("Decrypting clb....");
                        Console.WriteLine("");

                        (inFile + ".dec").IfFileExistsDel();

                        using (var decryptedStreamBinWriter = new BinaryWriter(File.Open(inFile + ".dec", FileMode.Append, FileAccess.Write)))
                        {
                            inFileReader.BaseStream.Position = 0;
                            inFileReader.BaseStream.ExCopyTo(decryptedStreamBinWriter.BaseStream, readPos);

                            var blockCount = cryptBodySize / 8;
                            Decryption.DecryptBlocks(xorTable, blockCount, readPos, writePos, inFileReader, decryptedStreamBinWriter, false);
                        }

                        inFileReader.Dispose();

                        inFile.CreateFinalFile(inFile + ".dec");

                        ExitType.Success.ExitProgram($"Finished decrypting '{Path.GetFileName(inFile)}'.");
                        break;

                    case CryptActions.e:
                        Console.WriteLine("Computing clb checksum....");
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
                        }

                        inFileReader.Dispose();

                        Console.WriteLine("Encrypting clb....");
                        Console.WriteLine("");

                        (inFile + ".enc").IfFileExistsDel();

                        using (var inFileReaderTmp = new BinaryReader(File.Open(inFile + ".tmp", FileMode.Open, FileAccess.Read)))
                        {
                            using (var encryptedStreamBinWriter = new BinaryWriter(File.Open(inFile + ".enc", FileMode.Append, FileAccess.Write)))
                            {
                                inFileReaderTmp.BaseStream.Position = 0;
                                inFileReaderTmp.BaseStream.ExCopyTo(encryptedStreamBinWriter.BaseStream, writePos);

                                cryptBodySize.CryptoLengthCheck();
                                var blockCount = cryptBodySize / 8;
                                Encryption.EncryptBlocks(xorTable, blockCount, readPos, writePos, inFileReaderTmp, encryptedStreamBinWriter, false);
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