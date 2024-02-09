using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;
using System.ComponentModel.DataAnnotations;
using DisplayMagicianShared;

namespace DisplayMagicianConsole
{
    class ProfileMustExistValidator : IArgumentValidator
    {

        public ValidationResult GetValidationResult(CommandArgument argumentProfileName, ValidationContext context)
        {
            // This validator only runs if there is a value
            if (argumentProfileName.Value == "") return ValidationResult.Success;
            var profileName = (string)argumentProfileName.Value;

            // Try to find the Profile Name
            if (!ProfileRepository.ContainsProfile(profileName))
            {
                Console.WriteLine($"ProfileMustExistValidator/GetValidationResult: Couldn't find Profile Name or ID supplied via command line: '{profileName}'. Please check the Profile Name or ID you supplied on the command line is correct.");
                return new ValidationResult($"Couldn't find Profile Name or ID supplied via command line: '{profileName}'. Please check the Profile Name or ID you supplied on the command line is correct.");
            }

            ProfileItem profile = ProfileRepository.GetProfile(profileName);
            Console.WriteLine($"Using Profile: '{profile.Name}' (ID:{profile.UUID})");
            return ValidationResult.Success;
        }
    }

    /*class ShortcutMustExistValidator : IArgumentValidator
    {

        public ValidationResult GetValidationResult(CommandArgument argumentShortcutName, ValidationContext context)
        {
            // This validator only runs if there is a string provided
            if (argumentShortcutName.Value == "") return ValidationResult.Success;
            string shortcutName = (string) argumentShortcutName.Value;

            // Check if the UUID or ShortcutName are provided
            if (!ShortcutRepository.ContainsShortcut(shortcutName))
            {
                logger.Error($"ProfileMustExistValidator/GetValidationResult: Couldn't find Shortcut Name supplied via command line: '{shortcutName}'. Please check the Shortcut Name you supplied on the command line is correct.");
                return new ValidationResult($"Couldn't find Shortcut Name supplied via command line: '{shortcutName}'. Please check the Shortcut Name you supplied on the command line is correct.");
            }

            ShortcutItem shortcut = ShortcutRepository.GetShortcut(shortcutName);
            Console.WriteLine($"Using Shortcut: '{shortcut.Name}' (ID: {shortcut.UUID})");
            return ValidationResult.Success;
        }
    }*/


    class FileOptionMustExistValidator : IOptionValidator
    {       
        public ValidationResult GetValidationResult(CommandOption optionFullFileName, ValidationContext context)
        {
            // This validator only runs if there is a string provided
            if (optionFullFileName.Value() == "") return ValidationResult.Success;
            var fileNameAndPath = optionFullFileName.Value();

            // Check that the file exists
            if (!File.Exists(fileNameAndPath))
            {
                Console.WriteLine($"ProfileMustExistValidator/GetValidationResult: Couldn't find the file '{optionFullFileName.Value()}' supplied to '{optionFullFileName.LongName}'. Please check you specified the full path to the file on the command line.");
                return new ValidationResult($"Couldn't find the file '{optionFullFileName.Value()}' supplied to '{optionFullFileName.LongName}'. Please check you specified the full path to the file on the command line.");
            }

            return ValidationResult.Success;
        }
    }

    class FileArgumentMustExistValidator : IArgumentValidator
    {
       
        public ValidationResult GetValidationResult(CommandArgument argumentFullFileName, ValidationContext context)
        {
            // This validator only runs if there is a string provided
            if (argumentFullFileName.Value == "") return ValidationResult.Success;
            var fileNameAndPath = argumentFullFileName.Value;

            // Check that the file exists
            if (!File.Exists(fileNameAndPath))
            {
                Console.WriteLine($"ProfileMustExistValidator/GetValidationResult: Couldn't find the file '{argumentFullFileName.Value}' supplied to '{argumentFullFileName.Name}'. Please check you specified the full path to the file on the command line.");
                return new ValidationResult($"Couldn't find the file '{argumentFullFileName.Value}' supplied to '{argumentFullFileName.Name}'. Please check you specified the full path to the file on the command line.");
            }

            return ValidationResult.Success;
        }
    }

}
