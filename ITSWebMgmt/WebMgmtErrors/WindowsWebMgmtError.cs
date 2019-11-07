﻿using ITSWebMgmt.Controllers;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.WebMgmtErrors
{
    public abstract class ComputerWebMgmtError : WebMgmtError
    {
        protected ComputerController computer;

        public ComputerWebMgmtError(ComputerController computer)
        {
            this.computer = computer;
        }
    }
    public class MissingDataFromSCCM : ComputerWebMgmtError
    {
        public MissingDataFromSCCM(ComputerController computer) : base(computer)
        {
            Heading = "Computer missing data from SCCM";
            Description = "Some infomation cannot be shown due to missing information from SCCM";
            Severeness = Severity.Warning;
        }

        public override bool HaveError() => computer.ComputerModel.Windows.LogicalDisk.Count == 0;
    }

    public class DriveAlmostFull : ComputerWebMgmtError
    {
        public DriveAlmostFull(ComputerController computer) : base(computer)
        {
            Heading = "Less than 5 GB space avilable";
            Description = "Having an almost full drive might cause troubles";
            Severeness = Severity.Warning;
        }

        public override bool HaveError()
        {
            if (computer.ComputerModel.Windows.LogicalDisk.Count != 0)
            {
                int space = computer.ComputerModel.Windows.LogicalDisk.GetPropertyInGB("FreeSpace");
                if (space == 0) return false;
                return space <= 5;
            }

            return false;
        }
    }

    public class ManagerAndComputerNotInSameDomain : ComputerWebMgmtError
    {
        public ManagerAndComputerNotInSameDomain(ComputerController computer) : base(computer)
        {
            Heading = "The computer and the user in \"managed by\" are in different domains";
            Description = "This may or may not be an error, but user cannot have local administrator rights until this is corrected";
            Severeness = Severity.Warning;
        }

        public override bool HaveError()
        {
            ComputerModel compModel = computer.ComputerModel;
            if (compModel.IsWindows)
            {
                WindowsComputerModel winModel = compModel.Windows;
                if (!compModel.Windows.ManagedBy.Contains('@'))
                {
                    return false;
                }
                string managerDomain = compModel.Windows.ManagedBy.Split('@')[1];
                //We are only interested in the bit after @, as that is the actual domain.
                //I am sure there is a prettier way of doing this, but this works too.
                //In case you don't know what is happening, I am using @ as a delimiter for the mail of the manager,
                //Which results in two indexes in the resulting array. As the domain is in the second index, we index by 1 to get it.
                return !winModel.Domain.Equals(managerDomain);
            }
            else
            {
                return false;
            }
        }
    }

    public class MissingPCConfig : ComputerWebMgmtError
    {
        public MissingPCConfig(ComputerController computer) : base(computer)
        {
            Heading = "The PC is missing config";
            Description = @"<p>The computer is not in the Administrativ10 PC or AAU10 PC group. Plase add it to one of them</p>
                            <p>It can take over 30 secounds before you get responce, that says if it succesfully was added, please be patient</p>
                            <button id=""AddToAdministrativ10"">Add computer to Administrativ10 PC</button>
                            <br />
                            <br />
                            <button id=""AddToAAU10"">Add computer to AAU10 PC</button>
                            <script>
                                $(""#AddToAdministrativ10"").click(function ()
                                {
                                    sendPostRequest(""AddToAdministrativ10"")
                                });

                                $(""#AddToAAU10"").click(function ()
                                {
                                    sendPostRequest(""AddToAAU10"")
                                });
                            </script>";
            Severeness = Severity.Error;
        }

        public override bool HaveError()
        {
            DateTime temp = computer.ComputerModel.Windows.WhenCreated;
            return (computer.ComputerModel.Windows.ConfigPC == "Unknown" && computer.ComputerModel.Windows.WhenCreated > DateTime.Parse("2019-01-01"));
        }
    }

    public class NotStandardComputerOU : ComputerWebMgmtError
    {
        public NotStandardComputerOU(ComputerController computer) : base(computer)
        {
            Heading = "Computer is in a wrong OU";
            Description = "The computer is getting wroung GPO settings. Fix by using task \"Move computer to OU Clients.\" ";
            Severeness = Severity.Error;
        }

        public override bool HaveError() => !computer.computerIsInRightOU(computer.ComputerModel.Windows.DistinguishedName);
    }

    public class PasswordExpired : ComputerWebMgmtError
    {
        public PasswordExpired(ComputerController computer) : base(computer)
        {
            Heading = "Computer admin password is expired";
            Description = "The computer admin password is expired";
            Severeness = Severity.Warning;
        }

        public override bool HaveError()
        {
            throw new NotImplementedException();
        }
    }
}