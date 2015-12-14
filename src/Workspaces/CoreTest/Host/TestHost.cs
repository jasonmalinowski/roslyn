// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis.Host.Mef;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Host
{
    internal static class TestHost
    {
#pragma warning disable RS0026 // Do not use shared mutable state in tests
        private static HostServices s_testServices;
#pragma warning restore RS0026 // Do not use shared mutable state in tests

        public static HostServices Services
        {
            get
            {
                if (s_testServices == null)
                {
                    var tmp = MefHostServices.Create(MefHostServices.DefaultAssemblies.Concat(new[] { typeof(TestHost).Assembly }));
                    System.Threading.Interlocked.CompareExchange(ref s_testServices, tmp, null);
                }

                return s_testServices;
            }
        }
    }
}
