﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace WriteService
{
    [RunInstaller(true)]
    public partial class Installer1 : Installer
    {
        ServiceInstaller serviceInstaller;
        ServiceProcessInstaller processInstaller;

        public Installer1()
        {
            InitializeComponent();
            serviceInstaller = new ServiceInstaller();
            processInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };
            serviceInstaller.StartType = ServiceStartMode.Manual;
            serviceInstaller.ServiceName = "WriteFile";
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
