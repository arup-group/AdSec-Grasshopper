namespace Oasys.LicenceManager.Cli;

using System;
using CommandLine;
using AdSec;

public class Options
{
    [Option('l', "licenseId", Required = true, HelpText = "Set the license ID.")]
    public string? LicenseId { get; set; }

    [Option('p', "password", Required = true, HelpText = "Set the license password.")]
    public string? Password { get; set; }
}

public static class ActivateLicense
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunLicenseActivation)
            .WithNotParsed(HandleParseError);
    }

    private static void RunLicenseActivation(Options options)
    {
        try
        {
            // Activating the license with the provided ID and Password
            var licenseDetails = ILicense.ActivateLicense(options.LicenseId, options.Password);

            // If it's a floating network license, close the session
            ILicense.CloseNetworkSession();

            if (licenseDetails.ActionStatus)
            {
                Console.WriteLine("License activated successfully!");
            }
            else
            {
                Console.WriteLine("Failed to activate the license");
                foreach (var warning in licenseDetails.Warnings)
                {
                    Console.WriteLine($"warning: {warning.Description}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during license activation: {ex.Message}");
        }
    }

    private static void HandleParseError(IEnumerable<Error> errs)
    {
        // Handle errors (optional)
        Console.WriteLine("Failed to parse command-line arguments.");
    }
}