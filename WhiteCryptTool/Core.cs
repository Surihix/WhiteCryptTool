using System;
using System.IO;
using WhiteCryptTool.SupportClasses;
using static WhiteCryptTool.SupportClasses.ToolEnums;

namespace WhiteCryptTool
{
    internal class Core
    {
        static void Main(string[] args)
        {
            var exampleMsgArray = new string[]
            {
                "Examples:",
                "To decrypt a filelist file (13-2 & LR): WhiteCryptTool.exe -d -filelist \"filelistu.bin\"",
                "To decrypt a clb file (for all 3 games): WhiteCryptTool.exe -d -clb \"common.clb\"", "",
                "To encrypt a filelist file (13-2 & LR): WhiteCryptTool.exe -e -filelist \"filelistu.bin\"",
                "To encrypt a clb file (for all 3 games): WhiteCryptTool.exe -e -clb \"common.clb\"", "",
                "Important:", "Change the filename mentioned in the example to the name or path of" +
                "\nthe file that you are trying to decrypt or encrypt.", ""
            };

            var actionSwitchesMsgArray = new string[]
            {
                "Action Switches:", "-d = To Decrypt", "-e = To Encrypt"
            };

            var cryptTypeSwitchesMsgArray = new string[]
            {
                "Crypt Type Switches:", "-filelist = For filelist files", "-clb = For clb files"
            };


            // Check length
            if (args.Length < 2)
            {
                ExitType.Error.ExitProgram($"Enough arguments not specified\n\n{string.Join("\n", actionSwitchesMsgArray)}\n\n{string.Join("\n", exampleMsgArray)}");
            }

            // Set CryptAction
            var cryptAction = new CryptActions();
            if (Enum.TryParse(args[0].Replace("-", ""), false, out CryptActions convertedActionSwitch))
            {
                cryptAction = convertedActionSwitch;
            }
            else
            {
                ExitType.Error.ExitProgram($"Invalid or no action switch specified\n\n{string.Join("\n", actionSwitchesMsgArray)}");
            }

            // Set CryptType
            var cryptType = new CryptType();
            if (Enum.TryParse(args[1].Replace("-", ""), false, out CryptType convertedTypeSwitch))
            {
                cryptType = convertedTypeSwitch;
            }
            else
            {
                ExitType.Error.ExitProgram($"Invalid or no crypt type switch specified\n\n{string.Join("\n", cryptTypeSwitchesMsgArray)}");
            }

            // Set file
            var inFile = args[2];

            if (!File.Exists(inFile))
            {
                ExitType.Error.ExitProgram("Specified file is missing");
            }

            Console.WriteLine("");

            try
            {
                switch (cryptType)
                {
                    case CryptType.filelist:
                        CryptFilelist.ProcessFilelist(cryptAction, inFile);
                        break;

                    case CryptType.clb:
                        CryptClb.ProcessClb(cryptAction, inFile);
                        break;
                }
            }
            catch (Exception ex)
            {
                ExitType.Error.ExitProgram($"An Exception has occured\n{ex}");
            }
        }

        enum CryptType
        {
            filelist,
            clb
        }
    }
}