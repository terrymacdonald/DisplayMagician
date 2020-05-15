using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;
using System.ComponentModel.DataAnnotations;
using HeliosPlus.Shared;
using HeliosPlus.GameLibraries;

namespace HeliosPlus
{
    class ProfileMustExistValidator : IOptionValidator
    {
        public ValidationResult GetValidationResult(CommandOption optionProfile, ValidationContext context)
        {
            // This validator only runs if there is a value
            if (!optionProfile.HasValue()) return ValidationResult.Success;
            var profile = optionProfile.Value();

            // Create an array of display profiles we have
            var profiles = Profile.LoadAllProfiles().ToArray();
            // Check if the user supplied a --profile option using the profiles' ID
            var profileIndex = profiles.Length > 0 ? Array.FindIndex(profiles, p => p.Id.Equals(profile, StringComparison.InvariantCultureIgnoreCase)) : -1;
            // If the profileID wasn't there, maybe they used the profile name?
            if (profileIndex == -1)
            {
                // Try and lookup the profile in the profiles' Name fields
                profileIndex = profiles.Length > 0 ? Array.FindIndex(profiles, p => p.Name.StartsWith(profile, StringComparison.InvariantCultureIgnoreCase)) : -1;
            }
            // If the profileID still isn't there, then raise the alarm
            if (profileIndex == -1)
            {
                return new ValidationResult($"Couldn't find Profile Name or ID supplied via command line: '{optionProfile.LongName}'. Please check the Profile Name or ID you supplied on the command line is correct.");
            }

            Console.WriteLine($"Using Profile: '{profiles[profileIndex].Name}' (ID:{profiles[profileIndex].Id})");
            return ValidationResult.Success;
        }
    }

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
                return new ValidationResult($"Couldn't find the file '{argumentFullFileName.Value}' supplied to '{argumentFullFileName.Name}'. Please check you specified the full path to the file on the command line.");
            }

            return ValidationResult.Success;
        }
    }

    class SteamArgumentMustExistValidator : IArgumentValidator
    {
        public ValidationResult GetValidationResult(CommandArgument argumentSteamGameId, ValidationContext context)
        {
            // This validator only runs if there is a string provided
            if (argumentSteamGameId.Value == "") return ValidationResult.Success;
            var steamGameId = argumentSteamGameId.Value;

            // Check that the Steam Game exists
            /*if (!Steam.IsInstalled(steamGameId))
            {
                return new ValidationResult($"Couldn't find a Steam Game with ID '{argumentSteamGameId.Value}'  within Steam. Please check you specified the correct Steam Game ID you supplied in the --steam '{argumentSteamGameId.Name}' command line argument.");
            }*/

            return ValidationResult.Success;
        }
    }

    class UplayArgumentMustExistValidator : IArgumentValidator
    {
        public ValidationResult GetValidationResult(CommandArgument argumentUplayGameId, ValidationContext context)
        {
            // This validator only runs if there is a string provided
            if (argumentUplayGameId.Value == "") return ValidationResult.Success;
            var steamGameId = argumentUplayGameId.Value;

            // Check that the Uplay Game exists
            /*if (!Steam.IsInstalled(uplayGameId))
            {
                return new ValidationResult($"Couldn't find a Uplay Game with ID '{argumentUplayGameId.Value}'  within Uplay. Please check you specified the correct Uplay Game ID you supplied in the --steam '{argumentUplayGameId.Name}' command line argument.");
            }*/

            return ValidationResult.Success;
        }
    }



}
